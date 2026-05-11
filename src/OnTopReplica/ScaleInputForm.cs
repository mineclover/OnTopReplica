using System;
using System.Windows.Forms;

namespace OnTopReplica {
    /// <summary>
    /// Simple form for entering scale percentage with live preview and Cancel-restore.
    /// </summary>
    internal class ScaleInputForm : Form {
        private NumericUpDown numericScale;
        private Button buttonOK;
        private Button buttonCancel;
        private Label labelScale;
        private Label labelPercent;

        private readonly MainForm _targetForm;
        private System.Drawing.Size _originalClientSize;
        private bool _restoreOnClose = true;

        public double ScalePercentage {
            get { return (double)numericScale.Value; }
            set { numericScale.Value = (decimal)value; }
        }

        public ScaleInputForm(MainForm targetForm) {
            _targetForm = targetForm;
            InitializeComponent();

            if (_targetForm != null) {
                _originalClientSize = _targetForm.ClientSize;
            }
        }

        /// <summary>
        /// Computes the scale factor as ratio (e.g. 100% -> 1.0). Exposed for testability.
        /// </summary>
        public static double ToScaleFactor(decimal percentage) {
            return (double)percentage / 100.0;
        }

        private void InitializeComponent() {
            this.numericScale = new NumericUpDown();
            this.buttonOK = new Button();
            this.buttonCancel = new Button();
            this.labelScale = new Label();
            this.labelPercent = new Label();

            ((System.ComponentModel.ISupportInitialize)(this.numericScale)).BeginInit();
            this.SuspendLayout();

            this.labelScale.AutoSize = true;
            this.labelScale.Location = new System.Drawing.Point(12, 15);
            this.labelScale.Name = "labelScale";
            this.labelScale.Size = new System.Drawing.Size(41, 13);
            this.labelScale.TabIndex = 0;
            this.labelScale.Text = Strings.MenuCtxScale;

            this.numericScale.Location = new System.Drawing.Point(70, 12);
            this.numericScale.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            this.numericScale.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            this.numericScale.Name = "numericScale";
            this.numericScale.Size = new System.Drawing.Size(80, 20);
            this.numericScale.TabIndex = 1;
            this.numericScale.DecimalPlaces = 2;
            this.numericScale.Increment = new decimal(new int[] { 1, 0, 0, 0 });
            this.numericScale.Value = new decimal(new int[] { 100, 0, 0, 0 });
            this.numericScale.ValueChanged += NumericValue_Changed;

            this.labelPercent.AutoSize = true;
            this.labelPercent.Location = new System.Drawing.Point(156, 15);
            this.labelPercent.Name = "labelPercent";
            this.labelPercent.Size = new System.Drawing.Size(15, 13);
            this.labelPercent.TabIndex = 2;
            this.labelPercent.Text = "%";

            this.buttonOK.DialogResult = DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(14, 45);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 3;
            this.buttonOK.Text = Strings.MenuCtxOk;
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += (s, e) => _restoreOnClose = false;

            this.buttonCancel.DialogResult = DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(95, 45);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = Strings.MenuCtxCancel;
            this.buttonCancel.UseVisualStyleBackColor = true;

            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(184, 80);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.labelPercent);
            this.Controls.Add(this.numericScale);
            this.Controls.Add(this.labelScale);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ScaleInputForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = Strings.MenuCtxSetScale;
            this.TopMost = true;

            ((System.ComponentModel.ISupportInitialize)(this.numericScale)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void NumericValue_Changed(object sender, EventArgs e) {
            if (_targetForm == null) return;

            bool hasSource = _targetForm.ThumbnailPanel.IsShowingThumbnail
                          || _targetForm.CurrentSourceMode == MainForm.SourceMode.Image;
            if (!hasSource) return;

            _targetForm.FitToThumbnailScale(ToScaleFactor(numericScale.Value));
        }

        protected override void OnFormClosing(FormClosingEventArgs e) {
            base.OnFormClosing(e);

            if (_restoreOnClose && _targetForm != null && DialogResult != DialogResult.OK) {
                _targetForm.ClientSize = _originalClientSize;
            }
        }
    }
}
