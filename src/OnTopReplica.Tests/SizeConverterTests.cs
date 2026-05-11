using System.Drawing;

namespace OnTopReplica.Tests {

    public class SizeConverterTests {

        OnTopReplica.StartupOptions.SizeConverter NewConverter() {
            return new OnTopReplica.StartupOptions.SizeConverter();
        }

        [Test]
        public void ConvertFrom_Positive_ParsesWidthHeight() {
            var s = (Size)NewConverter().ConvertFrom("640, 480");
            Assert.AreEqual(new Size(640, 480), s);
        }

        [Test]
        public void ConvertFrom_Negative_ParsesNegativeValues() {
            // Regression for commit 63e9487: regex must allow negative values.
            var s = (Size)NewConverter().ConvertFrom("-10, -20");
            Assert.AreEqual(new Size(-10, -20), s);
        }

        [Test]
        public void ConvertFrom_MixedSigns_Parses() {
            var s = (Size)NewConverter().ConvertFrom("-5, 100");
            Assert.AreEqual(new Size(-5, 100), s);
        }

        [Test]
        public void ConvertTo_SizeToString_FormatsCorrectly() {
            var str = (string)NewConverter().ConvertTo(new Size(800, 600), typeof(string));
            Assert.AreEqual("800, 600", str);
        }

        [Test]
        public void ConvertTo_NegativeSize_FormatsCorrectly() {
            var str = (string)NewConverter().ConvertTo(new Size(-50, -75), typeof(string));
            Assert.AreEqual("-50, -75", str);
        }

        [Test]
        public void RoundTrip_NegativeValues_Preserved() {
            var c = NewConverter();
            var original = new Size(-123, -456);
            var str = (string)c.ConvertTo(original, typeof(string));
            var parsed = (Size)c.ConvertFrom(str);
            Assert.AreEqual(original, parsed);
        }
    }
}
