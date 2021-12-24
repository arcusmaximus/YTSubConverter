using System;
using System.Globalization;
using System.Xml;

namespace YTSubConverter.Shared.Util
{
    internal static class XmlExtensions
    {
        public static int? GetIntAttribute(this XmlElement elem, string attr, string ns = "")
        {
            return elem.GetTypedAttribute<int>(attr, ns, int.TryParse);
        }

        public static float? GetFloatAttribute(this XmlElement elem, string attr, string ns = "")
        {
            return elem.GetTypedAttribute(
                attr,
                ns,
                (string textValue, out float parsedValue) => float.TryParse(textValue, NumberStyles.Float, CultureInfo.InvariantCulture, out parsedValue)
            );
        }

        public static T? GetEnumAttribute<T>(this XmlElement elem, string attr, string ns = "")
            where T : struct
        {
            return elem.GetTypedAttribute<T>(attr, ns, TryParseEnum);
        }

        private static bool TryParseEnum<T>(string part, out T value)
            where T : struct
        {
            return Enum.TryParse(part, true, out value);
        }

        public delegate bool Parser<T>(string text, out T value);

        public static T? GetTypedAttribute<T>(this XmlElement elem, string attr, string ns, Parser<T> tryParse)
            where T : struct
        {
            string textValue = elem.GetAttribute(attr, ns);
            if (string.IsNullOrEmpty(textValue))
                return null;

            if (!tryParse(textValue, out T parsedValue))
                return null;

            return parsedValue;
        }
    }
}
