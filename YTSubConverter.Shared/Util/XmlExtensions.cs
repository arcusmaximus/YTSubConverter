using System.Xml;

namespace Arc.YTSubConverter.Shared.Util
{
    internal static class XmlExtensions
    {
        public static int? GetIntAttribute(this XmlElement elem, string attrName)
        {
            XmlAttribute attr = elem.Attributes[attrName];
            return int.TryParse(attr?.Value, out int value) ? (int?)value : null;
        }
    }
}
