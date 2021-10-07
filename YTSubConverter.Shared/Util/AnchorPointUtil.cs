namespace YTSubConverter.Shared.Util
{
    public static class AnchorPointUtil
    {
        public static bool IsLeftAligned(AnchorPoint anchorPoint)
        {
            return anchorPoint == AnchorPoint.TopLeft ||
                   anchorPoint == AnchorPoint.MiddleLeft ||
                   anchorPoint == AnchorPoint.BottomLeft;
        }

        public static bool IsCenterAligned(AnchorPoint anchorPoint)
        {
            return anchorPoint == AnchorPoint.TopCenter ||
                   anchorPoint == AnchorPoint.Center ||
                   anchorPoint == AnchorPoint.BottomCenter;
        }

        public static bool IsRightAligned(AnchorPoint anchorPoint)
        {
            return anchorPoint == AnchorPoint.TopRight ||
                   anchorPoint == AnchorPoint.MiddleRight ||
                   anchorPoint == AnchorPoint.BottomRight;
        }

        public static bool IsTopAligned(AnchorPoint anchorPoint)
        {
            return anchorPoint == AnchorPoint.TopLeft ||
                   anchorPoint == AnchorPoint.TopCenter ||
                   anchorPoint == AnchorPoint.TopRight;
        }

        public static bool IsMiddleAligned(AnchorPoint anchorPoint)
        {
            return anchorPoint == AnchorPoint.MiddleLeft ||
                   anchorPoint == AnchorPoint.Center ||
                   anchorPoint == AnchorPoint.MiddleRight;
        }

        public static bool IsBottomAligned(AnchorPoint anchorPoint)
        {
            return anchorPoint == AnchorPoint.BottomLeft ||
                   anchorPoint == AnchorPoint.BottomCenter ||
                   anchorPoint == AnchorPoint.BottomRight;
        }

        public static AnchorPoint GetVerticalOpposite(AnchorPoint anchorPoint)
        {
            switch (anchorPoint)
            {
                case AnchorPoint.TopLeft:
                    return AnchorPoint.BottomLeft;

                case AnchorPoint.TopCenter:
                    return AnchorPoint.BottomCenter;

                case AnchorPoint.TopRight:
                    return AnchorPoint.BottomRight;

                case AnchorPoint.BottomLeft:
                    return AnchorPoint.TopLeft;

                case AnchorPoint.BottomCenter:
                    return AnchorPoint.TopCenter;

                case AnchorPoint.BottomRight:
                    return AnchorPoint.TopRight;

                default:
                    return anchorPoint;
            }
        }

        public static AnchorPoint MakeTopAligned(AnchorPoint anchorPoint)
        {
            switch (anchorPoint)
            {
                case AnchorPoint.MiddleLeft:
                case AnchorPoint.BottomLeft:
                    return AnchorPoint.TopLeft;

                case AnchorPoint.Center:
                case AnchorPoint.BottomCenter:
                    return AnchorPoint.TopCenter;

                case AnchorPoint.MiddleRight:
                case AnchorPoint.BottomRight:
                    return AnchorPoint.TopRight;

                default:
                    return anchorPoint;
            }
        }

        public static AnchorPoint MakeBottomAligned(AnchorPoint anchorPoint)
        {
            switch (anchorPoint)
            {
                case AnchorPoint.TopLeft:
                case AnchorPoint.MiddleLeft:
                    return AnchorPoint.BottomLeft;

                case AnchorPoint.TopCenter:
                case AnchorPoint.Center:
                    return AnchorPoint.BottomCenter;

                case AnchorPoint.TopRight:
                case AnchorPoint.MiddleRight:
                    return AnchorPoint.BottomRight;

                default:
                    return anchorPoint;
            }
        }
    }
}
