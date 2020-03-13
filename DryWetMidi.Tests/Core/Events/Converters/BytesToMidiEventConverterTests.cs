using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Core
{
    [TestFixture]
    public sealed class BytesToMidiEventConverterTests
    {
        #region Test methods

        [Test]
        public void Convert_StatusByte_DataBytes()
        {
            using (var bytesToMidiEventConverter = new BytesToMidiEventConverter())
            {
                Convert_StatusByte_DataBytes(
                    bytesToMidiEventConverter,
                    0x92,
                    new byte[] { 0x12, 0x56 },
                    new NoteOnEvent((SevenBitNumber)0x12, (SevenBitNumber)0x56)
                    {
                        Channel = (FourBitNumber)0x2
                    });
                Convert_StatusByte_DataBytes(
                    bytesToMidiEventConverter,
                    0xB3,
                    new byte[] { 0x23, 0x7F },
                    new ControlChangeEvent((SevenBitNumber)0x23, (SevenBitNumber)0x7F)
                    {
                        Channel = (FourBitNumber)0x3
                    });
            }
        }

        [Test]
        public void Convert_Bytes()
        {
            using (var bytesToMidiEventConverter = new BytesToMidiEventConverter())
            {
                Convert_Bytes(
                    bytesToMidiEventConverter,
                    new byte[] { 0x92, 0x12, 0x56 },
                    new NoteOnEvent((SevenBitNumber)0x12, (SevenBitNumber)0x56)
                    {
                        Channel = (FourBitNumber)0x2
                    });
                Convert_Bytes(
                    bytesToMidiEventConverter,
                    new byte[] { 0x90, 0x12, 0x00 },
                    new NoteOffEvent((SevenBitNumber)0x12, (SevenBitNumber)0x00)
                    {
                        Channel = (FourBitNumber)0x0
                    });
                Convert_Bytes(
                    bytesToMidiEventConverter,
                    new byte[] { 0xB3, 0x23, 0x7F },
                    new ControlChangeEvent((SevenBitNumber)0x23, (SevenBitNumber)0x7F)
                    {
                        Channel = (FourBitNumber)0x3
                    });
            }
        }

        [Test]
        public void Convert_Bytes_SilentNoteOnAsNoteOn()
        {
            using (var bytesToMidiEventConverter = new BytesToMidiEventConverter())
            {
                bytesToMidiEventConverter.ReadingSettings.SilentNoteOnPolicy = SilentNoteOnPolicy.NoteOn;

                Convert_Bytes(
                    bytesToMidiEventConverter,
                    new byte[] { 0x92, 0x12, 0x00 },
                    new NoteOnEvent((SevenBitNumber)0x12, (SevenBitNumber)0x00)
                    {
                        Channel = (FourBitNumber)0x2
                    });
                Convert_Bytes(
                    bytesToMidiEventConverter,
                    new byte[] { 0xB3, 0x23, 0x7F },
                    new ControlChangeEvent((SevenBitNumber)0x23, (SevenBitNumber)0x7F)
                    {
                        Channel = (FourBitNumber)0x3
                    });
            }
        }

        [Test]
        public void Convert_Bytes_Offset_Length()
        {
            using (var bytesToMidiEventConverter = new BytesToMidiEventConverter())
            {
                var bytes = new byte[] { 0x92, 0x12, 0x56, 0xB3, 0x23, 0x7F };

                Convert_Bytes_Offset_Length(
                    bytesToMidiEventConverter,
                    bytes,
                    0,
                    3,
                    new NoteOnEvent((SevenBitNumber)0x12, (SevenBitNumber)0x56)
                    {
                        Channel = (FourBitNumber)0x2
                    });
                Convert_Bytes_Offset_Length(
                    bytesToMidiEventConverter,
                    bytes,
                    3,
                    3,
                    new ControlChangeEvent((SevenBitNumber)0x23, (SevenBitNumber)0x7F)
                    {
                        Channel = (FourBitNumber)0x3
                    });
            }
        }

        #endregion

        #region Private methods

        private void Convert_StatusByte_DataBytes(BytesToMidiEventConverter bytesToMidiEventConverter, byte statusByte, byte[] dataBytes, MidiEvent expectedMidiEvent)
        {
            var midiEvent = bytesToMidiEventConverter.Convert(statusByte, dataBytes);
            CompareEvents(expectedMidiEvent, midiEvent);
        }

        private void Convert_Bytes(BytesToMidiEventConverter bytesToMidiEventConverter, byte[] bytes, MidiEvent expectedMidiEvent)
        {
            var midiEvent = bytesToMidiEventConverter.Convert(bytes);
            CompareEvents(expectedMidiEvent, midiEvent);
        }

        private void Convert_Bytes_Offset_Length(BytesToMidiEventConverter bytesToMidiEventConverter, byte[] bytes, int offset, int length, MidiEvent expectedMidiEvent)
        {
            var midiEvent = bytesToMidiEventConverter.Convert(bytes, offset, length);
            CompareEvents(expectedMidiEvent, midiEvent);
        }

        private static void CompareEvents(MidiEvent expectedMidiEvent, MidiEvent actualMidiEvent)
        {
            MidiAsserts.AreEventsEqual(
                expectedMidiEvent,
                actualMidiEvent,
                false,
                "MIDI event read incorrectly.");
        }

        #endregion
    }
}
