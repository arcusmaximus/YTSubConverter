using YTSubConverter.Shared;
using Gtk;

namespace YTSubConverter.UI.Linux
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Application.Init("ytsubconverter", ref args);
            if (args.Length > 0)
            {
                using GtkTextMeasurer textMeasurer = new();
                CommandLineHandler.Handle(args, textMeasurer);
                return;
            }
            
            using var window = new MainWindow();
            window.Show();
            Application.Run();
        }
    }
}
