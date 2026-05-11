using System;
using System.Drawing;
using System.Windows.Forms;
using OnTopReplica.Properties;

namespace OnTopReplica.SidePanels {

    /// <summary>
    /// Side panel for managing image overlay presets:
    /// add the current overlay state as a preset, apply a saved preset,
    /// delete entries. Stored in <c>Settings.Default.ImagePresets</c>.
    /// </summary>
    class ImagePresetPanel : SidePanel {

        ListBox _list;
        Button _btnApply;
        Button _btnAddCurrent;
        Button _btnUpdate;
        Button _btnDelete;
        Button _btnLoadNew;
        Label _lblStatus;

        public ImagePresetPanel() {
            BuildUI();
        }

        public override string Title {
            get { return Strings.MenuImagePresets; }
        }

        void BuildUI() {
            this.Size = new Size(260, 320);

            _list = new ListBox {
                Location = new Point(8, 8),
                Size = new Size(244, 180),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                IntegralHeight = false
            };
            _list.DoubleClick += (s, e) => ApplySelected();

            _btnApply = new Button {
                Text = Strings.PresetApply,
                Location = new Point(8, 196),
                Size = new Size(120, 24),
                Anchor = AnchorStyles.Left | AnchorStyles.Bottom
            };
            _btnApply.Click += (s, e) => ApplySelected();

            _btnDelete = new Button {
                Text = Strings.PresetDelete,
                Location = new Point(132, 196),
                Size = new Size(120, 24),
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom
            };
            _btnDelete.Click += (s, e) => DeleteSelected();

            _btnAddCurrent = new Button {
                Text = Strings.PresetAddCurrent,
                Location = new Point(8, 224),
                Size = new Size(120, 24),
                Anchor = AnchorStyles.Left | AnchorStyles.Bottom
            };
            _btnAddCurrent.Click += (s, e) => AddCurrentAsPreset();

            _btnUpdate = new Button {
                Text = Strings.PresetUpdate,
                Location = new Point(132, 224),
                Size = new Size(120, 24),
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom
            };
            _btnUpdate.Click += (s, e) => UpdateSelectedFromCurrent();

            _btnLoadNew = new Button {
                Text = Strings.PresetLoadFile,
                Location = new Point(8, 252),
                Size = new Size(244, 24),
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            _btnLoadNew.Click += (s, e) => LoadNewFile();

            _lblStatus = new Label {
                Location = new Point(8, 286),
                Size = new Size(244, 24),
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                Text = ""
            };

            Controls.Add(_list);
            Controls.Add(_btnApply);
            Controls.Add(_btnDelete);
            Controls.Add(_btnAddCurrent);
            Controls.Add(_btnUpdate);
            Controls.Add(_btnLoadNew);
            Controls.Add(_lblStatus);
        }

        public override void OnFirstShown(MainForm form) {
            base.OnFirstShown(form);
            RefreshList();
        }

        void RefreshList() {
            _list.Items.Clear();
            var presets = Settings.Default.ImagePresets;
            if (presets == null) return;
            foreach (var p in presets) _list.Items.Add(p);
        }

        ImagePreset SelectedPreset {
            get { return _list.SelectedItem as ImagePreset; }
        }

        void ApplySelected() {
            var p = SelectedPreset;
            if (p == null) { _lblStatus.Text = Strings.PresetStatusSelectFirst; return; }
            ParentMainForm.ApplyImagePreset(p);
        }

        void DeleteSelected() {
            var p = SelectedPreset;
            if (p == null) return;
            var presets = Settings.Default.ImagePresets;
            presets.Remove(p);
            Settings.Default.Save();
            RefreshList();
        }

        void AddCurrentAsPreset() {
            var preset = ParentMainForm.CaptureCurrentAsImagePreset();
            if (preset == null) { _lblStatus.Text = Strings.PresetStatusNoImage; return; }

            string name = PromptForName(System.IO.Path.GetFileNameWithoutExtension(preset.Path));
            if (string.IsNullOrEmpty(name)) return;
            preset.Name = name;

            if (Settings.Default.ImagePresets == null)
                Settings.Default.ImagePresets = new ImagePresetArray();
            Settings.Default.ImagePresets.Add(preset);
            Settings.Default.Save();
            RefreshList();
        }

        void UpdateSelectedFromCurrent() {
            var existing = SelectedPreset;
            if (existing == null) { _lblStatus.Text = Strings.PresetStatusSelectFirst; return; }
            var fresh = ParentMainForm.CaptureCurrentAsImagePreset();
            if (fresh == null) { _lblStatus.Text = Strings.PresetStatusNoImage; return; }

            existing.Path = fresh.Path;
            existing.Location = fresh.Location;
            existing.Size = fresh.Size;
            existing.Scale = fresh.Scale;
            existing.Opacity = fresh.Opacity;
            Settings.Default.Save();
            RefreshList();
        }

        void LoadNewFile() {
            using (var dlg = new OpenFileDialog()) {
                dlg.Filter = "Images (*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff)|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff|All files (*.*)|*.*";
                dlg.CheckFileExists = true;
                if (dlg.ShowDialog(this) == DialogResult.OK) {
                    ParentMainForm.SetImage(dlg.FileName);
                }
            }
        }

        static string PromptForName(string suggested) {
            using (var f = new Form()) {
                f.Text = Strings.PresetPromptTitle;
                f.FormBorderStyle = FormBorderStyle.FixedDialog;
                f.StartPosition = FormStartPosition.CenterParent;
                f.ClientSize = new Size(280, 90);
                f.MaximizeBox = false; f.MinimizeBox = false; f.ShowInTaskbar = false;

                var txt = new TextBox { Location = new Point(12, 12), Size = new Size(256, 23), Text = suggested ?? "" };
                var ok = new Button { Text = Strings.MenuCtxOk, Location = new Point(112, 50), Size = new Size(75, 24), DialogResult = DialogResult.OK };
                var cancel = new Button { Text = Strings.MenuCtxCancel, Location = new Point(193, 50), Size = new Size(75, 24), DialogResult = DialogResult.Cancel };
                f.Controls.AddRange(new Control[] { txt, ok, cancel });
                f.AcceptButton = ok; f.CancelButton = cancel;

                return f.ShowDialog() == DialogResult.OK ? txt.Text.Trim() : null;
            }
        }
    }
}
