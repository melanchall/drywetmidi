using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Utilities
{
    internal static class MidiAsserts
    {
        #region Methods

        private static void AreEqual(Note expectedNote, Note actualNote, string message)
        {
            Assert.AreEqual(expectedNote.NoteNumber, actualNote.NoteNumber, $"{message} Note number is invalid.");
            Assert.AreEqual(expectedNote.Channel, actualNote.Channel, $"{message} Channel is invalid.");
            Assert.AreEqual(expectedNote.Velocity, actualNote.Velocity, $"{message} Velocity is invalid.");
            Assert.AreEqual(expectedNote.OffVelocity, actualNote.OffVelocity, $"{message} Off velocity is invalid.");
            Assert.AreEqual(expectedNote.Time, actualNote.Time, $"{message} Time is invalid.");
            Assert.AreEqual(expectedNote.Length, actualNote.Length, $"{message} Length is invalid.");
        }

        private static void AreEqual(Rest expectedRest, Rest actualRest, string message)
        {
            Assert.AreEqual(expectedRest.NoteNumber, actualRest.NoteNumber, $"{message} Note number is invalid.");
            Assert.AreEqual(expectedRest.Channel, actualRest.Channel, $"{message} Channel is invalid.");
            Assert.AreEqual(expectedRest.Time, actualRest.Time, $"{message} Time is invalid.");
            Assert.AreEqual(expectedRest.Length, actualRest.Length, $"{message} Length is invalid.");
        }

        private static void AreEqual(Chord expectedChord, Chord actualChord, string message)
        {
            AreEqual(expectedChord.Notes, actualChord.Notes, message);
        }

        private static void AreEqual(TimedEvent expectedTimedEvent, TimedEvent actualTimedEvent, bool compareDeltaTimes, long timesEpsilon, string message)
        {
            Assert.IsTrue(Math.Abs(expectedTimedEvent.Time - actualTimedEvent.Time) <= timesEpsilon, $"{message} Time is invalid.");

            string eventsEqualityCheckMessage;
            Assert.IsTrue(
                MidiEvent.Equals(expectedTimedEvent.Event, actualTimedEvent.Event, new MidiEventEqualityCheckSettings { CompareDeltaTimes = compareDeltaTimes }, out eventsEqualityCheckMessage),
                $"{message} {eventsEqualityCheckMessage}");
        }

        public static void AreEqual(
            IEnumerable<ITimedObject> expectedTimedObjects,
            IEnumerable<ITimedObject> actualTimedObjects,
            string message)
        {
            AreEqual(expectedTimedObjects, actualTimedObjects, true, 0, message);
        }

        public static void AreEqual(
            IEnumerable<ITimedObject> expectedTimedObjects,
            IEnumerable<ITimedObject> actualTimedObjects,
            bool compareDeltaTimes,
            long timesEpsilon,
            string message)
        {
            var expectedCount = expectedTimedObjects.Count();
            var actualCount = actualTimedObjects.Count();

            Assert.AreEqual(expectedCount, actualCount, $"{message} Objects count is invalid.");

            var expectedTimedObjectsEnumerator = expectedTimedObjects.GetEnumerator();
            var actualTimedObjectsEnumerator = actualTimedObjects.GetEnumerator();

            var i = 0;

            while (expectedTimedObjectsEnumerator.MoveNext() && actualTimedObjectsEnumerator.MoveNext())
            {
                var expectedChord = expectedTimedObjectsEnumerator.Current;
                var actualChord = actualTimedObjectsEnumerator.Current;

                AreEqual(expectedChord, actualChord, compareDeltaTimes, timesEpsilon, $"{message} Object {i} is invalid.");

                i++;
            }
        }

        public static void AreEqual(
            ITimedObject expectedTimedObject,
            ITimedObject actualTimedObject,
            string message)
        {
            AreEqual(expectedTimedObject, actualTimedObject, true, 0, message);
        }

        public static void AreEqual(
            ITimedObject expectedTimedObject,
            ITimedObject actualTimedObject,
            bool compareDeltaTimes,
            long timesEpsilon,
            string message)
        {
            if (ReferenceEquals(expectedTimedObject, actualTimedObject))
                return;

            if (ReferenceEquals(null, expectedTimedObject) || ReferenceEquals(null, actualTimedObject))
                Assert.Fail($"{message} One of objects is null.");

            var timedEvent = expectedTimedObject as TimedEvent;
            if (timedEvent != null)
            {
                AreEqual(timedEvent, actualTimedObject as TimedEvent, compareDeltaTimes, timesEpsilon, $"{message} Timed event is invalid.");
                return;
            }

            var note = expectedTimedObject as Note;
            if (note != null)
            {
                AreEqual(note, actualTimedObject as Note, $"{message} Note is invalid.");
                return;
            }

            var chord = expectedTimedObject as Chord;
            if (chord != null)
            {
                AreEqual(chord, actualTimedObject as Chord, $"{message} Chord is invalid.");
                return;
            }

            var rest = expectedTimedObject as Rest;
            if (rest != null)
            {
                AreEqual(rest, actualTimedObject as Rest, $"{message} Rest is invalid.");
                return;
            }

            Assert.Inconclusive($"Comparing of {expectedTimedObject} and {actualTimedObject} is not implemented.");
        }

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
