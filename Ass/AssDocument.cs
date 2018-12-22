using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Arc.YTSubConverter.Util;

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
                              { "2c", HandleSecondaryColorTag },
                              { "3c", HandleOutlineColorTag },
                              { "4c", HandleShadowColorTag },
                              { "1a", HandleForeColorAlphaTag },
                              { "2a", HandleSecondaryColorAlphaTag },
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

                ExtendedLine line = ParseLine(dialogue, style, options);
                Lines.AddRange(ExpandLine(line));
            }

            foreach (Line line in Lines)
            {
                MergeIdenticallyFormattedSections(line);
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
            DateTime endTime = SnapTimeToFrame(dialogue.End).AddMilliseconds(33);
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
                    context.Section.Duration = TimeSpan.Zero;
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

            return line;
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

        private static void HandleSecondaryColorTag(TagContext context, string arg)
        {
            context.Section.SecondaryColor = ParseColor(arg, context.Section.SecondaryColor.A);
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
            context.Section.ForeColor = ColorUtil.ChangeColorAlpha(context.Section.ForeColor, alpha);
        }

        private static void HandleSecondaryColorAlphaTag(TagContext context, string arg)
        {
            int alpha = 255 - ParseHex(arg);
            context.Section.SecondaryColor = ColorUtil.ChangeColorAlpha(context.Section.SecondaryColor, alpha);
        }

        private static void HandleOutlineAlphaTag(TagContext context, string arg)
        {
            if (!context.Style.HasOutline)
                return;

            int alpha = 255 - ParseHex(arg);

            if (context.Style.OutlineIsBox)
                context.Section.BackColor = ColorUtil.ChangeColorAlpha(context.Section.BackColor, alpha);
            else
                context.Section.ShadowColor = ColorUtil.ChangeColorAlpha(context.Section.ShadowColor, alpha);
        }

        private static void HandleShadowAlphaTag(TagContext context, string arg)
        {
            if (!context.Style.HasShadow)
                return;

            int alpha = 255 - ParseHex(arg);
            context.Section.ShadowColor = ColorUtil.ChangeColorAlpha(context.Section.ShadowColor, alpha);
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

            line.FadeInitialAlpha = 255 - (int)args[0];
            line.FadeMidAlpha = 255 - (int)args[1];
            line.FadeFinalAlpha = 255 - (int)args[2];

            line.FadeInStartTime = line.Start.AddMilliseconds(args[3]);
            line.FadeInEndTime = line.Start.AddMilliseconds(args[4]);
            line.FadeOutStartTime = line.Start.AddMilliseconds(args[5]);
            line.FadeOutEndTime = line.Start.AddMilliseconds(args[6]);
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

            if (style.HasShadow && section.ShadowColor.IsEmpty)
            {
                section.ShadowColor = style.ShadowColor;
                section.ShadowType = options?.ShadowType ?? ShadowType.SoftShadow;
            }
        }

        private static IEnumerable<ExtendedLine> ExpandLine(ExtendedLine line)
        {
            return ExpandLineForKaraoke(line).SelectMany(ExpandLineForFade);
        }

        private static IEnumerable<ExtendedLine> ExpandLineForKaraoke(ExtendedLine line)
        {
            if (((ExtendedSection)line.Sections[0]).Duration == TimeSpan.Zero)
            {
                yield return line;
                yield break;
            }

            SortedList<TimeSpan, int> visibleSectionsPerStep = GetKaraokeSteps(line);

            for (int stepIdx = 0; stepIdx < visibleSectionsPerStep.Count; stepIdx++)
            {
                TimeSpan timeOffset = visibleSectionsPerStep.Keys[stepIdx];
                int numVisibleSections = visibleSectionsPerStep.Values[stepIdx];

                ExtendedLine stepLine = (ExtendedLine)line.Clone();
                ChangeLineTimes(stepLine, line.Start + timeOffset, stepIdx < visibleSectionsPerStep.Count - 1 ? line.Start + visibleSectionsPerStep.Keys[stepIdx + 1] : line.End);
                for (int i = numVisibleSections; i < stepLine.Sections.Count; i++)
                {
                    stepLine.Sections[i].ForeColor = ((ExtendedSection)stepLine.Sections[i]).SecondaryColor;
                }

                ExtendedSection singingSection = (ExtendedSection)stepLine.Sections[numVisibleSections - 1];
                if (!singingSection.CurrentWordTextColor.IsEmpty)
                    singingSection.ForeColor = singingSection.CurrentWordTextColor;

                if (!singingSection.CurrentWordShadowColor.IsEmpty)
                    singingSection.ShadowColor = singingSection.CurrentWordShadowColor;

                // Hack: make sure YttDocument will also recognize the final (single-color) step as a karaoke line
                // so it gets the exact same position as the previous steps
                if (stepIdx == visibleSectionsPerStep.Count - 1)
                    stepLine.Sections.Add(new Section(string.Empty));

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
                    visibleSectionsPerStep.Add(currentTimeOffset, currentVisibleSections);
                    currentTimeOffset += section.Duration;
                }
                else
                {
                    visibleSectionsPerStep[visibleSectionsPerStep.Keys.Last()] = currentVisibleSections;
                }
            }
            return visibleSectionsPerStep;
        }

        private static void ChangeLineTimes(ExtendedLine line, DateTime start, DateTime end)
        {
            if (line.UseFade)
            {
                TimeSpan offset = start - line.Start;
                line.FadeInStartTime -= offset;
                line.FadeInEndTime -= offset;
                line.FadeOutStartTime -= offset;
                line.FadeOutEndTime -= offset;

                if (line.FadeInStartTime < line.Start)
                {
                    if (line.FadeInEndTime > line.Start)
                    {
                        double fadeInProgress = (line.Start - line.FadeInStartTime).TotalMilliseconds / (line.FadeInEndTime - line.FadeInStartTime).TotalMilliseconds;
                        line.FadeInitialAlpha += (int)((line.FadeMidAlpha - line.FadeInitialAlpha) * fadeInProgress);
                        line.FadeInStartTime = line.Start;
                    }
                    else
                    {
                        line.FadeInStartTime = line.Start;
                        line.FadeInEndTime = line.Start;
                    }
                }

                if (line.FadeOutEndTime > line.End)
                {
                    if (line.FadeOutStartTime < line.End)
                    {
                        double fadeOutProgress = (line.End - line.FadeOutStartTime).TotalMilliseconds / (line.FadeOutEndTime - line.FadeOutStartTime).TotalMilliseconds;
                        line.FadeFinalAlpha += (int)((line.FadeFinalAlpha - line.FadeMidAlpha) * fadeOutProgress);
                        line.FadeOutEndTime = line.End;
                    }
                    else
                    {
                        line.FadeOutStartTime = line.End;
                        line.FadeOutEndTime = line.End;
                    }
                }
            }

            line.Start = start;
            line.End = end;
        }

        private static IEnumerable<ExtendedLine> ExpandLineForFade(ExtendedLine line)
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
