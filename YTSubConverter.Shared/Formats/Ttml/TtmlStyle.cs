using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using YTSubConverter.Shared.Util;

namespace YTSubConverter.Shared.Formats.Ttml
{
    public class TtmlStyle
    {
        public const int DefaultFontSize = 38;

        private List<string> _fontFamilies;
        private TtmlLength? _fontSize;
        private TtmlFontWeight? _fontWeight;
        private TtmlFontStyle? _fontStyle;
        private bool? _underline;
        private bool? _lineThrough;
        private bool? _overline;
        private Color? _color;
        private Color? _backgroundColor;
        private float? _opacity;
        private TtmlTextAlign? _textAlign;
        private TtmlDisplayAlign? _displayAlign;
        private TtmlTextCombine? _textCombine;
        private TtmlOutline _textOutline;
        private List<TtmlShadow> _textShadows;
        private TtmlDisplayMode? _display;
        private TtmlVisibility? _visibility;
        private TtmlSize? _origin;
        private TtmlPosition? _position;
        private TtmlSize? _extent;
        private TtmlDirection? _direction;
        private TtmlWritingMode? _writingMode;
        private TtmlTextOrientation? _textOrientation;
        private TtmlRubyMode? _rubyMode;
        private TtmlRubyPosition? _rubyPosition;

        private TtmlStyle()
        {
        }

        public TtmlStyle(TtmlStyle baseStyle)
        {
            BaseStyle = baseStyle;
        }

        public TtmlStyle(XmlElement elem, XmlNamespaceManager nsmgr, TtmlStyle baseStyle)
            : this(baseStyle)
        {
            string xml = nsmgr.LookupNamespace("xml");
            string tts = nsmgr.LookupNamespace("tts");

            Id = elem.GetAttributeNode("id", xml)?.Value;

            _fontFamilies = GetFontFamilies(elem, tts);
            _fontSize = elem.GetTypedAttribute<TtmlLength>("fontSize", tts, TtmlLength.TryParse);
            _fontWeight = elem.GetEnumAttribute<TtmlFontWeight>("fontWeight", tts);
            _fontStyle = elem.GetEnumAttribute<TtmlFontStyle>("fontStyle", tts);
            GetTextDecoration(elem, tts, out _underline, out _lineThrough, out _overline);
            _color = elem.GetTypedAttribute<Color>("color", tts, TtmlColor.TryParse);
            _backgroundColor = elem.GetTypedAttribute<Color>("backgroundColor", tts, TtmlColor.TryParse);
            _opacity = elem.GetFloatAttribute("opacity", tts);
            _textAlign = elem.GetEnumAttribute<TtmlTextAlign>("textAlign", tts);
            _displayAlign = elem.GetEnumAttribute<TtmlDisplayAlign>("displayAlign", tts);
            _textCombine = elem.GetEnumAttribute<TtmlTextCombine>("textCombine", tts);
            _textOutline = GetTextOutline(elem, tts);
            _textShadows = GetTextShadows(elem, tts);
            _display = elem.GetEnumAttribute<TtmlDisplayMode>("display", tts);
            _visibility = elem.GetEnumAttribute<TtmlVisibility>("visibility", tts);
            _origin = elem.GetTypedAttribute<TtmlSize>("origin", tts, TtmlSize.TryParse);
            _position = elem.GetTypedAttribute<TtmlPosition>("position", tts, TtmlPosition.TryParse);
            _extent = elem.GetTypedAttribute<TtmlSize>("extent", tts, TtmlSize.TryParse);
            _direction = elem.GetEnumAttribute<TtmlDirection>("direction", tts);
            _writingMode = elem.GetEnumAttribute<TtmlWritingMode>("writingMode", tts);
            _textOrientation = elem.GetEnumAttribute<TtmlTextOrientation>("textOrientation", tts);
            _rubyMode = elem.GetEnumAttribute<TtmlRubyMode>("ruby", tts);
            _rubyPosition = elem.GetEnumAttribute<TtmlRubyPosition>("rubyPosition", tts);
        }

