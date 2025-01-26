using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
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
        public void CheckPlaybackDataChangesOnTheFly_AddEventWithinNote_AfterCurrentTime_1()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 1), TempoMap),
            };
            var objectToAdd = new TimedEvent(new TextEvent("A")).SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(500,
                        (playback, collection) => collection.Add(objectToAdd)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                });
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddEventWithinNote_AfterCurrentTime_2()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 1), TempoMap),
            };
            var objectToAdd1 = new TimedEvent(new TextEvent("A")).SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap);
            var objectToAdd2 = new TimedEvent(new TextEvent("B")).SetTime(new MetricTimeSpan(0, 0, 0, 900), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(500,
                        (playback, collection) => collection.Add(objectToAdd1, objectToAdd2)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new TextEvent("B"), TimeSpan.FromMilliseconds(900)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                });
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddEventWithinNote_AfterCurrentTime_3()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 1), TempoMap),
            };
            var objectToAdd1 = new TimedEvent(new TextEvent("A")).SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap);
            var objectToAdd2 = new TimedEvent(new TextEvent("B")).SetTime(new MetricTimeSpan(0, 0, 0, 900), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(500,
                        (playback, collection) => collection.Add(objectToAdd1)),
                    new PlaybackChanger(300,
                        (playback, collection) => collection.Add(objectToAdd2)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new TextEvent("B"), TimeSpan.FromMilliseconds(900)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                });
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddEventWithinNote_BeforeCurrentTime()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 1), TempoMap),
            };
            var objectToAdd = new TimedEvent(new TextEvent("A")).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(500,
                        (playback, collection) => collection.Add(objectToAdd)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                });
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddNotesInAdvance()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 900), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 100), TempoMap),
            };

            var objectToAdd1 = new Note((SevenBitNumber)50)
                .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 100), TempoMap);
            var objectToAdd2 = new Note((SevenBitNumber)60)
                .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 100), TempoMap);
            var objectToAdd3 = new Note((SevenBitNumber)70) { Channel = (FourBitNumber)4 }
                .SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 150), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(100,
                        (playback, collection) => collection.Add(objectToAdd1)),
                    new PlaybackChanger(200,
                        (playback, collection) => collection.Add(objectToAdd2)),
                    new PlaybackChanger(300,
                        (playback, collection) => collection.Add(objectToAdd3)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)60, Note.DefaultVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)60, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(850)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(900)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                });
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddProgramChangeWithinNote_AfterCurrentTime_1()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 1), TempoMap),
            };
            var objectToAdd = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(500,
                        (playback, collection) => collection.Add(objectToAdd)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                });
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddProgramChangeWithinNote_AfterCurrentTime_2()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 1), TempoMap),
            };
            var objectToAdd1 = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap);
            var objectToAdd2 = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)50)).SetTime(new MetricTimeSpan(0, 0, 0, 900), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(500,
                        (playback, collection) => collection.Add(objectToAdd1, objectToAdd2)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)50), TimeSpan.FromMilliseconds(900)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                });
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddProgramChangeWithinNote_AfterCurrentTime_3()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 1), TempoMap),
            };
            var objectToAdd1 = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap);
            var objectToAdd2 = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)70) { Channel = (FourBitNumber)4 }).SetTime(new MetricTimeSpan(0, 0, 0, 900), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(500,
                        (playback, collection) => collection.Add(objectToAdd1, objectToAdd2)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(900)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                });
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddProgramChangeWithinNote_AfterCurrentTime_4()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 1), TempoMap),
            };
            var objectToAdd1 = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap);
            var objectToAdd2 = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)50)).SetTime(new MetricTimeSpan(0, 0, 0, 900), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(500,
                        (playback, collection) => collection.Add(objectToAdd1)),
                    new PlaybackChanger(300,
                        (playback, collection) => collection.Add(objectToAdd2)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)50), TimeSpan.FromMilliseconds(900)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                });
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddProgramChangeWithinNote_BeforeCurrentTime_1()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 1), TempoMap),
            };
            var objectToAdd = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(500,
                        (playback, collection) => collection.Add(objectToAdd)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                });
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddProgramChangeWithinNote_BeforeCurrentTime_2()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 1), TempoMap),
            };
            var objectToAdd1 = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var objectToAdd2 = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)50)).SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(500,
                        (playback, collection) => collection.Add(objectToAdd1, objectToAdd2)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)50), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                });
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddProgramChangeWithinNote_BeforeCurrentTime_3()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 1), TempoMap),
            };
            var objectToAdd1 = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var objectToAdd2 = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)70) { Channel = (FourBitNumber)4 }).SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(500,
                        (playback, collection) => collection.Add(objectToAdd1, objectToAdd2)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                });
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddProgramChangeWithinNote_MoveToTime_1()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 1), TempoMap),
            };
            var objectToAdd = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(500,
                        (playback, collection) =>
                        {
                            collection.Add(objectToAdd);
                            playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 700));
                        }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                });
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddProgramChangeWithinNote_MoveToTime_2()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 1), TempoMap),
            };
            var objectToAdd1 = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap);
            var objectToAdd2 = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)50)).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(500,
                        (playback, collection) =>
                        {
                            collection.Add(objectToAdd1, objectToAdd2);
                            playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 700));
                        }),
                    new PlaybackChanger(100,
                        (playback, collection) =>
                        {
                            playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 400));
                        }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)50), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)50), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1200)),
                });
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddPitchBendWithinNote_AfterCurrentTime_1()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 1), TempoMap),
            };
            var objectToAdd = new TimedEvent(new PitchBendEvent(5000)).SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(500,
                        (playback, collection) => collection.Add(objectToAdd)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new PitchBendEvent(5000), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                },
                setupPlayback: playback => playback.TrackPitchValue = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddPitchBendWithinNote_AfterCurrentTime_2()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 1), TempoMap),
            };
            var objectToAdd1 = new TimedEvent(new PitchBendEvent(7000)).SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap);
            var objectToAdd2 = new TimedEvent(new PitchBendEvent(5000)).SetTime(new MetricTimeSpan(0, 0, 0, 900), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(500,
                        (playback, collection) => collection.Add(objectToAdd1, objectToAdd2)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new PitchBendEvent(7000), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new PitchBendEvent(5000), TimeSpan.FromMilliseconds(900)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                },
                setupPlayback: playback => playback.TrackPitchValue = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddPitchBendWithinNote_AfterCurrentTime_3()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 1), TempoMap),
            };
            var objectToAdd1 = new TimedEvent(new PitchBendEvent(7000)).SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap);
            var objectToAdd2 = new TimedEvent(new PitchBendEvent(7000) { Channel = (FourBitNumber)4 }).SetTime(new MetricTimeSpan(0, 0, 0, 900), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(500,
                        (playback, collection) => collection.Add(objectToAdd1, objectToAdd2)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new PitchBendEvent(7000), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new PitchBendEvent(7000) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(900)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                },
                setupPlayback: playback => playback.TrackPitchValue = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddPitchBendWithinNote_AfterCurrentTime_4()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 1), TempoMap),
            };
            var objectToAdd1 = new TimedEvent(new PitchBendEvent(7000)).SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap);
            var objectToAdd2 = new TimedEvent(new PitchBendEvent(5000)).SetTime(new MetricTimeSpan(0, 0, 0, 900), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(500,
                        (playback, collection) => collection.Add(objectToAdd1)),
                    new PlaybackChanger(300,
                        (playback, collection) => collection.Add(objectToAdd2)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new PitchBendEvent(7000), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new PitchBendEvent(5000), TimeSpan.FromMilliseconds(900)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                },
                setupPlayback: playback => playback.TrackPitchValue = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddPitchBendWithinNote_BeforeCurrentTime_1()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 1), TempoMap),
            };
            var objectToAdd = new TimedEvent(new PitchBendEvent(7000)).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(500,
                        (playback, collection) => collection.Add(objectToAdd)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new PitchBendEvent(7000), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                },
                setupPlayback: playback => playback.TrackPitchValue = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddPitchBendWithinNote_BeforeCurrentTime_2()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 1), TempoMap),
            };
            var objectToAdd1 = new TimedEvent(new PitchBendEvent(7000)).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var objectToAdd2 = new TimedEvent(new PitchBendEvent(5000)).SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(500,
                        (playback, collection) => collection.Add(objectToAdd1, objectToAdd2)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new PitchBendEvent(5000), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                },
                setupPlayback: playback => playback.TrackPitchValue = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddPitchBendWithinNote_BeforeCurrentTime_3()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 1), TempoMap),
            };
            var objectToAdd1 = new TimedEvent(new PitchBendEvent(7000)).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var objectToAdd2 = new TimedEvent(new PitchBendEvent(7000) { Channel = (FourBitNumber)4 }).SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(500,
                        (playback, collection) => collection.Add(objectToAdd1, objectToAdd2)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new PitchBendEvent(7000), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new PitchBendEvent(7000) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                },
                setupPlayback: playback => playback.TrackPitchValue = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddPitchBendWithinNote_MoveToTime_1()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 1), TempoMap),
            };
            var objectToAdd = new TimedEvent(new PitchBendEvent(7000)).SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(500, (playback, collection) =>
                        {
                            collection.Add(objectToAdd);
                            playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 700));
                        }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new PitchBendEvent(7000), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                },
                setupPlayback: playback => playback.TrackPitchValue = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddPitchBendWithinNote_MoveToTime_2()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 1), TempoMap),
            };
            var objectToAdd1 = new TimedEvent(new PitchBendEvent(7000)).SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap);
            var objectToAdd2 = new TimedEvent(new PitchBendEvent(5000)).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(500,
                        (playback, collection) =>
                        {
                            collection.Add(objectToAdd1, objectToAdd2);
                            playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 700));
                        }),
                    new PlaybackChanger(100,
                        (playback, collection) =>
                        {
                            playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 400));
                        }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new PitchBendEvent(5000), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new PitchBendEvent(7000), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new PitchBendEvent(5000), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new PitchBendEvent(7000), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1200)),
                },
                setupPlayback: playback => playback.TrackPitchValue = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddControlChangeWithinNote_AfterCurrentTime_1()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 1), TempoMap),
            };
            var objectToAdd = new TimedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(500,
                        (playback, collection) => collection.Add(objectToAdd)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                },
                setupPlayback: playback => playback.TrackControlValue = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddControlChangeWithinNote_AfterCurrentTime_2()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 1), TempoMap),
            };
            var objectToAdd1 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap);
            var objectToAdd2 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)50)).SetTime(new MetricTimeSpan(0, 0, 0, 900), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(500,
                        (playback, collection) => collection.Add(objectToAdd1, objectToAdd2)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)50), TimeSpan.FromMilliseconds(900)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                },
                setupPlayback: playback => playback.TrackControlValue = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddControlChangeWithinNote_AfterCurrentTime_3()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 1), TempoMap),
            };
            var objectToAdd1 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap);
            var objectToAdd2 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70) { Channel = (FourBitNumber)4 }).SetTime(new MetricTimeSpan(0, 0, 0, 900), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(500,
                        (playback, collection) => collection.Add(objectToAdd1, objectToAdd2)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(900)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                },
                setupPlayback: playback => playback.TrackControlValue = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddControlChangeWithinNote_AfterCurrentTime_4()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 1), TempoMap),
            };
            var objectToAdd1 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap);
            var objectToAdd2 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)50)).SetTime(new MetricTimeSpan(0, 0, 0, 900), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(500,
                        (playback, collection) => collection.Add(objectToAdd1)),
                    new PlaybackChanger(300,
                        (playback, collection) => collection.Add(objectToAdd2)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)50), TimeSpan.FromMilliseconds(900)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                },
                setupPlayback: playback => playback.TrackControlValue = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddControlChangeWithinNote_AfterCurrentTime_5()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 1), TempoMap),
            };
            var objectToAdd1 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap);
            var objectToAdd2 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70) { Channel = (FourBitNumber)4 }).SetTime(new MetricTimeSpan(0, 0, 0, 900), TempoMap);
            var objectToAdd3 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)40, (SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 800), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(500,
                        (playback, collection) => collection.Add(objectToAdd1, objectToAdd2, objectToAdd3)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)40, (SevenBitNumber)70), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(900)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                },
                setupPlayback: playback => playback.TrackControlValue = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddControlChangeWithinNote_BeforeCurrentTime_1()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 1), TempoMap),
            };
            var objectToAdd = new TimedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(500,
                        (playback, collection) => collection.Add(objectToAdd)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                },
                setupPlayback: playback => playback.TrackControlValue = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddControlChangeWithinNote_BeforeCurrentTime_2()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 1), TempoMap),
            };
            var objectToAdd1 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var objectToAdd2 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)50)).SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(500,
                        (playback, collection) => collection.Add(objectToAdd1, objectToAdd2)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)50), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                },
                setupPlayback: playback => playback.TrackControlValue = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddControlChangeWithinNote_BeforeCurrentTime_3()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 1), TempoMap),
            };
            var objectToAdd1 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var objectToAdd2 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70) { Channel = (FourBitNumber)4 }).SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(500,
                        (playback, collection) => collection.Add(objectToAdd1, objectToAdd2)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                },
                setupPlayback: playback => playback.TrackControlValue = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddControlChangeWithinNote_MoveToTime_1()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 1), TempoMap),
            };
            var objectToAdd = new TimedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(500,
                        (playback, collection) =>
                        {
                            collection.Add(objectToAdd);
                            playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 700));
                        }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                },
                setupPlayback: playback => playback.TrackControlValue = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddControlChangeWithinNote_MoveToTime_2()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 1), TempoMap),
            };
            var objectToAdd1 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap);
            var objectToAdd2 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)50)).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(500,
                        (playback, collection) =>
                        {
                            collection.Add(objectToAdd1, objectToAdd2);
                            playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 700));
                        }),
                    new PlaybackChanger(100,
                        (playback, collection) =>
                        {
                            playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 400));
                        }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)50), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)50), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1200)),
                },
                setupPlayback: playback => playback.TrackControlValue = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddNote_1([Values(1.0, 0.5, 0.1)] double scaleFactor)
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, (int)(1000 * scaleFactor)), TempoMap),
            };
            var objectToAdd = new Note((SevenBitNumber)50)
                .SetTime(new MetricTimeSpan(0, 0, 0, (int)(300 * scaleFactor)), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, (int)(500 * scaleFactor)), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger((int)(400 * scaleFactor),
                        (playback, collection) =>
                        {
                            collection.Add(objectToAdd);
                            CheckDuration(TimeSpan.FromMilliseconds((int)(1000 * scaleFactor)), playback);
                        }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds((int)(400 * scaleFactor))),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds((int)(800 * scaleFactor))),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds((int)(1000 * scaleFactor))),
                },
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddNote_2([Values(1.0, 0.5, 0.1)] double scaleFactor)
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, (int)(1000 * scaleFactor)), TempoMap),
            };
            var objectToAdd1 = new Note((SevenBitNumber)50)
                .SetTime(new MetricTimeSpan(0, 0, 0, (int)(300 * scaleFactor)), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, (int)(500 * scaleFactor)), TempoMap);
            var objectToAdd2 = new Note((SevenBitNumber)50) { Channel = (FourBitNumber)6 }
                .SetTime(new MetricTimeSpan(0, 0, 0, (int)(350 * scaleFactor)), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, (int)(500 * scaleFactor)), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger((int)(400 * scaleFactor),
                        (playback, collection) =>
                        {
                            collection.Add(objectToAdd1, objectToAdd2);
                            CheckDuration(TimeSpan.FromMilliseconds((int)(1000 * scaleFactor)), playback);
                        }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds((int)(400 * scaleFactor))),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity) { Channel = (FourBitNumber)6 }, TimeSpan.FromMilliseconds((int)(400 * scaleFactor))),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds((int)(800 * scaleFactor))),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity) { Channel = (FourBitNumber)6 }, TimeSpan.FromMilliseconds((int)(850 * scaleFactor))),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds((int)(1000 * scaleFactor))),
                },
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddNote_3([Values(1.0, 0.5, 0.1)] double scaleFactor)
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, (int)(500 * scaleFactor)), TempoMap),
            };
            var objectToAdd1 = new Note((SevenBitNumber)50)
                .SetTime(new MetricTimeSpan(0, 0, 0, (int)(600 * scaleFactor)), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, (int)(300 * scaleFactor)), TempoMap);
            var objectToAdd2 = new Note((SevenBitNumber)80)
                .SetTime(new MetricTimeSpan(0, 0, 0, (int)(1000 * scaleFactor)), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, (int)(300 * scaleFactor)), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger((int)(200 * scaleFactor),
                        (playback, collection) =>
                        {
                            collection.Add(objectToAdd1);
                            CheckDuration(TimeSpan.FromMilliseconds((int)(900 * scaleFactor)), playback);

                            playback.MoveToTime(new MetricTimeSpan(0, 0, 0, (int)(700 * scaleFactor)));
                        }),
                    new PlaybackChanger((int)(100 * scaleFactor),
                        (playback, collection) =>
                        {
                            collection.Add(objectToAdd2);
                            CheckDuration(TimeSpan.FromMilliseconds((int)(1300 * scaleFactor)), playback);

                            playback.MoveToTime(new MetricTimeSpan(0, 0, 0, (int)(1100 * scaleFactor)));
                        }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds((int)(200 * scaleFactor))),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds((int)(200 * scaleFactor))),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds((int)(300 * scaleFactor))),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)80, Note.DefaultVelocity), TimeSpan.FromMilliseconds((int)(300 * scaleFactor))),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)80, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds((int)(500 * scaleFactor))),
                },
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddNote_4([Values(1.0, 0.5, 0.1)] double scaleFactor)
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, (int)(500 * scaleFactor)), TempoMap),
            };
            var objectToAdd1 = new Note((SevenBitNumber)50)
                .SetTime(new MetricTimeSpan(0, 0, 0, (int)(600 * scaleFactor)), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, (int)(300 * scaleFactor)), TempoMap);
            var objectToAdd2 = new Note((SevenBitNumber)80)
                .SetTime(new MetricTimeSpan(0, 0, 0, (int)(1000 * scaleFactor)), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, (int)(300 * scaleFactor)), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger((int)(200 * scaleFactor),
                        (playback, collection) =>
                        {
                            collection.Add(objectToAdd1, objectToAdd2);
                            CheckDuration(TimeSpan.FromMilliseconds((int)(1300 * scaleFactor)), playback);
                        }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds((int)(500 * scaleFactor))),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds((int)(600 * scaleFactor))),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds((int)(900 * scaleFactor))),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)80, Note.DefaultVelocity), TimeSpan.FromMilliseconds((int)(1000 * scaleFactor))),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)80, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds((int)(1300 * scaleFactor))),
                },
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddNote_5()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap),
            };
            var objectToAdd = new Note((SevenBitNumber)50)
                .SetTime(new MetricTimeSpan(0, 0, 0), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(100,
                        (playback, collection) => collection.Add(objectToAdd)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                },
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddNote_6()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap),
            };
            var objectToAdd = new Note((SevenBitNumber)50)
                .SetTime(new MetricTimeSpan(0, 0, 0), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 1000), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(100,
                        (playback, collection) => collection.Add(objectToAdd)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                },
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_AddEventWithinNote_AfterCurrentTime_1()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
            };
            var objectToAdd = new TimedEvent(new TextEvent("A")).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(200,
                        (playback, collection) => collection.Add(objectToAdd)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                },
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_AddEventWithinNote_AfterCurrentTime_2()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
            };
            var objectToAdd1 = new TimedEvent(new TextEvent("A")).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var objectToAdd2 = new TimedEvent(new TextEvent("B")).SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(200,
                        (playback, collection) => collection.Add(objectToAdd1, objectToAdd2)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new TextEvent("B"), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new TextEvent("B"), TimeSpan.FromMilliseconds(900)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                },
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_AddEventWithinNote_AfterCurrentTime_3()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
            };
            var objectToAdd1 = new TimedEvent(new TextEvent("A")).SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap);
            var objectToAdd2 = new TimedEvent(new TextEvent("B")).SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(200,
                        (playback, collection) => collection.Add(objectToAdd1)),
                    new PlaybackChanger(300,
                        (playback, collection) => collection.Add(objectToAdd2)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new TextEvent("B"), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(1100)),
                    new ReceivedEvent(new TextEvent("B"), TimeSpan.FromMilliseconds(1300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1400)),
                },
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_AddEventWithinNote_BeforeOrEqualToCurrentTime()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 1), TempoMap),
            };
            var objectToAdd = new TimedEvent(new TextEvent("A")).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(500,
                        (playback, collection) => collection.Add(objectToAdd)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(1000)),
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(1000 + 300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(2000)),
                },
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_AddNotesInAdvance()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 900), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 100), TempoMap),
            };

            var objectToAdd1 = new Note((SevenBitNumber)50)
                .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 100), TempoMap);
            var objectToAdd2 = new Note((SevenBitNumber)60)
                .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 100), TempoMap);
            var objectToAdd3 = new Note((SevenBitNumber)70) { Channel = (FourBitNumber)4 }
                .SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 150), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(100,
                        (playback, collection) => collection.Add(objectToAdd1)),
                    new PlaybackChanger(200,
                        (playback, collection) => collection.Add(objectToAdd2)),
                    new PlaybackChanger(300,
                        (playback, collection) => collection.Add(objectToAdd3)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)60, Note.DefaultVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)60, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(850)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(900)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(1200)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1300)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)60, Note.DefaultVelocity), TimeSpan.FromMilliseconds(1400)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)60, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1500)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(1700)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(1850)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(1900)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(2000)),
                },
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_AddProgramChangeWithinNote_AfterCurrentTime_1()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 600), TempoMap),
            };
            var objectToAdd = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(100,
                        (playback, collection) => collection.Add(objectToAdd)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70), TimeSpan.FromMilliseconds(900)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1200)),
                },
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_AddProgramChangeWithinNote_AfterCurrentTime_2()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 600), TempoMap),
            };
            var objectToAdd1 = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var objectToAdd2 = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)50)).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(100,
                        (playback, collection) => collection.Add(objectToAdd1, objectToAdd2)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)50), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70), TimeSpan.FromMilliseconds(900)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)50), TimeSpan.FromMilliseconds(1100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1200)),
                },
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_AddProgramChangeWithinNote_AfterCurrentTime_3()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 600), TempoMap),
            };
            var objectToAdd1 = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var objectToAdd2 = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)70) { Channel = (FourBitNumber)4 }).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(100,
                        (playback, collection) => collection.Add(objectToAdd1, objectToAdd2)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70), TimeSpan.FromMilliseconds(900)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(1100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1200)),
                },
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_AddProgramChangeWithinNote_AfterCurrentTime_4()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 600), TempoMap),
            };
            var objectToAdd1 = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var objectToAdd2 = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)50)).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(100,
                        (playback, collection) => collection.Add(objectToAdd1)),
                    new PlaybackChanger(300,
                        (playback, collection) => collection.Add(objectToAdd2)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)50), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70), TimeSpan.FromMilliseconds(900)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)50), TimeSpan.FromMilliseconds(1100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1200)),
                },
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_AddProgramChangeWithinNote_BeforeCurrentTime_1()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 600), TempoMap),
            };
            var objectToAdd = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(500,
                        (playback, collection) => collection.Add(objectToAdd)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70), TimeSpan.FromMilliseconds(900)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1200)),
                },
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_AddProgramChangeWithinNote_BeforeCurrentTime_2()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 600), TempoMap),
            };
            var objectToAdd1 = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var objectToAdd2 = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)50)).SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(500,
                        (playback, collection) => collection.Add(objectToAdd1, objectToAdd2)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)50), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70), TimeSpan.FromMilliseconds(900)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)50), TimeSpan.FromMilliseconds(1000)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1200)),
                },
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_AddProgramChangeWithinNote_BeforeCurrentTime_3()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 600), TempoMap),
            };
            var objectToAdd1 = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var objectToAdd2 = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)70) { Channel = (FourBitNumber)4 }).SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(500,
                        (playback, collection) => collection.Add(objectToAdd1, objectToAdd2)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70), TimeSpan.FromMilliseconds(900)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(1000)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1200)),
                },
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_AddProgramChangeWithinNote_MoveToTime_1()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 600), TempoMap),
            };
            var objectToAdd = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(300,
                        (playback, collection) =>
                        {
                            collection.Add(objectToAdd);
                            playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 500));
                        }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                },
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_AddProgramChangeWithinNote_MoveToTime_2()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 1), TempoMap),
            };
            var objectToAdd1 = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap);
            var objectToAdd2 = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)50)).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(500,
                        (playback, collection) =>
                        {
                            collection.Add(objectToAdd1, objectToAdd2);
                            playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 700));
                        }),
                    new PlaybackChanger(100,
                        (playback, collection) =>
                        {
                            playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 400));
                        }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)50), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)50), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1200)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(1200)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)50), TimeSpan.FromMilliseconds(1500)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70), TimeSpan.FromMilliseconds(1800)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(2200)),
                },
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_AddPitchBendWithinNote_AfterCurrentTime_1()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 600), TempoMap),
            };
            var objectToAdd = new TimedEvent(new PitchBendEvent(5000)).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(100,
                        (playback, collection) => collection.Add(objectToAdd)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new PitchBendEvent(5000), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new PitchBendEvent(5000), TimeSpan.FromMilliseconds(900)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1200)),
                },
                setupPlayback: playback => playback.TrackPitchValue = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_AddPitchBendWithinNote_AfterCurrentTime_2()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 600), TempoMap),
            };
            var objectToAdd1 = new TimedEvent(new PitchBendEvent(7000)).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var objectToAdd2 = new TimedEvent(new PitchBendEvent(5000)).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(100,
                        (playback, collection) => collection.Add(objectToAdd1, objectToAdd2)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new PitchBendEvent(7000), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new PitchBendEvent(5000), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new PitchBendEvent(7000), TimeSpan.FromMilliseconds(900)),
                    new ReceivedEvent(new PitchBendEvent(5000), TimeSpan.FromMilliseconds(1100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1200)),
                },
                setupPlayback: playback => playback.TrackPitchValue = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_AddPitchBendWithinNote_AfterCurrentTime_3()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 600), TempoMap),
            };
            var objectToAdd1 = new TimedEvent(new PitchBendEvent(7000)).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var objectToAdd2 = new TimedEvent(new PitchBendEvent(7000) { Channel = (FourBitNumber)4 }).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(100,
                        (playback, collection) => collection.Add(objectToAdd1, objectToAdd2)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new PitchBendEvent(7000), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new PitchBendEvent(7000) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new PitchBendEvent(7000), TimeSpan.FromMilliseconds(900)),
                    new ReceivedEvent(new PitchBendEvent(7000) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(1100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1200)),
                },
                setupPlayback: playback => playback.TrackPitchValue = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_AddPitchBendWithinNote_AfterCurrentTime_4()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 600), TempoMap),
            };
            var objectToAdd1 = new TimedEvent(new PitchBendEvent(7000)).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var objectToAdd2 = new TimedEvent(new PitchBendEvent(5000)).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(100,
                        (playback, collection) => collection.Add(objectToAdd1)),
                    new PlaybackChanger(300,
                        (playback, collection) => collection.Add(objectToAdd2)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new PitchBendEvent(7000), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new PitchBendEvent(5000), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new PitchBendEvent(7000), TimeSpan.FromMilliseconds(900)),
                    new ReceivedEvent(new PitchBendEvent(5000), TimeSpan.FromMilliseconds(1100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1200)),
                },
                setupPlayback: playback => playback.TrackPitchValue = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_AddPitchBendWithinNote_BeforeCurrentTime_1()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
            };
            var objectToAdd = new TimedEvent(new PitchBendEvent(7000)).SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(300,
                        (playback, collection) => collection.Add(objectToAdd)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new PitchBendEvent(7000), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new PitchBendEvent(7000), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                },
                setupPlayback: playback => playback.TrackPitchValue = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_AddPitchBendWithinNote_BeforeCurrentTime_2()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 600), TempoMap),
            };
            var objectToAdd1 = new TimedEvent(new PitchBendEvent(7000)).SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);
            var objectToAdd2 = new TimedEvent(new PitchBendEvent(5000)).SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(300,
                        (playback, collection) => collection.Add(objectToAdd1, objectToAdd2)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new PitchBendEvent(5000), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new PitchBendEvent(7000), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new PitchBendEvent(5000), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1200)),
                },
                setupPlayback: playback => playback.TrackPitchValue = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_AddPitchBendWithinNote_BeforeCurrentTime_3()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 600), TempoMap),
            };
            var objectToAdd1 = new TimedEvent(new PitchBendEvent(7000)).SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);
            var objectToAdd2 = new TimedEvent(new PitchBendEvent(7000) { Channel = (FourBitNumber)4 }).SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(300,
                        (playback, collection) => collection.Add(objectToAdd1, objectToAdd2)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new PitchBendEvent(7000), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new PitchBendEvent(7000) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new PitchBendEvent(7000), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new PitchBendEvent(7000) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1200)),
                },
                setupPlayback: playback => playback.TrackPitchValue = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_AddPitchBendWithinNote_MoveToTime_1()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
            };
            var objectToAdd = new TimedEvent(new PitchBendEvent(7000)).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(200, (playback, collection) =>
                        {
                            collection.Add(objectToAdd);
                            playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 400));
                        }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new PitchBendEvent(7000), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new PitchBendEvent(7000), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                },
                setupPlayback: playback => playback.TrackPitchValue = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_AddPitchBendWithinNote_MoveToTime_2()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 1), TempoMap),
            };
            var objectToAdd1 = new TimedEvent(new PitchBendEvent(7000)).SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap);
            var objectToAdd2 = new TimedEvent(new PitchBendEvent(5000)).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(500,
                        (playback, collection) =>
                        {
                            collection.Add(objectToAdd1, objectToAdd2);
                            playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 700));
                        }),
                    new PlaybackChanger(100,
                        (playback, collection) =>
                        {
                            playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 400));
                        }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new PitchBendEvent(5000), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new PitchBendEvent(7000), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new PitchBendEvent(5000), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new PitchBendEvent(7000), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1200)),

                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(1200)),
                    new ReceivedEvent(new PitchBendEvent(5000), TimeSpan.FromMilliseconds(1500)),
                    new ReceivedEvent(new PitchBendEvent(7000), TimeSpan.FromMilliseconds(1800)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(2200)),
                },
                setupPlayback: playback => playback.TrackPitchValue = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_AddControlChangeWithinNote_AfterCurrentTime_1()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 600), TempoMap),
            };
            var objectToAdd = new TimedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(100,
                        (playback, collection) => collection.Add(objectToAdd)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70), TimeSpan.FromMilliseconds(900)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1200)),
                },
                setupPlayback: playback => playback.TrackControlValue = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_AddControlChangeWithinNote_AfterCurrentTime_2()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 600), TempoMap),
            };
            var objectToAdd1 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var objectToAdd2 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)50)).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(100,
                        (playback, collection) => collection.Add(objectToAdd1, objectToAdd2)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)50), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70), TimeSpan.FromMilliseconds(900)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)50), TimeSpan.FromMilliseconds(1100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1200)),
                },
                setupPlayback: playback => playback.TrackControlValue = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_AddControlChangeWithinNote_AfterCurrentTime_3()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 600), TempoMap),
            };
            var objectToAdd1 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var objectToAdd2 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70) { Channel = (FourBitNumber)4 }).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(100,
                        (playback, collection) => collection.Add(objectToAdd1, objectToAdd2)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70), TimeSpan.FromMilliseconds(900)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(1100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1200)),
                },
                setupPlayback: playback => playback.TrackControlValue = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_AddControlChangeWithinNote_AfterCurrentTime_4()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 600), TempoMap),
            };
            var objectToAdd1 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var objectToAdd2 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)50)).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(100,
                        (playback, collection) => collection.Add(objectToAdd1)),
                    new PlaybackChanger(300,
                        (playback, collection) => collection.Add(objectToAdd2)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)50), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70), TimeSpan.FromMilliseconds(900)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)50), TimeSpan.FromMilliseconds(1100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1200)),
                },
                setupPlayback: playback => playback.TrackControlValue = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_AddControlChangeWithinNote_AfterCurrentTime_5()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 600), TempoMap),
            };
            var objectToAdd1 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var objectToAdd2 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70) { Channel = (FourBitNumber)4 }).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);
            var objectToAdd3 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)40, (SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(100,
                        (playback, collection) => collection.Add(objectToAdd1, objectToAdd2, objectToAdd3)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)40, (SevenBitNumber)70), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70), TimeSpan.FromMilliseconds(900)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)40, (SevenBitNumber)70), TimeSpan.FromMilliseconds(1000)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(1100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1200)),
                },
                setupPlayback: playback => playback.TrackControlValue = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_AddControlChangeWithinNote_BeforeCurrentTime_1()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 600), TempoMap),
            };
            var objectToAdd = new TimedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(300,
                        (playback, collection) => collection.Add(objectToAdd)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1200)),
                },
                setupPlayback: playback => playback.TrackControlValue = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_AddControlChangeWithinNote_BeforeCurrentTime_2()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
            };
            var objectToAdd1 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);
            var objectToAdd2 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)50)).SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(300,
                        (playback, collection) => collection.Add(objectToAdd1, objectToAdd2)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)50), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)50), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                },
                setupPlayback: playback => playback.TrackControlValue = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_AddControlChangeWithinNote_BeforeCurrentTime_3()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
            };
            var objectToAdd1 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);
            var objectToAdd2 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70) { Channel = (FourBitNumber)4 }).SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(300,
                        (playback, collection) => collection.Add(objectToAdd1, objectToAdd2)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                },
                setupPlayback: playback => playback.TrackControlValue = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_AddControlChangeWithinNote_MoveToTime_1()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 600), TempoMap),
            };
            var objectToAdd = new TimedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(200,
                        (playback, collection) =>
                        {
                            collection.Add(objectToAdd);
                            playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 400));
                        }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                },
                setupPlayback: playback => playback.TrackControlValue = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_AddControlChangeWithinNote_MoveToTime_2()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 1), TempoMap),
            };
            var objectToAdd1 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap);
            var objectToAdd2 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)50)).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(500,
                        (playback, collection) =>
                        {
                            collection.Add(objectToAdd1, objectToAdd2);
                            playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 700));
                        }),
                    new PlaybackChanger(100,
                        (playback, collection) =>
                        {
                            playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 400));
                        }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)50), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)50), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1200)),

                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(1200)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)50), TimeSpan.FromMilliseconds(1500)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)50, (SevenBitNumber)70), TimeSpan.FromMilliseconds(1800)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(2200)),
                },
                setupPlayback: playback => playback.TrackControlValue = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_AddNote_1([Values(1.0, 0.5, 0.1)] double scaleFactor)
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, (int)(500 * scaleFactor)), TempoMap),
            };
            var objectToAdd = new Note((SevenBitNumber)50)
                .SetTime(new MetricTimeSpan(0, 0, 0, (int)(100 * scaleFactor)), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, (int)(300 * scaleFactor)), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger((int)(200 * scaleFactor),
                        (playback, collection) =>
                        {
                            collection.Add(objectToAdd);
                            CheckDuration(TimeSpan.FromMilliseconds((int)(500 * scaleFactor)), playback);
                        }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds((int)(200 * scaleFactor))),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds((int)(400 * scaleFactor))),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds((int)(500 * scaleFactor))),

                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds((int)(500 * scaleFactor))),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds((int)(600 * scaleFactor))),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds((int)(900 * scaleFactor))),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds((int)(1000 * scaleFactor))),
                },
                setupPlayback: playback => playback.TrackNotes = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_AddNote_2([Values(1.0, 0.5, 0.1)] double scaleFactor)
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, (int)(1000 * scaleFactor)), TempoMap),
            };
            var objectToAdd1 = new Note((SevenBitNumber)50)
                .SetTime(new MetricTimeSpan(0, 0, 0, (int)(300 * scaleFactor)), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, (int)(500 * scaleFactor)), TempoMap);
            var objectToAdd2 = new Note((SevenBitNumber)50) { Channel = (FourBitNumber)6 }
                .SetTime(new MetricTimeSpan(0, 0, 0, (int)(350 * scaleFactor)), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, (int)(500 * scaleFactor)), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger((int)(400 * scaleFactor),
                        (playback, collection) =>
                        {
                            collection.Add(objectToAdd1, objectToAdd2);
                            CheckDuration(TimeSpan.FromMilliseconds((int)(1000 * scaleFactor)), playback);
                        }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds((int)(400 * scaleFactor))),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity) { Channel = (FourBitNumber)6 }, TimeSpan.FromMilliseconds((int)(400 * scaleFactor))),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds((int)(800 * scaleFactor))),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity) { Channel = (FourBitNumber)6 }, TimeSpan.FromMilliseconds((int)(850 * scaleFactor))),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds((int)(1000 * scaleFactor))),

                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds((int)(1000 * scaleFactor))),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds((int)(1300 * scaleFactor))),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity) { Channel = (FourBitNumber)6 }, TimeSpan.FromMilliseconds((int)(1350 * scaleFactor))),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds((int)(1800 * scaleFactor))),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity) { Channel = (FourBitNumber)6 }, TimeSpan.FromMilliseconds((int)(1850 * scaleFactor))),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds((int)(2000 * scaleFactor))),
                },
                setupPlayback: playback => playback.TrackNotes = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_AddNote_3([Values(1.0, 0.5, 0.1)] double scaleFactor)
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, (int)(500 * scaleFactor)), TempoMap),
            };
            var objectToAdd1 = new Note((SevenBitNumber)50)
                .SetTime(new MetricTimeSpan(0, 0, 0, (int)(600 * scaleFactor)), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, (int)(300 * scaleFactor)), TempoMap);
            var objectToAdd2 = new Note((SevenBitNumber)80)
                .SetTime(new MetricTimeSpan(0, 0, 0, (int)(1000 * scaleFactor)), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, (int)(300 * scaleFactor)), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger((int)(200 * scaleFactor),
                        (playback, collection) =>
                        {
                            collection.Add(objectToAdd1);
                            CheckDuration(TimeSpan.FromMilliseconds((int)(900 * scaleFactor)), playback);

                            playback.MoveToTime(new MetricTimeSpan(0, 0, 0, (int)(700 * scaleFactor)));
                        }),
                    new PlaybackChanger((int)(100 * scaleFactor),
                        (playback, collection) =>
                        {
                            collection.Add(objectToAdd2);
                            CheckDuration(TimeSpan.FromMilliseconds((int)(1300 * scaleFactor)), playback);

                            playback.MoveToTime(new MetricTimeSpan(0, 0, 0, (int)(1100 * scaleFactor)));
                        }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds((int)(200 * scaleFactor))),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds((int)(200 * scaleFactor))),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds((int)(300 * scaleFactor))),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)80, Note.DefaultVelocity), TimeSpan.FromMilliseconds((int)(300 * scaleFactor))),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)80, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds((int)(500 * scaleFactor))),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds((int)(500 * scaleFactor))),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds((int)(1000 * scaleFactor))),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds((int)(1100 * scaleFactor))),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds((int)(1400 * scaleFactor))),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)80, Note.DefaultVelocity), TimeSpan.FromMilliseconds((int)(1500 * scaleFactor))),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)80, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds((int)(1800 * scaleFactor))),
                },
                setupPlayback: playback => playback.TrackNotes = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_AddNote_4([Values(1.0, 0.5, 0.1)] double scaleFactor)
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, (int)(500 * scaleFactor)), TempoMap),
            };
            var objectToAdd1 = new Note((SevenBitNumber)50)
                .SetTime(new MetricTimeSpan(0, 0, 0, (int)(600 * scaleFactor)), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, (int)(300 * scaleFactor)), TempoMap);
            var objectToAdd2 = new Note((SevenBitNumber)80)
                .SetTime(new MetricTimeSpan(0, 0, 0, (int)(1000 * scaleFactor)), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, (int)(300 * scaleFactor)), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger((int)(200 * scaleFactor),
                        (playback, collection) =>
                        {
                            collection.Add(objectToAdd1, objectToAdd2);
                            CheckDuration(TimeSpan.FromMilliseconds((int)(1300 * scaleFactor)), playback);
                        }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds((int)(500 * scaleFactor))),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds((int)(600 * scaleFactor))),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds((int)(900 * scaleFactor))),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)80, Note.DefaultVelocity), TimeSpan.FromMilliseconds((int)(1000 * scaleFactor))),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)80, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds((int)(1300 * scaleFactor))),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds((int)(1300 * scaleFactor))),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds((int)(1800 * scaleFactor))),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds((int)(1900 * scaleFactor))),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds((int)(2200 * scaleFactor))),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)80, Note.DefaultVelocity), TimeSpan.FromMilliseconds((int)(2300 * scaleFactor))),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)80, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds((int)(2600 * scaleFactor))),
                },
                setupPlayback: playback => playback.TrackNotes = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_AddNote_5()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap),
            };
            var objectToAdd = new Note((SevenBitNumber)50)
                .SetTime(new MetricTimeSpan(0, 0, 0), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(100,
                        (playback, collection) => collection.Add(objectToAdd)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(1100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1400)),
                },
                setupPlayback: playback => playback.TrackNotes = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_AddNote_6()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap),
            };
            var objectToAdd = new Note((SevenBitNumber)50)
                .SetTime(new MetricTimeSpan(0, 0, 0), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 1000), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(100,
                        (playback, collection) => collection.Add(objectToAdd)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(1000)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(1400)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1700)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(2000)),
                },
                setupPlayback: playback => playback.TrackNotes = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddNoteOnEvent()
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap),
            };

            var objectToAdd = new TimedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity))
                .SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(100,
                        (playback, collection) => collection.Add(objectToAdd)),
                },
                expectedReceivedEvents: Array.Empty<ReceivedEvent>(),
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddNoteOffEvent()
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap),
            };

            var objectToAdd = new TimedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultVelocity))
                .SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(100,
                        (playback, collection) => collection.Add(objectToAdd)),
                },
                expectedReceivedEvents: Array.Empty<ReceivedEvent>(),
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddNoteEvents_1()
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
            };

            var objectToAdd1 = new TimedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity))
                .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap);
            var objectToAdd2 = new TimedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity))
                .SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);

            var notesStarted = new List<Note>();
            var notesFinished = new List<Note>();

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(50,
                        (playback, collection) => collection.Add(objectToAdd1)),
                    new PlaybackChanger(50,
                        (playback, collection) => collection.Add(objectToAdd2)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new TextEvent("END"), TimeSpan.FromMilliseconds(700)),
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
        public void CheckPlaybackDataChangesOnTheFly_AddNoteEvents_2()
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
            };

            var objectToAdd1 = new TimedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity))
                .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap);
            var objectToAdd2 = new TimedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity))
                .SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);

            var notesStarted = new List<Note>();
            var notesFinished = new List<Note>();

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(50,
                        (playback, collection) => collection.Add(objectToAdd1)),
                    new PlaybackChanger(200,
                        (playback, collection) => collection.Add(objectToAdd2)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(250)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new TextEvent("END"), TimeSpan.FromMilliseconds(700)),
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
        public void CheckPlaybackDataChangesOnTheFly_AddNoteEvents_3()
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
            };

            var objectToAdd1 = new TimedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity))
                .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap);
            var objectToAdd2 = new TimedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity))
                .SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);

            var notesStarted = new List<Note>();
            var notesFinished = new List<Note>();

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(50,
                        (playback, collection) => collection.Add(objectToAdd1, objectToAdd2)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new TextEvent("END"), TimeSpan.FromMilliseconds(700)),
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
        public void CheckPlaybackDataChangesOnTheFly_AddNoteEvents_4()
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
            };

            var objectToAdd1 = new TimedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity))
                .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap);
            var objectToAdd2 = new TimedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity))
                .SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);

            var notesStarted = new List<Note>();
            var notesFinished = new List<Note>();

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(300,
                        (playback, collection) => collection.Add(objectToAdd1, objectToAdd2)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new TextEvent("END"), TimeSpan.FromMilliseconds(700)),
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
        public void CheckPlaybackDataChangesOnTheFly_AddNoteEvents_5()
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
                new TimedEvent(new TextEvent("NEAREND"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 550), TempoMap),
            };

            var objectToAdd1 = new TimedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity))
                .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap);
            var objectToAdd2 = new TimedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity))
                .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap);
            var objectToAdd3 = new TimedEvent(new NoteOnEvent((SevenBitNumber)50, (SevenBitNumber)80))
                .SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);
            var objectToAdd4 = new TimedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity))
                .SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap);

            var notesStarted = new List<Note>();
            var notesFinished = new List<Note>();

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(100,
                        (playback, collection) => collection.Add(objectToAdd1)),
                    new PlaybackChanger(50,
                        (playback, collection) => collection.Add(objectToAdd2)),
                    new PlaybackChanger(200,
                        (playback, collection) => collection.Add(objectToAdd3)),
                    new PlaybackChanger(50,
                        (playback, collection) => collection.Add(objectToAdd4)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, (SevenBitNumber)80), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new TextEvent("NEAREND"), TimeSpan.FromMilliseconds(550)),
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

            MidiAsserts.AreEqual(
                new[]
                {
                    new Note((SevenBitNumber)50)
                        .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap)
                        .SetLength(new MetricTimeSpan(0, 0, 0, 200), TempoMap),
                    new Note((SevenBitNumber)50) { Velocity = (SevenBitNumber)80 }
                        .SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap)
                        .SetLength(new MetricTimeSpan(0, 0, 0, 100), TempoMap),
                },
                notesStarted,
                "Invalid started/finished notes.");
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddNoteEvents_6()
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap),
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap),
            };
            var objectToAdd = new TimedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity))
                .SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(100,
                        (playback, collection) => collection.Add(objectToAdd)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new TextEvent("END"), TimeSpan.FromMilliseconds(600)),
                },
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddNoteEvents_7()
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap),
            };
            var objectToAdd = new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity))
                .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(100,
                        (playback, collection) => collection.Add(objectToAdd)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new TextEvent("END"), TimeSpan.FromMilliseconds(600)),
                },
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddNoteEvents_8()
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap),
            };
            var objectToAdd = new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity))
                .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(300,
                        (playback, collection) => collection.Add(objectToAdd)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new TextEvent("END"), TimeSpan.FromMilliseconds(600)),
                },
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddNoteEvents_9()
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap),
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap),
            };
            var objectToAdd = new TimedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity))
                .SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(300,
                        (playback, collection) => collection.Add(objectToAdd)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new TextEvent("END"), TimeSpan.FromMilliseconds(600)),
                },
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddNoteEvents_10()
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)80, Note.DefaultOffVelocity))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap),
            };
            var objectToAdd1 = new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity))
                .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap);
            var objectToAdd2 = new TimedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity))
                .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap);
            var objectToAdd3 = new TimedEvent(new NoteOnEvent((SevenBitNumber)80, Note.DefaultVelocity))
                .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(300,
                        (playback, collection) => collection.Add(objectToAdd1)),
                    new PlaybackChanger(50,
                        (playback, collection) => collection.Add(objectToAdd2)),
                    new PlaybackChanger(50,
                        (playback, collection) => collection.Add(objectToAdd3)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(350)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)80, Note.DefaultVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)80, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new TextEvent("END"), TimeSpan.FromMilliseconds(600)),
                },
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_AddNoteEvents_11()
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap),
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap),
            };
            var objectToAdd1 = new TimedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity))
                .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var objectToAdd2 = new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity))
                .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap);
            var objectToAdd3 = new TimedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity))
                .SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new PlaybackChanger(100,
                        (playback, collection) => collection.Add(objectToAdd1)),
                    new PlaybackChanger(150,
                        (playback, collection) => collection.Add(objectToAdd2, objectToAdd3)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new TextEvent("END"), TimeSpan.FromMilliseconds(600)),
                },
                setupPlayback: playback => playback.TrackNotes = true);
        }

        #endregion
    }
}
