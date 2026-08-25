using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace RenderingLab
{
    /// <summary>
    /// Marker feature: documents that planar capture is driven by PlanarReflectionPlane.
    /// Keeps a slot on UniversalRendererData so TA students see it next to SSAO / Outline.
    /// </summary>
    public class PlanarReflectionRendererFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class Settings
        {
            [Range(0.25f, 1f)] public float defaultScale = 0.5f;
            public bool enabled = true;
        }

        public Settings settings = new Settings();

        public override void Create() { }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!settings.enabled)
                Shader.SetGlobalFloat("_LabPlanarEnabled", 0f);
        }
    }
}
