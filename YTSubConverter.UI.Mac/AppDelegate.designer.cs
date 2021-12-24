// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace YTSubConverter.UI.Mac
{
	partial class AppDelegate
	{
		[Outlet]
		AppKit.NSMenuItem _miAutoconvert { get; set; }

		[Outlet]
		AppKit.NSMenuItem _miConvert { get; set; }

		[Outlet]
		AppKit.NSMenuItem _miHide { get; set; }

		[Outlet]
		AppKit.NSMenuItem _miHideOthers { get; set; }

		[Outlet]
		AppKit.NSMenuItem _miOpen { get; set; }

		[Outlet]
		AppKit.NSMenuItem _miQuit { get; set; }

		[Outlet]
		AppKit.NSMenuItem _miShowAll { get; set; }

		[Outlet]
		AppKit.NSMenu _mnuFile { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (_mnuFile != null) {
				_mnuFile.Dispose ();
				_mnuFile = null;
			}

			if (_miAutoconvert != null) {
				_miAutoconvert.Dispose ();
				_miAutoconvert = null;
			}

			if (_miConvert != null) {
				_miConvert.Dispose ();
				_miConvert = null;
			}

			if (_miHide != null) {
				_miHide.Dispose ();
				_miHide = null;
			}

			if (_miHideOthers != null) {
				_miHideOthers.Dispose ();
				_miHideOthers = null;
			}

			if (_miOpen != null) {
				_miOpen.Dispose ();
				_miOpen = null;
			}

			if (_miQuit != null) {
				_miQuit.Dispose ();
				_miQuit = null;
			}

			if (_miShowAll != null) {
				_miShowAll.Dispose ();
				_miShowAll = null;
			}
		}
	}
}
