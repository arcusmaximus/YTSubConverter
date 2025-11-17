using System;
using System.Drawing;
using YTSubConverter.Shared;

namespace YTSubConverter.UI.Win
{
    internal class WinTextMeasurer : ITextMeasurer
    {
        private Graphics _graphics;
        private Font _lastFont;

        public WinTextMeasurer()
        {
            _graphics = Graphics.FromHwnd(IntPtr.Zero);
        }

        public SizeF Measure(string text, string font, float size, bool bold, bool italic)
        {
            if (_lastFont == null ||
                _lastFont.Name != font ||
                _lastFont.Size != size ||
                _lastFont.Bold != bold ||
                _lastFont.Italic != italic)
            {
                _lastFont?.Dispose();

                FontStyle style = FontStyle.Regular;
                if (bold)
                    style |= FontStyle.Bold;

                if (italic)
                    style |= FontStyle.Italic;

                _lastFont = new Font(font, size, style);
            }

            SizeF result = _graphics.MeasureString(text, _lastFont, new PointF(), StringFormat.GenericTypographic);
            return new SizeF(result.Width * 0.97f, result.Height);
        }

        public void Dispose()
        {
            if (_graphics != null)
            {
                _graphics.Dispose();
                _graphics = null;
            }

            if (_lastFont != null)
            {
                _lastFont.Dispose();
                _lastFont = null;
            }
        }
    }
}
