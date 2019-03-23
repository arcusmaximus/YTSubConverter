using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Arc.YTSubConverter.Animations;
using Arc.YTSubConverter.Formats.Ass.Tags;
using Arc.YTSubConverter.Util;

namespace Arc.YTSubConverter.Formats.Ass
{
    internal class AssDocument : SubtitleDocument
    {
        private readonly Dictionary<string, AssTagHandlerBase> _tagHandlers = new Dictionary<string, AssTagHandlerBase>();

        public AssDocument(string filePath, List<AssStyleOptions> styleOptions = null)
        {
            RegisterTagHandlers();

            Dictionary<string, AssDocumentSection> fileSections = ReadDocument(filePath);

            AssDocumentSection infoSection = fileSections["Script Info"];
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

                List<AssLine> lines = ParseLine(dialogue, style, options);
                Lines.AddRange(lines.SelectMany(ExpandLine));
            }

            foreach (AssLine line in Lines)
            {
                MergeIdenticallyFormattedSections(line);
                line.NormalizeAlpha();
            }
        }

        public List<AssStyle> Styles
        {
            get;
        }

        private void RegisterTagHandlers()
        {
            RegisterTagHandler(new AssAlignmentTagHandler());
            RegisterTagHandler(new AssBoldTagHandler());
            RegisterTagHandler(new AssChromaTagHandler());
            RegisterTagHandler(new AssComplexFadeTagHandler());
            RegisterTagHandler(new AssFontTagHandler());
            RegisterTagHandler(new AssForeAlphaTagHandler());
            RegisterTagHandler(new AssForeColorTagHandler("c"));
            RegisterTagHandler(new AssForeColorTagHandler("1c"));
            RegisterTagHandler(new AssItalicTagHandler());
            RegisterTagHandler(new AssKaraokeTagHandler());
            RegisterTagHandler(new AssKaraokeTypeTagHandler());
            RegisterTagHandler(new AssMoveTagHandler());
            RegisterTagHandler(new AssOutlineAlphaTagHandler());
            RegisterTagHandler(new AssOutlineColorTagHandler());
            RegisterTagHandler(new AssPositionTagHandler());
            RegisterTagHandler(new AssResetTagHandler());
            RegisterTagHandler(new AssSecondaryAlphaTagHandler());
            RegisterTagHandler(new AssSecondaryColorTagHandler());
            RegisterTagHandler(new AssShadowAlphaTagHandler());
            RegisterTagHandler(new AssShadowColorTagHandler());
            RegisterTagHandler(new AssShakeTagHandler());
            RegisterTagHandler(new AssSimpleFadeTagHandler());
            RegisterTagHandler(new AssTransformTagHandler());
            RegisterTagHandler(new AssUnderlineTagHandler());
        }

        private void RegisterTagHandler(AssTagHandlerBase handler)
        {
            _tagHandlers.Add(handler.Tag, handler);
        }

        private Dictionary<string, AssDocumentSection> ReadDocument(string filePath)
        {
            Dictionary<string, AssDocumentSection> sections = new Dictionary<string, AssDocumentSection>();
            AssDocumentSection currentSection = null;

            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith(";"))
                        continue;

                    if (line.StartsWith("[") && line.EndsWith("]"))
                    {
                        currentSection = new AssDocumentSection();
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

        private List<AssLine> ParseLine(AssDialogue dialogue, AssStyle style, AssStyleOptions styleOptions)
        {
            DateTime startTime = TimeUtil.SnapTimeToFrame(dialogue.Start.AddMilliseconds(32));
            DateTime endTime = TimeUtil.SnapTimeToFrame(dialogue.End).AddMilliseconds(32);
            AssLine line = new AssLine(startTime, endTime) { AnchorPoint = style.AnchorPoint };

            AssTagContext context = new AssTagContext
                                 {
                                     Document = this,
                                     Dialogue = dialogue,
                                     Style = style,
                                     StyleOptions = styleOptions,
                                     Line = line,
                                     Section = new AssSection(null)
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

                    context.Section = (AssSection)context.Section.Clone();
                    context.Section.Text = null;
                    context.Section.Duration = TimeSpan.Zero;
                }

                CaptureCollection tags = match.Groups["tag"].Captures;
                CaptureCollection arguments = match.Groups["arg"].Captures;
                for (int i = 0; i < tags.Count; i++)
                {
                    if (_tagHandlers.TryGetValue(tags[i].Value, out AssTagHandlerBase handler))
                        handler.Handle(context, arguments[i].Value);
                }

                start = match.Index + match.Length;
            }

            if (start < text.Length)
            {
                context.Section.Text = text.Substring(start, text.Length - start).Replace("\\N", "\r\n");
                line.Sections.Add(context.Section);
            }

            List<AssLine> lines = new List<AssLine> { line };
            foreach (AssTagContext.PostProcessor postProcessor in context.PostProcessors)
            {
                List<AssLine> extraLines = postProcessor();
                if (extraLines != null)
                    lines.AddRange(extraLines);
            }

            return lines;
        }

