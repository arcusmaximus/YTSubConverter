using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AppKit;
using YTSubConverter.Shared;
using YTSubConverter.Shared.Formats;
using YTSubConverter.Shared.Formats.Ass;
using YTSubConverter.Shared.Util;
using Foundation;

namespace YTSubConverter.UI.Mac
{
    public partial class ViewController : NSViewController
    {
        private readonly Dictionary<string, AssStyleOptions> _styleOptions;
        private readonly HashSet<string> _builtinStyleNames;
        private Dictionary<string, AssStyle> _styles;
        private AssStyle _defaultStyle;

        private bool _previewSuspended;
        private DateTime _lastAutoConvertTime = DateTime.MinValue;

        private FileSystemWatcher _subtitleRenameWatcher;
        private FileSystemWatcher _subtitleModifyWatcher;
        private NSOpenPanel _dlgOpenSubtitles;

        public ViewController(IntPtr handle)
            : base(handle)
        {
            List<AssStyleOptions> builtinStyleOptions = AssStyleOptionsList.LoadFromString(Shared.Resources.DefaultStyleOptions);
            List<AssStyleOptions> customStyleOptions = AssStyleOptionsList.LoadFromFile();
            _styleOptions = customStyleOptions.Concat(builtinStyleOptions).ToDictionaryOverwrite(o => o.Name);
            _builtinStyleNames = builtinStyleOptions.Select(o => o.Name).ToHashSet();
        }

        public override void ViewDidAppear()
        {
            base.ViewDidAppear();

            ((View)View).Controller = this;
            View.RegisterForDraggedTypes(new[] { NSPasteboard.NSPasteboardTypeFileUrl.ToString() });

            Version version = Assembly.GetEntryAssembly().GetName().Version;
            View.Window.Title = $"YTSubConverter {version.Major}.{version.Minor}.{version.Build}";

            _dlgOpenSubtitles = NSOpenPanel.OpenPanel;
            _dlgOpenSubtitles.CanChooseDirectories = false;
            _dlgOpenSubtitles.AllowsMultipleSelection = false;

            _dlgOpenSubtitles.AllowedFileTypes = Regex.Matches(Shared.Resources.SubtitleFileFilter, @"\*\.(\w+)")
                                                      .Cast<Match>()
                                                      .Select(m => m.Groups[1].Value)
                                                      .ToArray();

            _subtitleRenameWatcher = new FileSystemWatcher { NotifyFilter = NotifyFilters.FileName };
            _subtitleRenameWatcher.Renamed += HandleTmpFileRenamed;

            _subtitleModifyWatcher = new FileSystemWatcher { NotifyFilter = NotifyFilters.LastWrite };
            _subtitleModifyWatcher.Changed += HandleFileModified;

            _lstStyles.FocusRingType = NSFocusRingType.None;
            _lstStyles.SelectionDidChange += _lstStyles_SelectedIndexChanged;

            LocalizeUI();
            ClearUi();
        }

        private void LocalizeUI()
        {
            NSMenuItem[] menuItems = NSApplication.SharedApplication.MainMenu.Items[0].Submenu.Items;
            menuItems[0].Title = Resources.HideApplication;
            menuItems[1].Title = Resources.HideOthers;
            menuItems[2].Title = Resources.ShowAll;
            menuItems[4].Title = Resources.QuitApplication;

            _grpStyleOptions.Title = Shared.Resources.StyleOptions;
            _lblShadowTypes.StringValue = Shared.Resources.ShadowTypes;
            _chkGlow.Title = Shared.Resources.Glow;
            _chkBevel.Title = Shared.Resources.Bevel;
            _chkSoftShadow.Title = Shared.Resources.SoftShadow;
            _chkHardShadow.Title = Shared.Resources.HardShadow;
            _chkKaraoke.Title = Shared.Resources.UseForKaraoke;
            _chkHighlightCurrentWord.Title = Shared.Resources.HighlightCurrentWord;
            _lblCurrentWordTextColor.StringValue = Shared.Resources.KaraokeTextColor;
            _lblCurrentWordOutlineColor.StringValue = Shared.Resources.KaraokeOutlineColor;
            _lblCurrentWordShadowColor.StringValue = Shared.Resources.KaraokeShadowColor;
            _chkAutoConvert.Title = Shared.Resources.Autoconvert;
            _btnConvert.Title = Shared.Resources.Convert;
        }

