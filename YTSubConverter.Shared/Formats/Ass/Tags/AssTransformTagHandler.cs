using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using YTSubConverter.Shared.Animations;
using YTSubConverter.Shared.Util;

namespace YTSubConverter.Shared.Formats.Ass.Tags
{
    internal class AssTransformTagHandler : AssTagHandlerBase
    {
        private delegate void TransformTagHandler(AssTagContext context, DateTime startTime, DateTime endTime, float accel, string arg);

        private static readonly Dictionary<string, TransformTagHandler> TransformTagHandlers;

        static AssTransformTagHandler()
        {
            TransformTagHandlers = new Dictionary<string, TransformTagHandler>
                                   {
                                       { "c", HandleTransformForeColorTag },
                                       { "1c", HandleTransformForeColorTag },
                                       { "2c", HandleTransformSecondaryColorTag },
                                       { "3c", HandleTransformOutlineColorTag },
                                       { "4c", HandleTransformShadowColorTag },
                                       { "alpha", HandleTransformAlphaTag },
                                       { "1a", HandleTransformForeAlphaTag },
                                       { "2a", HandleTransformSecondaryAlphaTag },
                                       { "3a", HandleTransformOutlineAlphaTag },
                                       { "4a", HandleTransformShadowAlphaTag },
                                       { "fs", HandleTransformFontSizeTag }
                                   };
        }

        public override string Tag => "t";

        public override bool AffectsWholeLine => false;

        public override void Handle(AssTagContext context, string arg)
        {
            if (!TryParseArgs(context, arg, out DateTime startTime, out DateTime endTime, out float accel, out string modifiers))
                return;

            context.Line.AndroidDarkTextHackAllowed = false;

            foreach (Match match in Regex.Matches(modifiers, @"\\(?<tag>\d?[a-z]+)(?<arg>[^\\]*)"))
            {
                if (TransformTagHandlers.TryGetValue(match.Groups["tag"].Value, out TransformTagHandler handler))
                    handler(context, startTime, endTime, accel, match.Groups["arg"].Value.Trim());
            }
        }

        private static bool TryParseArgs(AssTagContext context, string arg, out DateTime startTime, out DateTime endTime, out float accel, out string modifiers)
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
                    if (!TryParseFloat(args[0], out accel))
                        return false;

                    modifiers = args[1];
                    return true;
                }

                case 3:
                {
                    if (!TryParseInt(args[0], out int t1) ||
                        !TryParseInt(args[1], out int t2))
                        return false;

                    startTime = context.Line.Start.AddMilliseconds(t1);
                    endTime = context.Line.Start.AddMilliseconds(t2);
                    modifiers = args[2];
                    return true;
                }

