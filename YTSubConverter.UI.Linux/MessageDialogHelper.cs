using Gtk;

namespace YTSubConverter.UI.Linux
{
    internal static class MessageDialogHelper
    {
        public static void Show(string text, MessageType type, ButtonsType buttons, Window parentWindow)
        {
            var dialog = new MessageDialog(parentWindow, DialogFlags.Modal, type, buttons, null)
                         {
                             Text = text
                         };
            dialog.Run();
            dialog.Destroy();
        }
    }
}
