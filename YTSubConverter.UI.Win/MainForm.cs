using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using YTSubConverter.Shared;
using YTSubConverter.Shared.Formats;
using YTSubConverter.Shared.Formats.Ass;
using YTSubConverter.Shared.Util;

namespace YTSubConverter.UI.Win
{
    public partial class MainForm : Form
    {
        private readonly float _windowsScaleFactor;

        private readonly Dictionary<string, AssStyleOptions> _styleOptions;
        private readonly HashSet<string> _builtinStyleNames;
        private Dictionary<string, AssStyle> _styles;
        private AssStyle _defaultStyle;
        private bool _previewSuspended;
        private DateTime _lastAutoConvertTime = DateTime.MinValue;

        public MainForm()
        {
            InitializeComponent();
            _windowsScaleFactor = AutoScaleDimensions.Height / 12;

            LocalizeUI();

            List<AssStyleOptions> builtinStyleOptions = AssStyleOptionsList.LoadFromString(Resources.DefaultStyleOptions);
            List<AssStyleOptions> customStyleOptions = AssStyleOptionsList.LoadFromFile();
            _styleOptions = customStyleOptions.Concat(builtinStyleOptions).ToDictionaryOverwrite(o => o.Name);
            _builtinStyleNames = builtinStyleOptions.Select(o => o.Name).ToHashSet();

            ExpandCollapseStyleOptions();
            ClearUI();
        }

        private void LocalizeUI()
        {
            Version version = Assembly.GetEntryAssembly().GetName().Version;
            Text = $"YTSubConverter {version.Major}.{version.Minor}.{version.Build}";

            _chkStyleOptions.Text = Resources.StyleOptions;
            _lblShadowTypes.Text = Resources.ShadowTypes;
            _chkGlow.Text = Resources.Glow;
            _chkBevel.Text = Resources.Bevel;
            _chkSoftShadow.Text = Resources.SoftShadow;
            _chkHardShadow.Text = Resources.HardShadow;
            _chkKaraoke.Text = Resources.UseForKaraoke;
            _chkHighlightCurrentWord.Text = Resources.HighlightCurrentWord;
            _lblCurrentWordTextColor.Text = Resources.KaraokeTextColor;
            _lblCurrentWordOutlineColor.Text = Resources.KaraokeOutlineColor;
            _lblCurrentWordShadowColor.Text = Resources.KaraokeShadowColor;
            UpdateBackgroundImageButton();
            _chkAutoConvert.Text = Resources.Autoconvert;
            _btnConvert.Text = Resources.Convert;

            _dlgOpenSubtitles.Filter = Resources.SubtitleFileFilter;
            _dlgOpenImage.Filter = Resources.ImageFileFilter;

            if (!string.IsNullOrEmpty(Resources.FontName))
            {
                AutoScaleMode = AutoScaleMode.None;
                Font = new Font(Resources.FontName, Font.Size);
                _btnBrowse.Height = _txtInputFile.Height;
                _btnCurrentWordTextColor.Height = _txtCurrentWordTextColor.Height;
                _btnCurrentWordOutlineColor.Height = _txtCurrentWordOutlineColor.Height;
                _btnCurrentWordShadowColor.Height = _txtCurrentWordShadowColor.Height;
            }
        }

        private AssStyleOptions SelectedStyleOptions
        {
            get { return (AssStyleOptions)_lstStyles.SelectedItem; }
        }

        private void _chkStyleOptions_CheckedChanged(object sender, EventArgs e)
        {
            ExpandCollapseStyleOptions();
        }

        private void ExpandCollapseStyleOptions()
        {
            Height = (int)((_chkStyleOptions.Checked ? 488 : 142) * _windowsScaleFactor);
        }

        private void _btnBrowse_Click(object sender, EventArgs e)
        {
            if (_dlgOpenSubtitles.ShowDialog() != DialogResult.OK)
                return;

            LoadFile(_dlgOpenSubtitles.FileName);
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
                MessageBox.Show(string.Format(Resources.FailedToLoadFile0, ex.Message), Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                ClearUI();
            }
        }

