using System;
using System.Collections.Generic;
using System.Linq;
using Arc.YTSubConverter.Formats.Ass;
using Arc.YTSubConverter.Util;

namespace Arc.YTSubConverter.Animations
{
    internal static class Animator
    {
        public static IEnumerable<AssDocument.ExtendedLine> Expand(AssDocument.ExtendedLine originalLine)
        {
            SortedList<TimeRange, List<AnimationWithSectionIndex>> animClusters = ClusterAnimations(originalLine);
            if (animClusters.Count == 0)
            {
                yield return originalLine;
                yield break;
            }

            AssDocument.ExtendedLine lastLine = CreatePreAnimationClusterLine(originalLine, originalLine.Start, animClusters.Keys[0].Start, animClusters.Values[0]);
            if (lastLine.End > lastLine.Start)
                yield return lastLine;

            for (int i = 0; i < animClusters.Count; i++)
            {
                TimeRange clusterRange = animClusters.Keys[i];
                List<AnimationWithSectionIndex> clusterAnims = animClusters.Values[i];
                foreach (AssDocument.ExtendedLine frameLine in CreateFrameLines(lastLine, clusterRange, clusterAnims))
                {
                    yield return lastLine = frameLine;
                }

                DateTime interAnimStart = TimeUtil.FrameToTime(TimeUtil.TimeToFrame(clusterRange.End) + 1);
                DateTime interAnimEnd = i < animClusters.Count - 1 ? animClusters.Keys[i + 1].Start : originalLine.End;
                if (interAnimEnd > interAnimStart)
                    yield return lastLine = CreatePostAnimationClusterLine(lastLine, interAnimStart, interAnimEnd, clusterAnims);
            }

            lastLine.End = originalLine.End;
        }

        private static SortedList<TimeRange, List<AnimationWithSectionIndex>> ClusterAnimations(AssDocument.ExtendedLine line)
        {
            List<AnimationWithSectionIndex> allAnims = GetAnimationsWithSectionIndex(line);
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

        private static List<AnimationWithSectionIndex> GetAnimationsWithSectionIndex(AssDocument.ExtendedLine line)
        {
            List<AnimationWithSectionIndex> animations = new List<AnimationWithSectionIndex>();

            foreach (Animation anim in line.Animations)
            {
                animations.Add(new AnimationWithSectionIndex(anim, -1));
            }

            for (int i = 0; i < line.Sections.Count; i++)
            {
                AssDocument.ExtendedSection section = (AssDocument.ExtendedSection)line.Sections[i];
                foreach (Animation anim in section.Animations)
                {
                    animations.Add(new AnimationWithSectionIndex(anim, i));
                }
            }

            return animations;
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

        private static AssDocument.ExtendedLine CreatePreAnimationClusterLine(AssDocument.ExtendedLine originalLine, DateTime start, DateTime end, List<AnimationWithSectionIndex> animsWithSection)
        {
            AssDocument.ExtendedLine newLine = (AssDocument.ExtendedLine)originalLine.Clone();
            newLine.Start = start;
            newLine.End = end;
            foreach (AnimationWithSectionIndex animWithSection in animsWithSection.OrderByDescending(a => a.Animation.StartTime))
            {
                ApplyAnimation(newLine, animWithSection, 0);
            }
            return newLine;
        }

        private static AssDocument.ExtendedLine CreatePostAnimationClusterLine(AssDocument.ExtendedLine originalLine, DateTime start, DateTime end, List<AnimationWithSectionIndex> animsWithSection)
        {
            AssDocument.ExtendedLine newLine = (AssDocument.ExtendedLine)originalLine.Clone();
            newLine.Start = start;
            newLine.End = end;
            foreach (AnimationWithSectionIndex animWithSection in animsWithSection.OrderBy(a => a.Animation.EndTime))
            {
                ApplyAnimation(newLine, animWithSection, 1);
            }
            return newLine;
        }

        private static IEnumerable<AssDocument.ExtendedLine> CreateFrameLines(AssDocument.ExtendedLine originalLine, TimeRange timeRange, List<AnimationWithSectionIndex> animations)
        {
            int rangeStartFrame = TimeUtil.TimeToFrame(timeRange.Start);
            int rangeEndFrame = TimeUtil.TimeToFrame(timeRange.End);

            const int frameStepSize = 2;
            int lastIterationFrame = rangeStartFrame + (rangeEndFrame - 1 - rangeStartFrame) / frameStepSize * frameStepSize;
            if (lastIterationFrame == rangeStartFrame)
                yield break;

            AssDocument.ExtendedLine frameLine = originalLine;
            for (int frame = rangeStartFrame; frame <= lastIterationFrame; frame += frameStepSize)
            {
                frameLine = (AssDocument.ExtendedLine)frameLine.Clone();
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

        private static void ApplyAnimation(AssDocument.ExtendedLine line, AnimationWithSectionIndex animWithSectionIdx, float t)
        {
            animWithSectionIdx.Animation.Apply(line, animWithSectionIdx.SectionIndex >= 0 ? (AssDocument.ExtendedSection)line.Sections[animWithSectionIdx.SectionIndex] : null, t);
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
