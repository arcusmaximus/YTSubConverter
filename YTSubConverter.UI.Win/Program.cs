using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Arc.YTSubConverter.Shared;

namespace Arc.YTSubConverter.UI.Win
{
    internal static class Program
    {
        [STAThread]
        internal static void Main(string[] args)
        {
            PreloadResources();

            if (args.Length > 0)
            {
                AttachConsole(ATTACH_PARENT_PROCESS);
                CommandLineHandler.Handle(args);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        /// <summary>
        /// Manually load the resources available in the .exe so the ILMerged release build doesn't need satellite assemblies anymore
        /// </summary>
        private static void PreloadResources()
        {
            PreloadResources<Resources>(Resources.ResourceManager);
        }

        private static void PreloadResources<TResources>(ResourceManager resourceManager)
        {
            Assembly assembly = typeof(TResources).Assembly;
            FieldInfo resourceSetsField = typeof(ResourceManager).GetField("_resourceSets", BindingFlags.NonPublic | BindingFlags.Instance);
            Dictionary<string, ResourceSet> resourceSets = (Dictionary<string, ResourceSet>)resourceSetsField.GetValue(resourceManager);

            foreach (string resourceName in assembly.GetManifestResourceNames())
            {
                Match match = Regex.Match(resourceName, Regex.Escape(typeof(TResources).FullName) + @"\.([-\w]+)\.resources$");
                if (!match.Success)
                    continue;

                string culture = match.Groups[1].Value;
                using Stream stream = assembly.GetManifestResourceStream(resourceName);
                ResourceSet resSet = new ResourceSet(stream);
                resourceSets.Add(culture, resSet);
            }
        }

        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);

        private const int ATTACH_PARENT_PROCESS = -1;
    }
}
