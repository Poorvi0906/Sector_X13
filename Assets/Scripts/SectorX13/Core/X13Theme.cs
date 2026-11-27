using UnityEngine;

namespace SectorX13.Core
{
    /// <summary>
    /// Centralized visual theme constants matching the SECTOR X-13 dark-navy and cyan UI specification.
    /// </summary>
    public static class X13Theme
    {
        public static readonly Color BackgroundNavy = new Color(0.020f, 0.055f, 0.090f, 1f);
        public static readonly Color HeaderNavy = new Color(0.015f, 0.043f, 0.075f, 0.98f);
        public static readonly Color CardSurface = new Color(0.039f, 0.110f, 0.165f, 1f);
        public static readonly Color CardElevated = new Color(0.055f, 0.145f, 0.215f, 1f);
        public static readonly Color BorderSubtle = new Color(0.086f, 0.247f, 0.329f, 1f);
        public static readonly Color CyanAccent = new Color(0.020f, 0.851f, 0.961f, 1f);
        public static readonly Color CyanDim = new Color(0.020f, 0.550f, 0.650f, 1f);
        public static readonly Color ButtonPrimaryBg = new Color(0.020f, 0.780f, 0.920f, 1f);
        public static readonly Color ButtonSecondaryBg = new Color(0.055f, 0.160f, 0.230f, 1f);
        public static readonly Color ButtonDisabledBg = new Color(0.045f, 0.095f, 0.135f, 0.75f);

        public static readonly Color TextPrimary = new Color(0.957f, 0.973f, 0.984f, 1f);
        public static readonly Color TextSecondary = new Color(0.561f, 0.659f, 0.722f, 1f);
        public static readonly Color TextDark = new Color(0.015f, 0.050f, 0.085f, 1f);

        // Risk colors (always paired with explicit text labels per accessibility/safety spec)
        public static readonly Color RiskLow = new Color(0.098f, 0.851f, 0.549f, 1f);
        public static readonly Color RiskMedium = new Color(0.961f, 0.651f, 0.137f, 1f);
        public static readonly Color RiskHigh = new Color(0.949f, 0.282f, 0.282f, 1f);
        public static readonly Color RiskUnknown = new Color(0.561f, 0.659f, 0.722f, 1f);

        public const float ReferenceWidth = 1080f;
        public const float ReferenceHeight = 1920f;
        public const float HeaderHeight = 140f;
        public const float BottomNavHeight = 148f;

        private static Font _cachedFont;

        public static Font DefaultFont
        {
            get
            {
                if (_cachedFont == null)
                {
                    _cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    if (_cachedFont == null)
                    {
                        _cachedFont = Font.CreateDynamicFontFromOSFont("Segoe UI", 24);
                    }
                }
                return _cachedFont;
            }
        }
    }
}
