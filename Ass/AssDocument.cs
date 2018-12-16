using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Arc.YTSubConverter.Ass
{
    internal partial class AssDocument : SubtitleDocument
    {
        private static readonly Dictionary<string, Action<TagContext, string>> TagHandlers;

        static AssDocument()
        {
            TagHandlers = new Dictionary<string, Action<TagContext, string>>
                          {
                              { "b", HandleBoldTag },
                              { "i", HandleItalicTag },
                              { "u", HandleUnderlineTag },
                              { "fn", HandleFontTag },
                              { "c", HandleForeColorTag },
                              { "1c", HandleForeColorTag },
                              { "3c", HandleOutlineColorTag },
                              { "4c", HandleShadowColorTag },
                              { "1a", HandleForeColorAlphaTag },
                              { "3a", HandleOutlineAlphaTag },
                              { "4a", HandleShadowAlphaTag },
                              { "pos", HandlePositionTag },
                              { "k", HandleDurationTag },
                              { "r", HandleResetTag },
                              { "fad", HandleSimpleFadeTag },
                              { "fade", HandleComplexFadeTag }
                          };
        }

        public AssDocument(string filePath, List<AssStyleOptions> styleOptions = null)
        {
            Dictionary<string, AssSection> fileSections = ReadDocument(filePath);
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

                ExtendedLine line = ParseLine(dialogue, style, options);
                Lines.AddRange(ExpandLine(line));
            }
        }

        public List<AssStyle> Styles
        {
            get;
        }

        private static Dictionary<string, AssSection> ReadDocument(string filePath)
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

                    Match match = Regex.Match(line, @"(\w+):(.+)");
                    if (!match.Success)
                        throw new InvalidDataException($"Unrecognized line in .ass: {line}");

                    if (currentSection == null)
                        throw new InvalidDataException($"Line {line} is not inside a section");

                    string type = match.Groups[1].Value;
                    List<string> values = match.Groups[2].Value.Split(",", currentSection.Format?.Count, s => s.Trim());
                    if (type == "Format")
                    {
                        if (currentSection.Format != null)
                            throw new InvalidDataException("Section has multiple Format items");

                        currentSection.SetFormat(values);
                    }
                    else
                    {
                        currentSection.AddItem(type, values);
                    }
                }
            }

            return sections;
        }

        private static ExtendedLine ParseLine(AssDialogue dialogue, AssStyle style, AssStyleOptions styleOptions)
        {
            DateTime startTime = SnapTimeToFrame(dialogue.Start.AddMilliseconds(33));
            DateTime endTime = SnapTimeToFrame(dialogue.End);
            ExtendedLine line = new ExtendedLine(startTime, endTime) { AnchorPoint = style.AnchorPoint };

            TagContext context = new TagContext
                                 {
                                     Dialogue = dialogue,
                                     Style = style,
                                     StyleOptions = styleOptions,
                                     Line = line,
                                     Section = new ExtendedSection(null)
                                 };

            ApplyStyle(context.Section, style, styleOptions);
            
            string text = Regex.Replace(dialogue.Text, @"(?:\\N)+$", "");
            int start = 0;
            foreach (Match match in Regex.Matches(text, @"\{(?:\\(?<cmd>fn|\d?[a-z]+)(?<arg>.*?))+\}"))
            {
                int end = match.Index;

                if (end > start)
                {
                    context.Section.Text = text.Substring(start, end - start).Replace("\\N", "\r\n");
                    line.Sections.Add(context.Section);

                    context.Section = (ExtendedSection)context.Section.Clone();
                    context.Section.Text = null;
                }

                CaptureCollection commands = match.Groups["cmd"].Captures;
                CaptureCollection arguments = match.Groups["arg"].Captures;
                for (int i = 0; i < commands.Count; i++)
                {
                    if (TagHandlers.TryGetValue(commands[i].Value, out var handler))
                        handler(context, arguments[i].Value);
                }

                start = match.Index + match.Length;
            }

            if (start < text.Length)
            {
                context.Section.Text = text.Substring(start, text.Length - start).Replace("\\N", "\r\n");
                line.Sections.Add(context.Section);
            }

            SetSectionTimeOffsets(line);
            return line;
        }

        private static void SetSectionTimeOffsets(Line line)
        {
            TimeSpan offset = TimeSpan.Zero;
            foreach (ExtendedSection section in line.Sections.Cast<ExtendedSection>())
            {
                section.TimeOffset = offset;
                offset += section.Duration;
            }
        }

        private static IEnumerable<Line> ExpandLine(ExtendedLine line)
        {
            if (!line.UseFade)
            {
                yield return line;
                yield break;
            }

            int startFrame = TimeToFrame(line.Start.AddMilliseconds(2));
            int fadeInStartFrame = TimeToFrame(line.FadeInStartTime);
            int fadeInEndFrame = TimeToFrame(line.FadeInEndTime);
            int fadeOutStartFrame = TimeToFrame(line.FadeOutStartTime);
            int fadeOutEndFrame = TimeToFrame(line.FadeOutEndTime);
            int endFrame = TimeToFrame(line.End);

            if (startFrame < fadeInStartFrame)
                yield return CreateAlphaBlendedLine(line, startFrame, fadeInStartFrame, line.FadeInitialAlpha);
            else
                fadeInStartFrame = startFrame;

            foreach (ExtendedLine fadeLine in CreateAlphaBlendedLines(line, fadeInStartFrame, fadeInEndFrame, line.FadeInitialAlpha, line.FadeMidAlpha))
            {
                yield return fadeLine;
            }

            if (fadeInEndFrame < fadeOutStartFrame)
                yield return CreateAlphaBlendedLine(line, fadeInEndFrame, fadeOutStartFrame, line.FadeMidAlpha);
            else
                fadeOutStartFrame = fadeInEndFrame;

            foreach (ExtendedLine fadeLine in CreateAlphaBlendedLines(line, fadeOutStartFrame, fadeOutEndFrame, line.FadeMidAlpha, line.FadeFinalAlpha))
            {
                yield return fadeLine;
            }

            if (fadeOutEndFrame < endFrame)
                yield return CreateAlphaBlendedLine(line, fadeOutEndFrame, endFrame, line.FadeFinalAlpha);
        }

        private static IEnumerable<ExtendedLine> CreateAlphaBlendedLines(ExtendedLine baseLine, int startFrame, int endFrame, int startAlpha, int endAlpha)
        {
            const int frameStepSize = 2;
            int lastIterationFrame = startFrame + (endFrame - 1 - startFrame) / frameStepSize * frameStepSize;
            for (int frame = startFrame; frame <= lastIterationFrame; frame += frameStepSize)
            {
                float t = ((float)frame - startFrame) / (lastIterationFrame - startFrame);
                int alpha = startAlpha + (int)(t * (endAlpha - startAlpha));
                yield return CreateAlphaBlendedLine(baseLine, frame, Math.Min(frame + frameStepSize, endFrame), alpha);
            }
        }

        private static ExtendedLine CreateAlphaBlendedLine(ExtendedLine baseLine, int startFrame, int endFrame, int alpha)
        {
            ExtendedLine newLine = (ExtendedLine)baseLine.Clone();
            newLine.Start = FrameToTime(startFrame);
            newLine.End = FrameToTime(endFrame);
            newLine.MultiplySectionAlphas(alpha / 255F);
            return newLine;
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
        }

        private static void HandleOutlineColorTag(TagContext context, string arg)
        {
            if (!context.Style.HasOutline)
                return;

            if (context.Style.OutlineIsBox)
                context.Section.BackColor = ParseColor(arg, context.Section.BackColor.A);
            else
                context.Section.ShadowColor = ParseColor(arg, context.Section.ShadowColor.A);
        }

        private static void HandleShadowColorTag(TagContext context, string arg)
        {
            if (!context.Style.HasShadow)
                return;

            context.Section.ShadowColor = ParseColor(arg, context.Section.ShadowColor.A);
        }

        private static void HandleForeColorAlphaTag(TagContext context, string arg)
        {
            int alpha = 255 - ParseHex(arg);
            context.Section.ForeColor = ChangeColorAlpha(context.Section.ForeColor, alpha);
        }

        private static void HandleOutlineAlphaTag(TagContext context, string arg)
        {
            if (!context.Style.HasOutline)
                return;

            int alpha = 255 - ParseHex(arg);

            if (context.Style.OutlineIsBox)
                context.Section.BackColor = ChangeColorAlpha(context.Section.BackColor, alpha);
            else
                context.Section.ShadowColor = ChangeColorAlpha(context.Section.ShadowColor, alpha);
        }

        private static void HandleShadowAlphaTag(TagContext context, string arg)
        {
            if (!context.Style.HasShadow)
                return;

            int alpha = 255 - ParseHex(arg);
            context.Section.ShadowColor = ChangeColorAlpha(context.Section.ShadowColor, alpha);
        }

        private static void HandlePositionTag(TagContext context, string arg)
        {
            List<float> coords = ParseNumberList(arg);
            if (coords == null || coords.Count != 2)
                return;

            context.Line.Position = new PointF(coords[0], coords[1]);
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
            line.UseFade = true;

            line.FadeInitialAlpha = 0;
            line.FadeMidAlpha = 255;
            line.FadeFinalAlpha = 0;

            line.FadeInStartTime = line.Start;
            line.FadeInEndTime = line.Start.AddMilliseconds(times[0]);
            line.FadeOutStartTime = line.End.AddMilliseconds(-times[1]);
            line.FadeOutEndTime = line.End;
        }

        private static void HandleComplexFadeTag(TagContext context, string arg)
        {
            List<float> args = ParseNumberList(arg);
            if (args == null || args.Count != 7)
                return;

            ExtendedLine line = context.Line;
            line.UseFade = true;

            line.FadeInitialAlpha = (int)args[0];
            line.FadeMidAlpha = (int)args[1];
            line.FadeFinalAlpha = (int)args[2];

            line.FadeInStartTime = line.Start.AddMilliseconds(args[3]);
            line.FadeInEndTime = line.Start.AddMilliseconds(args[4]);
            line.FadeOutStartTime = line.Start.AddMilliseconds(args[5]);
            line.FadeOutEndTime = line.Start.AddMilliseconds(args[6]);
        }

        private static void ApplyStyle(Section section, AssStyle style, AssStyleOptions options)
        {
            section.Font = style.Font;
            section.Bold = style.Bold;
            section.Italic = style.Italic;
            section.Underline = style.Underline;
            section.ForeColor = style.PrimaryColor;

            section.BackColor = Color.Empty;
            section.ShadowColor = Color.Empty;
            section.ShadowType = ShadowType.None;

            if (style.HasOutline)
            {
                if (style.OutlineIsBox)
                {
                    section.BackColor = style.OutlineColor;
                }
                else
                {
                    section.ShadowColor = style.OutlineColor;
                    section.ShadowType = ShadowType.Glow;
                }
            }

            if (style.HasShadow && section.ShadowColor == Color.Empty)
            {
                section.ShadowColor = style.ShadowColor;
                section.ShadowType = options?.ShadowType ?? ShadowType.SoftShadow;
            }
        }

        private static int ParseHex(string arg)
        {
            return int.Parse(arg.Substring(2, arg.Length - 3), NumberStyles.AllowHexSpecifier);
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

        private static Color ChangeColorAlpha(Color color, int alpha)
        {
            return Color.FromArgb(alpha, color.R, color.G, color.B);
        }

        private static int TimeToFrame(DateTime time)
        {
            return (int)(time.TimeOfDay.TotalMilliseconds / 33.36666666666667);
        }

        private static DateTime FrameToTime(int frame)
        {
            if (frame == 0)
                return TimeBase;

            int ms = (int)(frame * 33.36666666666667);
            return TimeBase + TimeSpan.FromMilliseconds(ms);
        }

        private static DateTime SnapTimeToFrame(DateTime time)
        {
            int frame = TimeToFrame(time);
            return FrameToTime(frame);
        }

        private struct TagContext
        {
            public AssDialogue Dialogue;
            public AssStyle Style;
            public AssStyleOptions StyleOptions;
            public ExtendedLine Line;
            public ExtendedSection Section;
        }
    }
}
