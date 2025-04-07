using System;
using Melanchall.DryWetMidi.Core;
using NUnit.Framework;
using NUnit.Framework.Legacy;

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
            ClassicAssert.Throws<ArgumentOutOfRangeException>(() => pitchBendEvent.PitchValue = PitchBendEvent.MaxPitchValue + 1);
        }

        [Test]
        public void PitchBend_CreateWithInvalidPitchValue()
        {
            ClassicAssert.Throws<ArgumentOutOfRangeException>(() => new PitchBendEvent(PitchBendEvent.MaxPitchValue + 1));
        }

        [Test]
        public void PitchBend_SetPitchValue()
        {
            const ushort pitchValue = 1234;

            var pitchBendEvent = new PitchBendEvent();
            pitchBendEvent.PitchValue = pitchValue;
            ClassicAssert.AreEqual(pitchValue, pitchBendEvent.PitchValue, "Pitch value is set incorrectly.");
        }

        [Test]
        public void PitchBend_CreateWithPitchValue()
        {
            const ushort pitchValue = 1234;

            var pitchBendEvent = new PitchBendEvent(pitchValue);
            ClassicAssert.AreEqual(pitchValue, pitchBendEvent.PitchValue, "Pitch value is set incorrectly.");
        }

        [Test]
        public void PitchBend_CreateDefault()
        {
            var pitchBendEvent = new PitchBendEvent();
            ClassicAssert.AreEqual(PitchBendEvent.DefaultPitchValue, pitchBendEvent.PitchValue, "Pitch value is invalid.");
        }

        #endregion
    }
}
