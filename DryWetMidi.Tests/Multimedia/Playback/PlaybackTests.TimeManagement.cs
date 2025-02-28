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
            var stopAfter = TimeSpan.FromMilliseconds(200);
            var stopPeriod = TimeSpan.FromMilliseconds(700);

            var firstEventTime = TimeSpan.Zero;
            var lastEventTime = TimeSpan.FromSeconds(2);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(200);
            var secondAfterResumeDelay = TimeSpan.FromMilliseconds(500);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(100);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)firstEventTime, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)(firstEventTime + lastEventTime), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(stopAfter, p =>
                    {
                        p.Stop();
                        p.MoveToStart();
                    }),
                    new PlaybackAction(stopPeriod, p =>
                    {
                        p.Start();
                        CheckCurrentTime(p, TimeSpan.Zero, "stopped");
                    }),
                    new PlaybackAction(firstAfterResumeDelay, p =>
                    {
                        CheckCurrentTime(p, ScaleTimeSpan(firstAfterResumeDelay, speed), "resumed");
                    }),
                    new PlaybackAction(secondAfterResumeDelay, p =>
                    {
                        p.MoveToStart();
                        CheckCurrentTime(p, TimeSpan.Zero, "resumed");
                    }),
                    new PlaybackAction(thirdAfterResumeDelay, p =>
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
            var stopAfter = TimeSpan.FromMilliseconds(200);
            var stopPeriod = TimeSpan.FromMilliseconds(300);

            var stepAfterStop = TimeSpan.FromMilliseconds(400);
            var stepAfterResumed = TimeSpan.FromMilliseconds(300);

            var firstEventTime = TimeSpan.Zero;
            var lastEventTime = TimeSpan.FromSeconds(4);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(300);
            var secondAfterResumeDelay = TimeSpan.FromMilliseconds(500);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(200);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)firstEventTime, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)(firstEventTime + lastEventTime), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(stopAfter, p =>
                    {
                        p.Stop();
                        p.MoveForward((MetricTimeSpan)stepAfterStop);
                    }),
                    new PlaybackAction(stopPeriod, p =>
                    {
                        p.Start();
                        CheckCurrentTime(p, ScaleTimeSpan(stopAfter, speed) + stepAfterStop, "stopped");
                    }),
                    new PlaybackAction(firstAfterResumeDelay, p =>
                    {
                        CheckCurrentTime(p, ScaleTimeSpan(stopAfter + firstAfterResumeDelay, speed) + stepAfterStop, "resumed");
                    }),
                    new PlaybackAction(secondAfterResumeDelay, p =>
                    {
                        p.MoveForward((MetricTimeSpan)stepAfterResumed);
                        CheckCurrentTime(p, ScaleTimeSpan(stopAfter + firstAfterResumeDelay + secondAfterResumeDelay, speed) + stepAfterStop + stepAfterResumed, "resumed");
                    }),
                    new PlaybackAction(thirdAfterResumeDelay, p =>
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
                    new PlaybackAction(stopAfter, p =>
                    {
                        p.Stop();
                        p.MoveForward((MetricTimeSpan)stepAfterStop);
                    }),
                    new PlaybackAction(stopPeriod, p =>
                    {
                        p.Start();
                        CheckCurrentTime(p, TimeSpan.FromSeconds(4), "stopped");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(), stopAfter),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = false;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void MoveForward_BeyondPlaybackEnd()
        {
            var stopAfter = TimeSpan.FromMilliseconds(200);
            var stopPeriod = TimeSpan.FromMilliseconds(200);

            var stepAfterStop = TimeSpan.FromMilliseconds(300);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(400), TempoMap),
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(600), TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(700), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(stopAfter, p =>
                    {
                        p.Stop();
                        p.MoveForward((MetricTimeSpan)stepAfterStop);
                    }),
                    new PlaybackAction(stopPeriod, p =>
                    {
                        p.Start();
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(450), "stopped");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(), stopAfter),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = false;
                    playback.PlaybackEnd = new MetricTimeSpan(0, 0, 0, 450);
                });
        }

        [Retry(RetriesNumber)]
        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(0.5)]
        public void MoveBack(double speed)
        {
            var stopAfter = TimeSpan.FromMilliseconds(700);
            var stopPeriod = TimeSpan.FromMilliseconds(200);

            var stepAfterStop = TimeSpan.FromMilliseconds(300);
            Assert.LessOrEqual(stepAfterStop, ScaleTimeSpan(stopAfter, speed), "Step after stop is invalid.");

            var stepAfterResumed = TimeSpan.FromMilliseconds(100);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(200);
            var secondAfterResumeDelay = TimeSpan.FromMilliseconds(500);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(200);

            var lastEventTime = TimeSpan.FromMilliseconds(3000);
            Assert.GreaterOrEqual(lastEventTime, ScaleTimeSpan(stopAfter + firstAfterResumeDelay + secondAfterResumeDelay + thirdAfterResumeDelay, speed) - stepAfterStop - stepAfterResumed, "Last event time is invalid.");

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(stopAfter, p =>
                    {
                        p.Stop();
                        p.MoveBack((MetricTimeSpan)stepAfterStop);
                    }),
                    new PlaybackAction(stopPeriod, p =>
                    {
                        p.Start();
                        CheckCurrentTime(p, ScaleTimeSpan(stopAfter, speed) - stepAfterStop, "stopped");
                    }),
                    new PlaybackAction(firstAfterResumeDelay, p =>
                    {
                        CheckCurrentTime(p, ScaleTimeSpan(stopAfter + firstAfterResumeDelay, speed) - stepAfterStop, "resumed");
                    }),
                    new PlaybackAction(secondAfterResumeDelay, p =>
                    {
                        p.MoveBack((MetricTimeSpan)stepAfterResumed);
                        CheckCurrentTime(p, ScaleTimeSpan(stopAfter + firstAfterResumeDelay + secondAfterResumeDelay, speed) - stepAfterStop - stepAfterResumed, "resumed");
                    }),
                    new PlaybackAction(thirdAfterResumeDelay, p =>
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
            var stopAfter = TimeSpan.FromMilliseconds(500);
            var stopPeriod = TimeSpan.FromMilliseconds(600);

            var stepAfterStop = TimeSpan.FromMilliseconds(2000);
            var stepAfterResumed = TimeSpan.FromMilliseconds(500);

            var lastEventTime = TimeSpan.FromSeconds(2);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(200);
            var secondAfterResumeDelay = TimeSpan.FromMilliseconds(400);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(300);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(stopAfter, p =>
                    {
                        p.Stop();
                        p.MoveBack((MetricTimeSpan)stepAfterStop);
                    }),
                    new PlaybackAction(stopPeriod, p =>
                    {
                        p.Start();
                        CheckCurrentTime(p, TimeSpan.Zero, "stopped");
                    }),
                    new PlaybackAction(firstAfterResumeDelay, p =>
                    {
                        CheckCurrentTime(p, firstAfterResumeDelay, "resumed");
                    }),
                    new PlaybackAction(secondAfterResumeDelay, p =>
                    {
                        p.MoveBack((MetricTimeSpan)stepAfterResumed);
                        CheckCurrentTime(p, firstAfterResumeDelay + secondAfterResumeDelay - stepAfterResumed, "resumed");
                    }),
                    new PlaybackAction(thirdAfterResumeDelay, p =>
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
        public void MoveBack_BeyondPlaybackStart_1()
        {
            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(0), TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(200), TempoMap),
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(400), TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(600), TempoMap),
                },
                actions: new PlaybackAction[]
                {
                    new PlaybackAction(200, p =>
                    {
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(500), "A");
                        p.MoveBack(new MetricTimeSpan(0, 0, 0, 400));
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(300), "B");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(500))
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;
                    playback.TrackNotes = false;
                    playback.PlaybackStart = new MetricTimeSpan(0, 0, 0, 300);
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void MoveBack_BeyondPlaybackStart_2()
        {
            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(0), TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(200), TempoMap),
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(400), TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(600), TempoMap),
                },
                actions: new PlaybackAction[]
                {
                    new PlaybackAction(200, p =>
                    {
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(500), "A");
                        p.MoveBack(new MetricTimeSpan(0, 0, 2, 400));
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(300), "B");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(500))
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;
                    playback.TrackNotes = false;
                    playback.PlaybackStart = new MetricTimeSpan(0, 0, 0, 300);
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void MoveToTime()
        {
            var lastEventTime = TimeSpan.FromMilliseconds(2000);

            var stopAfter = TimeSpan.FromMilliseconds(800);
            var stopPeriod = TimeSpan.FromMilliseconds(400);

            var moveTime1 = TimeSpan.FromMilliseconds(200);
            var moveTime2 = TimeSpan.FromMilliseconds(1600);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(200);
            var secondAfterResumeDelay = TimeSpan.FromMilliseconds(400);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(200);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new PlaybackAction[]
                {
                    new PlaybackAction(stopAfter, p =>
                    {
                        p.Stop();
                        p.MoveToTime((MetricTimeSpan)moveTime1);
                        CheckCurrentTime(p, moveTime1, "first move");
                    }),
                    new PlaybackAction(stopPeriod, p =>
                    {
                        p.Start();
                        CheckCurrentTime(p, moveTime1, "stopped");
                    }),
                    new PlaybackAction(firstAfterResumeDelay,
                        p => CheckCurrentTime(p, moveTime1 + firstAfterResumeDelay, "resumed")),
                    new PlaybackAction(secondAfterResumeDelay,
                        p =>
                        {
                            CheckCurrentTime(p, moveTime1 + firstAfterResumeDelay + secondAfterResumeDelay, "resumed");
                            p.MoveToTime((MetricTimeSpan)moveTime2);
                            CheckCurrentTime(p, moveTime2, "resumed");
                        }),
                    new PlaybackAction(thirdAfterResumeDelay,
                        p => CheckCurrentTime(p, moveTime2 + thirdAfterResumeDelay, "resumed"))
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(), stopAfter + stopPeriod + firstAfterResumeDelay + secondAfterResumeDelay + lastEventTime - moveTime2)
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;
                    playback.TrackNotes = false;
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
                    new PlaybackAction(stopAfter, p =>
                    {
                        p.Stop();
                        p.MoveToTime(new MetricTimeSpan(0, 0, 10));
                    }),
                    new PlaybackAction(stopPeriod, p =>
                    {
                        p.Start();
                        CheckCurrentTime(p, TimeSpan.FromSeconds(4), "stopped");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(), stopAfter),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = false;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void MoveToTime_BeyondPlaybackEnd()
        {
            var stopAfter = TimeSpan.FromMilliseconds(200);
            var stopPeriod = TimeSpan.FromMilliseconds(200);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(400), TempoMap),
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(600), TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(700), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(stopAfter, p =>
                    {
                        p.Stop();
                        p.MoveToTime(new MetricTimeSpan(0, 0, 0, 500));
                    }),
                    new PlaybackAction(stopPeriod, p =>
                    {
                        p.Start();
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(450), "stopped");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(), stopAfter),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = false;
                    playback.PlaybackEnd = new MetricTimeSpan(0, 0, 0, 450);
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
                actions: Array.Empty<PlaybackAction>(),
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