        private void PopulateUI(string filePath, SubtitleDocument document)
        {
            _txtInputFile.Text = filePath;

            AssDocument assDoc = document as AssDocument;
            if (assDoc != null)
            {
                _grpStyleOptions.Enabled = true;
                RefreshStyleList(assDoc);
            }

            _chkAutoConvert.Enabled = true;
            _chkAutoConvert.Checked = false;

            _subtitleModifyWatcher.EnableRaisingEvents = false;
            _subtitleModifyWatcher.Path = Path.GetDirectoryName(filePath);
            _subtitleModifyWatcher.Filter = Path.GetFileName(filePath);

            // Aegisub doesn't write straight to the .ass file, but instead creates a separate <name>_tmp_<number>.ass
            // and renames that to the original name.
            _subtitleRenameWatcher.EnableRaisingEvents = false;
            _subtitleRenameWatcher.Path = Path.GetDirectoryName(filePath);
            _subtitleRenameWatcher.Filter = Path.GetFileNameWithoutExtension(filePath) + "_tmp_*" + Path.GetExtension(filePath);
            
            _btnConvert.Enabled = true;
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
            _lstStyles.DataSource = document.Styles.Select(s => _styleOptions[s.Name]).ToList();

            int styleIndex = document.Styles.IndexOf(s => s.Name == selectedStyleName);
            if (styleIndex >= 0)
                _lstStyles.SelectedIndex = styleIndex;
            else if (_lstStyles.Items.Count > 0)
                _lstStyles.SelectedIndex = 0;
        }

        private void ClearUI()
        {
            _styles = null;
            _defaultStyle = null;

            _txtInputFile.Text = Resources.DragNDropTip;
            _grpStyleOptions.Enabled = false;
            _lstStyles.DataSource = null;
            UpdateStylePreview();
            _chkAutoConvert.Enabled = false;
            _chkAutoConvert.Checked = false;
            _btnConvert.Enabled = false;
        }

        private void _lstStyles_SelectedIndexChanged(object sender, EventArgs e)
        {
            AssStyleOptions options = SelectedStyleOptions;
            if (options == null)
            {
                _spltStyleOptions.Panel2.Enabled = false;
                _brwPreview.DocumentText = string.Empty;
                return;
            }

            AssStyle style = _styles[options.Name];
            _previewSuspended = true;

            _spltStyleOptions.Panel2.Enabled = true;
            _pnlOptions.Enabled = !_builtinStyleNames.Contains(style.Name);
            _pnlShadowType.Enabled = style.HasShadow;

            if (style.HasOutline && !style.HasOutlineBox)
            {
                _chkGlow.Checked = true;
                _chkGlow.Enabled = false;
            }
            else
            {
                _chkGlow.Checked = style.HasShadow && options.ShadowTypes.Contains(ShadowType.Glow);
                _chkGlow.Enabled = true;
            }

            _chkBevel.Checked = style.HasShadow && options.ShadowTypes.Contains(ShadowType.Bevel);
            _chkSoftShadow.Checked = style.HasShadow && options.ShadowTypes.Contains(ShadowType.SoftShadow);
            _chkHardShadow.Checked = style.HasShadow && options.ShadowTypes.Contains(ShadowType.HardShadow);

            Color currentWordTextColor = options.CurrentWordTextColor;
            Color currentWordOutlineColor = options.CurrentWordOutlineColor;
            Color currentWordShadowColor = options.CurrentWordShadowColor;

            _chkKaraoke.Checked = options.IsKaraoke;
            _chkHighlightCurrentWord.Checked = options.IsKaraoke && !currentWordTextColor.IsEmpty;

            _txtCurrentWordTextColor.Enabled = _chkHighlightCurrentWord.Checked;
            _txtCurrentWordTextColor.Text = _txtCurrentWordTextColor.Enabled ? ColorUtil.ToHtml(currentWordTextColor) : string.Empty;
            _btnCurrentWordTextColor.Enabled = _txtCurrentWordTextColor.Enabled;

            _txtCurrentWordOutlineColor.Enabled = _chkHighlightCurrentWord.Checked && style.HasOutline && !style.HasOutlineBox;
            _txtCurrentWordOutlineColor.Text = _txtCurrentWordOutlineColor.Enabled ? ColorUtil.ToHtml(currentWordOutlineColor) : string.Empty;
            _btnCurrentWordOutlineColor.Enabled = _txtCurrentWordOutlineColor.Enabled;

            _txtCurrentWordShadowColor.Enabled = _chkHighlightCurrentWord.Checked && style.HasShadow;
            _txtCurrentWordShadowColor.Text = _txtCurrentWordShadowColor.Enabled ? ColorUtil.ToHtml(currentWordShadowColor) : string.Empty;
            _btnCurrentWordShadowColor.Enabled = _txtCurrentWordShadowColor.Enabled;

            UpdateBackgroundImageButton();

            _previewSuspended = false;
            UpdateStylePreview();
        }

        private void _chkGlow_CheckedChanged(object sender, EventArgs e)
        {
            SelectedStyleOptions.SetShadowTypeEnabled(ShadowType.Glow, _chkGlow.Checked);
            UpdateStylePreview();
        }

