using System.Drawing;
using AppKit;
using CoreGraphics;
using Foundation;
using YTSubConverter.Shared;

namespace YTSubConverter.UI.Mac
{
    internal class CocoaTextMeasurer : ITextMeasurer
    {
        private NSLayoutManager _layout;
        private NSTextContainer _container;
        private NSTextStorage _storage;
        private NSFont _lastFont;

        public CocoaTextMeasurer()
        {
            _layout = new();
            _container = new(new CGSize(9999, 9999)) { LineFragmentPadding = 0 };
            _layout.AddTextContainer(_container);

            _storage = new();
            _storage.AddLayoutManager(_layout);
        }

        public SizeF Measure(string text, string fontName, float fontSize, bool bold, bool italic)
        {
            if (_lastFont == null || _lastFont.FontName != fontName || _lastFont.PointSize != fontSize)
            {
                _lastFont?.Dispose();
                _lastFont = NSFont.FromFontName(fontName, fontSize) ?? NSFont.SystemFontOfSize(fontSize);
            }
            
            using NSAttributedString str = new(text, new NSStringAttributes { Font = _lastFont });
            _storage.SetString(str);
            _layout.EnsureLayoutForTextContainer(_container);
            CGRect rect = _layout.GetUsedRectForTextContainer(_container);
            return new SizeF((float)rect.Width, (float)rect.Height);
        }

        public void Dispose()
        {
            _layout?.Dispose();
            _container?.Dispose();
            _storage?.Dispose();
            _lastFont?.Dispose();
        }
    }
}
