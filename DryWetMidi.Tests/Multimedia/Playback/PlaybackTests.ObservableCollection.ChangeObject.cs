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
                    new PlaybackChanger(400,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap))),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
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
                    new PlaybackChanger(200,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap))),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(500)),
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
                    new PlaybackChanger(300,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);
                                ((ProgramChangeEvent)((TimedEvent)obj).Event).ProgramNumber = (SevenBitNumber)71;
                            })),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)71), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
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
                    new PlaybackChanger(300,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange1,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap))),
                    new PlaybackChanger(300,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange2,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap))),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)50), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
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
                    new PlaybackChanger(300,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 800), TempoMap))),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)80), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new ProgramChangeEvent(), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)80), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(900)),
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
                    new PlaybackChanger(300,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange1,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap))),
                    new PlaybackChanger(300,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange2,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap))),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
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
                    new PlaybackChanger(300,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);
                                ((PitchBendEvent)((TimedEvent)obj).Event).PitchValue = 7100;
                            })),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new PitchBendEvent(7100), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
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
                    new PlaybackChanger(300,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange1,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap))),
                    new PlaybackChanger(300,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange2,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap))),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new PitchBendEvent(7000), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new PitchBendEvent(5000), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new PitchBendEvent(7000), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
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
                    new PlaybackChanger(300,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 800), TempoMap))),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new PitchBendEvent(8000), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new PitchBendEvent(), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new PitchBendEvent(8000), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(900)),
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
                    new PlaybackChanger(300,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange1,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap))),
                    new PlaybackChanger(300,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange2,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap))),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new PitchBendEvent(7000), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new PitchBendEvent(7000) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
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
                    new PlaybackChanger(300,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);
                                ((ControlChangeEvent)((TimedEvent)obj).Event).ControlValue = (SevenBitNumber)71;
                            })),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)71), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
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
                    new PlaybackChanger(300,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange1,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap))),
                    new PlaybackChanger(300,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange2,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap))),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)70), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)50), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)70), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
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
                    new PlaybackChanger(300,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 800), TempoMap))),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)80), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)0), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)80), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(900)),
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
                    new PlaybackChanger(300,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange1,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap))),
                    new PlaybackChanger(300,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange2,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap))),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)70), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)70) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
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
                    new PlaybackChanger(300,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap))),
                        },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
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
                    new PlaybackChanger(200,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                obj.SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);
                                ((Note)obj).Channel = (FourBitNumber)4;
                                ((Note)obj).NoteNumber = (SevenBitNumber)71;
                            })),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)71, Note.DefaultVelocity) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)71, Note.DefaultOffVelocity) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(800)),
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
                    new PlaybackChanger(400,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                obj.SetTime(new MetricTimeSpan(), TempoMap);
                                ((Note)obj).Velocity = (SevenBitNumber)30;
                            })),
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
                    new PlaybackChanger(350,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);
                                ((Note)obj).Velocity = (SevenBitNumber)30;
                            })),
                },
                expectedReceivedEvents: new ReceivedEvent[]
                {
                },
                setupPlayback: playback => playback.TrackNotes = true);
        }

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
                    new PlaybackChanger(200,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange1,
                            obj =>
                            {
                                obj.SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
                                ((Note)obj).Velocity = (SevenBitNumber)30;
                            })),
                    new PlaybackChanger(300,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange2,
                            obj =>
                            {
                                obj.SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap);
                                ((Note)obj).NoteNumber = (SevenBitNumber)80;
                            })),
                    new PlaybackChanger(300,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange2,
                            obj =>
                            {
                                obj.SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
                                ((Note)obj).Channel = (FourBitNumber)5;
                                playback.MoveToStart();
                            })),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, (SevenBitNumber)30), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)60, Note.DefaultVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)60, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)80, Note.DefaultVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)80, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, (SevenBitNumber)30), TimeSpan.FromMilliseconds(1100)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)80, Note.DefaultVelocity) { Channel = (FourBitNumber)5 }, TimeSpan.FromMilliseconds(1100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1400)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)80, Note.DefaultOffVelocity) { Channel = (FourBitNumber)5 }, TimeSpan.FromMilliseconds(1400)),
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
                    new PlaybackChanger(50,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                var chord = (Chord)obj;
                                chord.Notes.Remove(note1);
                            })),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
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
                    new PlaybackChanger(50,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                var chord = (Chord)obj;
                                chord.Notes.Remove(note2);
                                chord.Notes.Add(note3);
                            })),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
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
                    new PlaybackChanger(50,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                var chord = (Chord)obj;
                                chord.Notes
                                    .Last()
                                    .SetTime(new MetricTimeSpan(0, 0, 0, 150), TempoMap)
                                    .SetLength(new MetricTimeSpan(0, 0, 0, 200), TempoMap);
                            })),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(150)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(350)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
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
                    new PlaybackChanger(50,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                var chord = (Chord)obj;
                                chord.Notes.Clear();
                            })),
                },
                expectedReceivedEvents: new ReceivedEvent[]
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
                    new PlaybackChanger(200,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                var chord = (Chord)obj;
                                chord.Notes.Remove(note1);
                            })),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
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
                    new PlaybackChanger(200,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                var chord = (Chord)obj;
                                chord.Notes.Remove(note2);
                                chord.Notes.Add(note3);
                            })),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
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
                    new PlaybackChanger(200,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                var chord = (Chord)obj;
                                chord.Notes
                                    .Last()
                                    .SetTime(new MetricTimeSpan(0, 0, 0, 150), TempoMap)
                                    .SetLength(new MetricTimeSpan(0, 0, 0, 200), TempoMap);
                            })),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(350)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
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
                    new PlaybackChanger(200,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                var chord = (Chord)obj;
                                chord.Notes.Clear();
                            })),
                },
                expectedReceivedEvents: new ReceivedEvent[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(200)),
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
                    new PlaybackChanger(200,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap))),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(100)),
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
                    new PlaybackChanger(200,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap))),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1000)),
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
                    new PlaybackChanger(300,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);
                                ((ProgramChangeEvent)((TimedEvent)obj).Event).ProgramNumber = (SevenBitNumber)71;
                            })),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)71), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)71), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1400)),
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
                    new PlaybackChanger(300,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange1,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap))),
                    new PlaybackChanger(300,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange2,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap))),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)50), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)50), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70), TimeSpan.FromMilliseconds(900)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1400)),
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
                    new PlaybackChanger(300,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 800), TempoMap))),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)80), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new ProgramChangeEvent(), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)80), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(900)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(900)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1600)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(1600)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)80), TimeSpan.FromMilliseconds(1700)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1800)),
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
                    new PlaybackChanger(300,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange1,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap))),
                    new PlaybackChanger(300,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange2,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap))),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)70), TimeSpan.FromMilliseconds(900)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1400)),
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
                    new PlaybackChanger(300,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);
                                ((PitchBendEvent)((TimedEvent)obj).Event).PitchValue = 7100;
                            })),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new PitchBendEvent(7100), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new PitchBendEvent(7100), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1400)),
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
                    new PlaybackChanger(300,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange1,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap))),
                    new PlaybackChanger(300,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange2,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap))),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new PitchBendEvent(7000), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new PitchBendEvent(5000), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new PitchBendEvent(7000), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new PitchBendEvent(5000), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new PitchBendEvent(7000), TimeSpan.FromMilliseconds(900)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1400)),
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
                    new PlaybackChanger(300,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 800), TempoMap))),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new PitchBendEvent(8000), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new PitchBendEvent(), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new PitchBendEvent(8000), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(900)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(900)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1600)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(1600)),
                    new ReceivedEvent(new PitchBendEvent(8000), TimeSpan.FromMilliseconds(1700)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1800)),
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
                    new PlaybackChanger(300,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange1,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap))),
                    new PlaybackChanger(300,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange2,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap))),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new PitchBendEvent(7000), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new PitchBendEvent(7000) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new PitchBendEvent(7000) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new PitchBendEvent(7000), TimeSpan.FromMilliseconds(900)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1400)),
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
                    new PlaybackChanger(300,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);
                                ((ControlChangeEvent)((TimedEvent)obj).Event).ControlValue = (SevenBitNumber)71;
                            })),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)71), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)71), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1400)),
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
                    new PlaybackChanger(300,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange1,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap))),
                    new PlaybackChanger(300,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange2,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap))),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)70), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)50), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)70), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)50), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)70), TimeSpan.FromMilliseconds(900)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1400)),
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
                    new PlaybackChanger(300,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 800), TempoMap))),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)80), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)0), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)80), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(900)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(900)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1600)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(1600)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)80), TimeSpan.FromMilliseconds(1700)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1800)),
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
                    new PlaybackChanger(300,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange1,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap))),
                    new PlaybackChanger(300,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange2,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap))),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.Zero),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)70), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)70) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)70) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new ControlChangeEvent((SevenBitNumber)5, (SevenBitNumber)70), TimeSpan.FromMilliseconds(900)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1400)),
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
                    new PlaybackChanger(300,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap))),
                        },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
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
                    new PlaybackChanger(200,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                obj.SetTime(new MetricTimeSpan(0, 0, 0, 500), TempoMap);
                                ((Note)obj).Channel = (FourBitNumber)4;
                                ((Note)obj).NoteNumber = (SevenBitNumber)71;
                            })),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)71, Note.DefaultVelocity) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)71, Note.DefaultOffVelocity) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)71, Note.DefaultVelocity) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(1300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)71, Note.DefaultOffVelocity) { Channel = (FourBitNumber)4 }, TimeSpan.FromMilliseconds(1600)),
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
                    new PlaybackChanger(400,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                obj.SetTime(new MetricTimeSpan(), TempoMap);
                                ((Note)obj).Velocity = (SevenBitNumber)30;
                            })),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)30), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(700)),
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
                    new PlaybackChanger(350,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                obj.SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap);
                                ((Note)obj).Velocity = (SevenBitNumber)30;
                            })),
                },
                expectedReceivedEvents: new ReceivedEvent[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)30), TimeSpan.FromMilliseconds(450)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(650)),
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
                    new PlaybackChanger(200,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange1,
                            obj =>
                            {
                                obj.SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
                                ((Note)obj).Velocity = (SevenBitNumber)30;
                            })),
                    new PlaybackChanger(300,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange2,
                            obj =>
                            {
                                obj.SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap);
                                ((Note)obj).NoteNumber = (SevenBitNumber)80;
                            })),
                    new PlaybackChanger(300,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange2,
                            obj =>
                            {
                                obj.SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
                                ((Note)obj).Channel = (FourBitNumber)5;
                                playback.MoveToStart();
                            })),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, (SevenBitNumber)30), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)60, Note.DefaultVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)60, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(600)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)80, Note.DefaultVelocity), TimeSpan.FromMilliseconds(700)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)80, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, (SevenBitNumber)30), TimeSpan.FromMilliseconds(1100)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)80, Note.DefaultVelocity) { Channel = (FourBitNumber)5 }, TimeSpan.FromMilliseconds(1100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(1400)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)80, Note.DefaultOffVelocity) { Channel = (FourBitNumber)5 }, TimeSpan.FromMilliseconds(1400)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, (SevenBitNumber)30), TimeSpan.FromMilliseconds(1700)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)80, Note.DefaultVelocity) { Channel = (FourBitNumber)5 }, TimeSpan.FromMilliseconds(1700)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(2000)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)80, Note.DefaultOffVelocity) { Channel = (FourBitNumber)5 }, TimeSpan.FromMilliseconds(2000)),
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
                    new PlaybackChanger(50,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                var chord = (Chord)obj;
                                chord.Notes.Remove(note1);
                            })),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
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
                    new PlaybackChanger(50,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                var chord = (Chord)obj;
                                chord.Notes.Remove(note2);
                                chord.Notes.Add(note3);
                            })),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
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
                    new PlaybackChanger(50,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                var chord = (Chord)obj;
                                chord.Notes
                                    .Last()
                                    .SetTime(new MetricTimeSpan(0, 0, 0, 150), TempoMap)
                                    .SetLength(new MetricTimeSpan(0, 0, 0, 200), TempoMap);
                            })),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(150)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(350)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(550)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(750)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
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
                    new PlaybackChanger(50,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                var chord = (Chord)obj;
                                chord.Notes.Clear();
                            })),
                },
                expectedReceivedEvents: new ReceivedEvent[]
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
                    new PlaybackChanger(200,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                var chord = (Chord)obj;
                                chord.Notes.Remove(note1);
                            })),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
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
                    new PlaybackChanger(200,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                var chord = (Chord)obj;
                                chord.Notes.Remove(note2);
                                chord.Notes.Add(note3);
                            })),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
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
                    new PlaybackChanger(200,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                var chord = (Chord)obj;
                                chord.Notes
                                    .Last()
                                    .SetTime(new MetricTimeSpan(0, 0, 0, 150), TempoMap)
                                    .SetLength(new MetricTimeSpan(0, 0, 0, 200), TempoMap);
                            })),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(350)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(400)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(550)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(750)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(800)),
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
                    new PlaybackChanger(200,
                        (playback, collection) => collection.ChangeObject(
                            objectToChange,
                            obj =>
                            {
                                var chord = (Chord)obj;
                                chord.Notes.Clear();
                            })),
                },
                expectedReceivedEvents: new ReceivedEvent[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(200)),
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
                    new PlaybackChanger(100,
                        (playback, collection) => collection.ChangeObject(
                            noteOnEvent,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap))),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, SevenBitNumber.MaxValue), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, SevenBitNumber.MinValue), TimeSpan.FromMilliseconds(400)),
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
                    new PlaybackChanger(200,
                        (playback, collection) => collection.ChangeObject(
                            noteOnEvent,
                            obj =>
                            {
                                ((NoteOnEvent)((TimedEvent)obj).Event).Velocity = (SevenBitNumber)70;
                                obj.SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
                            })),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, SevenBitNumber.MaxValue), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, SevenBitNumber.MinValue), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, (SevenBitNumber)70), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, SevenBitNumber.MinValue), TimeSpan.FromMilliseconds(400)),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = true;
                    playback.NotesPlaybackStarted += (_, e) => notesStarted.AddRange(e.Notes);
                    playback.NotesPlaybackFinished += (_, e) => notesFinished.AddRange(e.Notes);
                });

            CollectionAssert.AreEquivalent(notesStarted, notesFinished, "Mismatch.");
            Assert.AreEqual(2, notesStarted.Count, "Invalid count of started notes.");
            Assert.AreEqual(2, notesFinished.Count, "Invalid count of finished notes.");

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
                    new PlaybackChanger(200,
                        (playback, collection) => collection.ChangeObject(
                            noteOnEvent,
                            obj => ((NoteOnEvent)((TimedEvent)obj).Event).Channel = (FourBitNumber)4)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, SevenBitNumber.MaxValue), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, SevenBitNumber.MinValue), TimeSpan.FromMilliseconds(200)),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = true;
                    playback.NotesPlaybackStarted += (_, e) => notesStarted.AddRange(e.Notes);
                    playback.NotesPlaybackFinished += (_, e) => notesFinished.AddRange(e.Notes);
                });

            CollectionAssert.AreEquivalent(notesStarted, notesFinished, "Mismatch.");
            Assert.AreEqual(1, notesStarted.Count, "Invalid count of started notes.");
            Assert.AreEqual(1, notesFinished.Count, "Invalid count of finished notes.");

            MidiAsserts.AreEqual(
                new[]
                {
                    new Note((SevenBitNumber)50) { Velocity = SevenBitNumber.MaxValue }
                        .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap)
                        .SetLength(new MetricTimeSpan(0, 0, 0, 300 /* TODO: 100 */), TempoMap),
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
                    new PlaybackChanger(50,
                        (playback, collection) => collection.ChangeObject(
                            noteOffEvent,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap))),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, SevenBitNumber.MaxValue), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, SevenBitNumber.MinValue), TimeSpan.FromMilliseconds(300)),
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
                    new PlaybackChanger(200,
                        (playback, collection) => collection.ChangeObject(
                            noteOffEvent,
                            obj => obj.SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap))),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, SevenBitNumber.MaxValue), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, SevenBitNumber.MinValue), TimeSpan.FromMilliseconds(300)),
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
                    new PlaybackChanger(200,
                        (playback, collection) => collection.ChangeObject(
                            noteOffEvent,
                            obj =>
                            {
                                ((NoteOffEvent)((TimedEvent)obj).Event).Velocity = (SevenBitNumber)70;
                                obj.SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap);
                            })),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, SevenBitNumber.MaxValue), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, (SevenBitNumber)70), TimeSpan.FromMilliseconds(300)),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = true;
                    playback.NotesPlaybackStarted += (_, e) => notesStarted.AddRange(e.Notes);
                    playback.NotesPlaybackFinished += (_, e) => notesFinished.AddRange(e.Notes);
                });

            Assert.AreEqual(1, notesStarted.Count, "Invalid count of started notes.");
            Assert.AreEqual(1, notesFinished.Count, "Invalid count of finished notes.");

            MidiAsserts.AreEqual(
                new[]
                {
                    new Note((SevenBitNumber)50) { Velocity = SevenBitNumber.MaxValue }
                        .SetTime(new MetricTimeSpan(0, 0, 0, 100 /* TODO: 100 */), TempoMap)
                        .SetLength(new MetricTimeSpan(0, 0, 0, 300), TempoMap),
                },
                notesStarted,
                "Invalid notes started.");

            MidiAsserts.AreEqual(
                new[]
                {
                    new Note((SevenBitNumber)50) { Velocity = SevenBitNumber.MaxValue, OffVelocity = (SevenBitNumber)70 }
                        .SetTime(new MetricTimeSpan(0, 0, 0, 100 /* TODO: 100 */), TempoMap)
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
                    new PlaybackChanger(200,
                        (playback, collection) => collection.ChangeObject(
                            noteOffEvent,
                            obj => ((NoteOffEvent)((TimedEvent)obj).Event).Channel = (FourBitNumber)4)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)50, SevenBitNumber.MaxValue), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)50, SevenBitNumber.MinValue), TimeSpan.FromMilliseconds(200)),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = true;
                    playback.NotesPlaybackStarted += (_, e) => notesStarted.AddRange(e.Notes);
                    playback.NotesPlaybackFinished += (_, e) => notesFinished.AddRange(e.Notes);
                });

            Assert.AreEqual(1, notesStarted.Count, "Invalid count of started notes.");
            Assert.AreEqual(1, notesFinished.Count, "Invalid count of finished notes.");

            MidiAsserts.AreEqual(
                new[]
                {
                    new Note((SevenBitNumber)50) { Velocity = SevenBitNumber.MaxValue }
                        .SetTime(new MetricTimeSpan(0, 0, 0, 100), TempoMap)
                        .SetLength(new MetricTimeSpan(0, 0, 0, 300 /* TODO: 100 */), TempoMap),
                },
                notesStarted,
                "Invalid notes.");
        }

        #endregion
    }
}