        private AssStyleOptions SelectedStyleOptions
        {
            get
            {
                if (_lstStyles.SelectedRow < 0)
                    return null;

                var dataSource = (SimpleTableViewDataSource<AssStyleOptions>)_lstStyles.DataSource;
                return dataSource?[(int)_lstStyles.SelectedRow];
            }
        }

        partial void _btnBrowse_Click(NSObject sender)
        {
            NSModalResponse result = (NSModalResponse)(int)_dlgOpenSubtitles.RunModal();
            if (result != NSModalResponse.OK)
                return;

            LoadFile(_dlgOpenSubtitles.Filename);
        }

        private void LoadFile(string filePath)
        {
            ClearUi();

            try
            {
                SubtitleDocument doc = SubtitleDocument.Load(filePath);
                PopulateUi(filePath, doc);
            }
            catch (Exception ex)
            {
                Alert.Show(string.Format(Shared.Resources.FailedToLoadFile0, ex.Message), NSAlertStyle.Critical);
                ClearUi();
            }
        }

        private void PopulateUi(string filePath, SubtitleDocument document)
        {
            _lblInputFile.StringValue = filePath;

            AssDocument assDoc = document as AssDocument;
            if (assDoc != null)
            {
                _grpStyleOptions.SetEnabled(true);
                RefreshStyleList(assDoc);
            }

            _chkAutoConvert.SetEnabled(true);
            _chkAutoConvert.SetChecked(false);

            _subtitleModifyWatcher.EnableRaisingEvents = false;
            _subtitleModifyWatcher.Path = Path.GetDirectoryName(filePath);
            _subtitleModifyWatcher.Filter = Path.GetFileName(filePath);

            // Aegisub doesn't write straight to the .ass file, but instead creates a separate <name>_tmp_<number>.ass
            // and renames that to the original name.
            _subtitleRenameWatcher.EnableRaisingEvents = false;
            _subtitleRenameWatcher.Path = Path.GetDirectoryName(filePath);
            _subtitleRenameWatcher.Filter = Path.GetFileNameWithoutExtension(filePath) + "_tmp_*" + Path.GetExtension(filePath);

            _btnConvert.SetEnabled(true);
        }

        private void RefreshStyleList(AssDocument document)
        {
            _styles = document.Styles.ToDictionary(s => s.Name);
            _defaultStyle = document.DefaultStyle;
            foreach (AssStyle style in document.Styles)
            {
                if (!_styleOptions.ContainsKey(style.Name))
                    _styleOptions.Add(style.Name, new AssStyleOptions(style));
            }

            int selectedIndex = (int)_lstStyles.SelectedRow;
            List<AssStyleOptions> styleOptions = document.Styles.Select(s => _styleOptions[s.Name]).ToList();
            _lstStyles.DataSource = new SimpleTableViewDataSource<AssStyleOptions>(styleOptions, s => s.Name);
            if (selectedIndex < styleOptions.Count)
                _lstStyles.SelectRow(selectedIndex, false);
            else if (styleOptions.Count > 0)
                _lstStyles.SelectRow(0, false);
        }

