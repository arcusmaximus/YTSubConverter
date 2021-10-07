using System;
using System.Collections.Generic;
using System.Linq;
using YTSubConverter.Shared.Formats.Ass;
using YTSubConverter.Shared.Util;

namespace YTSubConverter.Shared.Animations
{
    internal static class Animator
    {
        public static IEnumerable<AssLine> Expand(AssDocument document, AssLine originalLine)
        {
            List<AnimationWithSectionIndex> anims = GetAnimationsWithSectionIndex(originalLine);
            if (anims.Count == 0)
            {
                yield return originalLine;
                yield break;
            }

            SortedList<TimeRange, List<AnimationWithSectionIndex>> animClusters = ClusterAnimations(originalLine, anims);
            AssLine lastLine = CreateInitialLine(originalLine, anims);
            if (animClusters.Count == 0 || animClusters.Keys[0].Start > lastLine.Start)
                yield return lastLine;

            for (int i = 0; i < animClusters.Count; i++)
            {
                TimeRange clusterRange = animClusters.Keys[i];
                List<AnimationWithSectionIndex> clusterAnims = animClusters.Values[i];
                foreach (AssLine frameLine in CreateFrameLines(document, lastLine, clusterRange, clusterAnims))
                {
                    lastLine.End = frameLine.Start;
                    yield return lastLine = frameLine;
                }

                DateTime interAnimStart = clusterRange.End;
                DateTime interAnimEnd = i < animClusters.Count - 1 ? animClusters.Keys[i + 1].Start : originalLine.End;
                if (interAnimEnd > interAnimStart)
                    yield return lastLine = CreatePostAnimationClusterLine(originalLine, lastLine, interAnimStart, interAnimEnd, clusterAnims);
            }

            lastLine.End = originalLine.End;
        }

        private static List<AnimationWithSectionIndex> GetAnimationsWithSectionIndex(AssLine line)
        {
            List<AnimationWithSectionIndex> animations = new List<AnimationWithSectionIndex>();

            foreach (Animation anim in line.Animations.Where(a => a is MoveAnimation))
            {
                animations.Add(new AnimationWithSectionIndex(anim, -1));
            }

            foreach (Animation anim in line.Animations.Where(a => !(a is MoveAnimation)))
            {
                animations.Add(new AnimationWithSectionIndex(anim, -1));
            }

            for (int i = 0; i < line.Sections.Count; i++)
            {
                AssSection section = (AssSection)line.Sections[i];
                foreach (Animation anim in section.Animations)
                {
                    animations.Add(new AnimationWithSectionIndex(anim, i));
                }
            }

            return animations;
        }

        private static SortedList<TimeRange, List<AnimationWithSectionIndex>> ClusterAnimations(AssLine line, List<AnimationWithSectionIndex> allAnims)
        {
            List<TimeRange> clusterRanges = GetAnimationClusterTimeRanges(allAnims.Select(a => a.Animation));

            TimeRange lineRange = new TimeRange(line.Start, line.End);
            var clusters = new SortedList<TimeRange, List<AnimationWithSectionIndex>>();
            foreach (TimeRange clusterRange in clusterRanges)
            {
                if (!clusterRange.Overlaps(lineRange))
                    continue;

                List<AnimationWithSectionIndex> clusterAnims = new List<AnimationWithSectionIndex>();
                foreach (AnimationWithSectionIndex animWithSection in allAnims)
                {
                    if (clusterRange.Contains(animWithSection.Animation.StartTime) && animWithSection.Animation.EndTime > animWithSection.Animation.StartTime)
                        clusterAnims.Add(animWithSection);
                }

                clusterRange.IntersectWith(lineRange);
                clusterRange.Start = TimeUtil.RoundTimeToFrameCenter(clusterRange.Start);
                clusterRange.End = TimeUtil.RoundTimeToFrameCenter(clusterRange.End);
                clusters.FetchValue(clusterRange, () => new List<AnimationWithSectionIndex>()).AddRange(clusterAnims);
            }
            return clusters;
        }

        private static List<TimeRange> GetAnimationClusterTimeRanges(IEnumerable<Animation> animations)
        {
            List<TimeRange> clusterRanges = new List<TimeRange>();
            foreach (Animation animation in animations)
            {
                TimeRange animationRange = new TimeRange(animation.StartTime, animation.EndTime);
                for (int i = clusterRanges.Count - 1; i >= 0; i--)
                {
                    if (clusterRanges[i].Overlaps(animationRange))
                    {
                        animationRange.UnionWith(clusterRanges[i]);
                        clusterRanges.RemoveAt(i);
                    }
                }
                clusterRanges.Add(animationRange);
            }
            return clusterRanges;
        }

