using System.Collections.Generic;
using Melanchall.DryWetMidi.Core;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Utilities
{
    internal static class MidiAsserts
    {
        #region Methods

        public static void AreEqual(EventsCollection eventsCollection1, EventsCollection eventsCollection2, bool compareDeltaTimes, string message = null)
        {
            var areEqual = EventsCollectionEquality.Equals(
                eventsCollection1,
                eventsCollection2,
                new MidiEventEqualityCheckSettings { CompareDeltaTimes = compareDeltaTimes },
                out var eventsComparingMessage);

            Assert.IsTrue(areEqual, $"{message} {eventsComparingMessage}");
        }

        public static void AreEqual(TrackChunk trackChunk1, TrackChunk trackChunk2, bool compareDeltaTimes, string message = null)
        {
            var areEqual = MidiChunkEquality.Equals(
                trackChunk1,
                trackChunk2,
                new MidiChunkEqualityCheckSettings
                {
                    EventEqualityCheckSettings = new MidiEventEqualityCheckSettings
                    {
                        CompareDeltaTimes = compareDeltaTimes
                    }
                },
                out var chunksComparingMessage);

            Assert.IsTrue(areEqual, $"{message} {chunksComparingMessage}");
        }

        public static void AreEqual(IEnumerable<TrackChunk> trackChunks1, IEnumerable<TrackChunk> trackChunks2, bool compareDeltaTimes, string message = null)
        {
            var trackChunksEnumerator1 = trackChunks1.GetEnumerator();
            var trackChunksEnumerator2 = trackChunks2.GetEnumerator();

            while (true)
            {
                var trackChunksEnumerated1 = !trackChunksEnumerator1.MoveNext();
                var trackChunksEnumerated2 = !trackChunksEnumerator2.MoveNext();
                if (trackChunksEnumerated1 || trackChunksEnumerated2)
                    break;

                string chunksComparingMessage;
                var areEqual = MidiChunkEquality.Equals(
                    trackChunksEnumerator1.Current,
                    trackChunksEnumerator2.Current,
                    new MidiChunkEqualityCheckSettings
                    {
                        EventEqualityCheckSettings = new MidiEventEqualityCheckSettings
                        {
                            CompareDeltaTimes = compareDeltaTimes
                        }
                    },
                    out chunksComparingMessage);

                Assert.IsTrue(areEqual, $"{message} {chunksComparingMessage}");
            }

            Assert.IsTrue(trackChunksEnumerator1.Current == null && trackChunksEnumerator2.Current == null, $"{message} Chunks collections have different length.");
        }

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
