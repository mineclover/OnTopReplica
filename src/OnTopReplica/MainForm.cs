using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using OnTopReplica.Native;
using OnTopReplica.Properties;
using OnTopReplica.StartupOptions;
using OnTopReplica.Update;
using OnTopReplica.WindowSeekers;
using WindowsFormsAero.Dwm;
using WindowsFormsAero.TaskDialog;

namespace OnTopReplica {

    partial class MainForm : AspectRatioForm {

        //GUI elements
        ThumbnailPanel _thumbnailPanel;
        ImagePanel _imagePanel;

        /// <summary>Source the overlay window is currently showing.</summary>
        public enum SourceMode { None, Thumbnail, Image }
        SourceMode _sourceMode = SourceMode.None;

        public SourceMode CurrentSourceMode {
            get { return _sourceMode; }
        }

        //Managers
        readonly MessagePumpManager _msgPumpManager = new MessagePumpManager();
        WindowListMenuManager _windowListManager;
        public FullscreenFormManager FullscreenManager { get; private set; }

        Options _startupOptions;

        public MainForm(Options startupOptions) {
            _startupOptions = startupOptions;

            FullscreenManager = new FullscreenFormManager(this);
            _quickRegionDrawingHandler = new ThumbnailPanel.RegionDrawnHandler(HandleQuickRegionDrawn);
            
            //WinForms init pass
            InitializeComponent();

            //Store default values
            DefaultNonClickTransparencyKey = this.TransparencyKey;
            DefaultBorderStyle = this.FormBorderStyle;

            //Thumbnail panel
            _thumbnailPanel = new ThumbnailPanel {
                Location = Point.Empty,
                Dock = DockStyle.Fill
            };
            _thumbnailPanel.CloneClick += new EventHandler<CloneClickEventArgs>(Thumbnail_CloneClick);
            Controls.Add(_thumbnailPanel);

            //Image panel (image-overlay mode, initially hidden)
            _imagePanel = new ImagePanel {
                Location = Point.Empty,
                Dock = DockStyle.Fill,
                Visible = false
            };
            Controls.Add(_imagePanel);

            //Drag-drop image files anywhere onto the form
            this.AllowDrop = true;
            this.DragEnter += MainForm_DragEnter;
            this.DragDrop += MainForm_DragDrop;

            //Resize submenu: "Fit to image size (1:1)" and "Fit to monitor" + Placement mode
            var miFitImage = new ToolStripMenuItem(Strings.MenuFitImageSize);
            miFitImage.Click += (s, ev) => FitToImageScale(1.0);

            var miFitMonitor = new ToolStripMenuItem(Strings.MenuFitMonitor);
            miFitMonitor.Click += (s, ev) => FullscreenManager.ToggleCoverMonitor();

            var miPlacement = new ToolStripMenuItem(Strings.MenuPlacementMode);
            miPlacement.Click += (s, ev) => TogglePlacementMode();
            miPlacement.CheckOnClick = false;

            //Insert right after the Fullscreen item so they stay grouped at the bottom of the fit options
            int idx = menuResize.Items.IndexOf(fullscreenToolStripMenuItem1);
            menuResize.Items.Insert(idx + 1, miFitImage);
            menuResize.Items.Insert(idx + 2, miFitMonitor);
            menuResize.Items.Insert(idx + 3, miPlacement);

            //Reflect placement-mode checked state and enable image-only items
            menuResize.Opening += (s, ev) => {
                bool imageMode = (CurrentSourceMode == SourceMode.Image);
                miFitImage.Enabled = imageMode;
                miPlacement.Enabled = imageMode;
                miPlacement.Checked = _placementMode;
            };

            //Image-mode context menu entries (inserted at top of menu)
            var miLoadImage = new ToolStripMenuItem(Strings.MenuLoadImage);
            miLoadImage.Click += (s, ev) => PromptLoadImage();
            var miPresets = new ToolStripMenuItem(Strings.MenuImagePresets);
            miPresets.Click += (s, ev) => ToggleImagePresetPanel();
            var miUnloadImage = new ToolStripMenuItem(Strings.MenuUnloadImage);
            miUnloadImage.Click += (s, ev) => UnsetImage();
            menuContext.Items.Insert(0, miLoadImage);
            menuContext.Items.Insert(1, miPresets);
            menuContext.Items.Insert(2, miUnloadImage);
            menuContext.Items.Insert(3, new ToolStripSeparator());
            menuContext.Opening += (s, ev) => {
                miUnloadImage.Visible = (_sourceMode == SourceMode.Image);
            };

            //Set native renderer on context menus
            Asztal.Szótár.NativeToolStripRenderer.SetToolStripRenderer(
                menuContext, menuWindows, menuOpacity, menuResize, menuFullscreenContext
            );

            //Set to Key event preview
            this.KeyPreview = true;

            Log.Write("Main form constructed");
        }

