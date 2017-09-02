using Melanchall.DryWetMidi.Smf.Interaction;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction
{
    [TestClass]
    public sealed class MidiTimeTests
    {
        #region TestMethods

        [TestMethod]
        [Description("Try parse MIDI time in form of 'time'.")]
        public void TryParse_Valid()
        {
            TimeParsingTester.TestTryParse("123", (MidiTime)123);
        }

        [TestMethod]
        [Description("Try parse negative MIDI time.")]
        public void TryParse_Inalid_Negative()
        {
            Assert.IsFalse(MidiTime.TryParse("-234", out var time));
        }

        [TestMethod]
        [Description("Try parse NaN time.")]
        public void TryParse_Inalid_NaN()
        {
            Assert.IsFalse(MidiTime.TryParse("abc", out var time));
        }

        [TestMethod]
        [Description("Parse string representation of a time.")]
        public void Parse_ToString()
        {
            TimeParsingTester.TestToString(new MidiTime(987));
        }

        #endregion
    }
}