        public static TtmlStyle CreateDefaultInitialStyle()
        {
            return new TtmlStyle
                   {
                       FontFamilies = new List<string> { "default" },
                       FontSize = new TtmlLength(DefaultFontSize, TtmlUnit.Pixels),
                       FontWeight = TtmlFontWeight.Normal,
                       FontStyle = TtmlFontStyle.Normal,
                       TextDecoration = TtmlTextDecoration.None,
                       Color = Color.White,
                       BackgroundColor = Color.FromArgb(192, 8, 8, 8),
                       Opacity = 1,
                       TextAlign = TtmlTextAlign.Center,
                       DisplayAlign = TtmlDisplayAlign.After,
                       TextCombine = TtmlTextCombine.None,
                       TextShadows = new List<TtmlShadow>(),
                       Display = TtmlDisplayMode.Auto,
                       Visibility = TtmlVisibility.Visible,
                       Direction = TtmlDirection.Ltr,
                       WritingMode = TtmlWritingMode.Lrtb,
                       TextOrientation = TtmlTextOrientation.Mixed,
                       RubyMode = TtmlRubyMode.None,
                       RubyPosition = TtmlRubyPosition.Before,
                       IsInitial = true
                   };
        }

        public static TtmlStyle CreateAggregateStyle(XmlElement elem, XmlNamespaceManager nsmgr, bool includeStyleElements, TtmlDocument doc)
        {
            TtmlStyle style = doc.InitialStyle;
            
            // style="" attribute
            string styleId = elem.GetAttribute("style");
            if (!string.IsNullOrEmpty(styleId))
            {
                TtmlStyle referencedStyle = doc.Styles.GetOrDefault(styleId);
                if (referencedStyle != null)
                    style = referencedStyle;
            }

            // Child <style> elements
            if (includeStyleElements)
            {
                foreach (XmlElement styleElem in elem.SelectNodes("tt:style", nsmgr))
                {
                    style = new TtmlStyle(styleElem, nsmgr, style);
                }
            }

            // Inline formatting attributes
            style = new TtmlStyle(elem, nsmgr, style);

            return style;
        }

        public string Id
        {
            get;
        }

        public bool IsInitial
        {
            get;
            set;
        }

        public TtmlStyle BaseStyle
        {
            get;
            set;
        }

        public TtmlStyle InitialStyle
        {
            get
            {
                TtmlStyle style = this;
                while (!style.IsInitial)
                {
                    style = style.BaseStyle;
                }
                return style;
            }
        }

        public List<string> FontFamilies
        {
            get => _fontFamilies ?? BaseStyle.FontFamilies;
            set => _fontFamilies = value;
        }

        public TtmlLength FontSize
        {
            get => _fontSize ?? BaseStyle.FontSize;
            set => _fontSize = value;
        }

        public TtmlFontWeight FontWeight
        {
            get => _fontWeight ?? BaseStyle.FontWeight;
            set => _fontWeight = value;
        }

        public TtmlFontStyle FontStyle
        {
            get => _fontStyle ?? BaseStyle.FontStyle;
            set => _fontStyle = value;
        }

        public TtmlTextDecoration TextDecoration
        {
            get
            {
                TtmlTextDecoration decoration = BaseStyle?.TextDecoration ?? 0;
                SetDecorationFlag(TtmlTextDecoration.Underline, _underline);
                SetDecorationFlag(TtmlTextDecoration.LineThrough, _lineThrough);
                SetDecorationFlag(TtmlTextDecoration.Overline, _overline);
                return decoration;

                void SetDecorationFlag(TtmlTextDecoration flag, bool? set)
                {
                    if (set == null)
                        return;

                    if (set.Value)
                        decoration |= flag;
                    else
                        decoration &= ~flag;
                }
            }
            set
            {
                _underline = (value & TtmlTextDecoration.Underline) != 0;
                _lineThrough = (value & TtmlTextDecoration.LineThrough) != 0;
                _overline = (value & TtmlTextDecoration.Overline) != 0;
            }
        }

