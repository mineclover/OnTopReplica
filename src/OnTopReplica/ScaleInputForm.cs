using System;
using System.Reflection;
using System.Windows.Forms;

namespace OnTopReplica {
    /// <summary>
    /// Simple form for entering scale percentage.
    /// </summary>
    public class ScaleInputForm : Form {
        private NumericUpDown numericScale;
        private Button buttonOK;
        private Button buttonCancel;
        private Label labelScale;
        private Label labelPercent;

        private object _targetForm;

        public double ScalePercentage {
            get {
                return (double)numericScale.Value;
            }
            set {
                numericScale.Value = (decimal)value;
            }
        }

        public ScaleInputForm(object targetForm) {
            _targetForm = targetForm;
            InitializeComponent();
        }

        private void InitializeComponent() {
            this.numericScale = new NumericUpDown();
            this.buttonOK = new Button();
            this.buttonCancel = new Button();
            this.labelScale = new Label();
            this.labelPercent = new Label();

            ((System.ComponentModel.ISupportInitialize)(this.numericScale)).BeginInit();
            this.SuspendLayout();

            // labelScale
            this.labelScale.AutoSize = true;
            this.labelScale.Location = new System.Drawing.Point(12, 15);
            this.labelScale.Name = "labelScale";
            this.labelScale.Size = new System.Drawing.Size(41, 13);
            this.labelScale.TabIndex = 0;
            this.labelScale.Text = Strings.MenuCtxScale;

            // numericScale
            this.numericScale.Location = new System.Drawing.Point(70, 12);
            this.numericScale.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            this.numericScale.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            this.numericScale.Name = "numericScale";
            this.numericScale.Size = new System.Drawing.Size(80, 20);
            this.numericScale.TabIndex = 1;
            this.numericScale.DecimalPlaces = 2;
            this.numericScale.Increment = new decimal(new int[] { 1, 0, 0, 0 });
            this.numericScale.Value = new decimal(new int[] { 100, 0, 0, 0 });
            this.numericScale.ValueChanged += new EventHandler(NumericValue_Changed);

            // labelPercent
            this.labelPercent.AutoSize = true;
            this.labelPercent.Location = new System.Drawing.Point(156, 15);
            this.labelPercent.Name = "labelPercent";
            this.labelPercent.Size = new System.Drawing.Size(15, 13);
            this.labelPercent.TabIndex = 2;
            this.labelPercent.Text = "%";

            // buttonOK
            this.buttonOK.DialogResult = DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(14, 45);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 3;
            this.buttonOK.Text = Strings.MenuCtxOk;
            this.buttonOK.UseVisualStyleBackColor = true;

            // buttonCancel
            this.buttonCancel.DialogResult = DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(95, 45);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = Strings.MenuCtxCancel;
            this.buttonCancel.UseVisualStyleBackColor = true;

            // ScaleInputForm
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
            if (_targetForm != null) {
                try {
                    var type = _targetForm.GetType();
                    var thumbnailPanelProp = type.GetProperty("ThumbnailPanel");
                    if (thumbnailPanelProp != null) {
                        var thumbnailPanel = thumbnailPanelProp.GetValue(_targetForm, null);
                        var isShowingProp = thumbnailPanel.GetType().GetProperty("IsShowingThumbnail");
                        if (isShowingProp != null && (bool)isShowingProp.GetValue(thumbnailPanel, null)) {
                            double scale = (double)numericScale.Value / 100.0;
                            var method = type.GetMethod("FitToThumbnailScale");
                            if (method != null) {
                                method.Invoke(_targetForm, new object[] { scale });
                            }
                        }
                    }
                }
                catch {
                    // Silently ignore errors
                }
            }
        }
    }
}
