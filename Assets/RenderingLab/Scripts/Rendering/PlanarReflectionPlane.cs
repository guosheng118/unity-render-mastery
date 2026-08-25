using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace RenderingLab
{
    /// <summary>
    /// Renders a mirrored camera into _PlanarReflectionTexture.
    /// The Renderer Feature only binds / downsamples; the capture lives here because
    /// a second camera is still the production-proven path on URP 6.3 (native SSR lands later).
    /// </summary>
    [ExecuteAlways]
    public class PlanarReflectionPlane : MonoBehaviour
    {
        public MeshRenderer targetRenderer;
        public LayerMask cullingMask = ~0;
        [Range(0.25f, 1f)] public float resolutionScale = 0.5f;
        public float clipPlaneOffset = 0.07f;
        public bool disablePixelLights = true;

        Camera _reflCam;
        RenderTexture _rt;
        static bool _inside;

        public static readonly int PlanarTexId = Shader.PropertyToID("_PlanarReflectionTexture");
        public static readonly int PlanarOnId = Shader.PropertyToID("_LabPlanarEnabled");

        void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering += OnBeginCamera;
        }

        void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeginCamera;
            Cleanup();
        }

        void OnBeginCamera(ScriptableRenderContext ctx, Camera cam)
        {
            if (_inside) return;
            if (cam.cameraType == CameraType.Reflection) return;
            if (cam.orthographic) return;
            if (Shader.GetGlobalFloat(PlanarOnId) < 0.5f)
            {
                Shader.SetGlobalTexture(PlanarTexId, Texture2D.blackTexture);
                return;
            }

            var qc = QualityTierController.Instance;
            float scale = resolutionScale;
            if (qc != null && qc.catalog != null)
            {
                if (!qc.catalog.planarReflection[(int)qc.current])
                {
                    Shader.SetGlobalTexture(PlanarTexId, Texture2D.blackTexture);
                    return;
                }
                if (QualityTierUtil.IsHigh(qc.current)) scale = 1f;
                else if (QualityTierUtil.IsLow(qc.current)) scale = 0.25f;
            }

            EnsureResources(cam, scale);

            Vector3 pos = transform.position;
            Vector3 normal = transform.up;

            float d = -Vector3.Dot(normal, pos) - clipPlaneOffset;
            Vector4 plane = new Vector4(normal.x, normal.y, normal.z, d);
            Matrix4x4 reflection = Matrix4x4.zero;
            CalculateReflectionMatrix(ref reflection, plane);

            _reflCam.CopyFrom(cam);
            _reflCam.useOcclusionCulling = false;
            _reflCam.clearFlags = CameraClearFlags.SolidColor;
            _reflCam.backgroundColor = Color.black;
            _reflCam.cullingMask = cullingMask;
            _reflCam.targetTexture = _rt;
            _reflCam.worldToCameraMatrix = cam.worldToCameraMatrix * reflection;

            Vector4 clipPlane = CameraSpacePlane(_reflCam, pos, normal, 1f);
            _reflCam.projectionMatrix = cam.CalculateObliqueMatrix(clipPlane);
            _reflCam.cullingMatrix = _reflCam.projectionMatrix * _reflCam.worldToCameraMatrix;

            GL.invertCulling = true;
            _inside = true;
            int oldPixel = QualitySettings.pixelLightCount;
            if (disablePixelLights) QualitySettings.pixelLightCount = 0;
            _reflCam.Render();
            if (disablePixelLights) QualitySettings.pixelLightCount = oldPixel;
            _inside = false;
            GL.invertCulling = false;

            Shader.SetGlobalTexture(PlanarTexId, _rt);
            if (targetRenderer != null && targetRenderer.sharedMaterial != null)
                targetRenderer.sharedMaterial.SetTexture(PlanarTexId, _rt);
        }

        void EnsureResources(Camera cam, float scale)
        {
            int w = Mathf.Max(16, Mathf.RoundToInt(cam.pixelWidth * scale));
            int h = Mathf.Max(16, Mathf.RoundToInt(cam.pixelHeight * scale));
            if (_rt == null || _rt.width != w || _rt.height != h)
            {
                if (_rt != null) _rt.Release();
                _rt = new RenderTexture(w, h, 16, RenderTextureFormat.DefaultHDR)
                {
                    name = "PlanarReflection",
                    filterMode = FilterMode.Bilinear,
                    hideFlags = HideFlags.DontSave
                };
            }

            if (_reflCam == null)
            {
                var go = new GameObject("PlanarReflectionCamera");
                go.hideFlags = HideFlags.HideAndDontSave;
                _reflCam = go.AddComponent<Camera>();
                _reflCam.enabled = false;
                if (_reflCam.GetComponent<UniversalAdditionalCameraData>() == null)
                    _reflCam.gameObject.AddComponent<UniversalAdditionalCameraData>();
            }
        }

        void Cleanup()
        {
            if (_rt != null)
            {
                _rt.Release();
                _rt = null;
            }
            if (_reflCam != null)
                DestroyImmediate(_reflCam.gameObject);
        }

        Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float side)
        {
            Vector3 offsetPos = pos + normal * clipPlaneOffset;
            Matrix4x4 m = cam.worldToCameraMatrix;
            Vector3 cpos = m.MultiplyPoint(offsetPos);
            Vector3 cnormal = m.MultiplyVector(normal).normalized * side;
            return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
        }

        static void CalculateReflectionMatrix(ref Matrix4x4 m, Vector4 plane)
        {
            m.m00 = 1f - 2f * plane[0] * plane[0];
            m.m01 = -2f * plane[0] * plane[1];
            m.m02 = -2f * plane[0] * plane[2];
            m.m03 = -2f * plane[3] * plane[0];
            m.m10 = -2f * plane[1] * plane[0];
            m.m11 = 1f - 2f * plane[1] * plane[1];
            m.m12 = -2f * plane[1] * plane[2];
            m.m13 = -2f * plane[3] * plane[1];
            m.m20 = -2f * plane[2] * plane[0];
            m.m21 = -2f * plane[2] * plane[1];
            m.m22 = 1f - 2f * plane[2] * plane[2];
            m.m23 = -2f * plane[3] * plane[2];
            m.m30 = 0f;
            m.m31 = 0f;
            m.m32 = 0f;
            m.m33 = 1f;
        }
    }
}