        private void ClearUi()
        {
            _styles = null;
            _defaultStyle = null;

            _lblInputFile.StringValue = Shared.Resources.DragNDropTip;
            _lstStyles.DataSource = null;
            _grpStyleOptions.SetEnabled(false);
            _pnlOptions.SetEnabled(false);
            _chkGlow.SetChecked(false);
            _chkBevel.SetChecked(false);
            _chkSoftShadow.SetChecked(false);
            _chkHardShadow.SetChecked(false);
            _chkKaraoke.SetChecked(false);
            _chkHighlightCurrentWord.SetEnabled(false);
            _chkHighlightCurrentWord.SetChecked(false);
            _lblCurrentWordTextColor.SetEnabled(false);
            _btnCurrentWordTextColor.SetColor(Color.Empty);
            _lblCurrentWordOutlineColor.SetEnabled(false);
            _btnCurrentWordOutlineColor.SetColor(Color.Empty);
            _lblCurrentWordShadowColor.SetEnabled(false);
            _btnCurrentWordShadowColor.SetColor(Color.Empty);
            UpdateStylePreview();
            _chkAutoConvert.SetEnabled(false);
            _chkAutoConvert.SetChecked(false);
            _btnConvert.SetEnabled(false);
        }

        private void _lstStyles_SelectedIndexChanged(object sender, EventArgs e)
        {
            AssStyleOptions options = SelectedStyleOptions;
            if (options == null)
            {
                _pnlOptions.SetEnabled(false);
                _brwPreview.LoadHtmlString(string.Empty, null);
                return;
            }

            AssStyle style = _styles[options.Name];
            _previewSuspended = true;

            _pnlOptions.SetEnabled(!_builtinStyleNames.Contains(style.Name));
            _pnlShadowTypes.SetEnabled(style.HasShadow);

            if (style.HasOutline && !style.HasOutlineBox)
            {
                _chkGlow.SetChecked(true);
                _chkGlow.SetEnabled(false);
            }
            else
            {
                _chkGlow.SetChecked(style.HasShadow && options.ShadowTypes.Contains(ShadowType.Glow));
                _chkGlow.SetEnabled(true);
            }

            _chkBevel.SetChecked(style.HasShadow && options.ShadowTypes.Contains(ShadowType.Bevel));
            _chkSoftShadow.SetChecked(style.HasShadow && options.ShadowTypes.Contains(ShadowType.SoftShadow));
            _chkHardShadow.SetChecked(style.HasShadow && options.ShadowTypes.Contains(ShadowType.HardShadow));

            Color currentWordTextColor = options.CurrentWordTextColor;
            Color currentWordOutlineColor = options.CurrentWordOutlineColor;
            Color currentWordShadowColor = options.CurrentWordShadowColor;

            _chkKaraoke.SetChecked(options.IsKaraoke);
            _chkHighlightCurrentWord.SetChecked(!options.CurrentWordTextColor.IsEmpty);

            _btnCurrentWordTextColor.SetEnabled(_chkHighlightCurrentWord.IsChecked());
            _btnCurrentWordTextColor.SetColor(_btnCurrentWordTextColor.IsEnabled() ? currentWordTextColor : Color.Empty);

            _btnCurrentWordOutlineColor.SetEnabled(_chkHighlightCurrentWord.IsChecked() && style.HasOutline && !style.HasOutlineBox);
            _btnCurrentWordOutlineColor.SetColor(_btnCurrentWordOutlineColor.IsEnabled() ? currentWordOutlineColor : Color.Empty);

            _btnCurrentWordShadowColor.SetEnabled(_chkHighlightCurrentWord.IsChecked() && style.HasShadow);
            _btnCurrentWordShadowColor.SetColor(_btnCurrentWordShadowColor.IsEnabled() ? currentWordShadowColor : Color.Empty);

            _previewSuspended = false;
            UpdateStylePreview();
        }

        partial void _chkGlow_CheckedChanged(NSObject sender)
        {
            if (SelectedStyleOptions == null)
                return;

            SelectedStyleOptions.SetShadowTypeEnabled(ShadowType.Glow, _chkGlow.IsChecked());
            UpdateStylePreview();
        }

        partial void _chkBevel_CheckedChanged(NSObject sender)
        {
            if (SelectedStyleOptions == null)
                return;

            SelectedStyleOptions.SetShadowTypeEnabled(ShadowType.Bevel, _chkBevel.IsChecked());
            UpdateStylePreview();
        }

