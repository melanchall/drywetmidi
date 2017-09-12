using Melanchall.DryWetMidi.Smf.Interaction;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction
{
    [TestClass]
    public class MetricLengthTests
    {
        #region Test methods

        [TestMethod]
        [Description("Try parse metric length in form of 'Hours:Minutes:Seconds:Milliseconds'.")]
        public void TryParse_HoursMinutesSecondsMilliseconds()
        {
            LengthParsingTester.TestTryParse("2:4:30:567", new MetricLength(2, 4, 30, 567));
        }

        [TestMethod]
        [Description("Try parse metric length in form of 'Hours:Minutes:Seconds'.")]
        public void TryParse_HoursMinutesSeconds()
        {
            LengthParsingTester.TestTryParse("2:4:30", new MetricLength(2, 4, 30));
        }

        [TestMethod]
        [Description("Try parse metric length in form of 'Minutes:Seconds'.")]
        public void TryParse_MinutesSeconds()
        {
            LengthParsingTester.TestTryParse("4:30", new MetricLength(0, 4, 30));
        }

        [TestMethod]
        [Description("Parse string representation of a length.")]
        public void Parse_ToString()
        {
            LengthParsingTester.TestToString(new MetricLength(3, 6, 8, 987));
        }

        [TestMethod]
        [Description("Add metric length.")]
        public void Add_Metric()
        {
            var actual = new MetricLength(0, 1, 30).Add(new MetricLength(0, 0, 3));
            var expected = new MetricLength(0, 1, 33);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Description("Add musical length.")]
        public void Add_Musical()
        {
            var actual = new MetricLength(0, 1, 30).Add((MusicalLength)MusicalFraction.Eighth);
            var expected = new MathLength(new MetricLength(0, 1, 30),
                                          (MusicalLength)MusicalFraction.Eighth,
                                          MathOperation.Add);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Description("Subtract metric length.")]
        public void Subtract_Metric()
        {
            var actual = new MetricLength(0, 1, 30).Subtract(new MetricLength(0, 1, 3));
            var expected = new MetricLength(0, 0, 27);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Description("Subtract MIDI length.")]
        public void Subtract_Midi()
        {
            var actual = new MetricLength(0, 1, 30).Subtract(new MidiLength(300));
            var expected = new MathLength(new MetricLength(0, 1, 30),
                                          new MidiLength(300),
                                          MathOperation.Subtract);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Description("Multiply metric length.")]
        public void Multiply()
        {
            var actual = new MetricLength(0, 1, 30).Multiply(3);
            var expected = new MetricLength(0, 4, 30);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Description("Divide metric length.")]
        public void Divide()
        {
            var actual = new MetricLength(3, 1, 30).Divide(3);
            var expected = new MetricLength(1, 0, 30);

            Assert.AreEqual(expected, actual);
        }

        #endregion
    }
}
