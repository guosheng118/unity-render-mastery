using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace RenderingLab
{
    /// <summary>
    /// Runtime quality switch: Quality Settings level + URP Asset.
    /// Catalog is created by Rendering Lab > Initialize Project.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class QualityTierController : MonoBehaviour
    {
        public static QualityTierController Instance { get; private set; }

        public QualityTierCatalog catalog;
        public QualityTier current = QualityTier.PcHigh;
        public bool applyOnAwake = true;

        public static event System.Action<QualityTier> TierChanged;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (catalog == null)
                catalog = Resources.Load<QualityTierCatalog>("QualityTierCatalog");
            if (applyOnAwake)
                Apply(current);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void Apply(QualityTier tier)
        {
            current = tier;
            int index = Mathf.Clamp((int)tier, 0, QualitySettings.names.Length - 1);
            QualitySettings.SetQualityLevel(index, true);

            if (catalog != null)
            {
                var pipeline = catalog.GetPipeline(tier);
                if (pipeline != null)
                {
                    GraphicsSettings.defaultRenderPipeline = pipeline;
                    QualitySettings.renderPipeline = pipeline;
                }
            }

            TierChanged?.Invoke(tier);
        }

        public void CycleNext()
        {
            int next = ((int)current + 1) % 6;
            Apply((QualityTier)next);
        }

        public static string Describe(QualityTier tier, QualityTierCatalog catalog)
        {
            if (catalog == null)
                return QualityTierUtil.DisplayName(tier);
            int i = (int)tier;
            return
                $"{QualityTierUtil.DisplayName(tier)}\n" +
                $"Path {catalog.renderingPath[i]}  Scale {catalog.renderScale[i]:0.00}\n" +
                $"Shadow {catalog.shadowMapSize[i]}  HDR {(catalog.hdr[i] ? "ON" : "off")}  " +
                $"AddLights {(catalog.additionalLights[i] ? "ON" : "off")}  STP {(catalog.stp[i] ? "ON" : "off")}";
        }
    }
}
