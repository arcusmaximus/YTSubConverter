using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Arc.YTSubConverter.Animations;
using Arc.YTSubConverter.Util;

namespace Arc.YTSubConverter.Formats.Ass
{
    internal partial class AssDocument : SubtitleDocument
    {
        private delegate void TagHandler(TagContext context, string arg);
        private delegate void TransformTagHandler(TagContext context, DateTime startTime, DateTime endTime, int accel, string arg);

        private static readonly Dictionary<string, TagHandler> TagHandlers;
        private static readonly Dictionary<string, TransformTagHandler> TransformTagHandlers;

        static AssDocument()
        {
            TagHandlers = new Dictionary<string, TagHandler>
                          {
                              { "b", HandleBoldTag },
                              { "i", HandleItalicTag },
                              { "u", HandleUnderlineTag },
                              { "fn", HandleFontTag },
                              { "c", HandleForeColorTag },
                              { "1c", HandleForeColorTag },
                              { "2c", HandleSecondaryColorTag },
                              { "3c", HandleOutlineColorTag },
                              { "4c", HandleShadowColorTag },
                              { "1a", HandleForeColorAlphaTag },
                              { "2a", HandleSecondaryColorAlphaTag },
                              { "3a", HandleOutlineAlphaTag },
                              { "4a", HandleShadowAlphaTag },
                              { "pos", HandlePositionTag },
                              { "an", HandleAlignmentTag },
                              { "k", HandleDurationTag },
                              { "r", HandleResetTag },
                              { "fad", HandleSimpleFadeTag },
                              { "fade", HandleComplexFadeTag },
                              { "move", HandleMoveTag },
                              { "ytshake", HandleShakeTag },
                              { "ytchroma", HandleChromaTag },
                              { "t", HandleTransformTag }
                          };

            TransformTagHandlers = new Dictionary<string, TransformTagHandler>
                                   {
                                       { "c", HandleTransformForeColorTag },
                                       { "1c", HandleTransformForeColorTag },
                                       { "2c", HandleTransformSecondaryColorTag },
                                       { "3c", HandleTransformOutlineColorTag },
                                       { "4c", HandleTransformShadowColorTag },
                                       { "1a", HandleTransformForeColorAlphaTag },
                                       { "2a", HandleTransformSecondaryColorAlphaTag },
                                       { "3a", HandleTransformOutlineAlphaTag },
                                       { "4a", HandleTransformShadowAlphaTag }
                                   };
        }

        public AssDocument(string filePath, List<AssStyleOptions> styleOptions = null)
        {
            Dictionary<string, AssSection> fileSections = ReadDocument(filePath);

            AssSection infoSection = fileSections["Script Info"];
            VideoDimensions = new Size(infoSection.GetItemInt("PlayResX", 384), infoSection.GetItemInt("PlayResY", 288));

            Styles = fileSections["V4+ Styles"].MapItems("Style", i => new AssStyle(i))
                                               .ToList();

            Dictionary<string, AssStyle> styleLookup = Styles.ToDictionary(s => s.Name);
            Dictionary<string, AssStyleOptions> styleOptionsLookup = null;
            if (styleOptions != null)
                styleOptionsLookup = styleOptions.ToDictionary(o => o.Name);

            foreach (AssDialogue dialogue in fileSections["Events"].MapItems("Dialogue", i => new AssDialogue(i)))
            {
                AssStyle style = styleLookup.GetOrDefault(dialogue.Style);
                AssStyleOptions options = styleOptionsLookup?.GetOrDefault(dialogue.Style);

                List<ExtendedLine> lines = ParseLine(dialogue, style, options);
                Lines.AddRange(lines.SelectMany(ExpandLine));
            }

            foreach (ExtendedLine line in Lines)
            {
                MergeIdenticallyFormattedSections(line);
                line.NormalizeAlpha();
            }
        }

        public List<AssStyle> Styles
        {
            get;
        }

        private Dictionary<string, AssSection> ReadDocument(string filePath)
        {
            Dictionary<string, AssSection> sections = new Dictionary<string, AssSection>();
            AssSection currentSection = null;

            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith(";"))
                        continue;

                    if (line.StartsWith("[") && line.EndsWith("]"))
                    {
                        currentSection = new AssSection();
                        sections.Add(line.Substring(1, line.Length - 2), currentSection);
                        continue;
                    }

