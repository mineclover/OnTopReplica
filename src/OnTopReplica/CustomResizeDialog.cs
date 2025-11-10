using System;
using System.Drawing;
using System.Windows.Forms;

namespace OnTopReplica {
    /// <summary>
    /// Dialog for entering a custom resize ratio.
    /// </summary>
    public class CustomResizeDialog : Form {
        private NumericUpDown numericRatio;
        private Label labelPrompt;
        private Button buttonOK;
        private Button buttonCancel;
        private RadioButton radioPercentage;
        private RadioButton radioDecimal;

        /// <summary>
        /// Gets the resize ratio entered by the user (as a decimal, e.g., 0.5 for 50%, 1.0 for 100%, 2.0 for 200%).
        /// </summary>
        public double ResizeRatio {
            get {
                if (radioPercentage.Checked) {
                    return (double)numericRatio.Value / 100.0;
                }
                else {
                    return (double)numericRatio.Value;
                }
            }
        }

        public CustomResizeDialog() {
            InitializeComponents();
        }

        private void InitializeComponents() {
            // Form settings
            this.Text = "Custom Resize Ratio";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new Size(320, 150);
            this.AcceptButton = buttonOK;
            this.CancelButton = buttonCancel;

            // Label
            labelPrompt = new Label {
                Text = "Enter the resize ratio:",
                Location = new Point(20, 20),
                Size = new Size(280, 20),
                AutoSize = false
            };

            // Radio buttons for input mode
            radioPercentage = new RadioButton {
                Text = "Percentage (%)",
                Location = new Point(20, 50),
                Size = new Size(120, 20),
                Checked = true
            };
            radioPercentage.CheckedChanged += RadioButton_CheckedChanged;

            radioDecimal = new RadioButton {
                Text = "Decimal",
                Location = new Point(150, 50),
                Size = new Size(100, 20),
                Checked = false
            };
            radioDecimal.CheckedChanged += RadioButton_CheckedChanged;

            // Numeric input
            numericRatio = new NumericUpDown {
                Location = new Point(20, 80),
                Size = new Size(280, 25),
                Minimum = 1,
                Maximum = 1000,
                Value = 100,
                DecimalPlaces = 2,
                Increment = 1
            };

            // OK button
            buttonOK = new Button {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Location = new Point(140, 115),
                Size = new Size(80, 25)
            };

            // Cancel button
            buttonCancel = new Button {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Location = new Point(230, 115),
                Size = new Size(80, 25)
            };

            // Add controls
            this.Controls.Add(labelPrompt);
            this.Controls.Add(radioPercentage);
            this.Controls.Add(radioDecimal);
            this.Controls.Add(numericRatio);
            this.Controls.Add(buttonOK);
            this.Controls.Add(buttonCancel);
        }

        private void RadioButton_CheckedChanged(object sender, EventArgs e) {
            if (radioPercentage.Checked) {
                // Switch to percentage mode
                decimal currentValue = numericRatio.Value;
                if (currentValue <= 10) {
                    // Likely was in decimal mode, convert to percentage
                    numericRatio.Value = currentValue * 100;
                }
                numericRatio.DecimalPlaces = 0;
                numericRatio.Increment = 5;
                numericRatio.Minimum = 1;
                numericRatio.Maximum = 1000;
            }
            else {
                // Switch to decimal mode
                decimal currentValue = numericRatio.Value;
                if (currentValue > 10) {
                    // Likely was in percentage mode, convert to decimal
                    numericRatio.Value = currentValue / 100;
                }
                numericRatio.DecimalPlaces = 2;
                numericRatio.Increment = 0.1m;
                numericRatio.Minimum = 0.01m;
                numericRatio.Maximum = 10m;
            }
        }
    }
}
