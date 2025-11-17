using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using YTSubConverter.Shared;
using YTSubConverter.Shared.Formats;
using YTSubConverter.Shared.Formats.Ass;
using YTSubConverter.Shared.Util;
using Gtk;

namespace YTSubConverter.UI.Linux
{
    public class MainWindow : Window
    {
        private readonly Builder _builder;

        [Builder.Object] private FileChooserButton _btnInputFile;
        [Builder.Object] private Frame _grpStyleOptions;
        [Builder.Object] private Label _lblStyleOptions;
        [Builder.Object] private TreeView _lstStyles;
        [Builder.Object] private Box _pnlStyle;
        [Builder.Object] private Box _pnlOptions;
        [Builder.Object] private Frame _pnlShadowTypes;
        [Builder.Object] private Label _lblShadowTypes;
        [Builder.Object] private CheckButton _chkGlow;
        [Builder.Object] private CheckButton _chkBevel;
        [Builder.Object] private CheckButton _chkSoftShadow;
        [Builder.Object] private CheckButton _chkHardShadow;
        [Builder.Object] private CheckButton _chkKaraoke;
        [Builder.Object] private CheckButton _chkHighlightCurrentWord;
        [Builder.Object] private Label _lblCurrentWordTextColor;
        [Builder.Object] private ColorButton _btnCurrentWordTextColor;
        [Builder.Object] private Label _lblCurrentWordOutlineColor;
        [Builder.Object] private ColorButton _btnCurrentWordOutlineColor;
        [Builder.Object] private Label _lblCurrentWordShadowColor;
        [Builder.Object] private ColorButton _btnCurrentWordShadowColor;
        [Builder.Object] private Box _pnlPreview;
        [Builder.Object] private Label _lblConversionSuccess;
        [Builder.Object] private ToggleButton _chkAutoConvert;
        [Builder.Object] private Button _btnConvert;

        private readonly FileSystemWatcher _subtitleRenameWatcher;
        private readonly FileSystemWatcher _subtitleModifyWatcher;

        private readonly Dictionary<string, AssStyleOptions> _styleOptions;
        private readonly HashSet<string> _builtinStyleNames;
        private Dictionary<string, AssStyle> _styles;
        private AssStyle _defaultStyle;
        private bool _previewSuspended;
        private DateTime _lastAutoConvertTime = DateTime.MinValue;

        public MainWindow()
            : this(new Builder("YTSubConverter.UI.Linux.MainWindow.glade"))
        {
        }

        private MainWindow(Builder builder)
            : base(builder.GetObject("MainWindow").Handle)
        {
            _builder = builder;
            _builder.Autoconnect(this);

            List<AssStyleOptions> builtinStyleOptions = AssStyleOptionsList.LoadFromString(Resources.DefaultStyleOptions);
            List<AssStyleOptions> customStyleOptions = AssStyleOptionsList.LoadFromFile(GetStyleOptionsFilePath());
            _styleOptions = customStyleOptions.Concat(builtinStyleOptions).ToDictionaryOverwrite(o => o.Name);
            _builtinStyleNames = builtinStyleOptions.Select(o => o.Name).ToHashSet();

            _subtitleRenameWatcher = new FileSystemWatcher();
            _subtitleRenameWatcher.NotifyFilter = NotifyFilters.FileName;
            _subtitleRenameWatcher.Renamed += HandleTmpFileRenamed;

            _subtitleModifyWatcher = new FileSystemWatcher();
            _subtitleModifyWatcher.NotifyFilter = NotifyFilters.LastWrite;
            _subtitleModifyWatcher.Changed += HandleFileModified;

            LocalizeUI();
            ClearUI();

            Drag.DestSet(this, DestDefaults.All, [new TargetEntry("text/uri-list", 0, 0)], Gdk.DragAction.Copy);
        }

