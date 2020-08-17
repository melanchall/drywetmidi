using System;
using Melanchall.DryWetMidi.Core;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Core
{
    [TestFixture]
    public sealed class ChannelEventTests
    {
        #region Test methods

        [Test]
        public void PitchBend_SetInvalidPitchValue()
        {
            var pitchBendEvent = new PitchBendEvent();
            Assert.Throws<ArgumentOutOfRangeException>(() => pitchBendEvent.PitchValue = PitchBendEvent.MaxPitchValue + 1);
        }

        [Test]
        public void PitchBend_CreateWithInvalidPitchValue()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new PitchBendEvent(PitchBendEvent.MaxPitchValue + 1));
        }

        [Test]
        public void PitchBend_SetPitchValue()
        {
            const ushort pitchValue = 1234;

            var pitchBendEvent = new PitchBendEvent();
            pitchBendEvent.PitchValue = pitchValue;
            Assert.AreEqual(pitchValue, pitchBendEvent.PitchValue, "Pitch value is set incorrectly.");
        }

        [Test]
        public void PitchBend_CreateWithPitchValue()
        {
            const ushort pitchValue = 1234;

            var pitchBendEvent = new PitchBendEvent(pitchValue);
            Assert.AreEqual(pitchValue, pitchBendEvent.PitchValue, "Pitch value is set incorrectly.");
        }

        [Test]
        public void PitchBend_CreateDefault()
        {
            var pitchBendEvent = new PitchBendEvent();
            Assert.AreEqual(PitchBendEvent.MinPitchValue, pitchBendEvent.PitchValue, "Pitch value is invalid.");
        }

        #endregion
    }
}
