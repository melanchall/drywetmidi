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
            [Values(1, 2, 3, 4, 8, 16, 17, 32, 50, 51)] int notesCount,
            [Values(0, 10)] int gapMs)
        {
            var noteLengthMs = 40;
            var lastEventTime = notesCount * noteLengthMs + 40 + notesCount * gapMs;

            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, lastEventTime), TempoMap),
            };

            var objectsToAdd = SevenBitNumber
                .Values
                .Take(notesCount)
                .Select(n => new Note(n)
                    .SetTime(new MetricTimeSpan(0, 0, 0, n * noteLengthMs + 20 + n * gapMs), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, noteLengthMs), TempoMap))
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
                        new ReceivedEvent(new NoteOnEvent(n, Note.DefaultVelocity), TimeSpan.FromMilliseconds(n * noteLengthMs + 20 + n * gapMs)),
                        new ReceivedEvent(new NoteOffEvent(n, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds((n + 1) * noteLengthMs + 20 + n * gapMs)),
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
            [Values(1, 2, 3, 4, 8, 16, 17, 32, 50, 51)] int notesCount)
        {
            var noteLengthMs = 40;
            var lastEventTime = notesCount * noteLengthMs + 40;

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
                    .SetTime(new MetricTimeSpan(0, 0, 0, n * noteLengthMs + 20), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, noteLengthMs), TempoMap))
                .ToArray();

            var actions = SevenBitNumber
                .Values
                .Take(notesCount)
                .Select(n => new PlaybackChanger(n == 0 ? 0 : noteLengthMs,
                    (playback, collection) => collection.ChangeCollection(() =>
                    {
                        collection.Add(objectsToAdd[n]);
                        collection.ChangeObject(endObject, obj => obj
                            .SetTime(new MetricTimeSpan(0, 0, 0, n * noteLengthMs + 30), TempoMap));
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
                        new ReceivedEvent(new NoteOnEvent(n, Note.DefaultVelocity), TimeSpan.FromMilliseconds(n * noteLengthMs + 20)),
                        new ReceivedEvent(new TextEvent("END"), TimeSpan.FromMilliseconds(n * noteLengthMs + 30)),
                        new ReceivedEvent(new NoteOffEvent(n, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds((n + 1) * noteLengthMs + 20)),
                    })
                    .ToArray(),
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddAtAdvanceByOneWithOverlapping(
            [Values(1, 2, 3, 4, 8, 16, 17, 32, 50, 51)] int notesCount)
        {
            var overlappedMs = 5;
            var noteLengthMs = 40;
            var lastEventTime = notesCount * noteLengthMs + 40 - notesCount * overlappedMs;

            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, lastEventTime), TempoMap),
            };

            var objectsToAdd = SevenBitNumber
                .Values
                .Take(notesCount)
                .Select(n => new Note(n)
                    .SetTime(new MetricTimeSpan(0, 0, 0, n * noteLengthMs + 20 - n * overlappedMs), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, noteLengthMs), TempoMap))
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
                        new ReceivedEvent(new NoteOnEvent(n, Note.DefaultVelocity), TimeSpan.FromMilliseconds(n * noteLengthMs + 20 - n * overlappedMs)),
                        new ReceivedEvent(new NoteOffEvent(n, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds((n + 1) * noteLengthMs + 20 - n * overlappedMs)),
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
            [Values(1, 2, 3, 4, 8, 16, 17, 32, 50, 51)] int notesCount)
        {
            var noteLengthMs = 20;
            var lastEventTime = notesCount * noteLengthMs + 20;

            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, lastEventTime), TempoMap),
            };

            var objectsToAdd = SevenBitNumber
                .Values
                .Take(notesCount)
                .Select(n => new Note(n)
                    .SetTime(new MetricTimeSpan(0, 0, 0, n * noteLengthMs + 10), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, noteLengthMs), TempoMap))
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
                    .SetTime(new MetricTimeSpan(0, 0, 0, lastEventTime), TempoMap),
            };

            var objectsToAdd = SevenBitNumber
                .Values
                .Take(notesCount)
                .Select(n => new Note(n)
                    .SetTime(new MetricTimeSpan(0, 0, 0, n * noteLengthMs + 10), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, noteLengthMs), TempoMap))
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
                            objectsToAdd.AtTime(new MetricTimeSpan(0, 0, 0, addAtMs), TempoMap, LengthedObjectPart.Entire).First().GetTimedNoteOnEvent().Event,
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
            [Values(1, 2, 3, 4, 8, 16, 17, 32, 50, 51)] int notesCount)
        {
            var noteLengthMs = 20;

            var objectsToRemove = SevenBitNumber
                .Values
                .Take(notesCount)
                .Select(n => new Note(n)
                    .SetTime(new MetricTimeSpan(0, 0, 0, n * noteLengthMs + 10), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, noteLengthMs), TempoMap))
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
            [Values(1, 2, 3, 4, 8, 16, 17, 32, 50, 51)] int notesCount)
        {
            var noteLengthMs = 40;

            var objectsToRemove = SevenBitNumber
                .Values
                .Take(notesCount)
                .Select(n => new Note(n)
                    .SetTime(new MetricTimeSpan(0, 0, 0, n * noteLengthMs + 40), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, noteLengthMs), TempoMap))
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
                .SetTime(new MetricTimeSpan(0, 0, 0, 20), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, noteLengthMs), TempoMap);

            var actions = Enumerable
                .Range(0, 10)
                .Select(n => new PlaybackChanger(n == 0 ? 10 : noteLengthMs,
                    (playback, collection) => collection.ChangeObject(objectToShift, obj => obj
                        .SetTime(new MetricTimeSpan(0, 0, 0, (n + 1) * noteLengthMs + 20), TempoMap))))
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
            [Values(1, 2, 3, 4, 8, 16, 17, 32, 50, 51)] int notesCount,
            [Values(0, 10)] int gapMs,
            [Values] bool viaChangeCollection)
        {
            var noteLengthMs = 40;
            var lastEventTime = notesCount * noteLengthMs + 40 + notesCount * gapMs;

            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, lastEventTime), TempoMap),
            };

            var objectsToAdd = SevenBitNumber
                .Values
                .Take(notesCount)
                .Select(n => new Note(n)
                    .SetTime(new MetricTimeSpan(0, 0, 0, n * noteLengthMs + 20 + n * gapMs), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, noteLengthMs), TempoMap))
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
            [Values(1, 2, 3, 4, 8, 16, 17, 32, 50, 51)] int notesCount)
        {
            var noteLengthMs = 20;
            var lastEventTime = notesCount * noteLengthMs + 20;

            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, lastEventTime), TempoMap),
            };

            var objectsToAdd = SevenBitNumber
                .Values
                .Take(notesCount)
                .Select(n => new Note(n)
                    .SetTime(new MetricTimeSpan(0, 0, 0, n * noteLengthMs + 10), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, noteLengthMs), TempoMap))
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

        [Retry(OnTheFlyChecksRetriesNumber)]
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
                    new PlaybackChanger(20, (playback, collection) =>
                    {
                        collection.Add(object1);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(20), "A");
                    }),
                    new PlaybackChanger(20, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            object1,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 120), TempoMap));
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(40), "B");
                    }),
                    new PlaybackChanger(190, (playback, collection) =>
                    {
                        collection.Remove(object1);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(340), "C");
                    }),
                    new PlaybackChanger(110, (playback, collection) =>
                    {
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(450), "D");
                        collection.Add(object1, object2);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(260), "E");
                    }),
                    new PlaybackChanger(87, (playback, collection) =>
                    {
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(347), "F");
                        collection.ChangeObject(
                            object2,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 850), TempoMap));
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(435), "G");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2), TimeSpan.FromMilliseconds(120)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(160)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(210)),
                    new ReceivedEvent(new TextEvent("B"), TimeSpan.FromMilliseconds(290)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)80, Note.DefaultVelocity), TimeSpan.FromMilliseconds(352)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)80, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(402)),
                    new ReceivedEvent(new TextEvent("C"), TimeSpan.FromMilliseconds(439)),
                    new ReceivedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 4), TimeSpan.FromMilliseconds(464)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(467)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(542)),
                });
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
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
                    new PlaybackChanger(100, (playback, collection) =>
                    {
                        collection.Add(setTempoEvent);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(100), "A");
                    }),
                    new PlaybackChanger(100, (playback, collection) =>
                    {
                        collection.Add(programChangeEvent);
                        playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 450));
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(450), "B");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)8), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(250)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                });
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
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
                    new PlaybackChanger(500, (playback, collection) =>
                    {
                        collection.Add(setTempoEvent, programChangeEvent);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(450), "A");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)8), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(550)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                });
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
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
                    new PlaybackChanger(100, (playback, collection) =>
                    {
                        collection.Add(setTempoEvent);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(100), "A");
                        playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 260));
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(260), "B");
                    }),
                    new PlaybackChanger(240, (playback, collection) =>
                    {
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(500), "C");
                        playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 100));
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(100), "D");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)8), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(240)),
                    new ReceivedEvent(new ProgramChangeEvent(), TimeSpan.FromMilliseconds(340)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(340)),
                    new ReceivedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2), TimeSpan.FromMilliseconds(440)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)8), TimeSpan.FromMilliseconds(490)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(640)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(790)),
                });
        }

        #endregion
    }
}
