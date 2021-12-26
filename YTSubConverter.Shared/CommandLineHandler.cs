using System;
using System.Collections.Generic;
using System.IO;
using YTSubConverter.Shared.Formats;

namespace YTSubConverter.Shared
{
    public static class CommandLineHandler
    {
        public static void Handle(string[] args)
        {
            Arguments parsedArgs = ParseArguments(args);
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
                SubtitleDocument destinationDoc = SubtitleDocument.Convert(sourceDoc, Path.GetExtension(parsedArgs.DestinationFilePath), parsedArgs.ForVisualization);
                destinationDoc.Save(parsedArgs.DestinationFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
            }
        }

        private static Arguments ParseArguments(string[] args)
        {
            Arguments parsedArgs = new Arguments();

            List<string> filePaths = new List<string>();
            foreach (string arg in args)
            {
                if (arg.StartsWith("-"))
                {
                    switch (arg)
                    {
                        case "--visual":
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
                        ".sbv" => ".srt",
                        ".ytt" => ".reverse.ass",
                        ".srv3" => ".ass",
                        _ => ".ytt"
                    };
                parsedArgs.DestinationFilePath = Path.ChangeExtension(parsedArgs.SourceFilePath, destinationExtension);
            }
            else
            {
                parsedArgs.DestinationFilePath = filePaths[1];
            }

            return parsedArgs;
        }

        private class Arguments
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
