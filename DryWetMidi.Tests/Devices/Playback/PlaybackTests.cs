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

        private const int RetriesNumber = 3;

        private static readonly PlaybackAction NoPlaybackAction = (context, playback) => { };

        private static readonly object[] ParametersForDurationCheck =
        {
            new object[] { TimeSpan.Zero, TimeSpan.FromSeconds(2) },
            new object[] { TimeSpan.FromMilliseconds(500), TimeSpan.FromSeconds(3) },
            new object[] { TimeSpan.Zero, TimeSpan.FromSeconds(10) },
            new object[] { TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(12) }
        };

        #endregion

        #region Test methods

        [Retry(RetriesNumber)]
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

        [Retry(RetriesNumber)]
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

        [Retry(RetriesNumber)]
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

        [Retry(RetriesNumber)]
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

        [Retry(RetriesNumber)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        public void CheckPlaybackLooping(int repetitionsNumber)
        {
            var eventsToSend = new[]
            {
                new EventToSend(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)20) { Channel = (FourBitNumber)5 }, TimeSpan.Zero),
                new EventToSend(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)10) { Channel = (FourBitNumber)5 }, TimeSpan.FromSeconds(1)),
                new EventToSend(new NoteOnEvent(), TimeSpan.FromMilliseconds(500)),
                new EventToSend(new NoteOnEvent((SevenBitNumber)30, (SevenBitNumber)50), TimeSpan.Zero),
                new EventToSend(new NoteOffEvent(), TimeSpan.FromSeconds(2)),
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

        [Retry(RetriesNumber)]
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

        [Retry(RetriesNumber)]
        [Test]
        public void InterruptNotesOnStop()
        {
            var noteOnDelay = TimeSpan.Zero;
            var noteOffDelay = TimeSpan.FromSeconds(2);

            var stopAfter = TimeSpan.FromSeconds(1);
            var stopPeriod = TimeSpan.FromMilliseconds(400);

            CheckPlaybackStop(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOnEvent(), noteOnDelay),
                    new EventToSend(new NoteOffEvent(), noteOffDelay)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new NoteOnEvent(), noteOnDelay),
                    new EventToSend(new NoteOffEvent(), stopAfter),
                    new EventToSend(new NoteOffEvent(), noteOffDelay - stopAfter)
                },
                stopAfter: stopAfter,
                stopPeriod: stopPeriod,
                setupPlayback: (context, playback) => playback.InterruptNotesOnStop = true,
                afterStart: NoPlaybackAction,
                afterStop: NoPlaybackAction,
                afterResume: NoPlaybackAction);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void DontInterruptNotesOnStop()
        {
            var eventsToSend = new[]
            {
                new EventToSend(new NoteOnEvent(), TimeSpan.Zero),
                new EventToSend(new NoteOffEvent(), TimeSpan.FromSeconds(2))
            };

            CheckPlaybackStop(
                eventsToSend,
                eventsWillBeSent: eventsToSend,
                stopAfter: TimeSpan.FromSeconds(1),
                stopPeriod: TimeSpan.FromMilliseconds(400),
                setupPlayback: (context, playback) => playback.InterruptNotesOnStop = false,
                afterStart: NoPlaybackAction,
                afterStop: NoPlaybackAction,
                afterResume: NoPlaybackAction);
        }

        [Retry(RetriesNumber)]
        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(0.5)]
        public void GetCurrentTime(double speed)
        {
            var stopAfter = TimeSpan.FromSeconds(1);
            var stopPeriod = TimeSpan.FromMilliseconds(500);

            var firstEventTime = TimeSpan.Zero;
            var lastEventTime = TimeSpan.FromSeconds(5);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(500);
            var secondAfterResumeDelay = TimeSpan.FromMilliseconds(800);

            var eventsToSend = new[]
            {
                new EventToSend(new NoteOnEvent(), firstEventTime),
                new EventToSend(new NoteOffEvent(), lastEventTime)
            };

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
                    Tuple.Create<TimeSpan, PlaybackAction>(firstAfterResumeDelay, (context, playback) => CheckCurrentTime(playback, ScaleTimeSpan(stopAfter + firstAfterResumeDelay, speed), "resumed")),
                    Tuple.Create<TimeSpan, PlaybackAction>(secondAfterResumeDelay, (context, playback) => CheckCurrentTime(playback, ScaleTimeSpan(stopAfter + firstAfterResumeDelay + secondAfterResumeDelay, speed), "resumed"))
                },
                speed: speed);
        }

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

            CheckPlaybackStop(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOnEvent(), firstEventTime),
                    new EventToSend(new NoteOffEvent(), lastEventTime)
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
                    Tuple.Create<TimeSpan, PlaybackAction>(firstAfterResumeDelay, (context, playback) => CheckCurrentTime(playback, ScaleTimeSpan(firstAfterResumeDelay, speed), "resumed")),
                    Tuple.Create<TimeSpan, PlaybackAction>(secondAfterResumeDelay, (context, playback) =>
                    {
                        playback.MoveToStart();
                        CheckCurrentTime(playback, TimeSpan.Zero, "resumed");
                    }),
                    Tuple.Create<TimeSpan, PlaybackAction>(thirdAfterResumeDelay, (context, playback) => CheckCurrentTime(playback, ScaleTimeSpan(thirdAfterResumeDelay, speed), "resumed"))
                },
                explicitExpectedTimes: new[]
                {
                    firstEventTime,
                    stopAfter + stopPeriod,
                    stopAfter + stopPeriod + firstAfterResumeDelay + secondAfterResumeDelay,
                    stopAfter + stopPeriod + firstAfterResumeDelay + secondAfterResumeDelay + ScaleTimeSpan(lastEventTime, 1.0 / speed)
                },
                speed: speed);
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

            //

            CheckPlaybackStop(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOnEvent(), firstEventTime),
                    new EventToSend(new NoteOffEvent(), lastEventTime)
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
                    Tuple.Create<TimeSpan, PlaybackAction>(firstAfterResumeDelay, (context, playback) => CheckCurrentTime(playback, ScaleTimeSpan(stopAfter + firstAfterResumeDelay, speed) + stepAfterStop, "resumed")),
                    Tuple.Create<TimeSpan, PlaybackAction>(secondAfterResumeDelay, (context, playback) =>
                    {
                        playback.MoveForward((MetricTimeSpan)stepAfterResumed);
                        CheckCurrentTime(playback, ScaleTimeSpan(stopAfter + firstAfterResumeDelay + secondAfterResumeDelay, speed) + stepAfterStop + stepAfterResumed, "resumed");
                    }),
                    Tuple.Create<TimeSpan, PlaybackAction>(thirdAfterResumeDelay, (context, playback) => CheckCurrentTime(playback, ScaleTimeSpan(stopAfter + firstAfterResumeDelay + secondAfterResumeDelay + thirdAfterResumeDelay, speed) + stepAfterStop + stepAfterResumed, "resumed"))
                },
                explicitExpectedTimes: new[]
                {
                    firstEventTime,
                    ScaleTimeSpan(lastEventTime - stepAfterStop - stepAfterResumed, 1.0 / speed) + stopPeriod
                },
                speed: speed);
        }

        [Retry(RetriesNumber)]
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

        [Retry(RetriesNumber)]
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

        [Retry(RetriesNumber)]
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

        [Retry(RetriesNumber)]
        [TestCaseSource(nameof(ParametersForDurationCheck))]
        public void GetDuration(TimeSpan start, TimeSpan delayFromStart)
        {
            var tempoMap = TempoMap.Default;

            var eventsToSend = new[]
            {
                new EventToSend(new NoteOnEvent(), start),
                new EventToSend(new NoteOffEvent(), delayFromStart)
            };

            var eventsForPlayback = GetEventsForPlayback(eventsToSend, tempoMap);

            using (var outputDevice = OutputDevice.GetByName(SendReceiveUtilities.DeviceToTestOnName))
            using (var playback = new Playback(eventsForPlayback, tempoMap, outputDevice))
            {
                var duration = playback.GetDuration<MetricTimeSpan>();
                Assert.IsTrue(
                    AreTimeSpansEqual(duration, start + delayFromStart),
                    $"Duration is invalid. Actual is {duration}. Expected is {start + delayFromStart}.");
            }
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackNotes_MoveForwardToNote(bool useOutputDevice)
        {
            var noteNumber = (SevenBitNumber)60;
            var noteOnDelay = TimeSpan.FromSeconds(1);
            var noteOnVelocity = (SevenBitNumber)100;
            var noteOffDelay = TimeSpan.FromMilliseconds(800);
            var noteOffVelocity = (SevenBitNumber)80;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(1500);

            CheckTrackNotes(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOnEvent(noteNumber, noteOnVelocity), noteOnDelay),
                    new EventToSend(new NoteOffEvent(noteNumber, noteOffVelocity), noteOffDelay)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new NoteOnEvent(noteNumber, noteOnVelocity), moveFrom),
                    new EventToSend(new NoteOffEvent(noteNumber, noteOffVelocity), noteOnDelay + noteOffDelay - moveTo)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                notesWillBeStarted: new[] { 0 },
                notesWillBeFinished: new[] { 0 },
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackNotes_MoveBackToNote(bool useOutputDevice)
        {
            var noteNumber = (SevenBitNumber)60;
            var noteOnDelay = TimeSpan.Zero;
            var noteOnVelocity = (SevenBitNumber)100;
            var noteOffDelay = TimeSpan.FromMilliseconds(800);
            var noteOffVelocity = (SevenBitNumber)80;
            var pitchBendDelay = TimeSpan.FromMilliseconds(300);

            var moveFrom = TimeSpan.FromSeconds(1);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckTrackNotes(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOnEvent(noteNumber, noteOnVelocity), noteOnDelay),
                    new EventToSend(new NoteOffEvent(noteNumber, noteOffVelocity), noteOffDelay),
                    new EventToSend(new PitchBendEvent(), pitchBendDelay)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new NoteOnEvent(noteNumber, noteOnVelocity), noteOnDelay),
                    new EventToSend(new NoteOffEvent(noteNumber, noteOffVelocity), noteOffDelay),
                    new EventToSend(new NoteOnEvent(noteNumber, noteOnVelocity), moveFrom - (noteOnDelay + noteOffDelay)),
                    new EventToSend(new NoteOffEvent(noteNumber, noteOffVelocity), noteOnDelay + noteOffDelay - moveTo),
                    new EventToSend(new PitchBendEvent(), pitchBendDelay)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                notesWillBeStarted: new[] { 0, 0 },
                notesWillBeFinished: new[] { 0, 0 },
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackNotes_MoveForwardFromNote(bool useOutputDevice)
        {
            var noteNumber = (SevenBitNumber)60;
            var noteOnDelay = TimeSpan.Zero;
            var noteOnVelocity = (SevenBitNumber)100;
            var noteOffDelay = TimeSpan.FromMilliseconds(800);
            var noteOffVelocity = (SevenBitNumber)80;
            var pitchBendDelay = TimeSpan.FromMilliseconds(400);

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(1000);

            CheckTrackNotes(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOnEvent(noteNumber, noteOnVelocity), noteOnDelay),
                    new EventToSend(new NoteOffEvent(noteNumber, noteOffVelocity), noteOffDelay),
                    new EventToSend(new PitchBendEvent(), pitchBendDelay)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new NoteOnEvent(noteNumber, noteOnVelocity), noteOnDelay),
                    new EventToSend(new NoteOffEvent(noteNumber, noteOffVelocity), moveFrom - noteOnDelay),
                    new EventToSend(new PitchBendEvent(), noteOnDelay + noteOffDelay + pitchBendDelay - moveTo)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                notesWillBeStarted: new[] { 0 },
                notesWillBeFinished: new[] { 0 },
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackNotes_MoveBackFromNote(bool useOutputDevice)
        {
            var noteNumber = (SevenBitNumber)60;
            var noteOnDelay = TimeSpan.FromSeconds(1);
            var noteOnVelocity = (SevenBitNumber)100;
            var noteOffDelay = TimeSpan.FromMilliseconds(400);
            var noteOffVelocity = (SevenBitNumber)80;

            var moveFrom = TimeSpan.FromMilliseconds(1200);
            var moveTo = TimeSpan.FromMilliseconds(700);

            CheckTrackNotes(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOnEvent(noteNumber, noteOnVelocity), noteOnDelay),
                    new EventToSend(new NoteOffEvent(noteNumber, noteOffVelocity), noteOffDelay)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new NoteOnEvent(noteNumber, noteOnVelocity), noteOnDelay),
                    new EventToSend(new NoteOffEvent(noteNumber, noteOffVelocity), moveFrom - noteOnDelay),
                    new EventToSend(new NoteOnEvent(noteNumber, noteOnVelocity), noteOnDelay - moveTo),
                    new EventToSend(new NoteOffEvent(noteNumber, noteOffVelocity), noteOffDelay)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                notesWillBeStarted: new[] { 0, 0 },
                notesWillBeFinished: new[] { 0, 0 },
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackNotes_MoveForwardToSameNote(bool useOutputDevice)
        {
            var noteNumber = (SevenBitNumber)60;
            var noteOnDelay = TimeSpan.Zero;
            var noteOnVelocity = (SevenBitNumber)100;
            var noteOffDelay = TimeSpan.FromSeconds(1);
            var noteOffVelocity = (SevenBitNumber)80;

            var moveFrom = TimeSpan.FromMilliseconds(200);
            var moveTo = TimeSpan.FromMilliseconds(700);

            CheckTrackNotes(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOnEvent(noteNumber, noteOnVelocity), noteOnDelay),
                    new EventToSend(new NoteOffEvent(noteNumber, noteOffVelocity), noteOffDelay)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new NoteOnEvent(noteNumber, noteOnVelocity), noteOnDelay),
                    new EventToSend(new NoteOffEvent(noteNumber, noteOffVelocity), noteOnDelay + noteOffDelay - moveTo + moveFrom)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                notesWillBeStarted: new[] { 0 },
                notesWillBeFinished: new[] { 0 },
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackNotes_MoveBackToSameNote(bool useOutputDevice)
        {
            var noteNumber = (SevenBitNumber)60;
            var noteOnDelay = TimeSpan.Zero;
            var noteOnVelocity = (SevenBitNumber)100;
            var noteOffDelay = TimeSpan.FromSeconds(1);
            var noteOffVelocity = (SevenBitNumber)80;

            var moveFrom = TimeSpan.FromMilliseconds(700);
            var moveTo = TimeSpan.FromMilliseconds(400);

            CheckTrackNotes(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOnEvent(noteNumber, noteOnVelocity), noteOnDelay),
                    new EventToSend(new NoteOffEvent(noteNumber, noteOffVelocity), noteOffDelay)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new NoteOnEvent(noteNumber, noteOnVelocity), noteOnDelay),
                    new EventToSend(new NoteOffEvent(noteNumber, noteOffVelocity), noteOnDelay + noteOffDelay - moveTo + moveFrom)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                notesWillBeStarted: new[] { 0 },
                notesWillBeFinished: new[] { 0 },
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackNotes_MoveForwardFromNoteToNote(bool useOutputDevice)
        {
            var noteNumber1 = (SevenBitNumber)60;
            var noteOnDelay1 = TimeSpan.Zero;
            var noteOnVelocity1 = (SevenBitNumber)100;
            var noteOffDelay1 = TimeSpan.FromSeconds(1);
            var noteOffVelocity1 = (SevenBitNumber)80;

            var noteNumber2 = (SevenBitNumber)70;
            var noteOnDelay2 = TimeSpan.FromMilliseconds(200);
            var noteOnVelocity2 = (SevenBitNumber)95;
            var noteOffDelay2 = TimeSpan.FromMilliseconds(800);
            var noteOffVelocity2 = (SevenBitNumber)85;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(1400);

            CheckTrackNotes(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOnEvent(noteNumber1, noteOnVelocity1), noteOnDelay1),
                    new EventToSend(new NoteOffEvent(noteNumber1, noteOffVelocity1), noteOffDelay1),
                    new EventToSend(new NoteOnEvent(noteNumber2, noteOnVelocity2), noteOnDelay2),
                    new EventToSend(new NoteOffEvent(noteNumber2, noteOffVelocity2), noteOffDelay2)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new NoteOnEvent(noteNumber1, noteOnVelocity1), noteOnDelay1),
                    new EventToSend(new NoteOffEvent(noteNumber1, noteOffVelocity1), moveFrom),
                    new EventToSend(new NoteOnEvent(noteNumber2, noteOnVelocity2), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(noteNumber2, noteOffVelocity2), noteOnDelay1 + noteOffDelay1 + noteOnDelay2 + noteOffDelay2 - moveTo)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                notesWillBeStarted: new[] { 0, 1 },
                notesWillBeFinished: new[] { 0, 1 },
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackNotes_MoveBackFromNoteToNote(bool useOutputDevice)
        {
            var noteNumber1 = (SevenBitNumber)60;
            var noteOnDelay1 = TimeSpan.Zero;
            var noteOnVelocity1 = (SevenBitNumber)100;
            var noteOffDelay1 = TimeSpan.FromSeconds(1);
            var noteOffVelocity1 = (SevenBitNumber)80;

            var noteNumber2 = (SevenBitNumber)70;
            var noteOnDelay2 = TimeSpan.FromMilliseconds(200);
            var noteOnVelocity2 = (SevenBitNumber)95;
            var noteOffDelay2 = TimeSpan.FromMilliseconds(800);
            var noteOffVelocity2 = (SevenBitNumber)85;

            var moveFrom = TimeSpan.FromMilliseconds(1400);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckTrackNotes(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOnEvent(noteNumber1, noteOnVelocity1), noteOnDelay1),
                    new EventToSend(new NoteOffEvent(noteNumber1, noteOffVelocity1), noteOffDelay1),
                    new EventToSend(new NoteOnEvent(noteNumber2, noteOnVelocity2), noteOnDelay2),
                    new EventToSend(new NoteOffEvent(noteNumber2, noteOffVelocity2), noteOffDelay2)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new NoteOnEvent(noteNumber1, noteOnVelocity1), noteOnDelay1),
                    new EventToSend(new NoteOffEvent(noteNumber1, noteOffVelocity1), noteOffDelay1),
                    new EventToSend(new NoteOnEvent(noteNumber2, noteOnVelocity2), noteOnDelay2),
                    new EventToSend(new NoteOffEvent(noteNumber2, noteOffVelocity2), moveFrom - (noteOnDelay1 + noteOffDelay1 + noteOnDelay2)),
                    new EventToSend(new NoteOnEvent(noteNumber1, noteOnVelocity1), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(noteNumber1, noteOffVelocity1), noteOnDelay1 + noteOffDelay1 - moveTo),
                    new EventToSend(new NoteOnEvent(noteNumber2, noteOnVelocity2), noteOnDelay2),
                    new EventToSend(new NoteOffEvent(noteNumber2, noteOffVelocity2), noteOffDelay2)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                notesWillBeStarted: new[] { 0, 1, 0, 1 },
                notesWillBeFinished: new[] { 0, 1, 0, 1 },
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackNotes_StopStart_InterruptNotesOnStop()
        {
            var noteOnDelay = TimeSpan.Zero;
            var noteOffDelay = TimeSpan.FromSeconds(2);

            var stopAfter = TimeSpan.FromSeconds(1);
            var stopPeriod = TimeSpan.Zero;

            CheckPlaybackStop(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOnEvent(), noteOnDelay),
                    new EventToSend(new NoteOffEvent(), noteOffDelay)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new NoteOnEvent(), noteOnDelay),
                    new EventToSend(new NoteOffEvent(), stopAfter),
                    new EventToSend(new NoteOnEvent(), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(), noteOnDelay + noteOffDelay - stopAfter)
                },
                stopAfter: stopAfter,
                stopPeriod: stopPeriod,
                setupPlayback: (context, playback) =>
                {
                    playback.InterruptNotesOnStop = true;
                    playback.TrackNotes = true;
                },
                afterStart: NoPlaybackAction,
                afterStop: NoPlaybackAction,
                afterResume: NoPlaybackAction);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackNotes_StopStart_DontInterruptNotesOnStop()
        {
            var noteOnDelay = TimeSpan.Zero;
            var noteOffDelay = TimeSpan.FromSeconds(2);

            var stopAfter = TimeSpan.FromSeconds(1);
            var stopPeriod = TimeSpan.Zero;

            CheckPlaybackStop(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOnEvent(), noteOnDelay),
                    new EventToSend(new NoteOffEvent(), noteOffDelay)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new NoteOnEvent(), noteOnDelay),
                    new EventToSend(new NoteOffEvent(), noteOffDelay)
                },
                stopAfter: stopAfter,
                stopPeriod: stopPeriod,
                setupPlayback: (context, playback) =>
                {
                    playback.InterruptNotesOnStop = false;
                    playback.TrackNotes = true;
                },
                afterStart: NoPlaybackAction,
                afterStop: NoPlaybackAction,
                afterResume: NoPlaybackAction);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void ChangeOutputDeviceDuringPlayback()
        {
            var tempoMap = TempoMap.Default;
            var stopwatch = new Stopwatch();

            var changeDeviceAfter = TimeSpan.FromSeconds(1);
            var firstEventDelay = TimeSpan.Zero;
            var secondEventDelay = TimeSpan.FromSeconds(2);

            var eventsToSend = new[]
            {
                new EventToSend(new NoteOnEvent(), firstEventDelay),
                new EventToSend(new NoteOffEvent(), secondEventDelay)
            };

            var eventsForPlayback = GetEventsForPlayback(eventsToSend, tempoMap);

            var sentEventsA = new List<SentEvent>();
            var sentEventsB = new List<SentEvent>();

            var receivedEventsA = new List<ReceivedEvent>();
            var receivedEventsB = new List<ReceivedEvent>();

            using (var outputDeviceA = OutputDevice.GetByName(MidiDevicesNames.DeviceA))
            {
                outputDeviceA.EventSent += (_, e) => sentEventsA.Add(new SentEvent(e.Event, stopwatch.Elapsed));

                using (var inputDeviceA = InputDevice.GetByName(MidiDevicesNames.DeviceA))
                {
                    inputDeviceA.StartEventsListening();
                    inputDeviceA.EventReceived += (_, e) => receivedEventsA.Add(new ReceivedEvent(e.Event, stopwatch.Elapsed));

                    using (var outputDeviceB = OutputDevice.GetByName(MidiDevicesNames.DeviceB))
                    {
                        outputDeviceB.EventSent += (_, e) => sentEventsB.Add(new SentEvent(e.Event, stopwatch.Elapsed));

                        using (var inputDeviceB = InputDevice.GetByName(MidiDevicesNames.DeviceB))
                        {
                            inputDeviceB.StartEventsListening();
                            inputDeviceB.EventReceived += (_, e) => receivedEventsB.Add(new ReceivedEvent(e.Event, stopwatch.Elapsed));

                            using (var playback = new Playback(eventsForPlayback, tempoMap))
                            {
                                Assert.IsNull(playback.OutputDevice, "Output device is not null on playback created.");

                                playback.OutputDevice = outputDeviceA;
                                Assert.AreSame(outputDeviceA, playback.OutputDevice, "Output device was not changed to Device A.");

                                playback.Start();
                                stopwatch.Start();

                                Thread.Sleep(changeDeviceAfter);

                                playback.OutputDevice = outputDeviceB;
                                Assert.AreSame(outputDeviceB, playback.OutputDevice, "Output device was not changed to Device B.");

                                var playbackStopped = SpinWait.SpinUntil(() => !playback.IsRunning, firstEventDelay + secondEventDelay - changeDeviceAfter + SendReceiveUtilities.MaximumEventSendReceiveDelay);
                                Assert.IsTrue(playbackStopped, "Playback is running after completed.");
                            }
                        }
                    }
                }
            }

            CompareSentReceivedEvents(sentEventsA, receivedEventsA, new[] { eventsToSend.First() });
            CompareSentReceivedEvents(sentEventsB, receivedEventsB, new[] { eventsToSend.Last() });
        }

        [Test]
        public void CheckSnapPoints()
        {
            var tempoMap = TempoMap.Default;

            var eventsToSend = new[]
            {
                new EventToSend(new NoteOnEvent(), TimeSpan.Zero),
                new EventToSend(new NoteOffEvent(), TimeSpan.FromSeconds(10))
            };

            var eventsForPlayback = GetEventsForPlayback(eventsToSend, tempoMap);

            using (var playback = new Playback(eventsForPlayback, tempoMap))
            {
                Assert.IsNotNull(playback.Snapping, "Snapping is null.");
                CollectionAssert.IsEmpty(playback.Snapping.SnapPoints, "Snap points collection is not empty on start.");

                var customSnapPoint = playback.Snapping.AddSnapPoint(new MetricTimeSpan(0, 0, 0, 300), "Data");
                Assert.IsNull(customSnapPoint.SnapPointsGroup, "Snap points group for custom snap point is not null.");

                var snapPointsGroup = playback.Snapping.SnapToGrid(new SteppedGrid(new MetricTimeSpan(0, 0, 0, 100), new MetricTimeSpan(0, 0, 4)));
                Assert.IsNotNull(snapPointsGroup, "Grid snap points group is null.");

                var snapPoints = playback.Snapping.SnapPoints.ToList();
                Assert.AreEqual(4, snapPoints.Count, "Snap points count is invalid.");
                CollectionAssert.Contains(snapPoints, customSnapPoint, "Snap points doesn't contain custom one.");
                Assert.That(snapPoints.Select(p => p.IsEnabled), Is.All.True, "Not all snap points are enabled.");

                var gridSnapPoints = snapPoints.Where(p => p.SnapPointsGroup == snapPointsGroup).ToList();
                Assert.AreEqual(3, gridSnapPoints.Count, "Grid snap points count is invalid.");
            }
        }

        [Test]
        public void EnableDisableSnapPointsGroup()
        {
            var tempoMap = TempoMap.Default;

            var eventsToSend = new[]
            {
                new EventToSend(new NoteOnEvent(), TimeSpan.Zero),
                new EventToSend(new NoteOffEvent(), TimeSpan.FromSeconds(10))
            };

            var eventsForPlayback = GetEventsForPlayback(eventsToSend, tempoMap);

            using (var playback = new Playback(eventsForPlayback, tempoMap))
            {
                var snapPointsGroup = playback.Snapping.SnapToGrid(new SteppedGrid(new MetricTimeSpan(0, 0, 0, 100), new MetricTimeSpan(0, 0, 4)));
                Assert.That(playback.Snapping.SnapPoints.Select(p => p.IsEnabled), Is.All.True, "Not all snap points are enabled.");
                Assert.IsTrue(snapPointsGroup.IsEnabled, "Snap points group is not enabled on start.");
                Assert.That(GetActiveSnapPoints(playback), Has.Count.EqualTo(3), "Not all snap points are active.");

                snapPointsGroup.IsEnabled = false;
                Assert.IsFalse(snapPointsGroup.IsEnabled, "Snap points group is not disabled.");
                Assert.That(playback.Snapping.SnapPoints.Select(p => p.IsEnabled), Is.All.True, "Not all snap points are enabled after group disabled.");
                CollectionAssert.IsEmpty(GetActiveSnapPoints(playback), "Some snap points are active after group disabled.");

                snapPointsGroup.IsEnabled = true;
                Assert.IsTrue(snapPointsGroup.IsEnabled, "Snap points group is not enabled.");
                Assert.That(playback.Snapping.SnapPoints.Select(p => p.IsEnabled), Is.All.True, "Not all snap points are enabled.");
                Assert.That(GetActiveSnapPoints(playback), Has.Count.EqualTo(3), "Not all snap points are active after group enabled.");
            }
        }

        [Test]
        public void AddSnapPoint_WithoutData()
        {
            var tempoMap = TempoMap.Default;

            var eventsToSend = new[]
            {
                new EventToSend(new NoteOnEvent(), TimeSpan.Zero),
                new EventToSend(new NoteOffEvent(), TimeSpan.FromSeconds(10))
            };

            var eventsForPlayback = GetEventsForPlayback(eventsToSend, tempoMap);

            using (var playback = new Playback(eventsForPlayback, tempoMap))
            {
                var snapPoint = playback.Snapping.AddSnapPoint(new MetricTimeSpan(0, 0, 1));
                Assert.IsNotNull(snapPoint, "Snap point is null.");
                Assert.IsInstanceOf<Guid>(snapPoint.Data, "Snap point's data is not of Guid type.");
                CollectionAssert.Contains(playback.Snapping.SnapPoints, snapPoint, "Snap points doesn't contain the snap point.");
            }
        }

        [Test]
        public void AddSnapPoint_WithData()
        {
            var tempoMap = TempoMap.Default;

            var eventsToSend = new[]
            {
                new EventToSend(new NoteOnEvent(), TimeSpan.Zero),
                new EventToSend(new NoteOffEvent(), TimeSpan.FromSeconds(10))
            };

            var eventsForPlayback = GetEventsForPlayback(eventsToSend, tempoMap);

            using (var playback = new Playback(eventsForPlayback, tempoMap))
            {
                var snapPoint = playback.Snapping.AddSnapPoint(new MetricTimeSpan(0, 0, 1), "Data");
                Assert.IsNotNull(snapPoint, "Snap point is null.");
                Assert.AreEqual("Data", snapPoint.Data, "Snap point's data is invalid.");
                CollectionAssert.Contains(playback.Snapping.SnapPoints, snapPoint, "Snap points doesn't contain the snap point.");
            }
        }

        [Test]
        public void RemoveSnapPoint()
        {
            var tempoMap = TempoMap.Default;

            var eventsToSend = new[]
            {
                new EventToSend(new NoteOnEvent(), TimeSpan.Zero),
                new EventToSend(new NoteOffEvent(), TimeSpan.FromSeconds(10))
            };

            var eventsForPlayback = GetEventsForPlayback(eventsToSend, tempoMap);

            using (var playback = new Playback(eventsForPlayback, tempoMap))
            {
                var snapPoint = playback.Snapping.AddSnapPoint(new MetricTimeSpan(0, 0, 1));
                CollectionAssert.Contains(playback.Snapping.SnapPoints, snapPoint, "Snap points doesn't contain the snap point.");

                playback.Snapping.RemoveSnapPoint(snapPoint);
                CollectionAssert.DoesNotContain(playback.Snapping.SnapPoints, snapPoint, "Snap points contain the snap point after removing.");
            }
        }

        [Test]
        public void RemoveSnapPointsByData()
        {
            var tempoMap = TempoMap.Default;

            var eventsToSend = new[]
            {
                new EventToSend(new NoteOnEvent(), TimeSpan.Zero),
                new EventToSend(new NoteOffEvent(), TimeSpan.FromSeconds(10))
            };

            var eventsForPlayback = GetEventsForPlayback(eventsToSend, tempoMap);

            using (var playback = new Playback(eventsForPlayback, tempoMap))
            {
                var snapPoint1 = playback.Snapping.AddSnapPoint(new MetricTimeSpan(0, 0, 1), "DataX");
                var snapPoint2 = playback.Snapping.AddSnapPoint(new MetricTimeSpan(0, 0, 2), "DataY");
                var snapPoint3 = playback.Snapping.AddSnapPoint(new MetricTimeSpan(0, 0, 3), "Something");
                CollectionAssert.Contains(playback.Snapping.SnapPoints, snapPoint1, "Snap points doesn't contain the snap point #1.");
                CollectionAssert.Contains(playback.Snapping.SnapPoints, snapPoint2, "Snap points doesn't contain the snap point #2.");
                CollectionAssert.Contains(playback.Snapping.SnapPoints, snapPoint3, "Snap points doesn't contain the snap point #3.");

                playback.Snapping.RemoveSnapPointsByData((string data) => data.StartsWith("Data"));
                CollectionAssert.DoesNotContain(playback.Snapping.SnapPoints, snapPoint1, "Snap points contain the snap point #1.");
                CollectionAssert.DoesNotContain(playback.Snapping.SnapPoints, snapPoint2, "Snap points contain the snap point #2.");
                CollectionAssert.Contains(playback.Snapping.SnapPoints, snapPoint3, "Snap points doesn't contain the snap point #3.");
            }
        }

        [Retry(RetriesNumber)]
        [Test]
        public void MoveToSnapPoint()
        {
            var stopAfter = TimeSpan.FromSeconds(2);
            var stopPeriod = TimeSpan.FromSeconds(1);

            SnapPoint<string> snapPoint1 = null;
            SnapPoint<string> snapPoint2 = null;

            var snapPointTime1 = TimeSpan.FromSeconds(1);
            var snapPointTime2 = TimeSpan.FromSeconds(3);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(300);
            var secondAfterResumeDelay = TimeSpan.FromMilliseconds(600);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(200);

            var endTime = TimeSpan.FromSeconds(4);

            CheckPlaybackStop(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOnEvent(), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(), endTime)
                },
                eventsWillBeSent: new EventToSend[] { },
                stopAfter: stopAfter,
                stopPeriod: stopPeriod,
                setupPlayback: (context, playback) =>
                {
                    snapPoint1 = playback.Snapping.AddSnapPoint((MetricTimeSpan)snapPointTime1, "Data1");
                    snapPoint2 = playback.Snapping.AddSnapPoint((MetricTimeSpan)snapPointTime2, "Data2");
                },
                afterStart: NoPlaybackAction,
                afterStop: (context, playback) => playback.MoveToSnapPoint(snapPoint1),
                afterResume: (context, playback) => CheckCurrentTime(playback, snapPointTime1, "stopped"),
                runningAfterResume: new[]
                {
                    Tuple.Create<TimeSpan, PlaybackAction>(firstAfterResumeDelay, (context, playback) => CheckCurrentTime(playback, snapPointTime1 + firstAfterResumeDelay, "resumed")),
                    Tuple.Create<TimeSpan, PlaybackAction>(secondAfterResumeDelay, (context, playback) =>
                    {
                        playback.MoveToSnapPoint(snapPoint2);
                        CheckCurrentTime(playback, snapPointTime2, "resumed");
                    }),
                    Tuple.Create<TimeSpan, PlaybackAction>(thirdAfterResumeDelay, (context, playback) => CheckCurrentTime(playback, snapPointTime2 + thirdAfterResumeDelay, "resumed"))
                },
                explicitExpectedTimes: new[]
                {
                    TimeSpan.Zero,
                    endTime + stopPeriod + (stopAfter - snapPointTime1) - (snapPointTime2 - (snapPointTime1 + firstAfterResumeDelay + secondAfterResumeDelay))
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void MoveToPreviousSnapPoint_ByGroup()
        {
            var stopAfter = TimeSpan.FromSeconds(2);
            var stopPeriod = TimeSpan.FromSeconds(1);

            SnapPointsGroup snapPointsGroup = null;

            var snapPointTime1 = TimeSpan.FromSeconds(1);
            var snapPointTime2 = TimeSpan.FromMilliseconds(2100);

            var firstAfterResumeDelay = TimeSpan.FromSeconds(1);
            var secondAfterResumeDelay = TimeSpan.FromMilliseconds(600);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(200);

            var endTime = TimeSpan.FromSeconds(4);

            CheckPlaybackStop(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOnEvent(), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(), endTime)
                },
                eventsWillBeSent: new EventToSend[] { },
                stopAfter: stopAfter,
                stopPeriod: stopPeriod,
                setupPlayback: (context, playback) => snapPointsGroup = playback.Snapping.SnapToGrid(new ArbitraryGrid((MetricTimeSpan)snapPointTime1, (MetricTimeSpan)snapPointTime2)),
                afterStart: NoPlaybackAction,
                afterStop: (context, playback) => playback.MoveToPreviousSnapPoint(snapPointsGroup),
                afterResume: (context, playback) => CheckCurrentTime(playback, snapPointTime1, "stopped"),
                runningAfterResume: new[]
                {
                    Tuple.Create<TimeSpan, PlaybackAction>(firstAfterResumeDelay, (context, playback) => CheckCurrentTime(playback, snapPointTime1 + firstAfterResumeDelay, "resumed")),
                    Tuple.Create<TimeSpan, PlaybackAction>(secondAfterResumeDelay, (context, playback) =>
                    {
                        playback.MoveToPreviousSnapPoint(snapPointsGroup);
                        CheckCurrentTime(playback, snapPointTime2, "resumed");
                    }),
                    Tuple.Create<TimeSpan, PlaybackAction>(thirdAfterResumeDelay, (context, playback) => CheckCurrentTime(playback, snapPointTime2 + thirdAfterResumeDelay, "resumed"))
                },
                explicitExpectedTimes: new[]
                {
                    TimeSpan.Zero,
                    endTime + stopPeriod + (stopAfter - snapPointTime1) + (snapPointTime1 + firstAfterResumeDelay + secondAfterResumeDelay - snapPointTime2)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void MoveToPreviousSnapPoint_Global()
        {
            var stopAfter = TimeSpan.FromSeconds(2);
            var stopPeriod = TimeSpan.FromSeconds(1);

            var snapPointTime1 = TimeSpan.FromSeconds(1);
            var snapPointTime2 = TimeSpan.FromMilliseconds(2100);
            var snapPointTime3 = TimeSpan.FromMilliseconds(2300);

            var firstAfterResumeDelay = TimeSpan.FromSeconds(1);
            var secondAfterResumeDelay = TimeSpan.FromMilliseconds(600);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(200);

            var endTime = TimeSpan.FromSeconds(4);

            CheckPlaybackStop(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOnEvent(), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(), endTime)
                },
                eventsWillBeSent: new EventToSend[] { },
                stopAfter: stopAfter,
                stopPeriod: stopPeriod,
                setupPlayback: (context, playback) =>
                {
                    playback.Snapping.SnapToGrid(new ArbitraryGrid((MetricTimeSpan)snapPointTime1, (MetricTimeSpan)snapPointTime2));
                    playback.Snapping.AddSnapPoint((MetricTimeSpan)snapPointTime3, "Data");
                    var snapPoint = playback.Snapping.AddSnapPoint(new MetricTimeSpan(0, 0, 0, 2400), "Data");
                    snapPoint.IsEnabled = false;
                },
                afterStart: NoPlaybackAction,
                afterStop: (context, playback) => playback.MoveToPreviousSnapPoint(),
                afterResume: (context, playback) => CheckCurrentTime(playback, snapPointTime1, "stopped"),
                runningAfterResume: new[]
                {
                    Tuple.Create<TimeSpan, PlaybackAction>(firstAfterResumeDelay, (context, playback) => CheckCurrentTime(playback, snapPointTime1 + firstAfterResumeDelay, "resumed")),
                    Tuple.Create<TimeSpan, PlaybackAction>(secondAfterResumeDelay, (context, playback) =>
                    {
                        playback.MoveToPreviousSnapPoint();
                        CheckCurrentTime(playback, snapPointTime3, "resumed");
                    }),
                    Tuple.Create<TimeSpan, PlaybackAction>(thirdAfterResumeDelay, (context, playback) => CheckCurrentTime(playback, snapPointTime3 + thirdAfterResumeDelay, "resumed"))
                },
                explicitExpectedTimes: new[]
                {
                    TimeSpan.Zero,
                    endTime + stopPeriod + (stopAfter - snapPointTime1) + (snapPointTime1 + firstAfterResumeDelay + secondAfterResumeDelay - snapPointTime3)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void MoveToNextSnapPoint_ByGroup()
        {
            var stopAfter = TimeSpan.FromSeconds(1);
            var stopPeriod = TimeSpan.FromMilliseconds(1100);

            SnapPointsGroup snapPointsGroup = null;

            var snapPointTime1 = TimeSpan.FromMilliseconds(1200);
            var snapPointTime2 = TimeSpan.FromSeconds(3);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(300);
            var secondAfterResumeDelay = TimeSpan.FromMilliseconds(600);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(200);

            var endTime = TimeSpan.FromSeconds(4);

            CheckPlaybackStop(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOnEvent(), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(), endTime)
                },
                eventsWillBeSent: new EventToSend[] { },
                stopAfter: stopAfter,
                stopPeriod: stopPeriod,
                setupPlayback: (context, playback) => snapPointsGroup = playback.Snapping.SnapToGrid(new ArbitraryGrid((MetricTimeSpan)snapPointTime1, (MetricTimeSpan)snapPointTime2)),
                afterStart: NoPlaybackAction,
                afterStop: (context, playback) => playback.MoveToNextSnapPoint(snapPointsGroup),
                afterResume: (context, playback) => CheckCurrentTime(playback, snapPointTime1, "stopped"),
                runningAfterResume: new[]
                {
                    Tuple.Create<TimeSpan, PlaybackAction>(firstAfterResumeDelay, (context, playback) => CheckCurrentTime(playback, snapPointTime1 + firstAfterResumeDelay, "resumed")),
                    Tuple.Create<TimeSpan, PlaybackAction>(secondAfterResumeDelay, (context, playback) =>
                    {
                        playback.MoveToNextSnapPoint(snapPointsGroup);
                        CheckCurrentTime(playback, snapPointTime2, "resumed");
                    }),
                    Tuple.Create<TimeSpan, PlaybackAction>(thirdAfterResumeDelay, (context, playback) => CheckCurrentTime(playback, snapPointTime2 + thirdAfterResumeDelay, "resumed"))
                },
                explicitExpectedTimes: new[]
                {
                    TimeSpan.Zero,
                    endTime + stopPeriod - (snapPointTime1 - stopAfter) - (snapPointTime2 - (snapPointTime1 + firstAfterResumeDelay + secondAfterResumeDelay))
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void MoveToNextSnapPoint_Global()
        {
            var stopAfter = TimeSpan.FromSeconds(1);
            var stopPeriod = TimeSpan.FromMilliseconds(1100);

            var snapPointTime1 = TimeSpan.FromMilliseconds(1200);
            var snapPointTime2 = TimeSpan.FromSeconds(3);
            var snapPointTime3 = TimeSpan.FromMilliseconds(2900);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(300);
            var secondAfterResumeDelay = TimeSpan.FromMilliseconds(600);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(200);

            var endTime = TimeSpan.FromSeconds(4);

            CheckPlaybackStop(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOnEvent(), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(), endTime)
                },
                eventsWillBeSent: new EventToSend[] { },
                stopAfter: stopAfter,
                stopPeriod: stopPeriod,
                setupPlayback: (context, playback) =>
                {
                    playback.Snapping.SnapToGrid(new ArbitraryGrid((MetricTimeSpan)snapPointTime1, (MetricTimeSpan)snapPointTime2));
                    playback.Snapping.AddSnapPoint((MetricTimeSpan)snapPointTime3, "Data");
                    var snapPoint = playback.Snapping.AddSnapPoint(new MetricTimeSpan(0, 0, 0, 1100), "Data");
                    snapPoint.IsEnabled = false;
                },
                afterStart: NoPlaybackAction,
                afterStop: (context, playback) => playback.MoveToNextSnapPoint(),
                afterResume: (context, playback) => CheckCurrentTime(playback, snapPointTime1, "stopped"),
                runningAfterResume: new[]
                {
                    Tuple.Create<TimeSpan, PlaybackAction>(firstAfterResumeDelay, (context, playback) => CheckCurrentTime(playback, snapPointTime1 + firstAfterResumeDelay, "resumed")),
                    Tuple.Create<TimeSpan, PlaybackAction>(secondAfterResumeDelay, (context, playback) =>
                    {
                        playback.MoveToNextSnapPoint();
                        CheckCurrentTime(playback, snapPointTime3, "resumed");
                    }),
                    Tuple.Create<TimeSpan, PlaybackAction>(thirdAfterResumeDelay, (context, playback) => CheckCurrentTime(playback, snapPointTime3 + thirdAfterResumeDelay, "resumed"))
                },
                explicitExpectedTimes: new[]
                {
                    TimeSpan.Zero,
                    endTime + stopPeriod - (snapPointTime1 - stopAfter) - (snapPointTime3 - (snapPointTime1 + firstAfterResumeDelay + secondAfterResumeDelay))
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void SnapToNotesStarts()
        {
            var stopAfter = TimeSpan.FromSeconds(2);
            var stopPeriod = TimeSpan.FromSeconds(1);

            SnapPointsGroup snapPointsGroup = null;

            var snapPointTime1 = TimeSpan.FromSeconds(1);
            var snapPointTime2 = TimeSpan.FromSeconds(3);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(300);
            var secondAfterResumeDelay = TimeSpan.FromMilliseconds(600);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(200);

            var endTime = snapPointTime2 + TimeSpan.FromMilliseconds(1200);

            CheckPlaybackStop(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)100), snapPointTime1),
                    new EventToSend(new NoteOnEvent((SevenBitNumber)10, (SevenBitNumber)100), snapPointTime2 - snapPointTime1),
                    new EventToSend(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)100), TimeSpan.FromSeconds(1)),
                    new EventToSend(new NoteOffEvent((SevenBitNumber)10, (SevenBitNumber)100), TimeSpan.FromMilliseconds(200))
                },
                eventsWillBeSent: new EventToSend[] { },
                stopAfter: stopAfter,
                stopPeriod: stopPeriod,
                setupPlayback: (context, playback) =>
                {
                    snapPointsGroup = playback.Snapping.SnapToNotesStarts();
                    var snapPointsGroup2 = playback.Snapping.SnapToNotesStarts();
                    Assert.That(playback.Snapping.SnapPoints, Has.Count.EqualTo(2), "Count of snap points is invalid.");
                    Assert.AreSame(snapPointsGroup, snapPointsGroup2, "Snapping to notes starts creates new snap points group.");
                },
                afterStart: NoPlaybackAction,
                afterStop: (context, playback) => playback.MoveToPreviousSnapPoint(snapPointsGroup),
                afterResume: (context, playback) => CheckCurrentTime(playback, snapPointTime1, "stopped"),
                runningAfterResume: new[]
                {
                    Tuple.Create<TimeSpan, PlaybackAction>(firstAfterResumeDelay, (context, playback) => CheckCurrentTime(playback, snapPointTime1 + firstAfterResumeDelay, "resumed")),
                    Tuple.Create<TimeSpan, PlaybackAction>(secondAfterResumeDelay, (context, playback) =>
                    {
                        playback.MoveToNextSnapPoint(snapPointsGroup);
                        CheckCurrentTime(playback, snapPointTime2, "resumed");
                    }),
                    Tuple.Create<TimeSpan, PlaybackAction>(thirdAfterResumeDelay, (context, playback) => CheckCurrentTime(playback, snapPointTime2 + thirdAfterResumeDelay, "resumed"))
                },
                explicitExpectedTimes: new[]
                {
                    snapPointTime1,
                    stopAfter + stopPeriod,
                    stopAfter + stopPeriod + firstAfterResumeDelay + secondAfterResumeDelay,
                    stopAfter + stopPeriod + firstAfterResumeDelay + secondAfterResumeDelay + TimeSpan.FromSeconds(1),
                    endTime + stopPeriod + (stopAfter - snapPointTime1) - (snapPointTime2 - (snapPointTime1 + firstAfterResumeDelay + secondAfterResumeDelay))
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void SnapToNotesEnds()
        {
            var stopAfter = TimeSpan.FromSeconds(2);
            var stopPeriod = TimeSpan.FromSeconds(1);

            SnapPointsGroup snapPointsGroup = null;

            var snapPointTime1 = TimeSpan.FromSeconds(1);
            var snapPointTime2 = TimeSpan.FromSeconds(3);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(300);
            var secondAfterResumeDelay = TimeSpan.FromMilliseconds(600);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(200);

            var endTime = snapPointTime2 + TimeSpan.FromMilliseconds(200);

            CheckPlaybackStop(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)100), TimeSpan.FromMilliseconds(200)),
                    new EventToSend(new NoteOnEvent((SevenBitNumber)10, (SevenBitNumber)100), TimeSpan.FromMilliseconds(300)),
                    new EventToSend(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)100), snapPointTime1 - TimeSpan.FromMilliseconds(500)),
                    new EventToSend(new NoteOffEvent((SevenBitNumber)10, (SevenBitNumber)100), snapPointTime2 - snapPointTime1),
                    new EventToSend(new PitchBendEvent(100), TimeSpan.FromMilliseconds(200))
                },
                eventsWillBeSent: new EventToSend[] { },
                stopAfter: stopAfter,
                stopPeriod: stopPeriod,
                setupPlayback: (context, playback) =>
                {
                    snapPointsGroup = playback.Snapping.SnapToNotesEnds();
                    var snapPointsGroup2 = playback.Snapping.SnapToNotesEnds();
                    Assert.That(playback.Snapping.SnapPoints, Has.Count.EqualTo(2), "Count of snap points is invalid.");
                    Assert.AreSame(snapPointsGroup, snapPointsGroup2, "Snapping to notes starts creates new snap points group.");
                },
                afterStart: NoPlaybackAction,
                afterStop: (context, playback) => playback.MoveToPreviousSnapPoint(snapPointsGroup),
                afterResume: (context, playback) => CheckCurrentTime(playback, snapPointTime1, "stopped"),
                runningAfterResume: new[]
                {
                    Tuple.Create<TimeSpan, PlaybackAction>(firstAfterResumeDelay, (context, playback) => CheckCurrentTime(playback, snapPointTime1 + firstAfterResumeDelay, "resumed")),
                    Tuple.Create<TimeSpan, PlaybackAction>(secondAfterResumeDelay, (context, playback) =>
                    {
                        playback.MoveToNextSnapPoint(snapPointsGroup);
                        CheckCurrentTime(playback, snapPointTime2, "resumed");
                    }),
                    Tuple.Create<TimeSpan, PlaybackAction>(thirdAfterResumeDelay, (context, playback) => CheckCurrentTime(playback, snapPointTime2 + thirdAfterResumeDelay, "resumed"))
                },
                explicitExpectedTimes: new[]
                {
                    TimeSpan.FromMilliseconds(200),
                    TimeSpan.FromMilliseconds(500),
                    snapPointTime1,
                    stopAfter + stopPeriod,
                    stopAfter + stopPeriod + firstAfterResumeDelay + secondAfterResumeDelay,
                    endTime + stopPeriod + (stopAfter - snapPointTime1) - (snapPointTime2 - (snapPointTime1 + firstAfterResumeDelay + secondAfterResumeDelay))
                });
        }

        #endregion

        #region Private methods

        private IEnumerable<SnapPoint> GetActiveSnapPoints(Playback playback)
        {
            var time = TimeSpan.Zero;
            var result = new List<SnapPoint>();
            SnapPoint snapPoint = null;

            while ((snapPoint = playback.Snapping.GetNextSnapPoint(time)) != null)
            {
                result.Add(snapPoint);
                time = snapPoint.Time;
            }

            return result;
        }

        private void CheckTrackNotes(
            ICollection<EventToSend> eventsToSend,
            ICollection<EventToSend> eventsWillBeSent,
            TimeSpan moveFrom,
            TimeSpan moveTo,
            IEnumerable<int> notesWillBeStarted,
            IEnumerable<int> notesWillBeFinished,
            bool useOutputDevice)
        {
            if (useOutputDevice)
                CheckTrackNotesWithOutputDevice(eventsToSend, eventsWillBeSent, moveFrom, moveTo, notesWillBeStarted, notesWillBeFinished);
            else
                CheckTrackNotesWithoutOutputDevice(eventsToSend, eventsWillBeSent, moveFrom, moveTo, notesWillBeStarted, notesWillBeFinished);
        }

        private void CheckTrackNotesWithOutputDevice(
            ICollection<EventToSend> eventsToSend,
            ICollection<EventToSend> eventsWillBeSent,
            TimeSpan moveFrom,
            TimeSpan moveTo,
            IEnumerable<int> notesWillBeStarted,
            IEnumerable<int> notesWillBeFinished)
        {
            var playbackContext = new PlaybackContext();

            var receivedEvents = playbackContext.ReceivedEvents;
            var sentEvents = playbackContext.SentEvents;
            var stopwatch = playbackContext.Stopwatch;
            var tempoMap = playbackContext.TempoMap;

            var eventsForPlayback = GetEventsForPlayback(eventsToSend, tempoMap);

            var notes = eventsForPlayback.GetNotes().ToArray();
            var notesStarted = new List<Note>();
            var notesFinished = new List<Note>();

            using (var outputDevice = OutputDevice.GetByName(SendReceiveUtilities.DeviceToTestOnName))
            {
                SendReceiveUtilities.WarmUpDevice(outputDevice);
                outputDevice.EventSent += (_, e) => sentEvents.Add(new SentEvent(e.Event, stopwatch.Elapsed));

                using (var playback = new Playback(eventsForPlayback, tempoMap, outputDevice))
                {
                    playback.TrackNotes = true;
                    playback.NotesPlaybackStarted += (_, e) => notesStarted.AddRange(e.Notes);
                    playback.NotesPlaybackFinished += (_, e) => notesFinished.AddRange(e.Notes);

                    using (var inputDevice = InputDevice.GetByName(SendReceiveUtilities.DeviceToTestOnName))
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

                        SpinWait.SpinUntil(() => stopwatch.Elapsed >= moveFrom);
                        playback.MoveToTime((MetricTimeSpan)moveTo);

                        var timeout = TimeSpan.FromTicks(eventsWillBeSent.Sum(e => e.Delay.Ticks)) + SendReceiveUtilities.MaximumEventSendReceiveDelay;
                        var areEventsReceived = SpinWait.SpinUntil(() => receivedEvents.Count == eventsWillBeSent.Count, timeout);
                        Assert.IsTrue(areEventsReceived, $"Events are not received for timeout {timeout}.");

                        stopwatch.Stop();

                        var playbackStopped = SpinWait.SpinUntil(() => !playback.IsRunning, SendReceiveUtilities.MaximumEventSendReceiveDelay);
                        Assert.IsTrue(playbackStopped, "Playback is running after completed.");
                    }
                }
            }

            CompareSentReceivedEvents(sentEvents, receivedEvents, eventsWillBeSent.ToList());

            Assert.IsTrue(NoteEquality.AreEqual(notesStarted, notesWillBeStarted.Select(i => notes[i])), "Invalid notes started.");
            Assert.IsTrue(NoteEquality.AreEqual(notesFinished, notesWillBeFinished.Select(i => notes[i])), "Invalid notes finished.");
        }

        private void CheckTrackNotesWithoutOutputDevice(
            ICollection<EventToSend> eventsToSend,
            ICollection<EventToSend> eventsWillBeSent,
            TimeSpan moveFrom,
            TimeSpan moveTo,
            IEnumerable<int> notesWillBeStarted,
            IEnumerable<int> notesWillBeFinished)
        {
            var playbackContext = new PlaybackContext();

            var stopwatch = playbackContext.Stopwatch;
            var tempoMap = playbackContext.TempoMap;

            var eventsForPlayback = GetEventsForPlayback(eventsToSend, tempoMap);

            var notes = eventsForPlayback.GetNotes().ToArray();
            var notesStarted = new List<Note>();
            var notesFinished = new List<Note>();

            using (var playback = new Playback(eventsForPlayback, tempoMap))
            {
                playback.TrackNotes = true;
                playback.NotesPlaybackStarted += (_, e) => notesStarted.AddRange(e.Notes);
                playback.NotesPlaybackFinished += (_, e) => notesFinished.AddRange(e.Notes);

                stopwatch.Start();
                playback.Start();

                SpinWait.SpinUntil(() => stopwatch.Elapsed >= moveFrom);
                playback.MoveToTime((MetricTimeSpan)moveTo);

                Thread.Sleep(TimeSpan.FromTicks(eventsWillBeSent.Sum(e => e.Delay.Ticks)) + SendReceiveUtilities.MaximumEventSendReceiveDelay);

                stopwatch.Stop();

                var playbackStopped = SpinWait.SpinUntil(() => !playback.IsRunning, SendReceiveUtilities.MaximumEventSendReceiveDelay);
                Assert.IsTrue(playbackStopped, "Playback is running after completed.");
            }

            Assert.IsTrue(NoteEquality.AreEqual(notesStarted, notesWillBeStarted.Select(i => notes[i])), "Invalid notes started.");
            Assert.IsTrue(NoteEquality.AreEqual(notesFinished, notesWillBeFinished.Select(i => notes[i])), "Invalid notes finished.");
        }

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

            using (var outputDevice = OutputDevice.GetByName(SendReceiveUtilities.DeviceToTestOnName))
            {
                SendReceiveUtilities.WarmUpDevice(outputDevice);
                outputDevice.EventSent += (_, e) => sentEvents.Add(new SentEvent(e.Event, stopwatch.Elapsed));

                using (var playback = new Playback(eventsForPlayback, tempoMap, outputDevice))
                {
                    playback.Speed = speed;
                    beforePlaybackStarted(playbackContext, playback);

                    using (var inputDevice = InputDevice.GetByName(SendReceiveUtilities.DeviceToTestOnName))
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

            var eventsForPlayback = GetEventsForPlayback(eventsToSend, tempoMap);
            var expectedTimes = playbackContext.ExpectedTimes;

            if (explicitExpectedTimes != null)
                expectedTimes.AddRange(explicitExpectedTimes);
            else
            {
                var currentTime = TimeSpan.Zero;

                foreach (var eventWillBeSent in eventsWillBeSent)
                {
                    currentTime += eventWillBeSent.Delay;
                    var scaledCurrentTime = ScaleTimeSpan(currentTime, 1.0 / speed);
                    expectedTimes.Add(currentTime > stopAfter ? scaledCurrentTime + stopPeriod : scaledCurrentTime);
                }
            }

            using (var outputDevice = OutputDevice.GetByName(SendReceiveUtilities.DeviceToTestOnName))
            {
                SendReceiveUtilities.WarmUpDevice(outputDevice);
                outputDevice.EventSent += (_, e) => sentEvents.Add(new SentEvent(e.Event, stopwatch.Elapsed));

                using (var playback = new Playback(eventsForPlayback, tempoMap, outputDevice))
                {
                    playback.Speed = speed;
                    setupPlayback(playbackContext, playback);

                    using (var inputDevice = InputDevice.GetByName(SendReceiveUtilities.DeviceToTestOnName))
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

        private static IEnumerable<MidiEvent> GetEventsForPlayback(IEnumerable<EventToSend> eventsToSend, TempoMap tempoMap)
        {
            var eventsForPlayback = new List<MidiEvent>();
            var currentTime = TimeSpan.Zero;

            foreach (var eventToSend in eventsToSend)
            {
                var midiEvent = eventToSend.Event.Clone();
                midiEvent.DeltaTime = LengthConverter.ConvertFrom((MetricTimeSpan)eventToSend.Delay, (MetricTimeSpan)currentTime, tempoMap);
                currentTime += eventToSend.Delay;
                eventsForPlayback.Add(midiEvent);
            }

            return eventsForPlayback;
        }

        private void CompareSentReceivedEvents(
            IReadOnlyList<SentEvent> sentEvents,
            IReadOnlyList<ReceivedEvent> receivedEvents,
            IReadOnlyList<EventToSend> expectedEvents)
        {
            var currentTime = TimeSpan.Zero;

            for (var i = 0; i < sentEvents.Count; i++)
            {
                var sentEvent = sentEvents[i];
                var receivedEvent = receivedEvents[i];
                var expectedEvent = expectedEvents[i];
                var expectedTime = (currentTime += expectedEvent.Delay);

                Assert.IsTrue(
                    MidiEventEquality.AreEqual(sentEvent.Event, expectedEvent.Event, false),
                    $"Sent event {sentEvent.Event} doesn't match expected one {expectedEvent.Event}.");

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
