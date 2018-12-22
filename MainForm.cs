using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Arc.YTSubConverter.Ass;
using Arc.YTSubConverter.Util;

namespace Arc.YTSubConverter
{
    public partial class MainForm : Form
    {
        private Dictionary<string, AssStyleOptions> _styleOptions;
        private Dictionary<string, AssStyle> _styles;

        public MainForm()
        {
            InitializeComponent();

            _styleOptions = AssStyleOptionsList.Load().ToDictionary(o => o.Name);
            ExpandCollapseStyleOptions();
            ClearUi();
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
            Height = _chkStyleOptions.Checked ? 488 : 142;
        }

        private void _btnBrowse_Click(object sender, EventArgs e)
        {
            if (_dlgOpenFile.ShowDialog() != DialogResult.OK)
                return;

            LoadFile(_dlgOpenFile.FileName);
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
                MessageBox.Show($"Failed to load file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void PopulateUi(string filePath, SubtitleDocument doc)
        {
            _txtInputFile.Text = filePath;

            AssDocument assDoc = doc as AssDocument;
            if (assDoc != null)
            {
                _grpStyleOptions.Enabled = true;

                _styles = assDoc.Styles.ToDictionary(s => s.Name);
                foreach (AssStyle style in assDoc.Styles)
                {
                    if (!_styleOptions.ContainsKey(style.Name))
                        _styleOptions.Add(style.Name, new AssStyleOptions(style));
                }

                _lstStyles.DataSource = assDoc.Styles.Select(s => _styleOptions[s.Name]).ToList();
                if (_lstStyles.Items.Count > 0)
                    _lstStyles.SelectedIndex = 0;
            }

            _btnConvert.Enabled = true;
        }

        private void ClearUi()
        {
            _styles = null;

            _txtInputFile.Text = "Drag&drop .ass/.sbv file or click the \"...\" button";
            _grpStyleOptions.Enabled = false;
            _lstStyles.DataSource = null;
            UpdateStylePreview();
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

            _spltStyleOptions.Panel2.Enabled = true;
            _pnlShadowType.Enabled = style.HasShadow && (!style.HasOutline || style.HasOutlineBox);
            _radGlow.Checked = options.ShadowType == ShadowType.Glow;
            _radSoftShadow.Checked = options.ShadowType == ShadowType.SoftShadow;
            _radHardShadow.Checked = options.ShadowType == ShadowType.HardShadow;

            Color currentWordTextColor = options.CurrentWordTextColor;
            Color currentWordShadowColor = options.CurrentWordShadowColor;

            _chkKaraoke.Checked = options.IsKaraoke;
            _chkHighlightCurrentWord.Checked = !currentWordShadowColor.IsEmpty;
            _txtCurrentWordTextColor.Text = ColorTranslator.ToHtml(currentWordTextColor);
            _txtCurrentWordGlowColor.Text = ColorTranslator.ToHtml(currentWordShadowColor);
            UpdateStylePreview();
        }

        private void _radGlow_CheckedChanged(object sender, EventArgs e)
        {
            if (!_radGlow.Checked)
                return;

            SelectedStyleOptions.ShadowType = ShadowType.Glow;
            UpdateStylePreview();
        }

        private void _radSoftShadow_CheckedChanged(object sender, EventArgs e)
        {
            if (!_radSoftShadow.Checked)
                return;

            SelectedStyleOptions.ShadowType = ShadowType.SoftShadow;
            UpdateStylePreview();
        }

        private void _radHardShadow_CheckedChanged(object sender, EventArgs e)
        {
            if (!_radHardShadow.Checked)
                return;

            SelectedStyleOptions.ShadowType = ShadowType.HardShadow;
            UpdateStylePreview();
        }

        private void _chkKaraoke_CheckedChanged(object sender, EventArgs e)
        {
            SelectedStyleOptions.IsKaraoke = _chkKaraoke.Checked;
            _chkHighlightCurrentWord.Enabled = _chkKaraoke.Checked;
            _chkHighlightCurrentWord.Checked = false;
            UpdateStylePreview();
        }

        private void _chkHighlightCurrentWord_CheckedChanged(object sender, EventArgs e)
        {
            _txtCurrentWordTextColor.Enabled = _chkHighlightCurrentWord.Checked;
            _txtCurrentWordTextColor.Text = ColorTranslator.ToHtml(_chkHighlightCurrentWord.Checked ? _styles[SelectedStyleOptions.Name].PrimaryColor : Color.Empty);

            _txtCurrentWordGlowColor.Enabled = _chkHighlightCurrentWord.Checked;
            _txtCurrentWordGlowColor.Text = ColorTranslator.ToHtml(_chkHighlightCurrentWord.Checked ? _styles[SelectedStyleOptions.Name].ShadowColor : Color.Empty);

            UpdateStylePreview();
        }

        private void _txtCurrentWordTextColor_TextChanged(object sender, EventArgs e)
        {
            SelectedStyleOptions.CurrentWordTextColor = ColorTranslator.FromHtml(_txtCurrentWordTextColor.Text);
            UpdateStylePreview();
        }

        private void _txtCurrentWordGlowColor_TextChanged(object sender, EventArgs e)
        {
            SelectedStyleOptions.CurrentWordShadowColor = ColorTranslator.FromHtml(_txtCurrentWordGlowColor.Text);
            UpdateStylePreview();
        }

        private void UpdateStylePreview()
        {
            AssStyle style = _styles?[SelectedStyleOptions.Name];
            _brwPreview.DocumentText = StylePreviewGenerator.GenerateHtml(style, SelectedStyleOptions);
        }

        private async void _btnConvert_Click(object sender, EventArgs e)
        {
            try
            {
                string outputFilePath;
                if (Path.GetExtension(_txtInputFile.Text) == ".ass")
                {
                    AssDocument inputDoc = new AssDocument(_txtInputFile.Text, (List<AssStyleOptions>)_lstStyles.DataSource);
                    YttDocument outputDoc = new YttDocument(inputDoc);
                    outputFilePath = Path.ChangeExtension(_txtInputFile.Text, ".ytt");
                    outputDoc.Save(outputFilePath);
                }
                else
                {
                    SubtitleDocument inputDoc = SubtitleDocument.Load(_txtInputFile.Text);
                    SrtDocument outputDoc = new SrtDocument(inputDoc);
                    outputFilePath = Path.ChangeExtension(_txtInputFile.Text, ".srt");
                    outputDoc.Save(outputFilePath);
                }

                _lblConversionSuccess.Text = $"Successfully created {Path.GetFileName(outputFilePath)}";
                _lblConversionSuccess.Visible = true;
                await Task.Delay(4000);
                _lblConversionSuccess.Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (GetDroppedFilePath(e) != null)
                e.Effect = DragDropEffects.Copy;
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            LoadFile(GetDroppedFilePath(e));
        }

        private static string GetDroppedFilePath(DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return null;

            string[] filePaths = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (filePaths == null || filePaths.Length != 1)
                return null;

            string filePath = filePaths[0];
            string extension = Path.GetExtension(filePath);
            if (extension != ".ass" && extension != ".sbv")
                return null;

            return filePath;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            AssStyleOptionsList.Save(_styleOptions.Values);
        }
    }
}
