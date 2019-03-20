using System;

namespace Arc.YTSubConverter
{
    public enum AnchorPoint
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        Center,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }

    [Flags]
    public enum ShadowType
    {
        None = 0,
        Glow = 1,
        HardShadow = 2,
        SoftShadow = 4
    }

    public enum KaraokeType
    {
        Simple,
        Fade,
        Glitch
    }
}
