using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Arc.YTSubConverter.Formats;
using Arc.YTSubConverter.Formats.Ass;

namespace Arc.YTSubConverter
{
    internal static class Program
    {
        [STAThread]
        internal static void Main(string[] args)
        {
            PreloadResources();

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
                if (Path.GetExtension(filePath).Equals(".ass", StringComparison.InvariantCultureIgnoreCase))
                {
                    SubtitleDocument doc = new AssDocument(filePath, AssStyleOptionsList.Load());
                    new YttDocument(doc).Save(Path.ChangeExtension(filePath, ".ytt"));
                }
                else
                {
                    SubtitleDocument doc = SubtitleDocument.Load(filePath);
                    new SrtDocument(doc).Save(Path.ChangeExtension(filePath, ".srt"));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex}");
            }
        }

        /// <summary>
        /// Manually load the resources available in the .exe so the ILMerged release build doesn't need satellite assemblies anymore
        /// </summary>
        private static void PreloadResources()
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            FieldInfo resourceSetsField = typeof(ResourceManager).GetField("_resourceSets", BindingFlags.NonPublic | BindingFlags.Instance);
            Dictionary<string, ResourceSet> resourceSets = (Dictionary<string, ResourceSet>)resourceSetsField.GetValue(Resources.ResourceManager);

            foreach (string resourceName in assembly.GetManifestResourceNames())
            {
                Match match = Regex.Match(resourceName, "^" + Regex.Escape(typeof(Resources).FullName) + @"\.([-\w]+)\.resources$");
                if (!match.Success)
                    continue;

                string culture = match.Groups[1].Value;
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    ResourceSet resSet = new ResourceSet(stream);
                    resourceSets.Add(culture, resSet);
                }
            }
        }
    }
}
