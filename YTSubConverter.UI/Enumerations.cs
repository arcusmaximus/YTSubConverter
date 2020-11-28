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

    public enum ShadowType
    {
        Glow,
        Bevel,
        HardShadow,
        SoftShadow
    }

    public enum OffsetType
    {
        Regular,
        Subscript,
        Superscript
    }

    public enum RubyPosition
    {
        None,
        Above,
        Below
    }

    public enum RubyPart
    {
        None,
        Text,
        Parenthesis,
        RubyAbove,
        RubyBelow
    }

    public enum HorizontalTextDirection
    {
        LeftToRight,
        RightToLeft
    }

    public enum VerticalTextType
    {
        None,
        Positioned,
        Rotated
    }
}
