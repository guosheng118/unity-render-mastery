using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace RenderingLab
{
    /// <summary>
    /// Render Graph outline: draws LightMode=Outline on the Character layer with inverted hull.
    /// </summary>
    public class OutlineRendererFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class Settings
        {
            public LayerMask layerMask = -1;
            public RenderPassEvent passEvent = RenderPassEvent.AfterRenderingOpaques;
            public string shaderName = "RenderingLab/StylizedLit";
        }

        public Settings settings = new Settings();
        OutlinePass _pass;
        static readonly ShaderTagId kOutline = new ShaderTagId("Outline");

        public override void Create()
        {
            _pass = new OutlinePass(settings, kOutline);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (Shader.GetGlobalFloat("_LabOutlineEnabled") < 0.5f && Application.isPlaying)
                return;
            renderer.EnqueuePass(_pass);
        }

        class OutlinePass : ScriptableRenderPass
        {
            readonly Settings _settings;
            readonly ShaderTagId _tag;
            readonly FilteringSettings _filtering;

            class PassData
            {
                public RendererListHandle rendererList;
            }

            public OutlinePass(Settings settings, ShaderTagId tag)
            {
                _settings = settings;
                _tag = tag;
                renderPassEvent = settings.passEvent;
                int mask = settings.layerMask.value;
                if (LayerMask.NameToLayer("Character") >= 0)
                    mask = 1 << LayerMask.NameToLayer("Character");
                _filtering = new FilteringSettings(RenderQueueRange.opaque, mask);
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
                UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
                UniversalLightData lightData = frameData.Get<UniversalLightData>();
                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

                var drawing = RenderingUtils.CreateDrawingSettings(_tag, renderingData, cameraData, lightData, SortingCriteria.CommonOpaque);
                var param = new RendererListParams(renderingData.cullResults, drawing, _filtering);
                var list = renderGraph.CreateRendererList(param);

                using (var builder = renderGraph.AddRasterRenderPass<PassData>("Lab Outline", out var passData))
                {
                    passData.rendererList = list;
                    builder.UseRendererList(list);
                    builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
                    builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.Read);
                    builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                    {
                        ctx.cmd.DrawRendererList(data.rendererList);
                    });
                }
            }
        }
    }
}