        partial void _chkSoftShadow_CheckedChanged(NSObject sender)
        {
            if (SelectedStyleOptions == null)
                return;

            SelectedStyleOptions.SetShadowTypeEnabled(ShadowType.SoftShadow, _chkSoftShadow.IsChecked());
            UpdateStylePreview();
        }

        partial void _chkHardShadow_CheckedChanged(NSObject sender)
        {
            if (SelectedStyleOptions == null)
                return;

            SelectedStyleOptions.SetShadowTypeEnabled(ShadowType.HardShadow, _chkHardShadow.IsChecked());
            UpdateStylePreview();
        }

        partial void _chkKaraoke_CheckedChanged(NSObject sender)
        {
            if (SelectedStyleOptions == null)
                return;

            _previewSuspended = true;
            SelectedStyleOptions.IsKaraoke = _chkKaraoke.IsChecked();
            _chkHighlightCurrentWord.SetEnabled(_chkKaraoke.IsChecked());
            _chkHighlightCurrentWord.SetChecked(false);
            _previewSuspended = false;
            UpdateStylePreview();
        }

        partial void _chkHighlightCurrentWord_CheckedChanged(NSObject sender)
        {
            if (SelectedStyleOptions == null)
                return;

            AssStyle style = _styles?[SelectedStyleOptions.Name];
            _previewSuspended = true;

            bool textColorEnabled = _chkHighlightCurrentWord.IsChecked();
            _lblCurrentWordTextColor.SetEnabled(textColorEnabled);
            _btnCurrentWordTextColor.SetEnabled(textColorEnabled);
            _btnCurrentWordTextColor.SetColor(textColorEnabled ? ColorUtil.ChangeAlpha(style.PrimaryColor, 255) : Color.Empty);

            bool outlineColorEnabled = _chkHighlightCurrentWord.IsChecked() && style.HasOutline && !style.HasOutlineBox;
            _lblCurrentWordOutlineColor.SetEnabled(outlineColorEnabled);
            _btnCurrentWordOutlineColor.SetEnabled(outlineColorEnabled);
            _btnCurrentWordOutlineColor.SetColor(outlineColorEnabled ? ColorUtil.ChangeAlpha(style.OutlineColor, 255) : Color.Empty);

            bool shadowColorEnabled = _chkHighlightCurrentWord.IsChecked() && style.HasShadow;
            _lblCurrentWordShadowColor.SetEnabled(shadowColorEnabled);
            _btnCurrentWordShadowColor.SetEnabled(shadowColorEnabled);
            _btnCurrentWordShadowColor.SetColor(shadowColorEnabled ? ColorUtil.ChangeAlpha(style.ShadowColor, 255) : Color.Empty);

            _previewSuspended = false;
            UpdateStylePreview();
        }

        partial void _btnCurrentWordTextColor_ColorChanged(NSObject sender)
        {
            if (SelectedStyleOptions == null)
                return;

            SelectedStyleOptions.CurrentWordTextColor = _btnCurrentWordTextColor.GetColor();
            UpdateStylePreview();
        }

        partial void _btnCurrentWordOutlineColor_ColorChanged(NSObject sender)
        {
            if (SelectedStyleOptions == null)
                return;

            SelectedStyleOptions.CurrentWordOutlineColor = _btnCurrentWordOutlineColor.GetColor();
            UpdateStylePreview();
        }

        partial void _btnCurrentWordShadowColor_ColorChanged(NSObject sender)
        {
            if (SelectedStyleOptions == null)
                return;

            SelectedStyleOptions.CurrentWordShadowColor = _btnCurrentWordShadowColor.GetColor();
            UpdateStylePreview();
        }

        private void UpdateStylePreview()
        {
            if (_previewSuspended)
                return;

            AssStyle style = _styles?[SelectedStyleOptions.Name];
            _brwPreview.LoadHtmlString(HtmlStylePreviewGenerator.Generate(style, SelectedStyleOptions, _defaultStyle, 1), null);
        }