        private void _chkBevel_CheckedChanged(object sender, EventArgs e)
        {
            SelectedStyleOptions.SetShadowTypeEnabled(ShadowType.Bevel, _chkBevel.Checked);
            UpdateStylePreview();
        }

        private void _chkSoftShadow_CheckedChanged(object sender, EventArgs e)
        {
            SelectedStyleOptions.SetShadowTypeEnabled(ShadowType.SoftShadow, _chkSoftShadow.Checked);
            UpdateStylePreview();
        }

        private void _chkHardShadow_CheckedChanged(object sender, EventArgs e)
        {
            SelectedStyleOptions.SetShadowTypeEnabled(ShadowType.HardShadow, _chkHardShadow.Checked);
            UpdateStylePreview();
        }

        private void _chkKaraoke_CheckedChanged(object sender, EventArgs e)
        {
            _previewSuspended = true;
            SelectedStyleOptions.IsKaraoke = _chkKaraoke.Checked;
            _chkHighlightCurrentWord.Enabled = _chkKaraoke.Checked;
            _chkHighlightCurrentWord.Checked = false;
            _previewSuspended = false;
            UpdateStylePreview();
        }

        private void _chkHighlightCurrentWord_CheckedChanged(object sender, EventArgs e)
        {
            AssStyle style = _styles[SelectedStyleOptions.Name];
            _previewSuspended = true;

            bool textColorEnabled = _chkHighlightCurrentWord.Checked;
            _lblCurrentWordTextColor.Enabled = textColorEnabled;
            _txtCurrentWordTextColor.Enabled = textColorEnabled;
            _txtCurrentWordTextColor.Text = textColorEnabled ? ColorUtil.ToHtml(style.PrimaryColor) : string.Empty;
            _btnCurrentWordTextColor.Enabled = textColorEnabled;

            bool outlineColorEnabled = _chkHighlightCurrentWord.Checked && style.HasOutline && !style.HasOutlineBox;
            _lblCurrentWordOutlineColor.Enabled = outlineColorEnabled;
            _txtCurrentWordOutlineColor.Enabled = outlineColorEnabled;
            _txtCurrentWordOutlineColor.Text = outlineColorEnabled ? ColorUtil.ToHtml(style.OutlineColor) : string.Empty;
            _btnCurrentWordOutlineColor.Enabled = outlineColorEnabled;

            bool shadowColorEnabled = _chkHighlightCurrentWord.Checked && style.HasShadow;
            _lblCurrentWordShadowColor.Enabled = shadowColorEnabled;
            _txtCurrentWordShadowColor.Enabled = shadowColorEnabled;
            _txtCurrentWordShadowColor.Text = shadowColorEnabled ? ColorUtil.ToHtml(style.ShadowColor) : string.Empty;
            _btnCurrentWordShadowColor.Enabled = shadowColorEnabled;

            _previewSuspended = false;
            UpdateStylePreview();
        }

        private void _txtCurrentWordTextColor_TextChanged(object sender, EventArgs e)
        {
            Color textColor = ColorUtil.FromHtml(_txtCurrentWordTextColor.Text);
            _btnCurrentWordTextColor.BackColor = textColor;
            SelectedStyleOptions.CurrentWordTextColor = textColor;
            UpdateStylePreview();
        }

        private void _btnCurrentWordTextColor_Click(object sender, EventArgs e)
        {
            _dlgColor.Color = ColorUtil.FromHtml(_txtCurrentWordTextColor.Text);
            if (_dlgColor.ShowDialog() == DialogResult.OK)
                _txtCurrentWordTextColor.Text = ColorUtil.ToHtml(_dlgColor.Color);
        }

        private void _txtCurrentWordOutlineColor_TextChanged(object sender, EventArgs e)
        {
            Color outlineColor = ColorUtil.FromHtml(_txtCurrentWordOutlineColor.Text);
            _btnCurrentWordOutlineColor.BackColor = outlineColor;
            SelectedStyleOptions.CurrentWordOutlineColor = outlineColor;
            UpdateStylePreview();
        }

        private void _btnCurrentWordOutlineColor_Click(object sender, EventArgs e)
        {
            _dlgColor.Color = ColorUtil.FromHtml(_txtCurrentWordOutlineColor.Text);
            if (_dlgColor.ShowDialog() == DialogResult.OK)
                _txtCurrentWordOutlineColor.Text = ColorUtil.ToHtml(_dlgColor.Color);
        }

        private void _txtCurrentWordShadowColor_TextChanged(object sender, EventArgs e)
        {
            Color shadowColor = ColorUtil.FromHtml(_txtCurrentWordShadowColor.Text);
            _btnCurrentWordShadowColor.BackColor = shadowColor;
            SelectedStyleOptions.CurrentWordShadowColor = shadowColor;
            UpdateStylePreview();
        }

