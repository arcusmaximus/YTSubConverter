using System;

namespace YTSubConverter.Shared.Formats.Ttml
{
    internal class TtmlResolutionContext
    {
        public TtmlResolutionContext(TtmlDocument doc)
        {
            Document = doc;
        }

        public TtmlDocument Document
        {
            get;
        }

        public DateTime BeginTime
        {
            get;
            set;
        }

        public DateTime EndTime
        {
            get;
            set;
        }

        public TtmlStyle StylingStyle
        {
            get;
            set;
        }

        public TtmlStyle RegionStyle
        {
            get;
            set;
        }

        public TtmlStyle Style
        {
            get;
            set;
        }

        public static TtmlResolutionContext CreateInitialContext(TtmlDocument doc)
        {
            return new TtmlResolutionContext(doc)
                   {
                       BeginTime = SubtitleDocument.TimeBase,
                       EndTime = DateTime.MaxValue,
                       RegionStyle = doc.InitialStyle,
                       StylingStyle = doc.InitialStyle,
                       Style = doc.InitialStyle
                   };
        }

        public static TtmlResolutionContext Extend(TtmlResolutionContext parentContext, TtmlResolutionContext prevSiblingContext, TtmlContent content)
        {
            TtmlResolutionContext context = parentContext.Clone();

            if (content.TimeRange != null)
                (context.BeginTime, context.EndTime) = content.TimeRange.Resolve(parentContext, prevSiblingContext);

            if (content.Region?.Style != null)
                context.RegionStyle = content.Region.Style.CloneUsingNewInitialStyle(parentContext.RegionStyle);

            if (content.Style != null)
                context.StylingStyle = content.Style.CloneUsingNewInitialStyle(parentContext.StylingStyle);

            context.Style = context.StylingStyle.CloneUsingNewInitialStyle(context.RegionStyle);

            return context;
        }

        public TtmlResolutionContext Clone()
        {
            return new TtmlResolutionContext(Document)
                   {
                       BeginTime = BeginTime,
                       EndTime = EndTime,
                       StylingStyle = StylingStyle,
                       RegionStyle = RegionStyle,
                       Style = Style
                   };
        }
    }
}
