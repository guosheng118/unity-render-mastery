#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace RenderingLab.Editor
{
    /// <summary>
    /// Creates 6 URP Pipeline / Renderer assets and wires Quality Settings.
    /// Menu: Rendering Lab > Initialize Project
    /// </summary>
    public static class RenderingLabProjectSetup
    {
        const string SettingsDir = "Assets/RenderingLab/Settings";
        const string ResourcesDir = "Assets/RenderingLab/Resources";
        const string CatalogPath = ResourcesDir + "/QualityTierCatalog.asset";

        [InitializeOnLoadMethod]
        static void AutoInit()
        {
            EditorApplication.delayCall += () =>
            {
                if (GraphicsSettings.defaultRenderPipeline == null || HasMissingRendererFeatures())
                    Run(false);
            };
        }

        [MenuItem("Rendering Lab/Initialize Project")]
        public static void MenuInit() => Run(true);

        public static void Run(bool log)
        {
            Directory.CreateDirectory(SettingsDir);
            Directory.CreateDirectory(ResourcesDir);

            var catalog = AssetDatabase.LoadAssetAtPath<QualityTierCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<QualityTierCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }

            var tiers = new[]
            {
                QualityTier.PcHigh, QualityTier.PcMid, QualityTier.PcLow,
                QualityTier.MobileHigh, QualityTier.MobileMid, QualityTier.MobileLow
            };

            for (int i = 0; i < 6; i++)
            {
                string name = QualityTierUtil.DisplayNames[i].Replace(" ", "");
                var renderer = GetOrCreateRenderer(SettingsDir + "/" + name + "_Renderer.asset", tiers[i]);
                var pipeline = GetOrCreatePipeline(SettingsDir + "/" + name + "_Pipeline.asset", renderer, tiers[i]);
                catalog.pipelineAssets[i] = pipeline;
                catalog.rendererData[i] = renderer;
            }

            EditorUtility.SetDirty(catalog);
            AssignQualityPipelines(catalog);
            GraphicsSettings.defaultRenderPipeline = catalog.pipelineAssets[0];
            QualitySettings.renderPipeline = catalog.pipelineAssets[0];
            QualitySettings.SetQualityLevel(0, true);

            EnsureEditorBuildScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            if (log)
                Debug.Log("Rendering Lab: 6 quality pipelines ready. Open 00_Hub and press Play.");
        }

        static UniversalRendererData GetOrCreateRenderer(string path, QualityTier tier)
        {
            var data = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(path);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(data, path);
            }

            data.renderingMode = QualityTierUtil.IsLow(tier) || tier == QualityTier.MobileMid
                ? RenderingMode.Forward
                : RenderingMode.ForwardPlus;
            data.depthPrimingMode = DepthPrimingMode.Disabled;
            data.accurateGbufferNormals = false;
            data.shadowTransparentReceive = !QualityTierUtil.IsLow(tier);
            ClearRendererFeatures(data);
            EditorUtility.SetDirty(data);
            return data;
        }

        static bool HasMissingRendererFeatures()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<QualityTierCatalog>(CatalogPath);
            if (catalog == null || catalog.rendererData == null)
                return false;

            foreach (var data in catalog.rendererData)
            {
                if (data == null) continue;
                var so = new SerializedObject(data);
                var features = so.FindProperty("m_RendererFeatures");
                if (features == null) continue;
                for (int i = 0; i < features.arraySize; i++)
                {
                    var obj = features.GetArrayElementAtIndex(i).objectReferenceValue;
                    if (obj == null || obj is not ScriptableRendererFeature)
                        return true;
                }
            }
            return false;
        }

        static void ClearRendererFeatures(UniversalRendererData data)
        {
            var so = new SerializedObject(data);
            var features = so.FindProperty("m_RendererFeatures");
            var map = so.FindProperty("m_RendererFeatureMap");
            if (features == null) return;

            for (int i = features.arraySize - 1; i >= 0; i--)
            {
                var obj = features.GetArrayElementAtIndex(i).objectReferenceValue;
                if (obj != null)
                    Object.DestroyImmediate(obj, true);
            }

            features.arraySize = 0;
            if (map != null)
                map.arraySize = 0;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static UniversalRenderPipelineAsset GetOrCreatePipeline(string path, UniversalRendererData renderer, QualityTier tier)
        {
            var asset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(path);
            if (asset == null)
            {
                asset = UniversalRenderPipelineAsset.Create(renderer);
                AssetDatabase.CreateAsset(asset, path);
            }

            asset.msaaSampleCount = 1;
            asset.renderScale = new[] { 1f, 1f, 0.85f, 0.9f, 0.75f, 0.6f }[(int)tier];
            asset.supportsHDR = !QualityTierUtil.IsLow(tier);
            asset.shadowDistance = QualityTierUtil.IsMobile(tier) ? 25f : 80f;
            asset.shadowCascadeCount = QualityTierUtil.IsHigh(tier) ? 4 : QualityTierUtil.IsLow(tier) ? 1 : 2;
            asset.mainLightShadowmapResolution = new[] { 4096, 2048, 1024, 2048, 1024, 512 }[(int)tier];
            asset.maxAdditionalLightsCount = QualityTierUtil.IsHigh(tier) ? 8 : 4;
            asset.colorGradingMode = QualityTierUtil.IsMobile(tier) ? ColorGradingMode.LowDynamicRange : ColorGradingMode.HighDynamicRange;
            asset.colorGradingLutSize = QualityTierUtil.IsLow(tier) ? 16 : 32;
            asset.supportsCameraOpaqueTexture = !QualityTierUtil.IsLow(tier);
            asset.supportsCameraDepthTexture = !QualityTierUtil.IsLow(tier);

            bool mainShadows = tier != QualityTier.MobileLow;
            bool additionalShadows = QualityTierUtil.IsHigh(tier);
            bool probeBlend = QualityTierUtil.IsHigh(tier) || tier == QualityTier.PcMid;
            bool additionalPerPixel = !(QualityTierUtil.IsLow(tier) || tier == QualityTier.MobileMid);

            var so = new SerializedObject(asset);
            SetBool(so, "m_MainLightShadowsSupported", mainShadows);
            SetEnum(so, "m_AdditionalLightsRenderingMode", additionalPerPixel
                ? LightRenderingMode.PerPixel
                : LightRenderingMode.Disabled);
            SetBool(so, "m_AdditionalLightShadowsSupported", additionalShadows);
            SetBool(so, "m_AnyShadowsSupported", mainShadows || additionalShadows);
            SetBool(so, "m_ReflectionProbeBlending", probeBlend);
            SetBool(so, "m_ReflectionProbeBoxProjection", probeBlend);
            SetBool(so, "m_SoftShadowsSupported", !QualityTierUtil.IsLow(tier));

            var list = so.FindProperty("m_RendererDataList");
            if (list != null)
            {
                if (list.arraySize < 1) list.arraySize = 1;
                list.GetArrayElementAtIndex(0).objectReferenceValue = renderer;
            }
            var probe = so.FindProperty("m_LightProbeSystem");
            if (probe != null)
                probe.intValue = QualityTierUtil.IsLow(tier) ? 0 : 1;
            var upscale = so.FindProperty("m_UpscalingFilter");
            if (upscale != null)
                upscale.intValue = tier == QualityTier.PcHigh ? 4 : 0;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
            return asset;
        }

        static void AssignQualityPipelines(QualityTierCatalog catalog)
        {
            int current = QualitySettings.GetQualityLevel();
            for (int i = 0; i < Mathf.Min(6, QualitySettings.names.Length); i++)
            {
                QualitySettings.SetQualityLevel(i, false);
                QualitySettings.renderPipeline = catalog.pipelineAssets[i];
            }
            QualitySettings.SetQualityLevel(current, true);
        }

        static void EnsureEditorBuildScenes()
        {
            string[] paths =
            {
                "Assets/RenderingLab/Scenes/00_Hub.unity",
                "Assets/RenderingLab/Scenes/01_Lighting.unity",
                "Assets/RenderingLab/Scenes/02_GI_APV.unity",
                "Assets/RenderingLab/Scenes/03_Reflections.unity",
                "Assets/RenderingLab/Scenes/04_RendererFeatures.unity",
                "Assets/RenderingLab/Scenes/05_PostProcess.unity",
                "Assets/RenderingLab/Scenes/06_QualityTiers.unity",
                "Assets/RenderingLab/Scenes/07_NeonShowcase.unity"
            };
            var list = new EditorBuildSettingsScene[paths.Length];
            for (int i = 0; i < paths.Length; i++)
                list[i] = new EditorBuildSettingsScene(paths[i], true);
            EditorBuildSettings.scenes = list;
        }

        static void SetBool(SerializedObject so, string property, bool value)
        {
            var p = so.FindProperty(property);
            if (p != null)
                p.boolValue = value;
        }

        static void SetEnum(SerializedObject so, string property, System.Enum value)
        {
            var p = so.FindProperty(property);
            if (p != null)
                p.intValue = System.Convert.ToInt32(value);
        }
    }
}
#endif
