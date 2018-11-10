using System;
using System.IO;
using System.Reflection;

namespace Arc.YTSubConverter
{
    internal static class Program
    {
        internal static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                PrintUsage();
                return;
            }

            string filePath = args[0];
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Specified file does not exist");
                return;
            }

            switch (Path.GetExtension(filePath))
            {
                case ".sbv":
                    Convert(filePath, d => new SrtDocument(d), ".srt", false);
                    break;

                case ".ass":
                    Convert(filePath, d => new RtDocument(d), ".rt", true);
                    break;
            }
        }

        private static void Convert(string filePath, Func<SubtitleDocument, SubtitleDocument> convert, string newExtension, bool forPublish)
        {
            SubtitleDocument sourceDoc = SubtitleDocument.Load(filePath);
            SubtitleDocument targetDoc = convert(sourceDoc);
            if (forPublish)
            {
                targetDoc.Shift(new TimeSpan(0, 0, 0, 0, -60));
                targetDoc.CloseGaps();
            }
            targetDoc.Save(Path.ChangeExtension(filePath, newExtension));
        }

        private static void PrintUsage()
        {
            string assemblyName = Assembly.GetEntryAssembly().GetName().Name;
            Console.WriteLine("Usage:");
            Console.WriteLine($"    Convert .sbv to .srt: {assemblyName} file.sbv");
            Console.WriteLine($"    Convert .ass to .rt: {assemblyName} file.ass");
        }
    }
}
