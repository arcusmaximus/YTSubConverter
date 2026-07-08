namespace YTSubConverter.Shared.Util
{
    public static class AnchorPointUtil
    {
        public static bool IsLeftAligned(AnchorPoint anchorPoint)
        {
            return anchorPoint is AnchorPoint.TopLeft or
                                  AnchorPoint.MiddleLeft or
                                  AnchorPoint.BottomLeft;
        }

        public static bool IsCenterAligned(AnchorPoint anchorPoint)
        {
            return anchorPoint is AnchorPoint.TopCenter or
                                  AnchorPoint.Center or
                                  AnchorPoint.BottomCenter;
        }

        public static bool IsRightAligned(AnchorPoint anchorPoint)
        {
            return anchorPoint is AnchorPoint.TopRight or
                                  AnchorPoint.MiddleRight or
                                  AnchorPoint.BottomRight;
        }

        public static bool IsTopAligned(AnchorPoint anchorPoint)
        {
            return anchorPoint is AnchorPoint.TopLeft or
                                  AnchorPoint.TopCenter or
                                  AnchorPoint.TopRight;
        }

        public static bool IsMiddleAligned(AnchorPoint anchorPoint)
        {
            return anchorPoint is AnchorPoint.MiddleLeft or
                                  AnchorPoint.Center or
                                  AnchorPoint.MiddleRight;
        }

        public static bool IsBottomAligned(AnchorPoint anchorPoint)
        {
            return anchorPoint is AnchorPoint.BottomLeft or
                                  AnchorPoint.BottomCenter or
                                  AnchorPoint.BottomRight;
        }

        public static AnchorPoint GetVerticalOpposite(AnchorPoint anchorPoint)
        {
            return anchorPoint switch
            {
                AnchorPoint.TopLeft         => AnchorPoint.BottomLeft,
                AnchorPoint.TopCenter       => AnchorPoint.BottomCenter,
                AnchorPoint.TopRight        => AnchorPoint.BottomRight,
                AnchorPoint.BottomLeft      => AnchorPoint.TopLeft,
                AnchorPoint.BottomCenter    => AnchorPoint.TopCenter,
                AnchorPoint.BottomRight     => AnchorPoint.TopRight,
                _ => anchorPoint,
            };
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
