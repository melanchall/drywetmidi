using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Core
{
    [TestFixture]
    public sealed class MidiEventReaderTests
    {
        #region Test methods

        [Test]
        public void Read_StatusByte_DataBytes()
        {
            using (var midiEventReader = new MidiEventReader())
            {
                Read_StatusByte_DataBytes(
                    midiEventReader,
                    0x92,
                    new byte[] { 0x12, 0x56 },
                    new NoteOnEvent((SevenBitNumber)0x12, (SevenBitNumber)0x56)
                    {
                        Channel = (FourBitNumber)0x2
                    });
                Read_StatusByte_DataBytes(
                    midiEventReader,
                    0xB3,
                    new byte[] { 0x23, 0x7F },
                    new ControlChangeEvent((SevenBitNumber)0x23, (SevenBitNumber)0x7F)
                    {
                        Channel = (FourBitNumber)0x3
                    });
            }
        }

        [Test]
        public void Read_Bytes()
        {
            using (var midiEventReader = new MidiEventReader())
            {
                Read_Bytes(
                    midiEventReader,
                    new byte[] { 0x92, 0x12, 0x56 },
                    new NoteOnEvent((SevenBitNumber)0x12, (SevenBitNumber)0x56)
                    {
                        Channel = (FourBitNumber)0x2
                    });
                Read_Bytes(
                    midiEventReader,
                    new byte[] { 0x90, 0x12, 0x00 },
                    new NoteOffEvent((SevenBitNumber)0x12, (SevenBitNumber)0x00)
                    {
                        Channel = (FourBitNumber)0x0
                    });
                Read_Bytes(
                    midiEventReader,
                    new byte[] { 0xB3, 0x23, 0x7F },
                    new ControlChangeEvent((SevenBitNumber)0x23, (SevenBitNumber)0x7F)
                    {
                        Channel = (FourBitNumber)0x3
                    });
            }
        }

        [Test]
        public void Read_Bytes_SilentNoteOnAsNoteOn()
        {
            using (var midiEventReader = new MidiEventReader())
            {
                midiEventReader.ReadingSettings.SilentNoteOnPolicy = SilentNoteOnPolicy.NoteOn;

                Read_Bytes(
                    midiEventReader,
                    new byte[] { 0x92, 0x12, 0x00 },
                    new NoteOnEvent((SevenBitNumber)0x12, (SevenBitNumber)0x00)
                    {
                        Channel = (FourBitNumber)0x2
                    });
                Read_Bytes(
                    midiEventReader,
                    new byte[] { 0xB3, 0x23, 0x7F },
                    new ControlChangeEvent((SevenBitNumber)0x23, (SevenBitNumber)0x7F)
                    {
                        Channel = (FourBitNumber)0x3
                    });
            }
        }

        [Test]
        public void Read_Bytes_Offset_Length()
        {
            using (var midiEventReader = new MidiEventReader())
            {
                var bytes = new byte[] { 0x92, 0x12, 0x56, 0xB3, 0x23, 0x7F };

                Read_Bytes_Offset_Length(
                    midiEventReader,
                    bytes,
                    0,
                    3,
                    new NoteOnEvent((SevenBitNumber)0x12, (SevenBitNumber)0x56)
                    {
                        Channel = (FourBitNumber)0x2
                    });
                Read_Bytes_Offset_Length(
                    midiEventReader,
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

        private void Read_StatusByte_DataBytes(MidiEventReader midiEventReader, byte statusByte, byte[] dataBytes, MidiEvent expectedMidiEvent)
        {
            var midiEvent = midiEventReader.Read(statusByte, dataBytes);
            CompareEvents(expectedMidiEvent, midiEvent);
        }

        private void Read_Bytes(MidiEventReader midiEventReader, byte[] bytes, MidiEvent expectedMidiEvent)
        {
            var midiEvent = midiEventReader.Read(bytes);
            CompareEvents(expectedMidiEvent, midiEvent);
        }

        private void Read_Bytes_Offset_Length(MidiEventReader midiEventReader, byte[] bytes, int offset, int length, MidiEvent expectedMidiEvent)
        {
            var midiEvent = midiEventReader.Read(bytes, offset, length);
            CompareEvents(expectedMidiEvent, midiEvent);
        }

        private static void CompareEvents(MidiEvent expectedMidiEvent, MidiEvent actualMidiEvent)
        {
            var isMidiEventCorrect = MidiEventEquality.AreEqual(
                expectedMidiEvent,
                actualMidiEvent,
                false);
            Assert.IsTrue(isMidiEventCorrect, "MIDI event read incorrectly.");
        }

        #endregion
    }
}
