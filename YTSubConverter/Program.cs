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
            CommandLineArguments parsedArgs = ParseArguments(args);
            if (parsedArgs == null)
                return;

            if (!File.Exists(parsedArgs.SourceFilePath))
            {
                Console.WriteLine("Specified source file not found");
                return;
            }

            try
            {
                SubtitleDocument sourceDoc = SubtitleDocument.Load(parsedArgs.SourceFilePath);
                SubtitleDocument destinationDoc =
                    Path.GetExtension(parsedArgs.DestinationFilePath).ToLower() switch
                    {
                        ".ass" => parsedArgs.ForVisualization ? new VisualizingAssDocument(sourceDoc) : new AssDocument(sourceDoc),
                        ".srv3" => new YttDocument(sourceDoc),
                        ".ytt" => new YttDocument(sourceDoc),
                        _ => new SrtDocument(sourceDoc)
                    };
                destinationDoc.Save(parsedArgs.DestinationFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex}");
            }
        }

        private static CommandLineArguments ParseArguments(string[] args)
        {
            CommandLineArguments parsedArgs = new CommandLineArguments();

            List<string> filePaths = new List<string>();
            foreach (string arg in args)
            {
                if (arg.StartsWith("-"))
                {
                    switch (arg)
                    {
                        case "-viz":
                            parsedArgs.ForVisualization = true;
                            break;
                    }
                }
                else
                {
                    filePaths.Add(arg);
                }
            }

            if (filePaths.Count == 0)
            {
                Console.WriteLine("Please specify a source file.");
                return null;
            }

            if (filePaths.Count > 2)
            {
                Console.WriteLine("Too many file paths specified.");
                return null;
            }

            parsedArgs.SourceFilePath = filePaths[0];
            if (filePaths.Count == 1)
            {
                string destinationExtension =
                    Path.GetExtension(parsedArgs.SourceFilePath).ToLower() switch
                    {
                        ".ass" => ".ytt",
                        ".ytt" => ".reverse.ass",
                        ".srv3" => ".ass",
                        _ => ".srt"
                    };
                parsedArgs.DestinationFilePath = Path.ChangeExtension(parsedArgs.SourceFilePath, destinationExtension);
            }
            else
            {
                parsedArgs.DestinationFilePath = filePaths[1];
            }

            return parsedArgs;
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
                using Stream stream = assembly.GetManifestResourceStream(resourceName);
                ResourceSet resSet = new ResourceSet(stream);
                resourceSets.Add(culture, resSet);
            }
        }

        private class CommandLineArguments
        {
            public bool ForVisualization
            {
                get;
                set;
            }

            public string SourceFilePath
            {
                get;
                set;
            }


            public string DestinationFilePath
            {
                get;
                set;
            }
        }
    }
}
