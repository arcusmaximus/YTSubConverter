using System;
using System.Collections.Generic;
using Arc.YTSubConverter.Formats.Ass.KaraokeTypes;

namespace Arc.YTSubConverter.Formats.Ass.Tags
{
    internal class AssKaraokeTypeTagHandler : AssTagHandlerBase
    {
        private static readonly IKaraokeType SimpleKaraokeType = new SimpleKaraokeType();
        private static readonly IKaraokeType FadeKaraokeType = new FadeKaraokeType();
        private static readonly IKaraokeType GlitchKaraokeType = new GlitchKaraokeType();

        public override string Tag => "ytkt";

        public override bool AffectsWholeLine => true;

        public override void Handle(AssTagContext context, string arg)
        {
            context.Line.KaraokeType = GetKaraokeType(arg);
        }

        private static IKaraokeType GetKaraokeType(string arg)
        {
            List<string> args = ParseStringList(arg);
            string typeName = args != null ? args[0] : arg;
            switch (typeName.ToLower())
            {
                case "fade":
                    return FadeKaraokeType;

                case "glitch":
                    return GlitchKaraokeType;

                case "cursor":
                    return CreateCursorKaraokeType(args, false);

                case "lcursor":
                    return CreateCursorKaraokeType(args, true);

                default:
                    return SimpleKaraokeType;
            }
        }

        // \ytktCursor
        // \ytkt(Cursor,<cursor>)
        // \ytkt(Cursor,<formatting tags>,<cursor>)
        // \ytkt(Cursor,interval,<tags1>,<cursor1>,<tags2>,<cursor2>,...)
        private static IKaraokeType CreateCursorKaraokeType(List<string> args, bool beforeSinging)
        {
            TimeSpan interval;
            List<string> cursors = new List<string>();

            if (args == null || args.Count == 1)
            {
                interval = TimeSpan.FromHours(1);
                cursors.Add("_");
            }
            else if (args.Count == 2)
            {
                interval = TimeSpan.FromHours(1);
                cursors.Add(args[1]);
            }
            else if (args.Count == 3)
            {
                interval = TimeSpan.FromHours(1);
                cursors.Add("{" + args[1] + "}" + args[2]);
            }
            else
            {
                if (!int.TryParse(args[1], out int intervalMs))
                    intervalMs = int.MaxValue;

                interval = TimeSpan.FromMilliseconds(intervalMs);

                for (int i = 2; i < args.Count; i += 2)
                {
                    cursors.Add("{" + args[i] + "}" + (i + 1 < args.Count ? args[i + 1] : "_"));
                }
            }

            return new CursorKaraokeType(cursors, interval, beforeSinging);
        }
    }
}
