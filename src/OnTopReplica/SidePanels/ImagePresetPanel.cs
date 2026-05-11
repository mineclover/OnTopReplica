using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using OnTopReplica.Native;
using OnTopReplica.Properties;

namespace OnTopReplica.SidePanels {

    /// <summary>
    /// Multi-image layer manager. Each preset is a "layer" the user can apply
    /// instantly via the panel or its assigned global hotkey.
    /// </summary>
    class ImagePresetPanel : SidePanel {

        ListView _list;
        Button _btnApply;
        Button _btnAddCurrent;
        Button _btnUpdate;
        Button _btnDelete;
        Button _btnLoadNew;
        Button _btnUp;
        Button _btnDown;
        HotKeyTextBox _hotkeyBox;
        Button _btnAssignHotkey;
        Button _btnClearHotkey;
        Label _lblStatus;

        public ImagePresetPanel() {
            BuildUI();
        }

        public override string Title {
            get { return Strings.MenuImagePresets; }
        }

        void BuildUI() {
            this.Size = new Size(360, 420);

            _list = new ListView {
                Location = new Point(8, 8),
                Size = new Size(344, 220),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                View = View.Details,
                FullRowSelect = true,
                MultiSelect = false,
                HideSelection = false,
                GridLines = false
            };
            _list.Columns.Add("#", 32);
            _list.Columns.Add(Strings.PresetColumnName, 110);
            _list.Columns.Add(Strings.PresetColumnHotkey, 110);
            _list.Columns.Add(Strings.PresetColumnSize, 80);
            _list.DoubleClick += (s, e) => ApplySelected();
            _list.SelectedIndexChanged += (s, e) => OnSelectionChanged();

            int rowTop1 = 234;
            _btnApply = MakeButton(Strings.PresetApply, 8, rowTop1, 80, () => ApplySelected());
            _btnAddCurrent = MakeButton(Strings.PresetAddCurrent, 92, rowTop1, 80, () => AddCurrentAsPreset());
            _btnUpdate = MakeButton(Strings.PresetUpdate, 176, rowTop1, 88, () => UpdateSelectedFromCurrent());
            _btnDelete = MakeButton(Strings.PresetDelete, 268, rowTop1, 84, () => DeleteSelected());

            int rowTop2 = 262;
            _btnUp = MakeButton("▲", 8, rowTop2, 36, () => MoveSelected(-1));
            _btnDown = MakeButton("▼", 48, rowTop2, 36, () => MoveSelected(+1));
            _btnLoadNew = MakeButton(Strings.PresetLoadFile, 92, rowTop2, 260, () => LoadNewFile());

            // Hotkey assignment row
            int rowTop3 = 300;
            var lblHk = new Label {
                Text = Strings.PresetHotkeyLabel,
                Location = new Point(8, rowTop3 + 4),
                AutoSize = true
            };
            _hotkeyBox = new HotKeyTextBox {
                Location = new Point(80, rowTop3),
                Size = new Size(160, 23),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            _btnAssignHotkey = MakeButton(Strings.PresetAssignHotkey, 244, rowTop3, 60, () => AssignHotkeyToSelected());
            _btnClearHotkey = MakeButton(Strings.PresetClearHotkey, 308, rowTop3, 44, () => ClearHotkeyOfSelected());

            _lblStatus = new Label {
                Location = new Point(8, 332),
                Size = new Size(344, 36),
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                Text = "",
                ForeColor = Color.DarkSlateGray
            };

            Controls.Add(_list);
            Controls.Add(_btnApply);
            Controls.Add(_btnAddCurrent);
            Controls.Add(_btnUpdate);
            Controls.Add(_btnDelete);
            Controls.Add(_btnUp);
            Controls.Add(_btnDown);
            Controls.Add(_btnLoadNew);
            Controls.Add(lblHk);
            Controls.Add(_hotkeyBox);
            Controls.Add(_btnAssignHotkey);
            Controls.Add(_btnClearHotkey);
            Controls.Add(_lblStatus);
        }

        static Button MakeButton(string text, int x, int y, int width, Action onClick) {
            var b = new Button {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, 24),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            b.Click += (s, e) => onClick();
            return b;
        }

        public override void OnFirstShown(MainForm form) {
            base.OnFirstShown(form);
            RefreshList();
        }

        void RefreshList() {
            _list.BeginUpdate();
            _list.Items.Clear();
            var presets = Settings.Default.ImagePresets;
            if (presets != null) {
                for (int i = 0; i < presets.Count; i++) {
                    var p = presets[i];
                    var row = new ListViewItem(new[] {
                        (i + 1).ToString(),
                        p.Name ?? "",
                        p.Hotkey ?? "",
                        p.Size.Width + "×" + p.Size.Height
                    }) { Tag = p };
                    _list.Items.Add(row);
                }
            }
            _list.EndUpdate();
            OnSelectionChanged();
        }

        void OnSelectionChanged() {
            var p = SelectedPreset;
            _hotkeyBox.Text = p != null ? (p.Hotkey ?? "") : "";
        }

        ImagePreset SelectedPreset {
            get { return _list.SelectedItems.Count > 0 ? _list.SelectedItems[0].Tag as ImagePreset : null; }
        }

        int SelectedIndex {
            get { return _list.SelectedItems.Count > 0 ? _list.SelectedItems[0].Index : -1; }
        }

        void SetStatus(string text) { _lblStatus.Text = text ?? ""; }

        // ---- Actions ---------------------------------------------------------

        void ApplySelected() {
            var p = SelectedPreset;
            if (p == null) { SetStatus(Strings.PresetStatusSelectFirst); return; }
            ParentMainForm.ApplyImagePreset(p);
        }

        void DeleteSelected() {
            var p = SelectedPreset;
            if (p == null) return;
            Settings.Default.ImagePresets.Remove(p);
            Settings.Default.Save();
            ParentMainForm.RefreshHotkeys();
            RefreshList();
        }

        void AddCurrentAsPreset() {
            var preset = ParentMainForm.CaptureCurrentAsImagePreset();
            if (preset == null) { SetStatus(Strings.PresetStatusNoImage); return; }

            string name = PromptForName(Path.GetFileNameWithoutExtension(preset.Path));
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
            if (existing == null) { SetStatus(Strings.PresetStatusSelectFirst); return; }
            var fresh = ParentMainForm.CaptureCurrentAsImagePreset();
            if (fresh == null) { SetStatus(Strings.PresetStatusNoImage); return; }

            existing.Path = fresh.Path;
            existing.Location = fresh.Location;
            existing.Size = fresh.Size;
            existing.Scale = fresh.Scale;
            existing.Opacity = fresh.Opacity;
            // Hotkey preserved.
            Settings.Default.Save();
            RefreshList();
        }

        void MoveSelected(int delta) {
            int idx = SelectedIndex;
            if (idx < 0) return;
            var presets = Settings.Default.ImagePresets;
            int newIdx = idx + delta;
            if (newIdx < 0 || newIdx >= presets.Count) return;
            var p = presets[idx];
            presets.RemoveAt(idx);
            presets.Insert(newIdx, p);
            Settings.Default.Save();
            RefreshList();
            if (newIdx < _list.Items.Count) _list.Items[newIdx].Selected = true;
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

        // ---- Hotkey ----------------------------------------------------------

        void AssignHotkeyToSelected() {
            var p = SelectedPreset;
            if (p == null) { SetStatus(Strings.PresetStatusSelectFirst); return; }
            string spec = (_hotkeyBox.Text ?? "").Trim();
            if (string.IsNullOrEmpty(spec)) { SetStatus(Strings.PresetHotkeyEmpty); return; }

            //Validate spec
            try {
                int m, k;
                HotKeyMethods.TranslateStringToKeyValues(spec, out m, out k);
            }
            catch (Exception) {
                SetStatus(Strings.PresetHotkeyInvalid);
                return;
            }

            //Clear duplicates on other presets so OS registration won't silently drop later entries
            foreach (var other in Settings.Default.ImagePresets) {
                if (other != p && string.Equals(other.Hotkey, spec, StringComparison.OrdinalIgnoreCase)) {
                    other.Hotkey = "";
                }
            }

            p.Hotkey = spec;
            Settings.Default.Save();
            ParentMainForm.RefreshHotkeys();
            RefreshList();
            SetStatus(string.Format(Strings.PresetHotkeyAssigned, spec, p.Name));
        }

        void ClearHotkeyOfSelected() {
            var p = SelectedPreset;
            if (p == null) return;
            p.Hotkey = "";
            _hotkeyBox.Text = "";
            Settings.Default.Save();
            ParentMainForm.RefreshHotkeys();
            RefreshList();
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