        private void _btnCurrentWordShadowColor_Click(object sender, EventArgs e)
        {
            _dlgColor.Color = ColorUtil.FromHtml(_txtCurrentWordShadowColor.Text);
            if (_dlgColor.ShowDialog() == DialogResult.OK)
                _txtCurrentWordShadowColor.Text = ColorUtil.ToHtml(_dlgColor.Color);
        }

        private void _btnBackgroundImage_Click(object sender, EventArgs e)
        {
            AssStyleOptions options = SelectedStyleOptions;
            if (options.HasExistingBackgroundImage)
            {
                options.BackgroundImagePath = null;
            }
            else
            {
                if (_dlgOpenImage.ShowDialog() != DialogResult.OK)
                    return;

                options.BackgroundImagePath = _dlgOpenImage.FileName;
            }

            UpdateBackgroundImageButton();
            UpdateStylePreview();
        }

        private void UpdateBackgroundImageButton()
        {
            if (SelectedStyleOptions?.HasExistingBackgroundImage ?? false)
            {
                _btnBackgroundImage.Text = "×";
                _toolTip.SetToolTip(_btnBackgroundImage, Resources.ClearBackgroundImage);
            }
            else
            {
                _btnBackgroundImage.Text = "...";
                _toolTip.SetToolTip(_btnBackgroundImage, Resources.SelectBackgroundImage);
            }
        }

        private void UpdateStylePreview()
        {
            if (_previewSuspended)
                return;

            AssStyle style = _styles?[SelectedStyleOptions.Name];
            _brwPreview.DocumentText = HtmlStylePreviewGenerator.Generate(style, SelectedStyleOptions, _defaultStyle, _windowsScaleFactor);
        }

        private void _chkAutoConvert_CheckedChanged(object sender, EventArgs e)
        {
            _subtitleModifyWatcher.EnableRaisingEvents = _chkAutoConvert.Checked;
            _subtitleRenameWatcher.EnableRaisingEvents = _chkAutoConvert.Checked;
            if (_chkAutoConvert.Checked)
                _btnConvert_Click(sender, e);
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
            _btnConvert_Click(_btnConvert, EventArgs.Empty);
            _lastAutoConvertTime = DateTime.Now;
        }

        private async void _btnConvert_Click(object sender, EventArgs e)
        {
            try
            {
                string inputExtension = Path.GetExtension(_txtInputFile.Text).ToLower();
                SubtitleDocument outputDoc;
                string outputExtension;

                switch (inputExtension)
                {
                    case ".ass":
                    {
                        AssDocument inputDoc = new AssDocument(_txtInputFile.Text, (List<AssStyleOptions>)_lstStyles.DataSource);
                        outputDoc = new YttDocument(inputDoc);
                        outputExtension = ".ytt";

                        RefreshStyleList(inputDoc);
                        break;
                    }

                    case ".ytt":
                    case ".srv3":
                    {
                        YttDocument inputDoc = new YttDocument(_txtInputFile.Text);
                        outputDoc = new AssDocument(inputDoc);
                        outputExtension = inputExtension == ".ytt" ? ".reverse.ass" : ".ass";
                        break;
                    }

                    default:
                    {
                        SubtitleDocument inputDoc = SubtitleDocument.Load(_txtInputFile.Text);
                        outputDoc = new SrtDocument(inputDoc);
                        outputExtension = ".srt";
                        break;
                    }
                }

                string outputFilePath = Path.ChangeExtension(_txtInputFile.Text, outputExtension);
                outputDoc.Save(outputFilePath);

                _lblConversionSuccess.Text = string.Format(Resources.SuccessfullyCreated0, Path.GetFileName(outputFilePath));
                _lblConversionSuccess.Visible = true;
                await Task.Delay(4000);
                _lblConversionSuccess.Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (GetDroppedFilePath(e) != null)
                e.Effect = DragDropEffects.Copy;
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            string filePath = GetDroppedFilePath(e);
            if (filePath != null)
                LoadFile(filePath);
        }

        private static string GetDroppedFilePath(DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return null;

            string[] filePaths = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (filePaths == null || filePaths.Length != 1)
                return null;

            string filePath = filePaths[0];
            string extension = (Path.GetExtension(filePath) ?? string.Empty).ToLower();
            if (extension != ".ass" &&
                extension != ".ytt" &&
                extension != ".srv3" &&
                extension != ".sbv")
            {
                return null;
            }

            return filePath;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            AssStyleOptionsList.SaveToFile(
                _styleOptions.Where(p => !_builtinStyleNames.Contains(p.Key))
                             .Select(p => p.Value)
            );
        }
    }
}
