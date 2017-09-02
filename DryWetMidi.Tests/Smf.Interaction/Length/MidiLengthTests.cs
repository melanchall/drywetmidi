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

        #endregion
    }
}
