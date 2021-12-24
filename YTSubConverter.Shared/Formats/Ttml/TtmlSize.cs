using System;
using System.Drawing;

namespace YTSubConverter.Shared.Formats.Ttml
{
    public struct TtmlSize
    {
        public TtmlSize(TtmlLength width, TtmlLength height)
        {
            Width = width;
            Height = height;
        }

        public TtmlLength Width;
        public TtmlLength Height;

        public static bool TryParse(string text, out TtmlSize size)
        {
            if (string.IsNullOrEmpty(text))
            {
                size = new TtmlSize();
                return false;
            }

            TtmlMultipartAttributeReader reader = new TtmlMultipartAttributeReader(text);
            if (!reader.TryRead(TtmlLength.TryParse, out TtmlLength width) ||
                !reader.TryRead(TtmlLength.TryParse, out TtmlLength height) ||
                !reader.IsAtEnd)
            {
                size = new TtmlSize();
                return false;
            }

            size = new TtmlSize(width, height);
            return true;
        }

        public static TtmlSize Parse(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            if (!TryParse(text, out TtmlSize size))
                throw new FormatException();

            return size;
        }

        internal SizeF Resolve(TtmlResolutionContext context)
        {
            return new SizeF(
                Width.Resolve(context, TtmlProgression.Inline),
                Height.Resolve(context, TtmlProgression.Block)
            );
        }

        public override string ToString()
        {
            return Width + " " + Height;
        }
    }
}
