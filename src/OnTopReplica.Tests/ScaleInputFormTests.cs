using OnTopReplica;

namespace OnTopReplica.Tests {

    public class ScaleInputFormTests {

        [Test]
        public void ToScaleFactor_100Percent_Returns1() {
            Assert.AreClose(1.0, ScaleInputForm.ToScaleFactor(100m), 1e-9);
        }

        [Test]
        public void ToScaleFactor_50Percent_Returns0Point5() {
            Assert.AreClose(0.5, ScaleInputForm.ToScaleFactor(50m), 1e-9);
        }

        [Test]
        public void ToScaleFactor_FractionalPercent_PreservesDecimal() {
            Assert.AreClose(0.6777, ScaleInputForm.ToScaleFactor(67.77m), 1e-9);
        }

        [Test]
        public void ToScaleFactor_MinPercent_Returns0Point1() {
            Assert.AreClose(0.1, ScaleInputForm.ToScaleFactor(10m), 1e-9);
        }

        [Test]
        public void ToScaleFactor_MaxPercent_Returns10() {
            Assert.AreClose(10.0, ScaleInputForm.ToScaleFactor(1000m), 1e-9);
        }
    }
}