        internal static void ApplyStyle(AssSection section, AssStyle style, AssStyleOptions options)
        {
            section.Font = style.Font;
            section.Bold = style.Bold;
            section.Italic = style.Italic;
            section.Underline = style.Underline;
            section.ForeColor = style.PrimaryColor;
            section.SecondaryColor = style.SecondaryColor;
            if (options?.IsKaraoke ?? false)
            {
                section.CurrentWordForeColor = options.CurrentWordTextColor;
                section.CurrentWordOutlineColor = options.CurrentWordOutlineColor;
                section.CurrentWordShadowColor = options.CurrentWordShadowColor;
            }
            else
            {
                section.CurrentWordForeColor = Color.Empty;
                section.CurrentWordOutlineColor = Color.Empty;
                section.CurrentWordShadowColor = Color.Empty;
            }

            section.BackColor = Color.Empty;
            section.ShadowColors.Clear();

            if (style.HasShadow)
            {
                foreach (ShadowType shadowType in options?.ShadowTypes ?? new List<ShadowType> { ShadowType.SoftShadow })
                {
                    section.ShadowColors[shadowType] = style.ShadowColor;
                }
            }

            if (style.HasOutline)
            {
                if (style.OutlineIsBox)
                    section.BackColor = style.OutlineColor;
                else
                    section.ShadowColors[ShadowType.Glow] = style.OutlineColor;
            }
        }

        private static IEnumerable<AssLine> ExpandLine(AssLine line)
        {
            return ExpandLineForKaraoke(line).SelectMany(Animator.Expand);
        }

        private static IEnumerable<AssLine> ExpandLineForKaraoke(AssLine line)
        {
            if (line.Sections.Cast<AssSection>().All(s => s.Duration == TimeSpan.Zero))
            {
                yield return line;
                yield break;
            }

            SortedList<TimeSpan, int> activeSectionsPerStep = GetKaraokeSteps(line);
            for (int stepIdx = 0; stepIdx < activeSectionsPerStep.Count; stepIdx++)
            {
                yield return CreateKaraokeStepLine(line, activeSectionsPerStep, stepIdx);
            }
        }

        private static SortedList<TimeSpan, int> GetKaraokeSteps(AssLine line)
        {
            SortedList<TimeSpan, int> activeSectionsPerStep = new SortedList<TimeSpan, int>();
            TimeSpan timeOffset = TimeSpan.Zero;
            int numActiveSections = 0;
            foreach (AssSection section in line.Sections)
            {
                numActiveSections++;
                if (section.Duration > TimeSpan.Zero)
                {
                    activeSectionsPerStep[timeOffset] = numActiveSections;
                    timeOffset += section.Duration;
                }
                else
                {
                    TimeSpan prevTimeOffset = activeSectionsPerStep.Count > 0 ? activeSectionsPerStep.Keys.Last() : TimeSpan.Zero;
                    activeSectionsPerStep[prevTimeOffset] = numActiveSections;
                }
            }
            return activeSectionsPerStep;
        }

