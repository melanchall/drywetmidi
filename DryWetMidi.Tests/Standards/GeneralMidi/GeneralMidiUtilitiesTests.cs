using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Standards;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Standards
{
    [TestFixture]
    public sealed class GeneralMidiUtilitiesTests
    {
        #region Test methods

        [Test]
        public void AsSevenBitNumber_Program()
        {
            var values = Enum
                .GetValues(typeof(GeneralMidiProgram))
                .Cast<GeneralMidiProgram>()
                .ToArray();

            foreach (var value in values)
            {
                var sevenBitNumber = value.AsSevenBitNumber();
                ClassicAssert.AreEqual(
                    (byte)value,
                    (byte)sevenBitNumber,
                    $"Invalid seven-bit number.");
            }
        }

        [Test]
        public void AsSevenBitNumber_Percussion()
        {
            var values = Enum
                .GetValues(typeof(GeneralMidiPercussion))
                .Cast<GeneralMidiPercussion>()
                .ToArray();

            foreach (var value in values)
            {
                var sevenBitNumber = value.AsSevenBitNumber();
                ClassicAssert.AreEqual(
                    (byte)value,
                    (byte)sevenBitNumber,
                    $"Invalid seven-bit number.");
            }
        }

        [Test]
        public void GetProgramEvent()
        {
            var programs = Enum
                .GetValues(typeof(GeneralMidiProgram))
                .Cast<GeneralMidiProgram>()
                .ToArray();
            var channels = FourBitNumber
                .Values;

            foreach (var program in programs)
            {
                foreach (var channel in channels)
                {
                    var midiEvent = program.GetProgramEvent(channel);
                    MidiAsserts.AreEqual(
                        new ProgramChangeEvent(program.AsSevenBitNumber()) { Channel = channel },
                        midiEvent,
                        true,
                        "Invalid event.");
                }
            }
        }

        [Test]
        public void GetNoteOnEvent()
        {
            var percussions = Enum
                .GetValues(typeof(GeneralMidiPercussion))
                .Cast<GeneralMidiPercussion>()
                .ToArray();
            var channels = FourBitNumber
                .Values;
            var velocities = SevenBitNumber
                .Values;

            foreach (var percussion in percussions)
            {
                foreach (var channel in channels)
                {
                    foreach (var velocity in velocities)
                    {
                        var midiEvent = percussion.GetNoteOnEvent(velocity, channel);
                        MidiAsserts.AreEqual(
                            new NoteOnEvent(percussion.AsSevenBitNumber(), velocity) { Channel = channel },
                            midiEvent,
                            true,
                            "Invalid event.");
                    }
                }
            }
        }

        [Test]
        public void GetNoteOffEvent()
        {
            var percussions = Enum
                .GetValues(typeof(GeneralMidiPercussion))
                .Cast<GeneralMidiPercussion>()
                .ToArray();
            var channels = FourBitNumber
                .Values;
            var velocities = SevenBitNumber
                .Values;

            foreach (var percussion in percussions)
            {
                foreach (var channel in channels)
                {
                    foreach (var velocity in velocities)
                    {
                        var midiEvent = percussion.GetNoteOffEvent(velocity, channel);
                        MidiAsserts.AreEqual(
                            new NoteOffEvent(percussion.AsSevenBitNumber(), velocity) { Channel = channel },
                            midiEvent,
                            true,
                            "Invalid event.");
                    }
                }
            }
        }

        #endregion
    }
}