        private void LocalizeUI()
        {
            Version version = Assembly.GetEntryAssembly().GetName().Version;
            Title = $"YTSubConverter {version.Major}.{version.Minor}.{version.Build}";

            _lblStyleOptions.Text = Resources.StyleOptions;
            _lblShadowTypes.Text = Resources.ShadowTypes;
            _chkGlow.Label = Resources.Glow;
            _chkBevel.Label = Resources.Bevel;
            _chkSoftShadow.Label = Resources.SoftShadow;
            _chkHardShadow.Label = Resources.HardShadow;
            _chkKaraoke.Label = Resources.UseForKaraoke;
            _chkHighlightCurrentWord.Label = Resources.HighlightCurrentWord;
            _lblCurrentWordTextColor.Text = Resources.KaraokeTextColor;
            _lblCurrentWordOutlineColor.Text = Resources.KaraokeOutlineColor;
            _lblCurrentWordShadowColor.Text = Resources.KaraokeShadowColor;
            _chkAutoConvert.Label = Resources.Autoconvert;
            _btnConvert.Label = Resources.Convert;

            string[] filterParts = Resources.SubtitleFileFilter.Split('|');
            for (int i = 0; i < filterParts.Length; i += 2)
            {
                FileFilter filter = new FileFilter();
                filter.Name = filterParts[i] + " (" + filterParts[i + 1].Replace(";", " ") + ")";
                foreach (string pattern in filterParts[i + 1].Split(';'))
                {
                    filter.AddPattern(pattern);
                }
                _btnInputFile.AddFilter(filter);
            }

            _lstStyles.AppendPropertyColumn<AssStyleOptions>("", o => o.Name);
        }

        private AssStyleOptions SelectedStyleOptions
        {
            get { return (AssStyleOptions)_lstStyles.Selection.GetSelectedModelValue(); }
        }

        private void _btnInputFile_FileSet(object sender, EventArgs e)
        {
            LoadFile(_btnInputFile.Filename);
        }

        private void LoadFile(string filePath)
        {
            ClearUI();

            try
            {
                SubtitleDocument doc = SubtitleDocument.Load(filePath);
                PopulateUI(filePath, doc);
            }
            catch (Exception ex)
            {
                MessageDialogHelper.Show(string.Format(Resources.FailedToLoadFile0, ex.Message), MessageType.Error, ButtonsType.Ok, this);
                ClearUI();
            }
        }

        private void PopulateUI(string filePath, SubtitleDocument document)
        {
            _btnInputFile.SetFilename(filePath);

            if (document is AssDocument assDoc)
            {
                _grpStyleOptions.Sensitive = true;
                RefreshStyleList(assDoc);
            }

            _chkAutoConvert.Sensitive = true;
            _chkAutoConvert.Active = false;

            _subtitleModifyWatcher.EnableRaisingEvents = false;
            _subtitleModifyWatcher.Path = System.IO.Path.GetDirectoryName(filePath);
            _subtitleModifyWatcher.Filter = System.IO.Path.GetFileName(filePath);

            // Aegisub doesn't write straight to the .ass file, but instead creates a separate <name>_tmp_<number>.ass
            // and renames that to the original name.
            _subtitleRenameWatcher.EnableRaisingEvents = false;
            _subtitleRenameWatcher.Path = System.IO.Path.GetDirectoryName(filePath);
            _subtitleRenameWatcher.Filter = System.IO.Path.GetFileNameWithoutExtension(filePath) + "_tmp_*" + System.IO.Path.GetExtension(filePath);

            _btnConvert.Sensitive = true;
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

            string selectedStyleName = SelectedStyleOptions?.Name;

            ListStore store = new ListStore(typeof(AssStyleOptions));
            store.AddRange(document.Styles.Select(s => _styleOptions[s.Name]));
            _lstStyles.Model = store;

            int styleIndex = document.Styles.IndexOf(s => s.Name == selectedStyleName);

            if (styleIndex >= 0)
                _lstStyles.SetCursor(new TreePath([styleIndex]), _lstStyles.Columns[0], false);
            else if (store.IterNChildren() > 0)
                _lstStyles.SetCursor(new TreePath([0]), _lstStyles.Columns[0], false);
        }

        private void ClearUI()
        {
            _styles = null;
            _defaultStyle = null;

            _btnInputFile.UnselectAll();
            _grpStyleOptions.Sensitive = false;
            _lstStyles.Model = null;
            UpdateStylePreview();
            _chkAutoConvert.Sensitive = false;
            _chkAutoConvert.Active = false;
            _btnConvert.Sensitive = false;
        }

