using UnityEngine;
using UnityEngine.SceneManagement;

namespace RenderingLab
{
    /// <summary>
    /// Always-on IMGUI HUD: quality tiers, scene jump, debug buffers.
    /// </summary>
    public class QualityHud : MonoBehaviour
    {
        public bool show = true;
        [Range(0.6f, 1.6f)] public float uiScale = 1f;

        static readonly string[] SceneNames =
        {
            "00_Hub",
            "01_Lighting",
            "02_GI_APV",
            "03_Reflections",
            "04_RendererFeatures",
            "05_PostProcess",
            "06_QualityTiers",
            "07_NeonShowcase"
        };

        void OnGUI()
        {
            if (!show) return;

            var prev = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * uiScale);

            const int w = 360;
            GUILayout.BeginArea(new Rect(12, 12, w, Screen.height / uiScale - 24), GUI.skin.box);
            GUILayout.Label("Unity 6.3 渲染实验室");
            GUILayout.Label(SceneManager.GetActiveScene().name);

            var qc = QualityTierController.Instance;
            if (qc == null)
            {
                GUILayout.Label("没有 QualityTierController。会在场景启动时自动创建。");
            }
            else
            {
                GUILayout.Label(QualityTierController.Describe(qc.current, qc.catalog));
                GUILayout.BeginHorizontal();
                for (int i = 0; i < 6; i++)
                {
                    var tier = (QualityTier)i;
                    string label = i < 3 ? $"PC{i + 1}" : $"M{i - 2}";
                    if (GUILayout.Button(label))
                        qc.Apply(tier);
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(6);
            GUILayout.Label("场景");
            GUILayout.BeginHorizontal();
            for (int i = 0; i < SceneNames.Length; i++)
            {
                string shortName = SceneNames[i].Substring(0, 2);
                if (GUILayout.Button(shortName))
                    SceneManager.LoadScene(SceneNames[i]);
                if (i == 3)
                {
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            GUILayout.Label("Debug Buffer  (4 课 / Rendering Debugger 对照)");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Off")) Shader.SetGlobalFloat("_LabDebugMode", 0);
            if (GUILayout.Button("Albedo")) Shader.SetGlobalFloat("_LabDebugMode", 1);
            if (GUILayout.Button("NdotL")) Shader.SetGlobalFloat("_LabDebugMode", 2);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Shadow")) Shader.SetGlobalFloat("_LabDebugMode", 3);
            if (GUILayout.Button("GI")) Shader.SetGlobalFloat("_LabDebugMode", 4);
            if (GUILayout.Button("Rim")) Shader.SetGlobalFloat("_LabDebugMode", 5);
            GUILayout.EndHorizontal();

            GUILayout.Space(4);
            GUILayout.Label("H 隐藏 HUD   N 下一档");
            GUILayout.EndArea();
            GUI.matrix = prev;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
                show = !show;
            if (Input.GetKeyDown(KeyCode.N) && QualityTierController.Instance != null)
                QualityTierController.Instance.CycleNext();
        }
    }
}
