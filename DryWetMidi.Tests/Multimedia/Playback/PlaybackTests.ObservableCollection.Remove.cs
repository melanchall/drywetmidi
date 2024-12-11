using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed partial class PlaybackTests
    {
        #region Test methods

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_RemoveEventWithinNote_AfterCurrentTime()
        {
            var objectToRemove = new TimedEvent(new TextEvent("A")).SetTime(new MetricTimeSpan(0, 0, 0, 700), OnTheFlyChecksTempoMap);
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 1), OnTheFlyChecksTempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(500,
                        (playback, collection) => collection.Remove(objectToRemove)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                });
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_RemoveEventWithinNote_BeforeCurrentTime()
        {
            var objectToRemove = new TimedEvent(new TextEvent("A")).SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap);
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 1), OnTheFlyChecksTempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(500,
                        (playback, collection) => collection.Remove(objectToRemove)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                });
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_RemoveLastActiveEvent()
        {
            var objectToRemove = new TimedEvent(new TextEvent("B")).SetTime(new MetricTimeSpan(0, 0, 0, 500), OnTheFlyChecksTempoMap);
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A")),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(200,
                        (playback, collection) => collection.Remove(objectToRemove)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(0)),
                });
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_RemoveAtSameTime_AfterCurrentTime([Values(0, 1, 2)] int indexToRemove)
        {
            var objectToRemove1 = new TimedEvent(new TextEvent("A")).SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap);
            var objectToRemove2 = new TimedEvent(new TextEvent("B")).SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap);
            var objectToRemove3 = new TimedEvent(new TextEvent("C")).SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap);

            var objectsToRemove = new ITimedObject[]
            {
                objectToRemove1,
                objectToRemove2,
                objectToRemove3,
            };

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 500), OnTheFlyChecksTempoMap),
                objectToRemove1,
                objectToRemove2,
                objectToRemove3,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(200,
                        (playback, collection) => collection.Remove(objectsToRemove[indexToRemove])),
                },
                expectedReceivedEvents:
                    new[]
                    {
                        new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(0)),
                    }
                    .Concat(
                        objectsToRemove
                            .Except(new[] { objectsToRemove[indexToRemove] })
                            .Select(o => new ReceivedEvent(((TimedEvent)o).Event, TimeSpan.FromMilliseconds(300))))
                    .Concat(new[]
                    {
                        new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(500)),
                    })
                    .ToArray());
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_RemoveAtSameTime_BeforeCurrentTime([Values(0, 1, 2)] int indexToRemove)
        {
            var objectToRemove1 = new TimedEvent(new TextEvent("A")).SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap);
            var objectToRemove2 = new TimedEvent(new TextEvent("B")).SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap);
            var objectToRemove3 = new TimedEvent(new TextEvent("C")).SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap);

            var objectsToRemove = new ITimedObject[]
            {
                objectToRemove1,
                objectToRemove2,
                objectToRemove3,
            };

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 700), OnTheFlyChecksTempoMap),
                objectToRemove1,
                objectToRemove2,
                objectToRemove3,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(500,
                        (playback, collection) => collection.Remove(objectsToRemove[indexToRemove])),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new TextEvent("B"), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new TextEvent("C"), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                });
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_RemoveProgramChange_AfterCurrentTime_1()
        {
            var objectToRemove = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)7)).SetTime(new MetricTimeSpan(0, 0, 0, 250), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), OnTheFlyChecksTempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(200,
                        (playback, collection) => collection.Remove(objectToRemove)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                },
                setupPlayback: playback => playback.TrackProgram = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_RemoveProgramChange_AfterCurrentTime_2()
        {
            var objectToRemove = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)7)).SetTime(new MetricTimeSpan(0, 0, 0, 500), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), OnTheFlyChecksTempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(400,
                        (playback, collection) => collection.Remove(objectToRemove)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                },
                setupPlayback: playback => playback.TrackProgram = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_RemoveProgramChange_BeforeCurrentTime_1()
        {
            var objectToRemove = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)7)).SetTime(new MetricTimeSpan(0, 0, 0, 50), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), OnTheFlyChecksTempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(200,
                        (playback, collection) => collection.Remove(objectToRemove)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)7), TimeSpan.FromMilliseconds(50)),
                    new ReceivedEvent(new ProgramChangeEvent(), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                },
                setupPlayback: playback => playback.TrackProgram = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_RemoveProgramChange_BeforeCurrentTime_2()
        {
            var objectToRemove1 = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)7)).SetTime(new MetricTimeSpan(0, 0, 0, 50), OnTheFlyChecksTempoMap);
            var objectToRemove2 = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)8)).SetTime(new MetricTimeSpan(0, 0, 0, 100), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap),
                objectToRemove1,
                objectToRemove2,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(200,
                        (playback, collection) => collection.Remove(objectToRemove2)),
                    new PlaybackChanger(100,
                        (playback, collection) => collection.Remove(objectToRemove1)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)7), TimeSpan.FromMilliseconds(50)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)8), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)7), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new ProgramChangeEvent(), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                },
                setupPlayback: playback => playback.TrackProgram = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_RemovePitchBend_AfterCurrentTime_1()
        {
            var objectToRemove = new TimedEvent(new PitchBendEvent(7000)).SetTime(new MetricTimeSpan(0, 0, 0, 250), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), OnTheFlyChecksTempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(200,
                        (playback, collection) => collection.Remove(objectToRemove)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                },
                setupPlayback: playback => playback.TrackPitchValue = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_RemovePitchBend_AfterCurrentTime_2()
        {
            var objectToRemove = new TimedEvent(new PitchBendEvent(7000)).SetTime(new MetricTimeSpan(0, 0, 0, 500), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), OnTheFlyChecksTempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(400,
                        (playback, collection) => collection.Remove(objectToRemove)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                },
                setupPlayback: playback => playback.TrackPitchValue = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_RemovePitchBend_BeforeCurrentTime_1()
        {
            var objectToRemove = new TimedEvent(new PitchBendEvent(7000)).SetTime(new MetricTimeSpan(0, 0, 0, 50), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), OnTheFlyChecksTempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(200,
                        (playback, collection) => collection.Remove(objectToRemove)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new PitchBendEvent(7000), TimeSpan.FromMilliseconds(50)),
                    new ReceivedEvent(new PitchBendEvent(), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                },
                setupPlayback: playback => playback.TrackPitchValue = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_RemovePitchBend_BeforeCurrentTime_2()
        {
            var objectToRemove1 = new TimedEvent(new PitchBendEvent(7000)).SetTime(new MetricTimeSpan(0, 0, 0, 50), OnTheFlyChecksTempoMap);
            var objectToRemove2 = new TimedEvent(new PitchBendEvent(8000)).SetTime(new MetricTimeSpan(0, 0, 0, 100), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap),
                objectToRemove1,
                objectToRemove2,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(200,
                        (playback, collection) => collection.Remove(objectToRemove2)),
                    new PlaybackChanger(100,
                        (playback, collection) => collection.Remove(objectToRemove1)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new PitchBendEvent(7000), TimeSpan.FromMilliseconds(50)),
                    new ReceivedEvent(new PitchBendEvent(8000), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new PitchBendEvent(7000), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new PitchBendEvent(), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                },
                setupPlayback: playback => playback.TrackPitchValue = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_RemoveControlChange_AfterCurrentTime_1()
        {
            var objectToRemove = new TimedEvent(new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)20)).SetTime(new MetricTimeSpan(0, 0, 0, 250), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), OnTheFlyChecksTempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(200,
                        (playback, collection) => collection.Remove(objectToRemove)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                },
                setupPlayback: playback => playback.TrackControlValue = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_RemoveControlChange_AfterCurrentTime_2()
        {
            var objectToRemove = new TimedEvent(new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)20)).SetTime(new MetricTimeSpan(0, 0, 0, 500), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), OnTheFlyChecksTempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(400,
                        (playback, collection) => collection.Remove(objectToRemove)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                },
                setupPlayback: playback => playback.TrackControlValue = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_RemoveControlChange_BeforeCurrentTime_1()
        {
            var objectToRemove = new TimedEvent(new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)20)).SetTime(new MetricTimeSpan(0, 0, 0, 50), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), OnTheFlyChecksTempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(200,
                        (playback, collection) => collection.Remove(objectToRemove)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)20), TimeSpan.FromMilliseconds(50)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)7, SevenBitNumber.MinValue), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                },
                setupPlayback: playback => playback.TrackControlValue = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_RemoveControlChange_BeforeCurrentTime_2()
        {
            var objectToRemove1 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 50), OnTheFlyChecksTempoMap);
            var objectToRemove2 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)80)).SetTime(new MetricTimeSpan(0, 0, 0, 100), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap),
                objectToRemove1,
                objectToRemove2,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(200,
                        (playback, collection) => collection.Remove(objectToRemove2)),
                    new PlaybackChanger(100,
                        (playback, collection) => collection.Remove(objectToRemove1)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)70), TimeSpan.FromMilliseconds(50)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)80), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)70), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)7, SevenBitNumber.MinValue), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                },
                setupPlayback: playback => playback.TrackControlValue = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_RemoveControlChange_BeforeCurrentTime_3()
        {
            var objectToRemove1 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 50), OnTheFlyChecksTempoMap);
            var objectToRemove2 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)8, (SevenBitNumber)80)).SetTime(new MetricTimeSpan(0, 0, 0, 100), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap),
                objectToRemove1,
                objectToRemove2,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(200,
                        (playback, collection) => collection.Remove(objectToRemove2)),
                    new PlaybackChanger(100,
                        (playback, collection) => collection.Remove(objectToRemove1)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)70), TimeSpan.FromMilliseconds(50)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)8, (SevenBitNumber)80), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)8, SevenBitNumber.MinValue), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)7, SevenBitNumber.MinValue), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                },
                setupPlayback: playback => playback.TrackControlValue = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_RemoveControlChange_BeforeCurrentTime_4()
        {
            var objectToRemove1 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 50), OnTheFlyChecksTempoMap);
            var objectToRemove2 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)80) { Channel = (FourBitNumber)6 }).SetTime(new MetricTimeSpan(0, 0, 0, 100), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap),
                objectToRemove1,
                objectToRemove2,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(200,
                        (playback, collection) => collection.Remove(objectToRemove2)),
                    new PlaybackChanger(100,
                        (playback, collection) => collection.Remove(objectToRemove1)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)70), TimeSpan.FromMilliseconds(50)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)80) { Channel = (FourBitNumber)6 }, TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)7, SevenBitNumber.MinValue) { Channel = (FourBitNumber)6 }, TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)7, SevenBitNumber.MinValue), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                },
                setupPlayback: playback => playback.TrackControlValue = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_RemoveNotes_1()
        {
            var objectToRemove = new Note((SevenBitNumber)50)
                .SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 200), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 700), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 100), OnTheFlyChecksTempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(600,
                        (playback, collection) => collection.Remove(objectToRemove)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                },
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_RemoveNotes_2()
        {
            var objectToRemove = new Note((SevenBitNumber)50)
                .SetTime(new MetricTimeSpan(0, 0, 0, 500), OnTheFlyChecksTempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 200), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 100), OnTheFlyChecksTempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(450,
                        (playback, collection) => collection.Remove(objectToRemove)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                },
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_RemoveNotes_3()
        {
            var objectToRemove = new Note((SevenBitNumber)50)
                .SetTime(new MetricTimeSpan(0, 0, 0, 100), OnTheFlyChecksTempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 500), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 200), OnTheFlyChecksTempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(200,
                        (playback, collection) => collection.Remove(objectToRemove)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                },
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_RemoveNotes_4()
        {
            var objectToRemove1 = new Note((SevenBitNumber)50)
                .SetTime(new MetricTimeSpan(0, 0, 0, 100), OnTheFlyChecksTempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap);
            var objectToRemove2 = new Note((SevenBitNumber)60)
                .SetTime(new MetricTimeSpan(0, 0, 0, 450), OnTheFlyChecksTempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 100), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 700), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 100), OnTheFlyChecksTempoMap),
                objectToRemove1,
                objectToRemove2,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(200,
                        (playback, collection) => collection.Remove(objectToRemove1)),
                    new PlaybackChanger(300,
                        (playback, collection) => collection.Remove(objectToRemove2)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)60, Note.DefaultVelocity), TimeSpan.FromMilliseconds(450)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)60, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                },
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_RemoveNotes_5()
        {
            var objectToRemove = new Note((SevenBitNumber)50)
                .SetTime(new MetricTimeSpan(0, 0, 0, 100), OnTheFlyChecksTempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 400), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(200,
                        (playback, collection) => collection.Remove(objectToRemove)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(200)),
                },
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_RemoveNotes_6()
        {
            var objectToRemove1 = new Note((SevenBitNumber)50)
                .SetTime(new MetricTimeSpan(0, 0, 0, 600), OnTheFlyChecksTempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 200), OnTheFlyChecksTempoMap);
            var objectToRemove2 = new Note((SevenBitNumber)80)
                .SetTime(new MetricTimeSpan(0, 0, 0, 600), OnTheFlyChecksTempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 200), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 200), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap),
                objectToRemove1,
                objectToRemove2,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(700,
                        (playback, collection) => collection.Remove(objectToRemove1)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)80, Note.DefaultVelocity), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)80, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                },
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_RemoveEventWithinNote_AfterCurrentTime()
        {
            var objectToRemove = new TimedEvent(new TextEvent("A")).SetTime(new MetricTimeSpan(0, 0, 0, 400), OnTheFlyChecksTempoMap);
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 500), OnTheFlyChecksTempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(300,
                        (playback, collection) => collection.Remove(objectToRemove)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                },
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_RemoveEventWithinNote_BeforeCurrentTime()
        {
            var objectToRemove = new TimedEvent(new TextEvent("A")).SetTime(new MetricTimeSpan(0, 0, 0, 200), OnTheFlyChecksTempoMap);
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 500), OnTheFlyChecksTempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(300,
                        (playback, collection) => collection.Remove(objectToRemove)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                },
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_RemoveLastActiveEvent()
        {
            var objectToRemove = new TimedEvent(new TextEvent("B")).SetTime(new MetricTimeSpan(0, 0, 0, 500), OnTheFlyChecksTempoMap);
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A")),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(200,
                        (playback, collection) => collection.Remove(objectToRemove)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(0)),
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(200)),
                },
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_RemoveAtSameTime_AfterCurrentTime_1()
        {
            var objectToRemove1 = new TimedEvent(new TextEvent("A")).SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap);
            var objectToRemove2 = new TimedEvent(new TextEvent("B")).SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap);
            var objectToRemove3 = new TimedEvent(new TextEvent("C")).SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 500), OnTheFlyChecksTempoMap),
                objectToRemove1,
                objectToRemove2,
                objectToRemove3,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(200,
                        (playback, collection) => collection.Remove(objectToRemove1)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(0)),
                    new ReceivedEvent(new TextEvent("B"), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new TextEvent("C"), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new TextEvent("B"), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new TextEvent("C"), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                },
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_RemoveAtSameTime_AfterCurrentTime_2()
        {
            var objectToRemove1 = new TimedEvent(new TextEvent("A")).SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap);
            var objectToRemove2 = new TimedEvent(new TextEvent("B")).SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap);
            var objectToRemove3 = new TimedEvent(new TextEvent("C")).SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 500), OnTheFlyChecksTempoMap),
                objectToRemove1,
                objectToRemove2,
                objectToRemove3,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(200,
                        (playback, collection) => collection.Remove(objectToRemove2)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(0)),
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new TextEvent("C"), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new TextEvent("C"), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                },
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_RemoveAtSameTime_AfterCurrentTime_3()
        {
            var objectToRemove1 = new TimedEvent(new TextEvent("A")).SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap);
            var objectToRemove2 = new TimedEvent(new TextEvent("B")).SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap);
            var objectToRemove3 = new TimedEvent(new TextEvent("C")).SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 500), OnTheFlyChecksTempoMap),
                objectToRemove1,
                objectToRemove2,
                objectToRemove3,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(200,
                        (playback, collection) => collection.Remove(objectToRemove3)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(0)),
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new TextEvent("B"), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new TextEvent("B"), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                },
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_RemoveAtSameTime_BeforeCurrentTime_1()
        {
            var objectToRemove1 = new TimedEvent(new TextEvent("A")).SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap);
            var objectToRemove2 = new TimedEvent(new TextEvent("B")).SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap);
            var objectToRemove3 = new TimedEvent(new TextEvent("C")).SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 700), OnTheFlyChecksTempoMap),
                objectToRemove1,
                objectToRemove2,
                objectToRemove3,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(500,
                        (playback, collection) => collection.Remove(objectToRemove1)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new TextEvent("B"), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new TextEvent("C"), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new TextEvent("B"), TimeSpan.FromMilliseconds(1000)),
                    new ReceivedEvent(new TextEvent("C"), TimeSpan.FromMilliseconds(1000)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1400)),
                },
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_RemoveAtSameTime_BeforeCurrentTime_2()
        {
            var objectToRemove1 = new TimedEvent(new TextEvent("A")).SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap);
            var objectToRemove2 = new TimedEvent(new TextEvent("B")).SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap);
            var objectToRemove3 = new TimedEvent(new TextEvent("C")).SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 700), OnTheFlyChecksTempoMap),
                objectToRemove1,
                objectToRemove2,
                objectToRemove3,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(500,
                        (playback, collection) => collection.Remove(objectToRemove2)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new TextEvent("B"), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new TextEvent("C"), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(1000)),
                    new ReceivedEvent(new TextEvent("C"), TimeSpan.FromMilliseconds(1000)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1400)),
                },
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_RemoveAtSameTime_BeforeCurrentTime_3()
        {
            var objectToRemove1 = new TimedEvent(new TextEvent("A")).SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap);
            var objectToRemove2 = new TimedEvent(new TextEvent("B")).SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap);
            var objectToRemove3 = new TimedEvent(new TextEvent("C")).SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 700), OnTheFlyChecksTempoMap),
                objectToRemove1,
                objectToRemove2,
                objectToRemove3,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(500,
                        (playback, collection) => collection.Remove(objectToRemove3)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new TextEvent("B"), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new TextEvent("C"), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(1000)),
                    new ReceivedEvent(new TextEvent("B"), TimeSpan.FromMilliseconds(1000)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1400)),
                },
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_RemoveProgramChange_AfterCurrentTime_1()
        {
            var objectToRemove = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)7)).SetTime(new MetricTimeSpan(0, 0, 0, 250), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), OnTheFlyChecksTempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(200,
                        (playback, collection) => collection.Remove(objectToRemove)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(1100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1600)),
                },
                setupPlayback: playback => playback.TrackProgram = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_RemoveProgramChange_AfterCurrentTime_2()
        {
            var objectToRemove = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)7)).SetTime(new MetricTimeSpan(0, 0, 0, 500), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), OnTheFlyChecksTempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(400,
                        (playback, collection) => collection.Remove(objectToRemove)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(1100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1600)),
                },
                setupPlayback: playback => playback.TrackProgram = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_RemoveProgramChange_BeforeCurrentTime_1()
        {
            var objectToRemove = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)7)).SetTime(new MetricTimeSpan(0, 0, 0, 50), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), OnTheFlyChecksTempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(200,
                        (playback, collection) => collection.Remove(objectToRemove)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)7), TimeSpan.FromMilliseconds(50)),
                    new ReceivedEvent(new ProgramChangeEvent(), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(1100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1600)),
                },
                setupPlayback: playback => playback.TrackProgram = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_RemoveProgramChange_BeforeCurrentTime_2()
        {
            var objectToRemove1 = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)7)).SetTime(new MetricTimeSpan(0, 0, 0, 50), OnTheFlyChecksTempoMap);
            var objectToRemove2 = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)8)).SetTime(new MetricTimeSpan(0, 0, 0, 100), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap),
                objectToRemove1,
                objectToRemove2,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(200,
                        (playback, collection) => collection.Remove(objectToRemove2)),
                    new PlaybackChanger(100,
                        (playback, collection) => collection.Remove(objectToRemove1)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)7), TimeSpan.FromMilliseconds(50)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)8), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)7), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new ProgramChangeEvent(), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(1100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1400)),
                },
                setupPlayback: playback => playback.TrackProgram = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_RemovePitchBend_AfterCurrentTime_1()
        {
            var objectToRemove = new TimedEvent(new PitchBendEvent(7000)).SetTime(new MetricTimeSpan(0, 0, 0, 250), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), OnTheFlyChecksTempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(200,
                        (playback, collection) => collection.Remove(objectToRemove)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(1100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1600)),
                },
                setupPlayback: playback => playback.TrackPitchValue = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_RemovePitchBend_AfterCurrentTime_2()
        {
            var objectToRemove = new TimedEvent(new PitchBendEvent(7000)).SetTime(new MetricTimeSpan(0, 0, 0, 500), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), OnTheFlyChecksTempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(400,
                        (playback, collection) => collection.Remove(objectToRemove)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(1100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1600)),
                },
                setupPlayback: playback => playback.TrackPitchValue = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_RemovePitchBend_BeforeCurrentTime_1()
        {
            var objectToRemove = new TimedEvent(new PitchBendEvent(7000)).SetTime(new MetricTimeSpan(0, 0, 0, 50), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), OnTheFlyChecksTempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(200,
                        (playback, collection) => collection.Remove(objectToRemove)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new PitchBendEvent(7000), TimeSpan.FromMilliseconds(50)),
                    new ReceivedEvent(new PitchBendEvent(), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(1100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1600)),
                },
                setupPlayback: playback => playback.TrackPitchValue = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_RemovePitchBend_BeforeCurrentTime_2()
        {
            var objectToRemove1 = new TimedEvent(new PitchBendEvent(7000)).SetTime(new MetricTimeSpan(0, 0, 0, 50), OnTheFlyChecksTempoMap);
            var objectToRemove2 = new TimedEvent(new PitchBendEvent(8000)).SetTime(new MetricTimeSpan(0, 0, 0, 100), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap),
                objectToRemove1,
                objectToRemove2,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(200,
                        (playback, collection) => collection.Remove(objectToRemove2)),
                    new PlaybackChanger(100,
                        (playback, collection) => collection.Remove(objectToRemove1)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new PitchBendEvent(7000), TimeSpan.FromMilliseconds(50)),
                    new ReceivedEvent(new PitchBendEvent(8000), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new PitchBendEvent(7000), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new PitchBendEvent(), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(1100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1400)),
                },
                setupPlayback: playback => playback.TrackPitchValue = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_RemoveControlChange_AfterCurrentTime_1()
        {
            var objectToRemove = new TimedEvent(new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)20)).SetTime(new MetricTimeSpan(0, 0, 0, 250), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), OnTheFlyChecksTempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(200,
                        (playback, collection) => collection.Remove(objectToRemove)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(1100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1600)),
                },
                setupPlayback: playback => playback.TrackControlValue = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_RemoveControlChange_AfterCurrentTime_2()
        {
            var objectToRemove = new TimedEvent(new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)20)).SetTime(new MetricTimeSpan(0, 0, 0, 500), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), OnTheFlyChecksTempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(400,
                        (playback, collection) => collection.Remove(objectToRemove)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(1100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1600)),
                },
                setupPlayback: playback => playback.TrackControlValue = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_RemoveControlChange_BeforeCurrentTime_1()
        {
            var objectToRemove = new TimedEvent(new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)20)).SetTime(new MetricTimeSpan(0, 0, 0, 50), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), OnTheFlyChecksTempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(200,
                        (playback, collection) => collection.Remove(objectToRemove)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)20), TimeSpan.FromMilliseconds(50)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)7, SevenBitNumber.MinValue), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(1100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1600)),
                },
                setupPlayback: playback => playback.TrackControlValue = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_RemoveControlChange_BeforeCurrentTime_2()
        {
            var objectToRemove1 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 50), OnTheFlyChecksTempoMap);
            var objectToRemove2 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)80)).SetTime(new MetricTimeSpan(0, 0, 0, 100), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap),
                objectToRemove1,
                objectToRemove2,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(200,
                        (playback, collection) => collection.Remove(objectToRemove2)),
                    new PlaybackChanger(100,
                        (playback, collection) => collection.Remove(objectToRemove1)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)70), TimeSpan.FromMilliseconds(50)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)80), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)70), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)7, SevenBitNumber.MinValue), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(1100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1400)),
                },
                setupPlayback: playback => playback.TrackControlValue = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_RemoveControlChange_BeforeCurrentTime_3()
        {
            var objectToRemove1 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 50), OnTheFlyChecksTempoMap);
            var objectToRemove2 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)8, (SevenBitNumber)80)).SetTime(new MetricTimeSpan(0, 0, 0, 100), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap),
                objectToRemove1,
                objectToRemove2,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(200,
                        (playback, collection) => collection.Remove(objectToRemove2)),
                    new PlaybackChanger(100,
                        (playback, collection) => collection.Remove(objectToRemove1)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)70), TimeSpan.FromMilliseconds(50)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)8, (SevenBitNumber)80), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)8, SevenBitNumber.MinValue), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)7, SevenBitNumber.MinValue), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(1100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1400)),
                },
                setupPlayback: playback => playback.TrackControlValue = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_RemoveControlChange_BeforeCurrentTime_4()
        {
            var objectToRemove1 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 50), OnTheFlyChecksTempoMap);
            var objectToRemove2 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)80) { Channel = (FourBitNumber)6 }).SetTime(new MetricTimeSpan(0, 0, 0, 100), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap),
                objectToRemove1,
                objectToRemove2,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(200,
                        (playback, collection) => collection.Remove(objectToRemove2)),
                    new PlaybackChanger(100,
                        (playback, collection) => collection.Remove(objectToRemove1)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)70), TimeSpan.FromMilliseconds(50)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)80) { Channel = (FourBitNumber)6 }, TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)7, SevenBitNumber.MinValue) { Channel = (FourBitNumber)6 }, TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)7, SevenBitNumber.MinValue), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(1100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1400)),
                },
                setupPlayback: playback => playback.TrackControlValue = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_RemoveNotes_1()
        {
            var objectToRemove = new Note((SevenBitNumber)50)
                .SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 200), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 700), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 100), OnTheFlyChecksTempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(600,
                        (playback, collection) => collection.Remove(objectToRemove)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(1500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1600)),
                },
                setupPlayback: playback => playback.TrackNotes = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_RemoveNotes_2()
        {
            var objectToRemove = new Note((SevenBitNumber)50)
                .SetTime(new MetricTimeSpan(0, 0, 0, 500), OnTheFlyChecksTempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 200), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 100), OnTheFlyChecksTempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(450,
                        (playback, collection) => collection.Remove(objectToRemove)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(750)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(850)),
                },
                setupPlayback: playback => playback.TrackNotes = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_RemoveNotes_3()
        {
            var objectToRemove = new Note((SevenBitNumber)50)
                .SetTime(new MetricTimeSpan(0, 0, 0, 100), OnTheFlyChecksTempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 500), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 200), OnTheFlyChecksTempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(200,
                        (playback, collection) => collection.Remove(objectToRemove)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(1200)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1400)),
                },
                setupPlayback: playback => playback.TrackNotes = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_RemoveNotes_4()
        {
            var objectToRemove1 = new Note((SevenBitNumber)50)
                .SetTime(new MetricTimeSpan(0, 0, 0, 100), OnTheFlyChecksTempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap);
            var objectToRemove2 = new Note((SevenBitNumber)60)
                .SetTime(new MetricTimeSpan(0, 0, 0, 450), OnTheFlyChecksTempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 100), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 700), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 100), OnTheFlyChecksTempoMap),
                objectToRemove1,
                objectToRemove2,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(200,
                        (playback, collection) => collection.Remove(objectToRemove1)),
                    new PlaybackChanger(300,
                        (playback, collection) => collection.Remove(objectToRemove2)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)60, Note.DefaultVelocity), TimeSpan.FromMilliseconds(450)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)60, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(1500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1600)),
                },
                setupPlayback: playback => playback.TrackNotes = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_RemoveNotes_5()
        {
            var objectToRemove = new Note((SevenBitNumber)50)
                .SetTime(new MetricTimeSpan(0, 0, 0, 100), OnTheFlyChecksTempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 400), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(200,
                        (playback, collection) => collection.Remove(objectToRemove)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(200)),
                },
                setupPlayback: playback => playback.TrackNotes = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_RemoveNotes_6()
        {
            var objectToRemove1 = new Note((SevenBitNumber)50)
                .SetTime(new MetricTimeSpan(0, 0, 0, 600), OnTheFlyChecksTempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 200), OnTheFlyChecksTempoMap);
            var objectToRemove2 = new Note((SevenBitNumber)80)
                .SetTime(new MetricTimeSpan(0, 0, 0, 600), OnTheFlyChecksTempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 200), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 200), OnTheFlyChecksTempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap),
                objectToRemove1,
                objectToRemove2,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(700,
                        (playback, collection) => collection.Remove(objectToRemove1)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)80, Note.DefaultVelocity), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)80, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(1000)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1300)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)80, Note.DefaultVelocity), TimeSpan.FromMilliseconds(1400)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)80, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1600)),
                },
                setupPlayback: playback => playback.TrackNotes = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_RemoveNoteEvents_1([Values] bool removeNoteOn)
        {
            var objectToRemove1 = new TimedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity))
                .SetTime(new MetricTimeSpan(0, 0, 0, 200), OnTheFlyChecksTempoMap);
            var objectToRemove2 = new TimedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity))
                .SetTime(new MetricTimeSpan(0, 0, 0, 400), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                objectToRemove1,
                objectToRemove2,
            };

            var notesStarted = new List<Note>();
            var notesFinished = new List<Note>();

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(100,
                        (playback, collection) => collection.Remove(removeNoteOn ? objectToRemove1 : objectToRemove2)),
                },
                expectedReceivedEvents: new ReceivedEvent[0]
                {
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = true;
                    playback.NotesPlaybackStarted += (_, e) => notesStarted.AddRange(e.Notes);
                    playback.NotesPlaybackFinished += (_, e) => notesFinished.AddRange(e.Notes);
                });

            CollectionAssert.AreEquivalent(notesStarted, notesFinished, "Inconsistent notes started/finished lists.");
            Assert.AreEqual(0, notesStarted.Count, "Invalid notes started count.");
            Assert.AreEqual(0, notesFinished.Count, "Invalid notes finished count.");
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_RemoveNoteEvents_2([Values] bool removeNoteOn)
        {
            var objectToRemove1 = new TimedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity))
                .SetTime(new MetricTimeSpan(0, 0, 0, 200), OnTheFlyChecksTempoMap);
            var objectToRemove2 = new TimedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity))
                .SetTime(new MetricTimeSpan(0, 0, 0, 400), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                objectToRemove1,
                objectToRemove2,
            };

            var notesStarted = new List<Note>();
            var notesFinished = new List<Note>();

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(300,
                        (playback, collection) => collection.Remove(removeNoteOn ? objectToRemove1 : objectToRemove2)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(300)),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = true;
                    playback.NotesPlaybackStarted += (_, e) => notesStarted.AddRange(e.Notes);
                    playback.NotesPlaybackFinished += (_, e) => notesFinished.AddRange(e.Notes);
                });

            CollectionAssert.AreEquivalent(notesStarted, notesFinished, "Inconsistent notes started/finished lists.");
            Assert.AreEqual(1, notesStarted.Count, "Invalid notes started count.");
            Assert.AreEqual(1, notesFinished.Count, "Invalid notes finished count.");
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_RemoveNoteEvents_3()
        {
            var objectToRemove1 = new TimedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity))
                .SetTime(new MetricTimeSpan(0, 0, 0, 200), OnTheFlyChecksTempoMap);
            var objectToRemove2 = new TimedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity))
                .SetTime(new MetricTimeSpan(0, 0, 0, 400), OnTheFlyChecksTempoMap);
            
            var objectToAdd1 = new TimedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity))
                .SetTime(new MetricTimeSpan(0, 0, 0, 300), OnTheFlyChecksTempoMap);
            var objectToAdd2 = new TimedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity))
                .SetTime(new MetricTimeSpan(0, 0, 0, 600), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                objectToRemove1,
                objectToRemove2,
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 700), OnTheFlyChecksTempoMap),
            };

            var notesStarted = new List<Note>();
            var notesFinished = new List<Note>();

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(50,
                        (playback, collection) => collection.Remove(objectToRemove1)),
                    new PlaybackChanger(50,
                        (playback, collection) => collection.Add(objectToAdd1)),
                    new PlaybackChanger(250,
                        (playback, collection) => collection.Remove(objectToRemove2)),
                    new PlaybackChanger(50,
                        (playback, collection) => collection.Add(objectToAdd2)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(350)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new TextEvent("END"), TimeSpan.FromMilliseconds(700)),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = true;
                    playback.NotesPlaybackStarted += (_, e) => notesStarted.AddRange(e.Notes);
                    playback.NotesPlaybackFinished += (_, e) => notesFinished.AddRange(e.Notes);
                });

            CollectionAssert.AreEquivalent(notesStarted, notesFinished, "Inconsistent notes started/finished lists.");
            Assert.AreEqual(2, notesStarted.Count, "Invalid notes started count.");
            Assert.AreEqual(2, notesFinished.Count, "Invalid notes finished count.");
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_RemoveNoteEvents_4()
        {
            var objectToRemove1 = new TimedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity))
                .SetTime(new MetricTimeSpan(0, 0, 0, 200), OnTheFlyChecksTempoMap);
            var objectToRemove2 = new TimedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity))
                .SetTime(new MetricTimeSpan(0, 0, 0, 400), OnTheFlyChecksTempoMap);

            var initialObjects = new ITimedObject[]
            {
                objectToRemove1,
                objectToRemove2,
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 500), OnTheFlyChecksTempoMap),
            };

            var notesStarted = new List<Note>();
            var notesFinished = new List<Note>();

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(100,
                        (playback, collection) => collection.Remove(objectToRemove1, objectToRemove2)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new TextEvent("END"), TimeSpan.FromMilliseconds(500)),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = true;
                    playback.NotesPlaybackStarted += (_, e) => notesStarted.AddRange(e.Notes);
                    playback.NotesPlaybackFinished += (_, e) => notesFinished.AddRange(e.Notes);
                });

            CollectionAssert.AreEquivalent(notesStarted, notesFinished, "Inconsistent notes started/finished lists.");
            Assert.AreEqual(0, notesStarted.Count, "Invalid notes started count.");
            Assert.AreEqual(0, notesFinished.Count, "Invalid notes finished count.");
        }

        #endregion
    }
}
