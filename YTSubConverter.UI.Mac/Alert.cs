using AppKit;

namespace YTSubConverter.UI.Mac
{
    internal static class Alert
    {
        public static void Show(string message, NSAlertStyle style)
        {
            NSAlert alert = new NSAlert
                            {
                                MessageText = message,
                                AlertStyle = style
                            };
            alert.RunModal();
        }
    }
}
