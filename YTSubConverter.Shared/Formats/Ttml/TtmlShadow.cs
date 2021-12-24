using System;
using System.Drawing;
using System.Text;

namespace YTSubConverter.Shared.Formats.Ttml
{
    public class TtmlShadow
    {
        public TtmlSize Offset
        {
            get;
            set;
        }

        public TtmlLength BlurRadius
        {
            get;
            set;
        }

        public Color Color
        {
            get;
            set;
        }

        public static bool TryParse(string text, out TtmlShadow shadow)
        {
            shadow = null;

            if (string.IsNullOrEmpty(text))
                return false;

            TtmlMultipartAttributeReader reader = new TtmlMultipartAttributeReader(text);
            if (!reader.TryRead(TtmlLength.TryParse, out TtmlLength xOffset))
                return false;

            if (!reader.TryRead(TtmlLength.TryParse, out TtmlLength yOffset))
                return false;

            reader.TryRead(TtmlLength.TryParse, out TtmlLength blurRadius);
            reader.TryRead(TtmlColor.TryParse, out Color color);

            if (!reader.IsAtEnd)
                return false;

            shadow = new TtmlShadow
                     {
                         Offset = new TtmlSize(xOffset, yOffset),
                         BlurRadius = blurRadius,
                         Color = color
                     };
            return true;
        }

        public static TtmlShadow Parse(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            if (!TryParse(text, out TtmlShadow shadow))
                throw new FormatException();

            return shadow;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append(Offset);

            if (BlurRadius.Value > 0)
            {
                result.Append(" ");
                result.Append(BlurRadius);
            }

            if (!Color.IsEmpty)
            {
                result.Append(" ");
                result.Append(TtmlColor.ToString(Color));
            }

            return result.ToString();
        }
    }
}
