using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace RenderLab
{
    /// <summary>
    /// 平面倒影：把主相机绕地板平面镜像后画进 RT，地板 shader 用屏幕 UV 采样。
    /// 挂在地板上。角色 shader 不用改。
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class PlanarReflection : MonoBehaviour
    {
        [Tooltip("留空则用当前正在渲染的 Game 相机（不必打 MainCamera 标签）")]
        public Camera sourceCamera;

        [Range(0.25f, 1f)]
        [Tooltip("RT 相对屏幕的分辨率。0.5 自带一点软，接近参考图")]
        public float resolutionScale = 0.5f;

        [Tooltip("斜裁剪平面微偏，减少贴面闪烁")]
        public float clipPlaneOffset = 0.03f;

        public bool renderShadows;

        [Tooltip("Scene 视图也画倒影，编辑时方便看，会多一次开销")]
        public bool renderInSceneView;

        [Tooltip("主相机仍会画这些层（天空、星半球），但不写进角色倒影 RT。CopyFrom 会拷主相机遮罩，必须在这里扣掉。")]
        public LayerMask excludeLayers;

        [Tooltip("只画进天空倒影 RT 的层（星半球）。地板用 Fresnel 叠，不走角色 fade。留空则自动包含 SkyStar。")]
        public LayerMask skyReflectionLayers;

        static readonly int ReflectionTexId = Shader.PropertyToID("_PlanarReflectionTex");
        static readonly int ReflectionTexelId = Shader.PropertyToID("_PlanarReflectionTex_TexelSize");
        static readonly int SkyReflectionTexId = Shader.PropertyToID("_PlanarSkyTex");
        static readonly int SkyReflectionTexelId = Shader.PropertyToID("_PlanarSkyTex_TexelSize");
        static readonly int FxReflectionTexId = Shader.PropertyToID("_PlanarFxTex");

        Camera _reflectionCamera;
        RenderTexture _rt;
        RenderTexture _skyRt;
        RenderTexture _fxRt;
        int _rtW;
        int _rtH;
        bool _rendering;
        readonly List<FxMatState> _fxRestore = new List<FxMatState>();

        struct FxMatState
        {
            public Material mat;
            public float cull;
            public bool softOn;
        }

        void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        }

        void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
            Release();
        }

        void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            if (_rendering || !isActiveAndEnabled)
                return;

            if (camera.cameraType == CameraType.Preview || camera.cameraType == CameraType.Reflection)
                return;

            if (camera.cameraType == CameraType.SceneView)
            {
                if (!renderInSceneView)
                    return;
            }
            else if (camera.cameraType != CameraType.Game)
            {
                return;
            }

            if (sourceCamera != null && camera != sourceCamera && camera.cameraType != CameraType.SceneView)
                return;

            if (_reflectionCamera != null && camera == _reflectionCamera)
                return;

            if (camera.targetTexture != null)
                return;

            RenderReflection(camera);
        }

        void RenderReflection(Camera source)
        {
            Vector3 planePos = transform.position;
            Vector3 planeN = transform.up.normalized;
            if (planeN.sqrMagnitude < 1e-8f)
                return;

            EnsureCamera();
            EnsureRt(source);

            Camera dest = _reflectionCamera;
            dest.CopyFrom(source);
            dest.enabled = false;
            dest.clearFlags = CameraClearFlags.SolidColor;
            dest.backgroundColor = Color.clear;
            dest.allowMSAA = false;
            dest.allowHDR = true;
            dest.useOcclusionCulling = false;
            dest.depthTextureMode = DepthTextureMode.Depth;

            UniversalAdditionalCameraData dstData = dest.GetUniversalAdditionalCameraData();
            dstData.renderType = CameraRenderType.Base;
            dstData.renderPostProcessing = false;
            dstData.antialiasing = AntialiasingMode.None;
            dstData.renderShadows = renderShadows;
            dstData.requiresColorOption = CameraOverrideOption.Off;
            dstData.requiresDepthOption = CameraOverrideOption.On;
            dstData.dithering = false;
            dstData.volumeLayerMask = 0;

            float d = -Vector3.Dot(planeN, planePos) - clipPlaneOffset;
            Vector4 plane = new Vector4(planeN.x, planeN.y, planeN.z, d);
            Matrix4x4 reflection = ReflectionMatrix(plane);

            dest.worldToCameraMatrix = source.worldToCameraMatrix * reflection;
            Vector4 clipPlane = CameraSpacePlane(dest, planePos, planeN, 1f);
            dest.projectionMatrix = source.CalculateObliqueMatrix(clipPlane);
            dest.cullingMatrix = dest.projectionMatrix * dest.worldToCameraMatrix;

            Vector3 mirroredPos = ReflectPoint(source.transform.position, planePos, planeN);
            Vector3 mirroredFwd = Vector3.Reflect(source.transform.forward, planeN);
            Vector3 mirroredUp = Vector3.Reflect(source.transform.up, planeN);
            dest.transform.SetPositionAndRotation(mirroredPos, Quaternion.LookRotation(mirroredFwd, mirroredUp));

            Renderer floor = GetComponent<Renderer>();
            bool floorWasEnabled = floor != null && floor.enabled;
            if (floor != null)
                floor.enabled = false;

            _rendering = true;
            try
            {
                GL.invertCulling = true;

                int skyMask = SkyReflectionMask();
                if (skyMask != 0 && _skyRt != null)
                {
                    dest.targetTexture = _skyRt;
                    dest.cullingMask = skyMask;
                    dest.Render();
                    Shader.SetGlobalTexture(SkyReflectionTexId, _skyRt);
                    Shader.SetGlobalVector(SkyReflectionTexelId, new Vector4(1f / _skyRt.width, 1f / _skyRt.height, _skyRt.width, _skyRt.height));
                }

                int fxMask = FxLayerMask();
                if (fxMask != 0 && _fxRt != null)
                {
                    GL.invertCulling = false;
                    PushFxForPlanar();
                    dest.targetTexture = _fxRt;
                    dest.cullingMask = fxMask;
                    dest.Render();
                    Shader.SetGlobalTexture(FxReflectionTexId, _fxRt);
                    PopFxForPlanar();
                    GL.invertCulling = true;
                }

                dest.targetTexture = _rt;
                dest.cullingMask = CullingMaskWithoutSky(source.cullingMask);
                dest.Render();
            }
            finally
            {
                PopFxForPlanar();
                GL.invertCulling = false;
                _rendering = false;
                if (floor != null)
                    floor.enabled = floorWasEnabled;
            }

            Shader.SetGlobalTexture(ReflectionTexId, _rt);
            Shader.SetGlobalVector(ReflectionTexelId, new Vector4(1f / _rt.width, 1f / _rt.height, _rt.width, _rt.height));
        }

        void EnsureCamera()
        {
            if (_reflectionCamera != null)
                return;

            var go = new GameObject("Planar Reflection Camera");
            go.hideFlags = HideFlags.HideAndDontSave;
            _reflectionCamera = go.AddComponent<Camera>();
            _reflectionCamera.enabled = false;
            go.AddComponent<UniversalAdditionalCameraData>();
        }

        void EnsureRt(Camera source)
        {
            int w = Mathf.Max(1, Mathf.RoundToInt(source.pixelWidth * resolutionScale));
            int h = Mathf.Max(1, Mathf.RoundToInt(source.pixelHeight * resolutionScale));
            if (_rt != null && _skyRt != null && _fxRt != null && _rtW == w && _rtH == h)
                return;

            ReleaseRts();

            _rt = CreateReflectionRt(w, h, "PlanarReflection");
            _skyRt = CreateReflectionRt(w, h, "PlanarSkyReflection");
            _fxRt = CreateReflectionRt(w, h, "PlanarFxReflection");
            _rtW = w;
            _rtH = h;
        }

        static RenderTexture CreateReflectionRt(int w, int h, string rtName)
        {
            var rt = new RenderTexture(w, h, 16, RenderTextureFormat.DefaultHDR)
            {
                name = rtName,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
            rt.Create();
            return rt;
        }

        void ReleaseRts()
        {
            if (_rt != null)
            {
                _rt.Release();
                DestroyImmediate(_rt);
                _rt = null;
            }

            if (_skyRt != null)
            {
                _skyRt.Release();
                DestroyImmediate(_skyRt);
                _skyRt = null;
            }

            if (_fxRt != null)
            {
                _fxRt.Release();
                DestroyImmediate(_fxRt);
                _fxRt = null;
            }
        }

        void Release()
        {
            ReleaseRts();

            if (_reflectionCamera != null)
            {
                DestroyImmediate(_reflectionCamera.gameObject);
                _reflectionCamera = null;
            }
        }

        void PushFxForPlanar()
        {
            _fxRestore.Clear();
            int fx = LayerMask.NameToLayer("FX");
            if (fx < 0)
                return;

            ParticleSystemRenderer[] renderers = Object.FindObjectsByType<ParticleSystemRenderer>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < renderers.Length; i++)
            {
                ParticleSystemRenderer r = renderers[i];
                if (r == null || r.gameObject.layer != fx || r.sharedMaterial == null)
                    continue;

                Material mat = r.sharedMaterial;
                _fxRestore.Add(new FxMatState
                {
                    mat = mat,
                    cull = mat.HasProperty("_Cull") ? mat.GetFloat("_Cull") : 2f,
                    softOn = mat.IsKeywordEnabled("_SOFTPARTICLES_ON")
                });

                r.allowOcclusionWhenDynamic = false;
                if (mat.HasProperty("_Cull"))
                    mat.SetFloat("_Cull", 0f);
                mat.DisableKeyword("_SOFTPARTICLES_ON");
                if (mat.HasProperty("_SoftParticlesEnabled"))
                    mat.SetFloat("_SoftParticlesEnabled", 0f);
            }
        }

        void PopFxForPlanar()
        {
            for (int i = 0; i < _fxRestore.Count; i++)
            {
                FxMatState s = _fxRestore[i];
                if (s.mat == null)
                    continue;

                if (s.mat.HasProperty("_Cull"))
                    s.mat.SetFloat("_Cull", s.cull);
                if (s.softOn)
                {
                    s.mat.EnableKeyword("_SOFTPARTICLES_ON");
                    if (s.mat.HasProperty("_SoftParticlesEnabled"))
                        s.mat.SetFloat("_SoftParticlesEnabled", 1f);
                }
            }

            _fxRestore.Clear();
        }

        int CullingMaskWithoutSky(int sourceMask)
        {
            int mask = sourceMask & ~excludeLayers.value & ~SkyReflectionMask() & ~FxLayerMask();

            int skyStar = LayerMask.NameToLayer("SkyStar");
            if (skyStar >= 0)
                mask &= ~(1 << skyStar);

            return mask;
        }

        int SkyReflectionMask()
        {
            int mask = skyReflectionLayers.value;
            int skyStar = LayerMask.NameToLayer("SkyStar");
            if (skyStar >= 0)
                mask |= 1 << skyStar;
            return mask;
        }

        static int FxLayerMask()
        {
            int fx = LayerMask.NameToLayer("FX");
            return fx >= 0 ? 1 << fx : 0;
        }

        static Vector3 ReflectPoint(Vector3 point, Vector3 planePos, Vector3 planeN)
        {
            return point - 2f * Vector3.Dot(point - planePos, planeN) * planeN;
        }

        static Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
        {
            Vector3 offsetPos = pos + normal * 0.01f;
            Matrix4x4 m = cam.worldToCameraMatrix;
            Vector3 cpos = m.MultiplyPoint(offsetPos);
            Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
            return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
        }

        static Matrix4x4 ReflectionMatrix(Vector4 plane)
        {
            Matrix4x4 m = Matrix4x4.zero;
            m.m00 = 1f - 2f * plane.x * plane.x;
            m.m01 = -2f * plane.x * plane.y;
            m.m02 = -2f * plane.x * plane.z;
            m.m03 = -2f * plane.w * plane.x;

            m.m10 = -2f * plane.y * plane.x;
            m.m11 = 1f - 2f * plane.y * plane.y;
            m.m12 = -2f * plane.y * plane.z;
            m.m13 = -2f * plane.w * plane.y;

            m.m20 = -2f * plane.z * plane.x;
            m.m21 = -2f * plane.z * plane.y;
            m.m22 = 1f - 2f * plane.z * plane.z;
            m.m23 = -2f * plane.w * plane.z;

            m.m33 = 1f;
            return m;
        }
    }
}
