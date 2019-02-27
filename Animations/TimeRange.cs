using System;
using Arc.YTSubConverter.Util;

namespace Arc.YTSubConverter.Animations
{
    internal class TimeRange : IComparable<TimeRange>
    {
        public TimeRange(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
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

        public bool Contains(DateTime time)
        {
            return time >= Start && time < End;
        }

        public bool Intersects(TimeRange other)
        {
            return Start < other.End && End > other.Start;
        }

        public void IntersectWith(TimeRange other)
        {
            if (!Intersects(other))
                throw new InvalidOperationException("Can't intersect with a non-overlapping time range");

            Start = TimeUtil.Max(Start, other.Start);
            End = TimeUtil.Min(End, other.End);
        }

        public void UnionWith(TimeRange other)
        {
            if (!Intersects(other))
                throw new InvalidOperationException("Can't union with a non-overlapping time range");

            Start = TimeUtil.Min(Start, other.Start);
            End = TimeUtil.Max(End, other.End);
        }

        public int CompareTo(TimeRange other)
        {
            return Start.CompareTo(other.Start);
        }
    }
}
