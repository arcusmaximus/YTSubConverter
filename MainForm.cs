using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using Arc.YTSubConverter.Ass;

namespace Arc.YTSubConverter
{
    public partial class MainForm : Form
    {
        private const string StyleOptionsFileName = "StyleOptions.xml";

        private Dictionary<string, AssStyleOptions> _styleOptions;
        private Dictionary<string, AssStyle> _styles;

        public MainForm()
        {
            InitializeComponent();

            LoadStyleOptions();
            ExpandCollapseStyleOptions();
            ClearUi();
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
            if (_openFileDialog.ShowDialog() != DialogResult.OK)
                return;

            LoadFile(_openFileDialog.FileName);
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
            AssStyleOptions options = (AssStyleOptions)_lstStyles.SelectedItem;
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
            UpdateStylePreview();
        }

        private void _radGlow_CheckedChanged(object sender, EventArgs e)
        {
            if (!_radGlow.Checked)
                return;

            ((AssStyleOptions)_lstStyles.SelectedItem).ShadowType = ShadowType.Glow;
            UpdateStylePreview();
        }

        private void _radSoftShadow_CheckedChanged(object sender, EventArgs e)
        {
            if (!_radSoftShadow.Checked)
                return;

            ((AssStyleOptions)_lstStyles.SelectedItem).ShadowType = ShadowType.SoftShadow;
            UpdateStylePreview();
        }

        private void _radHardShadow_CheckedChanged(object sender, EventArgs e)
        {
            if (!_radHardShadow.Checked)
                return;

            ((AssStyleOptions)_lstStyles.SelectedItem).ShadowType = ShadowType.HardShadow;
            UpdateStylePreview();
        }

        private void UpdateStylePreview()
        {
            AssStyleOptions options = (AssStyleOptions)_lstStyles.SelectedItem;
            AssStyle style = _styles?[options.Name];
            _brwPreview.DocumentText = StylePreviewGenerator.GenerateHtml(style, options);
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
                    outputDoc.Shift(new TimeSpan(0, 0, 0, 0, -60));
                    outputDoc.CloseGaps();

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
            SaveStyleOptions();
        }

        private void LoadStyleOptions()
        {
            if (!File.Exists(StyleOptionsFileName))
            {
                _styleOptions = new Dictionary<string, AssStyleOptions>();
                return;
            }

            using (Stream stream = File.Open(StyleOptionsFileName, FileMode.Open, FileAccess.Read))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(AssStyleOptionsList));
                AssStyleOptionsList options = (AssStyleOptionsList)serializer.Deserialize(stream);
                _styleOptions = options.Options.ToDictionary(o => o.Name);
            }
        }

        private void SaveStyleOptions()
        {
            using (Stream stream = File.Open(StyleOptionsFileName, FileMode.Create, FileAccess.Write))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(AssStyleOptionsList));
                AssStyleOptionsList options = new AssStyleOptionsList(_styleOptions.Values);
                serializer.Serialize(stream, options);
            }
        }
    }
}
