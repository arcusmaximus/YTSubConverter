using System;
using System.Drawing;

namespace YTSubConverter.Shared.Formats.Ttml
{
    public class TtmlOutline
    {
        public Color Color
        {
            get;
            set;
        }

        public TtmlLength Thickness
        {
            get;
            set;
        }

        public TtmlLength BlurRadius
        {
            get;
            set;
        }

        public static bool TryParse(string text, out TtmlOutline outline)
        {
            if (string.IsNullOrEmpty(text))
            {
                outline = new TtmlOutline();
                return false;
            }

            if (text == "none")
            {
                outline = new TtmlOutline();
                return true;
            }

            outline = null;

            TtmlMultipartAttributeReader reader = new TtmlMultipartAttributeReader(text);

            reader.TryRead(TtmlColor.TryParse, out Color color);
            
            if (!reader.TryRead(TtmlLength.TryParse, out TtmlLength thickness))
                return false;

            reader.TryRead(TtmlLength.TryParse, out TtmlLength blurRadius);

            if (!reader.IsAtEnd)
                return false;

            outline = new TtmlOutline
                      {
                          Color = color,
                          Thickness = thickness,
                          BlurRadius = blurRadius
                      };
            return true;
        }

        public static TtmlOutline Parse(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            if (!TryParse(text, out TtmlOutline outline))
                throw new FormatException();

            return outline;
        }
    }
}
