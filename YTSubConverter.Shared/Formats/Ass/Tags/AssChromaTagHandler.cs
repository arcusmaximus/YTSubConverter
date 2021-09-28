using System.Collections.Generic;
using System.Drawing;
using Arc.YTSubConverter.Shared.Animations;
using Arc.YTSubConverter.Shared.Util;

namespace Arc.YTSubConverter.Shared.Formats.Ass.Tags
{
    /// <summary>
    /// Nonstandard tag: \ytchroma(intime, outtime), \ytchroma(offsetX, offsetY, intime, outtime), \ytchroma(color1, color2..., alpha, offsetX, offsetY, intime, outtime)
    /// </summary>
    internal class AssChromaTagHandler : AssTagHandlerBase
    {
        public override string Tag => "ytchroma";

        public override bool AffectsWholeLine => true;

        public override void Handle(AssTagContext context, string arg)
        {
            if (!TryParseArgs(arg, out List<Color> colors, out int alpha, out int maxOffsetX, out int maxOffsetY, out int chromaInMs, out int chromeOutMs))
                return;

            context.PostProcessors.Add(
                () =>
                {
                    AssLine originalLine = context.Line;
                    PointF center = originalLine.Position ?? context.Document.GetDefaultPosition(originalLine.AnchorPoint);
                    List<AssLine> chromaLines = new List<AssLine>();

                    if (colors.Count == 0)
                    {
                        Color baseColor = context.Line.Sections.Count > 0 ? context.Line.Sections[0].ForeColor : Color.White;
                        colors.Add(Color.FromArgb((int)(baseColor.R * alpha / 255.0f), 255, 0, 0));
                        colors.Add(Color.FromArgb((int)(baseColor.G * alpha / 255.0f), 0, 255, 0));
                        colors.Add(Color.FromArgb((int)(baseColor.B * alpha / 255.0f), 0, 0, 255));
                    }

                    if (chromaInMs > 0)
                    {
                        chromaLines.AddRange(CreateChromaLines(originalLine, colors, center, maxOffsetX, maxOffsetY, chromaInMs, true));
                        originalLine.Start = TimeUtil.RoundTimeToFrameCenter(originalLine.Start.AddMilliseconds(chromaInMs));
                    }

                    if (chromeOutMs > 0)
                    {
                        chromaLines.AddRange(CreateChromaLines(originalLine, colors, center, maxOffsetX, maxOffsetY, chromeOutMs, false));
                        originalLine.End = TimeUtil.RoundTimeToFrameCenter(originalLine.End.AddMilliseconds(-chromeOutMs));
                    }

                    return chromaLines;
                }
            );
        }

        private static bool TryParseArgs(string arg, out List<Color> colors, out int alpha, out int maxOffsetX, out int maxOffsetY, out int chromaInMs, out int chromaOutMs)
        {
            colors = new List<Color>();
            alpha = 128;
            maxOffsetX = 20;
            maxOffsetY = 0;
            chromaInMs = 270;
            chromaOutMs = 270;

            List<string> args = ParseStringList(arg);
            if (args == null)
                return false;

            if (args.Count >= 5)
                alpha = ParseHex(args[args.Count - 5]) & 255;

            if (args.Count >= 4 && !int.TryParse(args[args.Count - 4], out maxOffsetX))
                return false;

            if (args.Count >= 3 && !int.TryParse(args[args.Count - 3], out maxOffsetY))
                return false;

            if (args.Count >= 2 && !int.TryParse(args[args.Count - 2], out chromaInMs))
                return false;

            if (args.Count >= 1 && !int.TryParse(args[args.Count - 1], out chromaOutMs))
                return false;

            alpha = 255 - alpha;
            for (int i = 0; i < args.Count - 5; i++)
            {
                colors.Add(ParseColor(args[i], alpha));
            }
            return true;
        }

        private static IEnumerable<AssLine> CreateChromaLines(AssLine originalLine, List<Color> colors, PointF center, int maxOffsetX, int maxOffsetY, int durationMs, bool moveIn)
        {
            if (originalLine.Sections.Count == 0)
                yield break;

            for (int i = 0; i < colors.Count; i++)
            {
                if (colors[i].A == 0)
                    continue;

                AssLine chromaLine = (AssLine)originalLine.Clone();
                foreach (Section section in chromaLine.Sections)
                {
                    int alpha = (int)(section.ForeColor.A * (colors[i].A / 255.0f));
                    section.ForeColor = Color.FromArgb(alpha, colors[i].R, colors[i].G, colors[i].B);
                    section.BackColor = Color.Empty;
                    section.ShadowColors.Clear();
                }

                float offsetFactor = colors.Count > 1 ? (float)i / (colors.Count - 1) : 0.5f;
                float offsetX = offsetFactor * (-maxOffsetX * 2) + maxOffsetX;
                float offsetY = offsetFactor * (-maxOffsetY * 2) + maxOffsetY;
                PointF farPosition = new PointF(center.X + offsetX, center.Y + offsetY);
                PointF nearPosition = new PointF(center.X + offsetX / 5, center.Y + offsetY / 5);
                if (moveIn)
                {
                    chromaLine.End = TimeUtil.RoundTimeToFrameCenter(originalLine.Start.AddMilliseconds(durationMs));
                    chromaLine.Animations.Add(new MoveAnimation(chromaLine.Start, farPosition, chromaLine.End, nearPosition));
                }
                else
                {
                    chromaLine.Start = TimeUtil.RoundTimeToFrameCenter(originalLine.End.AddMilliseconds(-durationMs));
                    chromaLine.Animations.Add(new MoveAnimation(chromaLine.Start, nearPosition, chromaLine.End, farPosition));
                }
                yield return chromaLine;
            }
        }
    }
}
