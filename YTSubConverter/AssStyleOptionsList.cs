using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Arc.YTSubConverter
{
    [XmlRoot("StyleOptions")]
    public class AssStyleOptionsList
    {
        private const string FileName = "StyleOptions.xml";

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

        public static List<AssStyleOptions> Load(string filePath = FileName)
        {
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

        public static void Save(IEnumerable<AssStyleOptions> options, string filePath = FileName)
        {
            using Stream stream = File.Open(filePath, FileMode.Create, FileAccess.Write);
            XmlSerializer serializer = new XmlSerializer(typeof(AssStyleOptionsList));
            AssStyleOptionsList list = new AssStyleOptionsList(options);
            serializer.Serialize(stream, list);
        }
    }
}
