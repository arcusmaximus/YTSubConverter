using System.IO;

namespace YTSubConverter.Shared.Util
{
    internal static class PathUtil
    {
        public static string ToSafePath(string filePath)
        {
            if (OperatingSystem.IsWindows && !filePath.StartsWith(@"\\?\"))
                filePath = @"\\?\" + Path.GetFullPath(filePath);

            return filePath;
        }
    }
}
