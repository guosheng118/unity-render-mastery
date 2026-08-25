using UnityEngine;

namespace RenderingLab
{
    public class LabEnvironmentBuilder : MonoBehaviour
    {
        public void Build(LabModule module)
        {
            var existing = transform.Find("_Environment");
            if (existing) DestroyImmediate(existing.gameObject);

            var root = new GameObject("_Environment");
            root.transform.SetParent(transform, false);
            root.AddComponent<EnvironmentSlot>();

            int envLayer = LayerMask.NameToLayer("Environment");
            if (envLayer < 0) envLayer = 0;

            bool city = module is LabModule.NeonShowcase or LabModule.GiApv or LabModule.Reflections
                        or LabModule.PostProcess or LabModule.QualityTiers or LabModule.RendererFeatures;

            var floor = MakeBox(root.transform, "Floor", new Vector3(0, -0.01f, 1.5f), new Vector3(12, 0.02f, 12), envLayer, true);
            var wet = LabMaterials.WetGround();
            floor.GetComponent<MeshRenderer>().sharedMaterial = wet;
            var planar = floor.AddComponent<PlanarReflectionPlane>();
            planar.targetRenderer = floor.GetComponent<MeshRenderer>();

            if (city)
            {
                MakeBox(root.transform, "BackWall", new Vector3(0, 2.2f, 4.6f), new Vector3(12, 4.5f, 0.2f), envLayer, true)
                    .GetComponent<MeshRenderer>().sharedMaterial = LabMaterials.Wall(new Color(0.08f, 0.07f, 0.1f));
                MakeBox(root.transform, "LeftWall", new Vector3(-5.8f, 2.2f, 1.2f), new Vector3(0.2f, 4.5f, 8), envLayer, true)
                    .GetComponent<MeshRenderer>().sharedMaterial = LabMaterials.Wall(new Color(0.12f, 0.05f, 0.09f));
                MakeBox(root.transform, "RightWall", new Vector3(5.8f, 2.2f, 1.2f), new Vector3(0.2f, 4.5f, 8), envLayer, true)
                    .GetComponent<MeshRenderer>().sharedMaterial = LabMaterials.Wall(new Color(0.05f, 0.08f, 0.12f));

                for (int i = 0; i < 4; i++)
                {
                    var col = MakeBox(root.transform, "Column_" + i,
                        new Vector3(-3.5f + i * 2.3f, 1.5f, 3.4f),
                        new Vector3(0.35f, 3f, 0.35f), envLayer, true);
                    col.GetComponent<MeshRenderer>().sharedMaterial = LabMaterials.Wall(new Color(0.18f, 0.16f, 0.2f));
                }

                var sign = MakeBox(root.transform, "SignBoard", new Vector3(-2.2f, 2.4f, 4.45f), new Vector3(2.2f, 0.7f, 0.08f), envLayer, true);
                sign.GetComponent<MeshRenderer>().sharedMaterial = LabMaterials.Emissive(new Color(1f, 0.12f, 0.45f));

                var kiosk = MakeBox(root.transform, "Kiosk", new Vector3(2.6f, 0.7f, 2.6f), new Vector3(1.4f, 1.4f, 1.1f), envLayer, true);
                kiosk.GetComponent<MeshRenderer>().sharedMaterial = LabMaterials.Wall(new Color(0.15f, 0.16f, 0.2f));
            }

            if (module is LabModule.Reflections or LabModule.NeonShowcase)
            {
                var probeGo = new GameObject("ReflectionProbe_Box");
                probeGo.transform.SetParent(root.transform, false);
                probeGo.transform.position = new Vector3(0, 1.4f, 1.5f);
                var probe = probeGo.AddComponent<ReflectionProbe>();
                probe.mode = UnityEngine.Rendering.ReflectionProbeMode.Realtime;
                probe.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.EveryFrame;
                probe.timeSlicingMode = UnityEngine.Rendering.ReflectionProbeTimeSlicingMode.NoTimeSlicing;
                probe.size = new Vector3(10, 4, 10);
                probe.boxProjection = true;
                probe.intensity = 1f;
                probe.resolution = 256;
                probe.nearClipPlane = 0.3f;
                probe.farClipPlane = 20f;
            }

            if (module == LabModule.GiApv)
            {
                var volume = new GameObject("AdaptiveProbeVolume_Placeholder");
                volume.transform.SetParent(root.transform, false);
                volume.transform.position = new Vector3(0, 1.5f, 1.5f);
                var marker = volume.AddComponent<GiApvMarker>();
                marker.size = new Vector3(12, 4, 12);
            }
        }