        public Color Color
        {
            get => _color ?? BaseStyle.Color;
            set => _color = value;
        }

        public Color BackgroundColor
        {
            get => _backgroundColor ?? BaseStyle.BackgroundColor;
            set => _backgroundColor = value;
        }

        public float Opacity
        {
            get => _opacity ?? BaseStyle.Opacity;
            set => _opacity = value;
        }

        public TtmlTextAlign TextAlign
        {
            get => _textAlign ?? BaseStyle.TextAlign;
            set => _textAlign = value;
        }

        public TtmlDisplayAlign DisplayAlign
        {
            get => _displayAlign ?? BaseStyle.DisplayAlign;
            set => _displayAlign = value;
        }

        public TtmlTextCombine TextCombine
        {
            get => _textCombine ?? BaseStyle.TextCombine;
            set => _textCombine = value;
        }

        public TtmlOutline TextOutline
        {
            get => _textOutline ?? BaseStyle?.TextOutline;
            set => _textOutline = value;
        }

        public List<TtmlShadow> TextShadows
        {
            get => _textShadows ?? BaseStyle.TextShadows;
            set => _textShadows = value;
        }

        public TtmlDisplayMode Display
        {
            get => _display ?? BaseStyle.Display;
            set => _display = value;
        }

        public TtmlVisibility Visibility
        {
            get => _visibility ?? BaseStyle.Visibility;
            set => _visibility = value;
        }

        public TtmlSize? Origin
        {
            get => _origin ?? BaseStyle?.Origin;
            set => _origin = value;
        }

        public TtmlPosition? Position
        {
            get => _position ?? BaseStyle?.Position;
            set => _position = value;
        }

        public TtmlSize? Extent
        {
            get => _extent ?? BaseStyle?.Extent;
            set => _extent = value;
        }

        public TtmlDirection Direction
        {
            get => _direction ?? BaseStyle.Direction;
            set => _direction = value;
        }

        public TtmlWritingMode WritingMode
        {
            get => _writingMode ?? BaseStyle.WritingMode;
            set => _writingMode = value;
        }

        public TtmlTextOrientation TextOrientation
        {
            get => _textOrientation ?? BaseStyle.TextOrientation;
            set => _textOrientation = value;
        }

        public TtmlRubyMode RubyMode
        {
            get => _rubyMode ?? BaseStyle.RubyMode;
            set => _rubyMode = value;
        }

        public TtmlRubyPosition RubyPosition
        {
            get => _rubyPosition ?? BaseStyle.RubyPosition;
            set => _rubyPosition = value;
        }

        public TtmlStyle CloneUsingNewInitialStyle(TtmlStyle initial)
        {
            return new TtmlStyle
                   {
                       _fontFamilies = ResolveFieldBeforeInitial(s => s._fontFamilies),
                       _fontSize = ResolveFieldBeforeInitial(s => s._fontSize),
                       _fontWeight = ResolveFieldBeforeInitial(s => s._fontWeight),
                       _fontStyle = ResolveFieldBeforeInitial(s => s._fontStyle),
                       _underline = ResolveFieldBeforeInitial(s => s._underline),
                       _lineThrough = ResolveFieldBeforeInitial(s => s._lineThrough),
                       _overline = ResolveFieldBeforeInitial(s => s._overline),
                       _color = ResolveFieldBeforeInitial(s => s._color),
                       _backgroundColor = ResolveFieldBeforeInitial(s => s._backgroundColor),
                       _opacity = ResolveFieldBeforeInitial(s => s._opacity),
                       _textAlign = ResolveFieldBeforeInitial(s => s._textAlign),
                       _displayAlign = ResolveFieldBeforeInitial(s => s._displayAlign),
                       _textCombine = ResolveFieldBeforeInitial(s => s._textCombine),
                       _textOutline = ResolveFieldBeforeInitial(s => s._textOutline),
                       _textShadows = ResolveFieldBeforeInitial(s => s._textShadows),
                       _display = ResolveFieldBeforeInitial(s => s._display),
                       _visibility = ResolveFieldBeforeInitial(s => s._visibility),
                       _origin = ResolveFieldBeforeInitial(s => s._origin),
                       _position = ResolveFieldBeforeInitial(s => s._position),
                       _extent = ResolveFieldBeforeInitial(s => s._extent),
                       _direction = ResolveFieldBeforeInitial(s => s._direction),
                       _writingMode = ResolveFieldBeforeInitial(s => s._writingMode),
                       _textOrientation = ResolveFieldBeforeInitial(s => s._textOrientation),
                       _rubyMode = ResolveFieldBeforeInitial(s => s._rubyMode),
                       _rubyPosition = ResolveFieldBeforeInitial(s => s._rubyPosition),
                       BaseStyle = initial
                   };
        }

