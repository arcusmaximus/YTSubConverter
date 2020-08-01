using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Arc.YTSubConverter.Animations;
using Arc.YTSubConverter.Formats.Ass.KaraokeTypes;
using Arc.YTSubConverter.Formats.Ass.Tags;
using Arc.YTSubConverter.Util;

namespace Arc.YTSubConverter.Formats.Ass
{
    internal class AssDocument : SubtitleDocument
    {
        private static class EffectNames
        {
            public const string NoAndroidDarkTextHack = "no_android_dark_text_hack";
        }

        private readonly Dictionary<string, AssTagHandlerBase> _tagHandlers = new Dictionary<string, AssTagHandlerBase>();
        private readonly Dictionary<string, AssStyle> _styles;
        private readonly Dictionary<string, AssStyleOptions> _styleOptions;

        public AssDocument(string filePath, List<AssStyleOptions> styleOptions = null)
            : this(File.OpenRead(filePath), styleOptions)
        {
        }

        public AssDocument(SubtitleDocument doc)
            : this(new MemoryStream(Resources.DefaultStyles), AssStyleOptionsList.LoadFromString(Resources.DefaultStyleOptions))
        {
            float sizeFactor = (float)doc.VideoDimensions.Height / VideoDimensions.Height;
            foreach (AssStyle style in _styles.Values)
            {
                style.LineHeight *= sizeFactor;
            }

            VideoDimensions = doc.VideoDimensions;

            Lines.Clear();
            Lines.AddRange(doc.Lines.Select(l => l as AssLine ?? new AssLine(l)));
        }

        private AssDocument(Stream stream, List<AssStyleOptions> styleOptions)
        {
            RegisterTagHandlers();

            Dictionary<string, AssDocumentSection> fileSections = ReadDocument(stream);

            AssDocumentSection infoSection = fileSections["Script Info"];
            VideoDimensions = new Size(infoSection.GetItemInt("PlayResX", 384), infoSection.GetItemInt("PlayResY", 288));

            _styles = fileSections["V4+ Styles"].MapItems("Style", i => new AssStyle(i))
                                                .ToDictionaryOverwrite(s => s.Name);

            if (styleOptions != null)
                _styleOptions = styleOptions.ToDictionaryOverwrite(o => o.Name);

            DefaultStyle = _styles.GetOrDefault("Default") ?? _styles.First().Value;

            foreach (AssDialogue dialogue in fileSections["Events"].MapItems("Dialogue", i => new AssDialogue(i)))
            {
                AssStyle style = GetStyle(dialogue.Style);
                if (style == null)
                    throw new Exception($"Line \"{dialogue.Text}\" refers to style \"{dialogue.Style}\" which doesn't exist.");

                AssStyleOptions options = GetStyleOptions(dialogue.Style);

                List<AssLine> lines = ParseLine(dialogue, style, options);
                Lines.AddRange(lines.SelectMany(ExpandLine));
            }

            EmulateKaraokeForSimultaneousLines();
            foreach (AssLine line in Lines)
            {
                line.NormalizeAlpha();
            }
        }

        public AssStyle DefaultStyle
        {
            get;
        }

        public IEnumerable<AssStyle> Styles
        {
            get { return _styles.Values; }
        }

        public AssStyle GetStyle(string name)
        {
            return _styles.GetOrDefault(name);
        }

        public AssStyleOptions GetStyleOptions(AssStyle style)
        {
            return GetStyleOptions(style.Name);
        }

        public AssStyleOptions GetStyleOptions(string name)
        {
            return _styleOptions?.GetOrDefault(name);
        }

        public override void Save(string filePath)
        {
            using StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8);
            WriteHeader(writer);
            WriteStyles(writer);
            WriteLines(writer);
        }

