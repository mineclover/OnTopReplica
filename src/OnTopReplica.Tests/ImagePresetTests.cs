using System.Drawing;
using System.IO;
using System.Text;
using System.Xml;
using OnTopReplica;

namespace OnTopReplica.Tests {

    public class ImagePresetTests {

        [Test]
        public void Defaults_OpacityAndScaleAreOne() {
            var p = new ImagePreset();
            Assert.AreClose(1.0, p.Opacity, 1e-9);
            Assert.AreClose(1.0, p.Scale, 1e-9);
        }

        [Test]
        public void ArrayRoundTrip_PreservesAllFields() {
            var arr = new ImagePresetArray {
                new ImagePreset("A", @"C:\imgs\a.png", new Point(100, 200), new Size(1920, 1080), 1.0, 0.8),
                new ImagePreset("B", @"C:\imgs\b.png", new Point(-5, 0), new Size(800, 600), 0.6777, 0.5)
            };

            string xml;
            var sb = new StringBuilder();
            using (var w = XmlWriter.Create(sb)) {
                w.WriteStartElement("Root");
                arr.WriteXml(w);
                w.WriteEndElement();
            }
            xml = sb.ToString();

            var parsed = new ImagePresetArray();
            using (var r = XmlReader.Create(new StringReader(xml))) {
                r.ReadToDescendant("Root");
                parsed.ReadXml(r);
            }

            Assert.AreEqual(2, parsed.Count);
            Assert.AreEqual("A", parsed[0].Name);
            Assert.AreEqual(@"C:\imgs\a.png", parsed[0].Path);
            Assert.AreEqual(new Point(100, 200), parsed[0].Location);
            Assert.AreEqual(new Size(1920, 1080), parsed[0].Size);
            Assert.AreClose(1.0, parsed[0].Scale, 1e-9);
            Assert.AreClose(0.8, parsed[0].Opacity, 1e-9);

            Assert.AreEqual("B", parsed[1].Name);
            Assert.AreEqual(new Point(-5, 0), parsed[1].Location);
            Assert.AreClose(0.6777, parsed[1].Scale, 1e-9);
        }

        [Test]
        public void Parse_DropsEntryWithEmptyPath() {
            string xml = "<Root><ImagePreset name=\"good\"><Path>C:/a.png</Path><X>0</X><Y>0</Y><Width>1</Width><Height>1</Height><Scale>1</Scale><Opacity>1</Opacity></ImagePreset>"
                       + "<ImagePreset name=\"bad\"><Path></Path><X>0</X><Y>0</Y><Width>1</Width><Height>1</Height><Scale>1</Scale><Opacity>1</Opacity></ImagePreset></Root>";
            var parsed = new ImagePresetArray();
            using (var r = XmlReader.Create(new StringReader(xml))) {
                r.ReadToDescendant("Root");
                parsed.ReadXml(r);
            }
            Assert.AreEqual(1, parsed.Count);
            Assert.AreEqual("good", parsed[0].Name);
        }

        [Test]
        public void Parse_ClampsOpacityToValidRange() {
            string xml = "<Root><ImagePreset name=\"a\"><Path>x</Path><X>0</X><Y>0</Y><Width>1</Width><Height>1</Height><Scale>1</Scale><Opacity>5.0</Opacity></ImagePreset></Root>";
            var parsed = new ImagePresetArray();
            using (var r = XmlReader.Create(new StringReader(xml))) {
                r.ReadToDescendant("Root");
                parsed.ReadXml(r);
            }
            Assert.AreEqual(1, parsed.Count);
            Assert.AreClose(1.0, parsed[0].Opacity, 1e-9);
        }

        [Test]
        public void Parse_MissingFieldsUseDefaults() {
            string xml = "<Root><ImagePreset name=\"a\"><Path>x</Path></ImagePreset></Root>";
            var parsed = new ImagePresetArray();
            using (var r = XmlReader.Create(new StringReader(xml))) {
                r.ReadToDescendant("Root");
                parsed.ReadXml(r);
            }
            Assert.AreEqual(1, parsed.Count);
            Assert.AreEqual(new Point(0, 0), parsed[0].Location);
            Assert.AreClose(1.0, parsed[0].Scale, 1e-9);
            Assert.AreClose(1.0, parsed[0].Opacity, 1e-9);
        }

        [Test]
        public void ArrayRoundTrip_EmptyList() {
            var arr = new ImagePresetArray();
            var sb = new StringBuilder();
            using (var w = XmlWriter.Create(sb)) {
                w.WriteStartElement("Root");
                arr.WriteXml(w);
                w.WriteEndElement();
            }
            var parsed = new ImagePresetArray();
            using (var r = XmlReader.Create(new StringReader(sb.ToString()))) {
                r.ReadToDescendant("Root");
                parsed.ReadXml(r);
            }
            Assert.AreEqual(0, parsed.Count);
        }
    }
}
