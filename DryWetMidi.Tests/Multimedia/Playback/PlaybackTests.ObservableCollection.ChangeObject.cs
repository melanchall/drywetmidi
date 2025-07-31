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
        public void CheckPlaybackDataChangesOnTheFly_ChangeObject_1()
        {
            
            var objectToChange = new TimedEvent(new TextEvent("A")).SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 1000), TempoMap),
                objectToChange,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(400, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap));
                        CheckDuration(TimeSpan.FromMilliseconds(1000), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new SentReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(500)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                });
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_ChangeObject_2()
        {
            var objectToChange = new TimedEvent(new TextEvent("A")).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
                objectToChange,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap));
                        CheckDuration(TimeSpan.FromMilliseconds(500), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(500)),
                });
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_ChangeObject_ProgramChange_1()
        {
            var objectToChange = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
                objectToChange,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);
                                ((ProgramChangeEvent)((TimedEvent)obj).Event).ProgramNumber = (SevenBitNumber)71;
                            });
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new SentReceivedEvent(new ProgramChangeEvent((SevenBitNumber)71), TimeSpan.FromMilliseconds(300)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                });
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_ChangeObject_ProgramChange_2()
        {
            var objectToChange1 = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);
            var objectToChange2 = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)50)).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
                objectToChange1,
                objectToChange2,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange1,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap));
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange2,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap));
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new SentReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70), TimeSpan.FromMilliseconds(300)),
                    new SentReceivedEvent(new ProgramChangeEvent((SevenBitNumber)50), TimeSpan.FromMilliseconds(500)),
                    new SentReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70), TimeSpan.FromMilliseconds(600)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                });
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_ChangeObject_ProgramChange_3()
        {
            var objectToChange = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)80)).SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
                new Note((SevenBitNumber)50).SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap).SetLength(new MetricTimeSpan(0, 0, 0, 200), TempoMap),
                objectToChange,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 800), TempoMap));
                        CheckDuration(TimeSpan.FromMilliseconds(900), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new SentReceivedEvent(new ProgramChangeEvent((SevenBitNumber)80), TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new ProgramChangeEvent(), TimeSpan.FromMilliseconds(300)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(700)),
                    new SentReceivedEvent(new ProgramChangeEvent((SevenBitNumber)80), TimeSpan.FromMilliseconds(800)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(900)),
                });
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_ChangeObject_ProgramChange_4()
        {
            var objectToChange1 = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);
            var objectToChange2 = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)70) { Channel = (FourBitNumber)4 }).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
                objectToChange1,
                objectToChange2,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange1,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap));
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange2,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap));
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new SentReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70), TimeSpan.FromMilliseconds(300)),
                    new SentReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(500)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                });
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_ChangeObject_PitchBend_1()
        {
            var objectToChange = new TimedEvent(new PitchBendEvent(7000)).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
                objectToChange,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);
                                ((PitchBendEvent)((TimedEvent)obj).Event).PitchValue = 7100;
                            });
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new SentReceivedEvent(new PitchBendEvent(7100), TimeSpan.FromMilliseconds(300)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                },
                setupPlayback: playback => playback.TrackPitchValue = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_ChangeObject_PitchBend_2()
        {
            var objectToChange1 = new TimedEvent(new PitchBendEvent(7000)).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);
            var objectToChange2 = new TimedEvent(new PitchBendEvent(5000)).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
                objectToChange1,
                objectToChange2,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange1,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap));
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange2,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap));
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new SentReceivedEvent(new PitchBendEvent(7000), TimeSpan.FromMilliseconds(300)),
                    new SentReceivedEvent(new PitchBendEvent(5000), TimeSpan.FromMilliseconds(500)),
                    new SentReceivedEvent(new PitchBendEvent(7000), TimeSpan.FromMilliseconds(600)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                },
                setupPlayback: playback => playback.TrackPitchValue = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_ChangeObject_PitchBend_3()
        {
            var objectToChange = new TimedEvent(new PitchBendEvent(8000)).SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
                new Note((SevenBitNumber)50).SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap).SetLength(new MetricTimeSpan(0, 0, 0, 200), TempoMap),
                objectToChange,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 800), TempoMap));
                        CheckDuration(TimeSpan.FromMilliseconds(900), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new SentReceivedEvent(new PitchBendEvent(8000), TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new PitchBendEvent(), TimeSpan.FromMilliseconds(300)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(700)),
                    new SentReceivedEvent(new PitchBendEvent(8000), TimeSpan.FromMilliseconds(800)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(900)),
                },
                setupPlayback: playback => playback.TrackPitchValue = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_ChangeObject_PitchBend_4()
        {
            var objectToChange1 = new TimedEvent(new PitchBendEvent(7000)).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);
            var objectToChange2 = new TimedEvent(new PitchBendEvent(7000) { Channel = (FourBitNumber)4 }).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
                objectToChange1,
                objectToChange2,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange1,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap));
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange2,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap));
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new SentReceivedEvent(new PitchBendEvent(7000), TimeSpan.FromMilliseconds(300)),
                    new SentReceivedEvent(new PitchBendEvent(7000) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(500)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                },
                setupPlayback: playback => playback.TrackPitchValue = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_ChangeObject_ControlChange_1()
        {
            var objectToChange = new TimedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
                objectToChange,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);
                                ((ControlChangeEvent)((TimedEvent)obj).Event).ControlValue = (SevenBitNumber)71;
                            });
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new SentReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)71), TimeSpan.FromMilliseconds(300)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                },
                setupPlayback: playback => playback.TrackControlValue = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_ChangeObject_ControlChange_2()
        {
            var objectToChange1 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);
            var objectToChange2 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)50)).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
                objectToChange1,
                objectToChange2,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange1,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap));
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange2,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap));
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new SentReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)70), TimeSpan.FromMilliseconds(300)),
                    new SentReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)50), TimeSpan.FromMilliseconds(500)),
                    new SentReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)70), TimeSpan.FromMilliseconds(600)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                },
                setupPlayback: playback => playback.TrackControlValue = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_ChangeObject_ControlChange_3()
        {
            var objectToChange = new TimedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)80)).SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
                new Note((SevenBitNumber)50).SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap).SetLength(new MetricTimeSpan(0, 0, 0, 200), TempoMap),
                objectToChange,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 800), TempoMap));
                        CheckDuration(TimeSpan.FromMilliseconds(900), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new SentReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)80), TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)0), TimeSpan.FromMilliseconds(300)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(700)),
                    new SentReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)80), TimeSpan.FromMilliseconds(800)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(900)),
                },
                setupPlayback: playback => playback.TrackControlValue = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_ChangeObject_ControlChange_4()
        {
            var objectToChange1 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);
            var objectToChange2 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)70) { Channel = (FourBitNumber)4 }).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
                objectToChange1,
                objectToChange2,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange1,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap));
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange2,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap));
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new SentReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)70), TimeSpan.FromMilliseconds(300)),
                    new SentReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)70) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(500)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                },
                setupPlayback: playback => playback.TrackControlValue = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_ChangeObject_TrackNotes_1()
        {
            // ----[   | ]----
            //         |
            // v v v v | v v v
            //         |
            // ------[ |   ]--

            var tempoMap = TempoMap;

            var objectToChange = new Note((SevenBitNumber)70)
                .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var initialObjects = new ITimedObject[]
            {
                objectToChange,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(500), playback);
                        collection.ChangeObject(
                            objectToChange,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap));
                        CheckDuration(TimeSpan.FromMilliseconds(400), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(200)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                },
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_ChangeObject_TrackNotes_2()
        {
            // -[   | ]-------
            //      |   
            // v v v|v v v v v
            //      |  
            // -----|-[     ]-

            var tempoMap = TempoMap;

            var objectToChange = new Note((SevenBitNumber)70)
                .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var initialObjects = new ITimedObject[]
            {
                objectToChange,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(400), playback);
                        collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                obj.SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);
                                ((Note)obj).Channel = (FourBitNumber)4;
                                ((Note)obj).NoteNumber = (SevenBitNumber)71;
                            });
                        CheckDuration(TimeSpan.FromMilliseconds(800), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(200)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)71, Note.DefaultVelocity) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(500)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)71, Note.DefaultOffVelocity) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(800)),
                },
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_ChangeObject_TrackNotes_3()
        {
            // -------[  |  ]-
            //           |
            // v v v v v | v v
            //           |
            // -[     ]--|----

            var tempoMap = TempoMap;

            var objectToChange = new Note((SevenBitNumber)70)
                .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var initialObjects = new ITimedObject[]
            {
                objectToChange,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(400, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                        collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                obj.SetTime(new MetricTimeSpan(), TempoMap);
                                ((Note)obj).Velocity = (SevenBitNumber)30;
                            });
                        CheckDuration(TimeSpan.FromMilliseconds(300), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(300)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                },
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_ChangeObject_TrackNotes_4()
        {
            // ----------|-[     ]-
            //           |
            // v v v v v | v v v v
            //           |
            // -[     ]--|---------

            var tempoMap = TempoMap;

            var objectToChange = new Note((SevenBitNumber)70)
                .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 200), TempoMap);
            var initialObjects = new ITimedObject[]
            {
                objectToChange,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(350, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                        collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);
                                ((Note)obj).Velocity = (SevenBitNumber)30;
                            });
                        CheckDuration(TimeSpan.FromMilliseconds(300), playback);
                    }),
                },
                expectedReceivedEvents: Array.Empty<SentReceivedEvent>(),
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_ChangeObject_TrackNotes_5()
        {
            // -[  1| ]---------------
            // -----|---[  2  ]-------
            //      |
            // v v v|v v v v v v v v v
            //      |
            // -----|-[  1  ]---------
            // -----|---[  2  ]-------
            //      |
            //      +-----+
            //            |
            // -------[  1| ]---------
            // ---------[ |2  ]-------
            //            |
            // v v v v v v|v v v v v v
            //            |
            // -------[  1| ]---------
            // -----------|---[  2  ]-
            //            |
            //            +-------+
            //                    |
            // -------[  1  ]-----|---
            // ----------- ---[  2| ]-
            //                    |
            // v v v v v v v v v v|v v
            //                    |
            // -------[  1  ]-----|---
            // -------[  2  ]-----|---


            var tempoMap = TempoMap;

            var objectToChange1 = new Note((SevenBitNumber)50)
                .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var objectToChange2 = new Note((SevenBitNumber)60)
                .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                objectToChange1,
                objectToChange2,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                        collection.ChangeObject(
                            objectToChange1,
                            obj =>
                            {
                                obj.SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
                                ((Note)obj).Velocity = (SevenBitNumber)30;
                            });
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                        collection.ChangeObject(
                            objectToChange2,
                            obj =>
                            {
                                obj.SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap);
                                ((Note)obj).NoteNumber = (SevenBitNumber)80;
                            });
                        CheckDuration(TimeSpan.FromMilliseconds(1000), playback);
                    }),
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(1000), playback);
                        collection.ChangeObject(
                            objectToChange2,
                            obj =>
                            {
                                obj.SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
                                ((Note)obj).Channel = (FourBitNumber)5;
                                playback.MoveToStart();
                            });
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(200)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)50, (SevenBitNumber)30), TimeSpan.FromMilliseconds(300)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)60, Note.DefaultVelocity), TimeSpan.FromMilliseconds(400)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)60, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(500)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(600)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)80, Note.DefaultVelocity), TimeSpan.FromMilliseconds(700)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)80, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)50, (SevenBitNumber)30), TimeSpan.FromMilliseconds(1100)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)80, Note.DefaultVelocity) { Channel = (FourBitNumber)5 }, TimeSpan.FromMilliseconds(1100)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1400)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)80, Note.DefaultOffVelocity) { Channel = (FourBitNumber)5 }, TimeSpan.FromMilliseconds(1400)),
                },
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_ChangeObject_Chord_1()
        {
            var note1 = new Note((SevenBitNumber)70);
            var note2 = new Note((SevenBitNumber)50);
            var note3 = new Note((SevenBitNumber)90);

            var objectToChange = new Chord(note1, note2, note3);

            objectToChange
                .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            
            var initialObjects = new ITimedObject[]
            {
                objectToChange,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(50, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                var chord = (Chord)obj;
                                chord.Notes.Remove(note1);
                            });
                        CheckDuration(TimeSpan.FromMilliseconds(400), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                },
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_ChangeObject_Chord_2()
        {
            var note1 = new Note((SevenBitNumber)70);
            var note2 = new Note((SevenBitNumber)50);
            var note3 = new Note((SevenBitNumber)90)
                .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var objectToChange = new Chord(note1, note2);

            objectToChange
                .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                objectToChange,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(50, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                var chord = (Chord)obj;
                                chord.Notes.Remove(note2);
                                chord.Notes.Add(note3);
                            });
                        CheckDuration(TimeSpan.FromMilliseconds(400), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                },
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_ChangeObject_Chord_3()
        {
            var note1 = new Note((SevenBitNumber)70);
            var note2 = new Note((SevenBitNumber)50);

            var objectToChange = new Chord(note1, note2);

            objectToChange
                .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                objectToChange,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(50, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                var chord = (Chord)obj;
                                chord.Notes
                                    .Last()
                                    .SetTime(new MetricTimeSpan(0, 0, 0, 150), TempoMap)
                                    .SetLength(new MetricTimeSpan(0, 0, 0, 200), TempoMap);
                            });
                        CheckDuration(TimeSpan.FromMilliseconds(400), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(150)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(350)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                },
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_ChangeObject_Chord_4()
        {
            var note1 = new Note((SevenBitNumber)70);
            var note2 = new Note((SevenBitNumber)50);

            var objectToChange = new Chord(note1, note2);

            objectToChange
                .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                objectToChange,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(50, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                var chord = (Chord)obj;
                                chord.Notes.Clear();
                            });
                        CheckDuration(TimeSpan.FromMilliseconds(0), playback);
                    }),
                },
                expectedReceivedEvents: new SentReceivedEvent[]
                {
                },
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_ChangeObject_Chord_5()
        {
            var note1 = new Note((SevenBitNumber)70);
            var note2 = new Note((SevenBitNumber)50);
            var note3 = new Note((SevenBitNumber)90);

            var objectToChange = new Chord(note1, note2, note3);

            objectToChange
                .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                objectToChange,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                var chord = (Chord)obj;
                                chord.Notes.Remove(note1);
                            });
                        CheckDuration(TimeSpan.FromMilliseconds(400), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(200)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                },
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_ChangeObject_Chord_6()
        {
            var note1 = new Note((SevenBitNumber)70);
            var note2 = new Note((SevenBitNumber)50);
            var note3 = new Note((SevenBitNumber)90)
                .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var objectToChange = new Chord(note1, note2);

            objectToChange
                .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                objectToChange,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                var chord = (Chord)obj;
                                chord.Notes.Remove(note2);
                                chord.Notes.Add(note3);
                            });
                        CheckDuration(TimeSpan.FromMilliseconds(400), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(200)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(200)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                },
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_ChangeObject_Chord_7()
        {
            var note1 = new Note((SevenBitNumber)70);
            var note2 = new Note((SevenBitNumber)50);

            var objectToChange = new Chord(note1, note2);

            objectToChange
                .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                objectToChange,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                var chord = (Chord)obj;
                                chord.Notes
                                    .Last()
                                    .SetTime(new MetricTimeSpan(0, 0, 0, 150), TempoMap)
                                    .SetLength(new MetricTimeSpan(0, 0, 0, 200), TempoMap);
                            });
                        CheckDuration(TimeSpan.FromMilliseconds(400), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(350)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                },
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_ChangeObject_Chord_8()
        {
            var note1 = new Note((SevenBitNumber)70);
            var note2 = new Note((SevenBitNumber)50);

            var objectToChange = new Chord(note1, note2);

            objectToChange
                .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                objectToChange,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                var chord = (Chord)obj;
                                chord.Notes.Clear();
                            });
                        CheckDuration(TimeSpan.FromMilliseconds(0), playback);
                    }),
                },
                expectedReceivedEvents: new SentReceivedEvent[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(200)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(200)),
                },
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_ChangeObject_1()
        {

            var objectToChange = new TimedEvent(new TextEvent("A")).SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
                objectToChange,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap));
                        CheckDuration(TimeSpan.FromMilliseconds(500), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new SentReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(300)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(500)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(500)),
                    new SentReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(800)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                },
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_ChangeObject_2()
        {
            var objectToChange = new TimedEvent(new TextEvent("A")).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
                objectToChange,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap));
                        CheckDuration(TimeSpan.FromMilliseconds(500), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(500)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(500)),
                    new SentReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(600)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
                },
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_ChangeObject_ProgramChange_1()
        {
            var objectToChange = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
                objectToChange,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);
                                ((ProgramChangeEvent)((TimedEvent)obj).Event).ProgramNumber = (SevenBitNumber)71;
                            });
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new SentReceivedEvent(new ProgramChangeEvent((SevenBitNumber)71), TimeSpan.FromMilliseconds(300)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(700)),
                    new SentReceivedEvent(new ProgramChangeEvent((SevenBitNumber)71), TimeSpan.FromMilliseconds(800)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1400)),
                },
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_ChangeObject_ProgramChange_2()
        {
            var objectToChange1 = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);
            var objectToChange2 = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)50)).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
                objectToChange1,
                objectToChange2,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange1,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap));
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange2,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap));
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new SentReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70), TimeSpan.FromMilliseconds(300)),
                    new SentReceivedEvent(new ProgramChangeEvent((SevenBitNumber)50), TimeSpan.FromMilliseconds(500)),
                    new SentReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70), TimeSpan.FromMilliseconds(600)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(700)),
                    new SentReceivedEvent(new ProgramChangeEvent((SevenBitNumber)50), TimeSpan.FromMilliseconds(800)),
                    new SentReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70), TimeSpan.FromMilliseconds(900)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1400)),
                },
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_ChangeObject_ProgramChange_3()
        {
            var objectToChange = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)80)).SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
                new Note((SevenBitNumber)50).SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap).SetLength(new MetricTimeSpan(0, 0, 0, 200), TempoMap),
                objectToChange,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 800), TempoMap));
                        CheckDuration(TimeSpan.FromMilliseconds(900), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new SentReceivedEvent(new ProgramChangeEvent((SevenBitNumber)80), TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new ProgramChangeEvent(), TimeSpan.FromMilliseconds(300)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(700)),
                    new SentReceivedEvent(new ProgramChangeEvent((SevenBitNumber)80), TimeSpan.FromMilliseconds(800)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(900)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(900)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1600)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(1600)),
                    new SentReceivedEvent(new ProgramChangeEvent((SevenBitNumber)80), TimeSpan.FromMilliseconds(1700)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1800)),
                },
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_ChangeObject_ProgramChange_4()
        {
            var objectToChange1 = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);
            var objectToChange2 = new TimedEvent(new ProgramChangeEvent((SevenBitNumber)70) { Channel = (FourBitNumber)4 }).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
                objectToChange1,
                objectToChange2,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange1,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap));
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange2,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap));
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new SentReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70), TimeSpan.FromMilliseconds(300)),
                    new SentReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(500)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(700)),
                    new SentReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(800)),
                    new SentReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70), TimeSpan.FromMilliseconds(900)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1400)),
                },
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_ChangeObject_PitchBend_1()
        {
            var objectToChange = new TimedEvent(new PitchBendEvent(7000)).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
                objectToChange,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);
                                ((PitchBendEvent)((TimedEvent)obj).Event).PitchValue = 7100;
                            });
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new SentReceivedEvent(new PitchBendEvent(7100), TimeSpan.FromMilliseconds(300)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(700)),
                    new SentReceivedEvent(new PitchBendEvent(7100), TimeSpan.FromMilliseconds(800)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1400)),
                },
                setupPlayback: playback => playback.TrackPitchValue = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_ChangeObject_PitchBend_2()
        {
            var objectToChange1 = new TimedEvent(new PitchBendEvent(7000)).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);
            var objectToChange2 = new TimedEvent(new PitchBendEvent(5000)).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
                objectToChange1,
                objectToChange2,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange1,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap));
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange2,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap));
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new SentReceivedEvent(new PitchBendEvent(7000), TimeSpan.FromMilliseconds(300)),
                    new SentReceivedEvent(new PitchBendEvent(5000), TimeSpan.FromMilliseconds(500)),
                    new SentReceivedEvent(new PitchBendEvent(7000), TimeSpan.FromMilliseconds(600)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(700)),
                    new SentReceivedEvent(new PitchBendEvent(5000), TimeSpan.FromMilliseconds(800)),
                    new SentReceivedEvent(new PitchBendEvent(7000), TimeSpan.FromMilliseconds(900)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1400)),
                },
                setupPlayback: playback => playback.TrackPitchValue = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_ChangeObject_PitchBend_3()
        {
            var objectToChange = new TimedEvent(new PitchBendEvent(8000)).SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
                new Note((SevenBitNumber)50).SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap).SetLength(new MetricTimeSpan(0, 0, 0, 200), TempoMap),
                objectToChange,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 800), TempoMap));
                        CheckDuration(TimeSpan.FromMilliseconds(900), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new SentReceivedEvent(new PitchBendEvent(8000), TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new PitchBendEvent(), TimeSpan.FromMilliseconds(300)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(700)),
                    new SentReceivedEvent(new PitchBendEvent(8000), TimeSpan.FromMilliseconds(800)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(900)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(900)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1600)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(1600)),
                    new SentReceivedEvent(new PitchBendEvent(8000), TimeSpan.FromMilliseconds(1700)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1800)),
                },
                setupPlayback: playback => playback.TrackPitchValue = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_ChangeObject_PitchBend_4()
        {
            var objectToChange1 = new TimedEvent(new PitchBendEvent(7000)).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);
            var objectToChange2 = new TimedEvent(new PitchBendEvent(7000) { Channel = (FourBitNumber)4 }).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
                objectToChange1,
                objectToChange2,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange1,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap));
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange2,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap));
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new SentReceivedEvent(new PitchBendEvent(7000), TimeSpan.FromMilliseconds(300)),
                    new SentReceivedEvent(new PitchBendEvent(7000) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(500)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(700)),
                    new SentReceivedEvent(new PitchBendEvent(7000) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(800)),
                    new SentReceivedEvent(new PitchBendEvent(7000), TimeSpan.FromMilliseconds(900)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1400)),
                },
                setupPlayback: playback => playback.TrackPitchValue = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_ChangeObject_ControlChange_1()
        {
            var objectToChange = new TimedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
                objectToChange,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);
                                ((ControlChangeEvent)((TimedEvent)obj).Event).ControlValue = (SevenBitNumber)71;
                            });
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new SentReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)71), TimeSpan.FromMilliseconds(300)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(700)),
                    new SentReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)71), TimeSpan.FromMilliseconds(800)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1400)),
                },
                setupPlayback: playback => playback.TrackControlValue = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_ChangeObject_ControlChange_2()
        {
            var objectToChange1 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);
            var objectToChange2 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)50)).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
                objectToChange1,
                objectToChange2,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange1,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap));
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange2,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap));
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new SentReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)70), TimeSpan.FromMilliseconds(300)),
                    new SentReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)50), TimeSpan.FromMilliseconds(500)),
                    new SentReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)70), TimeSpan.FromMilliseconds(600)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(700)),
                    new SentReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)50), TimeSpan.FromMilliseconds(800)),
                    new SentReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)70), TimeSpan.FromMilliseconds(900)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1400)),
                },
                setupPlayback: playback => playback.TrackControlValue = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_ChangeObject_ControlChange_3()
        {
            var objectToChange = new TimedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)80)).SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
                new Note((SevenBitNumber)50).SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap).SetLength(new MetricTimeSpan(0, 0, 0, 200), TempoMap),
                objectToChange,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 800), TempoMap));
                        CheckDuration(TimeSpan.FromMilliseconds(900), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new SentReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)80), TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)0), TimeSpan.FromMilliseconds(300)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(700)),
                    new SentReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)80), TimeSpan.FromMilliseconds(800)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(900)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(900)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1600)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(1600)),
                    new SentReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)80), TimeSpan.FromMilliseconds(1700)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1800)),
                },
                setupPlayback: playback => playback.TrackControlValue = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_ChangeObject_ControlChange_4()
        {
            var objectToChange1 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)70)).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);
            var objectToChange2 = new TimedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)70) { Channel = (FourBitNumber)4 }).SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 0, 700), TempoMap),
                objectToChange1,
                objectToChange2,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange1,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap));
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange2,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap));
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new SentReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)70), TimeSpan.FromMilliseconds(300)),
                    new SentReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)70) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(500)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(700)),
                    new SentReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)70) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(800)),
                    new SentReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)70), TimeSpan.FromMilliseconds(900)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1400)),
                },
                setupPlayback: playback => playback.TrackControlValue = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_ChangeObject_TrackNotes_1()
        {
            // ----[   | ]----
            //         |
            // v v v v | v v v
            //         |
            // ------[ |   ]--

            var tempoMap = TempoMap;

            var objectToChange = new Note((SevenBitNumber)70)
                .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var initialObjects = new ITimedObject[]
            {
                objectToChange,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(500), playback);
                        collection.ChangeObject(
                            objectToChange,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap));
                        CheckDuration(TimeSpan.FromMilliseconds(400), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(200)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(500)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                },
                setupPlayback: playback => playback.TrackNotes = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_ChangeObject_TrackNotes_2()
        {
            // -[   | ]-------
            //      |   
            // v v v|v v v v v
            //      |  
            // -----|-[     ]-

            var tempoMap = TempoMap;

            var objectToChange = new Note((SevenBitNumber)70)
                .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var initialObjects = new ITimedObject[]
            {
                objectToChange,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(400), playback);
                        collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                obj.SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);
                                ((Note)obj).Channel = (FourBitNumber)4;
                                ((Note)obj).NoteNumber = (SevenBitNumber)71;
                            });
                        CheckDuration(TimeSpan.FromMilliseconds(800), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(200)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)71, Note.DefaultVelocity) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(500)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)71, Note.DefaultOffVelocity) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(800)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)71, Note.DefaultVelocity) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(1300)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)71, Note.DefaultOffVelocity) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(1600)),
                },
                setupPlayback: playback => playback.TrackNotes = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_ChangeObject_TrackNotes_3()
        {
            // -------[  |  ]-
            //           |
            // v v v v v | v v
            //           |
            // -[     ]--|----

            var tempoMap = TempoMap;

            var objectToChange = new Note((SevenBitNumber)70)
                .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var initialObjects = new ITimedObject[]
            {
                objectToChange,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(400, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                        collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                obj.SetTime(new MetricTimeSpan(), TempoMap);
                                ((Note)obj).Velocity = (SevenBitNumber)30;
                            });
                        CheckDuration(TimeSpan.FromMilliseconds(300), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(300)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)30), TimeSpan.FromMilliseconds(400)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                },
                setupPlayback: playback => playback.TrackNotes = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_ChangeObject_TrackNotes_4()
        {
            // ----------|-[     ]-
            //           |
            // v v v v v | v v v v
            //           |
            // -[     ]--|---------

            var tempoMap = TempoMap;

            var objectToChange = new Note((SevenBitNumber)70)
                .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 200), TempoMap);
            var initialObjects = new ITimedObject[]
            {
                objectToChange,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(350, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                        collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);
                                ((Note)obj).Velocity = (SevenBitNumber)30;
                            });
                        CheckDuration(TimeSpan.FromMilliseconds(300), playback);
                    }),
                },
                expectedReceivedEvents: new SentReceivedEvent[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)30), TimeSpan.FromMilliseconds(450)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(650)),
                },
                setupPlayback: playback => playback.TrackNotes = true,
                repeatsCount: 1);
        }

        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_ChangeObject_TrackNotes_5()
        {
            // -[  1| ]---------------
            // -----|---[  2  ]-------
            //      |
            // v v v|v v v v v v v v v
            //      |
            // -----|-[  1  ]---------
            // -----|---[  2  ]-------
            //      |
            //      +-----+
            //            |
            // -------[  1| ]---------
            // ---------[ |2  ]-------
            //            |
            // v v v v v v|v v v v v v
            //            |
            // -------[  1| ]---------
            // -----------|---[  2  ]-
            //            |
            //            +-------+
            //                    |
            // -------[  1  ]-----|---
            // ----------- ---[  2| ]-
            //                    |
            // v v v v v v v v v v|v v
            //                    |
            // -------[  1  ]-----|---
            // -------[  2  ]-----|---


            var tempoMap = TempoMap;

            var objectToChange1 = new Note((SevenBitNumber)50)
                .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
            var objectToChange2 = new Note((SevenBitNumber)60)
                .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                objectToChange1,
                objectToChange2,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                        collection.ChangeObject(
                            objectToChange1,
                            obj =>
                            {
                                obj.SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
                                ((Note)obj).Velocity = (SevenBitNumber)30;
                            });
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                    }),
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                        collection.ChangeObject(
                            objectToChange2,
                            obj =>
                            {
                                obj.SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap);
                                ((Note)obj).NoteNumber = (SevenBitNumber)80;
                            });
                        CheckDuration(TimeSpan.FromMilliseconds(1000), playback);
                    }),
                    new DynamicPlaybackAction(300, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(1000), playback);
                        collection.ChangeObject(
                            objectToChange2,
                            obj =>
                            {
                                obj.SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
                                ((Note)obj).Channel = (FourBitNumber)5;
                                playback.MoveToStart();
                            });
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(200)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)50, (SevenBitNumber)30), TimeSpan.FromMilliseconds(300)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)60, Note.DefaultVelocity), TimeSpan.FromMilliseconds(400)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)60, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(500)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(600)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)80, Note.DefaultVelocity), TimeSpan.FromMilliseconds(700)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)80, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)50, (SevenBitNumber)30), TimeSpan.FromMilliseconds(1100)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)80, Note.DefaultVelocity) { Channel = (FourBitNumber)5 }, TimeSpan.FromMilliseconds(1100)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1400)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)80, Note.DefaultOffVelocity) { Channel = (FourBitNumber)5 }, TimeSpan.FromMilliseconds(1400)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)50, (SevenBitNumber)30), TimeSpan.FromMilliseconds(1700)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)80, Note.DefaultVelocity) { Channel = (FourBitNumber)5 }, TimeSpan.FromMilliseconds(1700)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(2000)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)80, Note.DefaultOffVelocity) { Channel = (FourBitNumber)5 }, TimeSpan.FromMilliseconds(2000)),
                },
                setupPlayback: playback => playback.TrackNotes = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_ChangeObject_Chord_1()
        {
            var note1 = new Note((SevenBitNumber)70);
            var note2 = new Note((SevenBitNumber)50);
            var note3 = new Note((SevenBitNumber)90);

            var objectToChange = new Chord(note1, note2, note3);

            objectToChange
                .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                objectToChange,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(50, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(400), playback);
                        collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                var chord = (Chord)obj;
                                chord.Notes.Remove(note1);
                            });
                        CheckDuration(TimeSpan.FromMilliseconds(400), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(500)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(500)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                },
                setupPlayback: playback => playback.TrackNotes = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_ChangeObject_Chord_2()
        {
            var note1 = new Note((SevenBitNumber)70);
            var note2 = new Note((SevenBitNumber)50);
            var note3 = new Note((SevenBitNumber)90)
                .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var objectToChange = new Chord(note1, note2);

            objectToChange
                .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                objectToChange,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(50, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                var chord = (Chord)obj;
                                chord.Notes.Remove(note2);
                                chord.Notes.Add(note3);
                            });
                        CheckDuration(TimeSpan.FromMilliseconds(400), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(500)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(500)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                },
                setupPlayback: playback => playback.TrackNotes = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_ChangeObject_Chord_3()
        {
            var note1 = new Note((SevenBitNumber)70);
            var note2 = new Note((SevenBitNumber)50);

            var objectToChange = new Chord(note1, note2);

            objectToChange
                .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                objectToChange,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(50, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                var chord = (Chord)obj;
                                chord.Notes
                                    .Last()
                                    .SetTime(new MetricTimeSpan(0, 0, 0, 150), TempoMap)
                                    .SetLength(new MetricTimeSpan(0, 0, 0, 200), TempoMap);
                                CheckDuration(TimeSpan.FromMilliseconds(400), playback);
                            });
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(150)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(350)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(500)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(550)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(750)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                },
                setupPlayback: playback => playback.TrackNotes = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_ChangeObject_Chord_4()
        {
            var note1 = new Note((SevenBitNumber)70);
            var note2 = new Note((SevenBitNumber)50);

            var objectToChange = new Chord(note1, note2);

            objectToChange
                .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                objectToChange,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(50, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(400), playback);
                        collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                var chord = (Chord)obj;
                                chord.Notes.Clear();
                            });
                        CheckDuration(TimeSpan.FromMilliseconds(0), playback);
                    }),
                },
                expectedReceivedEvents: new SentReceivedEvent[]
                {
                },
                setupPlayback: playback => playback.TrackNotes = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_ChangeObject_Chord_5()
        {
            var note1 = new Note((SevenBitNumber)70);
            var note2 = new Note((SevenBitNumber)50);
            var note3 = new Note((SevenBitNumber)90);

            var objectToChange = new Chord(note1, note2, note3);

            objectToChange
                .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                objectToChange,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                var chord = (Chord)obj;
                                chord.Notes.Remove(note1);
                            });
                        CheckDuration(TimeSpan.FromMilliseconds(400), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(200)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(500)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(500)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                },
                setupPlayback: playback => playback.TrackNotes = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_ChangeObject_Chord_6()
        {
            var note1 = new Note((SevenBitNumber)70);
            var note2 = new Note((SevenBitNumber)50);
            var note3 = new Note((SevenBitNumber)90)
                .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var objectToChange = new Chord(note1, note2);

            objectToChange
                .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                objectToChange,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                var chord = (Chord)obj;
                                chord.Notes.Remove(note2);
                                chord.Notes.Add(note3);
                            });
                        CheckDuration(TimeSpan.FromMilliseconds(400), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(200)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(200)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(500)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(500)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                },
                setupPlayback: playback => playback.TrackNotes = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_ChangeObject_Chord_7()
        {
            var note1 = new Note((SevenBitNumber)70);
            var note2 = new Note((SevenBitNumber)50);

            var objectToChange = new Chord(note1, note2);

            objectToChange
                .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                objectToChange,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                var chord = (Chord)obj;
                                chord.Notes
                                    .Last()
                                    .SetTime(new MetricTimeSpan(0, 0, 0, 150), TempoMap)
                                    .SetLength(new MetricTimeSpan(0, 0, 0, 200), TempoMap);
                            });
                        CheckDuration(TimeSpan.FromMilliseconds(400), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(350)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(500)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(550)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(750)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                },
                setupPlayback: playback => playback.TrackNotes = true,
                repeatsCount: 1);
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Loop_ChangeObject_Chord_8()
        {
            var note1 = new Note((SevenBitNumber)70);
            var note2 = new Note((SevenBitNumber)50);

            var objectToChange = new Chord(note1, note2);

            objectToChange
                .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                objectToChange,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                var chord = (Chord)obj;
                                chord.Notes.Clear();
                            });
                        CheckDuration(TimeSpan.FromMilliseconds(0), playback);
                    }),
                },
                expectedReceivedEvents: new SentReceivedEvent[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(200)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(200)),
                },
                setupPlayback: playback => playback.TrackNotes = true,
                repeatsCount: 1);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_ChangeObject_NoteEvents_1()
        {
            var noteOnEvent = new TimedEvent(new NoteOnEvent((SevenBitNumber)50, SevenBitNumber.MaxValue))
                .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                noteOnEvent,
                new TimedEvent(new NoteOffEvent((SevenBitNumber)50, SevenBitNumber.MinValue))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap),
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(400), playback);
                        collection.ChangeObject(
                            noteOnEvent,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap));
                        CheckDuration(TimeSpan.FromMilliseconds(400), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)50, SevenBitNumber.MaxValue), TimeSpan.FromMilliseconds(300)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)50, SevenBitNumber.MinValue), TimeSpan.FromMilliseconds(400)),
                },
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_ChangeObject_NoteEvents_2()
        {
            var noteOnEvent = new TimedEvent(new NoteOnEvent((SevenBitNumber)50, SevenBitNumber.MaxValue))
                .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                noteOnEvent,
                new TimedEvent(new NoteOffEvent((SevenBitNumber)50, SevenBitNumber.MinValue))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap),
            };

            var notesStarted = new List<Note>();
            var notesFinished = new List<Note>();

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            noteOnEvent,
                            obj =>
                            {
                                ((NoteOnEvent)((TimedEvent)obj).Event).Velocity = (SevenBitNumber)70;
                                obj.SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
                            });
                        CheckDuration(TimeSpan.FromMilliseconds(400), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)50, SevenBitNumber.MaxValue), TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)50, SevenBitNumber.MinValue), TimeSpan.FromMilliseconds(200)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)50, (SevenBitNumber)70), TimeSpan.FromMilliseconds(300)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)50, SevenBitNumber.MinValue), TimeSpan.FromMilliseconds(400)),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = true;
                    playback.NotesPlaybackStarted += (_, e) => notesStarted.AddRange(e.Notes);
                    playback.NotesPlaybackFinished += (_, e) => notesFinished.AddRange(e.Notes);
                });

            CollectionAssert.AreEquivalent(notesStarted, notesFinished, "Mismatch.");
            ClassicAssert.AreEqual(2, notesStarted.Count, "Invalid count of started notes.");
            ClassicAssert.AreEqual(2, notesFinished.Count, "Invalid count of finished notes.");

            MidiAsserts.AreEqual(
                new[]
                {
                    new Note((SevenBitNumber)50) { Velocity = SevenBitNumber.MaxValue }
                        .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap)
                        .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap),
                    new Note((SevenBitNumber)50) { Velocity = (SevenBitNumber)70 }
                        .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap)
                        .SetLength(new MetricTimeSpan(0, 0, 0, 100), TempoMap),
                },
                notesStarted,
                "Invalid notes.");
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_ChangeObject_NoteEvents_3()
        {
            var noteOnEvent = new TimedEvent(new NoteOnEvent((SevenBitNumber)50, SevenBitNumber.MaxValue))
                .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                noteOnEvent,
                new TimedEvent(new NoteOffEvent((SevenBitNumber)50, SevenBitNumber.MinValue))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap),
            };

            var notesStarted = new List<Note>();
            var notesFinished = new List<Note>();

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200, (playback, collection) =>
                    {
                        collection.ChangeObject(
                            noteOnEvent,
                            obj => ((NoteOnEvent)((TimedEvent)obj).Event).Channel = (FourBitNumber)4);
                        CheckDuration(TimeSpan.FromMilliseconds(0), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)50, SevenBitNumber.MaxValue), TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)50, SevenBitNumber.MinValue), TimeSpan.FromMilliseconds(200)),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = true;
                    playback.NotesPlaybackStarted += (_, e) => notesStarted.AddRange(e.Notes);
                    playback.NotesPlaybackFinished += (_, e) => notesFinished.AddRange(e.Notes);
                });

            CollectionAssert.AreEquivalent(notesStarted, notesFinished, "Mismatch.");
            ClassicAssert.AreEqual(1, notesStarted.Count, "Invalid count of started notes.");
            ClassicAssert.AreEqual(1, notesFinished.Count, "Invalid count of finished notes.");

            MidiAsserts.AreEqual(
                new[]
                {
                    new Note((SevenBitNumber)50) { Velocity = SevenBitNumber.MaxValue }
                        .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap)
                        .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap),
                },
                notesStarted,
                "Invalid notes.");
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_ChangeObject_NoteEvents_4()
        {
            var noteOffEvent = new TimedEvent(new NoteOffEvent((SevenBitNumber)50, SevenBitNumber.MinValue))
                .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)50, SevenBitNumber.MaxValue))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap),
                noteOffEvent,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(50, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(400), playback);
                        collection.ChangeObject(
                            noteOffEvent,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap));
                        CheckDuration(TimeSpan.FromMilliseconds(300), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)50, SevenBitNumber.MaxValue), TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)50, SevenBitNumber.MinValue), TimeSpan.FromMilliseconds(300)),
                },
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_ChangeObject_NoteEvents_5()
        {
            var noteOffEvent = new TimedEvent(new NoteOffEvent((SevenBitNumber)50, SevenBitNumber.MinValue))
                .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)50, SevenBitNumber.MaxValue))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap),
                noteOffEvent,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(400), playback);
                        collection.ChangeObject(
                            noteOffEvent,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap));
                        CheckDuration(TimeSpan.FromMilliseconds(300), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)50, SevenBitNumber.MaxValue), TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)50, SevenBitNumber.MinValue), TimeSpan.FromMilliseconds(300)),
                },
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_ChangeObject_NoteEvents_6()
        {
            var noteOffEvent = new TimedEvent(new NoteOffEvent((SevenBitNumber)50, SevenBitNumber.MinValue))
                .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)50, SevenBitNumber.MaxValue))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap),
                noteOffEvent,
            };

            var notesStarted = new List<Note>();
            var notesFinished = new List<Note>();

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(400), playback);
                        collection.ChangeObject(
                            noteOffEvent,
                            obj =>
                            {
                                ((NoteOffEvent)((TimedEvent)obj).Event).Velocity = (SevenBitNumber)70;
                                obj.SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
                            });
                        CheckDuration(TimeSpan.FromMilliseconds(300), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)50, SevenBitNumber.MaxValue), TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)50, (SevenBitNumber)70), TimeSpan.FromMilliseconds(300)),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = true;
                    playback.NotesPlaybackStarted += (_, e) => notesStarted.AddRange(e.Notes);
                    playback.NotesPlaybackFinished += (_, e) => notesFinished.AddRange(e.Notes);
                });

            ClassicAssert.AreEqual(1, notesStarted.Count, "Invalid count of started notes.");
            ClassicAssert.AreEqual(1, notesFinished.Count, "Invalid count of finished notes.");

            MidiAsserts.AreEqual(
                new[]
                {
                    new Note((SevenBitNumber)50) { Velocity = SevenBitNumber.MaxValue }
                        .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap)
                        .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap),
                },
                notesStarted,
                "Invalid notes started.");

            MidiAsserts.AreEqual(
                new[]
                {
                    new Note((SevenBitNumber)50) { Velocity = SevenBitNumber.MaxValue, OffVelocity = (SevenBitNumber)70 }
                        .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap)
                        .SetLength(new MetricTimeSpan(0, 0, 0, 200), TempoMap),
                },
                notesFinished,
                "Invalid notes finished.");
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_ChangeObject_NoteEvents_7()
        {
            var noteOffEvent = new TimedEvent(new NoteOffEvent((SevenBitNumber)50, SevenBitNumber.MinValue))
                .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)50, SevenBitNumber.MaxValue))
                    .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap),
                noteOffEvent,
            };

            var notesStarted = new List<Note>();
            var notesFinished = new List<Note>();

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(200, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(400), playback);
                        collection.ChangeObject(
                            noteOffEvent,
                            obj => ((NoteOffEvent)((TimedEvent)obj).Event).Channel = (FourBitNumber)4);
                        CheckDuration(TimeSpan.FromMilliseconds(0), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)50, SevenBitNumber.MaxValue), TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)50, SevenBitNumber.MinValue), TimeSpan.FromMilliseconds(200)),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = true;
                    playback.NotesPlaybackStarted += (_, e) => notesStarted.AddRange(e.Notes);
                    playback.NotesPlaybackFinished += (_, e) => notesFinished.AddRange(e.Notes);
                });

            ClassicAssert.AreEqual(1, notesStarted.Count, "Invalid count of started notes.");
            ClassicAssert.AreEqual(1, notesFinished.Count, "Invalid count of finished notes.");

            MidiAsserts.AreEqual(
                new[]
                {
                    new Note((SevenBitNumber)50) { Velocity = SevenBitNumber.MaxValue }
                        .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap)
                        .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap),
                },
                notesStarted,
                "Invalid notes.");
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_ChangeObject_SetTempo_1()
        {
            var objectToChange = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                .SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 200), TempoMap),
                objectToChange,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(600), playback);
                        collection.ChangeObject(
                            objectToChange,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap));
                        CheckDuration(TimeSpan.FromMilliseconds(350), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2), TimeSpan.FromMilliseconds(200)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(250)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(350)),
                },
                additionalChecks: (playback, _) => MidiAsserts.AreEqual(
                    AddTempoChanges(TempoMap, (TimeSpan.FromMilliseconds(200), new Tempo(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))),
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_ChangeObject_SetTempo_2()
        {
            var objectToChange = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                .SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
                objectToChange,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(700), playback);
                        collection.ChangeObject(
                            objectToChange,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap));
                        CheckDuration(TimeSpan.FromMilliseconds(500), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2), TimeSpan.FromMilliseconds(200)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(250)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(500)),
                },
                additionalChecks: (playback, _) => MidiAsserts.AreEqual(
                    AddTempoChanges(TempoMap, (TimeSpan.FromMilliseconds(200), new Tempo(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))),
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_ChangeObject_SetTempo_3()
        {
            var objectToChange = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
                objectToChange,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(500), playback);
                        collection.ChangeObject(
                            objectToChange,
                            obj => ((SetTempoEvent)((TimedEvent)obj).Event).MicrosecondsPerQuarterNote = SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 4);
                        CheckDuration(TimeSpan.FromMilliseconds(350), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 4), TimeSpan.FromMilliseconds(200)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(225)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(350)),
                },
                additionalChecks: (playback, _) => MidiAsserts.AreEqual(
                    AddTempoChanges(TempoMap, (TimeSpan.FromMilliseconds(200), new Tempo(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 4))),
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        [Retry(OnTheFlyChecksRetriesNumber)]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_ChangeObject_SetTempo_4()
        {
            var objectToChange = new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap);

            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap)
                    .SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap),
                objectToChange,
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(400, (playback, collection) =>
                    {
                        CheckDuration(TimeSpan.FromMilliseconds(500), playback);
                        collection.ChangeObject(
                            objectToChange,
                            obj => ((SetTempoEvent)((TimedEvent)obj).Event).MicrosecondsPerQuarterNote = SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 4);
                        CheckCurrentTime(playback, TimeSpan.FromMilliseconds(300), "after tempo change");
                        CheckDuration(TimeSpan.FromMilliseconds(350), playback);
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2), TimeSpan.FromMilliseconds(200)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(250)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(450)),
                },
                additionalChecks: (playback, _) => MidiAsserts.AreEqual(
                    AddTempoChanges(TempoMap, (TimeSpan.FromMilliseconds(200), new Tempo(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 4))),
                    playback.TempoMap,
                    "Invalid tempo map."));
        }

        #endregion
    }
}
