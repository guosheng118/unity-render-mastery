#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace RenderingLab.Editor
{
    /// <summary>
    /// First-open wizard. Creates 6 URP assets, wires Quality Settings, features, and a Resources catalog.
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
                if (GraphicsSettings.defaultRenderPipeline == null)
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
                Debug.Log("Rendering Lab initialized. Open 00_Hub and press Play.");
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

            AddFeature<OutlineRendererFeature>(data);
            AddFeature<PlanarReflectionRendererFeature>(data);
            AddFeature<DebugBufferRendererFeature>(data);
            var ssr = AddFeature<SimpleSsrRendererFeature>(data);
            if (ssr != null)
                ssr.settings.enabled = false;

            TryAddSsao(data, catalogSsao: !QualityTierUtil.IsLow(tier) && tier != QualityTier.MobileMid);

            EditorUtility.SetDirty(data);
            return data;
        }

        static T AddFeature<T>(UniversalRendererData data) where T : ScriptableRendererFeature
        {
            var so = new SerializedObject(data);
            var features = so.FindProperty("m_RendererFeatures");
            if (features != null)
            {
                for (int i = 0; i < features.arraySize; i++)
                {
                    var obj = features.GetArrayElementAtIndex(i).objectReferenceValue as T;
                    if (obj != null) return obj;
                }
            }

            var feature = ScriptableObject.CreateInstance<T>();
            feature.name = typeof(T).Name;
            feature.SetActive(true);
            AssetDatabase.AddObjectToAsset(feature, data);
            InsertRendererFeature(data, feature);
            EditorUtility.SetDirty(data);
            return feature;
        }

        static void TryAddSsao(UniversalRendererData data, bool catalogSsao)
        {
            var so = new SerializedObject(data);
            var features = so.FindProperty("m_RendererFeatures");
            if (features != null)
            {
                for (int i = 0; i < features.arraySize; i++)
                {
                    var obj = features.GetArrayElementAtIndex(i).objectReferenceValue;
                    if (obj != null && obj.GetType().Name.Contains("AmbientOcclusion"))
                    {
                        ((ScriptableRendererFeature)obj).SetActive(catalogSsao);
                        return;
                    }
                }
            }

            var type = System.Type.GetType("UnityEngine.Rendering.Universal.ScreenSpaceAmbientOcclusion, Unity.RenderPipelines.Universal.Runtime");
            if (type == null) return;
            var feature = (ScriptableRendererFeature)ScriptableObject.CreateInstance(type);
            feature.name = "ScreenSpaceAmbientOcclusion";
            feature.SetActive(catalogSsao);
            AssetDatabase.AddObjectToAsset(feature, data);
            InsertRendererFeature(data, feature);
        }

        static void InsertRendererFeature(UniversalRendererData data, ScriptableRendererFeature feature)
        {
            var so = new SerializedObject(data);
            var features = so.FindProperty("m_RendererFeatures");
            var map = so.FindProperty("m_RendererFeatureMap");
            if (features == null) return;
            features.arraySize++;
            features.GetArrayElementAtIndex(features.arraySize - 1).objectReferenceValue = feature;
            if (map != null)
            {
                map.arraySize = features.arraySize;
                map.GetArrayElementAtIndex(map.arraySize - 1).longValue = feature.GetInstanceID();
            }
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
            asset.supportsMainLightShadows = tier != QualityTier.MobileLow;
            asset.mainLightShadowmapResolution = new[] { 4096, 2048, 1024, 2048, 1024, 512 }[(int)tier];
            asset.additionalLightsRenderingMode = QualityTierUtil.IsLow(tier) || tier == QualityTier.MobileMid
                ? LightRenderingMode.Disabled
                : LightRenderingMode.PerPixel;
            asset.maxAdditionalLightsCount = QualityTierUtil.IsHigh(tier) ? 8 : 4;
            asset.supportsAdditionalLightShadows = QualityTierUtil.IsHigh(tier);
            asset.reflectionProbeBlending = QualityTierUtil.IsHigh(tier) || tier == QualityTier.PcMid;
            asset.reflectionProbeBoxProjection = QualityTierUtil.IsHigh(tier) || tier == QualityTier.PcMid;
            asset.supportsSoftShadows = !QualityTierUtil.IsLow(tier);
            asset.colorGradingMode = QualityTierUtil.IsMobile(tier) ? ColorGradingMode.LowDynamicRange : ColorGradingMode.HighDynamicRange;
            asset.colorGradingLutSize = QualityTierUtil.IsLow(tier) ? 16 : 32;
            asset.supportsCameraOpaqueTexture = true;
            asset.supportsCameraDepthTexture = true;

            var so = new SerializedObject(asset);
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
                upscale.intValue = tier == QualityTier.PcHigh ? 4 : 0; // 4 often STP in Unity 6 enums; verify in Inspector
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
    }
}
#endif
