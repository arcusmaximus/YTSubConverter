using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Arc.YTSubConverter
{
    [XmlRoot("StyleOptions")]
    public class AssStyleOptionsList
    {
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
    }
}
