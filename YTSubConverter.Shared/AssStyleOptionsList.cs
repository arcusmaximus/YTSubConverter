using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace YTSubConverter.Shared
{
    [XmlRoot("StyleOptions")]
    public class AssStyleOptionsList
    {
        public const string FileName = "StyleOptions.xml";

        public AssStyleOptionsList()
        {
        }

        public AssStyleOptionsList(IEnumerable<AssStyleOptions> options)
        {
            Options = options.ToList();
        }

        [XmlElement("Style")]
        public List<AssStyleOptions> Options
        {
            get;
            set;
        }

        public static List<AssStyleOptions> LoadFromFile(string filePath = null)
        {
            filePath ??= GetDefaultFilePath();
            if (!File.Exists(filePath))
                return new List<AssStyleOptions>();

            try
            {
                using Stream stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
                XmlSerializer serializer = new XmlSerializer(typeof(AssStyleOptionsList));
                AssStyleOptionsList options = (AssStyleOptionsList)serializer.Deserialize(stream);
                return options.Options;
            }
            catch
            {
                return new List<AssStyleOptions>();
            }
        }

        public static List<AssStyleOptions> LoadFromString(string data)
        {
            try
            {
                using StringReader reader = new StringReader(data);
                XmlSerializer serializer = new XmlSerializer(typeof(AssStyleOptionsList));
                AssStyleOptionsList options = (AssStyleOptionsList)serializer.Deserialize(reader);
                return options.Options;
            }
            catch
            {
                return new List<AssStyleOptions>();
            }
        }

        public static void SaveToFile(IEnumerable<AssStyleOptions> options, string filePath = null)
        {
            using Stream stream = File.Open(filePath ?? GetDefaultFilePath(), FileMode.Create, FileAccess.Write);
            XmlSerializer serializer = new XmlSerializer(typeof(AssStyleOptionsList));
            AssStyleOptionsList list = new AssStyleOptionsList(options);
            serializer.Serialize(stream, list);
        }

        private static string GetDefaultFilePath()
        {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), FileName);
        }
    }
}
