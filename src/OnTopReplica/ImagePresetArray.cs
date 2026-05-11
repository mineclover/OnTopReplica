using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace OnTopReplica {

    /// <summary>
    /// XML-serializable list of <see cref="ImagePreset"/>. Stored under
    /// <c>Settings.Default.ImagePresets</c>.
    /// </summary>
    public class ImagePresetArray : List<ImagePreset>, IXmlSerializable {

        public System.Xml.Schema.XmlSchema GetSchema() { return null; }

        public void ReadXml(XmlReader reader) {
            this.Clear();

            var doc = XDocument.Load(reader);
            foreach (var el in doc.Descendants("ImagePreset")) {
                var p = Parse(el);
                if (p != null) this.Add(p);
            }
        }

        public void WriteXml(XmlWriter writer) {
            foreach (var p in this) Write(writer, p);
        }

        static ImagePreset Parse(XElement el) {
            try {
                string path = ((string)el.Element("Path") ?? "").Trim();
                if (string.IsNullOrEmpty(path)) return null;

                return new ImagePreset {
                    Name = ((string)el.Attribute("name") ?? "").Trim(),
                    Path = path,
                    Location = new Point(
                        ParseInt(el.Element("X")),
                        ParseInt(el.Element("Y"))
                    ),
                    Size = new Size(
                        ParseInt(el.Element("Width")),
                        ParseInt(el.Element("Height"))
                    ),
                    Scale = ClampOpacityLike(ParseDouble(el.Element("Scale"), 1.0), 0.01, 100.0),
                    Opacity = ClampOpacityLike(ParseDouble(el.Element("Opacity"), 1.0), 0.1, 1.0),
                };
            }
            catch (Exception ex) {
                System.Diagnostics.Debug.Fail("Failed to parse ImagePreset", ex.ToString());
                return null;
            }
        }

        static double ClampOpacityLike(double v, double min, double max) {
            if (v < min) return min;
            if (v > max) return max;
            return v;
        }

        static void Write(XmlWriter writer, ImagePreset p) {
            writer.WriteStartElement("ImagePreset");
            writer.WriteAttributeString("name", p.Name ?? "");
            writer.WriteElementString("Path", p.Path ?? "");
            writer.WriteElementString("X", p.Location.X.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("Y", p.Location.Y.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("Width", p.Size.Width.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("Height", p.Size.Height.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("Scale", p.Scale.ToString("R", CultureInfo.InvariantCulture));
            writer.WriteElementString("Opacity", p.Opacity.ToString("R", CultureInfo.InvariantCulture));
            writer.WriteEndElement();
        }

        static int ParseInt(XElement el) {
            if (el == null) return 0;
            int v;
            return int.TryParse(el.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out v) ? v : 0;
        }

        static double ParseDouble(XElement el, double fallback) {
            if (el == null) return fallback;
            double v;
            return double.TryParse(el.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out v) ? v : fallback;
        }
    }
}
