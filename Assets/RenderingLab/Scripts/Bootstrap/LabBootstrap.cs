using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace RenderingLab
{
    /// <summary>
    /// Quality-tier sandbox: one sun, a few URP Lit primitives, camera + HUD.
    /// Lighting / custom shaders / Renderer Features are added later, when you ask.
    /// </summary>
    [DefaultExecutionOrder(-500)]
    public class LabBootstrap : MonoBehaviour
    {
        public bool buildOnAwake = true;

        void Awake()
        {
            EnsureQualityLoop();
            ConfigureCamera();
            if (buildOnAwake)
                BuildSimpleStage();
        }

        public void BuildSimpleStage()
        {
            if (transform.Find("Stage") != null)
                return;

            var stage = new GameObject("Stage");
            stage.transform.SetParent(transform, false);

            var sun = new GameObject("Sun");
            sun.transform.SetParent(stage.transform, false);
            sun.transform.rotation = Quaternion.Euler(42f, -35f, 0f);
            var light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            light.color = new Color(1f, 0.96f, 0.9f);
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.85f;

            MakePrimitive(stage.transform, "Ground", PrimitiveType.Plane, Vector3.zero, new Vector3(2.4f, 1f, 2.4f), new Color(0.55f, 0.55f, 0.58f));
            MakePrimitive(stage.transform, "Cube", PrimitiveType.Cube, new Vector3(-0.85f, 0.5f, 0.15f), Vector3.one, new Color(0.82f, 0.32f, 0.28f));
            var capsule = MakePrimitive(stage.transform, "Capsule", PrimitiveType.Capsule, new Vector3(0f, 1f, 0.35f), new Vector3(0.7f, 1f, 0.7f), new Color(0.9f, 0.82f, 0.72f));
            MakePrimitive(stage.transform, "Sphere", PrimitiveType.Sphere, new Vector3(0.85f, 0.5f, 0.1f), Vector3.one, new Color(0.32f, 0.52f, 0.82f));

            var cam = Camera.main;
            if (cam != null)
            {
                var orbit = cam.GetComponent<LabOrbitCamera>();
                if (orbit != null)
                    orbit.target = capsule.transform;
            }
        }

        static GameObject MakePrimitive(Transform parent, string name, PrimitiveType type, Vector3 pos, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            Object.Destroy(go.GetComponent<Collider>());
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = DefaultLit(color);
            renderer.shadowCastingMode = ShadowCastingMode.On;
            return go;
        }

        static Material DefaultLit(Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Hidden/Universal Render Pipeline/FallbackError");
            var mat = new Material(shader);
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", color);
            else
                mat.color = color;
            return mat;
        }

        static void EnsureQualityLoop()
        {
            if (QualityTierController.Instance == null)
            {
                var go = new GameObject("QualityLoop");
                go.AddComponent<QualityTierController>();
                go.AddComponent<QualityHud>();
            }
            else if (FindFirstObjectByType<QualityHud>() == null)
            {
                QualityTierController.Instance.gameObject.AddComponent<QualityHud>();
            }
        }

        static void ConfigureCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("Main Camera");
                cam = go.AddComponent<Camera>();
                go.tag = "MainCamera";
            }

            if (cam.GetComponent<UniversalAdditionalCameraData>() == null)
                cam.gameObject.AddComponent<UniversalAdditionalCameraData>();

            cam.clearFlags = CameraClearFlags.Skybox;
            cam.allowHDR = true;
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 80f;
            cam.fieldOfView = 40f;
            cam.transform.SetPositionAndRotation(new Vector3(0.4f, 1.6f, -4.4f), Quaternion.Euler(8f, -4f, 0f));
            if (cam.GetComponent<LabOrbitCamera>() == null)
                cam.gameObject.AddComponent<LabOrbitCamera>();

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.35f, 0.38f, 0.45f);
            RenderSettings.ambientEquatorColor = new Color(0.22f, 0.22f, 0.24f);
            RenderSettings.ambientGroundColor = new Color(0.12f, 0.11f, 0.1f);
            RenderSettings.fog = false;
        }
    }
}
