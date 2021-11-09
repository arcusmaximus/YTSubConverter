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
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		WebKit.WKWebView _brwPreview { get; set; }

		[Outlet]
		AppKit.NSButton _btnConvert { get; set; }

		[Outlet]
		AppKit.NSColorWell _btnCurrentWordOutlineColor { get; set; }

		[Outlet]
		AppKit.NSColorWell _btnCurrentWordShadowColor { get; set; }

		[Outlet]
		AppKit.NSColorWell _btnCurrentWordTextColor { get; set; }

		[Outlet]
		AppKit.NSButton _chkAutoConvert { get; set; }

		[Outlet]
		AppKit.NSButton _chkBevel { get; set; }

		[Outlet]
		AppKit.NSButton _chkGlow { get; set; }

		[Outlet]
		AppKit.NSButton _chkHardShadow { get; set; }

		[Outlet]
		AppKit.NSButton _chkHighlightCurrentWord { get; set; }

		[Outlet]
		AppKit.NSButton _chkKaraoke { get; set; }

		[Outlet]
		AppKit.NSButton _chkSoftShadow { get; set; }

		[Outlet]
		AppKit.NSBox _grpStyleOptions { get; set; }

		[Outlet]
		AppKit.NSTextField _lblConversionSuccess { get; set; }

		[Outlet]
		AppKit.NSTextField _lblCurrentWordOutlineColor { get; set; }

		[Outlet]
		AppKit.NSTextField _lblCurrentWordShadowColor { get; set; }

		[Outlet]
		AppKit.NSTextField _lblCurrentWordTextColor { get; set; }

		[Outlet]
		AppKit.NSTextField _lblInputFile { get; set; }

		[Outlet]
		AppKit.NSTextField _lblShadowTypes { get; set; }

		[Outlet]
		AppKit.NSTableView _lstStyles { get; set; }

		[Outlet]
		AppKit.NSBox _pnlOptions { get; set; }

		[Outlet]
		AppKit.NSBox _pnlShadowTypes { get; set; }

		[Action ("_btnBrowse_Click:")]
		partial void _btnBrowse_Click (Foundation.NSObject sender);

		[Action ("_btnConvert_Click:")]
		partial void _btnConvert_Click (Foundation.NSObject sender);

		[Action ("_btnCurrentWordOutlineColor_ColorChanged:")]
		partial void _btnCurrentWordOutlineColor_ColorChanged (Foundation.NSObject sender);

		[Action ("_btnCurrentWordShadowColor_ColorChanged:")]
		partial void _btnCurrentWordShadowColor_ColorChanged (Foundation.NSObject sender);

		[Action ("_btnCurrentWordTextColor_ColorChanged:")]
		partial void _btnCurrentWordTextColor_ColorChanged (Foundation.NSObject sender);

		[Action ("_chkAutoConvert_CheckedChanged:")]
		partial void _chkAutoConvert_CheckedChanged (Foundation.NSObject sender);

		[Action ("_chkBevel_CheckedChanged:")]
		partial void _chkBevel_CheckedChanged (Foundation.NSObject sender);

		[Action ("_chkGlow_CheckedChanged:")]
		partial void _chkGlow_CheckedChanged (Foundation.NSObject sender);

		[Action ("_chkHardShadow_CheckedChanged:")]
		partial void _chkHardShadow_CheckedChanged (Foundation.NSObject sender);

		[Action ("_chkHighlightCurrentWord_CheckedChanged:")]
		partial void _chkHighlightCurrentWord_CheckedChanged (Foundation.NSObject sender);

		[Action ("_chkKaraoke_CheckedChanged:")]
		partial void _chkKaraoke_CheckedChanged (Foundation.NSObject sender);

		[Action ("_chkSoftShadow_CheckedChanged:")]
		partial void _chkSoftShadow_CheckedChanged (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (_brwPreview != null) {
				_brwPreview.Dispose ();
				_brwPreview = null;
			}

			if (_btnConvert != null) {
				_btnConvert.Dispose ();
				_btnConvert = null;
			}

			if (_chkAutoConvert != null) {
				_chkAutoConvert.Dispose ();
				_chkAutoConvert = null;
			}

			if (_chkBevel != null) {
				_chkBevel.Dispose ();
				_chkBevel = null;
			}

			if (_chkGlow != null) {
				_chkGlow.Dispose ();
				_chkGlow = null;
			}

			if (_chkHardShadow != null) {
				_chkHardShadow.Dispose ();
				_chkHardShadow = null;
			}

			if (_chkHighlightCurrentWord != null) {
				_chkHighlightCurrentWord.Dispose ();
				_chkHighlightCurrentWord = null;
			}

			if (_chkKaraoke != null) {
				_chkKaraoke.Dispose ();
				_chkKaraoke = null;
			}

			if (_chkSoftShadow != null) {
				_chkSoftShadow.Dispose ();
				_chkSoftShadow = null;
			}

			if (_grpStyleOptions != null) {
				_grpStyleOptions.Dispose ();
				_grpStyleOptions = null;
			}

			if (_lblConversionSuccess != null) {
				_lblConversionSuccess.Dispose ();
				_lblConversionSuccess = null;
			}

			if (_lblCurrentWordOutlineColor != null) {
				_lblCurrentWordOutlineColor.Dispose ();
				_lblCurrentWordOutlineColor = null;
			}

			if (_lblCurrentWordShadowColor != null) {
				_lblCurrentWordShadowColor.Dispose ();
				_lblCurrentWordShadowColor = null;
			}

			if (_lblCurrentWordTextColor != null) {
				_lblCurrentWordTextColor.Dispose ();
				_lblCurrentWordTextColor = null;
			}

			if (_lblShadowTypes != null) {
				_lblShadowTypes.Dispose ();
				_lblShadowTypes = null;
			}

			if (_lstStyles != null) {
				_lstStyles.Dispose ();
				_lstStyles = null;
			}

			if (_pnlOptions != null) {
				_pnlOptions.Dispose ();
				_pnlOptions = null;
			}

			if (_pnlShadowTypes != null) {
				_pnlShadowTypes.Dispose ();
				_pnlShadowTypes = null;
			}

			if (_btnCurrentWordTextColor != null) {
				_btnCurrentWordTextColor.Dispose ();
				_btnCurrentWordTextColor = null;
			}

			if (_btnCurrentWordOutlineColor != null) {
				_btnCurrentWordOutlineColor.Dispose ();
				_btnCurrentWordOutlineColor = null;
			}

			if (_btnCurrentWordShadowColor != null) {
				_btnCurrentWordShadowColor.Dispose ();
				_btnCurrentWordShadowColor = null;
			}

			if (_lblInputFile != null) {
				_lblInputFile.Dispose ();
				_lblInputFile = null;
			}
		}
	}
}
