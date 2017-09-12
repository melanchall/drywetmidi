using Melanchall.DryWetMidi.Smf.Interaction;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction
{
    [TestClass]
    public sealed class MidiLengthTests
    {
        #region TestMethods

        [TestMethod]
        [Description("Try parse MIDI length in form of 'length'.")]
        public void TryParse_Valid()
        {
            LengthParsingTester.TestTryParse("123", (MidiLength)123);
        }

        [TestMethod]
        [Description("Try parse negative MIDI length.")]
        public void TryParse_Inalid_Negative()
        {
            Assert.IsFalse(MidiLength.TryParse("-234", out var length));
        }

        [TestMethod]
        [Description("Try parse NaN length.")]
        public void TryParse_Inalid_NaN()
        {
            Assert.IsFalse(MidiLength.TryParse("abc", out var length));
        }

        [TestMethod]
        [Description("Parse string representation of a length.")]
        public void Parse_ToString()
        {
            LengthParsingTester.TestToString(new MidiLength(987));
        }

        [TestMethod]
        [Description("Add MIDI length.")]
        public void Add_Midi()
        {
            var actual = ((MidiLength)300).Add((MidiLength)40);
            var expected = (MidiLength)340;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Description("Add metric length.")]
        public void Add_Metric()
        {
            var actual = ((MidiLength)300).Add(new MetricLength(0, 1, 30));
            var expected = new MathLength((MidiLength)300,
                                          new MetricLength(0, 1, 30),
                                          MathOperation.Add);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Description("Subtract MIDI length.")]
        public void Subtract_Midi()
        {
            var actual = ((MidiLength)300).Subtract((MidiLength)40);
            var expected = (MidiLength)260;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Description("Subtract MIDI length.")]
        public void Subtract_Musical()
        {
            var actual = ((MidiLength)300).Subtract((MusicalLength)MusicalFraction.QuarterDotted);
            var expected = new MathLength((MidiLength)300,
                                          (MusicalLength)MusicalFraction.QuarterDotted,
                                          MathOperation.Subtract);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Description("Multiply MIDI length.")]
        public void Multiply()
        {
            var actual = ((MidiLength)350).Multiply(3);
            var expected = (MidiLength)1050;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Description("Divide MIDI length.")]
        public void Divide()
        {
            var actual = ((MidiLength)450).Divide(3);
            var expected = (MidiLength)150;

            Assert.AreEqual(expected, actual);
        }

        #endregion
    }
}
