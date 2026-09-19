using UnityEngine;

namespace RenderLab
{
    /// <summary>
    /// 挂在材质子资源上：白天/夜晚各 5 条渐变。运行时只采样烘好的 Ramp 贴图。
    /// </summary>
    public class GenshinRampData : ScriptableObject
    {
        public const int RowsPerMode = 5;

        public Gradient[] day = new Gradient[RowsPerMode];
        public Gradient[] night = new Gradient[RowsPerMode];

        public void Ensure()
        {
            day = EnsureArray(day, true);
            night = EnsureArray(night, false);
        }

        static Gradient[] EnsureArray(Gradient[] src, bool isDay)
        {
            var dst = new Gradient[RowsPerMode];
            for (int i = 0; i < RowsPerMode; i++)
            {
                if (src != null && i < src.Length && src[i] != null && src[i].colorKeys != null && src[i].colorKeys.Length > 0)
                    dst[i] = src[i];
                else
                    dst[i] = DefaultRow(i, isDay);
            }
            return dst;
        }

        static Gradient DefaultRow(int row, bool isDay)
        {
            var g = new Gradient();
            Color shadow = isDay
                ? new Color(0.38f, 0.32f, 0.32f)
                : new Color(0.18f, 0.20f, 0.32f);
            if (row == 2)
                shadow = isDay
                    ? new Color(0.42f, 0.45f, 0.62f)
                    : new Color(0.16f, 0.18f, 0.36f);

            g.SetKeys(
                new[]
                {
                    new GradientColorKey(shadow, 0f),
                    new GradientColorKey(Color.white, 0.55f),
                    new GradientColorKey(Color.white, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 1f)
                });
            return g;
        }
    }
}
