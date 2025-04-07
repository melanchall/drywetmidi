using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;
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
            var objectToRemove = new TimedEvent(new TextEvent("A")).SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap);
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 1), TempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(500,
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
            var objectToRemove = new TimedEvent(new TextEvent("A")).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 1), TempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(500,
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
            var objectToRemove = new TimedEvent(new TextEvent("B")).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A")),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200,
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
            var objectToRemove1 = new TimedEvent(new TextEvent("A")).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var objectToRemove2 = new TimedEvent(new TextEvent("B")).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var objectToRemove3 = new TimedEvent(new TextEvent("C")).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var objectsToRemove = new ITimedObject[]
            {
                objectToRemove1,
                objectToRemove2,
                objectToRemove3,
            };

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
                objectToRemove1,
                objectToRemove2,
                objectToRemove3,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200,
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
            var objectToRemove1 = new TimedEvent(new TextEvent("A")).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var objectToRemove2 = new TimedEvent(new TextEvent("B")).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var objectToRemove3 = new TimedEvent(new TextEvent("C")).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var objectsToRemove = new ITimedObject[]
            {
                objectToRemove1,
                objectToRemove2,
                objectToRemove3,
            };

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
                objectToRemove1,
                objectToRemove2,
                objectToRemove3,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(500,
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
            var objectToRemove = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)7)).SetTime(new MetricTimeSpan(0, 0, 0, 250), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200,
                        (playback, collection) => collection.Remove(objectToRemove)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                });
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_RemoveProgramChange_AfterCurrentTime_2()
        {
            var objectToRemove = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)7)).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(400,
                        (playback, collection) => collection.Remove(objectToRemove)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                });
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_RemoveProgramChange_BeforeCurrentTime_1()
        {
            var objectToRemove = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)7)).SetTime(new MetricTimeSpan(0, 0, 0, 50), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200,
                        (playback, collection) => collection.Remove(objectToRemove)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)7), TimeSpan.FromMilliseconds(50)),
                    new ReceivedEvent(new ProgramChangeEvent(), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                });
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_RemoveProgramChange_BeforeCurrentTime_2()
        {
            var objectToRemove1 = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)7)).SetTime(new MetricTimeSpan(0, 0, 0, 50), TempoMap);
            var objectToRemove2 = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)8)).SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap),
                objectToRemove1,
                objectToRemove2,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200,
                        (playback, collection) => collection.Remove(objectToRemove2)),
                    new DynamicPlaybackAction(100,
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
                });
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_RemovePitchBend_AfterCurrentTime_1()
        {
            var objectToRemove = new TimedEvent(new PitchBendEvent(7000)).SetTime(new MetricTimeSpan(0, 0, 0, 250), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200,
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
            var objectToRemove = new TimedEvent(new PitchBendEvent(7000)).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(400,
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
            var objectToRemove = new TimedEvent(new PitchBendEvent(7000)).SetTime(new MetricTimeSpan(0, 0, 0, 50), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200,
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
            var objectToRemove1 = new TimedEvent(new PitchBendEvent(7000)).SetTime(new MetricTimeSpan(0, 0, 0, 50), TempoMap);
            var objectToRemove2 = new TimedEvent(new PitchBendEvent(8000)).SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap),
                objectToRemove1,
                objectToRemove2,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200,
                        (playback, collection) => collection.Remove(objectToRemove2)),
                    new DynamicPlaybackAction(100,
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
            var objectToRemove = new TimedEvent(new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)20)).SetTime(new MetricTimeSpan(0, 0, 0, 250), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200,
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
            var objectToRemove = new TimedEvent(new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)20)).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(400,
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
            var objectToRemove = new TimedEvent(new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)20)).SetTime(new MetricTimeSpan(0, 0, 0, 50), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200,
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
            var objectToRemove1 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 50), TempoMap);
            var objectToRemove2 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)80)).SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap),
                objectToRemove1,
                objectToRemove2,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200,
                        (playback, collection) => collection.Remove(objectToRemove2)),
                    new DynamicPlaybackAction(100,
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
            var objectToRemove1 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 50), TempoMap);
            var objectToRemove2 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)8, (SevenBitNumber)80)).SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap),
                objectToRemove1,
                objectToRemove2,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200,
                        (playback, collection) => collection.Remove(objectToRemove2)),
                    new DynamicPlaybackAction(100,
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
            var objectToRemove1 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 50), TempoMap);
            var objectToRemove2 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)80) { Channel = (FourBitNumber)6 }).SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap),
                objectToRemove1,
                objectToRemove2,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200,
                        (playback, collection) => collection.Remove(objectToRemove2)),
                    new DynamicPlaybackAction(100,
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
                .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 200), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 100), TempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(600,
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
                .SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 200), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 100), TempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(450,
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
                .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 200), TempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200,
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
                .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var objectToRemove2 = new Note((SevenBitNumber)60)
                .SetTime(new MetricTimeSpan(0, 0, 0, 450), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 100), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 100), TempoMap),
                objectToRemove1,
                objectToRemove2,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200,
                        (playback, collection) => collection.Remove(objectToRemove1)),
                    new DynamicPlaybackAction(300,
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
                .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 400), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200,
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
                .SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 200), TempoMap);
            var objectToRemove2 = new Note((SevenBitNumber)80)
                .SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 200), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap),
                objectToRemove1,
                objectToRemove2,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(700,
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
            var objectToRemove = new TimedEvent(new TextEvent("A")).SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap);
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(300,
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
            var objectToRemove = new TimedEvent(new TextEvent("A")).SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap);
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(300,
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
            var objectToRemove = new TimedEvent(new TextEvent("B")).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A")),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200,
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
            var objectToRemove1 = new TimedEvent(new TextEvent("A")).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var objectToRemove2 = new TimedEvent(new TextEvent("B")).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var objectToRemove3 = new TimedEvent(new TextEvent("C")).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
                objectToRemove1,
                objectToRemove2,
                objectToRemove3,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200,
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
            var objectToRemove1 = new TimedEvent(new TextEvent("A")).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var objectToRemove2 = new TimedEvent(new TextEvent("B")).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var objectToRemove3 = new TimedEvent(new TextEvent("C")).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
                objectToRemove1,
                objectToRemove2,
                objectToRemove3,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200,
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
            var objectToRemove1 = new TimedEvent(new TextEvent("A")).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var objectToRemove2 = new TimedEvent(new TextEvent("B")).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var objectToRemove3 = new TimedEvent(new TextEvent("C")).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
                objectToRemove1,
                objectToRemove2,
                objectToRemove3,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200,
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
            var objectToRemove1 = new TimedEvent(new TextEvent("A")).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var objectToRemove2 = new TimedEvent(new TextEvent("B")).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var objectToRemove3 = new TimedEvent(new TextEvent("C")).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
                objectToRemove1,
                objectToRemove2,
                objectToRemove3,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(500,
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
            var objectToRemove1 = new TimedEvent(new TextEvent("A")).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var objectToRemove2 = new TimedEvent(new TextEvent("B")).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var objectToRemove3 = new TimedEvent(new TextEvent("C")).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
                objectToRemove1,
                objectToRemove2,
                objectToRemove3,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(500,
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
            var objectToRemove1 = new TimedEvent(new TextEvent("A")).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var objectToRemove2 = new TimedEvent(new TextEvent("B")).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var objectToRemove3 = new TimedEvent(new TextEvent("C")).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
                objectToRemove1,
                objectToRemove2,
                objectToRemove3,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(500,
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
            var objectToRemove = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)7)).SetTime(new MetricTimeSpan(0, 0, 0, 250), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200,
                        (playback, collection) => collection.Remove(objectToRemove)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(1100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1600)),
                },
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_RemoveProgramChange_AfterCurrentTime_2()
        {
            var objectToRemove = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)7)).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(400,
                        (playback, collection) => collection.Remove(objectToRemove)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(1100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1600)),
                },
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_RemoveProgramChange_BeforeCurrentTime_1()
        {
            var objectToRemove = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)7)).SetTime(new MetricTimeSpan(0, 0, 0, 50), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200,
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
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_RemoveProgramChange_BeforeCurrentTime_2()
        {
            var objectToRemove1 = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)7)).SetTime(new MetricTimeSpan(0, 0, 0, 50), TempoMap);
            var objectToRemove2 = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)8)).SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap),
                objectToRemove1,
                objectToRemove2,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200,
                        (playback, collection) => collection.Remove(objectToRemove2)),
                    new DynamicPlaybackAction(100,
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
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_RemovePitchBend_AfterCurrentTime_1()
        {
            var objectToRemove = new TimedEvent(new PitchBendEvent(7000)).SetTime(new MetricTimeSpan(0, 0, 0, 250), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200,
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
            var objectToRemove = new TimedEvent(new PitchBendEvent(7000)).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(400,
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
            var objectToRemove = new TimedEvent(new PitchBendEvent(7000)).SetTime(new MetricTimeSpan(0, 0, 0, 50), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200,
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
            var objectToRemove1 = new TimedEvent(new PitchBendEvent(7000)).SetTime(new MetricTimeSpan(0, 0, 0, 50), TempoMap);
            var objectToRemove2 = new TimedEvent(new PitchBendEvent(8000)).SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap),
                objectToRemove1,
                objectToRemove2,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200,
                        (playback, collection) => collection.Remove(objectToRemove2)),
                    new DynamicPlaybackAction(100,
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
            var objectToRemove = new TimedEvent(new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)20)).SetTime(new MetricTimeSpan(0, 0, 0, 250), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200,
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
            var objectToRemove = new TimedEvent(new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)20)).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(400,
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
            var objectToRemove = new TimedEvent(new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)20)).SetTime(new MetricTimeSpan(0, 0, 0, 50), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200,
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
            var objectToRemove1 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 50), TempoMap);
            var objectToRemove2 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)80)).SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap),
                objectToRemove1,
                objectToRemove2,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200,
                        (playback, collection) => collection.Remove(objectToRemove2)),
                    new DynamicPlaybackAction(100,
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
            var objectToRemove1 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 50), TempoMap);
            var objectToRemove2 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)8, (SevenBitNumber)80)).SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap),
                objectToRemove1,
                objectToRemove2,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200,
                        (playback, collection) => collection.Remove(objectToRemove2)),
                    new DynamicPlaybackAction(100,
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
            var objectToRemove1 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 50), TempoMap);
            var objectToRemove2 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)80) { Channel = (FourBitNumber)6 }).SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap),
                objectToRemove1,
                objectToRemove2,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200,
                        (playback, collection) => collection.Remove(objectToRemove2)),
                    new DynamicPlaybackAction(100,
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
                .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 200), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 100), TempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(600,
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
                .SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 200), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 100), TempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(450,
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
                .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 200), TempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200,
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
                .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var objectToRemove2 = new Note((SevenBitNumber)60)
                .SetTime(new MetricTimeSpan(0, 0, 0, 450), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 100), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 100), TempoMap),
                objectToRemove1,
                objectToRemove2,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200,
                        (playback, collection) => collection.Remove(objectToRemove1)),
                    new DynamicPlaybackAction(300,
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
                .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 400), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200,
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
                .SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 200), TempoMap);
            var objectToRemove2 = new Note((SevenBitNumber)80)
                .SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 200), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap),
                objectToRemove1,
                objectToRemove2,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(700,
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
                .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap);
            var objectToRemove2 = new TimedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity))
                .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap);

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
                    new DynamicPlaybackAction(100,
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
            ClassicAssert.AreEqual(0, notesStarted.Count, "Invalid notes started count.");
            ClassicAssert.AreEqual(0, notesFinished.Count, "Invalid notes finished count.");
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_RemoveNoteEvents_2([Values] bool removeNoteOn)
        {
            var objectToRemove1 = new TimedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity))
                .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap);
            var objectToRemove2 = new TimedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity))
                .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap);

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
                    new DynamicPlaybackAction(300,
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
            ClassicAssert.AreEqual(1, notesStarted.Count, "Invalid notes started count.");
            ClassicAssert.AreEqual(1, notesFinished.Count, "Invalid notes finished count.");
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_RemoveNoteEvents_3()
        {
            var objectToRemove1 = new TimedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity))
                .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap);
            var objectToRemove2 = new TimedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity))
                .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap);
            
            var objectToAdd1 = new TimedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity))
                .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var objectToAdd2 = new TimedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity))
                .SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                objectToRemove1,
                objectToRemove2,
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
            };

            var notesStarted = new List<Note>();
            var notesFinished = new List<Note>();

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(50,
                        (playback, collection) => collection.Remove(objectToRemove1)),
                    new DynamicPlaybackAction(50,
                        (playback, collection) => collection.Add(objectToAdd1)),
                    new DynamicPlaybackAction(250,
                        (playback, collection) => collection.Remove(objectToRemove2)),
                    new DynamicPlaybackAction(50,
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
            ClassicAssert.AreEqual(2, notesStarted.Count, "Invalid notes started count.");
            ClassicAssert.AreEqual(2, notesFinished.Count, "Invalid notes finished count.");
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_RemoveNoteEvents_4()
        {
            var objectToRemove1 = new TimedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity))
                .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap);
            var objectToRemove2 = new TimedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity))
                .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                objectToRemove1,
                objectToRemove2,
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
            };

            var notesStarted = new List<Note>();
            var notesFinished = new List<Note>();

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(100,
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
            ClassicAssert.AreEqual(0, notesStarted.Count, "Invalid notes started count.");
            ClassicAssert.AreEqual(0, notesFinished.Count, "Invalid notes finished count.");
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Remove_SetTempo_1()
        {
            var objectToRemove = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap);
            
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 100), TempoMap),
                objectToRemove,
                new Note((SevenBitNumber)90)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(150, (playback, collection) =>
                    {
                        collection.Remove(objectToRemove);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(150), "after remove");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(900)),
                },
                additionalChecks: playback => MidiAsserts.AreEqual(
                    TempoMap,
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Remove_SetTempo_2()
        {
            var objectToRemove = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 100), TempoMap),
                objectToRemove,
                new Note((SevenBitNumber)90)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(400, (playback, collection) =>
                    {
                        collection.Remove(objectToRemove);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(600), "after remove");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                },
                additionalChecks: playback => MidiAsserts.AreEqual(
                    TempoMap,
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Remove_SetTempo_3()
        {
            var objectToRemove = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 100), TempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(50, (playback, collection) =>
                    {
                        collection.Remove(objectToRemove);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(50), "after remove");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(100)),
                },
                additionalChecks: playback => MidiAsserts.AreEqual(
                    TempoMap,
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Remove_SetTempo_4()
        {
            var objectToRemove = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 100), TempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200, (playback, collection) =>
                    {
                        collection.Remove(objectToRemove);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(200), "after remove");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(100)),
                },
                additionalChecks: playback => MidiAsserts.AreEqual(
                    TempoMap,
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Remove_SetTempo_5()
        {
            var objectToRemove = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 100), TempoMap),
                objectToRemove,
                new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 4))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap),
                new Note((SevenBitNumber)90)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(50, (playback, collection) =>
                    {
                        collection.Remove(objectToRemove);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(50), "after remove");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 4), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(325)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(450)),
                },
                additionalChecks: playback => MidiAsserts.AreEqual(
                    AddTempoChanges(TempoMap, (TimeSpan.FromMilliseconds(300), new Tempo(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 4))),
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Remove_SetTempo_6()
        {
            var objectToRemove = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 100), TempoMap),
                objectToRemove,
                new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 4))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap),
                new Note((SevenBitNumber)90)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(220, (playback, collection) =>
                    {
                        collection.Remove(objectToRemove);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(240), "after remove");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 4), TimeSpan.FromMilliseconds(280)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(305)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(430)),
                },
                additionalChecks: playback => MidiAsserts.AreEqual(
                    AddTempoChanges(TempoMap, (TimeSpan.FromMilliseconds(300), new Tempo(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 4))),
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Remove_SetTempo_7()
        {
            var objectToRemove = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 100), TempoMap),
                objectToRemove,
                new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 4))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap),
                new Note((SevenBitNumber)90)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.Remove(objectToRemove);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(350), "after remove");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 4), TimeSpan.FromMilliseconds(250)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(275)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                },
                additionalChecks: playback => MidiAsserts.AreEqual(
                    AddTempoChanges(TempoMap, (TimeSpan.FromMilliseconds(300), new Tempo(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 4))),
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Remove_SetTempo_8()
        {
            var objectToRemove = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 4))
                .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 100), TempoMap),
                new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap),
                objectToRemove,
                new Note((SevenBitNumber)90)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(50, (playback, collection) =>
                    {
                        collection.Remove(objectToRemove);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(50), "after remove");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(550)),
                },
                additionalChecks: playback => MidiAsserts.AreEqual(
                    AddTempoChanges(TempoMap, (TimeSpan.FromMilliseconds(200), new Tempo(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))),
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Remove_SetTempo_9()
        {
            var objectToRemove = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 4))
                .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 100), TempoMap),
                new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap),
                objectToRemove,
                new Note((SevenBitNumber)90)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(220, (playback, collection) =>
                    {
                        collection.Remove(objectToRemove);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(220), "after remove");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(550)),
                },
                additionalChecks: playback => MidiAsserts.AreEqual(
                    AddTempoChanges(TempoMap, (TimeSpan.FromMilliseconds(200), new Tempo(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))),
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Remove_SetTempo_10()
        {
            var objectToRemove = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 4))
                .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 100), TempoMap),
                new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap),
                objectToRemove,
                new Note((SevenBitNumber)90)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.Remove(objectToRemove);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(350), "after remove");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 4), TimeSpan.FromMilliseconds(250)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(275)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(500)),
                },
                additionalChecks: playback => MidiAsserts.AreEqual(
                    AddTempoChanges(TempoMap, (TimeSpan.FromMilliseconds(200), new Tempo(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))),
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Remove_TimeSignature_1()
        {
            var objectToRemove = new TimedEvent(new TimeSignatureEvent(3, 4))
                .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 100), TempoMap),
                objectToRemove,
                new Note((SevenBitNumber)90)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(150, (playback, collection) =>
                    {
                        collection.Remove(objectToRemove);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(150), "after remove");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(900)),
                },
                additionalChecks: playback => MidiAsserts.AreEqual(
                    TempoMap,
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Remove_TimeSignature_2()
        {
            var objectToRemove = new TimedEvent(new TimeSignatureEvent(3, 4))
                .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 100), TempoMap),
                objectToRemove,
                new Note((SevenBitNumber)90)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(400, (playback, collection) =>
                    {
                        collection.Remove(objectToRemove);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(400), "after remove");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new TimeSignatureEvent(3, 4), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(900)),
                },
                additionalChecks: playback => MidiAsserts.AreEqual(
                    TempoMap,
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Remove_TimeSignature_3()
        {
            var objectToRemove = new TimedEvent(new TimeSignatureEvent(3, 4))
                .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 100), TempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(50, (playback, collection) =>
                    {
                        collection.Remove(objectToRemove);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(50), "after remove");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(100)),
                },
                additionalChecks: playback => MidiAsserts.AreEqual(
                    TempoMap,
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Remove_TimeSignature_4()
        {
            var objectToRemove = new TimedEvent(new TimeSignatureEvent(3, 4))
                .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 100), TempoMap),
                objectToRemove,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200, (playback, collection) =>
                    {
                        collection.Remove(objectToRemove);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(200), "after remove");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(100)),
                },
                additionalChecks: playback => MidiAsserts.AreEqual(
                    TempoMap,
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Remove_TimeSignature_5()
        {
            var objectToRemove = new TimedEvent(new TimeSignatureEvent(3, 4))
                .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 100), TempoMap),
                objectToRemove,
                new TimedEvent(new TimeSignatureEvent(3, 8))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap),
                new Note((SevenBitNumber)90)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(50, (playback, collection) =>
                    {
                        collection.Remove(objectToRemove);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(50), "after remove");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new TimeSignatureEvent(3, 8), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(900)),
                },
                additionalChecks: playback => MidiAsserts.AreEqual(
                    AddTimeSignatureChanges(TempoMap, (TimeSpan.FromMilliseconds(300), new TimeSignature(3, 8))),
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Remove_TimeSignature_6()
        {
            var objectToRemove = new TimedEvent(new TimeSignatureEvent(3, 4))
                .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 100), TempoMap),
                objectToRemove,
                new TimedEvent(new TimeSignatureEvent(3, 8))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap),
                new Note((SevenBitNumber)90)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(250, (playback, collection) =>
                    {
                        collection.Remove(objectToRemove);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(250), "after remove");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new TimeSignatureEvent(3, 4), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new TimeSignatureEvent(3, 8), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(900)),
                },
                additionalChecks: playback => MidiAsserts.AreEqual(
                    AddTimeSignatureChanges(TempoMap, (TimeSpan.FromMilliseconds(300), new TimeSignature(3, 8))),
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Remove_TimeSignature_7()
        {
            var objectToRemove = new TimedEvent(new TimeSignatureEvent(3, 4))
                .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 100), TempoMap),
                objectToRemove,
                new TimedEvent(new TimeSignatureEvent(3, 8))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap),
                new Note((SevenBitNumber)90)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.Remove(objectToRemove);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(300), "after remove");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new TimeSignatureEvent(3, 4), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new TimeSignatureEvent(3, 8), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(900)),
                },
                additionalChecks: playback => MidiAsserts.AreEqual(
                    AddTimeSignatureChanges(TempoMap, (TimeSpan.FromMilliseconds(300), new TimeSignature(3, 8))),
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Remove_TimeSignature_8()
        {
            var objectToRemove = new TimedEvent(new TimeSignatureEvent(3, 8))
                .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 100), TempoMap),
                new TimedEvent(new TimeSignatureEvent(3, 4))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap),
                objectToRemove,
                new Note((SevenBitNumber)90)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(50, (playback, collection) =>
                    {
                        collection.Remove(objectToRemove);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(50), "after remove");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new TimeSignatureEvent(3, 4), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(900)),
                },
                additionalChecks: playback => MidiAsserts.AreEqual(
                    AddTimeSignatureChanges(TempoMap, (TimeSpan.FromMilliseconds(200), new TimeSignature(3, 4))),
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Remove_TimeSignature_9()
        {
            var objectToRemove = new TimedEvent(new TimeSignatureEvent(3, 8))
                .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 100), TempoMap),
                new TimedEvent(new TimeSignatureEvent(3, 4))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap),
                objectToRemove,
                new Note((SevenBitNumber)90)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(220, (playback, collection) =>
                    {
                        collection.Remove(objectToRemove);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(220), "after remove");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new TimeSignatureEvent(3, 4), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(900)),
                },
                additionalChecks: playback => MidiAsserts.AreEqual(
                    AddTimeSignatureChanges(TempoMap, (TimeSpan.FromMilliseconds(200), new TimeSignature(3, 4))),
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Remove_TimeSignature_10()
        {
            var objectToRemove = new TimedEvent(new TimeSignatureEvent(3, 8))
                .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 100), TempoMap),
                new TimedEvent(new TimeSignatureEvent(3, 4))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap),
                objectToRemove,
                new Note((SevenBitNumber)90)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.Remove(objectToRemove);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(300), "after remove");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new TimeSignatureEvent(3, 4), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(900)),
                },
                additionalChecks: playback => MidiAsserts.AreEqual(
                    AddTimeSignatureChanges(TempoMap, (TimeSpan.FromMilliseconds(200), new TimeSignature(3, 4))),
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        #endregion
    }
}
