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
            MidiTime.TryParse("123", out var actualTime);
            Assert.AreEqual((MidiTime)123, actualTime);
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
            var expectedTime = new MidiTime(987);
            Assert.AreEqual(expectedTime, MidiTime.Parse(expectedTime.ToString()));
        }

        #endregion

        #region Private methods

        private static void TestTryParse(string input, MidiTime expectedTime)
        {
            MidiTime.TryParse(input, out var actualTime);
            Assert.AreEqual(expectedTime, actualTime);
        }

        #endregion
    }
}