        private T ResolveFieldBeforeInitial<T>(Func<TtmlStyle, T> getField)
        {
            TtmlStyle style = this;
            while (style != null && !style.IsInitial)
            {
                T value = getField(style);
                if (!EqualityComparer<T>.Default.Equals(value, default))
                    return value;

                style = style.BaseStyle;
            }
            return default;
        }

        private static List<string> GetFontFamilies(XmlElement elem, string tts)
        {
            string fontFamilies = elem.GetAttribute("fontFamily", tts);
            if (string.IsNullOrEmpty(fontFamilies))
                return null;

            Match match = Regex.Match(fontFamilies, @"^(?:\s*(?:""(?<font>(?:\\.|[^""])+)""|'(?<font>(?:\\.|[^'])+)'|(?<font>[^""',]+))\s*,?\s*)+$");
            if (!match.Success)
                return null;

            return match.Groups["font"]
                        .Captures
                        .Cast<Capture>()
                        .Select(c => Regex.Replace(c.Value, @"\\(.)", "$1").Trim())
                        .ToList();
        }

        private static void GetTextDecoration(XmlElement elem, string tts, out bool? underline, out bool? lineThrough, out bool? overline)
        {
            underline = null;
            lineThrough = null;
            overline = null;

            string textDecoration = elem.GetAttribute("textDecoration", tts);
            if (string.IsNullOrEmpty(textDecoration))
                return;

            foreach (string keyword in Regex.Split(textDecoration, @"\s+"))
            {
                switch (keyword)
                {
                    case "none":
                        underline = false;
                        lineThrough = false;
                        overline = false;
                        break;

                    case "underline":
                        underline = true;
                        break;

                    case "noUnderline":
                        underline = false;
                        break;

                    case "lineThrough":
                        lineThrough = true;
                        break;

                    case "noLineThrough":
                        lineThrough = false;
                        break;

                    case "overline":
                        overline = true;
                        break;

                    case "noOverline":
                        overline = false;
                        break;
                }
            }
        }

        private static List<TtmlShadow> GetTextShadows(XmlElement elem, string tts)
        {
            string textShadows = elem.GetAttribute("textShadow", tts);
            if (string.IsNullOrEmpty(textShadows))
                return null;

            if (textShadows == "none")
                return new List<TtmlShadow>();

            List<TtmlShadow> parsedTextShadows = new List<TtmlShadow>();
            foreach (Match match in Regex.Matches(textShadows, @"(?:\(.+?\)|[^\(\),])+"))
            {
                if (!TtmlShadow.TryParse(match.Value.Trim(), out TtmlShadow parsedTextShadow))
                    return null;

                parsedTextShadows.Add(parsedTextShadow);
            }
            return parsedTextShadows;
        }

        private static TtmlOutline GetTextOutline(XmlElement elem, string tts)
        {
            string textOutline = elem.GetAttribute("textOutline", tts);
            if (string.IsNullOrEmpty(textOutline))
                return null;

            TtmlOutline.TryParse(textOutline, out TtmlOutline outline);
            return outline;
        }

        public override string ToString()
        {
            return Id;
        }
    }
}
