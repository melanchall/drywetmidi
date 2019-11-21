using System;
using System.Diagnostics;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Devices
{
    [TestFixture]
    public sealed partial class PlaybackTests
    {
        #region Constants

        private static readonly SevenBitNumber TransposeBy = (SevenBitNumber)20;

        private static readonly NoteCallback NoteCallback = (d, rt, rl, t) =>
        {
            return new NotePlaybackData((SevenBitNumber)(d.NoteNumber + TransposeBy), d.Velocity, d.OffVelocity, d.Channel);
        };

        #endregion

        #region Test methods

        [Retry(RetriesNumber)]
        [Test]
        public void NoteCallback_ReturnNull_ReturnSkipNote()
        {
            CheckNoteCallback(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOnEvent(), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(), TimeSpan.FromSeconds(1)),
                    new EventToSend(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)80), TimeSpan.FromMilliseconds(500)),
                    new EventToSend(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)0), TimeSpan.FromMilliseconds(500))
                },
                expectedReceivedEvents: new ReceivedEvent[] { },
                changeCallbackAfter: TimeSpan.FromMilliseconds(500),
                noteCallback: (d, rt, rl, t) => null,
                secondNoteCallback: (d, rt, rl, t) => NotePlaybackData.SkipNote,
                expectedNotesStarted: new Note[] { },
                expectedNotesFinished: new Note[] { });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void NoteCallback_ReturnNull_ReturnOriginal()
        {
            var note = new Note((SevenBitNumber)100)
            {
                Velocity = (SevenBitNumber)80
            }
            .SetTimeAndLength((MetricTimeSpan)TimeSpan.FromMilliseconds(1500), (MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap.Default);

            CheckNoteCallback(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOnEvent(), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(), TimeSpan.FromSeconds(1)),
                    new EventToSend(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)80), TimeSpan.FromMilliseconds(500)),
                    new EventToSend(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)0), TimeSpan.FromMilliseconds(500))
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)80), TimeSpan.FromMilliseconds(1500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)0), TimeSpan.FromMilliseconds(2000))
                },
                changeCallbackAfter: TimeSpan.FromMilliseconds(500),
                noteCallback: (d, rt, rl, t) => null,
                secondNoteCallback: (d, rt, rl, t) => d,
                expectedNotesStarted: new[] { note },
                expectedNotesFinished: new[] { note });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void NoteCallback_ReturnOriginal_ReturnNull()
        {
            var note = new Note(SevenBitNumber.MinValue)
            {
                Velocity = SevenBitNumber.MinValue
            }
            .SetTimeAndLength((MetricTimeSpan)TimeSpan.Zero, (MetricTimeSpan)TimeSpan.FromSeconds(1), TempoMap.Default);

            CheckNoteCallback(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOnEvent(), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(), TimeSpan.FromSeconds(1)),
                    new EventToSend(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)80), TimeSpan.FromMilliseconds(500)),
                    new EventToSend(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)0), TimeSpan.FromMilliseconds(500))
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromSeconds(1))
                },
                changeCallbackAfter: TimeSpan.FromMilliseconds(500),
                noteCallback: (d, rt, rl, t) => d,
                secondNoteCallback: (d, rt, rl, t) => null,
                expectedNotesStarted: new[] { note },
                expectedNotesFinished: new[] { note });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void NoteCallback_Transpose()
        {
            var note1 = new Note(TransposeBy)
            {
                Velocity = SevenBitNumber.MinValue
            }
            .SetTimeAndLength((MetricTimeSpan)TimeSpan.Zero, (MetricTimeSpan)TimeSpan.FromSeconds(1), TempoMap.Default);

            var note2 = new Note((SevenBitNumber)(100 + TransposeBy))
            {
                Velocity = (SevenBitNumber)80
            }
            .SetTimeAndLength((MetricTimeSpan)TimeSpan.FromMilliseconds(1500), (MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap.Default);

            CheckNoteCallback(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOnEvent(), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(), TimeSpan.FromSeconds(1)),
                    new EventToSend(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)80), TimeSpan.FromMilliseconds(500)),
                    new EventToSend(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)0), TimeSpan.FromMilliseconds(500))
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), TimeSpan.FromSeconds(1)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)(100 + TransposeBy), (SevenBitNumber)80), TimeSpan.FromMilliseconds(1500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)(100 + TransposeBy), (SevenBitNumber)0), TimeSpan.FromMilliseconds(2000))
                },
                changeCallbackAfter: TimeSpan.FromMilliseconds(500),
                noteCallback: NoteCallback,
                secondNoteCallback: NoteCallback,
                expectedNotesStarted: new[] { note1, note2 },
                expectedNotesFinished: new[] { note1, note2 });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void NoteCallback_InterruptNotesOnStop()
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
                    new EventToSend(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), noteOnDelay),
                    new EventToSend(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), stopAfter),
                    new EventToSend(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), noteOffDelay - stopAfter)
                },
                stopAfter: stopAfter,
                stopPeriod: stopPeriod,
                setupPlayback: (context, playback) =>
                {
                    playback.InterruptNotesOnStop = true;
                    playback.NoteCallback = NoteCallback;
                },
                afterStart: NoPlaybackAction,
                afterStop: NoPlaybackAction,
                afterResume: NoPlaybackAction,
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), noteOnDelay),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), stopAfter),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), noteOnDelay + stopAfter + stopPeriod + noteOffDelay - stopAfter)
                });
        }

        [Retry(RetriesNumber)]
        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(0.5)]
        public void NoteCallback_MoveToStart(double speed)
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
                setupPlayback: (context, playback) => playback.NoteCallback = NoteCallback,
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
                speed: speed,
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), firstEventTime),
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), stopAfter + stopPeriod),
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), stopAfter + stopPeriod + firstAfterResumeDelay + secondAfterResumeDelay),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), stopAfter + stopPeriod + firstAfterResumeDelay + secondAfterResumeDelay + ScaleTimeSpan(lastEventTime, 1.0 / speed))
                });
        }

        [Retry(RetriesNumber)]
        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(0.5)]
        public void NoteCallback_MoveForward(double speed)
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
                setupPlayback: (context, playback) => playback.NoteCallback = NoteCallback,
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
                speed: speed,
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), firstEventTime),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), ScaleTimeSpan(lastEventTime - stepAfterStop - stepAfterResumed, 1.0 / speed) + stopPeriod)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void NoteCallback_MoveForward_BeyondDuration()
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
                setupPlayback: (context, playback) => playback.NoteCallback = NoteCallback,
                afterStart: NoPlaybackAction,
                afterStop: (context, playback) => playback.MoveForward((MetricTimeSpan)stepAfterStop),
                afterResume: (context, playback) => CheckCurrentTime(playback, TimeSpan.FromSeconds(4), "stopped"),
                runningAfterResume: null,
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), stopAfter + stopPeriod)
                });
        }

        [Retry(RetriesNumber)]
        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(0.5)]
        public void NoteCallback_MoveBack(double speed)
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
                setupPlayback: (context, playback) => playback.NoteCallback = NoteCallback,
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
                speed: speed,
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), ScaleTimeSpan(lastEventTime + stepAfterStop + stepAfterResumed, 1.0 / speed) + stopPeriod)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void NoteCallback_MoveBack_BeyondZero()
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
                setupPlayback: (context, playback) => playback.NoteCallback = NoteCallback,
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
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), stopAfter + stopPeriod),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), lastEventTime + stopAfter + stopPeriod + stepAfterResumed)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void NoteCallback_MoveToTime()
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
                setupPlayback: (context, playback) => playback.NoteCallback = NoteCallback,
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
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), TimeSpan.FromSeconds(11))
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void NoteCallback_MoveToTime_BeyondDuration()
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
                setupPlayback: (context, playback) => playback.NoteCallback = NoteCallback,
                afterStart: NoPlaybackAction,
                afterStop: (context, playback) => playback.MoveToTime(new MetricTimeSpan(0, 0, 10)),
                afterResume: (context, playback) => CheckCurrentTime(playback, TimeSpan.FromSeconds(4), "stopped"),
                runningAfterResume: null,
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), stopAfter + stopPeriod)
                });
        }

        #endregion
    }
}