        #region Event override

        protected override void OnHandleCreated(EventArgs e){
 	        base.OnHandleCreated(e);

            //Window init
            KeepAspectRatio = false;
            GlassMargins = new Padding(-1);

            //Managers
            _msgPumpManager.Initialize(this);
            _windowListManager = new WindowListMenuManager(this, menuWindows);
            _windowListManager.ParentMenus = new System.Windows.Forms.ContextMenuStrip[] {
                menuContext, menuFullscreenContext
            };

            //Platform specific form initialization
            Program.Platform.PostHandleFormInit(this);
        }

        protected override void OnShown(EventArgs e) {
            Log.Write("Main form shown");
            base.OnShown(e);

            //Apply startup options
            _startupOptions.Apply(this);
        }

        protected override void OnClosing(CancelEventArgs e) {
            Log.Write("Main form closing");
            base.OnClosing(e);

            _msgPumpManager.Dispose();
            Program.Platform.CloseForm(this);
        }

        protected override void OnClosed(EventArgs e) {
            Log.Write("Main form closed");
            base.OnClosed(e);
        }

        protected override void OnMove(EventArgs e) {
            base.OnMove(e);

            AdjustSidePanelLocation();
        }

        protected override void OnResizeEnd(EventArgs e) {
            base.OnResizeEnd(e);

            RefreshScreenLock();
        }

        protected override void OnResizing(EventArgs e) {
            //Update aspect ratio from thumbnail while resizing (but do not refresh, resizing does that anyway)
            if (_thumbnailPanel.IsShowingThumbnail) {
                SetAspectRatio(_thumbnailPanel.ThumbnailPixelSize, false);
            }
        }

        protected override void OnActivated(EventArgs e) {
            base.OnActivated(e);

            //Deactivate click-through if form is reactivated
            if (ClickThroughEnabled) {
                ClickThroughEnabled = false;
            }

            Program.Platform.RestoreForm(this);
        }

