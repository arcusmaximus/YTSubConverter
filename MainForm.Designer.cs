namespace Arc.YTSubConverter
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._brwPreview = new System.Windows.Forms.WebBrowser();
            this._txtInputFile = new System.Windows.Forms.TextBox();
            this._grpStyleOptions = new System.Windows.Forms.GroupBox();
            this._spltStyleOptions = new System.Windows.Forms.SplitContainer();
            this._lstStyles = new System.Windows.Forms.ListBox();
            this._pnlShadowType = new System.Windows.Forms.Panel();
            this._txtCurrentWordGlowColor = new System.Windows.Forms.TextBox();
            this._txtCurrentWordTextColor = new System.Windows.Forms.TextBox();
            this._lblCurrentWordGlowColor = new System.Windows.Forms.Label();
            this._lblCurrentWordTextColor = new System.Windows.Forms.Label();
            this._chkHighlightCurrentWord = new System.Windows.Forms.CheckBox();
            this._chkKaraoke = new System.Windows.Forms.CheckBox();
            this._lblShadowType = new System.Windows.Forms.Label();
            this._radGlow = new System.Windows.Forms.RadioButton();
            this._radSoftShadow = new System.Windows.Forms.RadioButton();
            this._radHardShadow = new System.Windows.Forms.RadioButton();
            this._dlgOpenFile = new System.Windows.Forms.OpenFileDialog();
            this._chkStyleOptions = new System.Windows.Forms.CheckBox();
            this._btnConvert = new System.Windows.Forms.Button();
            this._lblConversionSuccess = new System.Windows.Forms.Label();
            this._btnBrowse = new System.Windows.Forms.Button();
            this._grpStyleOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._spltStyleOptions)).BeginInit();
            this._spltStyleOptions.Panel1.SuspendLayout();
            this._spltStyleOptions.Panel2.SuspendLayout();
            this._spltStyleOptions.SuspendLayout();
            this._pnlShadowType.SuspendLayout();
            this.SuspendLayout();
            // 
            // _brwPreview
            // 
            this._brwPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this._brwPreview.Location = new System.Drawing.Point(0, 99);
            this._brwPreview.MinimumSize = new System.Drawing.Size(20, 20);
            this._brwPreview.Name = "_brwPreview";
            this._brwPreview.Size = new System.Drawing.Size(468, 232);
            this._brwPreview.TabIndex = 0;
            // 
            // _txtInputFile
            // 
            this._txtInputFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._txtInputFile.Location = new System.Drawing.Point(20, 6);
            this._txtInputFile.Name = "_txtInputFile";
            this._txtInputFile.ReadOnly = true;
            this._txtInputFile.Size = new System.Drawing.Size(721, 20);
            this._txtInputFile.TabIndex = 2;
            // 
            // _grpStyleOptions
            // 
            this._grpStyleOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._grpStyleOptions.Controls.Add(this._spltStyleOptions);
            this._grpStyleOptions.Location = new System.Drawing.Point(15, 32);
            this._grpStyleOptions.Name = "_grpStyleOptions";
            this._grpStyleOptions.Padding = new System.Windows.Forms.Padding(5, 10, 5, 5);
            this._grpStyleOptions.Size = new System.Drawing.Size(773, 359);
            this._grpStyleOptions.TabIndex = 4;
            this._grpStyleOptions.TabStop = false;
            // 
            // _spltStyleOptions
            // 
            this._spltStyleOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this._spltStyleOptions.Location = new System.Drawing.Point(5, 23);
            this._spltStyleOptions.Name = "_spltStyleOptions";
            // 
            // _spltStyleOptions.Panel1
            // 
            this._spltStyleOptions.Panel1.Controls.Add(this._lstStyles);
            // 
            // _spltStyleOptions.Panel2
            // 
            this._spltStyleOptions.Panel2.Controls.Add(this._brwPreview);
            this._spltStyleOptions.Panel2.Controls.Add(this._pnlShadowType);
            this._spltStyleOptions.Size = new System.Drawing.Size(763, 331);
            this._spltStyleOptions.SplitterDistance = 291;
            this._spltStyleOptions.TabIndex = 5;
            // 
            // _lstStyles
            // 
            this._lstStyles.Dock = System.Windows.Forms.DockStyle.Fill;
            this._lstStyles.FormattingEnabled = true;
            this._lstStyles.IntegralHeight = false;
            this._lstStyles.Location = new System.Drawing.Point(0, 0);
            this._lstStyles.Name = "_lstStyles";
            this._lstStyles.Size = new System.Drawing.Size(291, 331);
            this._lstStyles.TabIndex = 2;
            this._lstStyles.SelectedIndexChanged += new System.EventHandler(this._lstStyles_SelectedIndexChanged);
            // 
            // _pnlShadowType
            // 
            this._pnlShadowType.Controls.Add(this._txtCurrentWordGlowColor);
            this._pnlShadowType.Controls.Add(this._txtCurrentWordTextColor);
            this._pnlShadowType.Controls.Add(this._lblCurrentWordGlowColor);
            this._pnlShadowType.Controls.Add(this._lblCurrentWordTextColor);
            this._pnlShadowType.Controls.Add(this._chkHighlightCurrentWord);
            this._pnlShadowType.Controls.Add(this._chkKaraoke);
            this._pnlShadowType.Controls.Add(this._lblShadowType);
            this._pnlShadowType.Controls.Add(this._radGlow);
            this._pnlShadowType.Controls.Add(this._radSoftShadow);
            this._pnlShadowType.Controls.Add(this._radHardShadow);
            this._pnlShadowType.Dock = System.Windows.Forms.DockStyle.Top;
            this._pnlShadowType.Location = new System.Drawing.Point(0, 0);
            this._pnlShadowType.Name = "_pnlShadowType";
            this._pnlShadowType.Size = new System.Drawing.Size(468, 99);
            this._pnlShadowType.TabIndex = 5;
            // 
            // _txtCurrentWordGlowColor
            // 
            this._txtCurrentWordGlowColor.Enabled = false;
            this._txtCurrentWordGlowColor.Location = new System.Drawing.Point(309, 68);
            this._txtCurrentWordGlowColor.Name = "_txtCurrentWordGlowColor";
            this._txtCurrentWordGlowColor.Size = new System.Drawing.Size(65, 20);
            this._txtCurrentWordGlowColor.TabIndex = 9;
            this._txtCurrentWordGlowColor.TextChanged += new System.EventHandler(this._txtCurrentWordGlowColor_TextChanged);
            // 
            // _txtCurrentWordTextColor
            // 
            this._txtCurrentWordTextColor.Enabled = false;
            this._txtCurrentWordTextColor.Location = new System.Drawing.Point(309, 47);
            this._txtCurrentWordTextColor.Name = "_txtCurrentWordTextColor";
            this._txtCurrentWordTextColor.Size = new System.Drawing.Size(65, 20);
            this._txtCurrentWordTextColor.TabIndex = 9;
            this._txtCurrentWordTextColor.TextChanged += new System.EventHandler(this._txtCurrentWordTextColor_TextChanged);
            // 
            // _lblCurrentWordGlowColor
            // 
            this._lblCurrentWordGlowColor.AutoSize = true;
            this._lblCurrentWordGlowColor.Location = new System.Drawing.Point(229, 71);
            this._lblCurrentWordGlowColor.Name = "_lblCurrentWordGlowColor";
            this._lblCurrentWordGlowColor.Size = new System.Drawing.Size(60, 13);
            this._lblCurrentWordGlowColor.TabIndex = 8;
            this._lblCurrentWordGlowColor.Text = "Glow color:";
            // 
            // _lblCurrentWordTextColor
            // 
            this._lblCurrentWordTextColor.AutoSize = true;
            this._lblCurrentWordTextColor.Location = new System.Drawing.Point(229, 52);
            this._lblCurrentWordTextColor.Name = "_lblCurrentWordTextColor";
            this._lblCurrentWordTextColor.Size = new System.Drawing.Size(57, 13);
            this._lblCurrentWordTextColor.TabIndex = 7;
            this._lblCurrentWordTextColor.Text = "Text color:";
            // 
            // _chkHighlightCurrentWord
            // 
            this._chkHighlightCurrentWord.AutoSize = true;
            this._chkHighlightCurrentWord.Enabled = false;
            this._chkHighlightCurrentWord.Location = new System.Drawing.Point(204, 31);
            this._chkHighlightCurrentWord.Name = "_chkHighlightCurrentWord";
            this._chkHighlightCurrentWord.Size = new System.Drawing.Size(129, 17);
            this._chkHighlightCurrentWord.TabIndex = 6;
            this._chkHighlightCurrentWord.Text = "Highlight current word";
            this._chkHighlightCurrentWord.UseVisualStyleBackColor = true;
            this._chkHighlightCurrentWord.CheckedChanged += new System.EventHandler(this._chkHighlightCurrentWord_CheckedChanged);
            // 
            // _chkKaraoke
            // 
            this._chkKaraoke.AutoSize = true;
            this._chkKaraoke.Location = new System.Drawing.Point(204, 8);
            this._chkKaraoke.Name = "_chkKaraoke";
            this._chkKaraoke.Size = new System.Drawing.Size(102, 17);
            this._chkKaraoke.TabIndex = 5;
            this._chkKaraoke.Text = "Use for karaoke";
            this._chkKaraoke.UseVisualStyleBackColor = true;
            this._chkKaraoke.CheckedChanged += new System.EventHandler(this._chkKaraoke_CheckedChanged);
            // 
            // _lblShadowType
            // 
            this._lblShadowType.AutoSize = true;
            this._lblShadowType.Location = new System.Drawing.Point(8, 9);
            this._lblShadowType.Name = "_lblShadowType";
            this._lblShadowType.Size = new System.Drawing.Size(72, 13);
            this._lblShadowType.TabIndex = 3;
            this._lblShadowType.Text = "Shadow type:";
            // 
            // _radGlow
            // 
            this._radGlow.AutoSize = true;
            this._radGlow.Location = new System.Drawing.Point(26, 25);
            this._radGlow.Name = "_radGlow";
            this._radGlow.Size = new System.Drawing.Size(49, 17);
            this._radGlow.TabIndex = 4;
            this._radGlow.TabStop = true;
            this._radGlow.Text = "Glow";
            this._radGlow.UseVisualStyleBackColor = true;
            this._radGlow.CheckedChanged += new System.EventHandler(this._radGlow_CheckedChanged);
            // 
            // _radSoftShadow
            // 
            this._radSoftShadow.AutoSize = true;
            this._radSoftShadow.Location = new System.Drawing.Point(26, 48);
            this._radSoftShadow.Name = "_radSoftShadow";
            this._radSoftShadow.Size = new System.Drawing.Size(84, 17);
            this._radSoftShadow.TabIndex = 4;
            this._radSoftShadow.TabStop = true;
            this._radSoftShadow.Text = "Soft shadow";
            this._radSoftShadow.UseVisualStyleBackColor = true;
            this._radSoftShadow.CheckedChanged += new System.EventHandler(this._radSoftShadow_CheckedChanged);
            // 
            // _radHardShadow
            // 
            this._radHardShadow.AutoSize = true;
            this._radHardShadow.Location = new System.Drawing.Point(26, 70);
            this._radHardShadow.Name = "_radHardShadow";
            this._radHardShadow.Size = new System.Drawing.Size(88, 17);
            this._radHardShadow.TabIndex = 4;
            this._radHardShadow.TabStop = true;
            this._radHardShadow.Text = "Hard shadow";
            this._radHardShadow.UseVisualStyleBackColor = true;
            this._radHardShadow.CheckedChanged += new System.EventHandler(this._radHardShadow_CheckedChanged);
            // 
            // _dlgOpenFile
            // 
            this._dlgOpenFile.Filter = "Advanced SubStation Alpha|*.ass|YouTube subtitles|*.sbv";
            // 
            // _chkStyleOptions
            // 
            this._chkStyleOptions.Appearance = System.Windows.Forms.Appearance.Button;
            this._chkStyleOptions.AutoSize = true;
            this._chkStyleOptions.Location = new System.Drawing.Point(24, 29);
            this._chkStyleOptions.Name = "_chkStyleOptions";
            this._chkStyleOptions.Size = new System.Drawing.Size(77, 23);
            this._chkStyleOptions.TabIndex = 1;
            this._chkStyleOptions.Text = "Style options";
            this._chkStyleOptions.UseVisualStyleBackColor = true;
            this._chkStyleOptions.CheckedChanged += new System.EventHandler(this._chkStyleOptions_CheckedChanged);
            // 
            // _btnConvert
            // 
            this._btnConvert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._btnConvert.Location = new System.Drawing.Point(659, 397);
            this._btnConvert.Name = "_btnConvert";
            this._btnConvert.Size = new System.Drawing.Size(129, 41);
            this._btnConvert.TabIndex = 5;
            this._btnConvert.Text = "Convert";
            this._btnConvert.UseVisualStyleBackColor = true;
            this._btnConvert.Click += new System.EventHandler(this._btnConvert_Click);
            // 
            // _lblConversionSuccess
            // 
            this._lblConversionSuccess.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._lblConversionSuccess.ForeColor = System.Drawing.Color.Green;
            this._lblConversionSuccess.Location = new System.Drawing.Point(326, 411);
            this._lblConversionSuccess.Name = "_lblConversionSuccess";
            this._lblConversionSuccess.Size = new System.Drawing.Size(327, 13);
            this._lblConversionSuccess.TabIndex = 6;
            this._lblConversionSuccess.Text = "Conversion successful!";
            this._lblConversionSuccess.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this._lblConversionSuccess.Visible = false;
            // 
            // _btnBrowse
            // 
            this._btnBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnBrowse.Location = new System.Drawing.Point(744, 6);
            this._btnBrowse.Name = "_btnBrowse";
            this._btnBrowse.Size = new System.Drawing.Size(44, 20);
            this._btnBrowse.TabIndex = 0;
            this._btnBrowse.Text = "...";
            this._btnBrowse.UseVisualStyleBackColor = true;
            this._btnBrowse.Click += new System.EventHandler(this._btnBrowse_Click);
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this._lblConversionSuccess);
            this.Controls.Add(this._btnConvert);
            this.Controls.Add(this._chkStyleOptions);
            this.Controls.Add(this._grpStyleOptions);
            this.Controls.Add(this._btnBrowse);
            this.Controls.Add(this._txtInputFile);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "YouTube Styled Subtitle Converter";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
            this._grpStyleOptions.ResumeLayout(false);
            this._spltStyleOptions.Panel1.ResumeLayout(false);
            this._spltStyleOptions.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._spltStyleOptions)).EndInit();
            this._spltStyleOptions.ResumeLayout(false);
            this._pnlShadowType.ResumeLayout(false);
            this._pnlShadowType.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.WebBrowser _brwPreview;
        private System.Windows.Forms.TextBox _txtInputFile;
        private System.Windows.Forms.GroupBox _grpStyleOptions;
        private System.Windows.Forms.ListBox _lstStyles;
        private System.Windows.Forms.CheckBox _chkStyleOptions;
        private System.Windows.Forms.OpenFileDialog _dlgOpenFile;
        private System.Windows.Forms.SplitContainer _spltStyleOptions;
        private System.Windows.Forms.Label _lblShadowType;
        private System.Windows.Forms.RadioButton _radSoftShadow;
        private System.Windows.Forms.RadioButton _radHardShadow;
        private System.Windows.Forms.RadioButton _radGlow;
        private System.Windows.Forms.Button _btnConvert;
        private System.Windows.Forms.Label _lblConversionSuccess;
        private System.Windows.Forms.Panel _pnlShadowType;
        private System.Windows.Forms.Button _btnBrowse;
        private System.Windows.Forms.CheckBox _chkHighlightCurrentWord;
        private System.Windows.Forms.CheckBox _chkKaraoke;
        private System.Windows.Forms.Label _lblCurrentWordTextColor;
        private System.Windows.Forms.Label _lblCurrentWordGlowColor;
        private System.Windows.Forms.TextBox _txtCurrentWordGlowColor;
        private System.Windows.Forms.TextBox _txtCurrentWordTextColor;
    }
}