using System.Linq;
using System.Text;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Core
{
    [TestFixture]
    public sealed class MidiEventWriterTests
    {
        #region Test methods

        [Test]
        public void Write()
        {
            using (var midiEventWriter = new MidiEventWriter())
            {
                Write(
                    midiEventWriter,
                    new NoteOnEvent((SevenBitNumber)0x12, (SevenBitNumber)0x56)
                    {
                        Channel = (FourBitNumber)0x2
                    },
                    new byte[] { 0x92, 0x12, 0x56 });
                Write(
                    midiEventWriter,
                    new NoteOffEvent((SevenBitNumber)0x12, (SevenBitNumber)0x00)
                    {
                        Channel = (FourBitNumber)0x0
                    },
                    new byte[] { 0x80, 0x12, 0x00 });
                Write(
                    midiEventWriter,
                    new ControlChangeEvent((SevenBitNumber)0x23, (SevenBitNumber)0x7F)
                    {
                        Channel = (FourBitNumber)0x3
                    },
                    new byte[] { 0xB3, 0x23, 0x7F });
            }
        }

        [Test]
        public void Write_MinSize()
        {
            using (var midiEventWriter = new MidiEventWriter())
            {
                Write(
                    midiEventWriter,
                    new NoteOnEvent((SevenBitNumber)0x12, (SevenBitNumber)0x56)
                    {
                        Channel = (FourBitNumber)0x2
                    },
                    new byte[] { 0x92, 0x12, 0x56, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
                    10);
            }
        }

        [Test]
        public void Write_Settings()
        {
            using (var midiEventWriter = new MidiEventWriter())
            {
                midiEventWriter.WritingSettings.TextEncoding = Encoding.UTF8;

                var text = "Test▶▶▶";
                var bytes = Encoding.UTF8.GetBytes(text);
                Write(
                    midiEventWriter,
                    new TextEvent(text),
                    new byte[] { 0xFF, 0x01 }.Concat(DataTypesUtilities.GetVlqBytes(bytes.Length)).Concat(bytes).ToArray());
            }
        }

        #endregion

        #region Private methods

        private void Write(MidiEventWriter midiEventWriter, MidiEvent midiEvent, byte[] expectedBytes, int minSize = 0)
        {
            var bytes = midiEventWriter.Write(midiEvent, minSize);
            CollectionAssert.AreEqual(expectedBytes, bytes, $"Event [{midiEvent}] converted to bytes incorrectly.");
        }

        #endregion
    }
}
