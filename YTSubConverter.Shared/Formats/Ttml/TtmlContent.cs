using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace YTSubConverter.Shared.Formats.Ttml
{
    public abstract class TtmlContent
    {
        protected TtmlContent(XmlElement elem, XmlNamespaceManager nsmgr, TtmlDocument doc, TtmlContent parent)
        {
            TimeRange = TtmlTimeRange.Get(elem, doc);
            Style = TtmlStyle.CreateAggregateStyle(elem, nsmgr, false, doc);
            Region = TtmlRegion.Get(elem, nsmgr, doc);

            Parent = parent;
            Children = elem.ChildNodes
                           .Cast<XmlNode>()
                           .Select(n => Create(n, nsmgr, doc, this))
                           .Where(c => c != null)
                           .ToList();
        }

        protected TtmlContent(string text, TtmlContent parent)
        {
            Parent = parent;
            Children = new List<TtmlContent>();
            Text = text;
        }

        public static TtmlContent Create(XmlNode node, XmlNamespaceManager nsmgr, TtmlDocument doc, TtmlContent parent)
        {
            switch (node)
            {
                case XmlElement elem:
                    return elem.LocalName switch
                           {
                               "body" => new TtmlBody(elem, nsmgr, doc),
                               "div" => new TtmlDiv(elem, nsmgr, doc, parent),
                               "p" => new TtmlParagraph(elem, nsmgr, doc, parent),
                               "span" => new TtmlSpan(elem, nsmgr, doc, parent),
                               "br" => new TtmlSpan("\r\n", parent),
                               _ => null
                           };

                case XmlText _:
                case XmlWhitespace _ when parent is TtmlParagraph || parent is TtmlSpan:
                    return new TtmlSpan(node.Value.Replace("\r\n", " "), parent);

                default:
                    return null;
            }
        }

        public TtmlTimeRange TimeRange
        {
            get;
        }

        public TtmlStyle Style
        {
            get;
        }

        public TtmlRegion Region
        {
            get;
        }

        public TtmlContent Parent
        {
            get;
        }

        public List<TtmlContent> Children
        {
            get;
        }

        public string Text
        {
            get;
        }
    }
}
