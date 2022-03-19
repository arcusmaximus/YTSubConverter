namespace YTSubConverter.Shared
{
    public enum LineMergeType
    {
        MoveNew,
        MoveExisting
    }

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

    public enum RubyPart
    {
        None,
        Base,
        Parenthesis,
        TextBefore,
        TextAfter
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