                    Match match = Regex.Match(line, @"(\w+):\s*(.+)");
                    if (!match.Success)
                        throw new InvalidDataException($"Unrecognized line in .ass: {line}");

                    if (currentSection == null)
                        throw new InvalidDataException($"Line {line} is not inside a section");

                    string type = match.Groups[1].Value;
                    List<string> values = match.Groups[2].Value.Split(",", currentSection.Format?.Count);
                    if (type == "Format")
                    {
                        if (currentSection.Format != null)
                            throw new InvalidDataException("Section has multiple Format items");

                        currentSection.SetFormat(values.Select(v => v.Trim()).ToList());
                    }
                    else
                    {
                        currentSection.AddItem(type, values);
                    }
                }
            }

            return sections;
        }

        private List<ExtendedLine> ParseLine(AssDialogue dialogue, AssStyle style, AssStyleOptions styleOptions)
        {
            DateTime startTime = TimeUtil.SnapTimeToFrame(dialogue.Start.AddMilliseconds(32));
            DateTime endTime = TimeUtil.SnapTimeToFrame(dialogue.End).AddMilliseconds(32);
            ExtendedLine line = new ExtendedLine(startTime, endTime) { AnchorPoint = style.AnchorPoint };

            TagContext context = new TagContext
                                 {
                                     Document = this,
                                     Dialogue = dialogue,
                                     Style = style,
                                     StyleOptions = styleOptions,
                                     Line = line,
                                     Section = new ExtendedSection(null)
                                 };

            ApplyStyle(context.Section, style, styleOptions);
            
            string text = Regex.Replace(dialogue.Text, @"(?:\\N)+$", "");
            int start = 0;
            foreach (Match match in Regex.Matches(text, @"\{(?:\\(?<tag>fn|\d?[a-z]+)(?<arg>\([^\{\}\(\)]*\)|[^\{\}\(\)\\]*))+\}"))
            {
                int end = match.Index;

                if (end > start)
                {
                    context.Section.Text = text.Substring(start, end - start).Replace("\\N", "\r\n");
                    line.Sections.Add(context.Section);

                    context.Section = (ExtendedSection)context.Section.Clone();
                    context.Section.Text = null;
                    context.Section.Duration = TimeSpan.Zero;
                }

                CaptureCollection tags = match.Groups["tag"].Captures;
                CaptureCollection arguments = match.Groups["arg"].Captures;
                for (int i = 0; i < tags.Count; i++)
                {
                    if (TagHandlers.TryGetValue(tags[i].Value, out TagHandler handler))
                        handler(context, arguments[i].Value);
                }

                start = match.Index + match.Length;
            }

            if (start < text.Length)
            {
                context.Section.Text = text.Substring(start, text.Length - start).Replace("\\N", "\r\n");
                line.Sections.Add(context.Section);
            }

            List<ExtendedLine> lines = new List<ExtendedLine> { line };
            foreach (TagContext.PostProcessor postProcessor in context.PostProcessors)
            {
                List<ExtendedLine> extraLines = postProcessor();
                if (extraLines != null)
                    lines.AddRange(extraLines);
            }

            return lines;
        }

        private static void HandleBoldTag(TagContext context, string arg)
        {
            context.Section.Bold = arg != "0";
        }

        private static void HandleItalicTag(TagContext context, string arg)
        {
            context.Section.Italic = arg != "0";
        }

        private static void HandleUnderlineTag(TagContext context, string arg)
        {
            context.Section.Underline = arg != "0";
        }

        private static void HandleFontTag(TagContext context, string arg)
        {
            context.Section.Font = arg;
        }

        private static void HandleForeColorTag(TagContext context, string arg)
        {
            context.Section.ForeColor = ParseColor(arg, context.Section.ForeColor.A);
            context.Section.Animations.RemoveAll(a => a is ForeColorAnimation);
        }

        private static void HandleSecondaryColorTag(TagContext context, string arg)
        {
            context.Section.SecondaryColor = ParseColor(arg, context.Section.SecondaryColor.A);
            context.Section.Animations.RemoveAll(a => a is SecondaryColorAnimation);
        }

        private static void HandleOutlineColorTag(TagContext context, string arg)
        {
            if (!context.Style.HasOutline)
                return;

            if (context.Style.OutlineIsBox)
            {
                context.Section.BackColor = ParseColor(arg, context.Section.BackColor.A);
                context.Section.Animations.RemoveAll(a => a is BackColorAnimation);
            }
            else
            {
                context.Section.ShadowColor = ParseColor(arg, context.Section.ShadowColor.A);
                context.Section.Animations.RemoveAll(a => a is ShadowColorAnimation);
            }
        }

        private static void HandleShadowColorTag(TagContext context, string arg)
        {
            if (!context.Style.HasShadow)
                return;

            context.Section.ShadowColor = ParseColor(arg, context.Section.ShadowColor.A);
            context.Section.Animations.RemoveAll(a => a is ShadowColorAnimation);
        }

        private static void HandleForeColorAlphaTag(TagContext context, string arg)
        {
            int alpha = 255 - ParseHex(arg);
            context.Section.ForeColor = ColorUtil.ChangeColorAlpha(context.Section.ForeColor, alpha);
            context.Section.Animations.RemoveAll(a => a is ForeColorAnimation);
        }

        private static void HandleSecondaryColorAlphaTag(TagContext context, string arg)
        {
            int alpha = 255 - ParseHex(arg);
            context.Section.SecondaryColor = ColorUtil.ChangeColorAlpha(context.Section.SecondaryColor, alpha);
            context.Section.Animations.RemoveAll(a => a is SecondaryColorAnimation);
        }

        private static void HandleOutlineAlphaTag(TagContext context, string arg)
        {
            if (!context.Style.HasOutline)
                return;

            int alpha = 255 - ParseHex(arg);

            if (context.Style.OutlineIsBox)
            {
                context.Section.BackColor = ColorUtil.ChangeColorAlpha(context.Section.BackColor, alpha);
                context.Section.Animations.RemoveAll(a => a is BackColorAnimation);
            }
            else
            {
                context.Section.ShadowColor = ColorUtil.ChangeColorAlpha(context.Section.ShadowColor, alpha);
                context.Section.Animations.RemoveAll(a => a is ShadowColorAnimation);
            }
        }

        private static void HandleShadowAlphaTag(TagContext context, string arg)
        {
            if (!context.Style.HasShadow)
                return;

            int alpha = 255 - ParseHex(arg);
            context.Section.ShadowColor = ColorUtil.ChangeColorAlpha(context.Section.ShadowColor, alpha);
            context.Section.Animations.RemoveAll(a => a is ShadowColorAnimation);
        }

        private static void HandlePositionTag(TagContext context, string arg)
        {
            List<float> coords = ParseNumberList(arg);
            if (coords == null || coords.Count != 2 || context.Line.Position != null)
                return;

            context.Line.Position = new PointF(coords[0], coords[1]);
        }

        private static void HandleAlignmentTag(TagContext context, string arg)
        {
            if (!int.TryParse(arg, out int alignment))
                return;

            context.Line.AnchorPoint = GetAnchorPointFromAlignment(alignment);
        }

        private static void HandleDurationTag(TagContext context, string arg)
        {
            int centiSeconds = int.Parse(arg);
            context.Section.Duration = TimeSpan.FromMilliseconds(centiSeconds * 10);
        }

        private static void HandleResetTag(TagContext context, string arg)
        {
            ApplyStyle(context.Section, context.Style, context.StyleOptions);
        }

        private static void HandleSimpleFadeTag(TagContext context, string arg)
        {
            List<float> times = ParseNumberList(arg);
            if (times == null || times.Count != 2)
                return;

            ExtendedLine line = context.Line;
            DateTime fadeInStartTime = line.Start;
            DateTime fadeInEndTime = line.Start.AddMilliseconds(times[0]);
            DateTime fadeOutStartTime = line.End.AddMilliseconds(-times[1]);
            DateTime fadeOutEndTime = line.End;

            if (fadeInEndTime > fadeInStartTime)
                line.Animations.Add(new FadeAnimation(fadeInStartTime, 0, fadeInEndTime, 255));

            if (fadeOutEndTime > fadeOutStartTime)
                line.Animations.Add(new FadeAnimation(fadeOutStartTime, 255, fadeOutEndTime, 0));
        }

        private static void HandleComplexFadeTag(TagContext context, string arg)
        {
            List<float> args = ParseNumberList(arg);
            if (args == null || args.Count != 7)
                return;

            ExtendedLine line = context.Line;

            int initialAlpha = 255 - (int)args[0];
            int midAlpha = 255 - (int)args[1];
            int finalAlpha = 255 - (int)args[2];

            DateTime fadeInStartTime = line.Start.AddMilliseconds(args[3]);
            DateTime fadeInEndTime = line.Start.AddMilliseconds(args[4]);
            DateTime fadeOutStartTime = line.Start.AddMilliseconds(args[5]);
            DateTime fadeOutEndTime = line.Start.AddMilliseconds(args[6]);

            if (fadeInEndTime > fadeInStartTime)
                line.Animations.Add(new FadeAnimation(fadeInStartTime, initialAlpha, fadeInEndTime, midAlpha));

            if (fadeOutEndTime > fadeOutStartTime)
                line.Animations.Add(new FadeAnimation(fadeOutStartTime, midAlpha, fadeOutEndTime, finalAlpha));
        }

        private static void HandleMoveTag(TagContext context, string arg)
        {
            List<float> args = ParseNumberList(arg);
            if (args == null || args.Count < 4)
                return;

            ExtendedLine line = context.Line;
            PointF startPos = new PointF(args[0], args[1]);
            PointF endPos = new PointF(args[2], args[3]);

            DateTime startTime = line.Start;
            DateTime endTime = line.End;
            if (args.Count >= 6)
            {
                startTime = line.Start.AddMilliseconds(args[4]);
                endTime = line.Start.AddMilliseconds(args[5]);
            }

            if (endTime > startTime)
                line.Animations.Add(new MoveAnimation(startTime, startPos, endTime, endPos));
        }

        /// <summary>
        /// Nonstandard tag: \ytshake, \ytshake(radius), \ytshake(radiusX, radiusY), \ytshake(radius, t1, t2), \ytshake(radiusX, radiusY, t1, t2)
        /// </summary>
        private static void HandleShakeTag(TagContext context, string arg)
        {
            if (!TryParseShakeArgs(context, arg, out SizeF radius, out DateTime startTime, out DateTime endTime))
                return;

            context.PostProcessors.Add(
                () =>
                {
                    if (context.Line.Position != null)
                        context.Line.Animations.Add(new ShakeAnimation(startTime, endTime, context.Line.Position.Value, radius));

                    return null;
                }
            );
        }

        private static bool TryParseShakeArgs(TagContext context, string arg, out SizeF radius, out DateTime startTime, out DateTime endTime)
        {
            int defaultRadius = 20;
            radius = new SizeF(defaultRadius, defaultRadius);
            startTime = context.Line.Start;
            endTime = context.Line.End;

            if (string.IsNullOrWhiteSpace(arg))
                return true;

            List<float> args = ParseNumberList(arg);
            if (args == null)
                return false;

            switch (args.Count)
            {
                case 0:
                    return true;

                case 1:
                    radius = new SizeF(args[0], args[0]);
                    return true;

                case 2:
                    radius = new SizeF(args[0], args[1]);
                    return true;

                case 3:
                    radius = new SizeF(args[0], args[0]);
                    startTime = context.Line.Start.AddMilliseconds(args[1]);
                    endTime = context.Line.Start.AddMilliseconds(args[2]);
                    return true;

                case 4:
                    radius = new SizeF(args[0], args[1]);
                    startTime = context.Line.Start.AddMilliseconds(args[2]);
                    endTime = context.Line.Start.AddMilliseconds(args[3]);
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Nonstandard tag: \ytchroma(intime, outtime), \ytchroma(offsetX, offsetY, intime, outtime), \ytchroma(color1, color2..., alpha, offsetX, offsetY, intime, outtime)
        /// </summary>
        private static void HandleChromaTag(TagContext context, string arg)
        {
            if (!TryParseChromaArgs(arg, out List<Color> colors, out int alpha, out int maxOffsetX, out int maxOffsetY, out int chromaInMs, out int chromeOutMs))
                return;

            context.PostProcessors.Add(
                () =>
                {
                    ExtendedLine originalLine = context.Line;
                    if (originalLine.Position == null)
                        return null;

                    List<ExtendedLine> chromaLines = new List<ExtendedLine>();

                    if (colors.Count == 0)
                    {
                        Color baseColor = context.Line.Sections.Count > 0 ? context.Line.Sections[0].ForeColor : Color.White;
                        colors.Add(Color.FromArgb((int)(baseColor.R * alpha / 255.0f), 255, 0, 0));
                        colors.Add(Color.FromArgb((int)(baseColor.G * alpha / 255.0f), 0, 255, 0));
                        colors.Add(Color.FromArgb((int)(baseColor.B * alpha / 255.0f), 0, 0, 255));
                    }

                    if (chromaInMs > 0)
                    {
                        chromaLines.AddRange(CreateChromaLines(originalLine, colors, maxOffsetX, maxOffsetY, chromaInMs, true));
                        originalLine.Start = TimeUtil.SnapTimeToFrame(originalLine.Start.AddMilliseconds(chromaInMs));
                    }

                    if (chromeOutMs > 0)
                    {
                        chromaLines.AddRange(CreateChromaLines(originalLine, colors, maxOffsetX, maxOffsetY, chromeOutMs, false));
                        originalLine.End = TimeUtil.SnapTimeToFrame(originalLine.End.AddMilliseconds(-chromeOutMs));
                    }

                    return chromaLines;
                }
            );
        }

        private static bool TryParseChromaArgs(string arg, out List<Color> colors, out int alpha, out int maxOffsetX, out int maxOffsetY, out int chromaInMs, out int chromaOutMs)
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
                alpha = ParseHex(args[args.Count - 5]);

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

        private static IEnumerable<ExtendedLine> CreateChromaLines(ExtendedLine originalLine, List<Color> colors, int maxOffsetX, int maxOffsetY, int durationMs, bool moveIn)
        {
            if (originalLine.Sections.Count == 0)
                yield break;

            for (int i = 0; i < colors.Count; i++)
            {
                if (colors[i].A == 0)
                    continue;

                ExtendedLine chromaLine = new ExtendedLine(originalLine.Start, originalLine.End) { AnchorPoint = originalLine.AnchorPoint };
                chromaLine.Sections.Add(
                    new ExtendedSection(originalLine.Text)
                    {
                        ForeColor = colors[i],
                        Bold = originalLine.Sections[0].Bold,
                        Italic = originalLine.Sections[0].Italic,
                        Underline = originalLine.Sections[0].Underline
                    }
                );

                float offsetFactor = colors.Count > 1 ? (float)i / (colors.Count - 1) : 0.5f;
                float offsetX = offsetFactor * (-maxOffsetX * 2) + maxOffsetX;
                float offsetY = offsetFactor * (-maxOffsetY * 2) + maxOffsetY;
                PointF farPosition = new PointF(originalLine.Position.Value.X + offsetX, originalLine.Position.Value.Y + offsetY);
                PointF nearPosition = new PointF(originalLine.Position.Value.X + offsetX / 5, originalLine.Position.Value.Y + offsetY / 5);
                if (moveIn)
                {
                    chromaLine.End = TimeUtil.SnapTimeToFrame(originalLine.Start.AddMilliseconds(durationMs)).AddMilliseconds(32);
                    chromaLine.Animations.Add(new MoveAnimation(chromaLine.Start, farPosition, chromaLine.End, nearPosition));
                }
                else
                {
                    chromaLine.Start = TimeUtil.SnapTimeToFrame(originalLine.End.AddMilliseconds(-durationMs));
                    chromaLine.Animations.Add(new MoveAnimation(chromaLine.Start, nearPosition, chromaLine.End, farPosition));
                }
                yield return chromaLine;
            }
        }

        private static void HandleTransformTag(TagContext context, string arg)
        {
            if (!TryParseTransformArgs(context, arg, out DateTime startTime, out DateTime endTime, out int accel, out string modifiers))
                return;

            foreach (Match match in Regex.Matches(modifiers, @"\\(?<tag>\d?[a-z]+)(?<arg>[^\\]*)"))
            {
                if (TransformTagHandlers.TryGetValue(match.Groups["tag"].Value, out TransformTagHandler handler))
                    handler(context, startTime, endTime, accel, match.Groups["arg"].Value);
            }
        }

        private static bool TryParseTransformArgs(TagContext context, string arg, out DateTime startTime, out DateTime endTime, out int accel, out string modifiers)
        {
            startTime = context.Line.Start;
            endTime = context.Line.End;
            accel = 1;
            modifiers = null;

            List<string> args = ParseStringList(arg);
            if (args == null)
                return false;

            switch (args.Count)
            {
                case 1:
                {
                    modifiers = args[0];
                    return true;
                }

                case 2:
                {
                    if (!int.TryParse(args[0], out accel))
                        return false;

                    modifiers = args[1];
                    return true;
                }

                case 3:
                {
                    if (!int.TryParse(args[0], out int t1) ||
                        !int.TryParse(args[1], out int t2))
                        return false;

                    startTime = context.Line.Start.AddMilliseconds(t1);
                    endTime = context.Line.Start.AddMilliseconds(t2);
                    modifiers = args[2];
                    return true;
                }

                case 4:
                {
                    if (!int.TryParse(args[0], out int t1) ||
                        !int.TryParse(args[1], out int t2) ||
                        !int.TryParse(args[2], out accel))
                        return false;

                    startTime = context.Line.Start.AddMilliseconds(t1);
                    endTime = context.Line.Start.AddMilliseconds(t2);
                    modifiers = args[3];
                    return true;
                }

                default:
                    return false;
            }
        }

        private static void HandleTransformForeColorTag(TagContext context, DateTime startTime, DateTime endTime, int accel, string arg)
        {
            ForeColorAnimation anim = FetchColorAnimation(context, startTime, endTime, s => s.ForeColor, (s, e, c) => new ForeColorAnimation(s, c, e, c));
            anim.EndColor = ParseColor(arg, anim.EndColor.A);
        }

        private static void HandleTransformSecondaryColorTag(TagContext context, DateTime startTime, DateTime endTime, int accel, string arg)
        {
            SecondaryColorAnimation anim = FetchColorAnimation(context, startTime, endTime, s => s.SecondaryColor, (s, e, c) => new SecondaryColorAnimation(s, c, e, c));
            anim.EndColor = ParseColor(arg, anim.EndColor.A);
        }

        private static void HandleTransformOutlineColorTag(TagContext context, DateTime startTime, DateTime endTime, int accel, string arg)
        {
            if (!context.Style.HasOutline)
                return;

            if (context.Style.OutlineIsBox)
            {
                BackColorAnimation anim = FetchColorAnimation(context, startTime, endTime, s => s.BackColor, (s, e, c) => new BackColorAnimation(s, c, e, c));
                anim.EndColor = ParseColor(arg, anim.EndColor.A);
            }
            else
            {
                HandleTransformShadowColorTag(context, startTime, endTime, accel, arg);
            }
        }

        private static void HandleTransformShadowColorTag(TagContext context, DateTime startTime, DateTime endTime, int accel, string arg)
        {
            ShadowColorAnimation anim = FetchColorAnimation(context, startTime, endTime, s => s.ShadowColor, (s, e, c) => new ShadowColorAnimation(s, c, e, c));
            anim.EndColor = ParseColor(arg, anim.EndColor.A);
        }

        private static void HandleTransformForeColorAlphaTag(TagContext context, DateTime startTime, DateTime endTime, int accel, string arg)
        {
            ForeColorAnimation anim = FetchColorAnimation(context, startTime, endTime, s => s.ForeColor, (s, e, c) => new ForeColorAnimation(s, c, e, c));
            anim.EndColor = ColorUtil.ChangeColorAlpha(anim.EndColor, 255 - ParseHex(arg));
        }

        private static void HandleTransformSecondaryColorAlphaTag(TagContext context, DateTime startTime, DateTime endTime, int accel, string arg)
        {
            SecondaryColorAnimation anim = FetchColorAnimation(context, startTime, endTime, s => s.SecondaryColor, (s, e, c) => new SecondaryColorAnimation(s, c, e, c));
            anim.EndColor = ColorUtil.ChangeColorAlpha(anim.EndColor, 255 - ParseHex(arg));
        }

        private static void HandleTransformOutlineAlphaTag(TagContext context, DateTime startTime, DateTime endTime, int accel, string arg)
        {
            if (!context.Style.HasOutline)
                return;

            if (context.Style.OutlineIsBox)
            {
                BackColorAnimation anim = FetchColorAnimation(context, startTime, endTime, s => s.BackColor, (s, e, c) => new BackColorAnimation(s, c, e, c));
                anim.EndColor = ColorUtil.ChangeColorAlpha(anim.EndColor, 255 - ParseHex(arg));
            }
            else
            {
                HandleTransformShadowAlphaTag(context, startTime, endTime, accel, arg);
            }
        }

        private static void HandleTransformShadowAlphaTag(TagContext context, DateTime startTime, DateTime endTime, int accel, string arg)
        {
            ShadowColorAnimation anim = FetchColorAnimation(context, startTime, endTime, s => s.ShadowColor, (s, e, c) => new ShadowColorAnimation(s, c, e, c));
            anim.EndColor = ColorUtil.ChangeColorAlpha(anim.EndColor, 255 - ParseHex(arg));
        }

        private static T FetchColorAnimation<T>(
            TagContext context,
            DateTime startTime,
            DateTime endTime,
            Func<ExtendedSection, Color> getSectionColor,
            Func<DateTime, DateTime, Color, T> createAnim
        )
            where T : ColorAnimation
        {
            ExtendedSection section = context.Section;
            T anim = section.Animations.OfType<T>().FirstOrDefault(a => a.StartTime == startTime && a.EndTime == endTime);
            if (anim == null)
            {
                T prevAnim = section.Animations.OfType<T>().LastOrDefault();
                anim = createAnim(startTime, endTime, prevAnim?.EndColor ?? getSectionColor(section));
                section.Animations.Add(anim);
            }

            return anim;
        }

        private static void ApplyStyle(ExtendedSection section, AssStyle style, AssStyleOptions options)
        {
            section.Font = style.Font;
            section.Bold = style.Bold;
            section.Italic = style.Italic;
            section.Underline = style.Underline;
            section.ForeColor = style.PrimaryColor;
            section.SecondaryColor = style.SecondaryColor;
            if (options?.IsKaraoke ?? false)
            {
                section.CurrentWordTextColor = options.CurrentWordTextColor;
                section.CurrentWordShadowColor = options.CurrentWordShadowColor;
            }
            else
            {
                section.CurrentWordTextColor = Color.Empty;
                section.CurrentWordShadowColor = Color.Empty;
            }

            section.BackColor = Color.Empty;
            section.ShadowColor = Color.Empty;
            section.ShadowTypes = ShadowType.None;

            if (style.HasOutline)
            {
                if (style.OutlineIsBox)
                {
                    section.BackColor = style.OutlineColor;
                }
                else
                {
                    section.ShadowColor = style.OutlineColor;
                    section.ShadowTypes = ShadowType.Glow;
                }
            }

            if (style.HasShadow && section.ShadowColor.IsEmpty)
            {
                section.ShadowColor = style.ShadowColor;
                section.ShadowTypes = options?.ShadowTypes ?? ShadowType.SoftShadow;
            }
        }

        private static IEnumerable<ExtendedLine> ExpandLine(ExtendedLine line)
        {
            return ExpandLineForKaraoke(line).SelectMany(Animator.Expand);
        }

        private static IEnumerable<ExtendedLine> ExpandLineForKaraoke(ExtendedLine line)
        {
            if (line.Sections.Cast<ExtendedSection>().All(s => s.Duration == TimeSpan.Zero))
            {
                yield return line;
                yield break;
            }

            SortedList<TimeSpan, int> visibleSectionsPerStep = GetKaraokeSteps(line);

            for (int stepIdx = 0; stepIdx < visibleSectionsPerStep.Count; stepIdx++)
            {
                TimeSpan timeOffset = visibleSectionsPerStep.Keys[stepIdx];
                int numVisibleSections = visibleSectionsPerStep.Values[stepIdx];

                DateTime start = TimeUtil.SnapTimeToFrame((line.Start + timeOffset).AddMilliseconds(20));
                if (start >= line.End)
                    continue;

                DateTime end;
                if (stepIdx < visibleSectionsPerStep.Count - 1)
                    end = TimeUtil.SnapTimeToFrame((line.Start + visibleSectionsPerStep.Keys[stepIdx + 1]).AddMilliseconds(20));
                else
                    end = line.End;

                ExtendedLine stepLine = (ExtendedLine)line.Clone();
                stepLine.Start = start;
                stepLine.End = end;

                foreach (ExtendedSection section in stepLine.Sections.Take(numVisibleSections))
                {
                    section.Animations.RemoveAll(a => a is SecondaryColorAnimation);
                }

                foreach (ExtendedSection section in stepLine.Sections.Skip(numVisibleSections))
                {
                    section.ForeColor = section.SecondaryColor;
                    if (section.ForeColor.A == 0 && !section.Animations.OfType<ForeColorAnimation>().Any())
                        section.ShadowTypes = ShadowType.None;

                    foreach (SecondaryColorAnimation anim in section.Animations.OfType<SecondaryColorAnimation>().ToList())
                    {
                        section.Animations.Remove(anim);
                        section.Animations.Add(new ForeColorAnimation(anim.StartTime, anim.StartColor, anim.EndTime, anim.EndColor));
                    }
                }

                ExtendedSection singingSection = (ExtendedSection)stepLine.Sections[numVisibleSections - 1];
                if (!singingSection.CurrentWordTextColor.IsEmpty)
                    singingSection.ForeColor = singingSection.CurrentWordTextColor;

                if (!singingSection.CurrentWordShadowColor.IsEmpty)
                    singingSection.ShadowColor = singingSection.CurrentWordShadowColor;

                // Hack: make sure YttDocument will also recognize the final (single-color) step as a karaoke line
                // so it gets the exact same position as the previous steps
                stepLine.Sections.Add(new ExtendedSection(string.Empty));

                yield return stepLine;
            }
        }

        private static SortedList<TimeSpan, int> GetKaraokeSteps(ExtendedLine line)
        {
            SortedList<TimeSpan, int> visibleSectionsPerStep = new SortedList<TimeSpan, int>();
            TimeSpan currentTimeOffset = TimeSpan.Zero;
            int currentVisibleSections = 0;
            foreach (ExtendedSection section in line.Sections)
            {
                currentVisibleSections++;
                if (section.Duration > TimeSpan.Zero)
                {
                    visibleSectionsPerStep[currentTimeOffset] = currentVisibleSections;
                    currentTimeOffset += section.Duration;
                }
                else
                {
                    TimeSpan prevTimeOffset = visibleSectionsPerStep.Count > 0 ? visibleSectionsPerStep.Keys.Last() : TimeSpan.Zero;
                    visibleSectionsPerStep[prevTimeOffset] = currentVisibleSections;
                }
            }
            return visibleSectionsPerStep;
        }

        private static void MergeIdenticallyFormattedSections(Line line)
        {
            SectionFormatComparer comparer = new SectionFormatComparer();
            int i = 0;
            while (i < line.Sections.Count - 1)
            {
                if (comparer.Equals(line.Sections[i], line.Sections[i + 1]))
                {
                    line.Sections[i].Text += line.Sections[i + 1].Text;
                    line.Sections.RemoveAt(i + 1);
                }
                else
                {
                    i++;
                }
            }
        }

        internal static AnchorPoint GetAnchorPointFromAlignment(int alignment)
        {
            switch (alignment)
            {
                case 1:
                    return AnchorPoint.BottomLeft;

                case 2:
                    return AnchorPoint.BottomCenter;

                case 3:
                    return AnchorPoint.BottomRight;

                case 4:
                    return AnchorPoint.MiddleLeft;

                case 5:
                    return AnchorPoint.Center;

                case 6:
                    return AnchorPoint.MiddleRight;

                case 7:
                    return AnchorPoint.TopLeft;

                case 8:
                    return AnchorPoint.TopCenter;

                case 9:
                    return AnchorPoint.TopRight;

                default:
                    throw new ArgumentException($"{alignment} is not a valid alignment");
            }
        }

        private static int ParseHex(string arg)
        {
            if (!arg.StartsWith("&H") || !arg.EndsWith("&"))
                return 0;

            int.TryParse(arg.Substring(2, arg.Length - 3), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out int value);
            return value;
        }

        private static Color ParseColor(string arg, int alpha)
        {
            int bgr = ParseHex(arg);
            byte r = (byte)bgr;
            byte g = (byte)(bgr >> 8);
            byte b = (byte)(bgr >> 16);
            return Color.FromArgb(alpha, r, g, b);
        }

        private static List<float> ParseNumberList(string arg)
        {
            Match match = Regex.Match(arg, @"^\s*\((?:\s*,?\s*([\d\.]+))+\s*\)\s*$");
            if (!match.Success)
                return null;

            List<float> list = new List<float>();
            foreach (Capture capture in match.Groups[1].Captures)
            {
                list.Add(float.Parse(capture.Value, CultureInfo.InvariantCulture));
            }
            return list;
        }

        private static List<string> ParseStringList(string arg)
        {
            Match match = Regex.Match(arg, @"^\s*\((?:\s*,?\s*([^,\(\)]+))+\)\s*$");
            if (!match.Success)
                return null;

            return match.Groups[1].Captures
                                  .Cast<Capture>()
                                  .Select(c => c.Value.Trim())
                                  .ToList();
        }

        private class TagContext
        {
            public AssDocument Document;
            public AssDialogue Dialogue;
            public AssStyle Style;
            public AssStyleOptions StyleOptions;
            public ExtendedLine Line;
            public ExtendedSection Section;

            public delegate List<ExtendedLine> PostProcessor();

            public readonly List<PostProcessor> PostProcessors = new List<PostProcessor>();
        }
    }
}
