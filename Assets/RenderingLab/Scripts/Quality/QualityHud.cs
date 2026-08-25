using UnityEngine;
using UnityEngine.SceneManagement;

namespace RenderingLab
{
    public class QualityHud : MonoBehaviour
    {
        public bool show = true;
        [Range(0.6f, 1.6f)] public float uiScale = 1f;

        void OnGUI()
        {
            if (!show) return;

            var prev = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * uiScale);

            GUILayout.BeginArea(new Rect(12, 12, 360, 220), GUI.skin.box);
            GUILayout.Label("Quality Tiers");
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
                    string label = i < 3 ? $"PC{i + 1}" : $"M{i - 2}";
                    if (GUILayout.Button(label))
                        qc.Apply((QualityTier)i);
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(6);
            GUILayout.Label("H 隐藏 HUD   N 下一档   右键拖视角");
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
