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

        public static List<AssStyleOptions> Load()
        {
            if (!File.Exists(FileName))
                return new List<AssStyleOptions>();

            using (Stream stream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(AssStyleOptionsList));
                AssStyleOptionsList options = (AssStyleOptionsList)serializer.Deserialize(stream);
                return options.Options;
            }
        }

        public static void Save(IEnumerable<AssStyleOptions> options)
        {
            using (Stream stream = File.Open(FileName, FileMode.Create, FileAccess.Write))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(AssStyleOptionsList));
                AssStyleOptionsList list = new AssStyleOptionsList(options);
                serializer.Serialize(stream, list);
            }
        }
    }
}
