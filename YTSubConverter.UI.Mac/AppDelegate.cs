using AppKit;
using Foundation;

namespace Arc.YTSubConverter.UI.Mac
{
    [Register("AppDelegate")]
    public class AppDelegate : NSApplicationDelegate
    {
        public AppDelegate()
        {
        }

        public override void DidFinishLaunching(NSNotification notification)
        {
        }

        public override void WillTerminate(NSNotification notification)
        {
        }

        public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication sender)
        {
            return true;
        }
    }
}
