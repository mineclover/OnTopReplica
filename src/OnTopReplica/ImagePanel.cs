using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using OnTopReplica.Native;

namespace OnTopReplica {

    /// <summary>
    /// Displays a static image as the overlay source, drawn at the panel's client size.
    /// Alternative to <see cref="ThumbnailPanel"/> for the static-image comparison mode.
    /// </summary>
    class ImagePanel : Panel {

        Image _image;
        string _sourcePath;

        public ImagePanel() {
            BackColor = Color.Black;
            DoubleBuffered = true;
            SetStyle(ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
        }

        /// <summary>
        /// Gets the original pixel size of the loaded image (Size.Empty if none).
        /// </summary>
        public Size ImagePixelSize {
            get { return _image == null ? Size.Empty : _image.Size; }
        }

        /// <summary>
        /// Gets the path the current image was loaded from (null if none).
        /// </summary>
        public string SourcePath {
            get { return _sourcePath; }
        }

        public bool IsShowingImage {
            get { return _image != null; }
        }

        /// <summary>
        /// Loads an image from disk. The previous image, if any, is disposed.
        /// Throws on IO or decoding failure (caller handles).
        /// </summary>
        public void SetImage(string path) {
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (!File.Exists(path)) throw new FileNotFoundException("Image file not found.", path);

            // Load via stream so the file isn't locked by GDI+.
            Image loaded;
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                loaded = Image.FromStream(fs);
            }

            DisposeImage();
            _image = loaded;
            _sourcePath = path;
            Invalidate();
        }

        public void UnsetImage() {
            DisposeImage();
            _sourcePath = null;
            Invalidate();
        }

        void DisposeImage() {
            if (_image != null) {
                _image.Dispose();
                _image = null;
            }
        }

        protected override void OnPaint(PaintEventArgs e) {
            if (_image == null) {
                base.OnPaint(e);
                return;
            }

            var g = e.Graphics;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.DrawImage(_image, new Rectangle(Point.Empty, ClientSize));
        }

        protected override void OnPaintBackground(PaintEventArgs e) {
            if (_image == null) base.OnPaintBackground(e);
            // Skip background paint when image fills the panel (avoids flicker).
        }

        protected override void Dispose(bool disposing) {
            if (disposing) DisposeImage();
            base.Dispose(disposing);
        }

        // ----- Mouse pass-through to parent form (act like glass caption) -----
        //
        // A plain WinForms Panel captures clicks, blocking the form's
        // drag/right-click/double-click. ThumbnailPanel doesn't suffer this because
        // its content is painted by DWM outside the control. Here we forward:
        //   - LMB down  -> start window drag on the parent
        //   - LMB dblclk -> toggle fullscreen via parent
        //   - RMB up    -> show the parent's ContextMenuStrip
        //
        // Note: WM_NCHITTEST is not received by child controls, so we use the
        // ReleaseCapture + WM_NCLBUTTONDOWN(HTCAPTION) trick to initiate drag.

        const uint WM_NCLBUTTONDOWN = 0x00A1;
        const uint WM_NCLBUTTONDBLCLK = 0x00A3;
        const int HTCAPTION = 2;

        [DllImport("user32.dll")]
        static extern bool ReleaseCapture();

        /// <summary>
        /// When true (set by MainForm placement mode), the panel drags itself
        /// inside its parent instead of dragging the parent form.
        /// </summary>
        public bool PlacementModeActive { get; set; }

        Point _dragOffset;
        bool _dragging;

        protected override void OnMouseDown(MouseEventArgs e) {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left) {
                if (PlacementModeActive) {
                    _dragging = true;
                    _dragOffset = e.Location;
                    Capture = true;
                    return;
                }

                var form = FindForm();
                if (form == null) return;
                ReleaseCapture();
                MessagingMethods.SendMessage(form.Handle, WM_NCLBUTTONDOWN, (IntPtr)HTCAPTION, IntPtr.Zero);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);

            if (_dragging && PlacementModeActive && Parent != null) {
                var parentPt = Parent.PointToClient(Cursor.Position);
                int x = parentPt.X - _dragOffset.X;
                int y = parentPt.Y - _dragOffset.Y;

                //Constrain to parent bounds so the image cannot fully escape the canvas
                x = Math.Max(-Width + 20, Math.Min(Parent.ClientSize.Width - 20, x));
                y = Math.Max(-Height + 20, Math.Min(Parent.ClientSize.Height - 20, y));
                Location = new Point(x, y);
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e) {
            base.OnMouseDoubleClick(e);

            var form = FindForm();
            if (form == null || e.Button != MouseButtons.Left) return;

            // Mirror MainForm's caption-doubleclick → fullscreen toggle.
            MessagingMethods.SendMessage(form.Handle, WM_NCLBUTTONDBLCLK, (IntPtr)HTCAPTION, IntPtr.Zero);
        }

        protected override void OnMouseUp(MouseEventArgs e) {
            base.OnMouseUp(e);

            if (e.Button == MouseButtons.Left && _dragging) {
                _dragging = false;
                Capture = false;
                return;
            }

            if (e.Button != MouseButtons.Right) return;
            var main = FindForm() as MainForm;
            if (main != null) main.OpenContextMenu(Control.MousePosition);
        }
    }
}
