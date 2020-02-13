using Melanchall.DryWetMidi.Core;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Utilities
{
    internal static class MidiAsserts
    {
        #region Methods

        public static void AreEventsEqual(MidiEvent midiEvent1, MidiEvent midiEvent2, bool compareDeltaTimes, string message = null)
        {
            string eventsComparingMessage;
            var areEqual = MidiEvent.Equals(
                midiEvent1,
                midiEvent2,
                new MidiEventEqualityCheckSettings { CompareDeltaTimes = compareDeltaTimes },
                out eventsComparingMessage);

            Assert.IsTrue(areEqual, $"{message} {eventsComparingMessage}");
        }

        public static void AreFilesEqual(MidiFile midiFile1, MidiFile midiFile2, bool compareOriginalFormat, string message = null)
        {
            string filesComparingMessage;
            var areEqual = MidiFile.Equals(
                midiFile1,
                midiFile2,
                new MidiFileEqualityCheckSettings { CompareOriginalFormat = compareOriginalFormat },
                out filesComparingMessage);

            Assert.IsTrue(areEqual, $"{message} {filesComparingMessage}");
        }

        public static void AreFilesNotEqual(MidiFile midiFile1, MidiFile midiFile2, bool compareOriginalFormat, string message = null)
        {
            string filesComparingMessage;
            var areEqual = MidiFile.Equals(
                midiFile1,
                midiFile2,
                new MidiFileEqualityCheckSettings { CompareOriginalFormat = compareOriginalFormat },
                out filesComparingMessage);

            Assert.IsFalse(areEqual, $"{message} {filesComparingMessage}");
        }

        #endregion
    }
}
