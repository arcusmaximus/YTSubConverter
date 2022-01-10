using YTSubConverter.Shared;
using Gtk;

namespace YTSubConverter.UI.Linux
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                CommandLineHandler.Handle(args);
                return;
            }

            Application.Init();
            new MainWindow().Show();
            Application.Run();
        }
    }
}
