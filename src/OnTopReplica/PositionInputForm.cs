using System;
using System.Drawing;
using System.Windows.Forms;

namespace OnTopReplica {
    /// <summary>
    /// Simple form for entering window position coordinates.
    /// </summary>
    public class PositionInputForm : Form {
        private NumericUpDown numericX;
        private NumericUpDown numericY;
        private Button buttonOK;
        private Button buttonCancel;
        private Label labelX;
        private Label labelY;

        private readonly Form _targetForm;
        private Point _originalLocation;
        private bool _restoreOnClose = true;

        public Point Position {
            get {
                return new Point((int)numericX.Value, (int)numericY.Value);
            }
            set {
                numericX.Value = Clamp(value.X, numericX.Minimum, numericX.Maximum);
                numericY.Value = Clamp(value.Y, numericY.Minimum, numericY.Maximum);
            }
        }

        static decimal Clamp(int v, decimal min, decimal max) {
            if (v < min) return min;
            if (v > max) return max;
            return v;
        }

        public PositionInputForm(Form targetForm) {
            _targetForm = targetForm;
            InitializeComponent();

            if (_targetForm != null) {
                _originalLocation = _targetForm.Location;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e) {
            base.OnFormClosing(e);

            if (_restoreOnClose && _targetForm != null && DialogResult != DialogResult.OK) {
                _targetForm.Location = _originalLocation;
            }
        }

        private void InitializeComponent() {
            this.numericX = new NumericUpDown();
            this.numericY = new NumericUpDown();
            this.buttonOK = new Button();
            this.buttonCancel = new Button();
            this.labelX = new Label();
            this.labelY = new Label();

            ((System.ComponentModel.ISupportInitialize)(this.numericX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericY)).BeginInit();
            this.SuspendLayout();

            // labelX
            this.labelX.AutoSize = true;
            this.labelX.Location = new Point(12, 15);
            this.labelX.Name = "labelX";
            this.labelX.Size = new Size(20, 13);
            this.labelX.TabIndex = 0;
            this.labelX.Text = "X:";

            // numericX
            this.numericX.Location = new Point(50, 12);
            this.numericX.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            this.numericX.Minimum = new decimal(new int[] { 10000, 0, 0, -2147483648 });
            this.numericX.Name = "numericX";
            this.numericX.Size = new Size(120, 20);
            this.numericX.TabIndex = 1;
            this.numericX.ValueChanged += new EventHandler(NumericValue_Changed);

            // labelY
            this.labelY.AutoSize = true;
            this.labelY.Location = new Point(12, 45);
            this.labelY.Name = "labelY";
            this.labelY.Size = new Size(20, 13);
            this.labelY.TabIndex = 2;
            this.labelY.Text = "Y:";

            // numericY
            this.numericY.Location = new Point(50, 42);
            this.numericY.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            this.numericY.Minimum = new decimal(new int[] { 10000, 0, 0, -2147483648 });
            this.numericY.Name = "numericY";
            this.numericY.Size = new Size(120, 20);
            this.numericY.TabIndex = 3;
            this.numericY.ValueChanged += new EventHandler(NumericValue_Changed);

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

            // PositionInputForm
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new Size(184, 110);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.numericY);
            this.Controls.Add(this.labelY);
            this.Controls.Add(this.numericX);
            this.Controls.Add(this.labelX);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PositionInputForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = Strings.MenuCtxSetPosition;
            this.TopMost = true;

            ((System.ComponentModel.ISupportInitialize)(this.numericX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericY)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void NumericValue_Changed(object sender, EventArgs e) {
            if (_targetForm != null) {
                _targetForm.Location = new Point((int)numericX.Value, (int)numericY.Value);
            }
        }
    }
}
