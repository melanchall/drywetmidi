using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Standards;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.ComponentModel;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Standards
{
    [TestFixture]
    public sealed class GeneralSoundUtilitiesTests
    {
        #region Constants

        private const byte InvalidEnumValue = 1;

        #endregion

        #region Test methods

        [Test]
        public void GetPercussionSetEvents(
            [Values] GeneralSoundPercussionSet percussionSet,
            [Values(0, 9, 15)] byte channelNumber)
        {
            var channel = (FourBitNumber)channelNumber;

            var percussionSetEvents = percussionSet.GetPercussionSetEvents(channel).ToArray();

            MidiAsserts.AreEqual(
                new MidiEvent[]
                {
                    new ControlChangeEvent(ControlName.BankSelect.AsSevenBitNumber(), (SevenBitNumber)0x78) { Channel = channel },
                    new ControlChangeEvent(ControlName.LsbForBankSelect.AsSevenBitNumber(), (SevenBitNumber)0x00) { Channel = channel },
                    new ProgramChangeEvent((SevenBitNumber)(byte)percussionSet) { Channel = channel }
                },
                percussionSetEvents,
                true,
                $"Invalid events for '{percussionSet}' percussion set.");
        }

        [Test]
        public void GetProgramEvent(
            [Values] GeneralSoundPercussionSet percussionSet,
            [Values(0, 9, 15)] byte channelNumber)
        {
            var channel = (FourBitNumber)channelNumber;

            var programEvent = percussionSet.GetProgramEvent(channel);

            MidiAsserts.AreEqual(
                new ProgramChangeEvent((SevenBitNumber)(byte)percussionSet) { Channel = channel },
                programEvent,
                true,
                $"Invalid event for '{percussionSet}' percussion set.");
        }

        [Test]
        public void AsSevenBitNumber_PercussionSet([Values] GeneralSoundPercussionSet percussionSet) =>
            CheckAsSevenBitNumber(percussionSet, percussionSet.AsSevenBitNumber());

        [Test]
        public void AsSevenBitNumber_Cm6432LPercussion([Values] GeneralSoundCm6432LPercussion percussion) =>
            CheckAsSevenBitNumber(percussion, percussion.AsSevenBitNumber());

        [Test]
        public void AsSevenBitNumber_Tr808Percussion([Values] GeneralSoundTr808Percussion percussion) =>
            CheckAsSevenBitNumber(percussion, percussion.AsSevenBitNumber());

        [Test]
        public void AsSevenBitNumber_BrushPercussion([Values] GeneralSoundBrushPercussion percussion) =>
            CheckAsSevenBitNumber(percussion, percussion.AsSevenBitNumber());

        [Test]
        public void AsSevenBitNumber_ElectronicPercussion([Values] GeneralSoundElectronicPercussion percussion) =>
            CheckAsSevenBitNumber(percussion, percussion.AsSevenBitNumber());

        [Test]
        public void AsSevenBitNumber_JazzPercussion([Values] GeneralSoundJazzPercussion percussion) =>
            CheckAsSevenBitNumber(percussion, percussion.AsSevenBitNumber());

        [Test]
        public void AsSevenBitNumber_OrchestraPercussion([Values] GeneralSoundOrchestraPercussion percussion) =>
            CheckAsSevenBitNumber(percussion, percussion.AsSevenBitNumber());

        [Test]
        public void AsSevenBitNumber_PowerPercussion([Values] GeneralSoundPowerPercussion percussion) =>
            CheckAsSevenBitNumber(percussion, percussion.AsSevenBitNumber());

        [Test]
        public void AsSevenBitNumber_RoomPercussion([Values] GeneralSoundRoomPercussion percussion) =>
            CheckAsSevenBitNumber(percussion, percussion.AsSevenBitNumber());

        [Test]
        public void AsSevenBitNumber_SfxPercussion([Values] GeneralSoundSfxPercussion percussion) =>
            CheckAsSevenBitNumber(percussion, percussion.AsSevenBitNumber());

        [Test]
        public void AsSevenBitNumber_StandardPercussion([Values] GeneralSoundStandardPercussion percussion) =>
            CheckAsSevenBitNumber(percussion, percussion.AsSevenBitNumber());

        [Test]
        public void GetNoteEvents_Cm6432LPercussion([Values] GeneralSoundCm6432LPercussion percussion) =>
            CheckNoteEvents(percussion, GeneralSoundUtilities.GetNoteOnEvent, GeneralSoundUtilities.GetNoteOffEvent);

        [Test]
        public void GetNoteEvents_Tr808Percussion([Values] GeneralSoundTr808Percussion percussion) =>
            CheckNoteEvents(percussion, GeneralSoundUtilities.GetNoteOnEvent, GeneralSoundUtilities.GetNoteOffEvent);

        [Test]
        public void GetNoteEvents_BrushPercussion([Values] GeneralSoundBrushPercussion percussion) =>
            CheckNoteEvents(percussion, GeneralSoundUtilities.GetNoteOnEvent, GeneralSoundUtilities.GetNoteOffEvent);

        [Test]
        public void GetNoteEvents_ElectronicPercussion([Values] GeneralSoundElectronicPercussion percussion) =>
            CheckNoteEvents(percussion, GeneralSoundUtilities.GetNoteOnEvent, GeneralSoundUtilities.GetNoteOffEvent);

        [Test]
        public void GetNoteEvents_JazzPercussion([Values] GeneralSoundJazzPercussion percussion) =>
            CheckNoteEvents(percussion, GeneralSoundUtilities.GetNoteOnEvent, GeneralSoundUtilities.GetNoteOffEvent);

        [Test]
        public void GetNoteEvents_OrchestraPercussion([Values] GeneralSoundOrchestraPercussion percussion) =>
            CheckNoteEvents(percussion, GeneralSoundUtilities.GetNoteOnEvent, GeneralSoundUtilities.GetNoteOffEvent);

        [Test]
        public void GetNoteEvents_PowerPercussion([Values] GeneralSoundPowerPercussion percussion) =>
            CheckNoteEvents(percussion, GeneralSoundUtilities.GetNoteOnEvent, GeneralSoundUtilities.GetNoteOffEvent);

        [Test]
        public void GetNoteEvents_RoomPercussion([Values] GeneralSoundRoomPercussion percussion) =>
            CheckNoteEvents(percussion, GeneralSoundUtilities.GetNoteOnEvent, GeneralSoundUtilities.GetNoteOffEvent);

        [Test]
        public void GetNoteEvents_SfxPercussion([Values] GeneralSoundSfxPercussion percussion) =>
            CheckNoteEvents(percussion, GeneralSoundUtilities.GetNoteOnEvent, GeneralSoundUtilities.GetNoteOffEvent);

        [Test]
        public void GetNoteEvents_StandardPercussion([Values] GeneralSoundStandardPercussion percussion) =>
            CheckNoteEvents(percussion, GeneralSoundUtilities.GetNoteOnEvent, GeneralSoundUtilities.GetNoteOffEvent);

        [Test]
        public void InvalidPercussionSet()
        {
            var percussionSet = (GeneralSoundPercussionSet)InvalidEnumValue;
            var channel = (FourBitNumber)0;

            ClassicAssert.Throws<InvalidEnumArgumentException>(
                () => percussionSet.GetPercussionSetEvents(channel),
                "Exception is not thrown by GetPercussionSetEvents.");
            ClassicAssert.Throws<InvalidEnumArgumentException>(
                () => percussionSet.GetProgramEvent(channel),
                "Exception is not thrown by GetProgramEvent.");
            ClassicAssert.Throws<InvalidEnumArgumentException>(
                () => percussionSet.AsSevenBitNumber(),
                "Exception is not thrown by AsSevenBitNumber.");
        }

        [Test]
        public void InvalidPercussion()
        {
            CheckInvalidPercussion<GeneralSoundCm6432LPercussion>(
                GeneralSoundUtilities.AsSevenBitNumber,
                GeneralSoundUtilities.GetNoteOnEvent,
                GeneralSoundUtilities.GetNoteOffEvent);
            CheckInvalidPercussion<GeneralSoundTr808Percussion>(
                GeneralSoundUtilities.AsSevenBitNumber,
                GeneralSoundUtilities.GetNoteOnEvent,
                GeneralSoundUtilities.GetNoteOffEvent);
            CheckInvalidPercussion<GeneralSoundBrushPercussion>(
                GeneralSoundUtilities.AsSevenBitNumber,
                GeneralSoundUtilities.GetNoteOnEvent,
                GeneralSoundUtilities.GetNoteOffEvent);
            CheckInvalidPercussion<GeneralSoundElectronicPercussion>(
                GeneralSoundUtilities.AsSevenBitNumber,
                GeneralSoundUtilities.GetNoteOnEvent,
                GeneralSoundUtilities.GetNoteOffEvent);
            CheckInvalidPercussion<GeneralSoundJazzPercussion>(
                GeneralSoundUtilities.AsSevenBitNumber,
                GeneralSoundUtilities.GetNoteOnEvent,
                GeneralSoundUtilities.GetNoteOffEvent);
            CheckInvalidPercussion<GeneralSoundOrchestraPercussion>(
                GeneralSoundUtilities.AsSevenBitNumber,
                GeneralSoundUtilities.GetNoteOnEvent,
                GeneralSoundUtilities.GetNoteOffEvent);
            CheckInvalidPercussion<GeneralSoundPowerPercussion>(
                GeneralSoundUtilities.AsSevenBitNumber,
                GeneralSoundUtilities.GetNoteOnEvent,
                GeneralSoundUtilities.GetNoteOffEvent);
            CheckInvalidPercussion<GeneralSoundRoomPercussion>(
                GeneralSoundUtilities.AsSevenBitNumber,
                GeneralSoundUtilities.GetNoteOnEvent,
                GeneralSoundUtilities.GetNoteOffEvent);
            CheckInvalidPercussion<GeneralSoundSfxPercussion>(
                GeneralSoundUtilities.AsSevenBitNumber,
                GeneralSoundUtilities.GetNoteOnEvent,
                GeneralSoundUtilities.GetNoteOffEvent);
            CheckInvalidPercussion<GeneralSoundStandardPercussion>(
                GeneralSoundUtilities.AsSevenBitNumber,
                GeneralSoundUtilities.GetNoteOnEvent,
                GeneralSoundUtilities.GetNoteOffEvent);
        }

        #endregion

        #region Private methods

        private static void CheckAsSevenBitNumber<TEnum>(TEnum value, SevenBitNumber actualNumber)
            where TEnum : Enum
        {
            ClassicAssert.AreEqual(
                Convert.ToByte(value),
                (byte)actualNumber,
                $"Invalid seven-bit number for '{value}'.");
        }

        private static void CheckNoteEvents<TPercussion>(
            TPercussion percussion,
            Func<TPercussion, SevenBitNumber, FourBitNumber, NoteOnEvent> getNoteOnEvent,
            Func<TPercussion, SevenBitNumber, FourBitNumber, NoteOffEvent> getNoteOffEvent)
            where TPercussion : Enum
        {
            var noteNumber = (SevenBitNumber)Convert.ToByte(percussion);
            var velocity = (SevenBitNumber)70;
            var channel = (FourBitNumber)9;

            var noteOnEvent = getNoteOnEvent(percussion, velocity, channel);
            var noteOffEvent = getNoteOffEvent(percussion, velocity, channel);

            MidiAsserts.AreEqual(
                new NoteOnEvent(noteNumber, velocity) { Channel = channel },
                noteOnEvent,
                true,
                $"Invalid Note On event for '{percussion}'.");
            MidiAsserts.AreEqual(
                new NoteOffEvent(noteNumber, velocity) { Channel = channel },
                noteOffEvent,
                true,
                $"Invalid Note Off event for '{percussion}'.");
        }

        private static void CheckInvalidPercussion<TPercussion>(
            Func<TPercussion, SevenBitNumber> asSevenBitNumber,
            Func<TPercussion, SevenBitNumber, FourBitNumber, NoteOnEvent> getNoteOnEvent,
            Func<TPercussion, SevenBitNumber, FourBitNumber, NoteOffEvent> getNoteOffEvent)
            where TPercussion : Enum
        {
            var percussion = (TPercussion)Enum.ToObject(typeof(TPercussion), InvalidEnumValue);
            var velocity = (SevenBitNumber)70;
            var channel = (FourBitNumber)9;
            var percussionType = typeof(TPercussion).Name;

            ClassicAssert.Throws<InvalidEnumArgumentException>(
                () => asSevenBitNumber(percussion),
                $"Exception is not thrown by AsSevenBitNumber for {percussionType}.");
            ClassicAssert.Throws<InvalidEnumArgumentException>(
                () => getNoteOnEvent(percussion, velocity, channel),
                $"Exception is not thrown by GetNoteOnEvent for {percussionType}.");
            ClassicAssert.Throws<InvalidEnumArgumentException>(
                () => getNoteOffEvent(percussion, velocity, channel),
                $"Exception is not thrown by GetNoteOffEvent for {percussionType}.");
        }

        #endregion
    }
}
