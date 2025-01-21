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
        public void TrackProgram_NoProgramChanges_MoveToTime(
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
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap.Default),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(moveFromMs, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new StartEvent(), lastEventTime - (moveTo - moveFrom)),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackProgram_ProgramChangeAtZero_MoveToTime()
        {
            var lastEventTime = TimeSpan.FromSeconds(1);
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(100);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ProgramChangeEvent(programNumber)),
                    new TimedEvent(new StartEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap.Default),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new ProgramChangeEvent(programNumber), TimeSpan.Zero),
                    new ReceivedEvent(new StartEvent(), lastEventTime - (moveTo - moveFrom)),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackProgram_ProgramChangeAtZero_MoveToStart()
        {
            var lastEventTime = TimeSpan.FromSeconds(1);
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.Zero;

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ProgramChangeEvent(programNumber)),
                    new TimedEvent(new StartEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap.Default),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new ProgramChangeEvent(programNumber), TimeSpan.Zero),
                    new ReceivedEvent(new ProgramChangeEvent(programNumber), moveFrom),
                    new ReceivedEvent(new StartEvent(), lastEventTime - (moveTo - moveFrom)),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackProgram_FromBeforeProgramChange_ToBeforeProgramChange()
        {
            var programChangeTime = TimeSpan.FromMilliseconds(800);
            var lastEventTime = TimeSpan.FromSeconds(2);
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(100);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)programChangeTime, TempoMap.Default),
                    new TimedEvent(new StartEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap.Default),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, programChangeTime - (moveTo - moveFrom)),
                    new ReceivedEvent(new StartEvent(), moveFrom - moveTo + lastEventTime),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackProgram_FromBeforeProgramChange_ToAfterProgramChange()
        {
            var programChangeTime = TimeSpan.FromMilliseconds(500);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(300);
            var moveTo = TimeSpan.FromMilliseconds(700);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)programChangeTime, TempoMap.Default),
                    new TimedEvent(new StartEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap.Default),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, moveFrom),
                    new ReceivedEvent(new StartEvent(), lastEventTime - (moveTo - moveFrom)),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackProgram_FromAfterProgramChange_ToAfterProgramChange()
        {
            var programChangeTime = TimeSpan.FromMilliseconds(800);
            var lastEventTime = TimeSpan.FromSeconds(2);
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(1500);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)programChangeTime, TempoMap.Default),
                    new TimedEvent(new StartEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap.Default),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, programChangeTime),
                    new ReceivedEvent(new StartEvent(), lastEventTime - (moveTo - moveFrom)),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackProgram_FromAfterProgramChange_ToBeforeProgramChange()
        {
            var programChangeTime = TimeSpan.FromMilliseconds(700);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(800);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)programChangeTime, TempoMap.Default),
                    new TimedEvent(new StartEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap.Default),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, programChangeTime),
                    new ReceivedEvent(new ProgramChangeEvent(SevenBitNumber.MinValue) { Channel = (FourBitNumber)4 }, moveFrom),
                    new ReceivedEvent(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, programChangeTime + moveFrom - moveTo),
                    new ReceivedEvent(new StartEvent(), lastEventTime + moveFrom - moveTo),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackProgram_FromBeforeProgramChange_ToProgramChange()
        {
            var programChangeTime = TimeSpan.FromMilliseconds(800);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(800);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)programChangeTime, TempoMap.Default),
                    new TimedEvent(new StartEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap.Default),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, moveFrom),
                    new ReceivedEvent(new StartEvent(), lastEventTime - (moveTo - moveFrom)),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackProgram_FromAfterProgramChange_ToProgramChange()
        {
            var programChangeTime = TimeSpan.FromMilliseconds(500);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(700);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)programChangeTime, TempoMap.Default),
                    new TimedEvent(new StartEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap.Default),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, programChangeTime),
                    new ReceivedEvent(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, moveFrom),
                    new ReceivedEvent(new StartEvent(), lastEventTime - programChangeTime + moveFrom),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackProgram_EnableInMiddle_FromBeforeProgramChange_ToAfterProgramChange()
        {
            var programChangeTime = TimeSpan.FromMilliseconds(800);
            var pitchBendTime = TimeSpan.FromSeconds(1);
            var lastEventTime = TimeSpan.FromSeconds(2);
            var programNumber = (SevenBitNumber)100;
            var pitchValue = (ushort)1000;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(1200);
            var enableAfter = TimeSpan.FromMilliseconds(500);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)programChangeTime, TempoMap.Default),
                    new TimedEvent(new PitchBendEvent(pitchValue))
                        .SetTime((MetricTimeSpan)pitchBendTime, TempoMap.Default),
                    new TimedEvent(new StartEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap.Default),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                    new PlaybackChangerBase(enableAfter, p => p.TrackProgram = true),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, moveFrom + enableAfter),
                    new ReceivedEvent(new StartEvent(), moveFrom + lastEventTime - moveTo),
                },
                setupPlayback: playback =>
                {
                    playback.TrackProgram = false;
                    playback.TrackPitchValue = false;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackProgram_EnableInMiddle_FromAfterProgramChange_ToBeforeProgramChange()
        {
            var programChangeTime = TimeSpan.FromMilliseconds(800);
            var lastEventTime = TimeSpan.FromSeconds(2);
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(500);
            var enableAfter = TimeSpan.FromMilliseconds(150);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)programChangeTime, TempoMap.Default),
                    new TimedEvent(new StartEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap.Default),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                    new PlaybackChangerBase(enableAfter, p => p.TrackProgram = true),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, programChangeTime),
                    new ReceivedEvent(new ProgramChangeEvent(SevenBitNumber.MinValue) { Channel = (FourBitNumber)4 }, moveFrom + enableAfter),
                    new ReceivedEvent(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, moveFrom + programChangeTime - moveTo),
                    new ReceivedEvent(new StartEvent(), moveFrom - moveTo + lastEventTime),
                },
                setupPlayback: playback => playback.TrackProgram = false);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void DontTrackProgram_NoProgramChanges_MoveToTime(
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
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap.Default),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(moveFromMs, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new StartEvent(), lastEventTime - (moveTo - moveFrom)),
                },
                setupPlayback: playback => playback.TrackProgram = false);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void DontTrackProgram_ProgramChangeAtZero_MoveToTime()
        {
            var lastEventTime = TimeSpan.FromSeconds(1);
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(100);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ProgramChangeEvent(programNumber)),
                    new TimedEvent(new StartEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap.Default),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new ProgramChangeEvent(programNumber), TimeSpan.Zero),
                    new ReceivedEvent(new StartEvent(), lastEventTime - (moveTo - moveFrom)),
                },
                setupPlayback: playback => playback.TrackProgram = false);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void DontTrackProgram_ProgramChangeAtZero_MoveToStart()
        {
            var lastEventTime = TimeSpan.FromSeconds(1);
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.Zero;

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ProgramChangeEvent(programNumber)),
                    new TimedEvent(new StartEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap.Default),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new ProgramChangeEvent(programNumber), TimeSpan.Zero),
                    new ReceivedEvent(new ProgramChangeEvent(programNumber), moveFrom),
                    new ReceivedEvent(new StartEvent(), lastEventTime - (moveTo - moveFrom)),
                },
                setupPlayback: playback => playback.TrackProgram = false);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void DontTrackProgram_FromBeforeProgramChange_ToBeforeProgramChange()
        {
            var programChangeTime = TimeSpan.FromMilliseconds(800);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(100);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)programChangeTime, TempoMap.Default),
                    new TimedEvent(new StartEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap.Default),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, programChangeTime - (moveTo - moveFrom)),
                    new ReceivedEvent(new StartEvent(), lastEventTime - (moveTo - moveFrom)),
                },
                setupPlayback: playback => playback.TrackProgram = false);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void DontTrackProgram_FromBeforeProgramChange_ToAfterProgramChange()
        {
            var programChangeTime = TimeSpan.FromMilliseconds(500);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(300);
            var moveTo = TimeSpan.FromMilliseconds(700);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)programChangeTime, TempoMap.Default),
                    new TimedEvent(new StartEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap.Default),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new StartEvent(), lastEventTime - (moveTo - moveFrom)),
                },
                setupPlayback: playback => playback.TrackProgram = false);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void DontTrackProgram_FromAfterProgramChange_ToAfterProgramChange()
        {
            var programChangeTime = TimeSpan.FromMilliseconds(500);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(700);
            var moveTo = TimeSpan.FromMilliseconds(900);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)programChangeTime, TempoMap.Default),
                    new TimedEvent(new StartEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap.Default),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, programChangeTime),
                    new ReceivedEvent(new StartEvent(), lastEventTime - (moveTo - moveFrom)),
                },
                setupPlayback: playback => playback.TrackProgram = false);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void DontTrackProgram_FromAfterProgramChange_ToBeforeProgramChange()
        {
            var programChangeTime = TimeSpan.FromMilliseconds(500);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(700);
            var moveTo = TimeSpan.FromMilliseconds(300);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)programChangeTime, TempoMap.Default),
                    new TimedEvent(new StartEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap.Default),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, programChangeTime),
                    new ReceivedEvent(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, programChangeTime + moveFrom - moveTo),
                    new ReceivedEvent(new StartEvent(), moveFrom - moveTo + lastEventTime),
                },
                setupPlayback: playback => playback.TrackProgram = false);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void DontTrackProgram_FromBeforeProgramChange_ToProgramChange()
        {
            var programChangeTime = TimeSpan.FromMilliseconds(800);
            var lastEventTime = TimeSpan.FromSeconds(1);
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(800);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)programChangeTime, TempoMap.Default),
                    new TimedEvent(new StartEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap.Default),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, moveFrom),
                    new ReceivedEvent(new StartEvent(), lastEventTime - (moveTo - moveFrom)),
                },
                setupPlayback: playback => playback.TrackProgram = false);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void DontTrackProgram_FromAfterProgramChange_ToProgramChange()
        {
            var programChangeTime = TimeSpan.FromMilliseconds(800);
            var lastEventTime = TimeSpan.FromSeconds(2);
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(800);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)programChangeTime, TempoMap.Default),
                    new TimedEvent(new StartEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap.Default),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, programChangeTime),
                    new ReceivedEvent(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, moveFrom),
                    new ReceivedEvent(new StartEvent(), moveFrom + lastEventTime - programChangeTime),
                },
                setupPlayback: playback => playback.TrackProgram = false);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackProgram_DisableInMiddle_FromBeforeProgramChange_ToAfterProgramChange()
        {
            var programChangeTime = TimeSpan.FromMilliseconds(800);
            var pitchBendTime = TimeSpan.FromSeconds(1);
            var lastEventTime = TimeSpan.FromSeconds(2);
            var programNumber = (SevenBitNumber)100;
            var pitchValue = (ushort)1000;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(1200);
            var disableAfter = TimeSpan.FromMilliseconds(500);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)programChangeTime, TempoMap.Default),
                    new TimedEvent(new PitchBendEvent(pitchValue))
                        .SetTime((MetricTimeSpan)pitchBendTime, TempoMap.Default),
                    new TimedEvent(new StartEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap.Default),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                    new PlaybackChangerBase(disableAfter, p => p.TrackProgram = false),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, moveFrom),
                    new ReceivedEvent(new StartEvent(), moveFrom + lastEventTime - moveTo),
                },
                setupPlayback: playback => playback.TrackPitchValue = false);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackProgram_DisableInMiddle_FromAfterProgramChange_ToBeforeProgramChange()
        {
            var programChangeTime = TimeSpan.FromMilliseconds(800);
            var lastEventTime = TimeSpan.FromSeconds(2);
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(500);
            var disableAfter = TimeSpan.FromMilliseconds(150);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 })
                        .SetTime((MetricTimeSpan)programChangeTime, TempoMap.Default),
                    new TimedEvent(new StartEvent())
                        .SetTime((MetricTimeSpan)lastEventTime, TempoMap.Default),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                    new PlaybackChangerBase(disableAfter, p => p.TrackProgram = false),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, programChangeTime),
                    new ReceivedEvent(new ProgramChangeEvent(SevenBitNumber.MinValue) { Channel = (FourBitNumber)4 }, moveFrom),
                    new ReceivedEvent(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, moveFrom + programChangeTime - moveTo),
                    new ReceivedEvent(new StartEvent(), moveFrom - moveTo + lastEventTime),
                });
        }

        #endregion
    }
}
