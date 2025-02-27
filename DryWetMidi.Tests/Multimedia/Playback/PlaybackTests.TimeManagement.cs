using System;
using System.Diagnostics;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Common;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed partial class PlaybackTests
    {
        #region Test methods

        [Retry(RetriesNumber)]
        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(0.5)]
        public void MoveToStart(double speed)
        {
            var stopAfter = TimeSpan.FromMilliseconds(500);
            var stopPeriod = TimeSpan.FromSeconds(1);

            var firstEventTime = TimeSpan.Zero;
            var lastEventTime = TimeSpan.FromSeconds(5);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(400);
            var secondAfterResumeDelay = TimeSpan.FromSeconds(1);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(500);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)firstEventTime, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)(firstEventTime + lastEventTime), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(stopAfter, p =>
                    {
                        p.Stop();
                        p.MoveToStart();
                    }),
                    new PlaybackChangerBase(stopPeriod, p =>
                    {
                        p.Start();
                        CheckCurrentTime(p, TimeSpan.Zero, "stopped");
                    }),
                    new PlaybackChangerBase(firstAfterResumeDelay, p =>
                    {
                        CheckCurrentTime(p, ScaleTimeSpan(firstAfterResumeDelay, speed), "resumed");
                    }),
                    new PlaybackChangerBase(secondAfterResumeDelay, p =>
                    {
                        p.MoveToStart();
                        CheckCurrentTime(p, TimeSpan.Zero, "resumed");
                    }),
                    new PlaybackChangerBase(thirdAfterResumeDelay, p =>
                    {
                        CheckCurrentTime(p, ScaleTimeSpan(thirdAfterResumeDelay, speed), "resumed");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), firstEventTime),
                    new ReceivedEvent(new NoteOnEvent(), stopAfter + stopPeriod),
                    new ReceivedEvent(new NoteOnEvent(), stopAfter + stopPeriod + firstAfterResumeDelay + secondAfterResumeDelay),
                    new ReceivedEvent(new NoteOffEvent(), stopAfter + stopPeriod + firstAfterResumeDelay + secondAfterResumeDelay + ScaleTimeSpan(lastEventTime, 1.0 / speed)),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = false;
                    playback.InterruptNotesOnStop = false;
                    playback.Speed = speed;
                });
        }

        [Retry(RetriesNumber)]
        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(0.5)]
        public void MoveForward(double speed)
        {
            var stopAfter = TimeSpan.FromSeconds(1);
            var stopPeriod = TimeSpan.FromMilliseconds(500);

            var stepAfterStop = TimeSpan.FromSeconds(1);
            var stepAfterResumed = TimeSpan.FromMilliseconds(500);

            var firstEventTime = TimeSpan.Zero;
            var lastEventTime = TimeSpan.FromSeconds(8);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(600);
            var secondAfterResumeDelay = TimeSpan.FromSeconds(1);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(500);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)firstEventTime, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)(firstEventTime + lastEventTime), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(stopAfter, p =>
                    {
                        p.Stop();
                        p.MoveForward((MetricTimeSpan)stepAfterStop);
                    }),
                    new PlaybackChangerBase(stopPeriod, p =>
                    {
                        p.Start();
                        CheckCurrentTime(p, ScaleTimeSpan(stopAfter, speed) + stepAfterStop, "stopped");
                    }),
                    new PlaybackChangerBase(firstAfterResumeDelay, p =>
                    {
                        CheckCurrentTime(p, ScaleTimeSpan(stopAfter + firstAfterResumeDelay, speed) + stepAfterStop, "resumed");
                    }),
                    new PlaybackChangerBase(secondAfterResumeDelay, p =>
                    {
                        p.MoveForward((MetricTimeSpan)stepAfterResumed);
                        CheckCurrentTime(p, ScaleTimeSpan(stopAfter + firstAfterResumeDelay + secondAfterResumeDelay, speed) + stepAfterStop + stepAfterResumed, "resumed");
                    }),
                    new PlaybackChangerBase(thirdAfterResumeDelay, p =>
                    {
                        CheckCurrentTime(p, ScaleTimeSpan(stopAfter + firstAfterResumeDelay + secondAfterResumeDelay + thirdAfterResumeDelay, speed) + stepAfterStop + stepAfterResumed, "resumed");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), firstEventTime),
                    new ReceivedEvent(new NoteOffEvent(), ScaleTimeSpan(lastEventTime - stepAfterStop - stepAfterResumed, 1.0 / speed) + stopPeriod),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = false;
                    playback.InterruptNotesOnStop = false;
                    playback.Speed = speed;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void MoveForward_Repeated()
        {
            var tempoMap = TempoMap;
            var objects = Enumerable
                .Range(0, 5000)
                .Select(i => new Note((SevenBitNumber)70)
                    .SetTime(new MetricTimeSpan(TimeSpan.FromSeconds(i * 3)), tempoMap)
                    .SetLength(new MetricTimeSpan(TimeSpan.FromSeconds(3)), tempoMap));

            using (var playback = new Playback(objects, tempoMap))
            {
                var duration = playback.GetDuration<MetricTimeSpan>();
                var stepsCount = (int)Math.Round(duration.TotalSeconds / 2);
                var step = new MetricTimeSpan(0, 0, 2);

                playback.TrackNotes = true;
                playback.Start();

                for (var i = 0; i < stepsCount; i++)
                {
                    playback.MoveForward(step);
                }

                WaitOperations.Wait(() => !playback.IsRunning);
            }
        }

        [Retry(RetriesNumber)]
        [Test]
        public void MoveForward_BeyondDuration()
        {
            var stopAfter = TimeSpan.FromSeconds(2);
            var stopPeriod = TimeSpan.FromSeconds(2);

            var stepAfterStop = TimeSpan.FromSeconds(10);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)TimeSpan.FromSeconds(4), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(stopAfter, p =>
                    {
                        p.Stop();
                        p.MoveForward((MetricTimeSpan)stepAfterStop);
                    }),
                    new PlaybackChangerBase(stopPeriod, p =>
                    {
                        p.Start();
                        CheckCurrentTime(p, TimeSpan.FromSeconds(4), "stopped");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(), stopAfter + stopPeriod),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = false;
                    playback.InterruptNotesOnStop = false;
                });
        }

        [Retry(RetriesNumber)]
        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(0.5)]
        public void MoveBack(double speed)
        {
            var stopAfter = TimeSpan.FromSeconds(1);
            var stopPeriod = TimeSpan.FromMilliseconds(500);

            var stepAfterStop = TimeSpan.FromMilliseconds(300);
            Debug.Assert(stepAfterStop <= ScaleTimeSpan(stopAfter, speed), "Step after stop is invalid.");

            var stepAfterResumed = TimeSpan.FromMilliseconds(500);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(500);
            var secondAfterResumeDelay = TimeSpan.FromSeconds(1);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(500);

            var lastEventTime = TimeSpan.FromMilliseconds(5500);
            Debug.Assert(lastEventTime >= ScaleTimeSpan(stopAfter + firstAfterResumeDelay + secondAfterResumeDelay + thirdAfterResumeDelay, speed) - stepAfterStop - stepAfterResumed, "Last event time is invalid.");

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(stopAfter, p =>
                    {
                        p.Stop();
                        p.MoveBack((MetricTimeSpan)stepAfterStop);
                    }),
                    new PlaybackChangerBase(stopPeriod, p =>
                    {
                        p.Start();
                        CheckCurrentTime(p, ScaleTimeSpan(stopAfter, speed) - stepAfterStop, "stopped");
                    }),
                    new PlaybackChangerBase(firstAfterResumeDelay, p =>
                    {
                        CheckCurrentTime(p, ScaleTimeSpan(stopAfter + firstAfterResumeDelay, speed) - stepAfterStop, "resumed");
                    }),
                    new PlaybackChangerBase(secondAfterResumeDelay, p =>
                    {
                        p.MoveBack((MetricTimeSpan)stepAfterResumed);
                        CheckCurrentTime(p, ScaleTimeSpan(stopAfter + firstAfterResumeDelay + secondAfterResumeDelay, speed) - stepAfterStop - stepAfterResumed, "resumed");
                    }),
                    new PlaybackChangerBase(thirdAfterResumeDelay, p =>
                    {
                        CheckCurrentTime(p, ScaleTimeSpan(stopAfter + firstAfterResumeDelay + secondAfterResumeDelay + thirdAfterResumeDelay, speed) - stepAfterStop - stepAfterResumed, "resumed");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(), ScaleTimeSpan(lastEventTime + stepAfterStop + stepAfterResumed, 1.0 / speed) + stopPeriod),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = false;
                    playback.InterruptNotesOnStop = false;
                    playback.Speed = speed;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void MoveBack_BeyondZero()
        {
            var stopAfter = TimeSpan.FromSeconds(3);
            var stopPeriod = TimeSpan.FromSeconds(2);

            var stepAfterStop = TimeSpan.FromSeconds(5);
            var stepAfterResumed = TimeSpan.FromMilliseconds(500);

            var lastEventTime = TimeSpan.FromSeconds(5);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(500);
            var secondAfterResumeDelay = TimeSpan.FromSeconds(1);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(500);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(stopAfter, p =>
                    {
                        p.Stop();
                        p.MoveBack((MetricTimeSpan)stepAfterStop);
                    }),
                    new PlaybackChangerBase(stopPeriod, p =>
                    {
                        p.Start();
                        CheckCurrentTime(p, TimeSpan.Zero, "stopped");
                    }),
                    new PlaybackChangerBase(firstAfterResumeDelay, p =>
                    {
                        CheckCurrentTime(p, firstAfterResumeDelay, "resumed");
                    }),
                    new PlaybackChangerBase(secondAfterResumeDelay, p =>
                    {
                        p.MoveBack((MetricTimeSpan)stepAfterResumed);
                        CheckCurrentTime(p, firstAfterResumeDelay + secondAfterResumeDelay - stepAfterResumed, "resumed");
                    }),
                    new PlaybackChangerBase(thirdAfterResumeDelay, p =>
                    {
                        CheckCurrentTime(p, firstAfterResumeDelay + secondAfterResumeDelay + thirdAfterResumeDelay - stepAfterResumed, "resumed");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOnEvent(), stopAfter + stopPeriod),
                    new ReceivedEvent(new NoteOffEvent(), lastEventTime + stopAfter + stopPeriod + stepAfterResumed),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = false;
                    playback.InterruptNotesOnStop = false;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void MoveToTime()
        {
            var stopAfter = TimeSpan.FromSeconds(4);
            var stopPeriod = TimeSpan.FromSeconds(2);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)TimeSpan.FromSeconds(10), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(stopAfter, p =>
                    {
                        p.Stop();
                        p.MoveToTime(new MetricTimeSpan(0, 0, 1));
                    }),
                    new PlaybackChangerBase(stopPeriod, p =>
                    {
                        p.Start();
                        CheckCurrentTime(p, TimeSpan.FromSeconds(1), "stopped");
                    }),
                    new PlaybackChangerBase(TimeSpan.FromSeconds(1), p =>
                    {
                        CheckCurrentTime(p, TimeSpan.FromSeconds(2), "resumed");
                    }),
                    new PlaybackChangerBase(TimeSpan.FromSeconds(2), p =>
                    {
                        p.MoveToTime(new MetricTimeSpan(0, 0, 8));
                        CheckCurrentTime(p, TimeSpan.FromSeconds(8), "resumed");
                    }),
                    new PlaybackChangerBase(TimeSpan.FromSeconds(1), p =>
                    {
                        CheckCurrentTime(p, TimeSpan.FromSeconds(9), "resumed");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromSeconds(11)),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = false;
                    playback.InterruptNotesOnStop = false;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void MoveToTime_BeyondDuration()
        {
            var stopAfter = TimeSpan.FromSeconds(2);
            var stopPeriod = TimeSpan.FromSeconds(2);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)TimeSpan.FromSeconds(4), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(stopAfter, p =>
                    {
                        p.Stop();
                        p.MoveToTime(new MetricTimeSpan(0, 0, 10));
                    }),
                    new PlaybackChangerBase(stopPeriod, p =>
                    {
                        p.Start();
                        CheckCurrentTime(p, TimeSpan.FromSeconds(4), "stopped");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(), stopAfter + stopPeriod),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = false;
                    playback.InterruptNotesOnStop = false;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void MoveToTime_BeforeFirstStart()
        {
            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(300), TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(800), TempoMap),
                },
                actions: Array.Empty<PlaybackChangerBase>(),
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(600)),
                },
                setupPlayback: playback =>
                    playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 200)));
        }

        #endregion
    }
}
