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
        public void TrackPitchValue_NoPitchBend_MoveToTime(
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
                    new TimedEvent(new StartEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo))
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new StartEvent(), lastEventTime - (moveTo - moveFrom)),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackPitchValue_PitchBendAtZero_MoveToTime()
        {
            var lastEventTime = TimeSpan.FromSeconds(1);
            var pitchValue = (ushort)234;

            var moveFrom = TimeSpan.FromMilliseconds(100);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new PitchBendEvent(pitchValue))
                        .SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new StartEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo))
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new PitchBendEvent(pitchValue), TimeSpan.Zero),
                    new ReceivedEvent(new StartEvent(), lastEventTime - (moveTo - moveFrom)),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackPitchValue_PitchBendAtZero_MoveToStart()
        {
            var lastEventTime = TimeSpan.FromSeconds(1);
            var pitchValue = (ushort)234;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.Zero;

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new PitchBendEvent(pitchValue))
                        .SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new StartEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo))
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new PitchBendEvent(pitchValue), TimeSpan.Zero),
                    new ReceivedEvent(new PitchBendEvent(pitchValue), moveFrom),
                    new ReceivedEvent(new StartEvent(), lastEventTime - (moveTo - moveFrom)),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackPitchValue_FromBeforePitchBend_ToBeforePitchBend()
        {
            var pitchBendTime = TimeSpan.FromMilliseconds(800);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var pitchValue = (ushort)234;

            var moveFrom = TimeSpan.FromMilliseconds(100);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)pitchBendTime, TempoMap),
                    new TimedEvent(new StartEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo))
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, pitchBendTime - (moveTo - moveFrom)),
                    new ReceivedEvent(new StartEvent(), lastEventTime - (moveTo - moveFrom)),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackPitchValue_FromBeforePitchBend_ToAfterPitchBend()
        {
            var pitchBendTime = TimeSpan.FromMilliseconds(500);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var pitchValue = (ushort)234;

            var moveFrom = TimeSpan.FromMilliseconds(300);
            var moveTo = TimeSpan.FromMilliseconds(700);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)pitchBendTime, TempoMap),
                    new TimedEvent(new StartEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo))
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, moveFrom),
                    new ReceivedEvent(new StartEvent(), lastEventTime - (moveTo - moveFrom)),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackPitchValue_FromAfterPitchBend_ToAfterPitchBend()
        {
            var pitchBendTime = TimeSpan.FromMilliseconds(400);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var pitchValue = (ushort)234;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(700);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)pitchBendTime, TempoMap),
                    new TimedEvent(new StartEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo))
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, pitchBendTime),
                    new ReceivedEvent(new StartEvent(), lastEventTime - (moveTo - moveFrom)),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackPitchValue_FromAfterPitchBend_ToBeforePitchBend()
        {
            var pitchBendTime = TimeSpan.FromMilliseconds(500);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var pitchValue = (ushort)234;

            var moveFrom = TimeSpan.FromMilliseconds(700);
            var moveTo = TimeSpan.FromMilliseconds(300);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)pitchBendTime, TempoMap),
                    new TimedEvent(new StartEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo))
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, pitchBendTime),
                    new ReceivedEvent(new PitchBendEvent() { Channel = (FourBitNumber)4 }, moveFrom),
                    new ReceivedEvent(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, pitchBendTime + moveFrom - moveTo),
                    new ReceivedEvent(new StartEvent(), lastEventTime + moveFrom - moveTo),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackPitchValue_FromBeforePitchBend_ToPitchBend()
        {
            var pitchBendTime = TimeSpan.FromMilliseconds(800);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var pitchValue = (ushort)234;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(800);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)pitchBendTime, TempoMap),
                    new TimedEvent(new StartEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo))
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, moveFrom),
                    new ReceivedEvent(new StartEvent(), lastEventTime + moveFrom - moveTo),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackPitchValue_FromAfterPitchBend_ToPitchBend()
        {
            var pitchBendTime = TimeSpan.FromMilliseconds(800);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var pitchValue = (ushort)234;

            var moveFrom = TimeSpan.FromMilliseconds(900);
            var moveTo = TimeSpan.FromMilliseconds(800);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)pitchBendTime, TempoMap),
                    new TimedEvent(new StartEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo))
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, pitchBendTime),
                    new ReceivedEvent(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, moveFrom),
                    new ReceivedEvent(new StartEvent(), lastEventTime - pitchBendTime + moveFrom),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackPitchValue_EnableInMiddle_FromBeforePitchBend_ToAfterPitchBend()
        {
            var pitchBendTime = TimeSpan.FromMilliseconds(800);
            var programChangeTime = TimeSpan.FromSeconds(1);
            var lastEventTime = TimeSpan.FromSeconds(2);
            var pitchValue = (ushort)234;
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(1200);
            var enableAfter = TimeSpan.FromMilliseconds(500);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)pitchBendTime, TempoMap),
                    new TimedEvent(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)programChangeTime, TempoMap),
                    new TimedEvent(new StartEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                    new PlaybackChangerBase(enableAfter,
                        p => p.TrackPitchValue = true),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, moveFrom + enableAfter),
                    new ReceivedEvent(new StartEvent(), lastEventTime - moveTo + moveFrom),
                },
                setupPlayback: playback =>
                {
                    playback.TrackProgram = false;
                    playback.TrackPitchValue = false;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackPitchValue_EnableInMiddle_FromAfterPitchBend_ToBeforePitchBend()
        {
            var pitchBendTime = TimeSpan.FromMilliseconds(800);
            var lastEventTime = TimeSpan.FromSeconds(2);
            var pitchValue = (ushort)234;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(500);
            var enableAfter = TimeSpan.FromMilliseconds(150);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)pitchBendTime, TempoMap),
                    new TimedEvent(new StartEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                    new PlaybackChangerBase(enableAfter,
                        p => p.TrackPitchValue = true),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, pitchBendTime),
                    new ReceivedEvent(new PitchBendEvent() { Channel = (FourBitNumber)4 }, moveFrom + enableAfter),
                    new ReceivedEvent(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, pitchBendTime - moveTo + moveFrom),
                    new ReceivedEvent(new StartEvent(), lastEventTime - moveTo + moveFrom),
                },
                setupPlayback: playback => playback.TrackPitchValue = false);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void DontTrackPitchValue_NoPitchBend_MoveToTime(
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
                    new TimedEvent(new StartEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo))
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new StartEvent(), lastEventTime - (moveTo - moveFrom)),
                },
                setupPlayback: playback => playback.TrackPitchValue = false);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void DontTrackPitchValue_PitchBendAtZero_MoveToTime()
        {
            var lastEventTime = TimeSpan.FromSeconds(1);
            var pitchValue = (ushort)234;

            var moveFrom = TimeSpan.FromMilliseconds(100);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new PitchBendEvent(pitchValue))
                        .SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new StartEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo))
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new PitchBendEvent(pitchValue), TimeSpan.Zero),
                    new ReceivedEvent(new StartEvent(), lastEventTime - (moveTo - moveFrom)),
                },
                setupPlayback: playback => playback.TrackPitchValue = false);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void DontTrackPitchValue_PitchBendAtZero_MoveToStart()
        {
            var lastEventTime = TimeSpan.FromSeconds(1);
            var pitchValue = (ushort)234;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.Zero;

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new PitchBendEvent(pitchValue))
                        .SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new StartEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo))
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new PitchBendEvent(pitchValue), TimeSpan.Zero),
                    new ReceivedEvent(new PitchBendEvent(pitchValue), moveFrom),
                    new ReceivedEvent(new StartEvent(), lastEventTime - (moveTo - moveFrom)),
                },
                setupPlayback: playback => playback.TrackPitchValue = false);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void DontTrackPitchValue_FromBeforePitchBend_ToBeforePitchBend()
        {
            var pitchBendTime = TimeSpan.FromMilliseconds(800);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var pitchValue = (ushort)234;

            var moveFrom = TimeSpan.FromMilliseconds(100);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)pitchBendTime, TempoMap),
                    new TimedEvent(new StartEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo))
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, pitchBendTime - (moveTo - moveFrom)),
                    new ReceivedEvent(new StartEvent(), lastEventTime - (moveTo - moveFrom)),
                },
                setupPlayback: playback => playback.TrackPitchValue = false);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void DontTrackPitchValue_FromBeforePitchBend_ToAfterPitchBend()
        {
            var pitchBendTime = TimeSpan.FromMilliseconds(500);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var pitchValue = (ushort)234;

            var moveFrom = TimeSpan.FromMilliseconds(300);
            var moveTo = TimeSpan.FromMilliseconds(700);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)pitchBendTime, TempoMap),
                    new TimedEvent(new StartEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo))
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new StartEvent(), lastEventTime - (moveTo - moveFrom)),
                },
                setupPlayback: playback => playback.TrackPitchValue = false);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void DontTrackPitchValue_FromAfterPitchBend_ToAfterPitchBend()
        {
            var pitchBendTime = TimeSpan.FromMilliseconds(400);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var pitchValue = (ushort)234;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(700);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)pitchBendTime, TempoMap),
                    new TimedEvent(new StartEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo))
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, pitchBendTime),
                    new ReceivedEvent(new StartEvent(), lastEventTime - (moveTo - moveFrom)),
                },
                setupPlayback: playback => playback.TrackPitchValue = false);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void DontTrackPitchValue_FromAfterPitchBend_ToBeforePitchBend()
        {
            var pitchBendTime = TimeSpan.FromMilliseconds(500);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var pitchValue = (ushort)234;

            var moveFrom = TimeSpan.FromMilliseconds(700);
            var moveTo = TimeSpan.FromMilliseconds(300);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)pitchBendTime, TempoMap),
                    new TimedEvent(new StartEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo))
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, pitchBendTime),
                    new ReceivedEvent(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, pitchBendTime + moveFrom - moveTo),
                    new ReceivedEvent(new StartEvent(), lastEventTime + moveFrom - moveTo),
                },
                setupPlayback: playback => playback.TrackPitchValue = false);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void DontTrackPitchValue_FromBeforePitchBend_ToPitchBend()
        {
            var pitchBendTime = TimeSpan.FromMilliseconds(800);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var pitchValue = (ushort)234;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(800);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)pitchBendTime, TempoMap),
                    new TimedEvent(new StartEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo))
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, moveFrom),
                    new ReceivedEvent(new StartEvent(), lastEventTime + moveFrom - moveTo),
                },
                setupPlayback: playback => playback.TrackPitchValue = false);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void DontTrackPitchValue_FromAfterPitchBend_ToPitchBend()
        {
            var pitchBendTime = TimeSpan.FromMilliseconds(800);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var pitchValue = (ushort)234;

            var moveFrom = TimeSpan.FromMilliseconds(900);
            var moveTo = TimeSpan.FromMilliseconds(800);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)pitchBendTime, TempoMap),
                    new TimedEvent(new StartEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo))
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, pitchBendTime),
                    new ReceivedEvent(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, moveFrom),
                    new ReceivedEvent(new StartEvent(), lastEventTime - pitchBendTime + moveFrom),
                },
                setupPlayback: playback => playback.TrackPitchValue = false);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackPitchValue_DisableInMiddle_FromBeforePitchBend_ToAfterPitchBend()
        {
            var pitchBendTime = TimeSpan.FromMilliseconds(800);
            var programChangeTime = TimeSpan.FromSeconds(1);
            var lastEventTime = TimeSpan.FromSeconds(2);
            var pitchValue = (ushort)234;
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(1200);
            var disableAfter = TimeSpan.FromMilliseconds(500);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)pitchBendTime, TempoMap),
                    new TimedEvent(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)programChangeTime, TempoMap),
                    new TimedEvent(new StartEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                    new PlaybackChangerBase(disableAfter,
                        p => p.TrackPitchValue = false),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, moveFrom),
                    new ReceivedEvent(new StartEvent(), lastEventTime - moveTo + moveFrom),
                },
                setupPlayback: playback =>
                {
                    playback.TrackProgram = false;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackPitchValue_DisableInMiddle_FromAfterPitchBend_ToBeforePitchBend()
        {
            var pitchBendTime = TimeSpan.FromMilliseconds(800);
            var lastEventTime = TimeSpan.FromSeconds(2);
            var pitchValue = (ushort)234;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(500);
            var disableAfter = TimeSpan.FromMilliseconds(150);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)pitchBendTime, TempoMap),
                    new TimedEvent(new StartEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                    new PlaybackChangerBase(disableAfter,
                        p => p.TrackPitchValue = false),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, pitchBendTime),
                    new ReceivedEvent(new PitchBendEvent() { Channel = (FourBitNumber)4 }, moveFrom),
                    new ReceivedEvent(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, pitchBendTime - moveTo + moveFrom),
                    new ReceivedEvent(new StartEvent(), lastEventTime - moveTo + moveFrom),
                });
        }

        #endregion
    }
}
