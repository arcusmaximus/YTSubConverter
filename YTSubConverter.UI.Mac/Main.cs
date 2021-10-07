using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using AppKit;
using YTSubConverter.Shared;
using Foundation;

namespace YTSubConverter.UI.Mac
{
    public static class MainClass
    {
        public static void Main(string[] args)
        {
            NSApplication.Init();
            InitCulture();

            if (args.Length > 0)
            {
                CommandLineHandler.Handle(args);
                return;
            }

            NSApplication.Main(args);
        }

        private static void InitCulture()
        {
            string baseFolderPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            foreach (string language in NSLocale.PreferredLanguages.SelectMany(l => new[] { l, Regex.Replace(l, @"-\w+$", "") }))
            {
                if (language == "en" || Directory.Exists(Path.Combine(baseFolderPath, language)))
                {
                    CultureInfo culture = CultureInfo.GetCultureInfo(language);
                    Thread.CurrentThread.CurrentCulture = culture;
                    Thread.CurrentThread.CurrentUICulture = culture;
                    break;
                }
            }
        }
    }
}
