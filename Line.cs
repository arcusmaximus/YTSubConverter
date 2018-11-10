using System;
using System.Collections.Generic;
using System.Linq;

namespace Arc.YTSubConverter
{
    internal class Line
    {
        public Line(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }

        public Line(DateTime start, DateTime end, string content)
        {
            Start = start;
            End = end;
            Sections.Add(new Section(content));
        }

        public Line(DateTime start, DateTime end, IEnumerable<Section> sections)
        {
            Start = start;
            End = end;
            Sections.AddRange(sections);
        }

        public DateTime Start
        {
            get;
            set;
        }

        public DateTime End
        {
            get;
            set;
        }

        public List<Section> Sections { get; } = new List<Section>();

        public string Text
        {
            get { return string.Join("", Sections.Select(s => s.Text)); }
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