        private void RegisterTagHandlers()
        {
            RegisterTagHandler(new AssAlignmentTagHandler());
            RegisterTagHandler(new AssAlphaTagHandler());
            RegisterTagHandler(new AssBoldTagHandler());
            RegisterTagHandler(new AssChromaTagHandler());
            RegisterTagHandler(new AssComplexFadeTagHandler());
            RegisterTagHandler(new AssFontSizeTagHandler());
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
            RegisterTagHandler(new AssPackTagHandler());
            RegisterTagHandler(new AssPositionTagHandler());
            RegisterTagHandler(new AssRegularScriptTagHandler());
            RegisterTagHandler(new AssResetTagHandler());
            RegisterTagHandler(new AssRubyTagHandler());
            RegisterTagHandler(new AssSecondaryAlphaTagHandler());
            RegisterTagHandler(new AssSecondaryColorTagHandler());
            RegisterTagHandler(new AssShadowAlphaTagHandler());
            RegisterTagHandler(new AssShadowColorTagHandler());
            RegisterTagHandler(new AssShakeTagHandler());
            RegisterTagHandler(new AssSimpleFadeTagHandler());
            RegisterTagHandler(new AssSubscriptTagHandler());
            RegisterTagHandler(new AssSuperscriptTagHandler());
            RegisterTagHandler(new AssTransformTagHandler());
            RegisterTagHandler(new AssUnderlineTagHandler());
            RegisterTagHandler(new AssVerticalTypeTagHandler());
        }

        private void RegisterTagHandler(AssTagHandlerBase handler)
        {
            _tagHandlers.Add(handler.Tag, handler);
        }

        private Dictionary<string, AssDocumentSection> ReadDocument(Stream stream)
        {
            Dictionary<string, AssDocumentSection> sections = new Dictionary<string, AssDocumentSection>();
            AssDocumentSection currentSection = null;
            string line;

            using StreamReader reader = new StreamReader(stream);
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

            return sections;
        }

        private List<AssLine> ParseLine(AssDialogue dialogue, AssStyle style, AssStyleOptions styleOptions)
        {
            AssLine line = new AssLine(dialogue.Start, dialogue.End) { AnchorPoint = style.AnchorPoint };
            
            string[] effects = dialogue.Effect.Split(';');
            if (effects.Contains(EffectNames.NoAndroidDarkTextHack))
                line.AndroidDarkTextHackAllowed = false;

            AssTagContext context = new AssTagContext
                                    {
                                        Document = this,
                                        InitialStyle = style,
                                        InitialStyleOptions = styleOptions,
                                        Style = style,
                                        StyleOptions = styleOptions,
                                        Line = line,
                                        Section = new AssSection()
                                    };

            ApplyStyle(context.Section, style, styleOptions);
            CreateTagSections(line, dialogue.Text, context);
            CreateRubySections(line);

            List<AssLine> lines = new List<AssLine> { line };
            foreach (AssTagContext.PostProcessor postProcessor in context.PostProcessors)
            {
                List<AssLine> extraLines = postProcessor();
                if (extraLines != null)
                    lines.AddRange(extraLines);
            }

            return lines;
        }

        internal void CreateTagSections(AssLine line, string text, AssTagContext context)
        {
            text = Regex.Replace(text, @"(?:\\N)+$", "");
            HashSet<string> handledWholeLineTags = new HashSet<string>();

            int start = 0;
            foreach (Match tagGroupMatch in Regex.Matches(text, @"\{(.*?)\}"))
            {
                int end = tagGroupMatch.Index;

                if (end > start || (context.Section.Duration > TimeSpan.Zero && Regex.IsMatch(tagGroupMatch.Groups[1].Value, @"\\k\s*\d+")))
                {
                    if (end == start)
                        context.Section.Text = "\x200B";
                    else
                        context.Section.Text = ResolveEscapeSequences(text.Substring(start, end - start));

                    line.Sections.Add(context.Section);

                    context.Section = (AssSection)context.Section.Clone();
                    context.Section.Text = null;
                    context.Section.Duration = TimeSpan.Zero;
                }

                foreach (Match tagMatch in Regex.Matches(tagGroupMatch.Groups[1].Value, @"\\(?<tag>fn|r|\d?[a-z]+)\s*(?<arg>\([^\(\)]*(?:\)|$)|[^\\\(\)]*)"))
                {
                    if (!_tagHandlers.TryGetValue(tagMatch.Groups["tag"].Value, out AssTagHandlerBase handler))
                        continue;

                    if (handler.AffectsWholeLine && !handledWholeLineTags.Add(tagMatch.Groups["tag"].Value))
                        continue;

                    handler.Handle(context, tagMatch.Groups["arg"].Value.Trim());
                }

                start = tagGroupMatch.Index + tagGroupMatch.Length;
            }

            if (start < text.Length)
            {
                context.Section.Text = ResolveEscapeSequences(text.Substring(start, text.Length - start));
                line.Sections.Add(context.Section);
            }
        }

