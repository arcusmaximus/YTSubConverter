using AppKit;
using Foundation;

namespace YTSubConverter.UI.Mac
{
    [Register("AppDelegate")]
    public partial class AppDelegate : NSApplicationDelegate
    {
        public AppDelegate()
        {
        }

        public override void DidFinishLaunching(NSNotification notification)
        {
            LocalizeMenu();
        }

        public NSMenuItem OpenMenuItem => _miOpen;
        public NSMenuItem ConvertMenuItem => _miConvert;
        public NSMenuItem AutoconvertMenuItem => _miAutoconvert;

        private void LocalizeMenu()
        {
            _miHide.Title = Resources.HideApplication;
            _miHideOthers.Title = Resources.HideOthers;
            _miShowAll.Title = Resources.ShowAll;
            _miQuit.Title = Resources.QuitApplication;

            _mnuFile.Title = Resources.File;
            _miOpen.Title = Resources.Open;
            _miConvert.Title = Shared.Resources.Convert;
            _miAutoconvert.Title = Shared.Resources.Autoconvert;
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
