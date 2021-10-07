using System;

namespace YTSubConverter.Shared.Util
{
    public class TimeRange : Range<DateTime>
    {
        public TimeRange(DateTime start, DateTime end)
            : base(start, end)
        {
        }
    }
}
