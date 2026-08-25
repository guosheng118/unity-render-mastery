using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace RenderingLab
{
    public class LabLightingRig : MonoBehaviour
    {
        public void Build(LabModule module)
        {
            ClearGenerated();
            var root = new GameObject("_LightingRig");
            root.transform.SetParent(transform, false);

            // Key — warm, slightly above, shadows. Character rendering layer can be isolated later.
            var key = MakeLight(root.transform, "Key_Directional", LightType.Directional, new Color(1f, 0.92f, 0.82f), 1.15f, LightShadows.Soft);
            key.transform.rotation = Quaternion.Euler(38f, -35f, 0f);
            key.lightmapBakeType = LightmapBakeType.Mixed;
            key.renderingLayerMask = ~0;

            if (module is LabModule.Lighting or LabModule.NeonShowcase or LabModule.QualityTiers or LabModule.PostProcess)
            {
                var fill = MakeLight(root.transform, "Fill_Spot", LightType.Spot, new Color(0.35f, 0.55f, 1f), 4.5f, LightShadows.None);
                fill.transform.SetPositionAndRotation(new Vector3(2.4f, 2.1f, 1.2f), Quaternion.Euler(25f, -130f, 0f));
                fill.range = 12f;
                fill.innerSpotAngle = 28f;
                fill.spotAngle = 55f;
                fill.lightmapBakeType = LightmapBakeType.Realtime;

                var rim = MakeLight(root.transform, "Rim_Spot", LightType.Spot, new Color(0.55f, 0.85f, 1f), 6f, LightShadows.None);
                rim.transform.SetPositionAndRotation(new Vector3(-2.6f, 1.8f, 2.4f), Quaternion.Euler(15f, 150f, 0f));
                rim.range = 10f;
                rim.innerSpotAngle = 18f;
                rim.spotAngle = 40f;
                rim.lightmapBakeType = LightmapBakeType.Realtime;
            }

            if (module is LabModule.NeonShowcase or LabModule.GiApv or LabModule.Lighting or LabModule.PostProcess or LabModule.Reflections)
            {
                MakeNeon(root.transform, "Neon_Magenta", new Vector3(-2.2f, 2.4f, 1.6f), new Color(1f, 0.15f, 0.55f), 18f);
                MakeNeon(root.transform, "Neon_Cyan", new Vector3(2.4f, 1.6f, 2.0f), new Color(0.15f, 0.85f, 1f), 16f);
                MakeNeon(root.transform, "Neon_Amber", new Vector3(0.2f, 0.35f, 3.4f), new Color(1f, 0.55f, 0.12f), 10f);
            }

            if (module == LabModule.Lighting)
            {
                var solo = MakeLight(root.transform, "CharacterOnly_Point", LightType.Point, new Color(1f, 0.78f, 0.55f), 2.2f, LightShadows.None);
                solo.transform.position = new Vector3(0.4f, 1.5f, -0.8f);
                solo.range = 4f;
                solo.lightmapBakeType = LightmapBakeType.Realtime;
            }
        }

        void ClearGenerated()
        {
            var existing = transform.Find("_LightingRig");
            if (existing) DestroyImmediate(existing.gameObject);
        }

        static Light MakeLight(Transform parent, string name, LightType type, Color color, float intensity, LightShadows shadows)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var light = go.AddComponent<Light>();
            light.type = type;
            light.color = color;
            light.intensity = intensity;
            light.shadows = shadows;
            light.shadowStrength = 0.85f;
            light.shadowBias = 0.02f;
            light.shadowNormalBias = 0.4f;
            if (go.GetComponent<UniversalAdditionalLightData>() == null)
                go.AddComponent<UniversalAdditionalLightData>();
            return light;
        }

        static void MakeNeon(Transform parent, string name, Vector3 pos, Color color, float intensity)
        {
            var light = MakeLight(parent, name, LightType.Point, color, intensity, LightShadows.None);
            light.transform.position = pos;
            light.range = 6f;
            light.lightmapBakeType = LightmapBakeType.Mixed;

            var bulb = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bulb.name = name + "_Geo";
            bulb.transform.SetParent(parent, false);
            bulb.transform.position = pos;
            bulb.transform.localScale = new Vector3(0.08f, 0.7f, 0.08f);
            Object.Destroy(bulb.GetComponent<Collider>());
            var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            mat.color = color * 4f;
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", color * 4f);
            bulb.GetComponent<MeshRenderer>().sharedMaterial = mat;
            bulb.isStatic = true;
        }
    }
}