        partial void _chkAutoConvert_CheckedChanged(NSObject sender)
        {
            _subtitleModifyWatcher.EnableRaisingEvents = _chkAutoConvert.IsChecked();
            _subtitleRenameWatcher.EnableRaisingEvents = _chkAutoConvert.IsChecked();
            if (_chkAutoConvert.IsChecked())
                Convert();
        }

        private void HandleTmpFileRenamed(object sender, RenamedEventArgs e)
        {
            PerformAutoConvert();
        }

        private void HandleFileModified(object sender, FileSystemEventArgs e)
        {
            PerformAutoConvert();
        }

        private void PerformAutoConvert()
        {
            // The FileSystemWatcher may trigger multiple times in a row
            if ((DateTime.Now - _lastAutoConvertTime).TotalMilliseconds < 100)
                return;

            // Sleep a bit just in case an antivirus is still doing something with the file
            Thread.Sleep(100);
            InvokeOnMainThread(Convert);
            _lastAutoConvertTime = DateTime.Now;
        }

        partial void _btnConvert_Click(NSObject sender)
        {
            Convert();
        }

        private async void Convert()
        {
            try
            {
                string inputExtension = Path.GetExtension(_lblInputFile.StringValue).ToLower();
                SubtitleDocument outputDoc;
                string outputExtension;

                switch (inputExtension)
                {
                    case ".ass":
                    {

                        AssDocument inputDoc = new AssDocument(_lblInputFile.StringValue, ((SimpleTableViewDataSource<AssStyleOptions>)_lstStyles.DataSource)?.InnerList);
                        outputDoc = new YttDocument(inputDoc);
                        outputExtension = ".ytt";

                        RefreshStyleList(inputDoc);
                        break;
                    }

                    case ".ytt":
                    case ".srv3":
                    {
                        YttDocument inputDoc = new YttDocument(_lblInputFile.StringValue);
                        outputDoc = new AssDocument(inputDoc);
                        outputExtension = inputExtension == ".ytt" ? ".reverse.ass" : ".ass";
                        break;
                    }

                    default:
                    {
                        SubtitleDocument inputDoc = SubtitleDocument.Load(_lblInputFile.StringValue);
                        outputDoc = new SrtDocument(inputDoc);
                        outputExtension = ".srt";
                        break;
                    }
                }

                string outputFilePath = Path.ChangeExtension(_lblInputFile.StringValue, outputExtension);
                outputDoc.Save(outputFilePath);

                _lblConversionSuccess.StringValue = string.Format(Shared.Resources.SuccessfullyCreated0, Path.GetFileName(outputFilePath));
                _lblConversionSuccess.Hidden = false;
                await Task.Delay(4000);
                _lblConversionSuccess.Hidden = true;
            }
            catch (Exception ex)
            {
                Alert.Show(ex.Message, NSAlertStyle.Critical);
            }
        }

        public NSDragOperation GetDragOperation(NSDraggingInfo info)
        {
            if (!info.DraggingPasteboard.Types.Contains(NSPasteboard.NSPasteboardTypeFileUrl.ToString()))
                return NSDragOperation.None;

            NSUrl url = NSUrl.FromPasteboard(info.DraggingPasteboard);
            if (!url.IsFileUrl)
                return NSDragOperation.None;

            string filePath = url.Path;
            string extension = (Path.GetExtension(filePath) ?? string.Empty).ToLower();
            bool allowed = extension == ".ass" ||
                           extension == ".ytt" ||
                           extension == ".srv3" ||
                           extension == ".sbv";
            return allowed ? NSDragOperation.Copy : NSDragOperation.None;
        }

        public bool PerformDrag(NSDraggingInfo info)
        {
            if (GetDragOperation(info) == NSDragOperation.None)
                return false;

            NSUrl url = NSUrl.FromPasteboard(info.DraggingPasteboard);
            LoadFile(url.Path);
            return true;
        }

        public override void ViewWillDisappear()
        {
            AssStyleOptionsList.SaveToFile(
                _styleOptions.Where(p => !_builtinStyleNames.Contains(p.Key))
                             .Select(p => p.Value)
            );

            base.ViewWillDisappear();
        }
    }
}
