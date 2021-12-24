using System.Xml;

namespace YTSubConverter.Shared.Formats.Ttml
{
    public class TtmlBody : TtmlContent
    {
        public TtmlBody(XmlElement elem, XmlNamespaceManager nsmgr, TtmlDocument doc)
            : base(elem, nsmgr, doc, null)
        {
        }
    }
}