        private static AssLine CreateKaraokeStepLine(AssLine originalLine, SortedList<TimeSpan, int> activeSectionsPerStep, int stepIdx)
        {
            TimeSpan timeOffset = activeSectionsPerStep.Keys[stepIdx];
            int numActiveSections = activeSectionsPerStep.Values[stepIdx];

            DateTime startTime = TimeUtil.SnapTimeToFrame((originalLine.Start + timeOffset).AddMilliseconds(20));
            if (startTime >= originalLine.End)
                return null;

            DateTime endTime;
            if (stepIdx < activeSectionsPerStep.Count - 1)
                endTime = TimeUtil.SnapTimeToFrame((originalLine.Start + activeSectionsPerStep.Keys[stepIdx + 1]).AddMilliseconds(20)).AddMilliseconds(-1);
            else
                endTime = originalLine.End;

            AssLine stepLine = (AssLine)originalLine.Clone();
            stepLine.Start = startTime;
            stepLine.End = endTime;

            foreach (AssSection section in stepLine.Sections.Take(numActiveSections))
            {
                section.Animations.RemoveAll(a => a is SecondaryColorAnimation);
            }

            foreach (AssSection section in stepLine.Sections.Skip(numActiveSections))
            {
                section.ForeColor = section.SecondaryColor;

                section.Animations.RemoveAll(a => a is ForeColorAnimation);
                foreach (SecondaryColorAnimation anim in section.Animations.OfType<SecondaryColorAnimation>().ToList())
                {
                    section.Animations.Remove(anim);
                    section.Animations.Add(new ForeColorAnimation(anim.StartTime, anim.StartColor, anim.EndTime, anim.EndColor));
                }

                if (section.ForeColor.A == 0 && !section.Animations.OfType<ForeColorAnimation>().Any())
                    section.ShadowColors.Clear();
            }

            // Hack: make sure YttDocument will also recognize the final (single-color) step as a karaoke line
            // so it gets the exact same position as the previous steps
            stepLine.Sections.Add(new AssSection(string.Empty));

            AddKaraokeEffects(originalLine, stepLine, activeSectionsPerStep, stepIdx);
            return stepLine;
        }

        private static void AddKaraokeEffects(AssLine originalLine, AssLine stepLine, SortedList<TimeSpan, int> activeSectionsPerStep, int stepIdx)
        {
            int numActiveSections = activeSectionsPerStep.Values[stepIdx];
            AssSection singingSection = (AssSection)stepLine.Sections[numActiveSections - 1];

            switch (stepLine.KaraokeType)
            {
                case KaraokeType.Simple:
                    ApplySimpleKaraokeEffect(singingSection);
                    break;

                case KaraokeType.Fade:
                    ApplyFadeKaraokeEffect(originalLine, stepLine, activeSectionsPerStep, stepIdx);
                    break;

                case KaraokeType.Glitch:
                    ApplyGlitchKaraokeEffect(stepLine, singingSection);
                    break;
            }
        }

        private static void ApplySimpleKaraokeEffect(AssSection singingSection)
        {
            if (!singingSection.CurrentWordForeColor.IsEmpty)
                singingSection.ForeColor = singingSection.CurrentWordForeColor;

            if (!singingSection.CurrentWordShadowColor.IsEmpty)
            {
                foreach (ShadowType shadowType in singingSection.ShadowColors.Keys.ToList())
                {
                    singingSection.ShadowColors[shadowType] = singingSection.CurrentWordShadowColor;
                }
            }

            if (!singingSection.CurrentWordOutlineColor.IsEmpty && singingSection.ShadowColors.ContainsKey(ShadowType.Glow))
                singingSection.ShadowColors[ShadowType.Glow] = singingSection.CurrentWordOutlineColor;
        }

        private static void ApplyFadeKaraokeEffect(AssLine originalLine, AssLine stepLine, SortedList<TimeSpan, int> activeSectionsPerStep, int stepIdx)
        {
            int numActiveSections = activeSectionsPerStep.Values[stepIdx];
            AssSection singingSection = (AssSection)stepLine.Sections[numActiveSections - 1];
            ApplyFadeInKaraokeEffect(stepLine, singingSection);
            ApplyFadeOutKaraokeEffect(originalLine, stepLine, activeSectionsPerStep, stepIdx);
        }

        private static void ApplyFadeInKaraokeEffect(AssLine stepLine, AssSection singingSection)
        {
            DateTime fadeEndTime = TimeUtil.Min(stepLine.Start.AddMilliseconds(500), stepLine.End);

            if (singingSection.CurrentWordForeColor.IsEmpty)
            {
                if (singingSection.ForeColor != singingSection.SecondaryColor)
                    singingSection.Animations.Add(new ForeColorAnimation(stepLine.Start, singingSection.SecondaryColor, fadeEndTime, singingSection.ForeColor));
            }
            else
            {
                if (singingSection.CurrentWordForeColor != singingSection.SecondaryColor)
                    singingSection.Animations.Add(new ForeColorAnimation(stepLine.Start, singingSection.SecondaryColor, fadeEndTime, singingSection.CurrentWordForeColor));
            }

            if (!singingSection.CurrentWordShadowColor.IsEmpty)
            {
                foreach (KeyValuePair<ShadowType, Color> shadowColor in singingSection.ShadowColors)
                {
                    if (singingSection.CurrentWordShadowColor != shadowColor.Value)
                        singingSection.Animations.Add(new ShadowColorAnimation(shadowColor.Key, stepLine.Start, shadowColor.Value, fadeEndTime, singingSection.CurrentWordShadowColor));
                }
            }

            if (!singingSection.CurrentWordOutlineColor.IsEmpty && singingSection.CurrentWordOutlineColor != singingSection.ShadowColors.GetOrDefault(ShadowType.Glow))
            {
                singingSection.Animations.Add(new ShadowColorAnimation(
                    ShadowType.Glow, stepLine.Start, singingSection.ShadowColors[ShadowType.Glow], fadeEndTime, singingSection.CurrentWordOutlineColor));
            }
        }

