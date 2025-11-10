using System;
using System.Drawing;
using System.Windows.Forms;

namespace OnTopReplica {
    /// <summary>
    /// Dialog for entering custom screen position coordinates.
    /// </summary>
    public class CustomPositionDialog : Form {
        private NumericUpDown numericX;
        private NumericUpDown numericY;
        private Label labelX;
        private Label labelY;
        private Label labelPrompt;
        private Button buttonOK;
        private Button buttonCancel;
        private Button buttonCurrentPosition;

        /// <summary>
        /// Gets the custom position entered by the user.
        /// </summary>
        public Point CustomPosition {
            get {
                return new Point((int)numericX.Value, (int)numericY.Value);
            }
        }

        /// <summary>
        /// Gets or sets the initial position to display in the dialog.
        /// </summary>
        public Point InitialPosition {
            get {
                return new Point((int)numericX.Value, (int)numericY.Value);
            }
            set {
                numericX.Value = value.X;
                numericY.Value = value.Y;
            }
        }

        public CustomPositionDialog() {
            InitializeComponents();
        }

        private void InitializeComponents() {
            // Form settings
            this.Text = "Custom Position";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new Size(320, 180);
            this.AcceptButton = buttonOK;
            this.CancelButton = buttonCancel;

            // Label
            labelPrompt = new Label {
                Text = "Enter the screen coordinates (in pixels):",
                Location = new Point(20, 20),
                Size = new Size(280, 20),
                AutoSize = false
            };

            // X coordinate label
            labelX = new Label {
                Text = "X:",
                Location = new Point(20, 55),
                Size = new Size(80, 20),
                TextAlign = ContentAlignment.MiddleRight
            };

            // X coordinate input
            numericX = new NumericUpDown {
                Location = new Point(110, 52),
                Size = new Size(190, 25),
                Minimum = -10000,
                Maximum = 10000,
                Value = 0,
                DecimalPlaces = 0
            };

            // Y coordinate label
            labelY = new Label {
                Text = "Y:",
                Location = new Point(20, 90),
                Size = new Size(80, 20),
                TextAlign = ContentAlignment.MiddleRight
            };

            // Y coordinate input
            numericY = new NumericUpDown {
                Location = new Point(110, 87),
                Size = new Size(190, 25),
                Minimum = -10000,
                Maximum = 10000,
                Value = 0,
                DecimalPlaces = 0
            };

            // Current Position button
            buttonCurrentPosition = new Button {
                Text = "Use Current Position",
                Location = new Point(20, 122),
                Size = new Size(150, 25)
            };
            buttonCurrentPosition.Click += ButtonCurrentPosition_Click;

            // OK button
            buttonOK = new Button {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Location = new Point(140, 145),
                Size = new Size(80, 25)
            };

            // Cancel button
            buttonCancel = new Button {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Location = new Point(230, 145),
                Size = new Size(80, 25)
            };

            // Add controls
            this.Controls.Add(labelPrompt);
            this.Controls.Add(labelX);
            this.Controls.Add(numericX);
            this.Controls.Add(labelY);
            this.Controls.Add(numericY);
            this.Controls.Add(buttonCurrentPosition);
            this.Controls.Add(buttonOK);
            this.Controls.Add(buttonCancel);
        }

        private void ButtonCurrentPosition_Click(object sender, EventArgs e) {
            // Get the parent form's current position
            if (this.Owner is MainForm mainForm) {
                numericX.Value = mainForm.Location.X;
                numericY.Value = mainForm.Location.Y;
            }
        }
    }
}
