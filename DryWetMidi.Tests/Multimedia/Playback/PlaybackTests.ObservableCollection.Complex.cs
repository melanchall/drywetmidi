using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using System;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed partial class PlaybackTests
    {
        #region Constants

        private const int NoteLengthMs = 80;

        #endregion

        #region Test methods

        [MultimediaTestRetry]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddAtAdvanceByOne(
            [Values(1, 2, 3, 4, 8, 16, 17, 32, 50, 51)] int notesCount,
            [Values(0, 10)] int gapMs)
        {
            var lastEventTime = notesCount * NoteLengthMs + NoteLengthMs + notesCount * gapMs;

            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, lastEventTime), TempoMap),
            };

            var objectsToAdd = SevenBitNumber
                .Values
                .Take(notesCount)
                .Select(n => new Note(n)
                    .SetTime(new MetricTimeSpan(0, 0, 0, n * NoteLengthMs + NoteLengthMs / 2 + n * gapMs), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, NoteLengthMs), TempoMap))
                .ToArray();

            var actions = SevenBitNumber
                .Values
                .Take(notesCount)
                .Select(n => new DynamicPlaybackAction(
                    n == 0 ? 0 : NoteLengthMs + gapMs,
                    (playback, collection) =>
                    {
                        collection.Add(objectsToAdd[n]);
                        CheckDuration(TimeSpan.FromMilliseconds(lastEventTime), playback);
                    }))
                .ToArray();

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: actions,
                expectedReceivedEvents: SevenBitNumber
                    .Values
                    .Take(notesCount)
                    .SelectMany(n => new[]
                    {
                        new TimestampedEvent(new NoteOnEvent(n, Note.DefaultVelocity), TimeSpan.FromMilliseconds(n * NoteLengthMs + NoteLengthMs / 2 + n * gapMs)),
                        new TimestampedEvent(new NoteOffEvent(n, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds((n + 1) * NoteLengthMs + NoteLengthMs / 2 + n * gapMs)),
                    })
                    .Concat(new[]
                    {
                        new TimestampedEvent(new TextEvent("END"), TimeSpan.FromMilliseconds(lastEventTime)),
                    })
                    .ToArray(),
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [MultimediaTestRetry]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddAtAdvanceByOne_WithEndMovesBehindCurrentTime(
            [Values(1, 2, 3, 4, 8, 16, 17, 32, 50, 51)] int notesCount)
        {
            var lastEventTime = notesCount * NoteLengthMs + NoteLengthMs;

            var endObject = new TimedEvent(new TextEvent("END"))
                .SetTime(new MetricTimeSpan(0, 0, 0, lastEventTime), TempoMap);
            var initialObjects = new ITimedObject[]
            {
                endObject,
            };

            var objectsToAdd = SevenBitNumber
                .Values
                .Take(notesCount)
                .Select(n => new Note(n)
                    .SetTime(new MetricTimeSpan(0, 0, 0, n * NoteLengthMs + NoteLengthMs / 2), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, NoteLengthMs), TempoMap))
                .ToArray();

            var actions = SevenBitNumber
                .Values
                .Take(notesCount)
                .Select(n => new DynamicPlaybackAction(
                    n == 0 ? 0 : NoteLengthMs,
                    (playback, collection) =>
                    {
                        collection.ChangeCollection(() =>
                        {
                            collection.Add(objectsToAdd[n]);
                            collection.ChangeObject(
                                endObject,
                                obj => obj
                                    .SetTime(new MetricTimeSpan(0, 0, 0, n * NoteLengthMs + NoteLengthMs / 2 + NoteLengthMs / 4), TempoMap));
                        });
                        CheckDuration(TimeSpan.FromMilliseconds(n * NoteLengthMs + NoteLengthMs / 2 + NoteLengthMs), playback);
                    }))
                .ToArray();

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: actions,
                expectedReceivedEvents: SevenBitNumber
                    .Values
                    .Take(notesCount)
                    .SelectMany(n => new[]
                    {
                        new TimestampedEvent(new NoteOnEvent(n, Note.DefaultVelocity), TimeSpan.FromMilliseconds(n * NoteLengthMs + NoteLengthMs / 2)),
                        new TimestampedEvent(new TextEvent("END"), TimeSpan.FromMilliseconds(n * NoteLengthMs + NoteLengthMs / 2 + NoteLengthMs / 4)),
                        new TimestampedEvent(new NoteOffEvent(n, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds((n + 1) * NoteLengthMs + NoteLengthMs / 2)),
                    })
                    .ToArray(),
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [MultimediaTestRetry]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddAtAdvanceByOneWithOverlapping(
            [Values(1, 2, 3, 4, 8, 16, 17, 32, 50, 51)] int notesCount)
        {
            var overlappedMs = NoteLengthMs / 8;
            var lastEventTime = notesCount * NoteLengthMs + NoteLengthMs - notesCount * overlappedMs;

            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, lastEventTime), TempoMap),
            };

            var objectsToAdd = SevenBitNumber
                .Values
                .Take(notesCount)
                .Select(n => new Note(n)
                    .SetTime(new MetricTimeSpan(0, 0, 0, n * NoteLengthMs + NoteLengthMs / 2 - n * overlappedMs), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, NoteLengthMs), TempoMap))
                .ToArray();

            var actions = SevenBitNumber
                .Values
                .Take(notesCount)
                .Select(n => new DynamicPlaybackAction(
                    n == 0 ? 0 : NoteLengthMs - n * overlappedMs,
                    (playback, collection) =>
                    {
                        collection.Add(objectsToAdd[n]);
                        CheckDuration(TimeSpan.FromMilliseconds(lastEventTime), playback);
                    }))
                .ToArray();

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: actions,
                expectedReceivedEvents: SevenBitNumber
                    .Values
                    .Take(notesCount)
                    .SelectMany(n => new[]
                    {
                        new TimestampedEvent(new NoteOnEvent(n, Note.DefaultVelocity), TimeSpan.FromMilliseconds(n * NoteLengthMs + NoteLengthMs / 2 - n * overlappedMs)),
                        new TimestampedEvent(new NoteOffEvent(n, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds((n + 1) * NoteLengthMs + NoteLengthMs / 2 - n * overlappedMs)),
                    })
                    .OrderBy(e => e.Time)
                    .Concat(new[]
                    {
                        new TimestampedEvent(new TextEvent("END"), TimeSpan.FromMilliseconds(lastEventTime)),
                    })
                    .ToArray(),
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [MultimediaTestRetry]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_BatchAdd_1(
            [Values(1, 2, 3, 4, 8, 16, 17, 32, 50, 51)] int notesCount)
        {
            var lastEventTime = notesCount * NoteLengthMs + NoteLengthMs;

            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, lastEventTime), TempoMap),
            };

            var objectsToAdd = SevenBitNumber
                .Values
                .Take(notesCount)
                .Select(n => new Note(n)
                    .SetTime(new MetricTimeSpan(0, 0, 0, n * NoteLengthMs + NoteLengthMs / 2), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, NoteLengthMs), TempoMap))
                .ToArray();

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(0, (playback, collection) =>
                    {
                        collection.Add(objectsToAdd);
                        CheckDuration(TimeSpan.FromMilliseconds(lastEventTime), playback);
                    }),
                },
                expectedReceivedEvents: SevenBitNumber
                    .Values
                    .Take(notesCount)
                    .SelectMany(n => new[]
                    {
                        new TimestampedEvent(new NoteOnEvent(n, Note.DefaultVelocity), TimeSpan.FromMilliseconds(n * NoteLengthMs + NoteLengthMs / 2)),
                        new TimestampedEvent(new NoteOffEvent(n, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds((n + 1) * NoteLengthMs + NoteLengthMs / 2)),
                    })
                    .Concat(new[]
                    {
                        new TimestampedEvent(new TextEvent("END"), TimeSpan.FromMilliseconds(lastEventTime)),
                    })
                    .ToArray(),
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [MultimediaTestRetry]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_BatchAdd_2(
            [Values(8, 16, 17, 20, 32)] int notesCount,
            [Values(NoteLengthMs, NoteLengthMs * 2, NoteLengthMs * 3)] int addAtMs)
        {
            var lastEventTime = notesCount * NoteLengthMs + NoteLengthMs;

            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, lastEventTime), TempoMap),
            };

            var objectsToAdd = SevenBitNumber
                .Values
                .Take(notesCount)
                .Select(n => new Note(n)
                    .SetTime(new MetricTimeSpan(0, 0, 0, n * NoteLengthMs + NoteLengthMs / 2), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, NoteLengthMs), TempoMap))
                .ToArray();

            var receivedEvents = SevenBitNumber
                .Values
                .Take(notesCount)
                .SelectMany(n => new[]
                {
                    new TimestampedEvent(new NoteOnEvent(n, Note.DefaultVelocity), TimeSpan.FromMilliseconds(n * NoteLengthMs + NoteLengthMs / 2)),
                    new TimestampedEvent(new NoteOffEvent(n, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds((n + 1) * NoteLengthMs + NoteLengthMs / 2)),
                })
                .ToArray();

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(addAtMs, (playback, collection) =>
                    {
                        collection.Add(objectsToAdd);
                        CheckDuration(TimeSpan.FromMilliseconds(lastEventTime), playback);
                    }),
                },
                expectedReceivedEvents:
                    new[]
                    {
                        new TimestampedEvent(
                            objectsToAdd.AtTime(new MetricTimeSpan(0, 0, 0, addAtMs), TempoMap).First().GetTimedNoteOnEvent().Event,
                            TimeSpan.FromMilliseconds(addAtMs)),
                    }
                    .Concat(receivedEvents
                        .SkipWhile(e => e.Time < TimeSpan.FromMilliseconds(addAtMs))
                        .Concat(new[]
                        {
                            new TimestampedEvent(new TextEvent("END"), TimeSpan.FromMilliseconds(lastEventTime)),
                        }))
                    .ToArray(),
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [MultimediaTestRetry]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_RemoveByOne(
            [Values(1, 2, 3, 4, 8, 16, 17, 32, 50, 51)] int notesCount)
        {
            var objectsToRemove = SevenBitNumber
                .Values
                .Take(notesCount)
                .Select(n => new Note(n)
                    .SetTime(new MetricTimeSpan(0, 0, 0, n * NoteLengthMs + NoteLengthMs / 2), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, NoteLengthMs), TempoMap))
                .ToArray();

            var actions = SevenBitNumber
                .Values
                .Take(notesCount)
                .Select(n => new DynamicPlaybackAction(NoteLengthMs, (playback, collection) =>
                {
                    CheckDuration(TimeSpan.FromMilliseconds(notesCount * NoteLengthMs + NoteLengthMs / 2), playback);
                    collection.Remove(objectsToRemove[n]);
                    CheckDuration(TimeSpan.FromMilliseconds(n == notesCount - 1 ? 0 : notesCount * NoteLengthMs + NoteLengthMs / 2), playback);
                }))
                .ToArray();

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: objectsToRemove,
                actions: actions,
                expectedReceivedEvents: SevenBitNumber
                    .Values
                    .Take(notesCount)
                    .SelectMany(n => new[]
                    {
                        new TimestampedEvent(new NoteOnEvent(n, Note.DefaultVelocity), TimeSpan.FromMilliseconds(n * NoteLengthMs + NoteLengthMs / 2)),
                        new TimestampedEvent(new NoteOffEvent(n, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(n * NoteLengthMs + NoteLengthMs)),
                    })
                    .ToArray(),
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [MultimediaTestRetry]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_RemoveAtAdvance(
            [Values(1, 2, 3, 4, 8, 16, 17, 32, 50, 51)] int notesCount)
        {
            var objectsToRemove = SevenBitNumber
                .Values
                .Take(notesCount)
                .Select(n => new Note(n)
                    .SetTime(new MetricTimeSpan(0, 0, 0, n * NoteLengthMs + NoteLengthMs), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, NoteLengthMs), TempoMap))
                .ToArray();

            var actions = SevenBitNumber
                .Values
                .Take(notesCount)
                .Select(n => new DynamicPlaybackAction(n == 0 ? NoteLengthMs / 2 : NoteLengthMs, (playback, collection) =>
                {
                    CheckDuration(TimeSpan.FromMilliseconds(notesCount * NoteLengthMs + NoteLengthMs), playback);
                    collection.Remove(objectsToRemove[n]);
                    CheckDuration(TimeSpan.FromMilliseconds(n == notesCount - 1 ? 0 : notesCount * NoteLengthMs + NoteLengthMs), playback);
                }))
                .ToArray();

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: objectsToRemove,
                actions: actions,
                expectedReceivedEvents: Array.Empty<TimestampedEvent>(),
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [MultimediaTestRetry]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_ShiftNoteAtAdvance()
        {
            var noteNumber = (SevenBitNumber)70;

            var objectToShift = new Note(noteNumber)
                .SetTime(new MetricTimeSpan(0, 0, 0, NoteLengthMs / 2), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, NoteLengthMs), TempoMap);

            var actions = Enumerable
                .Range(0, 10)
                .Select(n => new DynamicPlaybackAction(n == 0 ? 10 : NoteLengthMs, (playback, collection) =>
                {
                    CheckDuration(TimeSpan.FromMilliseconds(n * NoteLengthMs + NoteLengthMs / 2 + NoteLengthMs), playback);
                    collection.ChangeObject(
                        objectToShift,
                        obj => obj
                            .SetTime(new MetricTimeSpan(0, 0, 0, (n + 1) * NoteLengthMs + NoteLengthMs / 2), TempoMap));
                    CheckDuration(TimeSpan.FromMilliseconds((n + 1) * NoteLengthMs + NoteLengthMs / 2 + NoteLengthMs), playback);
                }))
                .ToArray();

            var lastMs = 10 * NoteLengthMs + NoteLengthMs / 2;

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: new[] { objectToShift },
                actions: actions,
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new NoteOnEvent(noteNumber, Note.DefaultVelocity), TimeSpan.FromMilliseconds(lastMs)),
                    new TimestampedEvent(new NoteOffEvent(noteNumber, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(lastMs + NoteLengthMs)),
                },
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [MultimediaTestRetry]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddAndRemoveAtAdvanceByOne(
            [Values(1, 2, 3, 4, 8, 16, 17, 32, 50, 51)] int notesCount,
            [Values(0, 10)] int gapMs,
            [Values] bool viaChangeCollection)
        {
            var lastEventTime = notesCount * NoteLengthMs + NoteLengthMs + notesCount * gapMs;

            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, lastEventTime), TempoMap),
            };

            var objectsToAdd = SevenBitNumber
                .Values
                .Take(notesCount)
                .Select(n => new Note(n)
                    .SetTime(new MetricTimeSpan(0, 0, 0, n * NoteLengthMs + NoteLengthMs / 2 + n * gapMs), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, NoteLengthMs), TempoMap))
                .ToArray();

            var action = viaChangeCollection
                ? new Action<Playback, ObservableTimedObjectsCollection, int>((playback, collection, n) =>
                {
                    collection.ChangeCollection(() =>
                    {
                        collection.Add(objectsToAdd[n]);
                        collection.Remove(objectsToAdd[n]);
                    });
                    CheckDuration(TimeSpan.FromMilliseconds(lastEventTime), playback);
                })
                : (playback, collection, n) =>
                {
                    collection.Add(objectsToAdd[n]);
                    CheckDuration(TimeSpan.FromMilliseconds(lastEventTime), playback);
                    collection.Remove(objectsToAdd[n]);
                    CheckDuration(TimeSpan.FromMilliseconds(lastEventTime), playback);
                };

            var actions = SevenBitNumber
                .Values
                .Take(notesCount)
                .Select(n => new DynamicPlaybackAction(n == 0 ? 0 : NoteLengthMs,
                    (playback, collection) => action(playback, collection, n)))
                .ToArray();

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: actions,
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new TextEvent("END"), TimeSpan.FromMilliseconds(lastEventTime)),
                },
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [MultimediaTestRetry]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddAtAdvanceAndRemovePastByOne(
            [Values(1, 2, 3, 4, 8, 16, 17, 32, 50, 51)] int notesCount)
        {
            var lastEventTime = notesCount * NoteLengthMs + NoteLengthMs;

            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, lastEventTime), TempoMap),
            };

            var objectsToAdd = SevenBitNumber
                .Values
                .Take(notesCount)
                .Select(n => new Note(n)
                    .SetTime(new MetricTimeSpan(0, 0, 0, n * NoteLengthMs + NoteLengthMs / 2), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, NoteLengthMs), TempoMap))
                .ToArray();

            var actions = SevenBitNumber
                .Values
                .Take(notesCount)
                .SelectMany(n => new[]
                {
                    new DynamicPlaybackAction(n == 0 ? 0 : NoteLengthMs, (playback, collection) =>
                    {
                        collection.Add(objectsToAdd[n]);
                        CheckDuration(TimeSpan.FromMilliseconds(lastEventTime), playback);

                        if (n >= 2)
                        {
                            collection.Remove(objectsToAdd[n - 2]);
                            CheckDuration(TimeSpan.FromMilliseconds(lastEventTime), playback);
                        }
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
                        new TimestampedEvent(new NoteOnEvent(n, Note.DefaultVelocity), TimeSpan.FromMilliseconds(n * NoteLengthMs + NoteLengthMs / 2)),
                        new TimestampedEvent(new NoteOffEvent(n, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds((n + 1) * NoteLengthMs + NoteLengthMs / 2)),
                    })
                    .Concat(new[]
                    {
                        new TimestampedEvent(new TextEvent("END"), TimeSpan.FromMilliseconds(lastEventTime)),
                    })
                    .ToArray(),
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [MultimediaTestRetry]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_TempoChanges_1()
        {
            var object1 = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                .SetTime(new MetricTimeSpan(0, 0, 0, 80), TempoMap);

            var object2 = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 4))
                .SetTime(new MetricTimeSpan(0, 0, 0, 350), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap),
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 100), TempoMap),
                new TimedEvent(new TextEvent("B"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap),
                new Note((SevenBitNumber)80)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 200), TempoMap),
                new TimedEvent(new TextEvent("C"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 800), TempoMap),
                new Note((SevenBitNumber)90)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 900), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap),
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(20, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(1200), playback);
                        collection.Add(object1);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(20), "A");
                        CheckDuration(TimeSpan.FromMilliseconds(640), playback);
                    }),
                    new DynamicPlaybackAction(20, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(640), playback);
                        collection.ChangeObject(
                            object1,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 120), TempoMap));
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(40), "B");
                        CheckDuration(TimeSpan.FromMilliseconds(660), playback);
                    }),
                    new DynamicPlaybackAction(190, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(660), playback);
                        collection.Remove(object1);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(340), "C");
                        CheckDuration(TimeSpan.FromMilliseconds(1200), playback);
                    }),
                    new DynamicPlaybackAction(110, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(1200), playback);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(450), "D");
                        collection.Add(object1, object2);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(260), "E");
                        CheckDuration(TimeSpan.FromMilliseconds(447), playback);
                    }),
                    new DynamicPlaybackAction(87, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(447), playback);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(347), "F");
                        collection.ChangeObject(
                            object2,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 850), TempoMap));
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(455), "G");
                        CheckDuration(TimeSpan.FromMilliseconds(573), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(100)),
                    new TimestampedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2), TimeSpan.FromMilliseconds(120)),
                    new TimestampedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(160)),
                    new TimestampedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(210)),
                    new TimestampedEvent(new TextEvent("B"), TimeSpan.FromMilliseconds(290)),
                    new TimestampedEvent(new NoteOnEvent((SevenBitNumber)80, Note.DefaultVelocity), TimeSpan.FromMilliseconds(352)),
                    new TimestampedEvent(new NoteOffEvent((SevenBitNumber)80, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(402)),
                    new TimestampedEvent(new TextEvent("C"), TimeSpan.FromMilliseconds(439)),
                    new TimestampedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 4), TimeSpan.FromMilliseconds(464)),
                    new TimestampedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(467)),
                    new TimestampedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(542)),
                });
        }

        [MultimediaTestRetry]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_TempoChanges_2()
        {
            var setTempoEvent = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap);

            var programChangeEvent = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)8))
                .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)90)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap),
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(900), playback);
                        collection.Add(setTempoEvent);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(100), "A");
                        CheckDuration(TimeSpan.FromMilliseconds(650), playback);
                    }),
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        collection.Add(programChangeEvent);
                        playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 450));
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(450), "B");
                        CheckDuration(TimeSpan.FromMilliseconds(650), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new ProgramChangeEvent((SevenBitNumber)8), TimeSpan.FromMilliseconds(200)),
                    new TimestampedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(250)),
                    new TimestampedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                });
        }

        [MultimediaTestRetry]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_TempoChanges_3()
        {
            var setTempoEvent = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap);

            var programChangeEvent = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)8))
                .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)90)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap),
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(500, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(900), playback);
                        collection.Add(setTempoEvent, programChangeEvent);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(450), "A");
                        CheckDuration(TimeSpan.FromMilliseconds(650), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new ProgramChangeEvent((SevenBitNumber)8), TimeSpan.FromMilliseconds(500)),
                    new TimestampedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(550)),
                    new TimestampedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                });
        }

        [MultimediaTestRetry]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_TempoChanges_4()
        {
            var setTempoEvent = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap);

            var programChangeEvent = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)8))
                .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                programChangeEvent,
                new Note((SevenBitNumber)90)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap),
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(900), playback);
                        collection.Add(setTempoEvent);
                        CheckDuration(TimeSpan.FromMilliseconds(550), playback);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(100), "A");
                        playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 260));
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(260), "B");
                    }),
                    new DynamicPlaybackAction(240, (playback, collection) =>
                    {
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(500), "C");
                        playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 100));
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(100), "D");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new ProgramChangeEvent((SevenBitNumber)8), TimeSpan.FromMilliseconds(100)),
                    new TimestampedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(240)),
                    new TimestampedEvent(new ProgramChangeEvent(), TimeSpan.FromMilliseconds(340)),
                    new TimestampedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(340)),
                    new TimestampedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2), TimeSpan.FromMilliseconds(440)),
                    new TimestampedEvent(new ProgramChangeEvent((SevenBitNumber)8), TimeSpan.FromMilliseconds(490)),
                    new TimestampedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(640)),
                    new TimestampedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(790)),
                });
        }

        [MultimediaTestRetry]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_TempoMapChanges_1()
        {
            var setTempoEvent = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap);

            var timeSignatureEvent = new TimedEvent(new TimeSignatureEvent(3, 4))
                .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                        collection.Add(setTempoEvent, timeSignatureEvent);
                        CheckDuration(TimeSpan.FromMilliseconds(450), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2), TimeSpan.FromMilliseconds(200)),
                    new TimestampedEvent(new TimeSignatureEvent(3, 4), TimeSpan.FromMilliseconds(250)),
                    new TimestampedEvent(new TextEvent("END"), TimeSpan.FromMilliseconds(450)),
                },
                additionalChecks: (playback, _) => MidiAsserts.AreEqual(
                    AddTimeSignatureChanges(
                        AddTempoChanges(TempoMap, (TimeSpan.FromMilliseconds(200), new Tempo(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))),
                        (TimeSpan.FromMilliseconds(250), new TimeSignature(3, 4))),
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [MultimediaTestRetry]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_TempoMapChanges_2()
        {
            var setTempoEvent = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap);

            var timeSignatureEvent = new TimedEvent(new TimeSignatureEvent(3, 4))
                .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                        collection.Add(timeSignatureEvent, setTempoEvent);
                        CheckDuration(TimeSpan.FromMilliseconds(450), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2), TimeSpan.FromMilliseconds(200)),
                    new TimestampedEvent(new TimeSignatureEvent(3, 4), TimeSpan.FromMilliseconds(250)),
                    new TimestampedEvent(new TextEvent("END"), TimeSpan.FromMilliseconds(450)),
                },
                additionalChecks: (playback, _) => MidiAsserts.AreEqual(
                    AddTimeSignatureChanges(
                        AddTempoChanges(TempoMap, (TimeSpan.FromMilliseconds(200), new Tempo(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))),
                        (TimeSpan.FromMilliseconds(250), new TimeSignature(3, 4))),
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [MultimediaTestRetry]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_TempoMapChanges_3()
        {
            var setTempoEvent = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(50, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(900), playback);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(50), "A");
                        collection.Add(setTempoEvent);
                        CheckDuration(TimeSpan.FromMilliseconds(500), playback);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(50), "B");
                        playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 300));
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(300), "C");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(50)),
                    new TimestampedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(250)),
                });
        }

        [MultimediaTestRetry]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_TempoMapChanges_4()
        {
            var setTempoEvent = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 400), TempoMap),
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                        collection.Add(setTempoEvent);
                        CheckDuration(TimeSpan.FromMilliseconds(450), playback);
                        playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 500));
                    }),
                },
                expectedReceivedEvents: Array.Empty<TimestampedEvent>());
        }

        [MultimediaTestRetry]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_TempoMapChanges_5()
        {
            var setTempoEvent = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 400), TempoMap),
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                        collection.Add(setTempoEvent);
                        CheckDuration(TimeSpan.FromMilliseconds(450), playback);
                        playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 400));
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new TimestampedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(150)),
                });
        }

        [MultimediaTestRetry]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_TempoMapChanges_6()
        {
            var setTempoEvent = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 400), TempoMap),
                new Note((SevenBitNumber)80)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap),
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(1000), playback);
                        collection.Add(setTempoEvent);
                        playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 600));
                        CheckDuration(TimeSpan.FromMilliseconds(650), playback);
                    }),
                    new DynamicPlaybackAction(20, (playback, collection) =>
                    {
                        playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 350));
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new NoteOnEvent((SevenBitNumber)80, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new TimestampedEvent(new NoteOffEvent((SevenBitNumber)80, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(120)),
                    new TimestampedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(120)),
                    new TimestampedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(220)),
                    new TimestampedEvent(new NoteOnEvent((SevenBitNumber)80, Note.DefaultVelocity), TimeSpan.FromMilliseconds(270)),
                    new TimestampedEvent(new NoteOffEvent((SevenBitNumber)80, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(420)),
                });
        }

        [MultimediaTestRetry]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_TempoMapChanges_7()
        {
            var setTempoEvent = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote * 2))
                .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 400), TempoMap),
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                        collection.Add(setTempoEvent);
                        playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 1000));
                        CheckDuration(TimeSpan.FromMilliseconds(900), playback);
                    }),
                },
                expectedReceivedEvents: Array.Empty<TimestampedEvent>());
        }

        [MultimediaTestRetry]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_TempoMapChanges_8()
        {
            var setTempoEvent = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote * 2))
                .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 400), TempoMap),
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                        collection.Add(setTempoEvent);
                        playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 800));
                        CheckDuration(TimeSpan.FromMilliseconds(900), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new TimestampedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(200)),
                });
        }

        [MultimediaTestRetry]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_TempoMapChanges_9()
        {
            var setTempoEvent = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote * 2))
                .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 400), TempoMap),
                new Note((SevenBitNumber)80)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap),
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(1000), playback);
                        collection.Add(setTempoEvent);
                        CheckDuration(TimeSpan.FromMilliseconds(1700), playback);
                        playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 1600));
                    }),
                    new DynamicPlaybackAction(50, (playback, collection) =>
                    {
                        playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 400));
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new NoteOnEvent((SevenBitNumber)80, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new TimestampedEvent(new NoteOffEvent((SevenBitNumber)80, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(150)),
                    new TimestampedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(150)),
                    new TimestampedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(650)),
                    new TimestampedEvent(new NoteOnEvent((SevenBitNumber)80, Note.DefaultVelocity), TimeSpan.FromMilliseconds(850)),
                    new TimestampedEvent(new NoteOffEvent((SevenBitNumber)80, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1450)),
                });
        }

        #endregion
    }
}
