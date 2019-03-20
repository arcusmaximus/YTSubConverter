using System;
using System.Collections.Generic;
using System.Linq;

namespace Arc.YTSubConverter.Util
{
    internal static class TextUtil
    {
        private static readonly Dictionary<string, Measurer> Measurers = new Dictionary<string, Measurer>();

        public static readonly CharacterRange HiraganaRange = new CharacterRange((char)0x3041, (char)0x3097);
        public static readonly CharacterRange KatakanaRange = new CharacterRange((char)0x30A0, (char)0x3100);
        public static readonly CharacterRange IdeographExtensionRange = new CharacterRange((char)0x3400, (char)0x4DB6);
        public static readonly CharacterRange IdeographRange = new CharacterRange((char)0x4E00, (char)0x9FCC);
        public static readonly CharacterRange IdeographCompatibilityRange = new CharacterRange((char)0xF900, (char)0xFA6B);
        public static readonly CharacterRange HangulRange = new CharacterRange((char)0xAC00, (char)0xD7A4);

        public static int MeasureWidth(string text, string font, int pointSize, bool bold, bool italic, int pitch)
        {
            string key = $"{font}|{pointSize}|{bold}|{italic}|{pitch}";
            Measurer measurer = Measurers.FetchValue(key, () => new Measurer(font, pointSize, bold, italic, pitch));
            return text.Split(new[] { "\r\n" }, StringSplitOptions.None).Max(measurer.Measure);
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
