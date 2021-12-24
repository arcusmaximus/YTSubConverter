using System;
using System.Xml;
using YTSubConverter.Shared.Util;

namespace YTSubConverter.Shared.Formats.Ttml
{
    public class TtmlTimeRange
    {
        public static TtmlTimeRange Get(XmlElement elem, TtmlDocument doc)
        {
            DateTime? begin = GetTime(elem, "begin", doc);
            TimeSpan? duration = GetTime(elem, "dur", doc) - SubtitleDocument.TimeBase;
            DateTime? end = GetTime(elem, "end", doc);
            TtmlTimeContainer timeContainer = elem.GetEnumAttribute<TtmlTimeContainer>("timeContainer") ?? TtmlTimeContainer.Par;

            if (begin == null && duration == null && end == null)
                return null;

            begin ??= SubtitleDocument.TimeBase;

            if (duration != null && (end == null || begin.Value + duration.Value < end.Value))
                end = begin + duration;

            end ??= DateTime.MaxValue;

            return new TtmlTimeRange
                   {
                       Begin = begin.Value,
                       End = end.Value,
                       TimeContainer = timeContainer
                   };
        }

        public DateTime Begin
        {
            get;
            set;
        }

        public DateTime End
        {
            get;
            set;
        }

        public TtmlTimeContainer TimeContainer
        {
            get;
            set;
        }

        internal (DateTime, DateTime) Resolve(TtmlResolutionContext parentContext, TtmlResolutionContext prevSiblingContext)
        {
            DateTime reference = TimeContainer == TtmlTimeContainer.Par ? parentContext.BeginTime : prevSiblingContext?.EndTime ?? parentContext.BeginTime;
            DateTime begin = reference + (Begin - SubtitleDocument.TimeBase);
            DateTime end = End < DateTime.MaxValue ? reference + (End - SubtitleDocument.TimeBase) : DateTime.MaxValue;
            if (end > parentContext.EndTime)
                end = parentContext.EndTime;

            return (begin, end);
        }

        private static DateTime? GetTime(XmlElement elem, string attr, TtmlDocument doc)
        {
            return elem.GetTypedAttribute(
                attr,
                string.Empty,
                (string textValue, out DateTime parsedValue) => TtmlTime.TryParse(textValue, doc.FrameRate, doc.SubFrameRate, doc.TickRate, out parsedValue)
            );
        }
    }
}
