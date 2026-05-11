using System;
using System.Drawing;
using System.Windows.Forms;

namespace OnTopReplica {
    /// <summary>
    /// Simple form for entering window size.
    /// </summary>
    public class SizeInputForm : Form {
        private NumericUpDown numericWidth;
        private NumericUpDown numericHeight;
        private Button buttonOK;
        private Button buttonCancel;
        private Label labelWidth;
        private Label labelHeight;

        private readonly Form _targetForm;
        private Size _originalSize;
        private bool _restoreOnClose = true;

        public Size WindowSize {
            get {
                return new Size((int)numericWidth.Value, (int)numericHeight.Value);
            }
            set {
                numericWidth.Value = Clamp(value.Width, numericWidth.Minimum, numericWidth.Maximum);
                numericHeight.Value = Clamp(value.Height, numericHeight.Minimum, numericHeight.Maximum);
            }
        }

        static decimal Clamp(int v, decimal min, decimal max) {
            if (v < min) return min;
            if (v > max) return max;
            return v;
        }

        public SizeInputForm(Form targetForm) {
            _targetForm = targetForm;
            InitializeComponent();

            if (_targetForm != null) {
                _originalSize = _targetForm.Size;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e) {
            base.OnFormClosing(e);

            if (_restoreOnClose && _targetForm != null && DialogResult != DialogResult.OK) {
                _targetForm.Size = _originalSize;
            }
        }

        private void InitializeComponent() {
            this.numericWidth = new NumericUpDown();
            this.numericHeight = new NumericUpDown();
            this.buttonOK = new Button();
            this.buttonCancel = new Button();
            this.labelWidth = new Label();
            this.labelHeight = new Label();

            ((System.ComponentModel.ISupportInitialize)(this.numericWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericHeight)).BeginInit();
            this.SuspendLayout();

            // labelWidth
            this.labelWidth.AutoSize = true;
            this.labelWidth.Location = new Point(12, 15);
            this.labelWidth.Name = "labelWidth";
            this.labelWidth.Size = new Size(41, 13);
            this.labelWidth.TabIndex = 0;
            this.labelWidth.Text = Strings.MenuCtxWidth;

            // numericWidth
            this.numericWidth.Location = new Point(70, 12);
            this.numericWidth.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            this.numericWidth.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numericWidth.Name = "numericWidth";
            this.numericWidth.Size = new Size(100, 20);
            this.numericWidth.TabIndex = 1;
            this.numericWidth.ValueChanged += new EventHandler(NumericValue_Changed);

            // labelHeight
            this.labelHeight.AutoSize = true;
            this.labelHeight.Location = new Point(12, 45);
            this.labelHeight.Name = "labelHeight";
            this.labelHeight.Size = new Size(44, 13);
            this.labelHeight.TabIndex = 2;
            this.labelHeight.Text = Strings.MenuCtxHeight;

            // numericHeight
            this.numericHeight.Location = new Point(70, 42);
            this.numericHeight.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            this.numericHeight.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numericHeight.Name = "numericHeight";
            this.numericHeight.Size = new Size(100, 20);
            this.numericHeight.TabIndex = 3;
            this.numericHeight.ValueChanged += new EventHandler(NumericValue_Changed);

            // buttonOK
            this.buttonOK.DialogResult = DialogResult.OK;
            this.buttonOK.Location = new Point(14, 75);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new Size(75, 23);
            this.buttonOK.TabIndex = 4;
            this.buttonOK.Text = Strings.MenuCtxOk;
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += (s, e) => _restoreOnClose = false;

            // buttonCancel
            this.buttonCancel.DialogResult = DialogResult.Cancel;
            this.buttonCancel.Location = new Point(95, 75);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new Size(75, 23);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = Strings.MenuCtxCancel;
            this.buttonCancel.UseVisualStyleBackColor = true;

            // SizeInputForm
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new Size(184, 110);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.numericHeight);
            this.Controls.Add(this.labelHeight);
            this.Controls.Add(this.numericWidth);
            this.Controls.Add(this.labelWidth);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SizeInputForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = Strings.MenuCtxSetSize;
            this.TopMost = true;

            ((System.ComponentModel.ISupportInitialize)(this.numericWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericHeight)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void NumericValue_Changed(object sender, EventArgs e) {
            if (_targetForm != null) {
                _targetForm.Size = new Size((int)numericWidth.Value, (int)numericHeight.Value);
            }
        }
    }
}
