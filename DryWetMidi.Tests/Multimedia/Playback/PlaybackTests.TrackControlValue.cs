using System;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed partial class PlaybackTests
    {
        #region Test methods

        [Retry(RetriesNumber)]
        [Test]
        public void TrackControlValue_NoControlChanges_MoveToTime(
            [Values(0, 100)] int moveFromMs,
            [Values(0, 500)] int moveToMs)
        {
            var lastEventTime = TimeSpan.FromSeconds(1);

            var moveFrom = TimeSpan.FromMilliseconds(moveFromMs);
            var moveTo = TimeSpan.FromMilliseconds(moveToMs);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteAftertouchEvent(), lastEventTime - (moveTo - moveFrom)),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackControlValue_ControlChangeAtZero_MoveToTime()
        {
            var lastEventTime = TimeSpan.FromSeconds(1);
            var controlNumber = (SevenBitNumber)100;
            var controlValue = (SevenBitNumber)70;

            var moveFrom = TimeSpan.FromMilliseconds(100);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ControlChangeEvent(controlNumber, controlValue))
                        .SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new ControlChangeEvent(controlNumber, controlValue), TimeSpan.Zero),
                    new SentReceivedEvent(new NoteAftertouchEvent(), lastEventTime - (moveTo - moveFrom)),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackControlValue_ControlChangeAtZero_MoveToStart()
        {
            var lastEventTime = TimeSpan.FromSeconds(1);
            var controlNumber1 = (SevenBitNumber)100;
            var controlValue1 = (SevenBitNumber)70;

            var controlChangeDelay = TimeSpan.FromMilliseconds(800);
            var controlNumber2 = (SevenBitNumber)10;
            var controlValue2 = (SevenBitNumber)80;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.Zero;

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ControlChangeEvent(controlNumber1, controlValue1))
                        .SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new ControlChangeEvent(controlNumber2, controlValue2) { Channel = (FourBitNumber)10 })
                        .SetTime((MetricTimeSpan)controlChangeDelay, TempoMap),
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new ControlChangeEvent(controlNumber1, controlValue1), TimeSpan.Zero),
                    new SentReceivedEvent(new ControlChangeEvent(controlNumber1, controlValue1), moveFrom),
                    new SentReceivedEvent(new ControlChangeEvent(controlNumber2, controlValue2) { Channel = (FourBitNumber)10 }, moveFrom + controlChangeDelay),
                    new SentReceivedEvent(new NoteAftertouchEvent(), moveFrom + lastEventTime),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackControlValue_ControlChangesAtZero_MoveToStart()
        {
            var lastEventTime = TimeSpan.FromSeconds(1);
            var controlNumber1 = (SevenBitNumber)100;
            var controlValue1 = (SevenBitNumber)70;

            var controlNumber2 = (SevenBitNumber)10;
            var controlValue2 = (SevenBitNumber)80;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.Zero;

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ControlChangeEvent(controlNumber1, controlValue1))
                        .SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new ControlChangeEvent(controlNumber2, controlValue2))
                        .SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new ControlChangeEvent(controlNumber1, controlValue1), TimeSpan.Zero),
                    new SentReceivedEvent(new ControlChangeEvent(controlNumber2, controlValue2), TimeSpan.Zero),
                    new SentReceivedEvent(new ControlChangeEvent(controlNumber1, controlValue1), moveFrom),
                    new SentReceivedEvent(new ControlChangeEvent(controlNumber2, controlValue2), moveFrom),
                    new SentReceivedEvent(new NoteAftertouchEvent(), lastEventTime + moveFrom),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackControlValue_FromBeforeControlChange_ToBeforeControlChange()
        {
            var controlChangeTime = TimeSpan.FromMilliseconds(800);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var controlNumber = (SevenBitNumber)100;
            var controlValue = (SevenBitNumber)70;

            var moveFrom = TimeSpan.FromMilliseconds(100);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)controlChangeTime, TempoMap),
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, controlChangeTime - (moveTo - moveFrom)),
                    new SentReceivedEvent(new NoteAftertouchEvent(), lastEventTime - (moveTo - moveFrom)),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackControlValue_FromBeforeControlChange_ToAfterControlChange()
        {
            var controlChangeTime = TimeSpan.FromMilliseconds(500);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var controlNumber = (SevenBitNumber)100;
            var controlValue = (SevenBitNumber)10;

            var moveFrom = TimeSpan.FromMilliseconds(300);
            var moveTo = TimeSpan.FromMilliseconds(800);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)controlChangeTime, TempoMap),
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, moveFrom),
                    new SentReceivedEvent(new NoteAftertouchEvent(), lastEventTime - (moveTo - moveFrom)),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackControlValue_Default_FromBeforeControlChange_ToAfterControlChange()
        {
            var controlChangeTime = TimeSpan.FromMilliseconds(500);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var controlNumber = (SevenBitNumber)100;
            var controlValue = (SevenBitNumber)0;

            var moveFrom = TimeSpan.FromMilliseconds(300);
            var moveTo = TimeSpan.FromMilliseconds(700);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)controlChangeTime, TempoMap),
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, moveFrom),
                    new SentReceivedEvent(new NoteAftertouchEvent(), lastEventTime - (moveTo - moveFrom)),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackControlValue_FromAfterControlChange_ToAfterControlChange()
        {
            var controlChangeTime = TimeSpan.FromMilliseconds(500);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var controlNumber = (SevenBitNumber)0;
            var controlValue = (SevenBitNumber)90;

            var moveFrom = TimeSpan.FromMilliseconds(700);
            var moveTo = TimeSpan.FromMilliseconds(900);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)controlChangeTime, TempoMap),
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, controlChangeTime),
                    new SentReceivedEvent(new NoteAftertouchEvent(), lastEventTime - (moveTo - moveFrom)),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackControlValue_FromAfterControlChange_ToBeforeControlChange()
        {
            var controlChangeTime = TimeSpan.FromMilliseconds(800);
            var lastEventTime = TimeSpan.FromSeconds(2);
            var controlNumber = (SevenBitNumber)100;
            var controlValue = (SevenBitNumber)50;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)controlChangeTime, TempoMap),
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, controlChangeTime),
                    new SentReceivedEvent(new ControlChangeEvent(controlNumber, SevenBitNumber.MinValue) { Channel = (FourBitNumber)4 }, moveFrom),
                    new SentReceivedEvent(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, controlChangeTime - moveTo + moveFrom),
                    new SentReceivedEvent(new NoteAftertouchEvent(), lastEventTime  - moveTo + moveFrom),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackControlValue_Default_FromBeforeControlChange_ToControlChange()
        {
            var controlChangeTime = TimeSpan.FromMilliseconds(800);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var controlNumber = (SevenBitNumber)100;
            var controlValue = (SevenBitNumber)0;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(800);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)controlChangeTime, TempoMap),
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, moveFrom),
                    new SentReceivedEvent(new NoteAftertouchEvent(), lastEventTime - (moveTo - moveFrom)),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackControlValue_FromAfterControlChange_ToControlChange()
        {
            var controlChangeTime = TimeSpan.FromMilliseconds(800);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var controlNumber = (SevenBitNumber)100;
            var controlValue = (SevenBitNumber)50;

            var moveFrom = TimeSpan.FromMilliseconds(900);
            var moveTo = TimeSpan.FromMilliseconds(800);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)controlChangeTime, TempoMap),
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, controlChangeTime),
                    new SentReceivedEvent(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, moveFrom),
                    new SentReceivedEvent(new NoteAftertouchEvent(), lastEventTime - controlChangeTime + moveFrom),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackControlValue_EnableInMiddle_FromBeforeControlChange_ToAfterControlChange()
        {
            var controlChangeTime = TimeSpan.FromMilliseconds(800);
            var programChangeTime = TimeSpan.FromSeconds(1);
            var lastEventTime = TimeSpan.FromSeconds(2);
            var controlNumber = (SevenBitNumber)100;
            var controlValue = (SevenBitNumber)10;
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(1200);
            var enableAfter = TimeSpan.FromMilliseconds(500);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)controlChangeTime, TempoMap),
                    new TimedEvent(new ProgramChangeEvent(programNumber))
                        .SetTime((MetricTimeSpan)programChangeTime, TempoMap),
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                    new PlaybackAction(enableAfter,
                        p => p.TrackControlValue = true),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, moveFrom + enableAfter),
                    new SentReceivedEvent(new NoteAftertouchEvent(), lastEventTime + moveFrom - moveTo),
                },
                setupPlayback: playback =>
                {
                    playback.TrackControlValue = false;
                    playback.TrackProgram = false;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackControlValue_EnableInMiddle_FromAfterControlChange_ToBeforeControlChange()
        {
            var controlChangeTime = TimeSpan.FromMilliseconds(800);
            var lastEventTime = TimeSpan.FromSeconds(2);
            var controlNumber = (SevenBitNumber)100;
            var controlValue = (SevenBitNumber)50;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(500);
            var enableAfter = TimeSpan.FromMilliseconds(150);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)controlChangeTime, TempoMap),
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                    new PlaybackAction(enableAfter,
                        p => p.TrackControlValue = true),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, controlChangeTime),
                    new SentReceivedEvent(new ControlChangeEvent(controlNumber, SevenBitNumber.MinValue) { Channel = (FourBitNumber)4 }, moveFrom + enableAfter),
                    new SentReceivedEvent(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, controlChangeTime - moveTo + moveFrom),
                    new SentReceivedEvent(new NoteAftertouchEvent(), lastEventTime - moveTo + moveFrom),
                },
                setupPlayback: playback => playback.TrackControlValue = false);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void DontTrackControlValue_NoControlChanges_MoveToTime(
            [Values(0, 100)] int moveFromMs,
            [Values(0, 500)] int moveToMs)
        {
            var lastEventTime = TimeSpan.FromSeconds(1);

            var moveFrom = TimeSpan.FromMilliseconds(moveFromMs);
            var moveTo = TimeSpan.FromMilliseconds(moveToMs);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteAftertouchEvent(), lastEventTime - (moveTo - moveFrom)),
                },
                setupPlayback: playback => playback.TrackControlValue = false);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void DontTrackControlValue_ControlChangeAtZero_MoveToTime()
        {
            var lastEventTime = TimeSpan.FromSeconds(1);
            var controlNumber = (SevenBitNumber)100;
            var controlValue = (SevenBitNumber)70;

            var moveFrom = TimeSpan.FromMilliseconds(100);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ControlChangeEvent(controlNumber, controlValue))
                        .SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new ControlChangeEvent(controlNumber, controlValue), TimeSpan.Zero),
                    new SentReceivedEvent(new NoteAftertouchEvent(), lastEventTime - (moveTo - moveFrom)),
                },
                setupPlayback: playback => playback.TrackControlValue = false);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void DontTrackControlValue_ControlChangeAtZero_MoveToStart()
        {
            var lastEventTime = TimeSpan.FromSeconds(1);
            var controlNumber1 = (SevenBitNumber)100;
            var controlValue1 = (SevenBitNumber)70;

            var controlChangeDelay = TimeSpan.FromMilliseconds(800);
            var controlNumber2 = (SevenBitNumber)10;
            var controlValue2 = (SevenBitNumber)80;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.Zero;

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ControlChangeEvent(controlNumber1, controlValue1))
                        .SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new ControlChangeEvent(controlNumber2, controlValue2) { Channel = (FourBitNumber)10 })
                        .SetTime((MetricTimeSpan)controlChangeDelay, TempoMap),
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new ControlChangeEvent(controlNumber1, controlValue1), TimeSpan.Zero),
                    new SentReceivedEvent(new ControlChangeEvent(controlNumber1, controlValue1), moveFrom),
                    new SentReceivedEvent(new ControlChangeEvent(controlNumber2, controlValue2) { Channel = (FourBitNumber)10 }, moveFrom + controlChangeDelay),
                    new SentReceivedEvent(new NoteAftertouchEvent(), moveFrom + lastEventTime),
                },
                setupPlayback: playback => playback.TrackControlValue = false);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void DontTrackControlValue_ControlChangesAtZero_MoveToStart()
        {
            var lastEventTime = TimeSpan.FromSeconds(1);
            var controlNumber1 = (SevenBitNumber)100;
            var controlValue1 = (SevenBitNumber)70;

            var controlNumber2 = (SevenBitNumber)10;
            var controlValue2 = (SevenBitNumber)80;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.Zero;

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ControlChangeEvent(controlNumber1, controlValue1))
                        .SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new ControlChangeEvent(controlNumber2, controlValue2))
                        .SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new ControlChangeEvent(controlNumber1, controlValue1), TimeSpan.Zero),
                    new SentReceivedEvent(new ControlChangeEvent(controlNumber2, controlValue2), TimeSpan.Zero),
                    new SentReceivedEvent(new ControlChangeEvent(controlNumber1, controlValue1), moveFrom),
                    new SentReceivedEvent(new ControlChangeEvent(controlNumber2, controlValue2), moveFrom),
                    new SentReceivedEvent(new NoteAftertouchEvent(), lastEventTime + moveFrom),
                },
                setupPlayback: playback => playback.TrackControlValue = false);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void DontTrackControlValue_FromBeforeControlChange_ToBeforeControlChange()
        {
            var controlChangeTime = TimeSpan.FromMilliseconds(800);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var controlNumber = (SevenBitNumber)100;
            var controlValue = (SevenBitNumber)70;

            var moveFrom = TimeSpan.FromMilliseconds(100);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)controlChangeTime, TempoMap),
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, controlChangeTime - (moveTo - moveFrom)),
                    new SentReceivedEvent(new NoteAftertouchEvent(), lastEventTime - (moveTo - moveFrom)),
                },
                setupPlayback: playback => playback.TrackControlValue = false);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void DontTrackControlValue_FromBeforeControlChange_ToAfterControlChange()
        {
            var controlChangeTime = TimeSpan.FromMilliseconds(500);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var controlNumber = (SevenBitNumber)100;
            var controlValue = (SevenBitNumber)10;

            var moveFrom = TimeSpan.FromMilliseconds(300);
            var moveTo = TimeSpan.FromMilliseconds(800);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)controlChangeTime, TempoMap),
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteAftertouchEvent(), lastEventTime - (moveTo - moveFrom)),
                },
                setupPlayback: playback => playback.TrackControlValue = false);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void DontTrackControlValue_Default_FromBeforeControlChange_ToAfterControlChange()
        {
            var controlChangeTime = TimeSpan.FromMilliseconds(500);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var controlNumber = (SevenBitNumber)100;
            var controlValue = (SevenBitNumber)0;

            var moveFrom = TimeSpan.FromMilliseconds(300);
            var moveTo = TimeSpan.FromMilliseconds(700);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)controlChangeTime, TempoMap),
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteAftertouchEvent(), lastEventTime - (moveTo - moveFrom)),
                },
                setupPlayback: playback => playback.TrackControlValue = false);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void DontTrackControlValue_FromAfterControlChange_ToAfterControlChange()
        {
            var controlChangeTime = TimeSpan.FromMilliseconds(500);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var controlNumber = (SevenBitNumber)0;
            var controlValue = (SevenBitNumber)90;

            var moveFrom = TimeSpan.FromMilliseconds(700);
            var moveTo = TimeSpan.FromMilliseconds(900);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)controlChangeTime, TempoMap),
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, controlChangeTime),
                    new SentReceivedEvent(new NoteAftertouchEvent(), lastEventTime - (moveTo - moveFrom)),
                },
                setupPlayback: playback => playback.TrackControlValue = false);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void DontTrackControlValue_FromAfterControlChange_ToBeforeControlChange()
        {
            var controlChangeTime = TimeSpan.FromMilliseconds(800);
            var lastEventTime = TimeSpan.FromSeconds(2);
            var controlNumber = (SevenBitNumber)100;
            var controlValue = (SevenBitNumber)50;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)controlChangeTime, TempoMap),
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, controlChangeTime),
                    new SentReceivedEvent(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, controlChangeTime - moveTo + moveFrom),
                    new SentReceivedEvent(new NoteAftertouchEvent(), lastEventTime  - moveTo + moveFrom),
                },
                setupPlayback: playback => playback.TrackControlValue = false);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void DontTrackControlValue_Default_FromBeforeControlChange_ToControlChange()
        {
            var controlChangeTime = TimeSpan.FromMilliseconds(800);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var controlNumber = (SevenBitNumber)100;
            var controlValue = (SevenBitNumber)0;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(800);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)controlChangeTime, TempoMap),
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, moveFrom),
                    new SentReceivedEvent(new NoteAftertouchEvent(), lastEventTime - (moveTo - moveFrom)),
                },
                setupPlayback: playback => playback.TrackControlValue = false);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void DontTrackControlValue_FromAfterControlChange_ToControlChange()
        {
            var controlChangeTime = TimeSpan.FromMilliseconds(800);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var controlNumber = (SevenBitNumber)100;
            var controlValue = (SevenBitNumber)50;

            var moveFrom = TimeSpan.FromMilliseconds(900);
            var moveTo = TimeSpan.FromMilliseconds(800);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)controlChangeTime, TempoMap),
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, controlChangeTime),
                    new SentReceivedEvent(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, moveFrom),
                    new SentReceivedEvent(new NoteAftertouchEvent(), lastEventTime - controlChangeTime + moveFrom),
                },
                setupPlayback: playback => playback.TrackControlValue = false);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackControlValue_DisableInMiddle_FromBeforeControlChange_ToAfterControlChange()
        {
            var controlChangeTime = TimeSpan.FromMilliseconds(800);
            var programChangeTime = TimeSpan.FromSeconds(1);
            var lastEventTime = TimeSpan.FromSeconds(2);
            var controlNumber = (SevenBitNumber)100;
            var controlValue = (SevenBitNumber)10;
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(1200);
            var disableAfter = TimeSpan.FromMilliseconds(500);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)controlChangeTime, TempoMap),
                    new TimedEvent(new ProgramChangeEvent(programNumber))
                        .SetTime((MetricTimeSpan)programChangeTime, TempoMap),
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                    new PlaybackAction(disableAfter,
                        p => p.TrackControlValue = false),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, moveFrom),
                    new SentReceivedEvent(new NoteAftertouchEvent(), lastEventTime + moveFrom - moveTo),
                },
                setupPlayback: playback => playback.TrackProgram = false);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackControlValue_DisableInMiddle_FromAfterControlChange_ToBeforeControlChange()
        {
            var controlChangeTime = TimeSpan.FromMilliseconds(800);
            var lastEventTime = TimeSpan.FromSeconds(2);
            var controlNumber = (SevenBitNumber)100;
            var controlValue = (SevenBitNumber)50;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(500);
            var disableAfter = TimeSpan.FromMilliseconds(150);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)controlChangeTime, TempoMap),
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                    new PlaybackAction(disableAfter,
                        p => p.TrackControlValue = false),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, controlChangeTime),
                    new SentReceivedEvent(new ControlChangeEvent(controlNumber, SevenBitNumber.MinValue) { Channel = (FourBitNumber)4 }, moveFrom),
                    new SentReceivedEvent(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, controlChangeTime - moveTo + moveFrom),
                    new SentReceivedEvent(new NoteAftertouchEvent(), lastEventTime - moveTo + moveFrom),
                });
        }

        #endregion
    }
}
