namespace RenderingLab
{
    public enum QualityTier
    {
        PcHigh = 0,
        PcMid = 1,
        PcLow = 2,
        MobileHigh = 3,
        MobileMid = 4,
        MobileLow = 5
    }

    public static class QualityTierUtil
    {
        public static readonly string[] DisplayNames =
        {
            "PC High",
            "PC Mid",
            "PC Low",
            "Mobile High",
            "Mobile Mid",
            "Mobile Low"
        };

        public static string DisplayName(QualityTier tier) => DisplayNames[(int)tier];

        public static bool IsMobile(QualityTier tier) => (int)tier >= (int)QualityTier.MobileHigh;

        public static bool IsHigh(QualityTier tier) =>
            tier is QualityTier.PcHigh or QualityTier.MobileHigh;

        public static bool IsLow(QualityTier tier) =>
            tier is QualityTier.PcLow or QualityTier.MobileLow;
    }
}
