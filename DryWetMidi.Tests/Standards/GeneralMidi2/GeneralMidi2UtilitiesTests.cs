using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Standards;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Standards
{
    // TODO: add more tests
    [TestFixture]
    public sealed class GeneralMidi2UtilitiesTests
    {
        #region Test methods

        [Test]
        public void GetProgramEvents()
        {
            var channel = (FourBitNumber)3;
            var programEvents = GeneralMidi2Program.AcousticGrandPianoDark.GetProgramEvents(channel).ToArray();

            MidiAsserts.AreEqual(
                new MidiEvent[]
                {
                    new ControlChangeEvent(ControlName.BankSelect.AsSevenBitNumber(), (SevenBitNumber)0x79) { Channel = channel },
                    new ControlChangeEvent(ControlName.LsbForBankSelect.AsSevenBitNumber(), (SevenBitNumber)0x02) { Channel = channel },
                    new ProgramChangeEvent((SevenBitNumber)0x00) { Channel = channel }
                },
                programEvents,
                true,
                "Invalid events.");
        }

        [Test]
        public void GetPercussionSetEvents([Values] GeneralMidi2PercussionSet percussionSet)
        {
            var channel = (FourBitNumber)3;
            var programEvents = percussionSet.GetPercussionSetEvents(channel).ToArray();

            MidiAsserts.AreEqual(
                new MidiEvent[]
                {
                    new ControlChangeEvent(ControlName.BankSelect.AsSevenBitNumber(), (SevenBitNumber)0x78) { Channel = channel },
                    new ControlChangeEvent(ControlName.LsbForBankSelect.AsSevenBitNumber(), (SevenBitNumber)0x00) { Channel = channel },
                    new ProgramChangeEvent((SevenBitNumber)(byte)percussionSet) { Channel = channel }
                },
                programEvents,
                true,
                "Invalid events.");
        }

        #endregion
    }
}