        private static AssLine CreateInitialLine(AssLine originalLine, List<AnimationWithSectionIndex> anims)
        {
            AssLine newLine = (AssLine)originalLine.Clone();
            newLine.Start = TimeUtil.RoundTimeToFrameCenter(newLine.Start);

            foreach (AnimationWithSectionIndex anim in anims.Where(a => a.Animation.EndTime < originalLine.Start)
                                                            .OrderBy(a => a.Animation.EndTime))
            {
                ApplyAnimation(newLine, anim, 1);
            }

            foreach (AnimationWithSectionIndex anim in anims.Where(a => a.Animation.AffectsPast && a.Animation.StartTime >= originalLine.Start)
                                                            .OrderByDescending(a => a.Animation.StartTime))
            {
                ApplyAnimation(newLine, anim, 0);
            }

            return newLine;
        }

        private static AssLine CreatePostAnimationClusterLine(AssLine originalLine, AssLine lastLine, DateTime start, DateTime end, List<AnimationWithSectionIndex> animsWithSection)
        {
            AssLine newLine = (AssLine)lastLine.Clone();
            newLine.Start = start;
            newLine.End = end;
            foreach (AnimationWithSectionIndex animWithSection in animsWithSection.OrderBy(a => a.Animation.EndTime))
            {
                if (animWithSection.Animation.AffectsText)
                    ResetText(newLine, originalLine);

                ApplyAnimation(newLine, animWithSection, 1);
            }
            return newLine;
        }

        private static IEnumerable<AssLine> CreateFrameLines(AssDocument document, AssLine originalLine, TimeRange timeRange, List<AnimationWithSectionIndex> animations)
        {
            int rangeStartFrame = TimeUtil.StartTimeToFrame(timeRange.Start);
            int rangeEndFrame = TimeUtil.EndTimeToFrame(timeRange.End);

            const int frameStepSize = 2;
            int subStepFrames = (rangeEndFrame + 1 - rangeStartFrame) % frameStepSize;
            int lastIterationFrame = rangeEndFrame + 1 - subStepFrames - frameStepSize;

            bool needTextReset = animations.Any(a => a.Animation.AffectsText);

            AssLine frameLine = originalLine;
            for (int frame = rangeStartFrame; frame <= lastIterationFrame; frame += frameStepSize)
            {
                frameLine = (AssLine)frameLine.Clone();
                frameLine.Start = TimeUtil.FrameToStartTime(frame);
                frameLine.End = frame < lastIterationFrame ? TimeUtil.FrameToEndTime(frame + frameStepSize - 1) : timeRange.End;
                frameLine.Position = originalLine.Position ?? document.GetDefaultPosition(originalLine.AnchorPoint);
                if (needTextReset)
                    ResetText(frameLine, originalLine);

                float interpFrame = frame + (frameStepSize - 1) / 2.0f;

                foreach (AnimationWithSectionIndex animWithSection in animations)
                {
                    int animStartFrame = TimeUtil.StartTimeToFrame(animWithSection.Animation.StartTime);
                    int animEndFrame = TimeUtil.EndTimeToFrame(animWithSection.Animation.EndTime);
                    if (interpFrame >= animStartFrame && interpFrame < animEndFrame)
                    {
                        float t = (interpFrame - animStartFrame) / (animEndFrame - animStartFrame);
                        ApplyAnimation(frameLine, animWithSection, t);
                    }
                    else if (interpFrame >= animEndFrame && interpFrame < animEndFrame + frameStepSize)
                    {
                        ApplyAnimation(frameLine, animWithSection, 1);
                    }
                }
                yield return frameLine;
            }
        }

        private static void ApplyAnimation(AssLine line, AnimationWithSectionIndex animWithSectionIdx, float t)
        {
            animWithSectionIdx.Animation.Apply(line, animWithSectionIdx.SectionIndex >= 0 ? (AssSection)line.Sections[animWithSectionIdx.SectionIndex] : null, t);
        }

        private static void ResetText(AssLine frameLine, AssLine originalLine)
        {
            for (int i = 0; i < frameLine.Sections.Count; i++)
            {
                frameLine.Sections[i].Text = originalLine.Sections[i].Text;
            }
        }

        private struct AnimationWithSectionIndex
        {
            public AnimationWithSectionIndex(Animation animation, int sectionIndex)
            {
                Animation = animation;
                SectionIndex = sectionIndex;
            }

            public Animation Animation;
            public int SectionIndex;
        }
    }
}
