using System.Drawing;
using YTSubConverter.Shared;

namespace YTSubConverter.UI.Linux
{
    internal class GtkTextMeasurer : ITextMeasurer
    {
        private readonly Gtk.Window _window;
        private readonly Pango.Layout _layout;
        private readonly Pango.FontDescription _fontDesc;

        public GtkTextMeasurer()
        {
            _window = new(Gtk.WindowType.Popup);
            _layout = new(_window.PangoContext);
            _fontDesc = new();
        }

        public SizeF Measure(string text, string font, float fontSize, bool bold, bool italic)
        {
            _layout.SetText(text);
            _fontDesc.Family = font;
            _fontDesc.Size = (int)(fontSize * Pango.Scale.PangoScale);
            _fontDesc.Weight = bold ? Pango.Weight.Bold : Pango.Weight.Normal;
            _fontDesc.Style = italic ? Pango.Style.Italic : Pango.Style.Normal;
            _layout.FontDescription = _fontDesc;
            _layout.GetPixelExtents(out _, out Pango.Rectangle logicalRect);
            return new SizeF(logicalRect.Width * 1.02f, logicalRect.Height);
        }

        public void Dispose()
        {
            _layout?.Dispose();
            _fontDesc?.Dispose();
            _window?.Dispose();
        }
    }
}
