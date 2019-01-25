using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Devices
{
    [TestFixture]
    public sealed class PlaybackTests
    {
        #region Nested classes

        private sealed class PlaybackContext
        {
            public List<ReceivedEvent> ReceivedEvents { get; } = new List<ReceivedEvent>();

            public List<SentEvent> SentEvents { get; } = new List<SentEvent>();

            public object ReceivedEventsLockObject { get; } = new object();

            public Stopwatch Stopwatch { get; } = new Stopwatch();

            public TempoMap TempoMap { get; } = TempoMap.Default;

            public List<TimeSpan> ExpectedTimes { get; } = new List<TimeSpan>();
        }

        #endregion

        #region Delegates

        private delegate void PlaybackAction(PlaybackContext context, Playback playback);

        #endregion

        #region Constants

        private static readonly PlaybackAction NoPlaybackAction = (context, playback) => { };

        private static readonly object[] ParametersForDurationCheck = new[]
        {
            new object[] { TimeSpan.Zero, TimeSpan.FromSeconds(2) },
            new object[] { TimeSpan.FromMilliseconds(500), TimeSpan.FromSeconds(3) },
            new object[] { TimeSpan.Zero, TimeSpan.FromSeconds(10) },
            new object[] { TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(12) }
        };

        #endregion

        #region Test methods

        [Test]
        public void CheckPlaybackEvents_Normal()
        {
            CheckPlaybackEvents(
                expectedStartedRaised: 2,
                expectedStoppedRaised: 1,
                expectedFinishedRaised: 1,
                setupPlayback: NoPlaybackAction,
                beforeChecks: NoPlaybackAction,
                afterChecks: (context, playback) =>
                {
                    Assert.IsFalse(playback.IsRunning, "Playback is running after it should be finished.");
                });
        }

        [Test]
        public void CheckPlaybackEvents_Looped()
        {
            CheckPlaybackEvents(
                expectedStartedRaised: 2,
                expectedStoppedRaised: 1,
                expectedFinishedRaised: 0,
                setupPlayback: (context, playback) => playback.Loop = true,
                beforeChecks: (context, playback) => Thread.Sleep(TimeSpan.FromSeconds(10)),
                afterChecks: (context, playback) =>
                {
                    Assert.IsTrue(playback.IsRunning, "Playback is not running after waiting.");
                });
        }

        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(0.5)]
        public void CheckPlayback_NonBlocking(double speed)
        {
            var eventsToSend = new[]
            {
                new EventToSend(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)20) { Channel = (FourBitNumber)5 }, TimeSpan.Zero),
                new EventToSend(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)10) { Channel = (FourBitNumber)5 }, TimeSpan.FromSeconds(2)),
                new EventToSend(new NoteOnEvent(), TimeSpan.FromSeconds(1)),
                new EventToSend(new NoteOnEvent((SevenBitNumber)30, (SevenBitNumber)50), TimeSpan.Zero),
                new EventToSend(new NoteOffEvent(), TimeSpan.FromSeconds(3)),
                new EventToSend(new NoteOffEvent((SevenBitNumber)30, (SevenBitNumber)50), TimeSpan.Zero)
            };

            CheckPlayback(
                eventsToSend,
                speed,
                beforePlaybackStarted: NoPlaybackAction,
                startPlayback: (context, playback) => playback.Start(),
                afterPlaybackStarted: (context, playback) =>
                {
                    Assert.LessOrEqual(context.Stopwatch.Elapsed, SendReceiveUtilities.MaximumEventSendReceiveDelay, "Playback blocks current thread.");
                    Assert.IsTrue(playback.IsRunning, "Playback is not running after start.");
                },
                waiting: (context, playback) =>
                {
                    var timeout = context.ExpectedTimes.Last() + SendReceiveUtilities.MaximumEventSendReceiveDelay;
                    var areEventsReceived = SpinWait.SpinUntil(() => context.ReceivedEvents.Count == eventsToSend.Length, timeout);
                    Assert.IsTrue(areEventsReceived, $"Events are not received for timeout {timeout}.");
                },
                finalChecks: (context, playback) =>
                {
                    var playbackStopped = SpinWait.SpinUntil(() => !playback.IsRunning, SendReceiveUtilities.MaximumEventSendReceiveDelay);
                    Assert.IsTrue(playbackStopped, "Playback is running after completed.");
                });
        }

        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(0.5)]
        public void CheckPlayback_Blocking(double speed)
        {
            var eventsToSend = new[]
            {
                new EventToSend(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)20) { Channel = (FourBitNumber)5 }, TimeSpan.Zero),
                new EventToSend(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)10) { Channel = (FourBitNumber)5 }, TimeSpan.FromSeconds(2)),
                new EventToSend(new NoteOnEvent(), TimeSpan.FromSeconds(1)),
                new EventToSend(new NoteOnEvent((SevenBitNumber)30, (SevenBitNumber)50), TimeSpan.Zero),
                new EventToSend(new NoteOffEvent(), TimeSpan.FromSeconds(3)),
                new EventToSend(new NoteOffEvent((SevenBitNumber)30, (SevenBitNumber)50), TimeSpan.Zero)
            };

            CheckPlayback(
                eventsToSend,
                speed,
                beforePlaybackStarted: NoPlaybackAction,
                startPlayback: (context, playback) => playback.Play(),
                afterPlaybackStarted: (context, playback) =>
                {
                    Assert.GreaterOrEqual(context.Stopwatch.Elapsed, context.ExpectedTimes.Last(), "Playback doesn't block current thread.");
                },
                waiting: (context, playback) =>
                {
                    var areEventsReceived = SpinWait.SpinUntil(() => context.ReceivedEvents.Count == eventsToSend.Length, SendReceiveUtilities.MaximumEventSendReceiveDelay);
                    Assert.IsTrue(areEventsReceived, $"Events are not received.");
                },
                finalChecks: NoPlaybackAction);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(10)]
        public void CheckPlaybackLooping(int repetitionsNumber)
        {
            var eventsToSend = new[]
            {
                new EventToSend(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)20) { Channel = (FourBitNumber)5 }, TimeSpan.Zero),
                new EventToSend(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)10) { Channel = (FourBitNumber)5 }, TimeSpan.FromSeconds(2)),
                new EventToSend(new NoteOnEvent(), TimeSpan.FromSeconds(1)),
                new EventToSend(new NoteOnEvent((SevenBitNumber)30, (SevenBitNumber)50), TimeSpan.Zero),
                new EventToSend(new NoteOffEvent(), TimeSpan.FromSeconds(3)),
                new EventToSend(new NoteOffEvent((SevenBitNumber)30, (SevenBitNumber)50), TimeSpan.Zero)
            };

            CheckPlayback(
                eventsToSend,
                speed: 1.0,
                beforePlaybackStarted: (context, playback) =>
                {
                    var originalExpectedTimes = context.ExpectedTimes.ToList();

                    for (int i = 1; i < repetitionsNumber; i++)
                    {
                        var lastTime = context.ExpectedTimes.Last();
                        context.ExpectedTimes.AddRange(originalExpectedTimes.Select(t => lastTime + t));
                    }

                    playback.Loop = true;
                },
                startPlayback: (context, playback) => playback.Start(),
                afterPlaybackStarted: (context, playback) =>
                {
                    Assert.LessOrEqual(context.Stopwatch.Elapsed, SendReceiveUtilities.MaximumEventSendReceiveDelay, "Playback blocks current thread.");
                    Assert.IsTrue(playback.IsRunning, "Playback is not running after start.");
                },
                waiting: (context, playback) =>
                {
                    var timeout = context.ExpectedTimes.Last() + SendReceiveUtilities.MaximumEventSendReceiveDelay;
                    var areEventsReceived = SpinWait.SpinUntil(() => context.ReceivedEvents.Count >= eventsToSend.Length * repetitionsNumber, timeout);
                    Assert.IsTrue(areEventsReceived, $"Events are not received for timeout {timeout}.");
                },
                finalChecks: (context, playback) =>
                {
                    Assert.IsTrue(playback.IsRunning, "Playback is not running.");
                    playback.Stop();
                    Assert.IsFalse(playback.IsRunning, "Playback is running after stop.");

                    lock (context.ReceivedEventsLockObject)
                    {
                        var groupedReceivedEvents = context.ReceivedEvents.GroupBy(e => e.Event, new MidiEventEquality.EqualityComparer(false)).Take(eventsToSend.Length).ToArray();
                        Assert.IsTrue(groupedReceivedEvents.All(g => g.Count() >= repetitionsNumber), $"Events are not repeated {repetitionsNumber} times.");
                    }
                });
        }

        [Test]
        public void CheckPlaybackStop()
        {
            var eventsToSend = new[]
            {
                new EventToSend(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)20) { Channel = (FourBitNumber)5 }, TimeSpan.Zero),
                new EventToSend(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)10) { Channel = (FourBitNumber)5 }, TimeSpan.FromSeconds(2)),
                new EventToSend(new NoteOnEvent(), TimeSpan.FromSeconds(1)),
                new EventToSend(new NoteOnEvent((SevenBitNumber)30, (SevenBitNumber)50), TimeSpan.Zero),
                new EventToSend(new NoteOffEvent(), TimeSpan.FromSeconds(3)),
                new EventToSend(new NoteOffEvent((SevenBitNumber)30, (SevenBitNumber)50), TimeSpan.Zero)
            };

            CheckPlaybackStop(
                eventsToSend,
                eventsWillBeSent: eventsToSend,
                stopAfter: TimeSpan.FromMilliseconds(2500),
                stopPeriod: TimeSpan.FromSeconds(3),
                setupPlayback: NoPlaybackAction,
                afterStart: NoPlaybackAction,
                afterStop: NoPlaybackAction,
                afterResume: NoPlaybackAction);
        }

        [Test]
        public void CheckNoteStop_Interrupt()
        {
            CheckPlaybackStop(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOnEvent(), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(), TimeSpan.FromSeconds(5))
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new NoteOnEvent(), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(), TimeSpan.FromSeconds(1)),
                    new EventToSend(new NoteOffEvent(), TimeSpan.FromSeconds(4))
                },
                stopAfter: TimeSpan.FromSeconds(1),
                stopPeriod: TimeSpan.FromSeconds(2),
                setupPlayback: (context, playback) => playback.NoteStopPolicy = NoteStopPolicy.Interrupt,
                afterStart: NoPlaybackAction,
                afterStop: NoPlaybackAction,
                afterResume: NoPlaybackAction);
        }

        [Test]
        public void CheckNoteStop_Hold()
        {
            var eventsToSend = new[]
            {
                new EventToSend(new NoteOnEvent(), TimeSpan.Zero),
                new EventToSend(new NoteOffEvent(), TimeSpan.FromSeconds(5))
            };

            CheckPlaybackStop(
                eventsToSend,
                eventsWillBeSent: eventsToSend,
                stopAfter: TimeSpan.FromSeconds(1),
                stopPeriod: TimeSpan.FromSeconds(2),
                setupPlayback: (context, playback) => playback.NoteStopPolicy = NoteStopPolicy.Hold,
                afterStart: NoPlaybackAction,
                afterStop: NoPlaybackAction,
                afterResume: NoPlaybackAction);
        }

        [Test]
        public void CheckNoteStop_Split()
        {
            CheckPlaybackStop(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOnEvent(), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(), TimeSpan.FromSeconds(5))
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new NoteOnEvent(), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(), TimeSpan.FromSeconds(1)),
                    new EventToSend(new NoteOnEvent(), TimeSpan.FromTicks(1)),
                    new EventToSend(new NoteOffEvent(), TimeSpan.FromSeconds(4))
                },
                stopAfter: TimeSpan.FromSeconds(1),
                stopPeriod: TimeSpan.FromSeconds(2),
                setupPlayback: (context, playback) => playback.NoteStopPolicy = NoteStopPolicy.Split,
                afterStart: NoPlaybackAction,
                afterStop: NoPlaybackAction,
                afterResume: NoPlaybackAction);
        }

        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(0.5)]
        public void GetCurrentTime(double speed)
        {
            var eventsToSend = new[]
            {
                new EventToSend(new NoteOnEvent(), TimeSpan.Zero),
                new EventToSend(new NoteOffEvent(), TimeSpan.FromSeconds(10))
            };

            var stopAfter = TimeSpan.FromSeconds(2);
            var stopPeriod = TimeSpan.FromSeconds(2);

            CheckPlaybackStop(
                eventsToSend,
                eventsWillBeSent: eventsToSend,
                stopAfter: stopAfter,
                stopPeriod: stopPeriod,
                setupPlayback: (context, playback) => playback.Speed = speed,
                afterStart: (context, playback) => CheckCurrentTime(playback, TimeSpan.Zero, "started"),
                afterStop: (context, playback) => CheckCurrentTime(playback, ScaleTimeSpan(stopAfter, speed), "stopped"),
                afterResume: (context, playback) => CheckCurrentTime(playback, ScaleTimeSpan(stopAfter, speed), "resumed"),
                runningAfterResume: new[]
                {
                    Tuple.Create<TimeSpan, PlaybackAction>(TimeSpan.FromSeconds(1), (context, playback) => CheckCurrentTime(playback, ScaleTimeSpan(stopAfter + TimeSpan.FromSeconds(1), speed), "resumed")),
                    Tuple.Create<TimeSpan, PlaybackAction>(TimeSpan.FromSeconds(2), (context, playback) => CheckCurrentTime(playback, ScaleTimeSpan(stopAfter + TimeSpan.FromSeconds(3), speed), "resumed"))
                },
                speed: speed);
        }

        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(0.5)]
        public void MoveToStart(double speed)
        {
            var stopAfter = TimeSpan.FromSeconds(2);
            var stopPeriod = TimeSpan.FromSeconds(2);

            CheckPlaybackStop(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOnEvent(), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(), TimeSpan.FromSeconds(10))
                },
                eventsWillBeSent: new EventToSend[] { },
                stopAfter: stopAfter,
                stopPeriod: stopPeriod,
                setupPlayback: NoPlaybackAction,
                afterStart: NoPlaybackAction,
                afterStop: (context, playback) => playback.MoveToStart(),
                afterResume: (context, playback) => CheckCurrentTime(playback, TimeSpan.Zero, "stopped"),
                runningAfterResume: new[]
                {
                    Tuple.Create<TimeSpan, PlaybackAction>(TimeSpan.FromSeconds(1), (context, playback) => CheckCurrentTime(playback, ScaleTimeSpan(TimeSpan.FromSeconds(1), speed), "resumed")),
                    Tuple.Create<TimeSpan, PlaybackAction>(TimeSpan.FromSeconds(2), (context, playback) =>
                    {
                        playback.MoveToStart();
                        CheckCurrentTime(playback, TimeSpan.Zero, "resumed");
                    }),
                    Tuple.Create<TimeSpan, PlaybackAction>(TimeSpan.FromSeconds(2), (context, playback) => CheckCurrentTime(playback, ScaleTimeSpan(TimeSpan.FromSeconds(2), speed), "resumed"))
                },
                explicitExpectedTimes: new[]
                {
                    TimeSpan.Zero,
                    TimeSpan.FromSeconds(4),
                    TimeSpan.FromSeconds(7),
                    TimeSpan.FromSeconds(7) + ScaleTimeSpan(TimeSpan.FromSeconds(10), 1.0 / speed)
                },
                speed: speed);
        }

        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(0.5)]
        public void MoveForward(double speed)
        {
            var stopAfter = TimeSpan.FromSeconds(2);
            var stopPeriod = TimeSpan.FromSeconds(2);

            var stepAfterStop = TimeSpan.FromSeconds(2);
            var stepAfterResumed = TimeSpan.FromSeconds(1);

            CheckPlaybackStop(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOnEvent(), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(), TimeSpan.FromSeconds(15))
                },
                eventsWillBeSent: new EventToSend[] { },
                stopAfter: stopAfter,
                stopPeriod: stopPeriod,
                setupPlayback: NoPlaybackAction,
                afterStart: NoPlaybackAction,
                afterStop: (context, playback) => playback.MoveForward((MetricTimeSpan)stepAfterStop),
                afterResume: (context, playback) => CheckCurrentTime(playback, ScaleTimeSpan(stopAfter, speed) + stepAfterStop, "stopped"),
                runningAfterResume: new[]
                {
                    Tuple.Create<TimeSpan, PlaybackAction>(TimeSpan.FromSeconds(1), (context, playback) => CheckCurrentTime(playback, ScaleTimeSpan(stopAfter + TimeSpan.FromSeconds(1), speed) + stepAfterStop, "resumed")),
                    Tuple.Create<TimeSpan, PlaybackAction>(TimeSpan.FromSeconds(2), (context, playback) =>
                    {
                        playback.MoveForward((MetricTimeSpan)stepAfterResumed);
                        CheckCurrentTime(playback, ScaleTimeSpan(stopAfter + TimeSpan.FromSeconds(3), speed) + stepAfterStop + stepAfterResumed, "resumed");
                    }),
                    Tuple.Create<TimeSpan, PlaybackAction>(TimeSpan.FromSeconds(1), (context, playback) => CheckCurrentTime(playback, ScaleTimeSpan(stopAfter + TimeSpan.FromSeconds(4), speed) + stepAfterStop + stepAfterResumed, "resumed"))
                },
                explicitExpectedTimes: new[]
                {
                    TimeSpan.Zero,
                    ScaleTimeSpan(TimeSpan.FromSeconds(15) - stepAfterStop - stepAfterResumed, 1.0 / speed) + stopPeriod
                },
                speed: speed);
        }

        [Test]
        public void MoveForward_BeyondDuration()
        {
            var stopAfter = TimeSpan.FromSeconds(2);
            var stopPeriod = TimeSpan.FromSeconds(2);

            var stepAfterStop = TimeSpan.FromSeconds(10);

            CheckPlaybackStop(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOnEvent(), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(), TimeSpan.FromSeconds(4))
                },
                eventsWillBeSent: new EventToSend[] { },
                stopAfter: stopAfter,
                stopPeriod: stopPeriod,
                setupPlayback: NoPlaybackAction,
                afterStart: NoPlaybackAction,
                afterStop: (context, playback) => playback.MoveForward((MetricTimeSpan)stepAfterStop),
                afterResume: (context, playback) => CheckCurrentTime(playback, TimeSpan.FromSeconds(4), "stopped"),
                runningAfterResume: null,
                explicitExpectedTimes: new[]
                {
                    TimeSpan.Zero,
                    stopAfter + stopPeriod
                });
        }

        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(0.5)]
        public void MoveBack(double speed)
        {
            var stopAfter = TimeSpan.FromSeconds(3);
            var stopPeriod = TimeSpan.FromSeconds(2);

            var stepAfterStop = TimeSpan.FromSeconds(1);
            var stepAfterResumed = TimeSpan.FromMilliseconds(500);

            var lastEventTime = TimeSpan.FromSeconds(10);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(500);
            var secondAfterResumeDelay = TimeSpan.FromSeconds(1);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(500);

            CheckPlaybackStop(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOnEvent(), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(), lastEventTime)
                },
                eventsWillBeSent: new EventToSend[] { },
                stopAfter: stopAfter,
                stopPeriod: stopPeriod,
                setupPlayback: NoPlaybackAction,
                afterStart: NoPlaybackAction,
                afterStop: (context, playback) => playback.MoveBack((MetricTimeSpan)stepAfterStop),
                afterResume: (context, playback) => CheckCurrentTime(playback, ScaleTimeSpan(stopAfter, speed) - stepAfterStop, "stopped"),
                runningAfterResume: new[]
                {
                    Tuple.Create<TimeSpan, PlaybackAction>(firstAfterResumeDelay, (context, playback) => CheckCurrentTime(playback, ScaleTimeSpan(stopAfter + firstAfterResumeDelay, speed) - stepAfterStop, "resumed")),
                    Tuple.Create<TimeSpan, PlaybackAction>(secondAfterResumeDelay, (context, playback) =>
                    {
                        playback.MoveBack((MetricTimeSpan)stepAfterResumed);
                        CheckCurrentTime(playback, ScaleTimeSpan(stopAfter + firstAfterResumeDelay + secondAfterResumeDelay, speed) - stepAfterStop - stepAfterResumed, "resumed");
                    }),
                    Tuple.Create<TimeSpan, PlaybackAction>(thirdAfterResumeDelay, (context, playback) => CheckCurrentTime(playback, ScaleTimeSpan(stopAfter + firstAfterResumeDelay + secondAfterResumeDelay + thirdAfterResumeDelay, speed) - stepAfterStop - stepAfterResumed, "resumed"))
                },
                explicitExpectedTimes: new[]
                {
                    TimeSpan.Zero,
                    ScaleTimeSpan(lastEventTime + stepAfterStop + stepAfterResumed, 1.0 / speed) + stopPeriod
                },
                speed: speed);
        }

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

            CheckPlaybackStop(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOnEvent(), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(), lastEventTime)
                },
                eventsWillBeSent: new EventToSend[] { },
                stopAfter: stopAfter,
                stopPeriod: stopPeriod,
                setupPlayback: NoPlaybackAction,
                afterStart: NoPlaybackAction,
                afterStop: (context, playback) => playback.MoveBack((MetricTimeSpan)stepAfterStop),
                afterResume: (context, playback) => CheckCurrentTime(playback, TimeSpan.Zero, "stopped"),
                runningAfterResume: new[]
                {
                    Tuple.Create<TimeSpan, PlaybackAction>(firstAfterResumeDelay, (context, playback) => CheckCurrentTime(playback, firstAfterResumeDelay, "resumed")),
                    Tuple.Create<TimeSpan, PlaybackAction>(secondAfterResumeDelay, (context, playback) =>
                    {
                        playback.MoveBack((MetricTimeSpan)stepAfterResumed);
                        CheckCurrentTime(playback, firstAfterResumeDelay + secondAfterResumeDelay - stepAfterResumed, "resumed");
                    }),
                    Tuple.Create<TimeSpan, PlaybackAction>(thirdAfterResumeDelay, (context, playback) => CheckCurrentTime(playback, firstAfterResumeDelay + secondAfterResumeDelay + thirdAfterResumeDelay - stepAfterResumed, "resumed"))
                },
                explicitExpectedTimes: new[]
                {
                    TimeSpan.Zero,
                    stopAfter + stopPeriod,
                    lastEventTime + stopAfter + stopPeriod + stepAfterResumed
                });
        }

        [Test]
        public void MoveToTime()
        {
            var stopAfter = TimeSpan.FromSeconds(4);
            var stopPeriod = TimeSpan.FromSeconds(2);

            CheckPlaybackStop(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOnEvent(), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(), TimeSpan.FromSeconds(10))
                },
                eventsWillBeSent: new EventToSend[] { },
                stopAfter: stopAfter,
                stopPeriod: stopPeriod,
                setupPlayback: NoPlaybackAction,
                afterStart: NoPlaybackAction,
                afterStop: (context, playback) => playback.MoveToTime(new MetricTimeSpan(0, 0, 1)),
                afterResume: (context, playback) => CheckCurrentTime(playback, TimeSpan.FromSeconds(1), "stopped"),
                runningAfterResume: new[]
                {
                    Tuple.Create<TimeSpan, PlaybackAction>(TimeSpan.FromSeconds(1), (context, playback) => CheckCurrentTime(playback, TimeSpan.FromSeconds(2), "resumed")),
                    Tuple.Create<TimeSpan, PlaybackAction>(TimeSpan.FromSeconds(2), (context, playback) =>
                    {
                        playback.MoveToTime(new MetricTimeSpan(0, 0, 8));
                        CheckCurrentTime(playback, TimeSpan.FromSeconds(8), "resumed");
                    }),
                    Tuple.Create<TimeSpan, PlaybackAction>(TimeSpan.FromSeconds(1), (context, playback) => CheckCurrentTime(playback, TimeSpan.FromSeconds(9), "resumed"))
                },
                explicitExpectedTimes: new[]
                {
                    TimeSpan.Zero,
                    TimeSpan.FromSeconds(11)
                });
        }

        [Test]
        public void MoveToTime_BeyondDuration()
        {
            var stopAfter = TimeSpan.FromSeconds(2);
            var stopPeriod = TimeSpan.FromSeconds(2);

            CheckPlaybackStop(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOnEvent(), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(), TimeSpan.FromSeconds(4))
                },
                eventsWillBeSent: new EventToSend[] { },
                stopAfter: stopAfter,
                stopPeriod: stopPeriod,
                setupPlayback: NoPlaybackAction,
                afterStart: NoPlaybackAction,
                afterStop: (context, playback) => playback.MoveToTime(new MetricTimeSpan(0, 0, 10)),
                afterResume: (context, playback) => CheckCurrentTime(playback, TimeSpan.FromSeconds(4), "stopped"),
                runningAfterResume: null,
                explicitExpectedTimes: new[]
                {
                    TimeSpan.Zero,
                    stopAfter + stopPeriod
                });
        }

        [TestCaseSource(nameof(ParametersForDurationCheck))]
        public void GetDuration(TimeSpan start, TimeSpan delayFromStart)
        {
            var eventsToSend = new[]
            {
                new EventToSend(new NoteOnEvent(), start),
                new EventToSend(new NoteOffEvent(), delayFromStart)
            };

            CheckPlaybackStop(
                eventsToSend: eventsToSend,
                eventsWillBeSent: eventsToSend,
                stopAfter: TimeSpan.Zero,
                stopPeriod: TimeSpan.Zero,
                setupPlayback: NoPlaybackAction,
                afterStart: (context, playback) =>
                {
                    var duration = playback.GetDuration<MetricTimeSpan>();
                    Assert.IsTrue(
                        AreTimeSpansEqual(duration, start + delayFromStart),
                        $"Duration is invalid. Actual is {duration}. Expected is {start + delayFromStart}.");
                },
                afterStop: (context, playback) => { },
                afterResume: (context, playback) => { });
        }

        #endregion

        #region Private methods

        private void CheckPlaybackEvents(
            int expectedStartedRaised,
            int expectedStoppedRaised,
            int expectedFinishedRaised,
            PlaybackAction setupPlayback,
            PlaybackAction beforeChecks,
            PlaybackAction afterChecks)
        {
            var started = 0;
            var stopped = 0;
            var finished = 0;

            var playbackEvents = new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent
                {
                    DeltaTime = TimeConverter.ConvertFrom(new MetricTimeSpan(0, 0, 5), TempoMap.Default)
                }
            };

            using (var outputDevice = OutputDevice.GetByName(SendReceiveUtilities.DeviceToTestOnName))
            using (var playback = new Playback(playbackEvents, TempoMap.Default, outputDevice))
            {
                setupPlayback(null, playback);

                playback.Started += (sender, args) => started++;
                playback.Stopped += (sender, args) => stopped++;
                playback.Finished += (sender, args) => finished++;

                playback.Start();
                playback.Stop();
                playback.Start();

                beforeChecks(null, playback);

                Assert.IsTrue(
                    SpinWait.SpinUntil(() => started == expectedStartedRaised && stopped == expectedStoppedRaised && finished == expectedFinishedRaised, TimeSpan.FromSeconds(6)),
                    "Playback events are raised invalid number of times.");

                afterChecks(null, playback);
            }
        }

        private void CheckPlayback(
            ICollection<EventToSend> eventsToSend,
            double speed,
            PlaybackAction beforePlaybackStarted,
            PlaybackAction startPlayback,
            PlaybackAction afterPlaybackStarted,
            PlaybackAction waiting,
            PlaybackAction finalChecks)
        {
            var playbackContext = new PlaybackContext();

            var receivedEvents = playbackContext.ReceivedEvents;
            var sentEvents = playbackContext.SentEvents;
            var stopwatch = playbackContext.Stopwatch;
            var tempoMap = playbackContext.TempoMap;

            var eventsForPlayback = new List<MidiEvent>();
            var expectedTimes = playbackContext.ExpectedTimes;
            var currentTime = TimeSpan.Zero;

            foreach (var eventToSend in eventsToSend)
            {
                var midiEvent = eventToSend.Event.Clone();
                midiEvent.DeltaTime = LengthConverter.ConvertFrom((MetricTimeSpan)eventToSend.Delay, (MetricTimeSpan)currentTime, tempoMap);
                currentTime += eventToSend.Delay;
                eventsForPlayback.Add(midiEvent);
                expectedTimes.Add(TimeSpan.FromTicks(MathUtilities.RoundToLong(currentTime.Ticks / speed)));
            }

            using (var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA))
            {
                SendReceiveUtilities.WarmUpDevice(outputDevice);
                outputDevice.EventSent += (_, e) => sentEvents.Add(new SentEvent(e.Event, stopwatch.Elapsed));

                using (var playback = new Playback(eventsForPlayback, tempoMap, outputDevice))
                {
                    playback.Speed = speed;
                    beforePlaybackStarted(playbackContext, playback);

                    using (var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA))
                    {
                        inputDevice.EventReceived += (_, e) =>
                        {
                            lock (playbackContext.ReceivedEventsLockObject)
                            {
                                receivedEvents.Add(new ReceivedEvent(e.Event, stopwatch.Elapsed));
                            }
                        };
                        inputDevice.StartEventsListening();
                        stopwatch.Start();

                        startPlayback(playbackContext, playback);
                        afterPlaybackStarted(playbackContext, playback);
                        waiting(playbackContext, playback);

                        stopwatch.Stop();

                        finalChecks(playbackContext, playback);
                    }
                }
            }

            CompareSentReceivedEvents(sentEvents.Take(expectedTimes.Count).ToList(), receivedEvents.Take(expectedTimes.Count).ToList(), expectedTimes);
        }

        private void CheckPlaybackStop(
            ICollection<EventToSend> eventsToSend,
            ICollection<EventToSend> eventsWillBeSent,
            TimeSpan stopAfter,
            TimeSpan stopPeriod,
            PlaybackAction setupPlayback,
            PlaybackAction afterStart,
            PlaybackAction afterStop,
            PlaybackAction afterResume,
            IEnumerable<Tuple<TimeSpan, PlaybackAction>> runningAfterResume = null,
            ICollection<TimeSpan> explicitExpectedTimes = null,
            double speed = 1.0)
        {
            var playbackContext = new PlaybackContext();

            var receivedEvents = playbackContext.ReceivedEvents;
            var sentEvents = playbackContext.SentEvents;
            var stopwatch = playbackContext.Stopwatch;
            var tempoMap = playbackContext.TempoMap;

            var eventsForPlayback = new List<MidiEvent>();
            var expectedTimes = playbackContext.ExpectedTimes;
            var currentTime = TimeSpan.Zero;

            foreach (var eventToSend in eventsToSend)
            {
                var midiEvent = eventToSend.Event.Clone();
                midiEvent.DeltaTime = LengthConverter.ConvertFrom((MetricTimeSpan)eventToSend.Delay, (MetricTimeSpan)currentTime, tempoMap);
                currentTime += eventToSend.Delay;
                eventsForPlayback.Add(midiEvent);
            }

            if (explicitExpectedTimes != null)
                expectedTimes.AddRange(explicitExpectedTimes);
            else
            {
                currentTime = TimeSpan.Zero;
                foreach (var eventWillBeSent in eventsWillBeSent)
                {
                    currentTime += eventWillBeSent.Delay;
                    var scaledCurrentTime = ScaleTimeSpan(currentTime, 1.0 / speed);
                    expectedTimes.Add(currentTime > stopAfter ? scaledCurrentTime + stopPeriod : scaledCurrentTime);
                }
            }

            using (var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA))
            {
                SendReceiveUtilities.WarmUpDevice(outputDevice);
                outputDevice.EventSent += (_, e) => sentEvents.Add(new SentEvent(e.Event, stopwatch.Elapsed));

                using (var playback = new Playback(eventsForPlayback, tempoMap, outputDevice))
                {
                    playback.Speed = speed;
                    setupPlayback(playbackContext, playback);

                    using (var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA))
                    {
                        inputDevice.EventReceived += (_, e) =>
                        {
                            lock (playbackContext.ReceivedEventsLockObject)
                            {
                                receivedEvents.Add(new ReceivedEvent(e.Event, stopwatch.Elapsed));
                            }
                        };
                        inputDevice.StartEventsListening();
                        stopwatch.Start();
                        playback.Start();

                        afterStart(playbackContext, playback);

                        SpinWait.SpinUntil(() => stopwatch.Elapsed >= stopAfter);
                        playback.Stop();

                        afterStop(playbackContext, playback);

                        Thread.Sleep(stopPeriod);
                        playback.Start();

                        afterResume(playbackContext, playback);

                        if (runningAfterResume != null)
                        {
                            foreach (var check in runningAfterResume)
                            {
                                Thread.Sleep(check.Item1);
                                check.Item2(playbackContext, playback);
                            }
                        }

                        var timeout = expectedTimes.Last() + SendReceiveUtilities.MaximumEventSendReceiveDelay;
                        var areEventsReceived = SpinWait.SpinUntil(() => receivedEvents.Count == expectedTimes.Count, timeout);
                        Assert.IsTrue(areEventsReceived, $"Events are not received for timeout {timeout}.");

                        stopwatch.Stop();

                        var playbackStopped = SpinWait.SpinUntil(() => !playback.IsRunning, SendReceiveUtilities.MaximumEventSendReceiveDelay);
                        Assert.IsTrue(playbackStopped, "Playback is running after completed.");
                    }
                }
            }

            CompareSentReceivedEvents(sentEvents, receivedEvents, expectedTimes);
        }

        private void CompareSentReceivedEvents(
            IReadOnlyList<SentEvent> sentEvents,
            IReadOnlyList<ReceivedEvent> receivedEvents,
            IReadOnlyList<TimeSpan> expectedTimes)
        {
            for (var i = 0; i < sentEvents.Count; i++)
            {
                var sentEvent = sentEvents[i];
                var receivedEvent = receivedEvents[i];
                var expectedTime = expectedTimes[i];

                Assert.IsTrue(
                    MidiEventEquality.AreEqual(sentEvent.Event, receivedEvent.Event, false),
                    $"Received event {receivedEvent.Event} doesn't match sent one {sentEvent.Event}.");

                var offsetFromExpectedTime = (sentEvent.Time - expectedTime).Duration();
                Assert.LessOrEqual(
                    offsetFromExpectedTime,
                    SendReceiveUtilities.MaximumEventSendReceiveDelay,
                    $"Event was sent at wrong time (at {sentEvent.Time} instead of {expectedTime}).");
            }
        }

        private static void CheckCurrentTime(Playback playback, TimeSpan expectedCurrentTime, string afterPlaybackAction)
        {
            TimeSpan currentTime = (MetricTimeSpan)playback.GetCurrentTime(TimeSpanType.Metric);
            Assert.IsTrue(
                AreTimeSpansEqual(currentTime, expectedCurrentTime),
                $"Current time ({currentTime}) is invalid after playback {afterPlaybackAction} ({expectedCurrentTime}).");
        }

        private static bool AreTimeSpansEqual(TimeSpan timeSpan1, TimeSpan timeSpan2)
        {
            // TODO: decrease epsilon
            var epsilon = TimeSpan.FromMilliseconds(15);
            var delta = (timeSpan1 - timeSpan2).Duration();
            return delta <= epsilon;
        }

        private static TimeSpan ScaleTimeSpan(TimeSpan timeSpan, double scaleValue)
        {
            return TimeSpan.FromTicks(MathUtilities.RoundToLong(timeSpan.Ticks * scaleValue));
        }

        #endregion
    }
}
