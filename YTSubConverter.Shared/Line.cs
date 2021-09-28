using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Arc.YTSubConverter.Shared
{
    public class Line : ICloneable
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

        public Line(Line line)
        {
            Assign(line);
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

        public AnchorPoint AnchorPoint
        {
            get;
            set;
        } = AnchorPoint.BottomCenter;

        public PointF? Position
        {
            get;
            set;
        }

        public HorizontalTextDirection HorizontalTextDirection
        {
            get;
            set;
        }

        public VerticalTextType VerticalTextType
        {
            get;
            set;
        }

        public bool AndroidDarkTextHackAllowed
        {
            get;
            set;
        } = true;

        public override string ToString()
        {
            return Text;
        }

        public virtual object Clone()
        {
            return new Line(this);
        }

        protected virtual void Assign(Line line)
        {
            Start = line.Start;
            End = line.End;
            AnchorPoint = line.AnchorPoint;
            Position = line.Position;
            HorizontalTextDirection = line.HorizontalTextDirection;
            VerticalTextType = line.VerticalTextType;
            AndroidDarkTextHackAllowed = line.AndroidDarkTextHackAllowed;

            Sections.Clear();
            Sections.AddRange(line.Sections.Select(CreateSection));
        }

        protected virtual Section CreateSection(Section section)
        {
            return new Section(section);
        }
    }
}
