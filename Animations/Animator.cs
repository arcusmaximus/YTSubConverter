using System;
using System.Collections.Generic;
using System.Linq;
using Arc.YTSubConverter.Formats.Ass;
using Arc.YTSubConverter.Util;

namespace Arc.YTSubConverter.Animations
{
    internal static class Animator
    {
        public static IEnumerable<AssLine> Expand(AssLine originalLine)
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
                foreach (AssLine frameLine in CreateFrameLines(lastLine, clusterRange, clusterAnims))
                {
                    lastLine.End = frameLine.Start;
                    yield return lastLine = frameLine;
                }

                DateTime interAnimStart = TimeUtil.FrameToTime(TimeUtil.TimeToFrame(clusterRange.End) + 1);
                DateTime interAnimEnd = i < animClusters.Count - 1 ? animClusters.Keys[i + 1].Start : originalLine.End;
                if (interAnimEnd > interAnimStart)
                    yield return lastLine = CreatePostAnimationClusterLine(lastLine, interAnimStart, interAnimEnd, clusterAnims);
            }

            lastLine.End = originalLine.End;
        }

        private static List<AnimationWithSectionIndex> GetAnimationsWithSectionIndex(AssLine line)
        {
            List<AnimationWithSectionIndex> animations = new List<AnimationWithSectionIndex>();

            foreach (Animation anim in line.Animations)
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
                if (!clusterRange.Intersects(lineRange))
                    continue;

                List<AnimationWithSectionIndex> clusterAnims = new List<AnimationWithSectionIndex>();
                foreach (AnimationWithSectionIndex animWithSection in allAnims)
                {
                    if (clusterRange.Contains(animWithSection.Animation.StartTime) && animWithSection.Animation.EndTime > animWithSection.Animation.StartTime)
                        clusterAnims.Add(animWithSection);
                }

                clusterRange.IntersectWith(lineRange);
                clusterRange.Start = TimeUtil.SnapTimeToFrame(clusterRange.Start.AddMilliseconds(32));
                clusterRange.End = TimeUtil.SnapTimeToFrame(clusterRange.End).AddMilliseconds(32);
                clusters.Add(clusterRange, clusterAnims);
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
                    if (clusterRanges[i].Intersects(animationRange))
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

            foreach (AnimationWithSectionIndex anim in anims.Where(a => a.Animation.EndTime < originalLine.Start)
                                                            .OrderBy(a => a.Animation.EndTime))
            {
                ApplyAnimation(newLine, anim, 1);
            }

            foreach (AnimationWithSectionIndex anim in anims.Where(a => a.Animation.AffectsPast && a.Animation.StartTime > originalLine.End)
                                                            .OrderByDescending(a => a.Animation.StartTime))
            {
                ApplyAnimation(newLine, anim, 0);
            }

            return newLine;
        }

        private static AssLine CreatePostAnimationClusterLine(AssLine originalLine, DateTime start, DateTime end, List<AnimationWithSectionIndex> animsWithSection)
        {
            AssLine newLine = (AssLine)originalLine.Clone();
            newLine.Start = start;
            newLine.End = end;
            foreach (AnimationWithSectionIndex animWithSection in animsWithSection.OrderBy(a => a.Animation.EndTime))
            {
                ApplyAnimation(newLine, animWithSection, 1);
            }
            return newLine;
        }

        private static IEnumerable<AssLine> CreateFrameLines(AssLine originalLine, TimeRange timeRange, List<AnimationWithSectionIndex> animations)
        {
            int rangeStartFrame = TimeUtil.TimeToFrame(timeRange.Start);
            int rangeEndFrame = TimeUtil.TimeToFrame(timeRange.End);

            const int frameStepSize = 2;
            int lastIterationFrame = rangeStartFrame + (rangeEndFrame - 1 - rangeStartFrame) / frameStepSize * frameStepSize;
            if (lastIterationFrame == rangeStartFrame)
                yield break;

            AssLine frameLine = originalLine;
            for (int frame = rangeStartFrame; frame <= lastIterationFrame; frame += frameStepSize)
            {
                frameLine = (AssLine)frameLine.Clone();
                frameLine.Start = TimeUtil.FrameToTime(frame);
                frameLine.End = TimeUtil.FrameToTime(Math.Min(frame + frameStepSize, rangeEndFrame));

                int interpFrame = frame + frameStepSize / 2;

                foreach (AnimationWithSectionIndex animWithSection in animations)
                {
                    int animStartFrame = TimeUtil.TimeToFrame(animWithSection.Animation.StartTime);
                    int animEndFrame = TimeUtil.TimeToFrame(animWithSection.Animation.EndTime);
                    if (interpFrame >= animStartFrame && interpFrame < animEndFrame)
                    {
                        float t = (float)(interpFrame - animStartFrame) / (animEndFrame - animStartFrame);
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
