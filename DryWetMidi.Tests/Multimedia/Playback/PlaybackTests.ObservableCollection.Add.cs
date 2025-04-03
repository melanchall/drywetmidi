using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
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
                    new DynamicPlaybackAction(500, (playback, collection) =>
                    {
                        collection.Add(objectToAdd);
                        CheckDuration(TimeSpan.FromSeconds(1), playback);
                    }),
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
                    new DynamicPlaybackAction(500, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1, objectToAdd2);
                        CheckDuration(TimeSpan.FromSeconds(1), playback);
                    }),
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
                    new DynamicPlaybackAction(500, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1);
                        CheckDuration(TimeSpan.FromSeconds(1), playback);
                    }),
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.Add(objectToAdd2);
                        CheckDuration(TimeSpan.FromSeconds(1), playback);
                    }),
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
                    new DynamicPlaybackAction(500, (playback, collection) =>
                    {
                        collection.Add(objectToAdd);
                        CheckDuration(TimeSpan.FromSeconds(1), playback);
                    }),
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
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1);
                        CheckDuration(TimeSpan.FromSeconds(1), playback);
                    }),
                    new DynamicPlaybackAction(200, (playback, collection) =>
                    {
                        collection.Add(objectToAdd2);
                        CheckDuration(TimeSpan.FromSeconds(1), playback);
                    }),
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.Add(objectToAdd3);
                        CheckDuration(TimeSpan.FromSeconds(1), playback);
                    }),
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
                    new DynamicPlaybackAction(500, (playback, collection) =>
                    {
                        collection.Add(objectToAdd);
                        CheckDuration(TimeSpan.FromSeconds(1), playback);
                    }),
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
                    new DynamicPlaybackAction(500, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1, objectToAdd2);
                        CheckDuration(TimeSpan.FromSeconds(1), playback);
                    }),
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
                    new DynamicPlaybackAction(500, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1, objectToAdd2);
                        CheckDuration(TimeSpan.FromSeconds(1), playback);
                    }),
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
                    new DynamicPlaybackAction(500, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1);
                        CheckDuration(TimeSpan.FromSeconds(1), playback);
                    }),
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.Add(objectToAdd2);
                        CheckDuration(TimeSpan.FromSeconds(1), playback);
                    }),
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
                    new DynamicPlaybackAction(500, (playback, collection) =>
                    {
                        collection.Add(objectToAdd);
                        CheckDuration(TimeSpan.FromSeconds(1), playback);
                    }),
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
                    new DynamicPlaybackAction(500, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1, objectToAdd2);
                        CheckDuration(TimeSpan.FromSeconds(1), playback);
                    }),
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
                    new DynamicPlaybackAction(500, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1, objectToAdd2);
                        CheckDuration(TimeSpan.FromSeconds(1), playback);
                    }),
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
                    new DynamicPlaybackAction(500, (playback, collection) =>
                    {
                        collection.Add(objectToAdd);
                        playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 700));
                        CheckDuration(TimeSpan.FromSeconds(1), playback);
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
                    new DynamicPlaybackAction(500, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1, objectToAdd2);
                        playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 700));
                        CheckDuration(TimeSpan.FromSeconds(1), playback);
                    }),
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 400));
                        CheckDuration(TimeSpan.FromSeconds(1), playback);
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
                    new DynamicPlaybackAction(500, (playback, collection) =>
                    {
                        collection.Add(objectToAdd);
                        CheckDuration(TimeSpan.FromSeconds(1), playback);
                    }),
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
                    new DynamicPlaybackAction(500, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1, objectToAdd2);
                        CheckDuration(TimeSpan.FromSeconds(1), playback);
                    }),
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
                    new DynamicPlaybackAction(500, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1, objectToAdd2);
                        CheckDuration(TimeSpan.FromSeconds(1), playback);
                    }),
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
                    new DynamicPlaybackAction(500, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1);
                        CheckDuration(TimeSpan.FromSeconds(1), playback);
                    }),
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.Add(objectToAdd2);
                        CheckDuration(TimeSpan.FromSeconds(1), playback);
                    }),
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
                    new DynamicPlaybackAction(500, (playback, collection) =>
                    {
                        collection.Add(objectToAdd);
                        CheckDuration(TimeSpan.FromSeconds(1), playback);
                    }),
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
                    new DynamicPlaybackAction(500, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1, objectToAdd2);
                        CheckDuration(TimeSpan.FromSeconds(1), playback);
                    }),
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
                    new DynamicPlaybackAction(500, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1, objectToAdd2);
                        CheckDuration(TimeSpan.FromSeconds(1), playback);
                    }),
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
                    new DynamicPlaybackAction(500, (playback, collection) =>
                    {
                        collection.Add(objectToAdd);
                        playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 700));
                        CheckDuration(TimeSpan.FromSeconds(1), playback);
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
                    new DynamicPlaybackAction(500, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1, objectToAdd2);
                        playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 700));
                        CheckDuration(TimeSpan.FromSeconds(1), playback);
                    }),
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 400));
                        CheckDuration(TimeSpan.FromSeconds(1), playback);
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
                    new DynamicPlaybackAction(500, (playback, collection) =>
                    {
                        collection.Add(objectToAdd);
                        CheckDuration(TimeSpan.FromSeconds(1), playback);
                    }),
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
                    new DynamicPlaybackAction(500, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1, objectToAdd2);
                        CheckDuration(TimeSpan.FromSeconds(1), playback);
                    }),
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
                    new DynamicPlaybackAction(500, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1, objectToAdd2);
                        CheckDuration(TimeSpan.FromSeconds(1), playback);
                    }),
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
                    new DynamicPlaybackAction(500, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1);
                        CheckDuration(TimeSpan.FromSeconds(1), playback);
                    }),
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.Add(objectToAdd2);
                        CheckDuration(TimeSpan.FromSeconds(1), playback);
                    }),
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
                    new DynamicPlaybackAction(500, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1, objectToAdd2, objectToAdd3);
                        CheckDuration(TimeSpan.FromSeconds(1), playback);
                    }),
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
                    new DynamicPlaybackAction(500, (playback, collection) =>
                    {
                        collection.Add(objectToAdd);
                        CheckDuration(TimeSpan.FromSeconds(1), playback);
                    }),
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
                    new DynamicPlaybackAction(500, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1, objectToAdd2);
                        CheckDuration(TimeSpan.FromSeconds(1), playback);
                    }),
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
                    new DynamicPlaybackAction(500, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1, objectToAdd2);
                        CheckDuration(TimeSpan.FromSeconds(1), playback);
                    }),
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
                    new DynamicPlaybackAction(500, (playback, collection) =>
                    {
                        collection.Add(objectToAdd);
                        playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 700));
                        CheckDuration(TimeSpan.FromSeconds(1), playback);
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
                    new DynamicPlaybackAction(500, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1, objectToAdd2);
                        playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 700));
                        CheckDuration(TimeSpan.FromSeconds(1), playback);
                    }),
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 400));
                        CheckDuration(TimeSpan.FromSeconds(1), playback);
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
                    new DynamicPlaybackAction((int)(400 * scaleFactor), (playback, collection) =>
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
                    new DynamicPlaybackAction((int)(400 * scaleFactor), (playback, collection) =>
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
                    new DynamicPlaybackAction((int)(200 * scaleFactor), (playback, collection) =>
                    {
                        collection.Add(objectToAdd1);
                        CheckDuration(TimeSpan.FromMilliseconds((int)(900 * scaleFactor)), playback);

                        playback.MoveToTime(new MetricTimeSpan(0, 0, 0, (int)(700 * scaleFactor)));
                    }),
                    new DynamicPlaybackAction((int)(100 * scaleFactor), (playback, collection) =>
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
                    new DynamicPlaybackAction((int)(200 * scaleFactor), (playback, collection) =>
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
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        collection.Add(objectToAdd);
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
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
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                        collection.Add(objectToAdd);
                        CheckDuration(TimeSpan.FromSeconds(1), playback);
                    }),
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
                    new DynamicPlaybackAction(200, (playback, collection) =>
                    {
                        collection.Add(objectToAdd);
                        CheckDuration(TimeSpan.FromMilliseconds(500), playback);
                    }),
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
                    new DynamicPlaybackAction(200, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1, objectToAdd2);
                        CheckDuration(TimeSpan.FromMilliseconds(500), playback);
                    }),
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
                    new DynamicPlaybackAction(200, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1);
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.Add(objectToAdd2);
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
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
                    new DynamicPlaybackAction(500, (playback, collection) =>
                    {
                        collection.Add(objectToAdd);
                        CheckDuration(TimeSpan.FromMilliseconds(1000), playback);
                    }),
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
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1);
                        CheckDuration(TimeSpan.FromMilliseconds(1000), playback);
                    }),
                    new DynamicPlaybackAction(200, (playback, collection) =>
                    {
                        collection.Add(objectToAdd2);
                        CheckDuration(TimeSpan.FromMilliseconds(1000), playback);
                    }),
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.Add(objectToAdd3);
                        CheckDuration(TimeSpan.FromMilliseconds(1000), playback);
                    }),
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
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        collection.Add(objectToAdd);
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                    }),
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
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1, objectToAdd2);
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                    }),
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
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1, objectToAdd2);
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                    }),
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
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1);
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                    }),
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.Add(objectToAdd2);
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                    }),
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
                    new DynamicPlaybackAction(500, (playback, collection) =>
                    {
                        collection.Add(objectToAdd);
                    }),
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
                    new DynamicPlaybackAction(500, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1, objectToAdd2);
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                    }),
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
                    new DynamicPlaybackAction(500, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1, objectToAdd2);
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                    }),
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
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.Add(objectToAdd);
                        playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 500));
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
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
                    new DynamicPlaybackAction(500, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1, objectToAdd2);
                        playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 700));
                        CheckDuration(TimeSpan.FromMilliseconds(1000), playback);
                    }),
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 400));
                        CheckDuration(TimeSpan.FromMilliseconds(1000), playback);
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
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        collection.Add(objectToAdd);
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                    }),
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
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1, objectToAdd2);
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                    }),
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
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1, objectToAdd2);
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                    }),
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
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1);
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                    }),
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.Add(objectToAdd2);
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                    }),
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
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.Add(objectToAdd);
                        CheckDuration(TimeSpan.FromMilliseconds(500), playback);
                    }),
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
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1, objectToAdd2);
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                    }),
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
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1, objectToAdd2);
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                    }),
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
                    new DynamicPlaybackAction(200, (playback, collection) =>
                    {
                        collection.Add(objectToAdd);
                        playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 400));
                        CheckDuration(TimeSpan.FromMilliseconds(500), playback);
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
                    new DynamicPlaybackAction(500, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1, objectToAdd2);
                        playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 700));
                        CheckDuration(TimeSpan.FromMilliseconds(1000), playback);
                    }),
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 400));
                        CheckDuration(TimeSpan.FromMilliseconds(1000), playback);
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
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        collection.Add(objectToAdd);
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                    }),
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
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1, objectToAdd2);
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                    }),
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
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1, objectToAdd2);
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                    }),
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
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1);
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                    }),
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.Add(objectToAdd2);
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                    }),
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
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1, objectToAdd2, objectToAdd3);
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                    }),
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
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.Add(objectToAdd);
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                    }),
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
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1, objectToAdd2);
                        CheckDuration(TimeSpan.FromMilliseconds(500), playback);
                    }),
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
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1, objectToAdd2);
                        CheckDuration(TimeSpan.FromMilliseconds(500), playback);
                    }),
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
                    new DynamicPlaybackAction(200, (playback, collection) =>
                    {
                        collection.Add(objectToAdd);
                        playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 400));
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
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
                    new DynamicPlaybackAction(500, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1, objectToAdd2);
                        playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 700));
                        CheckDuration(TimeSpan.FromMilliseconds(1000), playback);
                    }),
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 400));
                        CheckDuration(TimeSpan.FromMilliseconds(1000), playback);
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
                    new DynamicPlaybackAction((int)(200 * scaleFactor), (playback, collection) =>
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
                    new DynamicPlaybackAction((int)(400 * scaleFactor), (playback, collection) =>
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
                    new DynamicPlaybackAction((int)(200 * scaleFactor), (playback, collection) =>
                    {
                        collection.Add(objectToAdd1);
                        CheckDuration(TimeSpan.FromMilliseconds((int)(900 * scaleFactor)), playback);

                        playback.MoveToTime(new MetricTimeSpan(0, 0, 0, (int)(700 * scaleFactor)));
                    }),
                    new DynamicPlaybackAction((int)(100 * scaleFactor), (playback, collection) =>
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
                    new DynamicPlaybackAction((int)(200 * scaleFactor), (playback, collection) =>
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
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        collection.Add(objectToAdd);
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
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
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                        collection.Add(objectToAdd);
                        CheckDuration(TimeSpan.FromMilliseconds(1000), playback);
                    }),
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
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(0), playback);
                        collection.Add(objectToAdd);
                        CheckDuration(TimeSpan.FromMilliseconds(0), playback);
                    }),
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
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(0), playback);
                        collection.Add(objectToAdd);
                        CheckDuration(TimeSpan.FromMilliseconds(0), playback);
                    }),
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
                    new DynamicPlaybackAction(50, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1);
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                    new DynamicPlaybackAction(50, (playback, collection) =>
                    {
                        collection.Add(objectToAdd2);
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
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
                    new DynamicPlaybackAction(50, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1);
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                    new DynamicPlaybackAction(200, (playback, collection) =>
                    {
                        collection.Add(objectToAdd2);
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
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
                    new DynamicPlaybackAction(50, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1, objectToAdd2);
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
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
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1, objectToAdd2);
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
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
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1);
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                    new DynamicPlaybackAction(50, (playback, collection) =>
                    {
                        collection.Add(objectToAdd2);
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                    new DynamicPlaybackAction(200, (playback, collection) =>
                    {
                        collection.Add(objectToAdd3);
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                    new DynamicPlaybackAction(50, (playback, collection) =>
                    {
                        collection.Add(objectToAdd4);
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
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
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        collection.Add(objectToAdd);
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                    }),
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
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        collection.Add(objectToAdd);
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                    }),
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
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.Add(objectToAdd);
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                    }),
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
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.Add(objectToAdd);
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                    }),
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
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1);
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                    }),
                    new DynamicPlaybackAction(50, (playback, collection) =>
                    {
                        collection.Add(objectToAdd2);
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                    }),
                    new DynamicPlaybackAction(50, (playback, collection) =>
                    {
                        collection.Add(objectToAdd3);
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                    }),
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
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1);
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                    }),
                    new DynamicPlaybackAction(150, (playback, collection) =>
                    {
                        collection.Add(objectToAdd2, objectToAdd3);
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                    }),
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

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Add_SetTempo_1()
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap),
                new TimedEvent(new TextEvent("B"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap),
                new TimedEvent(new TextEvent("C"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap),
            };

            var objectToAdd = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                .SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap);

            SnapPoint snapPoint = null;

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                        collection.Add(objectToAdd);
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new TextEvent("B"), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new TextEvent("C"), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2), TimeSpan.FromMilliseconds(700)),
                },
                setupPlayback: playback =>
                    snapPoint = playback.AddSnapPoint(new MetricTimeSpan(0, 0, 0, 400)),
                additionalChecks: playback => MidiAsserts.AreEqual(
                    AddTempoChanges(TempoMap, (TimeSpan.FromMilliseconds(700), new Tempo(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))),
                    playback.TempoMap,
                    "Invalid tempo map."));

            Assert.AreEqual(TimeSpan.FromMilliseconds(400), snapPoint.Time, "Invalid snap point time.");
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Add_SetTempo_2()
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap),
                new TimedEvent(new TextEvent("B"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap),
                new TimedEvent(new TextEvent("C"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap),
            };

            var objectToAdd = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                .SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap);

            SnapPoint snapPoint = null;

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(400, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                        collection.Add(objectToAdd);
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new TextEvent("B"), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new TextEvent("C"), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2), TimeSpan.FromMilliseconds(700)),
                },
                setupPlayback: playback =>
                    snapPoint = playback.AddSnapPoint(new MetricTimeSpan(0, 0, 0, 600)),
                additionalChecks: playback => MidiAsserts.AreEqual(
                    AddTempoChanges(TempoMap, (TimeSpan.FromMilliseconds(700), new Tempo(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))),
                    playback.TempoMap,
                    "Invalid tempo map."));

            Assert.AreEqual(TimeSpan.FromMilliseconds(600), snapPoint.Time, "Invalid snap point time.");
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Add_SetTempo_3()
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap),
                new TimedEvent(new TextEvent("B"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap),
                new TimedEvent(new TextEvent("C"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap),
            };

            var objectToAdd = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                .SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);

            SnapPoint snapPoint = null;

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                        collection.Add(objectToAdd);
                        CheckDuration(TimeSpan.FromMilliseconds(550), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new TextEvent("B"), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new TextEvent("C"), TimeSpan.FromMilliseconds(550)),
                },
                setupPlayback: playback =>
                    snapPoint = playback.AddSnapPoint(new MetricTimeSpan(0, 0, 0, 600)),
                additionalChecks: playback => MidiAsserts.AreEqual(
                    AddTempoChanges(TempoMap, (TimeSpan.FromMilliseconds(500), new Tempo(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))),
                    playback.TempoMap,
                    "Invalid tempo map."));

            Assert.AreEqual(TimeSpan.FromMilliseconds(550), snapPoint.Time, "Invalid snap point time.");
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Add_SetTempo_4()
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap),
                new TimedEvent(new TextEvent("B"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap),
                new TimedEvent(new TextEvent("C"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap),
            };

            var objectToAdd = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap);

            SnapPoint snapPoint = null;

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                        collection.Add(objectToAdd);
                        CheckDuration(TimeSpan.FromMilliseconds(500), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new TextEvent("B"), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new TextEvent("C"), TimeSpan.FromMilliseconds(500)),
                },
                setupPlayback: playback =>
                    snapPoint = playback.AddSnapPoint(new MetricTimeSpan(0, 0, 0, 600)),
                additionalChecks: playback => MidiAsserts.AreEqual(
                    AddTempoChanges(TempoMap, (TimeSpan.FromMilliseconds(400), new Tempo(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))),
                    playback.TempoMap,
                    "Invalid tempo map."));

            Assert.AreEqual(TimeSpan.FromMilliseconds(500), snapPoint.Time, "Invalid snap point time.");
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Add_SetTempo_5()
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap),
                new TimedEvent(new TextEvent("B"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap),
                new TimedEvent(new TextEvent("C"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap),
            };

            var objectToAdd = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote * 2))
                .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap);

            SnapPoint snapPoint = null;

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                        collection.Add(objectToAdd);
                        CheckDuration(TimeSpan.FromMilliseconds(800), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new TextEvent("B"), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote * 2), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new TextEvent("C"), TimeSpan.FromMilliseconds(800)),
                },
                setupPlayback: playback =>
                    snapPoint = playback.AddSnapPoint(new MetricTimeSpan(0, 0, 0, 600)),
                additionalChecks: playback => MidiAsserts.AreEqual(
                    AddTempoChanges(TempoMap, (TimeSpan.FromMilliseconds(400), new Tempo(SetTempoEvent.DefaultMicrosecondsPerQuarterNote * 2))),
                    playback.TempoMap,
                    "Invalid tempo map."));

            Assert.AreEqual(TimeSpan.FromMilliseconds(800), snapPoint.Time, "Invalid snap point time.");
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Add_SetTempo_6()
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap),
                new TimedEvent(new TextEvent("B"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap),
                new TimedEvent(new TextEvent("C"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap),
            };

            var objectToAdd = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            SnapPoint snapPoint = null;

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(400, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                        collection.Add(objectToAdd);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(350), "Invalid current time.");
                        CheckDuration(TimeSpan.FromMilliseconds(450), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new TextEvent("B"), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new TextEvent("C"), TimeSpan.FromMilliseconds(500)),
                },
                setupPlayback: playback =>
                    snapPoint = playback.AddSnapPoint(new MetricTimeSpan(0, 0, 0, 600)),
                additionalChecks: playback => MidiAsserts.AreEqual(
                    AddTempoChanges(TempoMap, (TimeSpan.FromMilliseconds(300), new Tempo(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))),
                    playback.TempoMap,
                    "Invalid tempo map."));

            Assert.AreEqual(TimeSpan.FromMilliseconds(450), snapPoint.Time, "Invalid snap point time.");
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Add_SetTempo_7()
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap),
                new TimedEvent(new TextEvent("B"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap),
                new TimedEvent(new TextEvent("C"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap),
            };

            var objectToAdd1 = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                .SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap);
            var objectToAdd2 = new TimedEvent(new TextEvent("D"))
                .SetTime(new MetricTimeSpan(0, 0, 0, 800), TempoMap);

            SnapPoint snapPoint1 = null;
            SnapPoint snapPoint2 = null;

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1);
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                    new DynamicPlaybackAction(500, (playback, collection) =>
                    {
                        collection.Add(objectToAdd2);
                        CheckDuration(TimeSpan.FromMilliseconds(750), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new TextEvent("B"), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new TextEvent("C"), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new TextEvent("D"), TimeSpan.FromMilliseconds(750)),
                },
                setupPlayback: playback =>
                {
                    snapPoint1 = playback.AddSnapPoint(new MetricTimeSpan(0, 0, 0, 600));
                    snapPoint2 = playback.AddSnapPoint(new MetricTimeSpan(0, 0, 0, 750));
                },
                additionalChecks: playback => MidiAsserts.AreEqual(
                    AddTempoChanges(TempoMap, (TimeSpan.FromMilliseconds(700), new Tempo(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))),
                    playback.TempoMap,
                    "Invalid tempo map."));

            Assert.AreEqual(TimeSpan.FromMilliseconds(600), snapPoint1.Time, "Invalid first snap point time.");
            Assert.AreEqual(TimeSpan.FromMilliseconds(725), snapPoint2.Time, "Invalid second snap point time.");
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Add_SetTempo_8()
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap),
                new TimedEvent(new TextEvent("B"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap),
                new TimedEvent(new TextEvent("C"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap),
                new TimedEvent(new TextEvent("D"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 800), TempoMap),
                new TimedEvent(new TextEvent("E"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 1000), TempoMap),
            };

            var objectToAdd1 = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                .SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);
            var objectToAdd2 = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 4))
                .SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap);

            SnapPoint snapPoint = null;

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(1000), playback);
                        collection.Add(objectToAdd1);
                        CheckDuration(TimeSpan.FromMilliseconds(750), playback);
                    }),
                    new DynamicPlaybackAction(495, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(750), playback);
                        collection.Add(objectToAdd2);
                        CheckDuration(TimeSpan.FromMilliseconds(675), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new TextEvent("B"), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new TextEvent("C"), TimeSpan.FromMilliseconds(550)),
                    new ReceivedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 4), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new TextEvent("D"), TimeSpan.FromMilliseconds(625)),
                    new ReceivedEvent(new TextEvent("E"), TimeSpan.FromMilliseconds(675)),
                },
                setupPlayback: playback =>
                    snapPoint = playback.AddSnapPoint(new MetricTimeSpan(0, 0, 0, 1000)),
                additionalChecks: playback => MidiAsserts.AreEqual(
                    AddTempoChanges(TempoMap,
                        (TimeSpan.FromMilliseconds(500), new Tempo(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2)),
                        (TimeSpan.FromMilliseconds(600), new Tempo(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 4))),
                    playback.TempoMap,
                    "Invalid tempo map."));

            Assert.AreEqual(TimeSpan.FromMilliseconds(675), snapPoint.Time, "Invalid snap point time.");
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Add_SetTempo_9()
        {
            var tempoMap = AddTempoChanges(TempoMap,
                (TimeSpan.FromMilliseconds(650), new Tempo(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 4)));

            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 200), tempoMap),
                new TimedEvent(new TextEvent("B"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), tempoMap),
                new TimedEvent(new TextEvent("C"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 600), tempoMap),
                new TimedEvent(new TextEvent("D"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 800), tempoMap),
            };

            var objectToAdd = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                .SetTime(new MetricTimeSpan(0, 0, 0, 300), tempoMap);

            SnapPoint snapPoint1 = null;
            SnapPoint snapPoint2 = null;

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(800), playback);
                        collection.Add(objectToAdd);
                        CheckDuration(TimeSpan.FromMilliseconds(625), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new TextEvent("B"), TimeSpan.FromMilliseconds(350)),
                    new ReceivedEvent(new TextEvent("C"), TimeSpan.FromMilliseconds(450)),
                    new ReceivedEvent(new TextEvent("D"), TimeSpan.FromMilliseconds(625)),
                },
                setupPlayback: playback =>
                {
                    snapPoint1 = playback.AddSnapPoint(new MetricTimeSpan(0, 0, 0, 400));
                    snapPoint2 = playback.AddSnapPoint(new MetricTimeSpan(0, 0, 0, 800));
                },
                additionalChecks: playback =>
                {
                    MidiAsserts.AreEqual(
                        AddTempoChanges(tempoMap,
                            (TimeSpan.FromMilliseconds(300), new Tempo(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))),
                        playback.TempoMap,
                        "Invalid tempo map.");

                    var lastTempoChange = playback.TempoMap.GetTempoChanges().Last();
                    var lastTempoChangeTime = lastTempoChange.TimeAs<MetricTimeSpan>(playback.TempoMap);
                    Assert.AreEqual(475, lastTempoChangeTime.TotalMilliseconds, "Invalid last tempo change time.");
                },
                tempoMap: tempoMap);

            Assert.AreEqual(TimeSpan.FromMilliseconds(350), snapPoint1.Time, "Invalid first snap point time.");
            Assert.AreEqual(TimeSpan.FromMilliseconds(625), snapPoint2.Time, "Invalid second snap point time.");
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Add_SetTempo_10()
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new NoteOffEvent())
                    .SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap),
            };

            var objectToAdd1 = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap);
            var objectToAdd2 = new TimedEvent(new NoteOnEvent())
                .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                        collection.Add(objectToAdd1);
                        CheckDuration(TimeSpan.FromMilliseconds(500), playback);
                    }),
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        collection.Add(objectToAdd2);
                        CheckDuration(TimeSpan.FromMilliseconds(500), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(450)),
                    new ReceivedEvent(new TextEvent("END"), TimeSpan.FromMilliseconds(500)),
                },
                additionalChecks: playback => MidiAsserts.AreEqual(
                    AddTempoChanges(TempoMap, (TimeSpan.FromMilliseconds(400), new Tempo(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))),
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Add_SetTempo_11()
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new NoteOnEvent())
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap),
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
            };

            var objectToAdd1 = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap);
            var objectToAdd2 = new TimedEvent(new NoteOffEvent())
                .SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                        collection.Add(objectToAdd1);
                        CheckDuration(TimeSpan.FromMilliseconds(450), playback);
                    }),
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        collection.Add(objectToAdd2);
                        CheckDuration(TimeSpan.FromMilliseconds(450), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.FromMilliseconds(250)),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(350)),
                    new ReceivedEvent(new TextEvent("END"), TimeSpan.FromMilliseconds(450)),
                },
                additionalChecks: playback => MidiAsserts.AreEqual(
                    AddTempoChanges(TempoMap, (TimeSpan.FromMilliseconds(200), new Tempo(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))),
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Add_SetTempo_12()
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new NoteOnEvent())
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap),
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
            };

            var objectToAdd1 = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap);
            var objectToAdd2 = new TimedEvent(new NoteOffEvent())
                .SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                        collection.Add(objectToAdd1);
                        CheckDuration(TimeSpan.FromMilliseconds(450), playback);
                    }),
                    new DynamicPlaybackAction(200, (playback, collection) =>
                    {
                        collection.Add(objectToAdd2);
                        CheckDuration(TimeSpan.FromMilliseconds(450), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(350)),
                    new ReceivedEvent(new TextEvent("END"), TimeSpan.FromMilliseconds(450)),
                },
                additionalChecks: playback => MidiAsserts.AreEqual(
                    AddTempoChanges(TempoMap, (TimeSpan.FromMilliseconds(200), new Tempo(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))),
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Add_SetTempo_13()
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new NoteOnEvent())
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap),
                new TimedEvent(new NoteOffEvent())
                    .SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
            };

            var objectToAdd = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                        collection.Add(objectToAdd);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(150), "before time jump");
                        playback.MoveToTime((MetricTimeSpan)TimeSpan.FromMilliseconds(300));
                        CheckDuration(TimeSpan.FromMilliseconds(400), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(300)),
                },
                additionalChecks: playback => MidiAsserts.AreEqual(
                    AddTempoChanges(TempoMap, (TimeSpan.FromMilliseconds(100), new Tempo(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))),
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Add_SetTempo_14()
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new NoteOnEvent())
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap),
                new TimedEvent(new NoteOffEvent())
                    .SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 900), TempoMap),
            };

            var objectToAdd = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(900), playback);
                        collection.Add(objectToAdd);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(150), "before time jump");
                        playback.MoveToTime((MetricTimeSpan)TimeSpan.FromMilliseconds(450));
                        CheckDuration(TimeSpan.FromMilliseconds(500), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new TextEvent("END"), TimeSpan.FromMilliseconds(250)),
                },
                additionalChecks: playback => MidiAsserts.AreEqual(
                    AddTempoChanges(TempoMap, (TimeSpan.FromMilliseconds(100), new Tempo(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))),
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Add_SetTempo_15()
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new NoteOnEvent())
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap),
                new TimedEvent(new NoteOffEvent())
                    .SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 900), TempoMap),
            };

            var objectToAdd = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(900), playback);
                        collection.Add(objectToAdd);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(150), "before first time jump");
                        playback.MoveToTime((MetricTimeSpan)TimeSpan.FromMilliseconds(300));
                        CheckDuration(TimeSpan.FromMilliseconds(500), playback);
                    }),
                    new DynamicPlaybackAction(50, (playback, collection) =>
                    {
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(350), "before second time jump");
                        playback.MoveToTime((MetricTimeSpan)TimeSpan.FromMilliseconds(450));
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(250)),
                    new ReceivedEvent(new TextEvent("END"), TimeSpan.FromMilliseconds(300)),
                },
                additionalChecks: playback => MidiAsserts.AreEqual(
                    AddTempoChanges(TempoMap, (TimeSpan.FromMilliseconds(100), new Tempo(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))),
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Add_SetTempo_16()
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 800), TempoMap),
            };

            var objectToAdd1 = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var objectToAdd2 = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(800), playback);
                        collection.Add(objectToAdd1, objectToAdd2);
                        CheckDuration(TimeSpan.FromMilliseconds(550), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new TextEvent("END"), TimeSpan.FromMilliseconds(550)),
                },
                additionalChecks: playback => MidiAsserts.AreEqual(
                    AddTempoChanges(TempoMap, (TimeSpan.FromMilliseconds(300), new Tempo(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))),
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Add_SetTempo_17()
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap),
                new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap),
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 800), TempoMap),
            };

            var objectToAdd = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                .SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(500), playback);
                        collection.Add(objectToAdd);
                        CheckDuration(TimeSpan.FromMilliseconds(500), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new TextEvent("END"), TimeSpan.FromMilliseconds(500)),
                },
                additionalChecks: playback => MidiAsserts.AreEqual(
                    AddTempoChanges(TempoMap, (TimeSpan.FromMilliseconds(200), new Tempo(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))),
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Add_SetTempo_PlaybackEnd()
        {
            var objectToAdd = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new NoteOnEvent())
                    .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap),
                new TimedEvent(new NoteOffEvent())
                    .SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
                new TimedEvent(new TextEvent("A"), 600),
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                        collection.Add(objectToAdd);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(100), "after tempo change");
                        CheckDuration(TimeSpan.FromMilliseconds(450), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(400)),
                },
                setupPlayback: playback =>
                    playback.PlaybackEnd = new MetricTimeSpan(0, 0, 0, 500),
                additionalChecks: playback =>
                {
                    MidiAsserts.AreEqual(
                        AddTempoChanges(TempoMap, (TimeSpan.FromMilliseconds(300), new Tempo(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))),
                        playback.TempoMap,
                        "Invalid tempo map.");

                    Assert.AreEqual(new MetricTimeSpan(0, 0, 0, 400), playback.PlaybackEnd, "Invalid playback end.");
                });
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Add_SetTempo_PlaybackStart()
        {
            var objectToAdd = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new NoteOnEvent())
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap),
                new TimedEvent(new NoteOffEvent())
                    .SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(50, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                        collection.Add(objectToAdd);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(225), "after tempo change");
                        CheckDuration(TimeSpan.FromMilliseconds(400), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.FromMilliseconds(75)),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(225)),
                },
                setupPlayback: playback =>
                    playback.PlaybackStart = new MetricTimeSpan(0, 0, 0, 300),
                additionalChecks: playback =>
                {
                    MidiAsserts.AreEqual(
                        AddTempoChanges(TempoMap, (TimeSpan.FromMilliseconds(100), new Tempo(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))),
                        playback.TempoMap,
                        "Invalid tempo map.");

                    Assert.AreEqual(new MetricTimeSpan(0, 0, 0, 200), playback.PlaybackStart, "Invalid playback start.");
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Add_TimeSignature_1()
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap),
                new TimedEvent(new TextEvent("B"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap),
                new TimedEvent(new TextEvent("C"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap),
            };

            var objectToAdd = new TimedEvent(new TimeSignatureEvent(3, 4))
                .SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                        collection.Add(objectToAdd);
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new TextEvent("B"), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new TextEvent("C"), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new TimeSignatureEvent(3, 4), TimeSpan.FromMilliseconds(700)),
                },
                additionalChecks: playback => MidiAsserts.AreEqual(
                    AddTimeSignatureChanges(TempoMap, (TimeSpan.FromMilliseconds(700), new TimeSignature(3, 4))),
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Add_TimeSignature_2()
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap),
                new TimedEvent(new TextEvent("B"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap),
                new TimedEvent(new TextEvent("C"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap),
            };

            var objectToAdd = new TimedEvent(new TimeSignatureEvent(3, 4))
                .SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(400, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                        collection.Add(objectToAdd);
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new TextEvent("B"), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new TextEvent("C"), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new TimeSignatureEvent(3, 4), TimeSpan.FromMilliseconds(700)),
                },
                additionalChecks: playback => MidiAsserts.AreEqual(
                    AddTimeSignatureChanges(TempoMap, (TimeSpan.FromMilliseconds(700), new TimeSignature(3, 4))),
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Add_TimeSignature_3()
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap),
                new TimedEvent(new TextEvent("B"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap),
                new TimedEvent(new TextEvent("C"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap),
            };

            var objectToAdd = new TimedEvent(new TimeSignatureEvent(3, 4))
                .SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        collection.Add(objectToAdd);
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new TextEvent("B"), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new TimeSignatureEvent(3, 4), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new TextEvent("C"), TimeSpan.FromMilliseconds(600)),
                },
                additionalChecks: playback => MidiAsserts.AreEqual(
                    AddTimeSignatureChanges(TempoMap, (TimeSpan.FromMilliseconds(500), new TimeSignature(3, 4))),
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Add_TimeSignature_4()
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap),
                new TimedEvent(new TextEvent("B"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap),
                new TimedEvent(new TextEvent("C"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap),
            };

            var objectToAdd = new TimedEvent(new TimeSignatureEvent(3, 4))
                .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        collection.Add(objectToAdd);
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new TextEvent("B"), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new TimeSignatureEvent(3, 4), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new TextEvent("C"), TimeSpan.FromMilliseconds(600)),
                },
                additionalChecks: playback => MidiAsserts.AreEqual(
                    AddTimeSignatureChanges(TempoMap, (TimeSpan.FromMilliseconds(400), new TimeSignature(3, 4))),
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Add_TimeSignature_5()
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap),
                new TimedEvent(new TextEvent("B"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap),
                new TimedEvent(new TextEvent("C"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap),
            };

            var objectToAdd = new TimedEvent(new TimeSignatureEvent(5, 2))
                .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        collection.Add(objectToAdd);
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new TextEvent("B"), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new TimeSignatureEvent(5, 2), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new TextEvent("C"), TimeSpan.FromMilliseconds(600)),
                },
                additionalChecks: playback => MidiAsserts.AreEqual(
                    AddTimeSignatureChanges(TempoMap, (TimeSpan.FromMilliseconds(400), new TimeSignature(5, 2))),
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Add_TimeSignature_6()
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap),
                new TimedEvent(new TextEvent("B"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap),
                new TimedEvent(new TextEvent("C"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap),
            };

            var objectToAdd = new TimedEvent(new TimeSignatureEvent(3, 4))
                .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(400, (playback, collection) =>
                    {
                        collection.Add(objectToAdd);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(400), "Invalid current time.");
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new TextEvent("B"), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new TextEvent("C"), TimeSpan.FromMilliseconds(600)),
                },
                additionalChecks: playback => MidiAsserts.AreEqual(
                    AddTimeSignatureChanges(TempoMap, (TimeSpan.FromMilliseconds(300), new TimeSignature(3, 4))),
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Add_TimeSignature_7()
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap),
                new TimedEvent(new TextEvent("B"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap),
                new TimedEvent(new TextEvent("C"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap),
            };

            var objectToAdd1 = new TimedEvent(new TimeSignatureEvent(3, 4))
                .SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap);
            var objectToAdd2 = new TimedEvent(new TextEvent("D"))
                .SetTime(new MetricTimeSpan(0, 0, 0, 800), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                        collection.Add(objectToAdd1);
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                    new DynamicPlaybackAction(500, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                        collection.Add(objectToAdd2);
                        CheckDuration(TimeSpan.FromMilliseconds(800), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new TextEvent("B"), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new TextEvent("C"), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new TimeSignatureEvent(3, 4), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new TextEvent("D"), TimeSpan.FromMilliseconds(800)),
                },
                additionalChecks: playback => MidiAsserts.AreEqual(
                    AddTimeSignatureChanges(TempoMap, (TimeSpan.FromMilliseconds(700), new TimeSignature(3, 4))),
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Add_TimeSignature_8()
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap),
                new TimedEvent(new TextEvent("B"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap),
                new TimedEvent(new TextEvent("C"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap),
                new TimedEvent(new TextEvent("D"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 800), TempoMap),
                new TimedEvent(new TextEvent("E"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 1000), TempoMap),
            };

            var objectToAdd1 = new TimedEvent(new TimeSignatureEvent(3, 4))
                .SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);
            var objectToAdd2 = new TimedEvent(new TimeSignatureEvent(3, 8))
                .SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1);
                        CheckDuration(TimeSpan.FromMilliseconds(1000), playback);
                    }),
                    new DynamicPlaybackAction(500, (playback, collection) =>
                    {
                        collection.Add(objectToAdd2);
                        CheckDuration(TimeSpan.FromMilliseconds(1000), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new TextEvent("B"), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new TimeSignatureEvent(3, 4), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new TextEvent("C"), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new TimeSignatureEvent(3, 8), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new TextEvent("D"), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new TextEvent("E"), TimeSpan.FromMilliseconds(1000)),
                },
                additionalChecks: playback => MidiAsserts.AreEqual(
                    AddTimeSignatureChanges(TempoMap,
                        (TimeSpan.FromMilliseconds(500), new TimeSignature(3, 4)),
                        (TimeSpan.FromMilliseconds(700), new TimeSignature(3, 8))),
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Add_TimeSignature_9()
        {
            var tempoMap = AddTimeSignatureChanges(TempoMap,
                (TimeSpan.FromMilliseconds(650), new TimeSignature(3, 8)));

            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 200), tempoMap),
                new TimedEvent(new TextEvent("B"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), tempoMap),
                new TimedEvent(new TextEvent("C"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 600), tempoMap),
                new TimedEvent(new TextEvent("D"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 800), tempoMap),
            };

            var objectToAdd = new TimedEvent(new TimeSignatureEvent(3, 4))
                .SetTime(new MetricTimeSpan(0, 0, 0, 300), tempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        collection.Add(objectToAdd);
                        CheckDuration(TimeSpan.FromMilliseconds(800), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new TimeSignatureEvent(3, 4), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new TextEvent("B"), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new TextEvent("C"), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new TextEvent("D"), TimeSpan.FromMilliseconds(800)),
                },
                additionalChecks: playback =>
                {
                    MidiAsserts.AreEqual(
                        AddTimeSignatureChanges(tempoMap,
                            (TimeSpan.FromMilliseconds(300), new TimeSignature(3, 4))),
                        playback.TempoMap,
                        "Invalid tempo map.");

                    var lastTimeSignatureChange = playback.TempoMap.GetTimeSignatureChanges().Last();
                    var lastTimeSignatureChangeTime = lastTimeSignatureChange.TimeAs<MetricTimeSpan>(playback.TempoMap);
                    Assert.AreEqual(650, lastTimeSignatureChangeTime.TotalMilliseconds, "Invalid last time signature change time.");
                },
                tempoMap: tempoMap);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Add_TimeSignature_10()
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new NoteOffEvent())
                    .SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap),
            };

            var objectToAdd1 = new TimedEvent(new TimeSignatureEvent(3, 4))
                .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap);
            var objectToAdd2 = new TimedEvent(new NoteOnEvent())
                .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1);
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                    }),
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        collection.Add(objectToAdd2);
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new TimeSignatureEvent(3, 4), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new TextEvent("END"), TimeSpan.FromMilliseconds(600)),
                },
                additionalChecks: playback => MidiAsserts.AreEqual(
                    AddTimeSignatureChanges(TempoMap, (TimeSpan.FromMilliseconds(400), new TimeSignature(3, 4))),
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Add_TimeSignature_11()
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new NoteOnEvent())
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap),
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
            };

            var objectToAdd1 = new TimedEvent(new TimeSignatureEvent(3, 4))
                .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap);
            var objectToAdd2 = new TimedEvent(new NoteOffEvent())
                .SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1);
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        collection.Add(objectToAdd2);
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new TimeSignatureEvent(3, 4), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new TextEvent("END"), TimeSpan.FromMilliseconds(700)),
                },
                additionalChecks: playback => MidiAsserts.AreEqual(
                    AddTimeSignatureChanges(TempoMap, (TimeSpan.FromMilliseconds(200), new TimeSignature(3, 4))),
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Add_TimeSignature_12()
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new NoteOnEvent())
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap),
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
            };

            var objectToAdd1 = new TimedEvent(new TimeSignatureEvent(3, 4))
                .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap);
            var objectToAdd2 = new TimedEvent(new NoteOffEvent())
                .SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1);
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                    new DynamicPlaybackAction(200, (playback, collection) =>
                    {
                        collection.Add(objectToAdd2);
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new TimeSignatureEvent(3, 4), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new TextEvent("END"), TimeSpan.FromMilliseconds(700)),
                },
                additionalChecks: playback => MidiAsserts.AreEqual(
                    AddTimeSignatureChanges(TempoMap, (TimeSpan.FromMilliseconds(200), new TimeSignature(3, 4))),
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Add_TimeSignature_13()
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new NoteOnEvent())
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap),
                new TimedEvent(new NoteOffEvent())
                    .SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
            };

            var objectToAdd = new TimedEvent(new TimeSignatureEvent(3, 4))
                .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                        collection.Add(objectToAdd);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(200), "before time jump");
                        playback.MoveToTime((MetricTimeSpan)TimeSpan.FromMilliseconds(500));
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(400)),
                },
                additionalChecks: playback => MidiAsserts.AreEqual(
                    AddTimeSignatureChanges(TempoMap, (TimeSpan.FromMilliseconds(100), new TimeSignature(3, 4))),
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Add_TimeSignature_14()
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new NoteOnEvent())
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap),
                new TimedEvent(new NoteOffEvent())
                    .SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 900), TempoMap),
            };

            var objectToAdd = new TimedEvent(new TimeSignatureEvent(3, 4))
                .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(900), playback);
                        collection.Add(objectToAdd);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(200), "before time jump");
                        playback.MoveToTime((MetricTimeSpan)TimeSpan.FromMilliseconds(800));
                        CheckDuration(TimeSpan.FromMilliseconds(900), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new TextEvent("END"), TimeSpan.FromMilliseconds(300)),
                },
                additionalChecks: playback => MidiAsserts.AreEqual(
                    AddTimeSignatureChanges(TempoMap, (TimeSpan.FromMilliseconds(100), new TimeSignature(3, 4))),
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Add_TimeSignature_15()
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new NoteOnEvent())
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap),
                new TimedEvent(new NoteOffEvent())
                    .SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 900), TempoMap),
            };

            var objectToAdd = new TimedEvent(new TimeSignatureEvent(3, 4))
                .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200, (playback, collection) =>
                    {
                        collection.Add(objectToAdd);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(200), "before first time jump");
                        playback.MoveToTime((MetricTimeSpan)TimeSpan.FromMilliseconds(500));
                        CheckDuration(TimeSpan.FromMilliseconds(900), playback);
                    }),
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(600), "before second time jump");
                        playback.MoveToTime((MetricTimeSpan)TimeSpan.FromMilliseconds(800));
                        CheckDuration(TimeSpan.FromMilliseconds(900), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new TextEvent("END"), TimeSpan.FromMilliseconds(400)),
                },
                additionalChecks: playback => MidiAsserts.AreEqual(
                    AddTimeSignatureChanges(TempoMap, (TimeSpan.FromMilliseconds(100), new TimeSignature(3, 4))),
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Add_TimeSignature_16()
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 800), TempoMap),
            };

            var objectToAdd1 = new TimedEvent(new TimeSignatureEvent(3, 4))
                .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var objectToAdd2 = new TimedEvent(new TimeSignatureEvent(3, 4))
                .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        collection.Add(objectToAdd1, objectToAdd2);
                        CheckDuration(TimeSpan.FromMilliseconds(800), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new TimeSignatureEvent(3, 4), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new TextEvent("END"), TimeSpan.FromMilliseconds(800)),
                },
                additionalChecks: playback => MidiAsserts.AreEqual(
                    AddTimeSignatureChanges(TempoMap, (TimeSpan.FromMilliseconds(300), new TimeSignature(3, 4))),
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Add_TimeSignature_17()
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TimeSignatureEvent(3, 4))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap),
                new TimedEvent(new TimeSignatureEvent(3, 4))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap),
                new TimedEvent(new TextEvent("END"))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 800), TempoMap),
            };

            var objectToAdd = new TimedEvent(new TimeSignatureEvent(3, 4))
                .SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        collection.Add(objectToAdd);
                        CheckDuration(TimeSpan.FromMilliseconds(800), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new TimeSignatureEvent(3, 4), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new TextEvent("END"), TimeSpan.FromMilliseconds(800)),
                },
                additionalChecks: playback => MidiAsserts.AreEqual(
                    AddTimeSignatureChanges(TempoMap, (TimeSpan.FromMilliseconds(200), new TimeSignature(3, 4))),
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        #endregion

        #region Private methods

        private TempoMap AddTempoChanges(TempoMap reference, params (TimeSpan Time, Tempo Tempo)[] tempoChanges)
        {
            var tempoMap = reference.Clone();

            foreach (var tempoChange in tempoChanges)
            {
                var midiTime = TimeConverter.ConvertFrom((MetricTimeSpan)tempoChange.Time, tempoMap);
                tempoMap.TempoLine.SetValue(midiTime, tempoChange.Tempo);
            }

            return tempoMap;
        }

        private TempoMap AddTimeSignatureChanges(TempoMap reference, params (TimeSpan Time, TimeSignature TimeSignature)[] timeSignatureChanges)
        {
            var tempoMap = reference.Clone();

            foreach (var timeSignatureChange in timeSignatureChanges)
            {
                var midiTime = TimeConverter.ConvertFrom((MetricTimeSpan)timeSignatureChange.Time, tempoMap);
                tempoMap.TimeSignatureLine.SetValue(midiTime, timeSignatureChange.TimeSignature);
            }

            return tempoMap;
        }

        #endregion
    }
}