                case 4:
                {
                    if (!TryParseInt(args[0], out int t1) ||
                        !TryParseInt(args[1], out int t2) ||
                        !TryParseFloat(args[2], out accel))
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

        private static void HandleTransformForeColorTag(AssTagContext context, DateTime startTime, DateTime endTime, float accel, string arg)
        {
            ForeColorAnimation anim = FetchColorAnimation(context, startTime, endTime, s => s.ForeColor, (s, e, c) => new ForeColorAnimation(s, c, e, c, accel));
            anim.EndColor = ParseColor(arg, anim.EndColor.A);
        }

        private static void HandleTransformSecondaryColorTag(AssTagContext context, DateTime startTime, DateTime endTime, float accel, string arg)
        {
            SecondaryColorAnimation anim = FetchColorAnimation(context, startTime, endTime, s => s.SecondaryColor, (s, e, c) => new SecondaryColorAnimation(s, c, e, c, accel));
            anim.EndColor = ParseColor(arg, anim.EndColor.A);
        }

        private static void HandleTransformOutlineColorTag(AssTagContext context, DateTime startTime, DateTime endTime, float accel, string arg)
        {
            if (!context.Style.HasOutline)
                return;

            if (context.Style.OutlineIsBox)
            {
                BackColorAnimation anim = FetchColorAnimation(context, startTime, endTime, s => s.BackColor, (s, e, c) => new BackColorAnimation(s, c, e, c, accel));
                anim.EndColor = ParseColor(arg, anim.EndColor.A);
            }
            else
            {
                HandleTransformShadowColorTag(context, ShadowType.Glow, startTime, endTime, accel, arg);
            }
        }

        private static void HandleTransformShadowColorTag(AssTagContext context, DateTime startTime, DateTime endTime, float accel, string arg)
        {
            foreach (ShadowType shadowType in context.Section.ShadowColors.Keys)
            {
                if (shadowType != ShadowType.Glow || !context.Style.HasOutline || context.Style.HasOutlineBox)
                    HandleTransformShadowColorTag(context, shadowType, startTime, endTime, accel, arg);
            }
        }

        private static void HandleTransformShadowColorTag(AssTagContext context, ShadowType shadowType, DateTime startTime, DateTime endTime, float accel, string arg)
        {
            ShadowColorAnimation anim = FetchColorAnimation(
                context,
                startTime,
                endTime,
                a => a.ShadowType == shadowType,
                s => s.ShadowColors[shadowType],
                (s, e, c) => new ShadowColorAnimation(shadowType, s, c, e, c, accel)
            );
            anim.EndColor = ParseColor(arg, anim.EndColor.A);
        }

        private static void HandleTransformAlphaTag(AssTagContext context, DateTime startTime, DateTime endTime, float accel, string arg)
        {
            HandleTransformForeAlphaTag(context, startTime, endTime, accel, arg);
            HandleTransformSecondaryAlphaTag(context, startTime, endTime, accel, arg);
            HandleTransformOutlineAlphaTag(context, startTime, endTime, accel, arg);
            HandleTransformShadowAlphaTag(context, startTime, endTime, accel, arg);
        }

        private static void HandleTransformForeAlphaTag(AssTagContext context, DateTime startTime, DateTime endTime, float accel, string arg)
        {
            ForeColorAnimation anim = FetchColorAnimation(context, startTime, endTime, s => s.ForeColor, (s, e, c) => new ForeColorAnimation(s, c, e, c, accel));
            anim.EndColor = ColorUtil.ChangeAlpha(anim.EndColor, 255 - (ParseHex(arg) & 255));
        }

        private static void HandleTransformSecondaryAlphaTag(AssTagContext context, DateTime startTime, DateTime endTime, float accel, string arg)
        {
            SecondaryColorAnimation anim = FetchColorAnimation(context, startTime, endTime, s => s.SecondaryColor, (s, e, c) => new SecondaryColorAnimation(s, c, e, c, accel));
            anim.EndColor = ColorUtil.ChangeAlpha(anim.EndColor, 255 - (ParseHex(arg) & 255));
        }

        private static void HandleTransformOutlineAlphaTag(AssTagContext context, DateTime startTime, DateTime endTime, float accel, string arg)
        {
            if (!context.Style.HasOutline)
                return;

            if (context.Style.OutlineIsBox)
            {
                BackColorAnimation anim = FetchColorAnimation(context, startTime, endTime, s => s.BackColor, (s, e, c) => new BackColorAnimation(s, c, e, c, accel));
                anim.EndColor = ColorUtil.ChangeAlpha(anim.EndColor, 255 - (ParseHex(arg) & 255));
            }
            else
            {
                HandleTransformShadowAlphaTag(context, ShadowType.Glow, startTime, endTime, accel, arg);
            }
        }

        private static void HandleTransformShadowAlphaTag(AssTagContext context, DateTime startTime, DateTime endTime, float accel, string arg)
        {
            foreach (KeyValuePair<ShadowType, Color> shadowColor in context.Section.ShadowColors)
            {
                if (shadowColor.Key != ShadowType.Glow || !context.Style.HasOutline || context.Style.HasOutlineBox)
                    HandleTransformShadowAlphaTag(context, shadowColor.Key, startTime, endTime, accel, arg);
            }
        }

        private static void HandleTransformShadowAlphaTag(AssTagContext context, ShadowType shadowType, DateTime startTime, DateTime endTime, float accel, string arg)
        {
            ShadowColorAnimation anim = FetchColorAnimation(
                context,
                startTime,
                endTime,
                a => a.ShadowType == shadowType,
                s => s.ShadowColors[shadowType],
                (s, e, c) => new ShadowColorAnimation(shadowType, s, c, e, c, accel)
            );
            anim.EndColor = ColorUtil.ChangeAlpha(anim.EndColor, 255 - (ParseHex(arg) & 255));
        }

        private static void HandleTransformFontSizeTag(AssTagContext context, DateTime startTime, DateTime endTime, float accel, string arg)
        {
            if (!TryParseFloat(arg, out float lineHeight))
                return;

            AssSection section = context.Section;
            section.Animations.RemoveAll(a => a is ScaleAnimation && a.StartTime >= startTime);

            ScaleAnimation prevAnim = section.Animations.OfType<ScaleAnimation>().LastOrDefault();
            float startScale = prevAnim?.EndScale ?? context.Section.Scale;
            float endScale = lineHeight / context.Document.DefaultStyle.LineHeight;
            context.Section.Animations.Add(new ScaleAnimation(startTime, startScale, endTime, endScale, accel));
        }

        private static T FetchColorAnimation<T>(
            AssTagContext context,
            DateTime startTime,
            DateTime endTime,
            Func<AssSection, Color> getSectionColor,
            Func<DateTime, DateTime, Color, T> createAnim
        )
            where T : ColorAnimation
        {
            return FetchColorAnimation(context, startTime, endTime, null, getSectionColor, createAnim);
        }

        private static T FetchColorAnimation<T>(
            AssTagContext context,
            DateTime startTime,
            DateTime endTime,
            Func<T, bool> predicate,
            Func<AssSection, Color> getSectionColor,
            Func<DateTime, DateTime, Color, T> createAnim
        )
            where T : ColorAnimation
        {
            context.Section.Animations.RemoveAll(a => a is T && a.StartTime > startTime);

            AssSection section = context.Section;
            IEnumerable<T> existingAnims = section.Animations.OfType<T>().Where(a => a.StartTime == startTime && a.EndTime == endTime);
            if (predicate != null)
                existingAnims = existingAnims.Where(predicate);

            T anim = existingAnims.FirstOrDefault();
            if (anim == null)
            {
                IEnumerable<T> prevAnims = section.Animations.OfType<T>();
                if (predicate != null)
                    prevAnims = prevAnims.Where(predicate);

                T prevAnim = prevAnims.LastOrDefault();
                anim = createAnim(startTime, endTime, prevAnim?.EndColor ?? getSectionColor(section));
                section.Animations.Add(anim);
            }

            return anim;
        }
    }
}
