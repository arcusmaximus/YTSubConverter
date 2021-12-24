using System.Xml;
using YTSubConverter.Shared.Util;

namespace YTSubConverter.Shared.Formats.Ttml
{
    public class TtmlRegion
    {
        public TtmlRegion(XmlElement elem, XmlNamespaceManager nsmgr, TtmlDocument doc)
        {
            string xml = nsmgr.LookupNamespace("xml");
            
            Id = elem.GetAttributeNode("id", xml)?.Value;
            TimeRange = TtmlTimeRange.Get(elem, doc);
            Style = TtmlStyle.CreateAggregateStyle(elem, nsmgr, true, doc);
        }

        public string Id
        {
            get;
        }

        public TtmlTimeRange TimeRange
        {
            get;
        }

        public TtmlStyle Style
        {
            get;
        }

        public static TtmlRegion Get(XmlElement elem, XmlNamespaceManager nsmgr, TtmlDocument doc)
        {
            // Child <region> element
            XmlElement regionElem = (XmlElement)elem.SelectSingleNode("tt:region", nsmgr);
            if (regionElem != null)
                return new TtmlRegion(regionElem, nsmgr, doc);

            // region="" attribute
            string regionId = elem.GetAttribute("region");
            if (!string.IsNullOrEmpty(regionId))
            {
                TtmlRegion referencedRegion = doc.Regions.GetOrDefault(regionId);
                if (referencedRegion != null)
                    return referencedRegion;
            }

            return null;
        }

        public override string ToString()
        {
            return Id;
        }
    }
}
