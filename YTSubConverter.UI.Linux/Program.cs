using YTSubConverter.Shared;
using Gtk;

namespace YTSubConverter.UI.Linux
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Application.Init();
            if (args.Length > 0)
            {
                CommandLineHandler.Handle(args);
                return;
            }

            new MainWindow().Show();
            Application.Run();
        }
    }
}
