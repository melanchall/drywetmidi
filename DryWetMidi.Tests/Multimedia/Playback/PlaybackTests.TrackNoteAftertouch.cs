using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Attributes;
using NUnit.Framework;
using System;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed partial class PlaybackTests
    {
        #region Test methods

        [TimingCritical]
        [Test]
        public void TrackNoteAftertouch_NoNoteAftertouchs_MoveToTime(
            [Values(0, 100)] int moveFromMs,
            [Values(0, 500)] int moveToMs)
        {
            var lastEventTime = TimeSpan.FromSeconds(1);

            var moveFrom = TimeSpan.FromMilliseconds(moveFromMs);
            var moveTo = TimeSpan.FromMilliseconds(moveToMs);

            CheckPlayback(
                useOutputEndpoint: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new TextEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new TextEvent(), lastEventTime - (moveTo - moveFrom)),
                });
        }

        [TimingCritical]
        [Test]
        public void TrackNoteAftertouch_NoteAftertouchAtZero_MoveToTime()
        {
            var lastEventTime = TimeSpan.FromSeconds(1);
            var noteNumber = (SevenBitNumber)100;
            var aftertouchValue = (SevenBitNumber)70;

            var moveFrom = TimeSpan.FromMilliseconds(100);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckPlayback(
                useOutputEndpoint: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteAftertouchEvent(noteNumber, aftertouchValue))
                        .SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new TextEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new NoteAftertouchEvent(noteNumber, aftertouchValue), TimeSpan.Zero),
                    new TimestampedEvent(new TextEvent(), lastEventTime - (moveTo - moveFrom)),
                });
        }

        [TimingCritical]
        [Test]
        public void TrackNoteAftertouch_NoteAftertouchAtZero_MoveToStart()
        {
            var lastEventTime = TimeSpan.FromSeconds(1);
            var noteNumber1 = (SevenBitNumber)100;
            var aftertouchValue1 = (SevenBitNumber)70;

            var noteAftertouchChangeDelay = TimeSpan.FromMilliseconds(800);
            var noteNumber2 = (SevenBitNumber)10;
            var aftertouchValue2 = (SevenBitNumber)80;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.Zero;

            CheckPlayback(
                useOutputEndpoint: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteAftertouchEvent(noteNumber1, aftertouchValue1))
                        .SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteAftertouchEvent(noteNumber2, aftertouchValue2) { Channel = (FourBitNumber)10 })
                        .SetTime((MetricTimeSpan)noteAftertouchChangeDelay, TempoMap),
                    new TimedEvent(new TextEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new NoteAftertouchEvent(noteNumber1, aftertouchValue1), TimeSpan.Zero),
                    new TimestampedEvent(new NoteAftertouchEvent(noteNumber1, aftertouchValue1), moveFrom),
                    new TimestampedEvent(new NoteAftertouchEvent(noteNumber2, aftertouchValue2) { Channel = (FourBitNumber)10 }, moveFrom + noteAftertouchChangeDelay),
                    new TimestampedEvent(new TextEvent(), moveFrom + lastEventTime),
                });
        }

        [TimingCritical]
        [Test]
        public void TrackNoteAftertouch_NoteAftertouchsAtZero_MoveToStart()
        {
            var lastEventTime = TimeSpan.FromSeconds(1);
            var noteNumber1 = (SevenBitNumber)100;
            var aftertouchValue1 = (SevenBitNumber)70;

            var noteNumber2 = (SevenBitNumber)10;
            var aftertouchValue2 = (SevenBitNumber)80;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.Zero;

            CheckPlayback(
                useOutputEndpoint: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteAftertouchEvent(noteNumber1, aftertouchValue1))
                        .SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteAftertouchEvent(noteNumber2, aftertouchValue2))
                        .SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new TextEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new NoteAftertouchEvent(noteNumber1, aftertouchValue1), TimeSpan.Zero),
                    new TimestampedEvent(new NoteAftertouchEvent(noteNumber2, aftertouchValue2), TimeSpan.Zero),
                    new TimestampedEvent(new NoteAftertouchEvent(noteNumber1, aftertouchValue1), moveFrom),
                    new TimestampedEvent(new NoteAftertouchEvent(noteNumber2, aftertouchValue2), moveFrom),
                    new TimestampedEvent(new TextEvent(), lastEventTime + moveFrom),
                });
        }

        [TimingCritical]
        [Test]
        public void TrackNoteAftertouch_FromBeforeNoteAftertouch_ToBeforeNoteAftertouch()
        {
            var noteAftertouchChangeTime = TimeSpan.FromMilliseconds(800);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var noteNumber = (SevenBitNumber)100;
            var aftertouchValue = (SevenBitNumber)70;

            var moveFrom = TimeSpan.FromMilliseconds(100);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckPlayback(
                useOutputEndpoint: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteAftertouchEvent(noteNumber, aftertouchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)noteAftertouchChangeTime, TempoMap),
                    new TimedEvent(new TextEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new NoteAftertouchEvent(noteNumber, aftertouchValue) { Channel = (FourBitNumber)4 }, noteAftertouchChangeTime - (moveTo - moveFrom)),
                    new TimestampedEvent(new TextEvent(), lastEventTime - (moveTo - moveFrom)),
                });
        }

        [TimingCritical]
        [Test]
        public void TrackNoteAftertouch_FromBeforeNoteAftertouch_ToAfterNoteAftertouch()
        {
            var noteAftertouchChangeTime = TimeSpan.FromMilliseconds(500);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var noteNumber = (SevenBitNumber)100;
            var aftertouchValue = (SevenBitNumber)10;

            var moveFrom = TimeSpan.FromMilliseconds(300);
            var moveTo = TimeSpan.FromMilliseconds(800);

            CheckPlayback(
                useOutputEndpoint: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteAftertouchEvent(noteNumber, aftertouchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)noteAftertouchChangeTime, TempoMap),
                    new TimedEvent(new TextEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new NoteAftertouchEvent(noteNumber, aftertouchValue) { Channel = (FourBitNumber)4 }, moveFrom),
                    new TimestampedEvent(new TextEvent(), lastEventTime - (moveTo - moveFrom)),
                });
        }

        [TimingCritical]
        [Test]
        public void TrackNoteAftertouch_Default_FromBeforeNoteAftertouch_ToAfterNoteAftertouch()
        {
            var noteAftertouchChangeTime = TimeSpan.FromMilliseconds(500);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var noteNumber = (SevenBitNumber)100;
            var aftertouchValue = (SevenBitNumber)0;

            var moveFrom = TimeSpan.FromMilliseconds(300);
            var moveTo = TimeSpan.FromMilliseconds(700);

            CheckPlayback(
                useOutputEndpoint: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteAftertouchEvent(noteNumber, aftertouchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)noteAftertouchChangeTime, TempoMap),
                    new TimedEvent(new TextEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new NoteAftertouchEvent(noteNumber, aftertouchValue) { Channel = (FourBitNumber)4 }, moveFrom),
                    new TimestampedEvent(new TextEvent(), lastEventTime - (moveTo - moveFrom)),
                });
        }

        [TimingCritical]
        [Test]
        public void TrackNoteAftertouch_FromAfterNoteAftertouch_ToAfterNoteAftertouch()
        {
            var noteAftertouchChangeTime = TimeSpan.FromMilliseconds(500);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var noteNumber = (SevenBitNumber)0;
            var aftertouchValue = (SevenBitNumber)90;

            var moveFrom = TimeSpan.FromMilliseconds(700);
            var moveTo = TimeSpan.FromMilliseconds(900);

            CheckPlayback(
                useOutputEndpoint: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteAftertouchEvent(noteNumber, aftertouchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)noteAftertouchChangeTime, TempoMap),
                    new TimedEvent(new TextEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new NoteAftertouchEvent(noteNumber, aftertouchValue) { Channel = (FourBitNumber)4 }, noteAftertouchChangeTime),
                    new TimestampedEvent(new TextEvent(), lastEventTime - (moveTo - moveFrom)),
                });
        }

        [TimingCritical]
        [Test]
        public void TrackNoteAftertouch_FromAfterNoteAftertouch_ToBeforeNoteAftertouch()
        {
            var noteAftertouchChangeTime = TimeSpan.FromMilliseconds(800);
            var lastEventTime = TimeSpan.FromSeconds(2);
            var noteNumber = (SevenBitNumber)100;
            var aftertouchValue = (SevenBitNumber)50;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckPlayback(
                useOutputEndpoint: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteAftertouchEvent(noteNumber, aftertouchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)noteAftertouchChangeTime, TempoMap),
                    new TimedEvent(new TextEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new NoteAftertouchEvent(noteNumber, aftertouchValue) { Channel = (FourBitNumber)4 }, noteAftertouchChangeTime),
                    new TimestampedEvent(new NoteAftertouchEvent(noteNumber, SevenBitNumber.MinValue) { Channel = (FourBitNumber)4 }, moveFrom),
                    new TimestampedEvent(new NoteAftertouchEvent(noteNumber, aftertouchValue) { Channel = (FourBitNumber)4 }, noteAftertouchChangeTime - moveTo + moveFrom),
                    new TimestampedEvent(new TextEvent(), lastEventTime  - moveTo + moveFrom),
                });
        }

        [TimingCritical]
        [Test]
        public void TrackNoteAftertouch_Default_FromBeforeNoteAftertouch_ToNoteAftertouch()
        {
            var noteAftertouchChangeTime = TimeSpan.FromMilliseconds(800);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var noteNumber = (SevenBitNumber)100;
            var aftertouchValue = (SevenBitNumber)0;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(800);

            CheckPlayback(
                useOutputEndpoint: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteAftertouchEvent(noteNumber, aftertouchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)noteAftertouchChangeTime, TempoMap),
                    new TimedEvent(new TextEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new NoteAftertouchEvent(noteNumber, aftertouchValue) { Channel = (FourBitNumber)4 }, moveFrom),
                    new TimestampedEvent(new TextEvent(), lastEventTime - (moveTo - moveFrom)),
                });
        }

        [TimingCritical]
        [Test]
        public void TrackNoteAftertouch_FromAfterNoteAftertouch_ToNoteAftertouch()
        {
            var noteAftertouchChangeTime = TimeSpan.FromMilliseconds(800);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var noteNumber = (SevenBitNumber)100;
            var aftertouchValue = (SevenBitNumber)50;

            var moveFrom = TimeSpan.FromMilliseconds(900);
            var moveTo = TimeSpan.FromMilliseconds(800);

            CheckPlayback(
                useOutputEndpoint: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteAftertouchEvent(noteNumber, aftertouchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)noteAftertouchChangeTime, TempoMap),
                    new TimedEvent(new TextEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new NoteAftertouchEvent(noteNumber, aftertouchValue) { Channel = (FourBitNumber)4 }, noteAftertouchChangeTime),
                    new TimestampedEvent(new NoteAftertouchEvent(noteNumber, aftertouchValue) { Channel = (FourBitNumber)4 }, moveFrom),
                    new TimestampedEvent(new TextEvent(), lastEventTime - noteAftertouchChangeTime + moveFrom),
                });
        }

        [TimingCritical]
        [Test]
        public void TrackNoteAftertouch_EnableInMiddle_FromBeforeNoteAftertouch_ToAfterNoteAftertouch()
        {
            var noteAftertouchChangeTime = TimeSpan.FromMilliseconds(800);
            var programChangeTime = TimeSpan.FromSeconds(1);
            var lastEventTime = TimeSpan.FromSeconds(2);
            var noteNumber = (SevenBitNumber)100;
            var aftertouchValue = (SevenBitNumber)10;
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(1200);
            var enableAfter = TimeSpan.FromMilliseconds(500);

            CheckPlayback(
                useOutputEndpoint: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteAftertouchEvent(noteNumber, aftertouchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)noteAftertouchChangeTime, TempoMap),
                    new TimedEvent(new ProgramChangeEvent(programNumber))
                        .SetTime((MetricTimeSpan)programChangeTime, TempoMap),
                    new TimedEvent(new TextEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                    new PlaybackAction(enableAfter,
                        p => p.TrackNoteAftertouch = true),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new NoteAftertouchEvent(noteNumber, aftertouchValue) { Channel = (FourBitNumber)4 }, moveFrom + enableAfter),
                    new TimestampedEvent(new TextEvent(), lastEventTime + moveFrom - moveTo),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNoteAftertouch = false;
                    playback.TrackProgram = false;
                });
        }

        [TimingCritical]
        [Test]
        public void TrackNoteAftertouch_EnableInMiddle_FromAfterNoteAftertouch_ToBeforeNoteAftertouch()
        {
            var noteAftertouchChangeTime = TimeSpan.FromMilliseconds(800);
            var lastEventTime = TimeSpan.FromSeconds(2);
            var noteNumber = (SevenBitNumber)100;
            var aftertouchValue = (SevenBitNumber)50;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(500);
            var enableAfter = TimeSpan.FromMilliseconds(150);

            CheckPlayback(
                useOutputEndpoint: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteAftertouchEvent(noteNumber, aftertouchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)noteAftertouchChangeTime, TempoMap),
                    new TimedEvent(new TextEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                    new PlaybackAction(enableAfter,
                        p => p.TrackNoteAftertouch = true),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new NoteAftertouchEvent(noteNumber, aftertouchValue) { Channel = (FourBitNumber)4 }, noteAftertouchChangeTime),
                    new TimestampedEvent(new NoteAftertouchEvent(noteNumber, SevenBitNumber.MinValue) { Channel = (FourBitNumber)4 }, moveFrom + enableAfter),
                    new TimestampedEvent(new NoteAftertouchEvent(noteNumber, aftertouchValue) { Channel = (FourBitNumber)4 }, noteAftertouchChangeTime - moveTo + moveFrom),
                    new TimestampedEvent(new TextEvent(), lastEventTime - moveTo + moveFrom),
                },
                setupPlayback: playback => playback.TrackNoteAftertouch = false);
        }

        [TimingCritical]
        [Test]
        public void DontTrackNoteAftertouch_NoNoteAftertouchs_MoveToTime(
            [Values(0, 100)] int moveFromMs,
            [Values(0, 500)] int moveToMs)
        {
            var lastEventTime = TimeSpan.FromSeconds(1);

            var moveFrom = TimeSpan.FromMilliseconds(moveFromMs);
            var moveTo = TimeSpan.FromMilliseconds(moveToMs);

            CheckPlayback(
                useOutputEndpoint: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new TextEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new TextEvent(), lastEventTime - (moveTo - moveFrom)),
                },
                setupPlayback: playback => playback.TrackNoteAftertouch = false);
        }

        [TimingCritical]
        [Test]
        public void DontTrackNoteAftertouch_NoteAftertouchAtZero_MoveToTime()
        {
            var lastEventTime = TimeSpan.FromSeconds(1);
            var noteNumber = (SevenBitNumber)100;
            var aftertouchValue = (SevenBitNumber)70;

            var moveFrom = TimeSpan.FromMilliseconds(100);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckPlayback(
                useOutputEndpoint: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteAftertouchEvent(noteNumber, aftertouchValue))
                        .SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new TextEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new NoteAftertouchEvent(noteNumber, aftertouchValue), TimeSpan.Zero),
                    new TimestampedEvent(new TextEvent(), lastEventTime - (moveTo - moveFrom)),
                },
                setupPlayback: playback => playback.TrackNoteAftertouch = false);
        }

        [TimingCritical]
        [Test]
        public void DontTrackNoteAftertouch_NoteAftertouchAtZero_MoveToStart()
        {
            var lastEventTime = TimeSpan.FromSeconds(1);
            var noteNumber1 = (SevenBitNumber)100;
            var aftertouchValue1 = (SevenBitNumber)70;

            var noteAftertouchChangeDelay = TimeSpan.FromMilliseconds(800);
            var noteNumber2 = (SevenBitNumber)10;
            var aftertouchValue2 = (SevenBitNumber)80;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.Zero;

            CheckPlayback(
                useOutputEndpoint: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteAftertouchEvent(noteNumber1, aftertouchValue1))
                        .SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteAftertouchEvent(noteNumber2, aftertouchValue2) { Channel = (FourBitNumber)10 })
                        .SetTime((MetricTimeSpan)noteAftertouchChangeDelay, TempoMap),
                    new TimedEvent(new TextEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new NoteAftertouchEvent(noteNumber1, aftertouchValue1), TimeSpan.Zero),
                    new TimestampedEvent(new NoteAftertouchEvent(noteNumber1, aftertouchValue1), moveFrom),
                    new TimestampedEvent(new NoteAftertouchEvent(noteNumber2, aftertouchValue2) { Channel = (FourBitNumber)10 }, moveFrom + noteAftertouchChangeDelay),
                    new TimestampedEvent(new TextEvent(), moveFrom + lastEventTime),
                },
                setupPlayback: playback => playback.TrackNoteAftertouch = false);
        }

        [TimingCritical]
        [Test]
        public void DontTrackNoteAftertouch_NoteAftertouchsAtZero_MoveToStart()
        {
            var lastEventTime = TimeSpan.FromSeconds(1);
            var noteNumber1 = (SevenBitNumber)100;
            var aftertouchValue1 = (SevenBitNumber)70;

            var noteNumber2 = (SevenBitNumber)10;
            var aftertouchValue2 = (SevenBitNumber)80;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.Zero;

            CheckPlayback(
                useOutputEndpoint: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteAftertouchEvent(noteNumber1, aftertouchValue1))
                        .SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteAftertouchEvent(noteNumber2, aftertouchValue2))
                        .SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new TextEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new NoteAftertouchEvent(noteNumber1, aftertouchValue1), TimeSpan.Zero),
                    new TimestampedEvent(new NoteAftertouchEvent(noteNumber2, aftertouchValue2), TimeSpan.Zero),
                    new TimestampedEvent(new NoteAftertouchEvent(noteNumber1, aftertouchValue1), moveFrom),
                    new TimestampedEvent(new NoteAftertouchEvent(noteNumber2, aftertouchValue2), moveFrom),
                    new TimestampedEvent(new TextEvent(), lastEventTime + moveFrom),
                },
                setupPlayback: playback => playback.TrackNoteAftertouch = false);
        }

        [TimingCritical]
        [Test]
        public void DontTrackNoteAftertouch_FromBeforeNoteAftertouch_ToBeforeNoteAftertouch()
        {
            var noteAftertouchChangeTime = TimeSpan.FromMilliseconds(800);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var noteNumber = (SevenBitNumber)100;
            var aftertouchValue = (SevenBitNumber)70;

            var moveFrom = TimeSpan.FromMilliseconds(100);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckPlayback(
                useOutputEndpoint: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteAftertouchEvent(noteNumber, aftertouchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)noteAftertouchChangeTime, TempoMap),
                    new TimedEvent(new TextEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new NoteAftertouchEvent(noteNumber, aftertouchValue) { Channel = (FourBitNumber)4 }, noteAftertouchChangeTime - (moveTo - moveFrom)),
                    new TimestampedEvent(new TextEvent(), lastEventTime - (moveTo - moveFrom)),
                },
                setupPlayback: playback => playback.TrackNoteAftertouch = false);
        }

        [TimingCritical]
        [Test]
        public void DontTrackNoteAftertouch_FromBeforeNoteAftertouch_ToAfterNoteAftertouch()
        {
            var noteAftertouchChangeTime = TimeSpan.FromMilliseconds(500);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var noteNumber = (SevenBitNumber)100;
            var aftertouchValue = (SevenBitNumber)10;

            var moveFrom = TimeSpan.FromMilliseconds(300);
            var moveTo = TimeSpan.FromMilliseconds(800);

            CheckPlayback(
                useOutputEndpoint: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteAftertouchEvent(noteNumber, aftertouchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)noteAftertouchChangeTime, TempoMap),
                    new TimedEvent(new TextEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new TextEvent(), lastEventTime - (moveTo - moveFrom)),
                },
                setupPlayback: playback => playback.TrackNoteAftertouch = false);
        }

        [TimingCritical]
        [Test]
        public void DontTrackNoteAftertouch_Default_FromBeforeNoteAftertouch_ToAfterNoteAftertouch()
        {
            var noteAftertouchChangeTime = TimeSpan.FromMilliseconds(500);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var noteNumber = (SevenBitNumber)100;
            var aftertouchValue = (SevenBitNumber)0;

            var moveFrom = TimeSpan.FromMilliseconds(300);
            var moveTo = TimeSpan.FromMilliseconds(700);

            CheckPlayback(
                useOutputEndpoint: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteAftertouchEvent(noteNumber, aftertouchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)noteAftertouchChangeTime, TempoMap),
                    new TimedEvent(new TextEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new TextEvent(), lastEventTime - (moveTo - moveFrom)),
                },
                setupPlayback: playback => playback.TrackNoteAftertouch = false);
        }

        [TimingCritical]
        [Test]
        public void DontTrackNoteAftertouch_FromAfterNoteAftertouch_ToAfterNoteAftertouch()
        {
            var noteAftertouchChangeTime = TimeSpan.FromMilliseconds(500);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var noteNumber = (SevenBitNumber)0;
            var aftertouchValue = (SevenBitNumber)90;

            var moveFrom = TimeSpan.FromMilliseconds(700);
            var moveTo = TimeSpan.FromMilliseconds(900);

            CheckPlayback(
                useOutputEndpoint: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteAftertouchEvent(noteNumber, aftertouchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)noteAftertouchChangeTime, TempoMap),
                    new TimedEvent(new TextEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new NoteAftertouchEvent(noteNumber, aftertouchValue) { Channel = (FourBitNumber)4 }, noteAftertouchChangeTime),
                    new TimestampedEvent(new TextEvent(), lastEventTime - (moveTo - moveFrom)),
                },
                setupPlayback: playback => playback.TrackNoteAftertouch = false);
        }

        [TimingCritical]
        [Test]
        public void DontTrackNoteAftertouch_FromAfterNoteAftertouch_ToBeforeNoteAftertouch()
        {
            var noteAftertouchChangeTime = TimeSpan.FromMilliseconds(800);
            var lastEventTime = TimeSpan.FromSeconds(2);
            var noteNumber = (SevenBitNumber)100;
            var aftertouchValue = (SevenBitNumber)50;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckPlayback(
                useOutputEndpoint: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteAftertouchEvent(noteNumber, aftertouchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)noteAftertouchChangeTime, TempoMap),
                    new TimedEvent(new TextEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new NoteAftertouchEvent(noteNumber, aftertouchValue) { Channel = (FourBitNumber)4 }, noteAftertouchChangeTime),
                    new TimestampedEvent(new NoteAftertouchEvent(noteNumber, aftertouchValue) { Channel = (FourBitNumber)4 }, noteAftertouchChangeTime - moveTo + moveFrom),
                    new TimestampedEvent(new TextEvent(), lastEventTime  - moveTo + moveFrom),
                },
                setupPlayback: playback => playback.TrackNoteAftertouch = false);
        }

        [TimingCritical]
        [Test]
        public void DontTrackNoteAftertouch_Default_FromBeforeNoteAftertouch_ToNoteAftertouch()
        {
            var noteAftertouchChangeTime = TimeSpan.FromMilliseconds(800);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var noteNumber = (SevenBitNumber)100;
            var aftertouchValue = (SevenBitNumber)0;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(800);

            CheckPlayback(
                useOutputEndpoint: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteAftertouchEvent(noteNumber, aftertouchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)noteAftertouchChangeTime, TempoMap),
                    new TimedEvent(new TextEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new NoteAftertouchEvent(noteNumber, aftertouchValue) { Channel = (FourBitNumber)4 }, moveFrom),
                    new TimestampedEvent(new TextEvent(), lastEventTime - (moveTo - moveFrom)),
                },
                setupPlayback: playback => playback.TrackNoteAftertouch = false);
        }

        [TimingCritical]
        [Test]
        public void DontTrackNoteAftertouch_FromAfterNoteAftertouch_ToNoteAftertouch()
        {
            var noteAftertouchChangeTime = TimeSpan.FromMilliseconds(800);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var noteNumber = (SevenBitNumber)100;
            var aftertouchValue = (SevenBitNumber)50;

            var moveFrom = TimeSpan.FromMilliseconds(900);
            var moveTo = TimeSpan.FromMilliseconds(800);

            CheckPlayback(
                useOutputEndpoint: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteAftertouchEvent(noteNumber, aftertouchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)noteAftertouchChangeTime, TempoMap),
                    new TimedEvent(new TextEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new NoteAftertouchEvent(noteNumber, aftertouchValue) { Channel = (FourBitNumber)4 }, noteAftertouchChangeTime),
                    new TimestampedEvent(new NoteAftertouchEvent(noteNumber, aftertouchValue) { Channel = (FourBitNumber)4 }, moveFrom),
                    new TimestampedEvent(new TextEvent(), lastEventTime - noteAftertouchChangeTime + moveFrom),
                },
                setupPlayback: playback => playback.TrackNoteAftertouch = false);
        }

        [TimingCritical]
        [Test]
        public void TrackNoteAftertouch_DisableInMiddle_FromBeforeNoteAftertouch_ToAfterNoteAftertouch()
        {
            var noteAftertouchChangeTime = TimeSpan.FromMilliseconds(800);
            var programChangeTime = TimeSpan.FromSeconds(1);
            var lastEventTime = TimeSpan.FromSeconds(2);
            var noteNumber = (SevenBitNumber)100;
            var aftertouchValue = (SevenBitNumber)10;
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(1200);
            var disableAfter = TimeSpan.FromMilliseconds(500);

            CheckPlayback(
                useOutputEndpoint: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteAftertouchEvent(noteNumber, aftertouchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)noteAftertouchChangeTime, TempoMap),
                    new TimedEvent(new ProgramChangeEvent(programNumber))
                        .SetTime((MetricTimeSpan)programChangeTime, TempoMap),
                    new TimedEvent(new TextEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                    new PlaybackAction(disableAfter,
                        p => p.TrackNoteAftertouch = false),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new NoteAftertouchEvent(noteNumber, aftertouchValue) { Channel = (FourBitNumber)4 }, moveFrom),
                    new TimestampedEvent(new TextEvent(), lastEventTime + moveFrom - moveTo),
                },
                setupPlayback: playback => playback.TrackProgram = false);
        }

        [TimingCritical]
        [Test]
        public void TrackNoteAftertouch_DisableInMiddle_FromAfterNoteAftertouch_ToBeforeNoteAftertouch()
        {
            var noteAftertouchChangeTime = TimeSpan.FromMilliseconds(800);
            var lastEventTime = TimeSpan.FromSeconds(2);
            var noteNumber = (SevenBitNumber)100;
            var aftertouchValue = (SevenBitNumber)50;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(500);
            var disableAfter = TimeSpan.FromMilliseconds(150);

            CheckPlayback(
                useOutputEndpoint: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteAftertouchEvent(noteNumber, aftertouchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)noteAftertouchChangeTime, TempoMap),
                    new TimedEvent(new TextEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                    new PlaybackAction(disableAfter,
                        p => p.TrackNoteAftertouch = false),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new NoteAftertouchEvent(noteNumber, aftertouchValue) { Channel = (FourBitNumber)4 }, noteAftertouchChangeTime),
                    new TimestampedEvent(new NoteAftertouchEvent(noteNumber, SevenBitNumber.MinValue) { Channel = (FourBitNumber)4 }, moveFrom),
                    new TimestampedEvent(new NoteAftertouchEvent(noteNumber, aftertouchValue) { Channel = (FourBitNumber)4 }, noteAftertouchChangeTime - moveTo + moveFrom),
                    new TimestampedEvent(new TextEvent(), lastEventTime - moveTo + moveFrom),
                });
        }

        #endregion
    }
}
