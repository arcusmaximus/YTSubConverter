using System;

namespace Arc.YTSubConverter.Shared.Util
{
    public class TimeRange : Range<DateTime>
    {
        public TimeRange(DateTime start, DateTime end)
            : base(start, end)
        {
        }
    }
}
