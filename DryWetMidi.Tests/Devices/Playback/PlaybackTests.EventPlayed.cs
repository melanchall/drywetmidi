using System;
using System.Diagnostics;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Devices
{
    [TestFixture]
    public sealed partial class PlaybackTests
    {
        #region Test methods

        [Retry(RetriesNumber)]
        [Test]
        public void EventPlayed()
        {
            CheckEventPlayedEvent(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOnEvent(), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(), TimeSpan.FromSeconds(1)),
                    new EventToSend(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)80), TimeSpan.FromMilliseconds(500)),
                    new EventToSend(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)0), TimeSpan.FromMilliseconds(500))
                },
                expectedPlayedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromSeconds(1)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)80), TimeSpan.FromMilliseconds(1500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)0), TimeSpan.FromMilliseconds(2000))
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void EventPlayed_AllEventsTypes()
        {
            var delay = 10;
            var eventsToSend = TypesProvider
                .GetAllEventTypes()
                .Select(type => type == typeof(UnknownMetaEvent)
                    ? new UnknownMetaEvent(0)
                    : (MidiEvent)Activator.CreateInstance(type))
                .Select(midiEvent => new EventToSend(midiEvent, TimeSpan.FromMilliseconds(delay)))
                .ToArray();

            CheckEventPlayedEvent(
                eventsToSend: eventsToSend,
                expectedPlayedEvents: eventsToSend.Select((e, i) => new ReceivedEvent(e.Event, TimeSpan.FromMilliseconds(delay * (i + 1)))).ToArray());
        }

        [Retry(RetriesNumber)]
        [Test]
        public void EventPlayed_InterruptNotesOnStop()
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
                setupPlayback: (context, playback) =>
                {
                    playback.InterruptNotesOnStop = true;
                },
                afterStart: NoPlaybackAction,
                afterStop: NoPlaybackAction,
                afterResume: NoPlaybackAction,
                expectedPlayedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), noteOnDelay),
                    new ReceivedEvent(new NoteOffEvent(), stopAfter),
                    new ReceivedEvent(new NoteOffEvent(), noteOnDelay + stopAfter + stopPeriod + noteOffDelay - stopAfter)
                });
        }

        [Retry(RetriesNumber)]
        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(0.5)]
        public void EventPlayed_MoveToStart(double speed)
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
                speed: speed,
                expectedPlayedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), firstEventTime),
                    new ReceivedEvent(new NoteOnEvent(), stopAfter + stopPeriod),
                    new ReceivedEvent(new NoteOnEvent(), stopAfter + stopPeriod + firstAfterResumeDelay + secondAfterResumeDelay),
                    new ReceivedEvent(new NoteOffEvent(), stopAfter + stopPeriod + firstAfterResumeDelay + secondAfterResumeDelay + ScaleTimeSpan(lastEventTime, 1.0 / speed))
                });
        }

        [Retry(RetriesNumber)]
        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(0.5)]
        public void EventPlayed_MoveForward(double speed)
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
                speed: speed,
                expectedPlayedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), firstEventTime),
                    new ReceivedEvent(new NoteOffEvent(), ScaleTimeSpan(lastEventTime - stepAfterStop - stepAfterResumed, 1.0 / speed) + stopPeriod)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void EventPlayed_MoveForward_BeyondDuration()
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
                expectedPlayedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(), stopAfter + stopPeriod)
                });
        }

        [Retry(RetriesNumber)]
        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(0.5)]
        public void EventPlayed_MoveBack(double speed)
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
                speed: speed,
                expectedPlayedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(), ScaleTimeSpan(lastEventTime + stepAfterStop + stepAfterResumed, 1.0 / speed) + stopPeriod)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void EventPlayed_MoveBack_BeyondZero()
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
                expectedPlayedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOnEvent(), stopAfter + stopPeriod),
                    new ReceivedEvent(new NoteOffEvent(), lastEventTime + stopAfter + stopPeriod + stepAfterResumed)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void EventPlayed_MoveToTime()
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
                expectedPlayedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromSeconds(11))
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void EventPlayed_MoveToTime_BeyondDuration()
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
                expectedPlayedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(), stopAfter + stopPeriod)
                });
        }

        #endregion
    }
}
