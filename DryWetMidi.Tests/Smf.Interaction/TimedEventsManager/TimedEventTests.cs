using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction
{
    [TestFixture]
    public sealed class TimedEventTests
    {
        #region Test methods

        [Test]
        [Description("Check that clone of a timed event equals to the original one.")]
        public void Clone()
        {
            var timedEvent = new TimedEvent(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)50));
            Assert.IsTrue(TimedEventEquality.AreEqual(timedEvent, timedEvent.Clone(), true),
                          "Clone of a timed event doesn't equal to the original one.");
        }

        #endregion
    }
}
