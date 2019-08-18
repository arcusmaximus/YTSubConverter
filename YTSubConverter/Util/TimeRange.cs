using System;

namespace Arc.YTSubConverter.Util
{
    internal class TimeRange : Range<DateTime>
    {
        public TimeRange(DateTime start, DateTime end)
            : base(start, end)
        {
        }
    }
}
