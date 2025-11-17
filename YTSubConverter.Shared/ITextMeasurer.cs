using System;
using System.Drawing;

namespace YTSubConverter.Shared
{
    public interface ITextMeasurer : IDisposable
    {
        SizeF Measure(string text, string font, float size, bool bold, bool italic);
    }
}