        private void _lstStyles_SelectionChanged(object sender, EventArgs e)
        {
            AssStyleOptions options = SelectedStyleOptions;
            if (options == null)
            {
                _pnlStyle.Sensitive = false;
                UpdateStylePreview();
                return;
            }

            AssStyle style = _styles[options.Name];
            _previewSuspended = true;

            _pnlStyle.Sensitive = true;
            _pnlOptions.Sensitive = !_builtinStyleNames.Contains(style.Name);
            _pnlShadowTypes.Sensitive = style.HasShadow;

            if (style.HasOutline && !style.HasOutlineBox)
            {
                _chkGlow.Active = true;
                _chkGlow.Sensitive = false;
            }
            else
            {
                _chkGlow.Active = style.HasShadow && options.ShadowTypes.Contains(Shared.ShadowType.Glow);
                _chkGlow.Sensitive = true;
            }

            _chkBevel.Active = style.HasShadow && options.ShadowTypes.Contains(Shared.ShadowType.Bevel);
            _chkSoftShadow.Active = style.HasShadow && options.ShadowTypes.Contains(Shared.ShadowType.SoftShadow);
            _chkHardShadow.Active = style.HasShadow && options.ShadowTypes.Contains(Shared.ShadowType.HardShadow);

            Color currentWordTextColor = options.CurrentWordTextColor;
            Color currentWordOutlineColor = options.CurrentWordOutlineColor;
            Color currentWordShadowColor = options.CurrentWordShadowColor;

            _chkKaraoke.Active = options.IsKaraoke;
            _chkHighlightCurrentWord.Active = options.IsKaraoke && !currentWordTextColor.IsEmpty;

            bool textColorEnabled = _chkHighlightCurrentWord.Active;
            _lblCurrentWordTextColor.Sensitive = textColorEnabled;
            _btnCurrentWordTextColor.Sensitive = textColorEnabled;
            _btnCurrentWordTextColor.SetColor(textColorEnabled ? currentWordTextColor : Color.Empty);

            bool outlineColorEnabled = _chkHighlightCurrentWord.Active && style.HasOutline && !style.HasOutlineBox;
            _lblCurrentWordOutlineColor.Sensitive = outlineColorEnabled;
            _btnCurrentWordOutlineColor.Sensitive = outlineColorEnabled;
            _btnCurrentWordOutlineColor.SetColor(outlineColorEnabled ? currentWordOutlineColor : Color.Empty);

            bool shadowColorEnabled = _chkHighlightCurrentWord.Active && style.HasShadow;
            _lblCurrentWordShadowColor.Sensitive = shadowColorEnabled;
            _btnCurrentWordShadowColor.Sensitive = shadowColorEnabled;
            _btnCurrentWordShadowColor.SetColor(shadowColorEnabled ? currentWordShadowColor : Color.Empty);

            _previewSuspended = false;
            UpdateStylePreview();
        }

        private void _chkGlow_Toggled(object sender, EventArgs e)
        {
            SelectedStyleOptions.SetShadowTypeEnabled(Shared.ShadowType.Glow, _chkGlow.Active);
            UpdateStylePreview();
        }

        private void _chkBevel_Toggled(object sender, EventArgs e)
        {
            SelectedStyleOptions.SetShadowTypeEnabled(Shared.ShadowType.Bevel, _chkBevel.Active);
            UpdateStylePreview();
        }

        private void _chkSoftShadow_Toggled(object sender, EventArgs e)
        {
            SelectedStyleOptions.SetShadowTypeEnabled(Shared.ShadowType.SoftShadow, _chkSoftShadow.Active);
            UpdateStylePreview();
        }

        private void _chkHardShadow_Toggled(object sender, EventArgs e)
        {
            SelectedStyleOptions.SetShadowTypeEnabled(Shared.ShadowType.HardShadow, _chkHardShadow.Active);
            UpdateStylePreview();
        }

