using System;
using Arc.YTSubConverter.Formats;

namespace Arc.YTSubConverter.Util
{
    internal static class TimeUtil
    {
        public static DateTime Min(DateTime date1, DateTime date2)
        {
            return date1 < date2 ? date1 : date2;
        }

        public static DateTime Max(DateTime date1, DateTime date2)
        {
            return date1 > date2 ? date1 : date2;
        }

        public static int TimeToFrame(DateTime time)
        {
            return (int)(time.TimeOfDay.TotalMilliseconds / 33.36666666666667);
        }

        public static DateTime FrameToTime(int frame)
        {
            if (frame == 0)
                return SubtitleDocument.TimeBase;

            int ms = (int)(frame * 33.36666666666667) + 1;
            return SubtitleDocument.TimeBase + TimeSpan.FromMilliseconds(ms);
        }

        public static DateTime SnapTimeToFrame(DateTime time)
        {
            int frame = TimeToFrame(time);
            return FrameToTime(frame);
        }
    }
}
