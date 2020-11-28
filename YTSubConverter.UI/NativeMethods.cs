using System;
using System.Runtime.InteropServices;

namespace Arc.YTSubConverter
{
    internal static class NativeMethods
    {
        [DllImport("user32", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("user32", CallingConvention = CallingConvention.StdCall)]
        public static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [DllImport("gdi32", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr CreateFontW(
            int height,
            int width,
            int escapement,
            int orientation,
            int weight,
            bool italic,
            bool underline,
            bool strikeout,
            int charset,
            int outputPrecision,
            int clipPrecision,
            int quality,
            int pitchAndFamily,
            [MarshalAs(UnmanagedType.LPWStr)] string face
        );

        [DllImport("gdi32", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);

        [DllImport("gdi32", CallingConvention = CallingConvention.StdCall)]
        public static extern int GetGlyphOutlineW(
            IntPtr hdc,
            int c,
            int format,
            ref GLYPHMETRICS metrics,
            int bufferSize,
            IntPtr pBuffer,
            ref MAT2 mat2
        );

        [DllImport("gdi32", CallingConvention = CallingConvention.StdCall)]
        public static extern bool DeleteObject(IntPtr h);

        [DllImport("kernel32", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr handle);

        public const int FW_NORMAL = 400;
        public const int FW_BOLD = 700;

        public const int DEFAULT_CHARSET = 1;
        public const int SHIFTJIS_CHARSET = 128;

        public const int OUT_DEFAULT_PRECIS = 0;
        public const int CLIP_DEFAULT_PRECIS = 0;

        public const int DEFAULT_QUALITY = 0;
        public const int ANTIALIASED_QUALITY = 4;

        public const int DEFAULT_PITCH = 0;
        public const int FF_DONTCARE = 0 << 4;

        public const int GGO_METRICS = 0;

        [StructLayout(LayoutKind.Sequential)]
        public struct GLYPHMETRICS
        {
            public int gmBlackBoxX;
            public int gmBlackBoxY;
            public POINT gmptGlyphOrigin;
            public short gmCellIncX;
            public short gmCellIncY;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MAT2
        {
            public MAT2(float em11, float em12, float em21, float em22)
            {
                eM11 = new FIXED(em11);
                eM12 = new FIXED(em12);
                eM21 = new FIXED(em21);
                eM22 = new FIXED(em22);
            }

            public FIXED eM11;
            public FIXED eM12;
            public FIXED eM21;
            public FIXED eM22;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FIXED
        {
            public FIXED(float value)
            {
                Value = checked((short)value);
                Fract = checked((ushort)Math.Abs(value - Value));
            }

            public ushort Fract;
            public short Value;
        }
    }
}