        private void _chkKaraoke_Toggled(object sender, EventArgs e)
        {
            _previewSuspended = true;
            SelectedStyleOptions.IsKaraoke = _chkKaraoke.Active;
            _chkHighlightCurrentWord.Sensitive = _chkKaraoke.Active;
            _chkHighlightCurrentWord.Active = false;
            _previewSuspended = false;
            UpdateStylePreview();
        }

        private void _chkHighlightCurrentWord_Toggled(object sender, EventArgs e)
        {
            AssStyle style = _styles[SelectedStyleOptions.Name];
            _previewSuspended = true;

            bool textColorEnabled = _chkHighlightCurrentWord.Active;
            _lblCurrentWordTextColor.Sensitive = textColorEnabled;
            _btnCurrentWordTextColor.Sensitive = textColorEnabled;
            _btnCurrentWordTextColor.SetColor(textColorEnabled ? style.PrimaryColor : Color.Empty);

            bool outlineColorEnabled = _chkHighlightCurrentWord.Active && style.HasOutline && !style.HasOutlineBox;
            _lblCurrentWordOutlineColor.Sensitive = outlineColorEnabled;
            _btnCurrentWordOutlineColor.Sensitive = outlineColorEnabled;
            _btnCurrentWordOutlineColor.SetColor(outlineColorEnabled ? style.OutlineColor : Color.Empty);

            bool shadowColorEnabled = _chkHighlightCurrentWord.Active && style.HasShadow;
            _lblCurrentWordShadowColor.Sensitive = shadowColorEnabled;
            _btnCurrentWordShadowColor.Sensitive = shadowColorEnabled;
            _btnCurrentWordShadowColor.SetColor(shadowColorEnabled ? style.ShadowColor : Color.Empty);

            _previewSuspended = false;
            UpdateStylePreview();
        }

        private void _btnCurrentWordTextColor_ColorSet(object sender, EventArgs e)
        {
            SelectedStyleOptions.CurrentWordTextColor = _btnCurrentWordTextColor.GetColor();
            UpdateStylePreview();
        }

        private void _btnCurrentWordOutlineColor_ColorSet(object sender, EventArgs e)
        {
            SelectedStyleOptions.CurrentWordOutlineColor = _btnCurrentWordOutlineColor.GetColor();
            UpdateStylePreview();
        }

        private void _btnCurrentWordShadowColor_ColorSet(object sender, EventArgs e)
        {
            SelectedStyleOptions.CurrentWordShadowColor = _btnCurrentWordShadowColor.GetColor();
            UpdateStylePreview();
        }

        private void UpdateStylePreview()
        {
            if (_previewSuspended)
                return;

            AssStyle style = SelectedStyleOptions != null ? _styles?[SelectedStyleOptions.Name] : null;
            GtkStylePreviewGenerator.Generate(_pnlPreview, style, SelectedStyleOptions, _defaultStyle);
        }

        private void _chkAutoConvert_Toggled(object sender, EventArgs e)
        {
            _subtitleModifyWatcher.EnableRaisingEvents = _chkAutoConvert.Active;
            _subtitleRenameWatcher.EnableRaisingEvents = _chkAutoConvert.Active;
            if (_chkAutoConvert.Active)
                _btnConvert_Clicked(sender, e);
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
            _btnConvert_Clicked(_btnConvert, EventArgs.Empty);
            _lastAutoConvertTime = DateTime.Now;
        }

