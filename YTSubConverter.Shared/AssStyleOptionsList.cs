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
                return [];

            try
            {
                using Stream stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
                XmlSerializer serializer = new(typeof(AssStyleOptionsList));
                AssStyleOptionsList options = (AssStyleOptionsList)serializer.Deserialize(stream);
                return options.Options;
            }
            catch
            {
                return [];
            }
        }

        public static List<AssStyleOptions> LoadFromString(string data)
        {
            try
            {
                using StringReader reader = new(data);
                XmlSerializer serializer = new(typeof(AssStyleOptionsList));
                AssStyleOptionsList options = (AssStyleOptionsList)serializer.Deserialize(reader);
                return options.Options;
            }
            catch
            {
                return [];
            }
        }

        public static void SaveToFile(IEnumerable<AssStyleOptions> options, string filePath = null)
        {
            using Stream stream = File.Open(filePath ?? GetDefaultFilePath(), FileMode.Create, FileAccess.Write);
            XmlSerializer serializer = new(typeof(AssStyleOptionsList));
            AssStyleOptionsList list = new(options);
            serializer.Serialize(stream, list);
        }

        public static string GetDefaultFilePath()
        {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), FileName);
        }
    }
}
