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
        public void TrackChannelAftertouch_NoChannelAftertouchChanges_MoveToTime(
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
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFromMs, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new NoteAftertouchEvent(), lastEventTime - (moveTo - moveFrom)),
                });
        }

        [TimingCritical]
        [Test]
        public void TrackChannelAftertouch_ChannelAftertouchChangeAtZero_MoveToTime()
        {
            var lastEventTime = TimeSpan.FromSeconds(1);
            var aftertouchValue = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(100);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckPlayback(
                useOutputEndpoint: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ChannelAftertouchEvent(aftertouchValue)),
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new ChannelAftertouchEvent(aftertouchValue), TimeSpan.Zero),
                    new TimestampedEvent(new NoteAftertouchEvent(), lastEventTime - (moveTo - moveFrom)),
                });
        }

        [TimingCritical]
        [Test]
        public void TrackChannelAftertouch_ChannelAftertouchChangeAtZero_MoveToStart()
        {
            var lastEventTime = TimeSpan.FromSeconds(1);
            var aftertouchValue = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.Zero;

            CheckPlayback(
                useOutputEndpoint: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ChannelAftertouchEvent(aftertouchValue)),
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new ChannelAftertouchEvent(aftertouchValue), TimeSpan.Zero),
                    new TimestampedEvent(new ChannelAftertouchEvent(aftertouchValue), moveFrom),
                    new TimestampedEvent(new NoteAftertouchEvent(), lastEventTime - (moveTo - moveFrom)),
                });
        }

        [TimingCritical]
        [Test]
        public void TrackChannelAftertouch_FromBeforeChannelAftertouch_ToBeforeChannelAftertouch()
        {
            var programChangeTime = TimeSpan.FromMilliseconds(800);
            var lastEventTime = TimeSpan.FromSeconds(2);
            var aftertouchValue = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(100);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckPlayback(
                useOutputEndpoint: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ChannelAftertouchEvent(aftertouchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)programChangeTime, TempoMap),
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new ChannelAftertouchEvent(aftertouchValue) { Channel = (FourBitNumber)4 }, programChangeTime - (moveTo - moveFrom)),
                    new TimestampedEvent(new NoteAftertouchEvent(), moveFrom - moveTo + lastEventTime),
                });
        }

        [TimingCritical]
        [Test]
        public void TrackChannelAftertouch_FromBeforeChannelAftertouch_ToAfterProgramChange()
        {
            var programChangeTime = TimeSpan.FromMilliseconds(500);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var aftertouchValue = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(300);
            var moveTo = TimeSpan.FromMilliseconds(700);

            CheckPlayback(
                useOutputEndpoint: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ChannelAftertouchEvent(aftertouchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)programChangeTime, TempoMap),
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new ChannelAftertouchEvent(aftertouchValue) { Channel = (FourBitNumber)4 }, moveFrom),
                    new TimestampedEvent(new NoteAftertouchEvent(), lastEventTime - (moveTo - moveFrom)),
                });
        }

        [TimingCritical]
        [Test]
        public void TrackChannelAftertouch_FromAfterChannelAftertouch_ToAfterChannelAftertouch()
        {
            var programChangeTime = TimeSpan.FromMilliseconds(800);
            var lastEventTime = TimeSpan.FromSeconds(2);
            var aftertouchValue = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(1500);

            CheckPlayback(
                useOutputEndpoint: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ChannelAftertouchEvent(aftertouchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)programChangeTime, TempoMap),
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new ChannelAftertouchEvent(aftertouchValue) { Channel = (FourBitNumber)4 }, programChangeTime),
                    new TimestampedEvent(new NoteAftertouchEvent(), lastEventTime - (moveTo - moveFrom)),
                });
        }

        [TimingCritical]
        [Test]
        public void TrackChannelAftertouch_FromAfterChannelAftertouch_ToBeforeChannelAftertouch()
        {
            var programChangeTime = TimeSpan.FromMilliseconds(700);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var aftertouchValue = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(800);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckPlayback(
                useOutputEndpoint: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ChannelAftertouchEvent(aftertouchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)programChangeTime, TempoMap),
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new ChannelAftertouchEvent(aftertouchValue) { Channel = (FourBitNumber)4 }, programChangeTime),
                    new TimestampedEvent(new ChannelAftertouchEvent(SevenBitNumber.MinValue) { Channel = (FourBitNumber)4 }, moveFrom),
                    new TimestampedEvent(new ChannelAftertouchEvent(aftertouchValue) { Channel = (FourBitNumber)4 }, programChangeTime + moveFrom - moveTo),
                    new TimestampedEvent(new NoteAftertouchEvent(), lastEventTime + moveFrom - moveTo),
                });
        }

        [TimingCritical]
        [Test]
        public void TrackChannelAftertouch_FromBeforeChannelAftertouch_ToChannelAftertouch()
        {
            var programChangeTime = TimeSpan.FromMilliseconds(800);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var aftertouchValue = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(800);

            CheckPlayback(
                useOutputEndpoint: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ChannelAftertouchEvent(aftertouchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)programChangeTime, TempoMap),
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new ChannelAftertouchEvent(aftertouchValue) { Channel = (FourBitNumber)4 }, moveFrom),
                    new TimestampedEvent(new NoteAftertouchEvent(), lastEventTime - (moveTo - moveFrom)),
                });
        }

        [TimingCritical]
        [Test]
        public void TrackChannelAftertouch_FromAfterChannelAftertouch_ToChannelAftertouch()
        {
            var programChangeTime = TimeSpan.FromMilliseconds(500);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var aftertouchValue = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(700);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckPlayback(
                useOutputEndpoint: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ChannelAftertouchEvent(aftertouchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)programChangeTime, TempoMap),
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new ChannelAftertouchEvent(aftertouchValue) { Channel = (FourBitNumber)4 }, programChangeTime),
                    new TimestampedEvent(new ChannelAftertouchEvent(aftertouchValue) { Channel = (FourBitNumber)4 }, moveFrom),
                    new TimestampedEvent(new NoteAftertouchEvent(), lastEventTime - programChangeTime + moveFrom),
                });
        }

        [TimingCritical]
        [Test]
        public void TrackChannelAftertouch_EnableInMiddle_FromBeforeChannelAftertouch_ToAfterChannelAftertouch()
        {
            var programChangeTime = TimeSpan.FromMilliseconds(800);
            var pitchBendTime = TimeSpan.FromSeconds(1);
            var lastEventTime = TimeSpan.FromSeconds(2);
            var aftertouchValue = (SevenBitNumber)100;
            var pitchValue = (ushort)1000;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(1200);
            var enableAfter = TimeSpan.FromMilliseconds(500);

            CheckPlayback(
                useOutputEndpoint: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ChannelAftertouchEvent(aftertouchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)programChangeTime, TempoMap),
                    new TimedEvent(new PitchBendEvent(pitchValue))
                        .SetTime((MetricTimeSpan)pitchBendTime, TempoMap),
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                    new PlaybackAction(enableAfter, p => p.TrackChannelAftertouch = true),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new ChannelAftertouchEvent(aftertouchValue) { Channel = (FourBitNumber)4 }, moveFrom + enableAfter),
                    new TimestampedEvent(new NoteAftertouchEvent(), moveFrom + lastEventTime - moveTo),
                },
                setupPlayback: playback =>
                {
                    playback.TrackChannelAftertouch = false;
                    playback.TrackPitchValue = false;
                });
        }

        [TimingCritical]
        [Test]
        public void TrackChannelAftertouch_EnableInMiddle_FromAfterChannelAftertouch_ToBeforeChannelAftertouch()
        {
            var programChangeTime = TimeSpan.FromMilliseconds(800);
            var lastEventTime = TimeSpan.FromSeconds(2);
            var aftertouchValue = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(500);
            var enableAfter = TimeSpan.FromMilliseconds(150);

            CheckPlayback(
                useOutputEndpoint: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ChannelAftertouchEvent(aftertouchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)programChangeTime, TempoMap),
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                    new PlaybackAction(enableAfter, p => p.TrackChannelAftertouch = true),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new ChannelAftertouchEvent(aftertouchValue) { Channel = (FourBitNumber)4 }, programChangeTime),
                    new TimestampedEvent(new ChannelAftertouchEvent(SevenBitNumber.MinValue) { Channel = (FourBitNumber)4 }, moveFrom + enableAfter),
                    new TimestampedEvent(new ChannelAftertouchEvent(aftertouchValue) { Channel = (FourBitNumber)4 }, moveFrom + programChangeTime - moveTo),
                    new TimestampedEvent(new NoteAftertouchEvent(), moveFrom - moveTo + lastEventTime),
                },
                setupPlayback: playback => playback.TrackChannelAftertouch = false);
        }

        [TimingCritical]
        [Test]
        public void DontTrackChannelAftertouch_NoChannelAftertouchChanges_MoveToTime(
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
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFromMs, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new NoteAftertouchEvent(), lastEventTime - (moveTo - moveFrom)),
                },
                setupPlayback: playback => playback.TrackChannelAftertouch = false);
        }

        [TimingCritical]
        [Test]
        public void DontTrackChannelAftertouch_ChannelAftertouchAtZero_MoveToTime()
        {
            var lastEventTime = TimeSpan.FromSeconds(1);
            var aftertouchValue = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(100);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckPlayback(
                useOutputEndpoint: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ChannelAftertouchEvent(aftertouchValue)),
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new ChannelAftertouchEvent(aftertouchValue), TimeSpan.Zero),
                    new TimestampedEvent(new NoteAftertouchEvent(), lastEventTime - (moveTo - moveFrom)),
                },
                setupPlayback: playback => playback.TrackChannelAftertouch = false);
        }

        [TimingCritical]
        [Test]
        public void DontTrackChannelAftertouch_ChannelAftertouchAtZero_MoveToStart()
        {
            var lastEventTime = TimeSpan.FromSeconds(1);
            var aftertouchValue = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.Zero;

            CheckPlayback(
                useOutputEndpoint: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ChannelAftertouchEvent(aftertouchValue)),
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new ChannelAftertouchEvent(aftertouchValue), TimeSpan.Zero),
                    new TimestampedEvent(new ChannelAftertouchEvent(aftertouchValue), moveFrom),
                    new TimestampedEvent(new NoteAftertouchEvent(), lastEventTime - (moveTo - moveFrom)),
                },
                setupPlayback: playback => playback.TrackChannelAftertouch = false);
        }

        [TimingCritical]
        [Test]
        public void DontTrackChannelAftertouch_FromBeforeChannelAftertouch_ToBeforeChannelAftertouch()
        {
            var programChangeTime = TimeSpan.FromMilliseconds(800);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var aftertouchValue = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(100);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckPlayback(
                useOutputEndpoint: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ChannelAftertouchEvent(aftertouchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)programChangeTime, TempoMap),
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new ChannelAftertouchEvent(aftertouchValue) { Channel = (FourBitNumber)4 }, programChangeTime - (moveTo - moveFrom)),
                    new TimestampedEvent(new NoteAftertouchEvent(), lastEventTime - (moveTo - moveFrom)),
                },
                setupPlayback: playback => playback.TrackChannelAftertouch = false);
        }

        [TimingCritical]
        [Test]
        public void DontTrackChannelAftertouch_FromBeforeChannelAftertouch_ToAfterChannelAftertouch()
        {
            var programChangeTime = TimeSpan.FromMilliseconds(500);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var aftertouchValue = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(300);
            var moveTo = TimeSpan.FromMilliseconds(700);

            CheckPlayback(
                useOutputEndpoint: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ChannelAftertouchEvent(aftertouchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)programChangeTime, TempoMap),
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new NoteAftertouchEvent(), lastEventTime - (moveTo - moveFrom)),
                },
                setupPlayback: playback => playback.TrackChannelAftertouch = false);
        }

        [TimingCritical]
        [Test]
        public void DontTrackChannelAftertouch_FromAfterChannelAftertouch_ToAfterChannelAftertouch()
        {
            var programChangeTime = TimeSpan.FromMilliseconds(500);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var aftertouchValue = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(700);
            var moveTo = TimeSpan.FromMilliseconds(900);

            CheckPlayback(
                useOutputEndpoint: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ChannelAftertouchEvent(aftertouchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)programChangeTime, TempoMap),
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new ChannelAftertouchEvent(aftertouchValue) { Channel = (FourBitNumber)4 }, programChangeTime),
                    new TimestampedEvent(new NoteAftertouchEvent(), lastEventTime - (moveTo - moveFrom)),
                },
                setupPlayback: playback => playback.TrackChannelAftertouch = false);
        }

        [TimingCritical]
        [Test]
        public void DontTrackChannelAftertouch_FromAfterChannelAftertouch_ToBeforeChannelAftertouch()
        {
            var programChangeTime = TimeSpan.FromMilliseconds(500);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var aftertouchValue = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(700);
            var moveTo = TimeSpan.FromMilliseconds(300);

            CheckPlayback(
                useOutputEndpoint: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ChannelAftertouchEvent(aftertouchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)programChangeTime, TempoMap),
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new ChannelAftertouchEvent(aftertouchValue) { Channel = (FourBitNumber)4 }, programChangeTime),
                    new TimestampedEvent(new ChannelAftertouchEvent(aftertouchValue) { Channel = (FourBitNumber)4 }, programChangeTime + moveFrom - moveTo),
                    new TimestampedEvent(new NoteAftertouchEvent(), moveFrom - moveTo + lastEventTime),
                },
                setupPlayback: playback => playback.TrackChannelAftertouch = false);
        }

        [TimingCritical]
        [Test]
        public void DontTrackChannelAftertouch_FromBeforeChannelAftertouch_ToChannelAftertouch()
        {
            var programChangeTime = TimeSpan.FromMilliseconds(800);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var aftertouchValue = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(800);

            CheckPlayback(
                useOutputEndpoint: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ChannelAftertouchEvent(aftertouchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)programChangeTime, TempoMap),
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new ChannelAftertouchEvent(aftertouchValue) { Channel = (FourBitNumber)4 }, moveFrom),
                    new TimestampedEvent(new NoteAftertouchEvent(), lastEventTime - (moveTo - moveFrom)),
                },
                setupPlayback: playback => playback.TrackChannelAftertouch = false);
        }

        [TimingCritical]
        [Test]
        public void DontTrackChannelAftertouch_FromAfterChannelAftertouch_ToChannelAftertouch()
        {
            var programChangeTime = TimeSpan.FromMilliseconds(800);
            var lastEventTime = TimeSpan.FromSeconds(2);
            var aftertouchValue = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(800);

            CheckPlayback(
                useOutputEndpoint: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ChannelAftertouchEvent(aftertouchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)programChangeTime, TempoMap),
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new ChannelAftertouchEvent(aftertouchValue) { Channel = (FourBitNumber)4 }, programChangeTime),
                    new TimestampedEvent(new ChannelAftertouchEvent(aftertouchValue) { Channel = (FourBitNumber)4 }, moveFrom),
                    new TimestampedEvent(new NoteAftertouchEvent(), moveFrom + lastEventTime - programChangeTime),
                },
                setupPlayback: playback => playback.TrackChannelAftertouch = false);
        }

        [TimingCritical]
        [Test]
        public void TrackChannelAftertouch_DisableInMiddle_FromBeforeChannelAftertouch_ToAfterChannelAftertouch()
        {
            var programChangeTime = TimeSpan.FromMilliseconds(800);
            var pitchBendTime = TimeSpan.FromSeconds(1);
            var lastEventTime = TimeSpan.FromSeconds(2);
            var aftertouchValue = (SevenBitNumber)100;
            var pitchValue = (ushort)1000;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(1200);
            var disableAfter = TimeSpan.FromMilliseconds(500);

            CheckPlayback(
                useOutputEndpoint: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ChannelAftertouchEvent(aftertouchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)programChangeTime, TempoMap),
                    new TimedEvent(new PitchBendEvent(pitchValue))
                        .SetTime((MetricTimeSpan)pitchBendTime, TempoMap),
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                    new PlaybackAction(disableAfter, p => p.TrackChannelAftertouch = false),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new ChannelAftertouchEvent(aftertouchValue) { Channel = (FourBitNumber)4 }, moveFrom),
                    new TimestampedEvent(new NoteAftertouchEvent(), moveFrom + lastEventTime - moveTo),
                },
                setupPlayback: playback => playback.TrackPitchValue = false);
        }

        [TimingCritical]
        [Test]
        public void TrackChannelAftertouch_DisableInMiddle_FromAfterChannelAftertouch_ToBeforeChannelAftertouch()
        {
            var programChangeTime = TimeSpan.FromMilliseconds(800);
            var lastEventTime = TimeSpan.FromSeconds(2);
            var aftertouchValue = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(500);
            var disableAfter = TimeSpan.FromMilliseconds(150);

            CheckPlayback(
                useOutputEndpoint: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ChannelAftertouchEvent(aftertouchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)programChangeTime, TempoMap),
                    new TimedEvent(new NoteAftertouchEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                    new PlaybackAction(disableAfter, p => p.TrackChannelAftertouch = false),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new ChannelAftertouchEvent(aftertouchValue) { Channel = (FourBitNumber)4 }, programChangeTime),
                    new TimestampedEvent(new ChannelAftertouchEvent(SevenBitNumber.MinValue) { Channel = (FourBitNumber)4 }, moveFrom),
                    new TimestampedEvent(new ChannelAftertouchEvent(aftertouchValue) { Channel = (FourBitNumber)4 }, moveFrom + programChangeTime - moveTo),
                    new TimestampedEvent(new NoteAftertouchEvent(), moveFrom - moveTo + lastEventTime),
                });
        }

        #endregion
    }
}
