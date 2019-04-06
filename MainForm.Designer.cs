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
            this._spltStyleOptions = new System.Windows.Forms.SplitContainer();
            this._lstStyles = new System.Windows.Forms.ListBox();
            this._brwPreview = new System.Windows.Forms.WebBrowser();
            this._pnlOptions = new System.Windows.Forms.Panel();
            this._pnlKaraokeType = new System.Windows.Forms.Panel();
            this._btnPickShadowColor = new System.Windows.Forms.Button();
            this._btnPickOutlineColor = new System.Windows.Forms.Button();
            this._btnPickTextColor = new System.Windows.Forms.Button();
            this._txtCurrentWordShadowColor = new System.Windows.Forms.TextBox();
            this._txtCurrentWordOutlineColor = new System.Windows.Forms.TextBox();
            this._txtCurrentWordTextColor = new System.Windows.Forms.TextBox();
            this._lblCurrentWordShadowColor = new System.Windows.Forms.Label();
            this._lblCurrentWordOutlineColor = new System.Windows.Forms.Label();
            this._lblCurrentWordTextColor = new System.Windows.Forms.Label();
            this._chkHighlightCurrentWord = new System.Windows.Forms.CheckBox();
            this._chkKaraoke = new System.Windows.Forms.CheckBox();
            this._pnlShadowType = new System.Windows.Forms.Panel();
            this._chkBevel = new System.Windows.Forms.CheckBox();
            this._chkHardShadow = new System.Windows.Forms.CheckBox();
            this._chkSoftShadow = new System.Windows.Forms.CheckBox();
            this._chkGlow = new System.Windows.Forms.CheckBox();
            this._lblShadowTypes = new System.Windows.Forms.Label();
            this._txtInputFile = new System.Windows.Forms.TextBox();
            this._grpStyleOptions = new System.Windows.Forms.GroupBox();
            this._dlgOpenFile = new System.Windows.Forms.OpenFileDialog();
            this._btnConvert = new System.Windows.Forms.Button();
            this._lblConversionSuccess = new System.Windows.Forms.Label();
            this._btnBrowse = new System.Windows.Forms.Button();
            this._chkStyleOptions = new System.Windows.Forms.CheckBox();
            this._dlgColor = new System.Windows.Forms.ColorDialog();
            ((System.ComponentModel.ISupportInitialize)(this._spltStyleOptions)).BeginInit();
            this._spltStyleOptions.Panel1.SuspendLayout();
            this._spltStyleOptions.Panel2.SuspendLayout();
            this._spltStyleOptions.SuspendLayout();
            this._pnlOptions.SuspendLayout();
            this._pnlKaraokeType.SuspendLayout();
            this._pnlShadowType.SuspendLayout();
            this._grpStyleOptions.SuspendLayout();
            this.SuspendLayout();
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
            this._spltStyleOptions.Panel2.Controls.Add(this._pnlOptions);
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
            this._lstStyles.TabIndex = 0;
            this._lstStyles.SelectedIndexChanged += new System.EventHandler(this._lstStyles_SelectedIndexChanged);
            // 
            // _brwPreview
            // 
            this._brwPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this._brwPreview.Location = new System.Drawing.Point(0, 119);
            this._brwPreview.MinimumSize = new System.Drawing.Size(20, 20);
            this._brwPreview.Name = "_brwPreview";
            this._brwPreview.Size = new System.Drawing.Size(468, 212);
            this._brwPreview.TabIndex = 0;
            // 
            // _pnlOptions
            // 
            this._pnlOptions.Controls.Add(this._pnlKaraokeType);
            this._pnlOptions.Controls.Add(this._pnlShadowType);
            this._pnlOptions.Dock = System.Windows.Forms.DockStyle.Top;
            this._pnlOptions.Location = new System.Drawing.Point(0, 0);
            this._pnlOptions.Name = "_pnlOptions";
            this._pnlOptions.Size = new System.Drawing.Size(468, 119);
            this._pnlOptions.TabIndex = 6;
            // 
            // _pnlKaraokeType
            // 
            this._pnlKaraokeType.Controls.Add(this._btnPickShadowColor);
            this._pnlKaraokeType.Controls.Add(this._btnPickOutlineColor);
            this._pnlKaraokeType.Controls.Add(this._btnPickTextColor);
            this._pnlKaraokeType.Controls.Add(this._txtCurrentWordShadowColor);
            this._pnlKaraokeType.Controls.Add(this._txtCurrentWordOutlineColor);
            this._pnlKaraokeType.Controls.Add(this._txtCurrentWordTextColor);
            this._pnlKaraokeType.Controls.Add(this._lblCurrentWordShadowColor);
            this._pnlKaraokeType.Controls.Add(this._lblCurrentWordOutlineColor);
            this._pnlKaraokeType.Controls.Add(this._lblCurrentWordTextColor);
            this._pnlKaraokeType.Controls.Add(this._chkHighlightCurrentWord);
            this._pnlKaraokeType.Controls.Add(this._chkKaraoke);
            this._pnlKaraokeType.Dock = System.Windows.Forms.DockStyle.Fill;
            this._pnlKaraokeType.Location = new System.Drawing.Point(198, 0);
            this._pnlKaraokeType.Name = "_pnlKaraokeType";
            this._pnlKaraokeType.Size = new System.Drawing.Size(270, 119);
            this._pnlKaraokeType.TabIndex = 6;
            // 
            // _btnPickShadowColor
            // 
            this._btnPickShadowColor.Location = new System.Drawing.Point(182, 89);
            this._btnPickShadowColor.Name = "_btnPickShadowColor";
            this._btnPickShadowColor.Size = new System.Drawing.Size(35, 20);
            this._btnPickShadowColor.TabIndex = 10;
            this._btnPickShadowColor.Text = "...";
            this._btnPickShadowColor.UseVisualStyleBackColor = true;
            this._btnPickShadowColor.Click += new System.EventHandler(this._btnPickShadowColor_Click);
            // 
            // _btnPickOutlineColor
            // 
            this._btnPickOutlineColor.Location = new System.Drawing.Point(182, 68);
            this._btnPickOutlineColor.Name = "_btnPickOutlineColor";
            this._btnPickOutlineColor.Size = new System.Drawing.Size(35, 20);
            this._btnPickOutlineColor.TabIndex = 8;
            this._btnPickOutlineColor.Text = "...";
            this._btnPickOutlineColor.UseVisualStyleBackColor = true;
            this._btnPickOutlineColor.Click += new System.EventHandler(this._btnPickOutlineColor_Click);
            // 
            // _btnPickTextColor
            // 
            this._btnPickTextColor.Location = new System.Drawing.Point(182, 47);
            this._btnPickTextColor.Name = "_btnPickTextColor";
            this._btnPickTextColor.Size = new System.Drawing.Size(35, 20);
            this._btnPickTextColor.TabIndex = 6;
            this._btnPickTextColor.Text = "...";
            this._btnPickTextColor.UseVisualStyleBackColor = true;
            this._btnPickTextColor.Click += new System.EventHandler(this._btnPickTextColor_Click);
            // 
            // _txtCurrentWordShadowColor
            // 
            this._txtCurrentWordShadowColor.Enabled = false;
            this._txtCurrentWordShadowColor.Location = new System.Drawing.Point(111, 89);
            this._txtCurrentWordShadowColor.Name = "_txtCurrentWordShadowColor";
            this._txtCurrentWordShadowColor.Size = new System.Drawing.Size(65, 20);
            this._txtCurrentWordShadowColor.TabIndex = 9;
            this._txtCurrentWordShadowColor.TextChanged += new System.EventHandler(this._txtCurrentWordShadowColor_TextChanged);
            // 
            // _txtCurrentWordOutlineColor
            // 
            this._txtCurrentWordOutlineColor.Enabled = false;
            this._txtCurrentWordOutlineColor.Location = new System.Drawing.Point(111, 68);
            this._txtCurrentWordOutlineColor.Name = "_txtCurrentWordOutlineColor";
            this._txtCurrentWordOutlineColor.Size = new System.Drawing.Size(65, 20);
            this._txtCurrentWordOutlineColor.TabIndex = 7;
            this._txtCurrentWordOutlineColor.TextChanged += new System.EventHandler(this._txtCurrentWordOutlineColor_TextChanged);
            // 
            // _txtCurrentWordTextColor
            // 
            this._txtCurrentWordTextColor.Enabled = false;
            this._txtCurrentWordTextColor.Location = new System.Drawing.Point(111, 47);
            this._txtCurrentWordTextColor.Name = "_txtCurrentWordTextColor";
            this._txtCurrentWordTextColor.Size = new System.Drawing.Size(65, 20);
            this._txtCurrentWordTextColor.TabIndex = 5;
            this._txtCurrentWordTextColor.TextChanged += new System.EventHandler(this._txtCurrentWordTextColor_TextChanged);
            // 
            // _lblCurrentWordShadowColor
            // 
            this._lblCurrentWordShadowColor.AutoSize = true;
            this._lblCurrentWordShadowColor.Location = new System.Drawing.Point(31, 92);
            this._lblCurrentWordShadowColor.Name = "_lblCurrentWordShadowColor";
            this._lblCurrentWordShadowColor.Size = new System.Drawing.Size(75, 13);
            this._lblCurrentWordShadowColor.TabIndex = 4;
            this._lblCurrentWordShadowColor.Text = "Shadow color:";
            // 
            // _lblCurrentWordOutlineColor
            // 
            this._lblCurrentWordOutlineColor.AutoSize = true;
            this._lblCurrentWordOutlineColor.Location = new System.Drawing.Point(31, 71);
            this._lblCurrentWordOutlineColor.Name = "_lblCurrentWordOutlineColor";
            this._lblCurrentWordOutlineColor.Size = new System.Drawing.Size(69, 13);
            this._lblCurrentWordOutlineColor.TabIndex = 3;
            this._lblCurrentWordOutlineColor.Text = "Outline color:";
            // 
            // _lblCurrentWordTextColor
            // 
            this._lblCurrentWordTextColor.AutoSize = true;
            this._lblCurrentWordTextColor.Location = new System.Drawing.Point(31, 52);
            this._lblCurrentWordTextColor.Name = "_lblCurrentWordTextColor";
            this._lblCurrentWordTextColor.Size = new System.Drawing.Size(57, 13);
            this._lblCurrentWordTextColor.TabIndex = 2;
            this._lblCurrentWordTextColor.Text = "Text color:";
            // 
            // _chkHighlightCurrentWord
            // 
            this._chkHighlightCurrentWord.AutoSize = true;
            this._chkHighlightCurrentWord.Enabled = false;
            this._chkHighlightCurrentWord.Location = new System.Drawing.Point(6, 31);
            this._chkHighlightCurrentWord.Name = "_chkHighlightCurrentWord";
            this._chkHighlightCurrentWord.Size = new System.Drawing.Size(129, 17);
            this._chkHighlightCurrentWord.TabIndex = 1;
            this._chkHighlightCurrentWord.Text = "Highlight current word";
            this._chkHighlightCurrentWord.UseVisualStyleBackColor = true;
            this._chkHighlightCurrentWord.CheckedChanged += new System.EventHandler(this._chkHighlightCurrentWord_CheckedChanged);
            // 
            // _chkKaraoke
            // 
            this._chkKaraoke.AutoSize = true;
            this._chkKaraoke.Location = new System.Drawing.Point(6, 8);
            this._chkKaraoke.Name = "_chkKaraoke";
            this._chkKaraoke.Size = new System.Drawing.Size(102, 17);
            this._chkKaraoke.TabIndex = 0;
            this._chkKaraoke.Text = "Use for karaoke";
            this._chkKaraoke.UseVisualStyleBackColor = true;
            this._chkKaraoke.CheckedChanged += new System.EventHandler(this._chkKaraoke_CheckedChanged);
            // 
            // _pnlShadowType
            // 
            this._pnlShadowType.Controls.Add(this._chkBevel);
            this._pnlShadowType.Controls.Add(this._chkHardShadow);
            this._pnlShadowType.Controls.Add(this._chkSoftShadow);
            this._pnlShadowType.Controls.Add(this._chkGlow);
            this._pnlShadowType.Controls.Add(this._lblShadowTypes);
            this._pnlShadowType.Dock = System.Windows.Forms.DockStyle.Left;
            this._pnlShadowType.Location = new System.Drawing.Point(0, 0);
            this._pnlShadowType.Name = "_pnlShadowType";
            this._pnlShadowType.Size = new System.Drawing.Size(198, 119);
            this._pnlShadowType.TabIndex = 5;
            // 
            // _chkBevel
            // 
            this._chkBevel.AutoSize = true;
            this._chkBevel.Location = new System.Drawing.Point(26, 48);
            this._chkBevel.Name = "_chkBevel";
            this._chkBevel.Size = new System.Drawing.Size(53, 17);
            this._chkBevel.TabIndex = 1;
            this._chkBevel.Text = "Bevel";
            this._chkBevel.UseVisualStyleBackColor = true;
            this._chkBevel.CheckedChanged += new System.EventHandler(this._chkBevel_CheckedChanged);
            // 
            // _chkHardShadow
            // 
            this._chkHardShadow.AutoSize = true;
            this._chkHardShadow.Location = new System.Drawing.Point(26, 80);
            this._chkHardShadow.Name = "_chkHardShadow";
            this._chkHardShadow.Size = new System.Drawing.Size(89, 17);
            this._chkHardShadow.TabIndex = 3;
            this._chkHardShadow.Text = "Hard shadow";
            this._chkHardShadow.UseVisualStyleBackColor = true;
            this._chkHardShadow.CheckedChanged += new System.EventHandler(this._chkHardShadow_CheckedChanged);
            // 
            // _chkSoftShadow
            // 
            this._chkSoftShadow.AutoSize = true;
            this._chkSoftShadow.Location = new System.Drawing.Point(26, 64);
            this._chkSoftShadow.Name = "_chkSoftShadow";
            this._chkSoftShadow.Size = new System.Drawing.Size(85, 17);
            this._chkSoftShadow.TabIndex = 2;
            this._chkSoftShadow.Text = "Soft shadow";
            this._chkSoftShadow.UseVisualStyleBackColor = true;
            this._chkSoftShadow.CheckedChanged += new System.EventHandler(this._chkSoftShadow_CheckedChanged);
            // 
            // _chkGlow
            // 
            this._chkGlow.AutoSize = true;
            this._chkGlow.Location = new System.Drawing.Point(26, 32);
            this._chkGlow.Name = "_chkGlow";
            this._chkGlow.Size = new System.Drawing.Size(50, 17);
            this._chkGlow.TabIndex = 0;
            this._chkGlow.Text = "Glow";
            this._chkGlow.UseVisualStyleBackColor = true;
            this._chkGlow.CheckedChanged += new System.EventHandler(this._chkGlow_CheckedChanged);
            // 
            // _lblShadowTypes
            // 
            this._lblShadowTypes.AutoSize = true;
            this._lblShadowTypes.Location = new System.Drawing.Point(8, 9);
            this._lblShadowTypes.Name = "_lblShadowTypes";
            this._lblShadowTypes.Size = new System.Drawing.Size(77, 13);
            this._lblShadowTypes.TabIndex = 3;
            this._lblShadowTypes.Text = "Shadow types:";
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
            // _dlgOpenFile
            // 
            this._dlgOpenFile.Filter = "Advanced SubStation Alpha|*.ass|YouTube subtitles|*.sbv";
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
            this._lblConversionSuccess.Text = "Status label";
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
            // _chkStyleOptions
            // 
            this._chkStyleOptions.Appearance = System.Windows.Forms.Appearance.Button;
            this._chkStyleOptions.AutoSize = true;
            this._chkStyleOptions.Location = new System.Drawing.Point(24, 29);
            this._chkStyleOptions.Name = "_chkStyleOptions";
            this._chkStyleOptions.Size = new System.Drawing.Size(77, 23);
            this._chkStyleOptions.TabIndex = 0;
            this._chkStyleOptions.Text = "Style options";
            this._chkStyleOptions.UseVisualStyleBackColor = true;
            this._chkStyleOptions.CheckedChanged += new System.EventHandler(this._chkStyleOptions_CheckedChanged);
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
            this.Text = "YTSubConverter";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
            this._spltStyleOptions.Panel1.ResumeLayout(false);
            this._spltStyleOptions.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._spltStyleOptions)).EndInit();
            this._spltStyleOptions.ResumeLayout(false);
            this._pnlOptions.ResumeLayout(false);
            this._pnlKaraokeType.ResumeLayout(false);
            this._pnlKaraokeType.PerformLayout();
            this._pnlShadowType.ResumeLayout(false);
            this._pnlShadowType.PerformLayout();
            this._grpStyleOptions.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.WebBrowser _brwPreview;
        private System.Windows.Forms.TextBox _txtInputFile;
        private System.Windows.Forms.GroupBox _grpStyleOptions;
        private System.Windows.Forms.ListBox _lstStyles;
        private System.Windows.Forms.OpenFileDialog _dlgOpenFile;
        private System.Windows.Forms.SplitContainer _spltStyleOptions;
        private System.Windows.Forms.Label _lblShadowTypes;
        private System.Windows.Forms.Button _btnConvert;
        private System.Windows.Forms.Label _lblConversionSuccess;
        private System.Windows.Forms.Panel _pnlShadowType;
        private System.Windows.Forms.Button _btnBrowse;
        private System.Windows.Forms.Panel _pnlOptions;
        private System.Windows.Forms.Panel _pnlKaraokeType;
        private System.Windows.Forms.TextBox _txtCurrentWordOutlineColor;
        private System.Windows.Forms.TextBox _txtCurrentWordTextColor;
        private System.Windows.Forms.Label _lblCurrentWordOutlineColor;
        private System.Windows.Forms.Label _lblCurrentWordTextColor;
        private System.Windows.Forms.CheckBox _chkHighlightCurrentWord;
        private System.Windows.Forms.CheckBox _chkKaraoke;
        private System.Windows.Forms.CheckBox _chkHardShadow;
        private System.Windows.Forms.CheckBox _chkSoftShadow;
        private System.Windows.Forms.CheckBox _chkGlow;
        private System.Windows.Forms.CheckBox _chkStyleOptions;
        private System.Windows.Forms.TextBox _txtCurrentWordShadowColor;
        private System.Windows.Forms.Label _lblCurrentWordShadowColor;
        private System.Windows.Forms.Button _btnPickShadowColor;
        private System.Windows.Forms.Button _btnPickOutlineColor;
        private System.Windows.Forms.Button _btnPickTextColor;
        private System.Windows.Forms.ColorDialog _dlgColor;
        private System.Windows.Forms.CheckBox _chkBevel;
    }
}