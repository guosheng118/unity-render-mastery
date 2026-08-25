using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace RenderingLab
{
    [CreateAssetMenu(menuName = "Rendering Lab/Quality Tier Catalog", fileName = "QualityTierCatalog")]
    public class QualityTierCatalog : ScriptableObject
    {
        public UniversalRenderPipelineAsset[] pipelineAssets = new UniversalRenderPipelineAsset[6];
        public UniversalRendererData[] rendererData = new UniversalRendererData[6];

        [Header("Feature flags (mirrors what the URP assets should enable)")]
        public bool[] planarReflection = { true, true, false, false, false, false };
        public bool[] outline = { true, true, true, true, true, false };
        public bool[] ssao = { true, true, false, true, false, false };
        public bool[] bloom = { true, true, true, true, true, false };
        public bool[] stp = { true, false, false, false, false, false };
        public bool[] additionalLights = { true, true, false, true, false, false };

        public string[] renderingPath =
        {
            "Forward+",
            "Forward+",
            "Forward",
            "Forward+",
            "Forward",
            "Forward"
        };

        public int[] shadowMapSize = { 4096, 2048, 1024, 2048, 1024, 512 };
        public float[] renderScale = { 1f, 1f, 0.85f, 0.9f, 0.75f, 0.6f };

        public UniversalRenderPipelineAsset GetPipeline(QualityTier tier)
        {
            int i = (int)tier;
            if (pipelineAssets == null || i >= pipelineAssets.Length) return null;
            return pipelineAssets[i];
        }
    }
}
