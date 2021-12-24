using System;
using System.Drawing;

namespace YTSubConverter.Shared.Formats.Ttml
{
    public struct TtmlPosition
    {
        public TtmlPosition(TtmlSize offset, TtmlPosHBase hBase, TtmlPosVBase vBase)
        {
            Offset = offset;
            HBase = hBase;
            VBase = vBase;
        }

        public TtmlSize Offset;

        public TtmlPosHBase HBase;

        public TtmlPosVBase VBase;

        public static bool TryParse(string text, out TtmlPosition position)
        {
            if (string.IsNullOrEmpty(text))
            {
                position = new TtmlPosition();
                return false;
            }

            TtmlMultipartAttributeReader reader = new TtmlMultipartAttributeReader(text);
            
            TtmlPosHBase hBase = TtmlPosHBase.Center;
            TtmlLength hOffset = new TtmlLength();
            TtmlPosVBase vBase = TtmlPosVBase.Center;
            TtmlLength vOffset = new TtmlLength();

            bool allowTwopart = reader.Count > 2;

            if (TryReadHorizontalCoord(reader, allowTwopart, ref hBase, ref hOffset) && !reader.IsAtEnd)
            {
                if (TryReadVerticalCoord(reader, allowTwopart, ref vBase, ref vOffset))
                {
                    if (!reader.IsAtEnd)
                    {
                        position = new TtmlPosition();
                        return false;
                    }
                }
                else
                {
                    if (hBase == TtmlPosHBase.Center)
                    {
                        reader.Reset();
                    }
                    else
                    {
                        position = new TtmlPosition();
                        return false;
                    }
                }
            }

            if (TryReadVerticalCoord(reader, allowTwopart, ref vBase, ref vOffset) && !reader.IsAtEnd)
            {
                if (!TryReadHorizontalCoord(reader, allowTwopart, ref hBase, ref hOffset))
                {
                    position = new TtmlPosition();
                    return false;
                }
            }

            if (!reader.IsAtEnd)
            {
                position = new TtmlPosition();
                return false;
            }

            position = new TtmlPosition(new TtmlSize(hOffset, vOffset), hBase, vBase);
            return true;
        }

        public static TtmlPosition Parse(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            if (!TryParse(text, out TtmlPosition position))
                throw new FormatException();

            return position;
        }

        internal PointF Resolve(TtmlResolutionContext context)
        {
            PointF resolved = (PointF)Offset.Resolve(context);

            switch (HBase)
            {
                case TtmlPosHBase.Center:
                    resolved.X = context.Document.VideoDimensions.Width / 2;
                    break;

                case TtmlPosHBase.Right:
                    resolved.X = context.Document.VideoDimensions.Width - resolved.X;
                    break;
            }

            switch (VBase)
            {
                case TtmlPosVBase.Center:
                    resolved.Y = context.Document.VideoDimensions.Height / 2;
                    break;

                case TtmlPosVBase.Bottom:
                    resolved.Y = context.Document.VideoDimensions.Height - resolved.Y;
                    break;
            }

            return resolved;
        }

        private static bool TryReadHorizontalCoord(TtmlMultipartAttributeReader reader, bool allowTwopart, ref TtmlPosHBase hBase, ref TtmlLength hOffset)
        {
            TtmlPosHBase tempHBase;
            TtmlLength tempHOffset;

            if (reader.TryReadEnum(out tempHBase))
            {
                hBase = tempHBase;

                if (allowTwopart && hBase != TtmlPosHBase.Center && reader.TryRead(TtmlLength.TryParse, out tempHOffset))
                {
                    hOffset = tempHOffset;
                    return true;
                }

                hOffset = new TtmlLength();
                return true;
            }

            if (reader.TryRead(TtmlLength.TryParse, out tempHOffset))
            {
                hBase = TtmlPosHBase.Left;
                hOffset = tempHOffset;
                return true;
            }

            return false;
        }

        private static bool TryReadVerticalCoord(TtmlMultipartAttributeReader reader, bool allowTwopart, ref TtmlPosVBase vBase, ref TtmlLength vOffset)
        {
            TtmlPosVBase tempVBase;
            TtmlLength tempVOffset;

            if (reader.TryReadEnum(out tempVBase))
            {
                vBase = tempVBase;

                if (allowTwopart && vBase != TtmlPosVBase.Center && reader.TryRead(TtmlLength.TryParse, out tempVOffset))
                {
                    vOffset = tempVOffset;
                    return true;
                }

                vOffset = new TtmlLength();
                return true;
            }

            if (reader.TryRead(TtmlLength.TryParse, out tempVOffset))
            {
                vBase = TtmlPosVBase.Top;
                vOffset = tempVOffset;
                return true;
            }

            return false;
        }

        private static string FormatHorizontalBase(TtmlPosHBase hBase)
        {
            return hBase switch
                   {
                       TtmlPosHBase.Left => "left",
                       TtmlPosHBase.Center => "center",
                       TtmlPosHBase.Right => "right",
                       _ => throw new ArgumentException()
                   };
        }

        private static string FormatVerticalBase(TtmlPosVBase vBase)
        {
            return vBase switch
                   {
                       TtmlPosVBase.Top => "top",
                       TtmlPosVBase.Center => "center",
                       TtmlPosVBase.Bottom => "bottom",
                       _ => throw new ArgumentException()
                   };
        }

        public override string ToString()
        {
            return $"{FormatHorizontalBase(HBase)} {Offset.Width} {FormatVerticalBase(VBase)} {Offset.Height}";
        }
    }
}