        private async void _btnConvert_Clicked(object sender, EventArgs e)
        {
            try
            {
                string inputExtension = System.IO.Path.GetExtension(_btnInputFile.Filename).ToLower();
                SubtitleDocument outputDoc;
                string outputExtension;

                switch (inputExtension)
                {
                    case ".ass":
                    {
                        List<AssStyleOptions> options = ((ListStore)_lstStyles.Model).AsEnumerable<AssStyleOptions>().ToList();
                        AssDocument inputDoc = new AssDocument(_btnInputFile.Filename, options);
                        outputDoc = new YttDocument(inputDoc);
                        outputExtension = ".ytt";

                        RefreshStyleList(inputDoc);
                        break;
                    }

                    case ".ytt":
                    case ".srv3":
                    {
                        YttDocument inputDoc = new YttDocument(_btnInputFile.Filename);
                        outputDoc = new AssDocument(inputDoc);
                        outputExtension = inputExtension == ".ytt" ? ".reverse.ass" : ".ass";
                        break;
                    }

                    case ".sbv":
                    {
                        SubtitleDocument inputDoc = SubtitleDocument.Load(_btnInputFile.Filename);
                        outputDoc = new SrtDocument(inputDoc);
                        outputExtension = ".srt";
                        break;
                    }

                    default:
                    {
                        SubtitleDocument inputDoc = SubtitleDocument.Load(_btnInputFile.Filename);
                        outputDoc = new YttDocument(inputDoc);
                        outputExtension = ".ytt";
                        break;
                    }
                }

                string outputFilePath = System.IO.Path.ChangeExtension(_btnInputFile.Filename, outputExtension);
                outputDoc.Save(outputFilePath);

                _lblConversionSuccess.Text = string.Format(Resources.SuccessfullyCreated0, System.IO.Path.GetFileName(outputFilePath));
                await Task.Delay(4000);
                _lblConversionSuccess.Text = string.Empty;
            }
            catch (Exception ex)
            {
                MessageDialogHelper.Show(ex.Message, MessageType.Error, ButtonsType.Ok, this);
            }
        }

        private void MainWindow_KeyPressEvent(object o, KeyPressEventArgs args)
        {
            if ((args.Event.State & Gdk.ModifierType.ControlMask) == 0)
                return;

            switch (args.Event.Key)
            {
                case Gdk.Key.o:
                    FileChooserNative dialog = new FileChooserNative(null, this, FileChooserAction.Open, null, null);
                    dialog.SetCurrentFolder(_btnInputFile.CurrentFolder);
                    foreach (FileFilter filter in _btnInputFile.Filters)
                    {
                        dialog.AddFilter(filter);
                    }

                    if (dialog.Run() == (int)ResponseType.Accept)
                    {
                        _btnInputFile.UnselectAll();
                        _btnInputFile.SelectFilename(dialog.Filename);
                        _btnInputFile.SetCurrentFolder(dialog.CurrentFolder);
                        _btnInputFile_FileSet(_btnInputFile, EventArgs.Empty);
                    }
                    dialog.Destroy();
                    break;

                case Gdk.Key.s:
                    if (_btnConvert.Sensitive)
                        _btnConvert_Clicked(_btnConvert, EventArgs.Empty);

                    break;

                case Gdk.Key.a:
                    if (_chkAutoConvert.Sensitive)
                        _chkAutoConvert.Active = !_chkAutoConvert.Active;

                    break;

                case Gdk.Key.q:
                    Close();
                    break;
            }
        }

        private void MainWindow_DragDataReceived(object sender, DragDataReceivedArgs e)
        {
            e.RetVal = false;
            if (e.SelectionData.Uris.Length != 1)
                return;

            Uri uri = new Uri(e.SelectionData.Uris[0]);
            if (!uri.IsFile)
                return;

            string filePath = uri.LocalPath;
            string extension = (System.IO.Path.GetExtension(filePath) ?? string.Empty).ToLower();
            if (!SubtitleDocument.IsExtensionSupported(extension))
                return;

            LoadFile(filePath);
            e.RetVal = true;
        }

        private void MainWindow_DeleteEvent(object sender, DeleteEventArgs e)
        {
            string filePath = GetStyleOptionsFilePath();
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filePath));
            AssStyleOptionsList.SaveToFile(
                _styleOptions.Where(p => !_builtinStyleNames.Contains(p.Key))
                             .Select(p => p.Value),
                filePath
            );

            Application.Quit();
            e.RetVal = true;
        }

        private static string GetStyleOptionsFilePath()
        {
            string filePath = AssStyleOptionsList.GetDefaultFilePath();
            if (File.Exists(filePath))
                return filePath;

            string configDir = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME") ??
                               System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");
            return System.IO.Path.Combine(configDir, "ytsubconverter", AssStyleOptionsList.FileName);
        }
    }
}
