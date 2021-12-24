using System.Xml;

namespace YTSubConverter.Shared.Formats.Ttml
{
    public class TtmlParagraph : TtmlContent
    {
        public TtmlParagraph(XmlElement elem, XmlNamespaceManager nsmgr, TtmlDocument doc, TtmlContent parent)
            : base(elem, nsmgr, doc, parent)
        {
        }
    }
}
