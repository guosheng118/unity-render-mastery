using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RenderLab
{
    /// <summary>
    /// Scene 可视化：主光方向 L、丢掉 Y 后的 L.xz、勾股斜边 length(L.xz)、以及除完后的单位水平方向。
    /// 挂在角色头附近；灯光不指定时用 RenderSettings.sun。
    /// Shader 里 GetMainLight().direction 是「指向灯光」，等于 -light.transform.forward。
    /// </summary>
    [ExecuteAlways]
    public class LightXzGizmo : MonoBehaviour
    {
        [Tooltip("留空则用场景太阳 / 第一盏 Directional")]
        public Light targetLight;

        [Tooltip("箭头长度（米）")]
        public float scale = 1.2f;

        [Tooltip("与脸 shader 一致：世界 +X 为脸朝前")]
        public bool characterForwardWorldPlusX = true;

        [Header("显示开关")]
        [Tooltip("未归一化的 XYZ：从原点沿世界轴画出 L.x / L.y / L.z 三条分量（长度就是分量值，不是 1）")]
        public bool showUnnormalizedXyz = true;

        [Tooltip("归一化后的水平方向：L.xz / length，shader 拿去和脸做点积的那条")]
        public bool showNormalizedXz = true;

        public bool showGameViewOverlay = true;

        const float Epsilon = 1e-4f;

        Light ResolveLight()
        {
            if (targetLight != null)
                return targetLight;
            if (RenderSettings.sun != null)
                return RenderSettings.sun;

            Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i].type == LightType.Directional)
                    return lights[i];
            }
            return null;
        }

        // 与 URP GetMainLight().direction 相同：从表面指向灯。
        static Vector3 ShaderLightDirection(Light light)
        {
            return -light.transform.forward.normalized;
        }

        void OnGUI()
        {
            if (!showGameViewOverlay)
                return;

            Light light = ResolveLight();
            if (light == null)
                return;

            Vector3 L = ShaderLightDirection(light);
            Vector2 Lxz = new Vector2(L.x, L.z);
            float lxzLen = Lxz.magnitude;
            Vector2 LxzUnit = lxzLen > Epsilon ? Lxz / lxzLen : new Vector2(1f, 0f);
            bool usedFallback = lxzLen <= Epsilon;

            const int w = 420;
            GUILayout.BeginArea(new Rect(12, 12, w, 230), GUI.skin.box);
            GUILayout.Label("Light XZ  (与 Face shader 同一套数)");
            GUILayout.Label($"未归一化 XYZ 开关: {(showUnnormalizedXyz ? "开" : "关")}    归一化 XZ 开关: {(showNormalizedXz ? "开" : "关")}");
            GUILayout.Label($"L     = ({L.x:F3}, {L.y:F3}, {L.z:F3})   |L|={L.magnitude:F3}  ← 已经是单位向量");
            GUILayout.Label($"L.xz  = ({Lxz.x:F3}, {Lxz.y:F3})          ← 丢掉 Y 的二维向量，还不是长度 1");
            GUILayout.Label($"length(L.xz) = {lxzLen:F3}               ← 勾股斜边 √(x²+z²)，是一个数");
            GUILayout.Label(usedFallback
                ? "xz 太短，fallback → (1, 0) 当成正前"
                : $"L.xz / length = ({LxzUnit.x:F3}, {LxzUnit.y:F3})   ← 这才归 1，方向不变");
            GUILayout.Label($"脸朝前 F.xz = (1, 0)   front=dot = {Vector2.Dot(LxzUnit, Vector2.right):F3}   (-1后 / 0侧 / 1前)");
            GUILayout.EndArea();
        }

        void OnDrawGizmos()
        {
            Light light = ResolveLight();
            if (light == null)
                return;

            Vector3 o = transform.position;
            Vector3 L = ShaderLightDirection(light);
            Vector3 Lxz3 = new Vector3(L.x, 0f, L.z);
            float lxzLen = Lxz3.magnitude;
            Vector3 LxzUnit = lxzLen > Epsilon
                ? Lxz3 / lxzLen
                : (characterForwardWorldPlusX ? Vector3.right : new Vector3(transform.right.x, 0f, transform.right.z).normalized);
            Vector3 LTip = o + L * scale;
            Vector3 projTip = o + Lxz3 * scale;
            Vector3 xEnd = o + Vector3.right * (L.x * scale);
            Vector3 yEnd = o + Vector3.up * (L.y * scale);
            Vector3 zEnd = o + Vector3.forward * (L.z * scale);
            Vector3 unitTip = o + LxzUnit * scale;
            Vector3 faceFwd = characterForwardWorldPlusX
                ? Vector3.right
                : new Vector3(transform.right.x, 0f, transform.right.z).normalized;

            // 世界 XZ 平面参考
            Gizmos.color = new Color(1f, 1f, 1f, 0.12f);
            Gizmos.DrawLine(o + Vector3.right * scale, o - Vector3.right * scale);
            Gizmos.DrawLine(o + Vector3.forward * scale, o - Vector3.forward * scale);

            // 完整灯光方向 L（三维，本身已是单位向量）
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(o, LTip);
            Gizmos.DrawSphere(LTip, 0.03f * scale);

            if (showUnnormalizedXyz)
            {
                // 未归一化 XYZ：三条轴上的分量，长度 = 各自的值（转顶光时 Y 变长、X/Z 变短）
                Gizmos.color = Color.red;
                Gizmos.DrawLine(o, xEnd);
                Gizmos.DrawSphere(xEnd, 0.025f * scale);

                Gizmos.color = new Color(0.2f, 0.85f, 0.2f);
                Gizmos.DrawLine(o, yEnd);
                Gizmos.DrawSphere(yEnd, 0.025f * scale);

                Gizmos.color = Color.blue;
                Gizmos.DrawLine(o, zEnd);
                Gizmos.DrawSphere(zEnd, 0.025f * scale);

                // 丢掉 Y 后的地面斜边 = length(L.xz)
                Gizmos.color = new Color(1f, 0.55f, 0f, 0.9f);
                Gizmos.DrawLine(LTip, projTip);
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(o, projTip);
            }

            if (showNormalizedXz)
            {
                Gizmos.color = lxzLen > Epsilon ? Color.green : Color.magenta;
                Gizmos.DrawLine(o, unitTip);
                Gizmos.DrawSphere(unitTip, 0.04f * scale);
            }

            Gizmos.color = Color.white;
            Gizmos.DrawLine(o, o + faceFwd * scale * 0.55f);

#if UNITY_EDITOR
            Handles.color = Color.yellow;
            Handles.Label(LTip + Vector3.up * 0.06f, $"L  ({L.x:F2}, {L.y:F2}, {L.z:F2})");

            if (showUnnormalizedXyz)
            {
                Handles.color = Color.red;
                Handles.Label(xEnd, $"未归一化 Lx = {L.x:F2}");
                Handles.color = new Color(0.2f, 0.85f, 0.2f);
                Handles.Label(yEnd, $"未归一化 Ly = {L.y:F2}");
                Handles.color = Color.blue;
                Handles.Label(zEnd, $"未归一化 Lz = {L.z:F2}");
                Handles.color = Color.cyan;
                Handles.Label(projTip + Vector3.up * 0.04f, $"L.xz 斜边 length = {lxzLen:F3}  (还不是 1)");
                Handles.color = new Color(1f, 0.55f, 0f);
                Handles.Label((LTip + projTip) * 0.5f, "丢掉的 Y");
            }

            if (showNormalizedXz)
            {
                Handles.color = Color.green;
                Handles.Label(unitTip + Vector3.up * 0.08f,
                    lxzLen > Epsilon ? "归一化 L.xz / length  (长度=1)" : "fallback (1,0) 顶光保护");
            }

            Handles.color = Color.white;
            Handles.Label(o + faceFwd * scale * 0.55f, "脸朝前 +X");
#endif
        }
    }
}