        static GameObject MakeBox(Transform parent, string name, Vector3 pos, Vector3 scale, int layer, bool contributeGi)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = scale;
            go.layer = layer;
            go.isStatic = contributeGi;
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            renderer.receiveShadows = true;
            renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.BlendProbes;
            renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.BlendProbes;
            return go;
        }
    }

    /// <summary>
    /// APV itself is a Unity editor object (Light > Adaptive Probe Volume).
    /// This marker shows the intended bake volume in the Scene view.
    /// </summary>
    public class GiApvMarker : MonoBehaviour
    {
        public Vector3 size = Vector3.one * 8f;

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.15f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(Vector3.zero, size);
            Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.8f);
            Gizmos.DrawWireCube(Vector3.zero, size);
        }
    }

    public static class LabMaterials
    {
        static Material _wet;
        static Material _wall;
        static Material _emit;

        public static Material WetGround()
        {
            var preset = Resources.Load<Material>("M_WetGround");
            if (preset != null)
            {
                _wet = new Material(preset) { name = "WetGround_Runtime" };
                return _wet;
            }
            var shader = Shader.Find("RenderingLab/WetGround") ?? Shader.Find("Universal Render Pipeline/Lit");
            _wet = new Material(shader) { name = "WetGround_Runtime" };
            if (_wet.HasProperty("_BaseColor"))
                _wet.SetColor("_BaseColor", new Color(0.07f, 0.08f, 0.1f));
            if (_wet.HasProperty("_Smoothness"))
                _wet.SetFloat("_Smoothness", 0.92f);
            if (_wet.HasProperty("_Metallic"))
                _wet.SetFloat("_Metallic", 0.15f);
            return _wet;
        }

        public static Material Wall(Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var mat = new Material(shader) { name = "Wall_Runtime" };
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            else mat.color = color;
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.25f);
            return mat;
        }

        public static Material Emissive(Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
            var mat = new Material(shader) { name = "Emissive_Runtime" };
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color * 3f);
            else mat.color = color * 3f;
            return mat;
        }

        public static Material Stylized(Color color, bool face, bool hair)
        {
            var preset = Resources.Load<Material>("M_StylizedLit");
            Material mat;
            if (preset != null)
                mat = new Material(preset) { name = "Stylized_Runtime" };
            else
            {
                var shader = Shader.Find("RenderingLab/StylizedLit") ?? Shader.Find("Universal Render Pipeline/Lit");
                mat = new Material(shader) { name = "Stylized_Runtime" };
            }
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_ShadowColor")) mat.SetColor("_ShadowColor", new Color(0.25f, 0.32f, 0.55f, 1f));
            if (mat.HasProperty("_RampThreshold")) mat.SetFloat("_RampThreshold", 0.42f);
            if (mat.HasProperty("_RampSmooth")) mat.SetFloat("_RampSmooth", 0.06f);
            if (mat.HasProperty("_RimColor")) mat.SetColor("_RimColor", new Color(0.55f, 0.85f, 1f, 1f));
            if (mat.HasProperty("_RimPower")) mat.SetFloat("_RimPower", 3.5f);
            if (mat.HasProperty("_RimIntensity")) mat.SetFloat("_RimIntensity", 0.55f);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", hair ? 0.72f : 0.35f);
            if (mat.HasProperty("_UseFaceShadow")) mat.SetFloat("_UseFaceShadow", face ? 1f : 0f);
            if (face) mat.EnableKeyword("_USEFACESHADOW_ON");
            else mat.DisableKeyword("_USEFACESHADOW_ON");
            if (mat.HasProperty("_UseHairAniso")) mat.SetFloat("_UseHairAniso", hair ? 1f : 0f);
            if (hair) mat.EnableKeyword("_USEHAIRANISO_ON");
            else mat.DisableKeyword("_USEHAIRANISO_ON");
            if (mat.HasProperty("_OutlineWidth")) mat.SetFloat("_OutlineWidth", 1.1f);
            if (mat.HasProperty("_OutlineColor")) mat.SetColor("_OutlineColor", new Color(0.05f, 0.03f, 0.08f, 1f));
            return mat;
        }
    }
}
