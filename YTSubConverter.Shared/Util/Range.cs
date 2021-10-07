using System;

namespace YTSubConverter.Shared.Util
{
    public class Range<T> : IComparable<Range<T>>
        where T : IComparable<T>
    {
        public Range(T start, T end)
        {
            Start = start;
            End = end;
        }

        public T Start
        {
            get;
            set;
        }

        public T End
        {
            get;
            set;
        }

        public bool Contains(T point)
        {
            return point.CompareTo(Start) >= 0 && point.CompareTo(End) < 0;
        }

        public bool Overlaps(Range<T> other)
        {
            return Start.CompareTo(other.End) < 0 && End.CompareTo(other.Start) > 0;
        }

        public void IntersectWith(Range<T> other)
        {
            if (!Overlaps(other))
                throw new InvalidOperationException("Can't intersect with a non-overlapping time range");

            Start =  Max(Start, other.Start);
            End = Min(End, other.End);
        }

        public void UnionWith(Range<T> other)
        {
            if (!Overlaps(other))
                throw new InvalidOperationException("Can't union with a non-overlapping time range");

            Start = Min(Start, other.Start);
            End = Max(End, other.End);
        }

        public int CompareTo(Range<T> other)
        {
            return Start.CompareTo(other.Start);
        }

        private static T Min(T x, T y)
        {
            return x.CompareTo(y) < 0 ? x : y;
        }

        private static T Max(T x, T y)
        {
            return x.CompareTo(y) > 0 ? x : y;
        }
    }
}
