using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Utilities
{
    internal static class MidiAsserts
    {
        #region Methods

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
            // TODO: rework on enumerators directly
            var expectedCount = expectedTimedObjects.Count();
            var actualCount = actualTimedObjects.Count();

            ClassicAssert.AreEqual(expectedCount, actualCount, $"{message} Objects count is invalid.");

            var expectedTimedObjectsEnumerator = expectedTimedObjects.GetEnumerator();
            var actualTimedObjectsEnumerator = actualTimedObjects.GetEnumerator();

            var i = 0;

            while (expectedTimedObjectsEnumerator.MoveNext() && actualTimedObjectsEnumerator.MoveNext())
            {
                var expectedTimedObject = expectedTimedObjectsEnumerator.Current;
                var actualTimedObject = actualTimedObjectsEnumerator.Current;

                AreEqual(expectedTimedObject, actualTimedObject, compareDeltaTimes, timesEpsilon, $"{message} Object {i} is invalid.");

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
                ClassicAssert.Fail($"{message} One of objects is null.");

            ClassicAssert.AreEqual(expectedTimedObject.GetType(), actualTimedObject.GetType(), $"{message} Different types.");

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

            ClassicAssert.Inconclusive($"Comparing of {expectedTimedObject} and {actualTimedObject} is not implemented.");
        }

        public static void AreEqual(EventsCollection eventsCollection1, EventsCollection eventsCollection2, bool compareDeltaTimes, string message = null)
        {
            var areEqual = EventsCollectionEquality.Equals(
                eventsCollection1,
                eventsCollection2,
                new MidiEventEqualityCheckSettings { CompareDeltaTimes = compareDeltaTimes },
                out var eventsComparingMessage);

            ClassicAssert.IsTrue(areEqual, $"{message} {eventsComparingMessage}");
        }

        public static void AreEqual(MidiChunk midiChunk1, MidiChunk midiChunk2, bool compareDeltaTimes, string message = null)
        {
            var areEqual = MidiChunkEquality.Equals(
                midiChunk1,
                midiChunk2,
                new MidiChunkEqualityCheckSettings
                {
                    EventEqualityCheckSettings = new MidiEventEqualityCheckSettings
                    {
                        CompareDeltaTimes = compareDeltaTimes
                    }
                },
                out var chunksComparingMessage);

            ClassicAssert.IsTrue(areEqual, $"{message} {chunksComparingMessage}");
        }

        public static void AreEqual(IEnumerable<MidiChunk> chunks1, IEnumerable<MidiChunk> chunks2, bool compareDeltaTimes, string message = null)
        {
            var chunksEnumerator1 = chunks1.GetEnumerator();
            var chunksEnumerator2 = chunks2.GetEnumerator();

            var i = 0;
            var chunksEnumerated1 = false;
            var chunksEnumerated2 = false;

            while (true)
            {
                chunksEnumerated1 = !chunksEnumerator1.MoveNext();
                chunksEnumerated2 = !chunksEnumerator2.MoveNext();
                if (chunksEnumerated1 || chunksEnumerated2)
                    break;

                string chunksComparingMessage;
                var areEqual = MidiChunkEquality.Equals(
                    chunksEnumerator1.Current,
                    chunksEnumerator2.Current,
                    new MidiChunkEqualityCheckSettings
                    {
                        EventEqualityCheckSettings = new MidiEventEqualityCheckSettings
                        {
                            CompareDeltaTimes = compareDeltaTimes
                        }
                    },
                    out chunksComparingMessage);

                ClassicAssert.IsTrue(areEqual, $"{message} Chunk {i} is invalid. {chunksComparingMessage}");

                i++;
            }

            ClassicAssert.IsTrue(chunksEnumerated1 && chunksEnumerated2, $"{message} Chunks collections have different length.");
        }

        public static void AreEqual(MidiFile midiFile1, MidiFile midiFile2, bool compareOriginalFormat, string message = null)
        {
            string filesComparingMessage;
            var areEqual = MidiFile.Equals(
                midiFile1,
                midiFile2,
                new MidiFileEqualityCheckSettings { CompareOriginalFormat = compareOriginalFormat },
                out filesComparingMessage);

            ClassicAssert.IsTrue(areEqual, $"{message} {filesComparingMessage}");
        }

        public static void AreEqual(IEnumerable<MidiFile> expectedFiles, IEnumerable<MidiFile> actualFiles, bool compareOriginalFormat, string message = null)
        {
            var expectedFilesEnumerator = expectedFiles.GetEnumerator();
            var actualFilesEnumerator = actualFiles.GetEnumerator();

            var i = 0;
            var expectedFilesEnumerated = false;
            var actualFilesEnumerated = false;

            while (true)
            {
                expectedFilesEnumerated = !expectedFilesEnumerator.MoveNext();
                actualFilesEnumerated = !actualFilesEnumerator.MoveNext();
                if (expectedFilesEnumerated || actualFilesEnumerated)
                    break;

                string filesComparingMessage;
                var areEqual = MidiFile.Equals(
                    expectedFilesEnumerator.Current,
                    actualFilesEnumerator.Current,
                    new MidiFileEqualityCheckSettings { CompareOriginalFormat = compareOriginalFormat },
                    out filesComparingMessage);

                ClassicAssert.IsTrue(areEqual, $"{message} File {i} is invalid. {filesComparingMessage}");

                i++;
            }

            ClassicAssert.IsTrue(expectedFilesEnumerated && actualFilesEnumerated, $"{message} Files collections have different length.");
        }

        public static void AreNotEqual(MidiFile midiFile1, MidiFile midiFile2, bool compareOriginalFormat, string message = null)
        {
            string filesComparingMessage;
            var areEqual = MidiFile.Equals(
                midiFile1,
                midiFile2,
                new MidiFileEqualityCheckSettings { CompareOriginalFormat = compareOriginalFormat },
                out filesComparingMessage);

            ClassicAssert.IsFalse(areEqual, $"{message} {filesComparingMessage}");
        }

        public static void AreEqual(MidiEvent midiEvent1, MidiEvent midiEvent2, bool compareDeltaTimes, string message = null)
        {
            string eventsComparingMessage;
            var areEqual = MidiEvent.Equals(
                midiEvent1,
                midiEvent2,
                new MidiEventEqualityCheckSettings { CompareDeltaTimes = compareDeltaTimes },
                out eventsComparingMessage);
            ClassicAssert.IsTrue(areEqual, $"{message} {eventsComparingMessage}");
        }

        public static void AreEqual(ICollection<MidiEvent> expectedEvents, ICollection<MidiEvent> actualEvents, bool compareDeltaTimes, string message = null)
        {
            ClassicAssert.AreEqual(expectedEvents.Count, actualEvents.Count, $"{message} Events count is invalid.");

            var expectedEventsArray = expectedEvents.ToArray();
            var actualEventsArray = actualEvents.ToArray();

            for (var i = 0; i < expectedEventsArray.Length; i++)
            {
                var expectedEvent = expectedEventsArray[i];
                var actualEvent = actualEventsArray[i];

                AreEqual(expectedEvent, actualEvent, compareDeltaTimes, $"{message} Invalid event {i}.");
            }
        }

        public static void AreEqual(TempoMap expectedTempoMap, TempoMap actualTempoMap, string message)
        {
            if (ReferenceEquals(expectedTempoMap, actualTempoMap))
                return;

            if (ReferenceEquals(null, expectedTempoMap) || ReferenceEquals(null, actualTempoMap))
                ClassicAssert.Fail($"{message} One of objects is null.");

            ClassicAssert.AreEqual(expectedTempoMap.TimeDivision, actualTempoMap.TimeDivision, $"{message} Invalid time division.");
            CollectionAssert.AreEqual(
                expectedTempoMap.GetTempoChanges(),
                actualTempoMap.GetTempoChanges(),
                $"{message} Invalid tempo changes.");
            CollectionAssert.AreEqual(
                expectedTempoMap.GetTimeSignatureChanges(),
                actualTempoMap.GetTimeSignatureChanges(),
                $"{message} Invalid time signature changes.");
        }

        private static void AreEqual(Note expectedNote, Note actualNote, string message)
        {
            ClassicAssert.AreEqual(expectedNote.NoteNumber, actualNote.NoteNumber, $"{message} Note number is invalid.");
            ClassicAssert.AreEqual(expectedNote.Channel, actualNote.Channel, $"{message} Channel is invalid.");
            ClassicAssert.AreEqual(expectedNote.Velocity, actualNote.Velocity, $"{message} Velocity is invalid.");
            ClassicAssert.AreEqual(expectedNote.OffVelocity, actualNote.OffVelocity, $"{message} Off velocity is invalid.");
            ClassicAssert.AreEqual(expectedNote.Time, actualNote.Time, $"{message} Time is invalid.");
            ClassicAssert.AreEqual(expectedNote.Length, actualNote.Length, $"{message} Length is invalid.");

            if (expectedNote.GetType() != typeof(Note))
            {
                ClassicAssert.AreEqual(expectedNote.GetType(), actualNote.GetType(), $"{message} Different types (custom comparison).");
                ClassicAssert.IsTrue(expectedNote.Equals(actualNote), $"{message} Custom comparison failed.");
            }
        }

        private static void AreEqual(Rest expectedRest, Rest actualRest, string message)
        {
            ClassicAssert.AreEqual(expectedRest.Key, actualRest.Key, $"{message} Key is invalid.");
            ClassicAssert.AreEqual(expectedRest.Time, actualRest.Time, $"{message} Time is invalid.");
            ClassicAssert.AreEqual(expectedRest.Length, actualRest.Length, $"{message} Length is invalid.");
        }

        private static void AreEqual(Chord expectedChord, Chord actualChord, string message)
        {
            AreEqual(expectedChord.Notes, actualChord.Notes, $"{message} Notes are invalid.");

            if (expectedChord.GetType() != typeof(Chord))
            {
                ClassicAssert.AreEqual(expectedChord.GetType(), actualChord.GetType(), $"{message} Different types (custom comparison).");
                ClassicAssert.IsTrue(expectedChord.Equals(actualChord), $"{message} Custom comparison failed.");
            }
        }

        private static void AreEqual(TimedEvent expectedTimedEvent, TimedEvent actualTimedEvent, bool compareDeltaTimes, long timesEpsilon, string message)
        {
            ClassicAssert.IsTrue(
                Math.Abs(expectedTimedEvent.Time - actualTimedEvent.Time) <= timesEpsilon,
                $"{message} Time is invalid ({expectedTimedEvent.Time}+-{timesEpsilon} expected, but was {actualTimedEvent.Time}).");

            string eventsEqualityCheckMessage;
            ClassicAssert.IsTrue(
                MidiEvent.Equals(expectedTimedEvent.Event, actualTimedEvent.Event, new MidiEventEqualityCheckSettings { CompareDeltaTimes = compareDeltaTimes }, out eventsEqualityCheckMessage),
                $"{message} {eventsEqualityCheckMessage}");

            if (expectedTimedEvent.GetType() != typeof(TimedEvent))
            {
                ClassicAssert.AreEqual(expectedTimedEvent.GetType(), actualTimedEvent.GetType(), $"{message} Different types (custom comparison).");
                ClassicAssert.IsTrue(expectedTimedEvent.Equals(actualTimedEvent), $"{message} Custom comparison failed.");
            }
        }

        #endregion
    }
}