        private static void ApplyFadeOutKaraokeEffect(AssLine originalLine, AssLine stepLine, SortedList<TimeSpan, int> activeSectionsPerStep, int stepIdx)
        {
            int stepFirstSectionIdx = 0;
            for (int prevStepIdx = 0; prevStepIdx < stepIdx; prevStepIdx++)
            {
                DateTime fadeStartTime = TimeUtil.SnapTimeToFrame((originalLine.Start + activeSectionsPerStep.Keys[prevStepIdx + 1]).AddMilliseconds(20));
                DateTime fadeEndTime = fadeStartTime.AddMilliseconds(1000);
                int stepLastSectionIdx = activeSectionsPerStep.Values[prevStepIdx] - 1;
                for (int sectionIdx = stepFirstSectionIdx; sectionIdx <= stepLastSectionIdx; sectionIdx++)
                {
                    AssSection section = (AssSection)stepLine.Sections[sectionIdx];
                    if (!section.CurrentWordForeColor.IsEmpty && section.CurrentWordForeColor != section.ForeColor)
                        section.Animations.Add(new ForeColorAnimation(fadeStartTime, section.CurrentWordForeColor, fadeEndTime, section.ForeColor));

                    if (!section.CurrentWordShadowColor.IsEmpty)
                    {
                        foreach (KeyValuePair<ShadowType, Color> shadowColor in section.ShadowColors)
                        {
                            if(section.CurrentWordShadowColor != shadowColor.Value)
                                section.Animations.Add(new ShadowColorAnimation(shadowColor.Key, fadeStartTime, section.CurrentWordShadowColor, fadeEndTime, shadowColor.Value));
                        }
                    }

                    if (!section.CurrentWordOutlineColor.IsEmpty && section.CurrentWordOutlineColor != section.ShadowColors.GetOrDefault(ShadowType.Glow))
                        section.Animations.Add(new ShadowColorAnimation(ShadowType.Glow, fadeStartTime, section.CurrentWordOutlineColor, fadeEndTime, section.ShadowColors[ShadowType.Glow]));
                }

                stepFirstSectionIdx = stepLastSectionIdx + 1;
            }
        }

        private static void ApplyGlitchKaraokeEffect(AssLine stepLine, AssSection singingSection)
        {
            if (singingSection.Text.Length == 0)
                return;

            DateTime glitchEndTime = TimeUtil.Min(stepLine.Start.AddMilliseconds(70), stepLine.End);
            Util.CharacterRange[] charRanges = GetGlitchKaraokeCharacterRanges(singingSection.Text[0]);
            singingSection.Animations.Add(new GlitchingCharAnimation(stepLine.Start, glitchEndTime, charRanges));
        }

        private static Util.CharacterRange[] GetGlitchKaraokeCharacterRanges(char c)
        {
            Util.CharacterRange[][] availableRanges =
                {
                    new[] { new Util.CharacterRange('A', 'Z'), new Util.CharacterRange('a', 'z') },
                    new[] { TextUtil.IdeographRange, TextUtil.IdeographExtensionRange, TextUtil.IdeographCompatibilityRange },
                    new[] { TextUtil.HiraganaRange },
                    new[] { TextUtil.KatakanaRange },
                    new[] { TextUtil.HangulRange }
                };

            foreach (Util.CharacterRange[] ranges in availableRanges)
            {
                if (ranges.Any(r => r.Contains(c)))
                    return ranges;
            }

            return new[]
                   {
                       new Util.CharacterRange('\x2300', '\x231A'),
                       new Util.CharacterRange('\x231C', '\x23E1')
                   };
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
    }
}
