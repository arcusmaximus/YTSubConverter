using System;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;

namespace YTSubConverter.Shared.Formats.Ttml
{
    public struct TtmlLength
    {
        public TtmlLength(float value, TtmlUnit unit)
        {
            Value = value;
            Unit = unit;
        }

        public float Value;

        public TtmlUnit Unit;

        public static bool TryParse(string text, out TtmlLength length)
        {
            if (string.IsNullOrEmpty(text))
            {
                length = new TtmlLength();
                return false;
            }

            Match match = Regex.Match(text, @"^([\+|\-]?[\d\.]+)(%|px|em|c|rw|rh)$");
            if (!match.Success)
            {
                length = new TtmlLength();
                return false;
            }

            float value = float.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            TtmlUnit unit = match.Groups[2].Value switch
                            {
                                "%" => TtmlUnit.Percent,
                                "px" => TtmlUnit.Pixels,
                                "em" => TtmlUnit.Em,
                                "c" => TtmlUnit.Cell,
                                "rw" => TtmlUnit.RootWidth,
                                "rh" => TtmlUnit.RootHeight,
                                _ => throw new FormatException()
                            };
            length = new TtmlLength(value, unit);
            return true;
        }

        public static TtmlLength Parse(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            if (!TryParse(text, out TtmlLength length))
                throw new FormatException();

            return length;
        }

        internal float Resolve(TtmlResolutionContext context, TtmlProgression progression)
        {
            return Unit switch
                   {
                       TtmlUnit.Percent => Value / 100 * GetSizeComponent(context.Document.VideoDimensions, progression),
                       TtmlUnit.Pixels => Value,
                       TtmlUnit.Em => Value * (context.Style.IsInitial
                                                   ? TtmlStyle.DefaultFontSize
                                                   : context.Style.InitialStyle.FontSize.Resolve(TtmlResolutionContext.CreateInitialContext(context.Document), progression)),
                       TtmlUnit.Cell => Value * GetSizeComponent(context.Document.VideoDimensions, progression) / GetSizeComponent(context.Document.CellResolution, progression),
                       TtmlUnit.RootWidth => Value / 100 * context.Document.VideoDimensions.Width,
                       TtmlUnit.RootHeight => Value / 100 * context.Document.VideoDimensions.Height
                   };
        }

        private static float GetSizeComponent(SizeF size, TtmlProgression progression)
        {
            return progression == TtmlProgression.Inline ? size.Width : size.Height;
        }

        public override string ToString()
        {
            string value = Value.ToString(CultureInfo.InvariantCulture);
            string unit = Unit switch
                          {
                              TtmlUnit.Percent => "%",
                              TtmlUnit.Pixels => "px",
                              TtmlUnit.Em => "em",
                              TtmlUnit.Cell => "c",
                              TtmlUnit.RootWidth => "rw",
                              TtmlUnit.RootHeight => "rh",
                              _ => throw new NotSupportedException()
                          };
            return value + unit;
        }
    }
}
