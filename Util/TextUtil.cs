using System;
using System.Collections.Generic;
using System.Text;

namespace Arc.YTSubConverter.Util
{
    public static class TextUtil
    {
        private static readonly Dictionary<string, Measurer> Measurers = new Dictionary<string, Measurer>();

        public static int CountOccurrences(this string str, string substr)
        {
            int pos = -1;
            int count = 0;
            while (true)
            {
                pos = str.IndexOf(substr, pos + 1);
                if (pos < 0)
                    return count;

                count++;
            }
        }

        public static string Repeat(this string str, int times)
        {
            if (times < 0)
                throw new ArgumentOutOfRangeException(nameof(times));

            if (times == 0)
                return string.Empty;

            if (times == 1)
                return str;

            StringBuilder result = new StringBuilder();
            for (int i = 0; i < times; i++)
            {
                result.Append(str);
            }
            return result.ToString();
        }

        public static int MeasureWidth(string text, string font, int pointSize, bool bold, bool italic, int pitch)
        {
            string key = $"{font}|{pointSize}|{bold}|{italic}|{pitch}";
            Measurer measurer = Measurers.FetchValue(key, () => new Measurer(font, pointSize, bold, italic, pitch));
            return measurer.Measure(text);
        }

        private class Measurer
        {
            private int _pitch;
            private readonly byte[] _charWidths;
            private readonly byte _defaultWidth;

            public Measurer(string fontName, int fontSize, bool bold, bool italic, int pitch)
            {
                IntPtr dc = NativeMethods.GetDC(IntPtr.Zero);
                IntPtr font = NativeMethods.CreateFontW(
                    -fontSize,
                    0,
                    0,
                    0,
                    bold ? NativeMethods.FW_BOLD : NativeMethods.FW_NORMAL,
                    italic,
                    false,
                    false,
                    NativeMethods.DEFAULT_CHARSET,
                    NativeMethods.OUT_DEFAULT_PRECIS,
                    NativeMethods.CLIP_DEFAULT_PRECIS,
                    NativeMethods.ANTIALIASED_QUALITY,
                    NativeMethods.DEFAULT_PITCH | NativeMethods.FF_DONTCARE,
                    fontName
                );

                NativeMethods.SelectObject(dc, font);

                _pitch = pitch;
                _charWidths = new byte[0x100];
                _charWidths[' '] = MeasureCharWidth(dc, 'o');
                for (int i = 0x21; i < 0x100; i++)
                {
                    char c = (char)i;
                    if (!char.IsControl(c))
                        _charWidths[i] = MeasureCharWidth(dc, c);
                }
                _defaultWidth = MeasureCharWidth(dc, '幅');

                NativeMethods.ReleaseDC(IntPtr.Zero, dc);
                NativeMethods.DeleteObject(font);
            }

            public int Measure(string text)
            {
                if (text.Length == 0)
                    return 0;

                int width = 0;
                foreach (char c in text)
                {
                    width += GetCharWidth(c);
                }
                return width + _pitch * (text.Length - 1);
            }

            private byte GetCharWidth(char c)
            {
                return c < 0x100 ? _charWidths[c] : _defaultWidth;
            }

            private static byte MeasureCharWidth(IntPtr dc, char c)
            {
                NativeMethods.GLYPHMETRICS metrics = new NativeMethods.GLYPHMETRICS();
                NativeMethods.MAT2 mat = new NativeMethods.MAT2(1, 0, 0, 1);
                NativeMethods.GetGlyphOutlineW(dc, c, NativeMethods.GGO_METRICS, ref metrics, 0, IntPtr.Zero, ref mat);
                return (byte)metrics.gmBlackBoxX;
            }
        }
    }
}
