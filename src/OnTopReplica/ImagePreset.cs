using System.Drawing;

namespace OnTopReplica {

    /// <summary>
    /// A saved configuration for displaying an image overlay:
    /// the file path, the window position, the display size (or scale of the original),
    /// and opacity. One preset is "applied" at a time via the preset panel or a hotkey.
    /// </summary>
    public class ImagePreset {

        public ImagePreset() {
            Opacity = 1.0;
            Scale = 1.0;
            Hotkey = "";
        }

        public ImagePreset(string name, string path, Point location, Size size, double scale, double opacity)
            : this() {
            Name = name;
            Path = path;
            Location = location;
            Size = size;
            Scale = scale;
            Opacity = opacity;
        }

        public string Name { get; set; }
        public string Path { get; set; }
        public Point Location { get; set; }
        public Size Size { get; set; }

        /// <summary>Scale factor relative to original image (1.0 = 1:1). Informational; Size is authoritative.</summary>
        public double Scale { get; set; }

        /// <summary>Window opacity 0..1.</summary>
        public double Opacity { get; set; }

        /// <summary>
        /// Global hotkey that applies this preset, in the HotKeyTextBox spec format
        /// (e.g. "[CTRL]+[ALT]+1"). Empty string = no hotkey.
        /// </summary>
        public string Hotkey { get; set; }

        public override string ToString() {
            return string.IsNullOrEmpty(Name) ? Path : Name;
        }
    }
}