        protected override void OnDeactivate(EventArgs e) {
            base.OnDeactivate(e);

            //HACK: sometimes, even if TopMost is true, the window loses its "always on top" status.
            //  This is a fix attempt that probably won't work...
            if (!FullscreenManager.IsFullscreen) { //fullscreen mode doesn't use TopMost
                TopMost = false;
                TopMost = true;
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e) {
            base.OnMouseWheel(e);

            if (!FullscreenManager.IsFullscreen) {
                if (_thumbnailPanel.IsShowingThumbnail) {
                    SetAspectRatio(_thumbnailPanel.ThumbnailPixelSize, false);
                }

                int change = (int)(e.Delta / 6.0); //assumes a mouse wheel "tick" is in the 80-120 range
                AdjustSize(change);

                RefreshScreenLock();
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e) {
            base.OnMouseDoubleClick(e);

            //This is handled by the WM_NCLBUTTONDBLCLK msg handler usually (because the GlassForm translates
            //clicks on client to clicks on caption). But if fullscreen mode disables GlassForm dragging, we need
            //this auxiliary handler to switch mode.
            FullscreenManager.Toggle();
        }

        protected override void OnMouseClick(MouseEventArgs e) {
            base.OnMouseClick(e);

            //Same story as above (OnMouseDoubleClick)
            if (e.Button == System.Windows.Forms.MouseButtons.Right) {
                OpenContextMenu(null);
            }
        }

        private ThumbnailPanel.RegionDrawnHandler _quickRegionDrawingHandler;

        protected override void WndProc(ref Message m) {
            if (_msgPumpManager != null) {
                if (_msgPumpManager.PumpMessage(ref m)) {
                    return;
                }
            }

            switch (m.Msg) {
                case WM.NCRBUTTONUP:
                    //Open context menu if right button clicked on caption (i.e. all of the window area because of glass)
                    if (m.WParam.ToInt32() == HT.CAPTION) {
                        OpenContextMenu(null);

                        m.Result = IntPtr.Zero;
                        return;
                    }
                    break;

                case WM.NCLBUTTONDOWN:
                    if ((ModifierKeys & Keys.Control) == Keys.Control &&
                        ThumbnailPanel.IsShowingThumbnail &&
                        !ThumbnailPanel.DrawMouseRegions) {

                        ThumbnailPanel.EnableMouseRegionsDrawingWithMouseDown();
                        ThumbnailPanel.RegionDrawn += _quickRegionDrawingHandler;

                        m.Result = IntPtr.Zero;
                        return;
                    }
                    break;

                case WM.NCLBUTTONDBLCLK:
                    //Toggle fullscreen mode if double click on caption (whole glass area)
                    if (m.WParam.ToInt32() == HT.CAPTION) {
                        FullscreenManager.Toggle();

                        m.Result = IntPtr.Zero;
                        return;
                    }
                    break;

                case WM.NCHITTEST:
                    //Make transparent to hit-testing if in click through mode
                    if (ClickThroughEnabled) {
                        m.Result = (IntPtr)HT.TRANSPARENT;

                        RefreshClickThroughComeBack();
                        return;
                    }
                    break;
            }

            base.WndProc(ref m);
        }

        private void HandleQuickRegionDrawn(object sender, ThumbnailRegion region) {
            //Reset region drawing state
            ThumbnailPanel.DrawMouseRegions = false;
            ThumbnailPanel.RegionDrawn -= _quickRegionDrawingHandler;

            SelectedThumbnailRegion = region;
        }

        #endregion

        #region Keyboard event handling

        protected override void OnKeyUp(KeyEventArgs e) {
            base.OnKeyUp(e);

            //ALT
            if (e.Modifiers == Keys.Alt) {
                if (e.KeyCode == Keys.Enter) {
                    e.Handled = true;
                    FullscreenManager.Toggle();
                }

                else if (e.KeyCode == Keys.D1 || e.KeyCode == Keys.NumPad1) {
                    FitToThumbnail(0.25);
                }

                else if (e.KeyCode == Keys.D2 || e.KeyCode == Keys.NumPad2) {
                    FitToThumbnail(0.5);
                }

                else if (e.KeyCode == Keys.D3 || e.KeyCode == Keys.NumPad3 ||
                         e.KeyCode == Keys.D0 || e.KeyCode == Keys.NumPad0) {
                    FitToThumbnail(1.0);
                }

                else if (e.KeyCode == Keys.D4 || e.KeyCode == Keys.NumPad4) {
                    FitToThumbnail(2.0);
                }
            }

            //F11 Fullscreen switch
            else if (e.KeyCode == Keys.F11) {
                e.Handled = true;
                FullscreenManager.Toggle();
            }

            //ESCAPE
            else if (e.KeyCode == Keys.Escape) {
                //Exit placement mode first (most intrusive UI state)
                if (_placementMode) {
                    ExitPlacementMode();
                }
                //Disable click-through
                else if (ClickThroughEnabled) {
                    ClickThroughEnabled = false;
                }
                //Toggle fullscreen
                else if (FullscreenManager.IsFullscreen) {
                    FullscreenManager.SwitchBack();
                }
                //Disable click forwarding
                else if (ClickForwardingEnabled) {
                    ClickForwardingEnabled = false;
                }
            }
        }

        #endregion

        #region Thumbnail operation

        /// <summary>
        /// Sets a new thumbnail.
        /// </summary>
        /// <param name="handle">Handle to the window to clone.</param>
        /// <param name="region">Region of the window to clone or null.</param>
        public void SetThumbnail(WindowHandle handle, ThumbnailRegion region) {
            try {
                Log.Write("Cloning window HWND {0} of class {1}", handle.Handle, handle.Class);

                CurrentThumbnailWindowHandle = handle;
                _thumbnailPanel.SetThumbnailHandle(handle, region);

                //Set aspect ratio (this will resize the form), do not refresh if in fullscreen
                SetAspectRatio(_thumbnailPanel.ThumbnailPixelSize, !FullscreenManager.IsFullscreen);
            }
            catch (Exception ex) {
                Log.WriteException("Unable to set new thumbnail", ex);

                ThumbnailError(ex, false, Strings.ErrorUnableToCreateThumbnail);
                _thumbnailPanel.UnsetThumbnail();
            }
        }

        /// <summary>
        /// Enables group mode on a list of window handles.
        /// </summary>
        /// <param name="handles">List of window handles.</param>
        public void SetThumbnailGroup(IList<WindowHandle> handles) {
            if (handles.Count == 0)
                return;

            //At last one thumbnail
            SetThumbnail(handles[0], null);

            //Handle if no real group
            if (handles.Count == 1)
                return;

            CurrentThumbnailWindowHandle = null;
            _msgPumpManager.Get<MessagePumpProcessors.GroupSwitchManager>().EnableGroupMode(handles);
        }

        /// <summary>
        /// Disables the cloned thumbnail.
        /// </summary>
        public void UnsetThumbnail() {
            //Unset handle
            CurrentThumbnailWindowHandle = null;
            _thumbnailPanel.UnsetThumbnail();

            //Disable aspect ratio
            KeepAspectRatio = false;
        }

        /// <summary>
        /// Gets or sets the region displayed of the current thumbnail.
        /// </summary>
        public ThumbnailRegion SelectedThumbnailRegion {
            get {
                if (!_thumbnailPanel.IsShowingThumbnail || !_thumbnailPanel.ConstrainToRegion)
                    return null;

                return _thumbnailPanel.SelectedRegion;
            }
            set {
                if (!_thumbnailPanel.IsShowingThumbnail)
                    return;

                _thumbnailPanel.SelectedRegion = value;

                SetAspectRatio(_thumbnailPanel.ThumbnailPixelSize, true);

                FixPositionAndSize();
            }
        }

        const int FixMargin = 10;

        /// <summary>
        /// Fixes the form's position and size, ensuring it is fully displayed in the current screen.
        /// </summary>
        private void FixPositionAndSize() {
            var screen = Screen.FromControl(this);

            if (Width > screen.WorkingArea.Width) {
                Width = screen.WorkingArea.Width - FixMargin;
            }
            if (Height > screen.WorkingArea.Height) {
                Height = screen.WorkingArea.Height - FixMargin;
            }
            if (Location.X + Width > screen.WorkingArea.Right) {
                Location = new Point(screen.WorkingArea.Right - Width - FixMargin, Location.Y);
            }
            if (Location.Y + Height > screen.WorkingArea.Bottom) {
                Location = new Point(Location.X, screen.WorkingArea.Bottom - Height - FixMargin);
            }
        }

        private void ThumbnailError(Exception ex, bool suppress, string title) {
            if (!suppress) {
                ShowErrorDialog(title, Strings.ErrorGenericThumbnailHandleError, ex.Message);
            }

            UnsetThumbnail();
        }

        /// <summary>Automatically sizes the window to fit the current source at scale p.</summary>
        /// <param name="p">Scale of the source (1.0 = original pixel size).</param>
        private void FitToThumbnail(double p) {
            try {
                if (_sourceMode == SourceMode.Image) {
                    FitToImageScale(p);
                    return;
                }

                Size originalSize = _thumbnailPanel.ThumbnailPixelSize;
                Size fittedSize = new Size((int)(originalSize.Width * p), (int)(originalSize.Height * p));
                ClientSize = fittedSize;
                RefreshScreenLock();
            }
            catch (Exception ex) {
                ThumbnailError(ex, false, Strings.ErrorUnableToFit);
            }
        }

        /// <summary>
        /// Fits to the current source (thumbnail or image) at the given scale.
        /// Named "Thumbnail" for backwards compatibility with ScaleInputForm.
        /// </summary>
        public void FitToThumbnailScale(double scale) {
            if (_sourceMode == SourceMode.Image) {
                FitToImageScale(scale);
            }
            else {
                FitToThumbnail(scale);
            }
        }

        #endregion

        #region Image source mode

        /// <summary>
        /// Loads an image file and switches the overlay into image mode at 1:1 pixel size.
        /// Disposes any prior image, unsets any DWM thumbnail, exits fullscreen,
        /// and temporarily clears resize lock so the form can resize to native pixels.
        /// </summary>
        public void SetImage(string path) {
            try {
                //Exit fullscreen so the form can be sized freely
                if (FullscreenManager.IsFullscreen) FullscreenManager.SwitchBack();

                //Release resize lock so we can fit the image's native size
                bool wasLocked = ResizeLockEnabled;
                if (wasLocked) ResizeLockEnabled = false;

                //Tear down thumbnail mode if active
                if (_sourceMode == SourceMode.Thumbnail) {
                    UnsetThumbnail();
                }

                _imagePanel.SetImage(path);
                _sourceMode = SourceMode.Image;

                //Show image panel, hide thumbnail panel
                _thumbnailPanel.Visible = false;
                _imagePanel.Visible = true;
                _imagePanel.BringToFront();

                //Default to 1:1 pixel size; enforce aspect so user resizes proportionally
                Size px = _imagePanel.ImagePixelSize;
                if (px.Width > 0 && px.Height > 0) {
                    KeepAspectRatio = false;
                    SetAspectRatio(px, true);
                    ClientSize = px;
                    KeepAspectRatio = true;
                }

                //Restore resize lock if it was on (re-pinned to new size)
                if (wasLocked) ResizeLockEnabled = true;

                RefreshScreenLock();
                Log.Write("Loaded image: {0} ({1}x{2})", path, px.Width, px.Height);
            }
            catch (Exception ex) {
                Log.WriteException("Unable to load image", ex);
                ShowErrorDialog(Strings.ErrorUnableToCreateThumbnail, ex.Message, ex.GetType().Name);
                UnsetImage();
            }
        }

        /// <summary>
        /// Unloads the current image and returns to "no source" state.
        /// </summary>
        public void UnsetImage() {
            _imagePanel.UnsetImage();
            _imagePanel.Visible = false;
            _thumbnailPanel.Visible = true;
            KeepAspectRatio = false;
            _sourceMode = SourceMode.None;
        }

        /// <summary>
        /// Fits the form to the loaded image at the given scale (1.0 = 1:1).
        /// </summary>
        public void FitToImageScale(double scale) {
            if (_sourceMode != SourceMode.Image || !_imagePanel.IsShowingImage) return;
            Size px = _imagePanel.ImagePixelSize;
            KeepAspectRatio = false;
            ClientSize = new Size((int)(px.Width * scale), (int)(px.Height * scale));
            KeepAspectRatio = true;
            RefreshScreenLock();
        }

        public ImagePanel ImagePanel {
            get { return _imagePanel; }
        }

        #region Placement mode
        //
        // Placement mode: the form expands to cover the current monitor and uses
        // TransparencyKey to make the background click-through. The ImagePanel
        // becomes a child positioned inside the form, draggable with the mouse.
        // Exiting placement collapses the form back around the image's final spot.

        bool _placementMode = false;
        Point _prePlacementLocation;
        Size _prePlacementClientSize;
        FormBorderStyle _prePlacementBorderStyle;
        Color _prePlacementBackColor;
        Color _prePlacementTransparencyKey;
        bool _prePlacementTopMost;

        static readonly Color PlacementChroma = Color.FromArgb(255, 0, 254); // magenta-ish, unlikely in real images

        public bool PlacementMode { get { return _placementMode; } }

        public void TogglePlacementMode() {
            if (_placementMode) ExitPlacementMode();
            else EnterPlacementMode();
        }

        void EnterPlacementMode() {
            if (_sourceMode != SourceMode.Image || _imagePanel == null || !_imagePanel.IsShowingImage) return;
            if (_placementMode) return;

            //Save state
            _prePlacementLocation = this.Location;
            _prePlacementClientSize = this.ClientSize;
            _prePlacementBorderStyle = this.FormBorderStyle;
            _prePlacementBackColor = this.BackColor;
            _prePlacementTransparencyKey = this.TransparencyKey;
            _prePlacementTopMost = this.TopMost;

            //Close any side panel; release aspect lock and resize lock for free placement
            if (IsSidePanelOpen) CloseSidePanel();
            bool wasLocked = ResizeLockEnabled;
            if (wasLocked) ResizeLockEnabled = false;
            KeepAspectRatio = false;

            //Resize form to current monitor's full bounds
            var screen = Screen.FromControl(this);
            this.FormBorderStyle = FormBorderStyle.None;
            this.Bounds = screen.Bounds;

            //Make background click-through transparent
            this.BackColor = PlacementChroma;
            this.TransparencyKey = PlacementChroma;
            this.TopMost = true;

            //Convert image panel from Dock=Fill to a positioned child
            _imagePanel.Dock = DockStyle.None;
            _imagePanel.Size = _prePlacementClientSize;
            _imagePanel.Location = new Point(
                _prePlacementLocation.X - screen.Bounds.X,
                _prePlacementLocation.Y - screen.Bounds.Y);
            _imagePanel.PlacementModeActive = true;

            _placementMode = true;
            Log.Write("Entered placement mode on monitor {0}", screen.DeviceName);
        }

        void ExitPlacementMode() {
            if (!_placementMode) return;

            var screen = Screen.FromControl(this);
            //Final on-screen position of the image
            Point imageScreenPos = new Point(
                screen.Bounds.X + _imagePanel.Location.X,
                screen.Bounds.Y + _imagePanel.Location.Y);
            Size imageSize = _imagePanel.Size;

            //Restore image panel to Fill
            _imagePanel.PlacementModeActive = false;
            _imagePanel.Dock = DockStyle.Fill;
            _imagePanel.Location = Point.Empty;

            //Restore form colors first (avoid flicker when resizing)
            this.TransparencyKey = _prePlacementTransparencyKey;
            this.BackColor = _prePlacementBackColor;
            this.FormBorderStyle = _prePlacementBorderStyle;
            this.TopMost = _prePlacementTopMost;

            //Apply the new bounds derived from the image's final position
            KeepAspectRatio = false;
            this.Location = imageScreenPos;
            this.ClientSize = imageSize;
            KeepAspectRatio = true;

            _placementMode = false;
            Log.Write("Exited placement mode at {0} size {1}", imageScreenPos, imageSize);
        }

        #endregion

        /// <summary>
        /// Applies a saved preset: loads the image, places the window, sets opacity.
        /// </summary>
        public void ApplyImagePreset(ImagePreset preset) {
            if (preset == null) return;
            if (string.IsNullOrEmpty(preset.Path)) return;

            try {
                //SetImage already handles fullscreen exit + resize lock release
                SetImage(preset.Path);
                if (_sourceMode != SourceMode.Image) return; // load failed

                KeepAspectRatio = false;
                if (preset.Size.Width > 0 && preset.Size.Height > 0) {
                    ClientSize = preset.Size;
                }
                Location = preset.Location;
                KeepAspectRatio = true;

                this.Opacity = Math.Max(0.1, Math.Min(1.0, preset.Opacity));
                Program.Platform.OnFormStateChange(this);

                Log.Write("Applied preset '{0}': {1} at {2} size {3}",
                    preset.Name, preset.Path, preset.Location, preset.Size);
            }
            catch (Exception ex) {
                Log.WriteException("Failed to apply preset", ex);
            }
        }

        /// <summary>
        /// Captures the current image-mode state as a new preset (without a name).
        /// Returns null when no image is loaded.
        /// </summary>
        public ImagePreset CaptureCurrentAsImagePreset() {
            if (_sourceMode != SourceMode.Image || _imagePanel == null || !_imagePanel.IsShowingImage)
                return null;
            if (string.IsNullOrEmpty(_imagePanel.SourcePath))
                return null;

            Size original = _imagePanel.ImagePixelSize;
            Size current = this.ClientSize;
            double scale = (original.Width > 0 && current.Width > 0)
                ? (double)current.Width / original.Width
                : 1.0;

            return new ImagePreset {
                Path = _imagePanel.SourcePath,
                Location = this.Location,
                Size = current,
                Scale = scale,
                Opacity = this.Opacity
            };
        }

        /// <summary>
        /// Toggles the image preset side panel: opens it if closed, closes it if currently shown.
        /// </summary>
        public void ToggleImagePresetPanel() {
            if (IsSidePanelOpen && _sidePanelContainer != null
                && _sidePanelContainer.CurrentSidePanel is SidePanels.ImagePresetPanel) {
                CloseSidePanel();
            }
            else {
                SetSidePanel(new SidePanels.ImagePresetPanel());
            }
        }

        void PromptLoadImage() {
            using (var dlg = new OpenFileDialog()) {
                dlg.Filter = "Images (*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff)|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff|All files (*.*)|*.*";
                dlg.CheckFileExists = true;
                if (dlg.ShowDialog(this) == DialogResult.OK) {
                    SetImage(dlg.FileName);
                }
            }
        }

        void MainForm_DragEnter(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                var paths = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (paths != null && paths.Length > 0 && IsSupportedImage(paths[0])) {
                    e.Effect = DragDropEffects.Copy;
                    return;
                }
            }
            e.Effect = DragDropEffects.None;
        }

        void MainForm_DragDrop(object sender, DragEventArgs e) {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            var paths = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (paths == null || paths.Length == 0) return;
            if (!IsSupportedImage(paths[0])) return;

            //Close any open side panel and exit fullscreen so the new image owns the form
            if (IsSidePanelOpen) CloseSidePanel();
            SetImage(paths[0]);
        }

        static bool IsSupportedImage(string path) {
            if (string.IsNullOrEmpty(path)) return false;
            var ext = System.IO.Path.GetExtension(path);
            if (string.IsNullOrEmpty(ext)) return false;
            ext = ext.ToLowerInvariant();
            return ext == ".png" || ext == ".jpg" || ext == ".jpeg"
                || ext == ".bmp" || ext == ".gif" || ext == ".webp" || ext == ".tiff" || ext == ".tif";
        }

        #endregion

        #region Accessors

        /// <summary>
        /// Gets the form's thumbnail panel.
        /// </summary>
        public ThumbnailPanel ThumbnailPanel {
            get {
                return _thumbnailPanel;
            }
        }

        /// <summary>
        /// Gets the form's message pump manager.
        /// </summary>
        public MessagePumpManager MessagePumpManager {
            get {
                return _msgPumpManager;
            }
        }

        /// <summary>
        /// Gets the form's window list drop down menu.
        /// </summary>
        public ContextMenuStrip MenuWindows {
            get {
                return menuWindows;
            }
        }

        /// <summary>
        /// Retrieves the window handle of the currently cloned thumbnail.
        /// </summary>
        public WindowHandle CurrentThumbnailWindowHandle {
            get;
            private set;
        }

        #endregion
        
    }
}
