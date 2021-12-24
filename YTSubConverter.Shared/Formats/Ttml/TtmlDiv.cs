using System.Xml;

namespace YTSubConverter.Shared.Formats.Ttml
{
    public class TtmlDiv : TtmlContent
    {
        public TtmlDiv(XmlElement elem, XmlNamespaceManager nsmgr, TtmlDocument doc, TtmlContent parent)
            : base(elem, nsmgr, doc, parent)
        {
        }
    }
}