        internal void ApplyStyle(AssSection section, AssStyle style)
        {
            ApplyStyle(section, style, GetStyleOptions(style.Name));
        }

        internal void ApplyStyle(AssSection section, AssStyle style, AssStyleOptions options)
        {
            section.Font = style.Font;
            section.Scale = style.LineHeight / DefaultStyle.LineHeight;
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

            section.Blur = 0;
        }

        private static void CreateRubySections(AssLine line)
        {
            for (int sectionIdx = line.Sections.Count - 1; sectionIdx >= 0; sectionIdx--)
            {
                AssSection section = (AssSection)line.Sections[sectionIdx];
                if (section.RubyPosition == RubyPosition.None)
                    continue;

                MatchCollection matches = Regex.Matches(section.Text, @"\[(?<text>.+?)/(?<ruby>.+?)\]");
                if (matches.Count == 0)
                    continue;

                line.Sections.RemoveAt(sectionIdx);

                int interStartPos = 0;
                int numSubSections = 0;
                foreach (Match match in matches)
                {
                    if (match.Index > interStartPos)
                        InsertRubySection(line, section, section.Text.Substring(interStartPos, match.Index - interStartPos), RubyPart.None, sectionIdx, ref numSubSections);

                    InsertRubySection(line, section, match.Groups["text"].Value, RubyPart.Text, sectionIdx, ref numSubSections);
                    InsertRubySection(line, section, "(", RubyPart.Parenthesis, sectionIdx, ref numSubSections);
                    InsertRubySection(line, section, match.Groups["ruby"].Value, section.RubyPosition == RubyPosition.Below ? RubyPart.RubyBelow : RubyPart.RubyAbove, sectionIdx, ref numSubSections);
                    InsertRubySection(line, section, ")", RubyPart.Parenthesis, sectionIdx, ref numSubSections);

                    interStartPos = match.Index + match.Length;
                }

                if (interStartPos < section.Text.Length)
                    InsertRubySection(line, section, section.Text.Substring(interStartPos), RubyPart.None, sectionIdx, ref numSubSections);

                ((AssSection)line.Sections[sectionIdx]).Duration = section.Duration;
            }
        }

        private static void InsertRubySection(AssLine line, Section format, string text, RubyPart rubyPart, int sectionIndex, ref int numSubSections)
        {
            AssSection section = (AssSection)format.Clone();
            section.Text = text;
            section.RubyPart = rubyPart;
            section.Duration = TimeSpan.Zero;
            line.Sections.Insert(sectionIndex + numSubSections, section);
            numSubSections++;
        }

        private IEnumerable<AssLine> ExpandLine(AssLine line)
        {
            return ExpandLineForKaraoke(line).SelectMany(l => Animator.Expand(this, l));
        }

        private IEnumerable<AssLine> ExpandLineForKaraoke(AssLine line)
        {
            if (line.Sections.Cast<AssSection>().All(s => s.Duration == TimeSpan.Zero))
                return new[] { line };

            if (CanUseNativeKaraoke(line))
            {
                ApplyNativeKaraoke(line);
                return new[] { line };
            }

            return CreateEmulatedKaraokeLines(line);
        }

        private static bool CanUseNativeKaraoke(AssLine line)
        {
            return line.KaraokeType.GetType() == typeof(SimpleKaraokeType) &&
                   line.Animations.Count == 0 &&
                   line.Sections.Cast<AssSection>().All(s => s.SecondaryColor.A == 0 &&
                                                             s.CurrentWordForeColor.IsEmpty &&
                                                             s.CurrentWordOutlineColor.IsEmpty &&
                                                             s.CurrentWordShadowColor.IsEmpty &&
                                                             s.Animations.Count == 0 &&
                                                             s.Duration != TimeSpan.Zero);
        }

