using System;
using Arc.YTSubConverter.Shared.Formats;

namespace Arc.YTSubConverter.Shared.Util
{
    public static class TimeUtil
    {
        public static DateTime Min(DateTime date1, DateTime date2)
        {
            return date1 < date2 ? date1 : date2;
        }

        public static DateTime Max(DateTime date1, DateTime date2)
        {
            return date1 > date2 ? date1 : date2;
        }

        public static int StartTimeToFrame(DateTime time)
        {
            if (time <= SubtitleDocument.TimeBase)
                return 0;

            return EndTimeToFrame(time) + 1;
        }

        public static int EndTimeToFrame(DateTime time)
        {
            return (int)((time.TimeOfDay.TotalMilliseconds + 1) / 33.36666666666667);
        }

        public static DateTime FrameToStartTime(int frame)
        {
            if (frame <= 0)
                return SubtitleDocument.TimeBase;

            return FrameToTime(frame).AddMilliseconds(-16);
        }

        public static DateTime FrameToEndTime(int frame)
        {
            return FrameToTime(frame).AddMilliseconds(16);
        }

        private static DateTime FrameToTime(int frame)
        {
            if (frame == 0)
                return SubtitleDocument.TimeBase;

            int ms = (int)(frame * 33.36666666666667);
            return SubtitleDocument.TimeBase + TimeSpan.FromMilliseconds(ms);
        }

        public static DateTime RoundTimeToFrameCenter(DateTime time)
        {
            if (time <= SubtitleDocument.TimeBase)
                return SubtitleDocument.TimeBase;

            return FrameToStartTime(StartTimeToFrame(time));
        }
    }
}
