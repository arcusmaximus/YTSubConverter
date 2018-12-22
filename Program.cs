using System;
using System.IO;
using System.Windows.Forms;
using Arc.YTSubConverter.Ass;

namespace Arc.YTSubConverter
{
    internal static class Program
    {
        [STAThread]
        internal static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                RunCommandLine(args);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        private static void RunCommandLine(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Too many arguments specified");
                return;
            }

            string filePath = args[0];
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Specified file not found");
                return;
            }

            try
            {
                SubtitleDocument doc = SubtitleDocument.Load(filePath);
                if (doc is AssDocument)
                {
                    doc = new AssDocument(filePath, AssStyleOptionsList.Load());
                    new YttDocument(doc).Save(Path.ChangeExtension(filePath, ".ytt"));
                }
                else
                {
                    new SrtDocument(doc).Save(Path.ChangeExtension(filePath, ".srt"));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex}");
            }
        }
    }
}