        private static void ApplyNativeKaraoke(AssLine line)
        {
            TimeSpan timeOffset = TimeSpan.Zero;
            foreach (AssSection section in line.Sections)
            {
                section.StartOffset = timeOffset;
                timeOffset += section.Duration;
            }
        }

        protected IEnumerable<AssLine> CreateEmulatedKaraokeLines(AssLine line)
        {
            SortedList<TimeSpan, int> activeSectionsPerStep = GetKaraokeSteps(line);
            for (int stepIdx = 0; stepIdx < activeSectionsPerStep.Count; stepIdx++)
            {
                IEnumerable<AssLine> stepLines = CreateKaraokeStepLines(line, activeSectionsPerStep, stepIdx);
                foreach (AssLine stepLine in stepLines)
                {
                    stepLine.Sections.RemoveAll(s => s.Text == "\x200B");       // Remove empty sections that were added in CreateTagSections()
                    yield return stepLine;
                }
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

        private IEnumerable<AssLine> CreateKaraokeStepLines(AssLine originalLine, SortedList<TimeSpan, int> activeSectionsPerStep, int stepIdx)
        {
            TimeSpan timeOffset = activeSectionsPerStep.Keys[stepIdx];
            int numActiveSections = activeSectionsPerStep.Values[stepIdx];

            DateTime startTime = TimeUtil.RoundTimeToFrameCenter(originalLine.Start + timeOffset);
            if (startTime >= originalLine.End)
                return new List<AssLine>();

            DateTime endTime;
            if (stepIdx < activeSectionsPerStep.Count - 1)
            {
                endTime = TimeUtil.RoundTimeToFrameCenter(originalLine.Start + activeSectionsPerStep.Keys[stepIdx + 1]);
                if (endTime > originalLine.End)
                    endTime = originalLine.End;
            }
            else
            {
                endTime = originalLine.End;
            }

            AssLine stepLine = (AssLine)originalLine.Clone();
            stepLine.Start = startTime;
            stepLine.End = endTime;

            foreach (AssSection section in stepLine.Sections.Take(numActiveSections))
            {
                section.Duration = TimeSpan.Zero;
                section.StartOffset = TimeSpan.Zero;
                section.Animations.RemoveAll(a => a is SecondaryColorAnimation);
            }

            foreach (AssSection section in stepLine.Sections.Skip(numActiveSections))
            {
                section.Duration = TimeSpan.Zero;
                section.StartOffset = TimeSpan.Zero;

                section.ForeColor = section.SecondaryColor;

                section.Animations.RemoveAll(a => a is ForeColorAnimation);
                foreach (SecondaryColorAnimation anim in section.Animations.OfType<SecondaryColorAnimation>().ToList())
                {
                    section.Animations.Remove(anim);
                    section.Animations.Add(new ForeColorAnimation(anim.StartTime, anim.StartColor, anim.EndTime, anim.EndColor, anim.Acceleration));
                }

                if (section.ForeColor.A == 0 && !section.Animations.OfType<ForeColorAnimation>().Any())
                {
                    section.ForeColor = Color.FromArgb(0);
                    section.BackColor = Color.FromArgb(0);
                    section.ShadowColors.Clear();
                }
            }

            return ApplyKaraokeType(originalLine, stepLine, activeSectionsPerStep, stepIdx);
        }

        private IEnumerable<AssLine> ApplyKaraokeType(AssLine originalLine, AssLine stepLine, SortedList<TimeSpan, int> activeSectionsPerStep, int stepIdx)
        {
            int prevNumActiveActions = stepIdx > 0 ? activeSectionsPerStep.Values[stepIdx - 1] : 0;
            int numActiveSections = activeSectionsPerStep.Values[stepIdx];
            List<AssSection> singingSections = stepLine.Sections
                                                       .Cast<AssSection>()
                                                       .Skip(prevNumActiveActions)
                                                       .Take(numActiveSections - prevNumActiveActions)
                                                       .ToList();

            AssKaraokeStepContext context =
                new AssKaraokeStepContext
                {
                    Document = this,
                    OriginalLine = originalLine,
                    ActiveSectionsPerStep = activeSectionsPerStep,
                    
                    StepLine = stepLine,
                    StepIndex = stepIdx,
                    NumActiveSections = numActiveSections,
                    SingingSections = singingSections
                };
            return stepLine.KaraokeType.Apply(context);
        }

        private void WriteHeader(StreamWriter writer)
        {
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            writer.WriteLine("[Script Info]");
            writer.WriteLine($"; Script generated by YTSubConverter {version.Major}.{version.Minor}.{version.Build}");
            writer.WriteLine("; https://github.com/arcusmaximus/YTSubConverter/");
            writer.WriteLine("ScriptType: v4.00+");
            writer.WriteLine("WrapStyle: 0");
            writer.WriteLine("ScaledBorderAndShadow: yes");
            writer.WriteLine("PlayResX: " + VideoDimensions.Width);
            writer.WriteLine("PlayResY: " + VideoDimensions.Height);
            writer.WriteLine();
        }

        private void WriteStyles(StreamWriter writer)
        {
            writer.WriteLine("[V4+ Styles]");
            writer.WriteLine(
                "Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, " +
                "StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding"
            );
            foreach (AssStyle style in _styles.Values)
            {
                writer.Write("Style: ");
                writer.Write(style.Name + ",");
                writer.Write(style.Font + ",");
                writer.Write(style.LineHeight.ToString(CultureInfo.InvariantCulture) + ",");
                writer.Write(StyleColorToHex(style.PrimaryColor) + ",");
                writer.Write(StyleColorToHex(style.SecondaryColor) + ",");
                writer.Write(StyleColorToHex(style.OutlineColor) + ",");
                writer.Write(StyleColorToHex(style.ShadowColor) + ",");
                writer.Write((style.Bold ? 1 : 0) + ",");
                writer.Write((style.Italic ? 1 : 0) + ",");
                writer.Write((style.Underline ? 1 : 0) + ",");
                writer.Write("0,100,100,0,0,");
                writer.Write((style.OutlineIsBox ? 3 : 1) + ",");
                writer.Write(style.OutlineThickness.ToString(CultureInfo.InvariantCulture) + ",");
                writer.Write(style.ShadowDistance.ToString(CultureInfo.InvariantCulture) + ",");
                writer.Write(GetAlignment(style.AnchorPoint) + ",");
                writer.Write("10,10,10,1");
                writer.WriteLine();
            }
            writer.WriteLine();
        }

        private static string StyleColorToHex(Color color)
        {
            return $"&H{255 - color.A:X02}{color.B:X02}{color.G:X02}{color.R:X02}";
        }

        protected void WriteLines(StreamWriter writer)
        {
            writer.WriteLine("[Events]");
            writer.WriteLine("Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text");

            foreach (AssLine line in Lines)
            {
                WriteLine(line, writer);
            }
        }

        protected virtual void WriteLine(AssLine line, StreamWriter writer)
        {
            if (line.Sections.Count == 0)
                return;

            AssStyle style = GetStyleMatchingStructure((AssSection)line.Sections[0]);
            WriteLineMetadata(line, style, writer);

            AssSection prevSection = new AssSection();
            AssStyle prevStyle = style;
            ApplyStyle(prevSection, prevStyle);

            AssLineContentBuilder lineContent = new AssLineContentBuilder();
            AppendLineTags(line, style, lineContent);

            RubyPart currentRubyPosition = RubyPart.None;
            for (int i = 0; i < line.Sections.Count; i++)
            {
                AssSection section = (AssSection)line.Sections[i];
                style = GetStyleMatchingStructure(section);
                if (style != prevStyle || (currentRubyPosition != RubyPart.None && section.RubyPart == RubyPart.None))
                {
                    lineContent.AppendTag("r", style);
                    currentRubyPosition = RubyPart.None;
                    prevSection = (AssSection)prevSection.Clone();
                    ApplyStyle(prevSection, style);
                }

                AppendSectionTags(section, prevSection, lineContent);

                if (section.RubyPart == RubyPart.Text)
                {
                    RubyPart rubyPart;
                    if (i + 4 > line.Sections.Count || ((rubyPart = line.Sections[i + 2].RubyPart) != RubyPart.RubyAbove && rubyPart != RubyPart.RubyBelow))
                        throw new InvalidDataException("Invalid ruby sequence");

                    if (rubyPart != currentRubyPosition)
                    {
                        lineContent.AppendTag("ytruby", rubyPart == RubyPart.RubyAbove ? 8 : 2);
                        currentRubyPosition = rubyPart;
                    }
                    lineContent.AppendText($"[{section.Text}/{line.Sections[i + 2].Text}]");
                    i += 3;
                }
                else
                {
                    lineContent.AppendText(section.Text);
                }

                prevSection = section;
                prevStyle = style;
            }

            writer.WriteLine(lineContent);
        }

        private void WriteLineMetadata(AssLine line, AssStyle style, StreamWriter writer)
        {
            string effects = !line.AndroidDarkTextHackAllowed ? EffectNames.NoAndroidDarkTextHack : string.Empty;
            writer.Write($"Dialogue: 0,{line.Start:H:mm:ss.ff},{line.End:H:mm:ss.ff},{style.Name},,0,0,0,{effects},");
        }

        private AssStyle GetStyleMatchingStructure(AssSection section)
        {
            HashSet<ShadowType> sectionShadowTypes = section.ShadowColors.Keys.ToHashSet();

            foreach (AssStyle style in _styles.Values)
            {
                AssStyleOptions options = GetStyleOptions(style);

                if (style.HasOutlineBox != (section.BackColor.A > 0))
                    continue;

                HashSet<ShadowType> styleShadowTypes = new HashSet<ShadowType>();
                if (style.HasOutline && !style.OutlineIsBox)
                    styleShadowTypes.Add(ShadowType.Glow);

                if (options != null)
                    styleShadowTypes.UnionWith(options.ShadowTypes);

                if (!styleShadowTypes.SetEquals(sectionShadowTypes))
                    continue;

                return style;
            }
            return null;
        }

        private void AppendLineTags(AssLine line, AssStyle style, AssLineContentBuilder lineContent)
        {
            if (line.AnchorPoint != style.AnchorPoint)
                lineContent.AppendTag("an", GetAlignment(line.AnchorPoint));

            if (line.Position != null)
                lineContent.AppendTag("pos", line.Position.Value.X, line.Position.Value.Y);

            if (line.VerticalTextType != VerticalTextType.None)
                lineContent.AppendTag("ytvert", AssVerticalTypeTagHandler.GetVerticalTextTypeId(line.VerticalTextType));
        }

        private void AppendSectionTags(AssSection section, AssSection prevSection, AssLineContentBuilder lineContent)
        {
            if (section.Font != prevSection.Font)
                lineContent.AppendTag("fn", section.Font);

            float prevLineHeight = ScaleToLineHeight(prevSection.Font, prevSection.Scale);
            float lineHeight = ScaleToLineHeight(section.Font, section.Scale);
            if (lineHeight != prevLineHeight)
                lineContent.AppendTag("fs", lineHeight);

            if (section.Bold != prevSection.Bold)
                lineContent.AppendTag("b", section.Bold);

            if (section.Italic != prevSection.Italic)
                lineContent.AppendTag("i", section.Italic);

            if (section.Underline != prevSection.Underline)
                lineContent.AppendTag("u", section.Underline);

            AppendColorTags("c", "1a", prevSection.ForeColor, section.ForeColor, lineContent);
            AppendColorTags("2c", "2a", prevSection.SecondaryColor, section.SecondaryColor, lineContent);

            if (section.BackColor.A > 0)
            {
                AppendColorTags("3c", "3a", prevSection.BackColor, section.BackColor, lineContent);
                if (prevSection.ShadowColors.Count == 1 && section.ShadowColors.Count == 1)
                    AppendColorTags("4c", "4a", prevSection.ShadowColors.Values.First(), section.ShadowColors.Values.First(), lineContent);
            }
            else if (prevSection.ShadowColors.Count == 1 && section.ShadowColors.Count == 1)
            {
                if (section.ShadowColors.Keys.First() == ShadowType.Glow)
                    AppendColorTags("3c", "3a", prevSection.ShadowColors.Values.First(), section.ShadowColors.Values.First(), lineContent);
                else
                    AppendColorTags("4c", "4a", prevSection.ShadowColors.Values.First(), section.ShadowColors.Values.First(), lineContent);
            }

            if (section.Offset != prevSection.Offset)
            {
                lineContent.AppendTag(
                    section.Offset switch
                    {
                        OffsetType.Subscript => "ytsub",
                        OffsetType.Superscript => "ytsup",
                        OffsetType.Regular => "ytsur",
                    }
                );
            }

            if (section.Packed != prevSection.Packed)
                lineContent.AppendTag("ytpack", section.Packed);

            if (section.Duration > TimeSpan.Zero)
                lineContent.AppendTag("k", (int)(section.Duration.TotalMilliseconds / 10));

            if (section.Blur != prevSection.Blur)
                lineContent.AppendTag("blur", section.Blur);
        }

        protected virtual float ScaleToLineHeight(string font, float scale)
        {
            return DefaultStyle.LineHeight * scale;
        }

        private void AppendColorTags(string colorTag, string alphaTag, Color prevColor, Color newColor, AssLineContentBuilder lineContent)
        {
            if (prevColor.R != newColor.R || prevColor.G != newColor.G || prevColor.B != newColor.B)
                lineContent.AppendTag(colorTag, newColor);

            if (prevColor.A != newColor.A)
                lineContent.AppendTag(alphaTag, 255 - newColor.A);
        }

        private void EmulateKaraokeForSimultaneousLines()
        {
            for (int i = 0; i < Lines.Count; i++)
            {
                AssLine line = (AssLine)Lines[i];
                if (line.Position != null || !line.Sections.Any(s => s.StartOffset > TimeSpan.Zero))
                    continue;

                if (!Lines.Any(l => l.Start < line.End && l.End > line.Start && l.Position == null && l.AnchorPoint == line.AnchorPoint))
                    continue;

                Lines.RemoveAt(i);
                Lines.InsertRange(i, CreateEmulatedKaraokeLines(line));
            }
        }

        private static string ResolveEscapeSequences(string text)
        {
            return text.Replace("\\h", "\xA0")
                       .Replace("\\n", "\xA0")
                       .Replace("\\N", "\r\n");
        }

        internal static AnchorPoint GetAnchorPoint(int alignment)
        {
            return alignment switch
                   {
                       1 => AnchorPoint.BottomLeft,
                       2 => AnchorPoint.BottomCenter,
                       3 => AnchorPoint.BottomRight,
                       4 => AnchorPoint.MiddleLeft,
                       5 => AnchorPoint.Center,
                       6 => AnchorPoint.MiddleRight,
                       7 => AnchorPoint.TopLeft,
                       8 => AnchorPoint.TopCenter,
                       9 => AnchorPoint.TopRight,
                       _ => throw new ArgumentException($"{alignment} is not a valid alignment")
                   };
        }

        internal static int GetAlignment(AnchorPoint anchorPoint)
        {
            return anchorPoint switch
                   {
                       AnchorPoint.BottomLeft => 1,
                       AnchorPoint.BottomCenter => 2,
                       AnchorPoint.BottomRight => 3,
                       AnchorPoint.MiddleLeft => 4,
                       AnchorPoint.Center => 5,
                       AnchorPoint.MiddleRight => 6,
                       AnchorPoint.TopLeft => 7,
                       AnchorPoint.TopCenter => 8,
                       AnchorPoint.TopRight => 9,
                       _ => throw new ArgumentException()
                   };
        }
    }
}
