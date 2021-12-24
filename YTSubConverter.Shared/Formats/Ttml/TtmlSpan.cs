using System.Xml;

namespace YTSubConverter.Shared.Formats.Ttml
{
    public class TtmlSpan : TtmlContent
    {
        public TtmlSpan(XmlElement elem, XmlNamespaceManager nsmgr, TtmlDocument doc, TtmlContent parent)
            : base(elem, nsmgr, doc, parent)
        {
        }

        public TtmlSpan(string text, TtmlContent parent)
            : base(text, parent)
        {
        }

        public TtmlParagraph Paragraph
        {
            get 
            {
                TtmlContent parent = Parent;
                while (parent != null)
                {
                    if (parent is TtmlParagraph paragraph)
                        return paragraph;

                    parent = parent.Parent;
                }
                return null;
            }
        }
    }
}
