using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace RenderingLab
{
    /// <summary>
    /// Builds a complete, playable lab scene from primitives so the project
    /// does not depend on baked YAML meshes. Drop your own FBX on CharacterSlot / EnvironmentSlot.
    /// </summary>
    [DefaultExecutionOrder(-500)]
    public class LabBootstrap : MonoBehaviour
    {
        public LabModule module = LabModule.NeonShowcase;
        public bool buildOnAwake = true;
        [Tooltip("If a child named CharacterSlot already has a MeshRenderer, that mesh is kept.")]
        public bool keepExistingCharacter;

        LabEnvironmentBuilder _env;
        LabLightingRig _lights;
        LabCharacterBuilder _character;

        void Awake()
        {
            EnsureQualityLoop();
            if (buildOnAwake)
                Build();
        }

        public void Build()
        {
            _env = GetComponent<LabEnvironmentBuilder>() ?? gameObject.AddComponent<LabEnvironmentBuilder>();
            _lights = GetComponent<LabLightingRig>() ?? gameObject.AddComponent<LabLightingRig>();
            _character = GetComponent<LabCharacterBuilder>() ?? gameObject.AddComponent<LabCharacterBuilder>();

            Shader.SetGlobalFloat("_LabDebugMode", 0f);
            Shader.SetGlobalFloat("_LabOutlineEnabled", 1f);
            Shader.SetGlobalFloat("_LabPlanarEnabled", 1f);
            _env.Build(module);
            _lights.Build(module);
            _character.Build(module, keepExistingCharacter);
            LabVolumeFactory.Ensure(module);
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
            cam.farClipPlane = 120f;
            cam.fieldOfView = 40f;
            cam.transform.SetPositionAndRotation(new Vector3(0.35f, 1.45f, -4.2f), Quaternion.Euler(8f, -4f, 0f));
            if (cam.GetComponent<LabOrbitCamera>() == null)
                cam.gameObject.AddComponent<LabOrbitCamera>();

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.18f, 0.22f, 0.38f);
            RenderSettings.ambientEquatorColor = new Color(0.12f, 0.1f, 0.16f);
            RenderSettings.ambientGroundColor = new Color(0.04f, 0.03f, 0.05f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.05f, 0.04f, 0.08f);
            RenderSettings.fogDensity = 0.018f;
            RenderSettings.reflectionIntensity = 0.85f;
        }
    }
}
