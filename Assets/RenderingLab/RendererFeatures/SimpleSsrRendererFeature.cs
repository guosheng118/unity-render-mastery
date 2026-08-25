using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace RenderingLab
{
    /// <summary>
    /// Teaching SSR (screen-space ray march). Not production ZZZ quality.
    /// Unity's official URP SSR is scheduled around 6.7 — use probes + planar on 6.3.
    /// </summary>
    public class SimpleSsrRendererFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class Settings
        {
            public bool enabled;
            [Range(8, 64)] public int maxSteps = 24;
            [Range(0.1f, 1f)] public float intensity = 0.35f;
            public Shader shader;
            public RenderPassEvent passEvent = RenderPassEvent.BeforeRenderingTransparents;
        }

        public Settings settings = new Settings();
        Material _mat;
        SsrPass _pass;

        public override void Create()
        {
            if (settings.shader == null)
                settings.shader = Shader.Find("RenderingLab/SimpleSSR");
            if (settings.shader != null)
                _mat = CoreUtils.CreateEngineMaterial(settings.shader);
            _pass = new SsrPass(_mat, settings);
            _pass.renderPassEvent = settings.passEvent;
        }

        protected override void Dispose(bool disposing)
        {
            if (_mat != null) CoreUtils.Destroy(_mat);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!settings.enabled || _mat == null) return;
            _pass.ConfigureInput(ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Normal);
            renderer.EnqueuePass(_pass);
        }

        class SsrPass : ScriptableRenderPass
        {
            readonly Material _mat;
            readonly Settings _settings;

            class PassData
            {
                public Material material;
                public TextureHandle color;
            }

            public SsrPass(Material mat, Settings settings)
            {
                _mat = mat;
                _settings = settings;
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
                using (var builder = renderGraph.AddRasterRenderPass<PassData>("Lab Simple SSR", out var passData))
                {
                    passData.material = _mat;
                    passData.color = resourceData.activeColorTexture;
                    builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
                    builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                    {
                        data.material.SetFloat("_MaxSteps", _settings.maxSteps);
                        data.material.SetFloat("_Intensity", _settings.intensity);
                        ctx.cmd.DrawProcedural(Matrix4x4.identity, data.material, 0, MeshTopology.Triangles, 3, 1);
                    });
                }
            }
        }
    }
}
