using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using NUnit.Framework;
using System;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed partial class PlaybackTests
    {
        #region Test methods

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddAtAdvanceByOne(
            [Values(1, 2, 3, 4, 8, 16, 17, 32, 50, 51, 64)] int notesCount,
            [Values(0, 10)] int gapMs)
        {
            var noteLengthMs = 20;
            var lastEventTime = notesCount * noteLengthMs + 20 + notesCount * gapMs;

            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, lastEventTime), OnTheFlyChecksTempoMap),
            };

            var objectsToAdd = SevenBitNumber
                .Values
                .Take(notesCount)
                .Select(n => new Note(n)
                    .SetTime(new MetricTimeSpan(0, 0, 0, n * noteLengthMs + 10 + n * gapMs), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, noteLengthMs), OnTheFlyChecksTempoMap))
                .ToArray();

            var actions = SevenBitNumber
                .Values
                .Take(notesCount)
                .Select(n => new PlaybackChanger(n == 0 ? 0 : noteLengthMs,
                    (playback, collection) => collection.Add(objectsToAdd[n])))
                .ToArray();

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: actions,
                expectedReceivedEvents: SevenBitNumber
                    .Values
                    .Take(notesCount)
                    .SelectMany(n => new[]
                    {
                        new ReceivedEvent(new NoteOnEvent(n, Note.DefaultVelocity), TimeSpan.FromMilliseconds(n * noteLengthMs + 10 + n * gapMs)),
                        new ReceivedEvent(new NoteOffEvent(n, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds((n + 1) * noteLengthMs + 10 + n * gapMs)),
                    })
                    .Concat(new[]
                    {
                        new ReceivedEvent(new TextEvent("END"), TimeSpan.FromMilliseconds(lastEventTime)),
                    })
                    .ToArray(),
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddAtAdvanceByOne_WithEndMovesBehindCurrentTime(
            [Values(1, 2, 3, 4, 8, 16, 17, 32, 50, 51, 64)] int notesCount)
        {
            var noteLengthMs = 20;
            var lastEventTime = notesCount * noteLengthMs + 20;

            var endObject = new TimedEvent(new TextEvent("END"))
                .SetTime(new MetricTimeSpan(0, 0, 0, lastEventTime), OnTheFlyChecksTempoMap);
            var initialObjects = new ITimedObject[]
            {
                endObject,
            };

            var objectsToAdd = SevenBitNumber
                .Values
                .Take(notesCount)
                .Select(n => new Note(n)
                    .SetTime(new MetricTimeSpan(0, 0, 0, n * noteLengthMs + 10), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, noteLengthMs), OnTheFlyChecksTempoMap))
                .ToArray();

            var actions = SevenBitNumber
                .Values
                .Take(notesCount)
                .Select(n => new PlaybackChanger(n == 0 ? 0 : noteLengthMs,
                    (playback, collection) => collection.ChangeCollection(() =>
                    {
                        collection.Add(objectsToAdd[n]);
                        collection.ChangeObject(endObject, obj => obj
                            .SetTime(new MetricTimeSpan(0, 0, 0, n * noteLengthMs + 15), OnTheFlyChecksTempoMap));
                    })))
                .ToArray();

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: actions,
                expectedReceivedEvents: SevenBitNumber
                    .Values
                    .Take(notesCount)
                    .SelectMany(n => new[]
                    {
                        new ReceivedEvent(new NoteOnEvent(n, Note.DefaultVelocity), TimeSpan.FromMilliseconds(n * noteLengthMs + 10)),
                        new ReceivedEvent(new TextEvent("END"), TimeSpan.FromMilliseconds((n + 1) * noteLengthMs + 5)),
                        new ReceivedEvent(new NoteOffEvent(n, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds((n + 1) * noteLengthMs + 10)),
                    })
                    .ToArray(),
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddAtAdvanceByOneWithOverlapping(
            [Values(1, 2, 3, 4, 8, 16, 17, 32, 50, 51, 64)] int notesCount)
        {
            var overlappedMs = 5;
            var noteLengthMs = 20;
            var lastEventTime = notesCount * noteLengthMs + 20 - notesCount * overlappedMs;

            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, lastEventTime), OnTheFlyChecksTempoMap),
            };

            var objectsToAdd = SevenBitNumber
                .Values
                .Take(notesCount)
                .Select(n => new Note(n)
                    .SetTime(new MetricTimeSpan(0, 0, 0, n * noteLengthMs + 10 - n * overlappedMs), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, noteLengthMs), OnTheFlyChecksTempoMap))
                .ToArray();

            var actions = SevenBitNumber
                .Values
                .Take(notesCount)
                .Select(n => new PlaybackChanger(n == 0 ? 0 : noteLengthMs - n * overlappedMs,
                    (playback, collection) => collection.Add(objectsToAdd[n])))
                .ToArray();

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: actions,
                expectedReceivedEvents: SevenBitNumber
                    .Values
                    .Take(notesCount)
                    .SelectMany(n => new[]
                    {
                        new ReceivedEvent(new NoteOnEvent(n, Note.DefaultVelocity), TimeSpan.FromMilliseconds(n * noteLengthMs + 10 - n * overlappedMs)),
                        new ReceivedEvent(new NoteOffEvent(n, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds((n + 1) * noteLengthMs + 10 - n * overlappedMs)),
                    })
                    .OrderBy(e => e.Time)
                    .Concat(new[]
                    {
                        new ReceivedEvent(new TextEvent("END"), TimeSpan.FromMilliseconds(lastEventTime)),
                    })
                    .ToArray(),
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_BatchAdd_1(
            [Values(1, 2, 3, 4, 8, 16, 17, 32, 50, 51, 64)] int notesCount)
        {
            var noteLengthMs = 20;
            var lastEventTime = notesCount * noteLengthMs + 20;

            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, lastEventTime), OnTheFlyChecksTempoMap),
            };

            var objectsToAdd = SevenBitNumber
                .Values
                .Take(notesCount)
                .Select(n => new Note(n)
                    .SetTime(new MetricTimeSpan(0, 0, 0, n * noteLengthMs + 10), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, noteLengthMs), OnTheFlyChecksTempoMap))
                .ToArray();

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(0,
                        (playback, collection) => collection.Add(objectsToAdd)),
                },
                expectedReceivedEvents: SevenBitNumber
                    .Values
                    .Take(notesCount)
                    .SelectMany(n => new[]
                    {
                        new ReceivedEvent(new NoteOnEvent(n, Note.DefaultVelocity), TimeSpan.FromMilliseconds(n * noteLengthMs + 10)),
                        new ReceivedEvent(new NoteOffEvent(n, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds((n + 1) * noteLengthMs + 10)),
                    })
                    .Concat(new[]
                    {
                        new ReceivedEvent(new TextEvent("END"), TimeSpan.FromMilliseconds(lastEventTime)),
                    })
                    .ToArray(),
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_BatchAdd_2(
            [Values(8, 16, 17, 20, 32)] int notesCount,
            [Values(20, 40, 60, 100)] int addAtMs)
        {
            var noteLengthMs = 20;
            var lastEventTime = notesCount * noteLengthMs + 20;

            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, lastEventTime), OnTheFlyChecksTempoMap),
            };

            var objectsToAdd = SevenBitNumber
                .Values
                .Take(notesCount)
                .Select(n => new Note(n)
                    .SetTime(new MetricTimeSpan(0, 0, 0, n * noteLengthMs + 10), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, noteLengthMs), OnTheFlyChecksTempoMap))
                .ToArray();

            var receivedEvents = SevenBitNumber
                .Values
                .Take(notesCount)
                .SelectMany(n => new[]
                {
                    new ReceivedEvent(new NoteOnEvent(n, Note.DefaultVelocity), TimeSpan.FromMilliseconds(n * noteLengthMs + 10)),
                    new ReceivedEvent(new NoteOffEvent(n, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds((n + 1) * noteLengthMs + 10)),
                })
                .ToArray();

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(addAtMs,
                        (playback, collection) => collection.Add(objectsToAdd)),
                },
                expectedReceivedEvents:
                    new[]
                    {
                        new ReceivedEvent(
                            objectsToAdd.AtTime(new MetricTimeSpan(0, 0, 0, addAtMs), OnTheFlyChecksTempoMap, LengthedObjectPart.Entire).First().GetTimedNoteOnEvent().Event,
                            TimeSpan.FromMilliseconds(addAtMs)),
                    }
                    .Concat(receivedEvents
                        .SkipWhile(e => e.Time < TimeSpan.FromMilliseconds(addAtMs))
                        .Concat(new[]
                        {
                            new ReceivedEvent(new TextEvent("END"), TimeSpan.FromMilliseconds(lastEventTime)),
                        }))
                    .ToArray(),
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_RemoveByOne(
            [Values(1, 2, 3, 4, 8, 16, 17, 32, 50, 51, 64)] int notesCount)
        {
            var noteLengthMs = 20;

            var objectsToRemove = SevenBitNumber
                .Values
                .Take(notesCount)
                .Select(n => new Note(n)
                    .SetTime(new MetricTimeSpan(0, 0, 0, n * noteLengthMs + 10), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, noteLengthMs), OnTheFlyChecksTempoMap))
                .ToArray();

            var actions = SevenBitNumber
                .Values
                .Take(notesCount)
                .Select(n => new PlaybackChanger(noteLengthMs,
                    (playback, collection) => collection.Remove(objectsToRemove[n])))
                .ToArray();

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: objectsToRemove,
                actions: actions,
                expectedReceivedEvents: SevenBitNumber
                    .Values
                    .Take(notesCount)
                    .SelectMany(n => new[]
                    {
                        new ReceivedEvent(new NoteOnEvent(n, Note.DefaultVelocity), TimeSpan.FromMilliseconds(n * noteLengthMs + 10)),
                        new ReceivedEvent(new NoteOffEvent(n, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(n * noteLengthMs + 20)),
                    })
                    .ToArray(),
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_RemoveAtAdvance(
            [Values(1, 2, 3, 4, 8, 16, 17, 32, 50, 51, 64)] int notesCount)
        {
            var noteLengthMs = 40;

            var objectsToRemove = SevenBitNumber
                .Values
                .Take(notesCount)
                .Select(n => new Note(n)
                    .SetTime(new MetricTimeSpan(0, 0, 0, n * noteLengthMs + 40), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, noteLengthMs), OnTheFlyChecksTempoMap))
                .ToArray();

            var actions = SevenBitNumber
                .Values
                .Take(notesCount)
                .Select(n => new PlaybackChanger(n == 0 ? 20 : noteLengthMs,
                    (playback, collection) => collection.Remove(objectsToRemove[n])))
                .ToArray();

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: objectsToRemove,
                actions: actions,
                expectedReceivedEvents: Array.Empty<ReceivedEvent>(),
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_ShiftNoteAtAdvance()
        {
            var noteLengthMs = 40;
            var noteNumber = (SevenBitNumber)70;

            var objectToShift = new Note(noteNumber)
                .SetTime(new MetricTimeSpan(0, 0, 0, 20), OnTheFlyChecksTempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, noteLengthMs), OnTheFlyChecksTempoMap);

            var actions = Enumerable
                .Range(0, 10)
                .Select(n => new PlaybackChanger(n == 0 ? 10 : noteLengthMs,
                    (playback, collection) => collection.ChangeObject(objectToShift, obj => obj
                        .SetTime(new MetricTimeSpan(0, 0, 0, (n + 1) * noteLengthMs + 20), OnTheFlyChecksTempoMap))))
                .ToArray();

            var lastMs = 10 * noteLengthMs + 20;

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: new[] { objectToShift },
                actions: actions,
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(noteNumber, Note.DefaultVelocity), TimeSpan.FromMilliseconds(lastMs)),
                    new ReceivedEvent(new NoteOffEvent(noteNumber, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(lastMs + noteLengthMs)),
                },
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddAndRemoveAtAdvanceByOne(
            [Values(1, 2, 3, 4, 8, 16, 17, 32, 50, 51, 64)] int notesCount,
            [Values(0, 10)] int gapMs,
            [Values] bool viaChangeCollection)
        {
            var noteLengthMs = 20;
            var lastEventTime = notesCount * noteLengthMs + 20 + notesCount * gapMs;

            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, lastEventTime), OnTheFlyChecksTempoMap),
            };

            var objectsToAdd = SevenBitNumber
                .Values
                .Take(notesCount)
                .Select(n => new Note(n)
                    .SetTime(new MetricTimeSpan(0, 0, 0, n * noteLengthMs + 10 + n * gapMs), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, noteLengthMs), OnTheFlyChecksTempoMap))
                .ToArray();

            var action = viaChangeCollection
                ? new Action<Playback, ObservableTimedObjectsCollection, int>((playback, collection, n) => collection.ChangeCollection(() =>
                {
                    collection.Add(objectsToAdd[n]);
                    collection.Remove(objectsToAdd[n]);
                }))
                : (playback, collection, n) =>
                {
                    collection.Add(objectsToAdd[n]);
                    collection.Remove(objectsToAdd[n]);
                };

            var actions = SevenBitNumber
                .Values
                .Take(notesCount)
                .Select(n => new PlaybackChanger(n == 0 ? 0 : noteLengthMs,
                    (playback, collection) => action(playback, collection, n)))
                .ToArray();

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: actions,
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new TextEvent("END"), TimeSpan.FromMilliseconds(lastEventTime)),
                },
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddAtAdvanceAndRemovePastByOne(
            [Values(1, 2, 3, 4, 8, 16, 17, 32, 50, 51, 64)] int notesCount)
        {
            var noteLengthMs = 20;
            var lastEventTime = notesCount * noteLengthMs + 20;

            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, lastEventTime), OnTheFlyChecksTempoMap),
            };

            var objectsToAdd = SevenBitNumber
                .Values
                .Take(notesCount)
                .Select(n => new Note(n)
                    .SetTime(new MetricTimeSpan(0, 0, 0, n * noteLengthMs + 10), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, noteLengthMs), OnTheFlyChecksTempoMap))
                .ToArray();

            var actions = SevenBitNumber
                .Values
                .Take(notesCount)
                .SelectMany(n => new[]
                {
                    new PlaybackChanger(n == 0 ? 0 : noteLengthMs,
                        (playback, collection) =>
                        {
                            collection.Add(objectsToAdd[n]);

                            if (n >= 2)
                                collection.Remove(objectsToAdd[n - 2]);
                        }),
                })
                .ToArray();

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: actions,
                expectedReceivedEvents: SevenBitNumber
                    .Values
                    .Take(notesCount)
                    .SelectMany(n => new[]
                    {
                        new ReceivedEvent(new NoteOnEvent(n, Note.DefaultVelocity), TimeSpan.FromMilliseconds(n * noteLengthMs + 10)),
                        new ReceivedEvent(new NoteOffEvent(n, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds((n + 1) * noteLengthMs + 10)),
                    })
                    .Concat(new[]
                    {
                        new ReceivedEvent(new TextEvent("END"), TimeSpan.FromMilliseconds(lastEventTime)),
                    })
                    .ToArray(),
                setupPlayback: playback => playback.TrackNotes = true);
        }

        #endregion
    }
}
