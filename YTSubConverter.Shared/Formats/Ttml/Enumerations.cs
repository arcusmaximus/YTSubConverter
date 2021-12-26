using System;

namespace YTSubConverter.Shared.Formats.Ttml
{
    public enum TtmlDisplayMode
    {
        Auto,
        None,
        InlineBlock
    }

    public enum TtmlVisibility
    {
        Visible,
        Hidden
    }

    public enum TtmlFontWeight
    {
        Normal,
        Bold
    }

    public enum TtmlFontStyle
    {
        Normal,
        Italic,
        Oblique
    }

    [Flags]
    public enum TtmlFontVariant
    {
        Normal = 0,
        Super = 1,
        Sub = 2,
        Full = 4,
        Half = 8,
        Ruby = 16
    }

    [Flags]
    public enum TtmlTextDecoration
    {
        None = 0,
        Underline = 1,
        LineThrough = 2,
        Overline = 4
    }

    public enum TtmlTextAlign
    {
        Left,
        Center,
        Right,
        Start,
        End,
        Justify
    }

    public enum TtmlDisplayAlign
    {
        Before,
        Center,
        After,
        Justify
    }

    public enum TtmlTextCombine
    {
        None,
        All
    }

    public enum TtmlDirection
    {
        Ltr,
        Rtl
    }

    public enum TtmlWritingMode
    {
        Lrtb,
        Rltb,
        Tbrl,
        Tblr,
        Lr,
        Rl,
        Tb
    }

    public enum TtmlTextOrientation
    {
        Mixed,
        Sideways,
        Upright
    }

    public enum TtmlRubyMode
    {
        None,
        Container,
        Base,
        BaseContainer,
        Text,
        TextContainer,
        Delimiter
    }

    public enum TtmlRubyPosition
    {
        Before,
        After,
        Outside
    }

    public enum TtmlPosHBase
    {
        Left,
        Center,
        Right
    }

    public enum TtmlPosVBase
    {
        Top,
        Center,
        Bottom
    }

    public enum TtmlUnit
    {
        Pixels,
        Percent,
        Em,
        Cell,
        RootWidth,
        RootHeight
    }

    public enum TtmlTimeContainer
    {
        Par,
        Seq
    }

    public enum TtmlProgression
    {
        Inline,
        Block
    }
}
