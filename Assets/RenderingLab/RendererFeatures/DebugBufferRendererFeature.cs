using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace RenderingLab
{
    /// <summary>
    /// Fullscreen debug overlay. Modes are driven by _LabDebugMode (HUD buttons).
    /// The actual buffers are encoded in StylizedLit when debug is on; this pass
    /// only composites a legend. Kept as a Render Graph sample you can copy.
    /// </summary>
    public class DebugBufferRendererFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class Settings
        {
            public RenderPassEvent passEvent = RenderPassEvent.AfterRenderingPostProcessing;
            public Shader overlayShader;
        }

        public Settings settings = new Settings();
        DebugPass _pass;
        Material _mat;

        public override void Create()
        {
            if (settings.overlayShader == null)
                settings.overlayShader = Shader.Find("RenderingLab/DebugOverlay");
            if (settings.overlayShader != null)
                _mat = CoreUtils.CreateEngineMaterial(settings.overlayShader);
            _pass = new DebugPass(_mat) { renderPassEvent = settings.passEvent };
        }

        protected override void Dispose(bool disposing)
        {
            if (_mat != null) CoreUtils.Destroy(_mat);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (_mat == null) return;
            if (Shader.GetGlobalFloat("_LabDebugMode") < 0.5f) return;
            renderer.EnqueuePass(_pass);
        }

        class DebugPass : ScriptableRenderPass
        {
            readonly Material _mat;

            class PassData
            {
                public Material material;
            }

            public DebugPass(Material mat) { _mat = mat; }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
                using (var builder = renderGraph.AddRasterRenderPass<PassData>("Lab Debug Overlay", out var passData))
                {
                    passData.material = _mat;
                    builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
                    builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                    {
                        ctx.cmd.DrawProcedural(Matrix4x4.identity, data.material, 0, MeshTopology.Triangles, 3, 1);
                    });
                }
            }
        }
    }
}
