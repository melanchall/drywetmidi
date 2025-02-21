using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Common;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Multimedia
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
                    : (MidiEvent)Activator.CreateInstance(type, true))
                .Select(midiEvent =>
                {
                    if (midiEvent is SetTempoEvent)
                        midiEvent = new SetTempoEvent(500001);

                    if (midiEvent is TimeSignatureEvent)
                        midiEvent = new TimeSignatureEvent(3, 4);

                    return new EventToSend(midiEvent, TimeSpan.FromMilliseconds(delay));
                })
                .OrderBy(e => e.Event.EventType == MidiEventType.NoteOn ? -1 : (int)e.Event.EventType)
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

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)noteOnDelay, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)(noteOnDelay + noteOffDelay), TempoMap),
                },
                actions: new PlaybackChangerBase[]
                {
                    new PlaybackChangerBase(stopAfter, p => p.Stop()),
                    new PlaybackChangerBase(stopPeriod, p => p.Start()),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), noteOnDelay),
                    new ReceivedEvent(new NoteOffEvent(), stopAfter),
                    new ReceivedEvent(new NoteOffEvent(), noteOnDelay + stopAfter + stopPeriod + noteOffDelay - stopAfter)
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = false;
                    playback.InterruptNotesOnStop = true;
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

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)firstEventTime, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)(firstEventTime + lastEventTime), TempoMap),
                },
                actions: new PlaybackChangerBase[]
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
                    new PlaybackChangerBase(firstAfterResumeDelay,
                        p => CheckCurrentTime(p, ScaleTimeSpan(firstAfterResumeDelay, speed), "resumed")),
                    new PlaybackChangerBase(secondAfterResumeDelay,
                        p =>
                        {
                            p.MoveToStart();
                            CheckCurrentTime(p, TimeSpan.Zero, "resumed");
                        }),
                    new PlaybackChangerBase(thirdAfterResumeDelay,
                        p => CheckCurrentTime(p, ScaleTimeSpan(thirdAfterResumeDelay, speed), "resumed"))
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), firstEventTime),
                    new ReceivedEvent(new NoteOnEvent(), stopAfter + stopPeriod),
                    new ReceivedEvent(new NoteOnEvent(), stopAfter + stopPeriod + firstAfterResumeDelay + secondAfterResumeDelay),
                    new ReceivedEvent(new NoteOffEvent(), stopAfter + stopPeriod + firstAfterResumeDelay + secondAfterResumeDelay + ScaleTimeSpan(lastEventTime, 1.0 / speed))
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;
                    playback.TrackNotes = false;
                    playback.Speed = speed;
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

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)firstEventTime, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)(firstEventTime + lastEventTime), TempoMap),
                },
                actions: new PlaybackChangerBase[]
                {
                    new PlaybackChangerBase(stopAfter, p =>
                    {
                        p.Stop();
                        p.MoveForward((MetricTimeSpan)stepAfterStop);
                    }),
                    new PlaybackChangerBase(stopPeriod, p =>
                    {
                        p.Start();
                    }),
                    new PlaybackChangerBase(firstAfterResumeDelay,
                        p => CheckCurrentTime(p, ScaleTimeSpan(stopAfter + firstAfterResumeDelay, speed) + stepAfterStop, "resumed")),
                    new PlaybackChangerBase(secondAfterResumeDelay,
                        p =>
                        {
                            p.MoveForward((MetricTimeSpan)stepAfterResumed);
                            CheckCurrentTime(p, ScaleTimeSpan(stopAfter + firstAfterResumeDelay + secondAfterResumeDelay, speed) + stepAfterStop + stepAfterResumed, "resumed");
                        }),
                    new PlaybackChangerBase(thirdAfterResumeDelay,
                        p => CheckCurrentTime(p, ScaleTimeSpan(stopAfter + firstAfterResumeDelay + secondAfterResumeDelay + thirdAfterResumeDelay, speed) + stepAfterStop + stepAfterResumed, "resumed"))
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), firstEventTime),
                    new ReceivedEvent(new NoteOffEvent(), ScaleTimeSpan(lastEventTime - stepAfterStop - stepAfterResumed, 1.0 / speed) + stopPeriod)
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;
                    playback.TrackNotes = false;
                    playback.Speed = speed;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void EventPlayed_MoveForward_BeyondDuration()
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
                actions: new PlaybackChangerBase[]
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
                    new ReceivedEvent(new NoteOffEvent(), stopAfter + stopPeriod)
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;
                    playback.TrackNotes = false;
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

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new PlaybackChangerBase[]
                {
                    new PlaybackChangerBase(stopAfter, p =>
                    {
                        p.Stop();
                        p.MoveBack((MetricTimeSpan)stepAfterStop);
                    }),
                    new PlaybackChangerBase(stopPeriod, p =>
                    {
                        p.Start();
                    }),
                    new PlaybackChangerBase(firstAfterResumeDelay,
                        p => CheckCurrentTime(p, ScaleTimeSpan(stopAfter + firstAfterResumeDelay, speed) - stepAfterStop, "resumed")),
                    new PlaybackChangerBase(secondAfterResumeDelay,
                        p =>
                        {
                            p.MoveBack((MetricTimeSpan)stepAfterResumed);
                            CheckCurrentTime(p, ScaleTimeSpan(stopAfter + firstAfterResumeDelay + secondAfterResumeDelay, speed) - stepAfterStop - stepAfterResumed, "resumed");
                        }),
                    new PlaybackChangerBase(thirdAfterResumeDelay,
                        p => CheckCurrentTime(p, ScaleTimeSpan(stopAfter + firstAfterResumeDelay + secondAfterResumeDelay + thirdAfterResumeDelay, speed) - stepAfterStop - stepAfterResumed, "resumed"))
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(), ScaleTimeSpan(lastEventTime + stepAfterStop + stepAfterResumed, 1.0 / speed) + stopPeriod)
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;
                    playback.TrackNotes = false;
                    playback.Speed = speed;
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

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new PlaybackChangerBase[]
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
                    new PlaybackChangerBase(firstAfterResumeDelay,
                        p => CheckCurrentTime(p, firstAfterResumeDelay, "resumed")),
                    new PlaybackChangerBase(secondAfterResumeDelay,
                        p =>
                        {
                            p.MoveBack((MetricTimeSpan)stepAfterResumed);
                            CheckCurrentTime(p, firstAfterResumeDelay + secondAfterResumeDelay - stepAfterResumed, "resumed");
                        }),
                    new PlaybackChangerBase(thirdAfterResumeDelay,
                        p => CheckCurrentTime(p, firstAfterResumeDelay + secondAfterResumeDelay + thirdAfterResumeDelay - stepAfterResumed, "resumed"))
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOnEvent(), stopAfter + stopPeriod),
                    new ReceivedEvent(new NoteOffEvent(), lastEventTime + stopAfter + stopPeriod + stepAfterResumed)
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;
                    playback.TrackNotes = false;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void EventPlayed_MoveToTime()
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
                actions: new PlaybackChangerBase[]
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
                    new PlaybackChangerBase(TimeSpan.FromSeconds(1),
                        p => CheckCurrentTime(p, TimeSpan.FromSeconds(2), "resumed")),
                    new PlaybackChangerBase(TimeSpan.FromSeconds(2),
                        p =>
                        {
                            p.MoveToTime(new MetricTimeSpan(0, 0, 8));
                            CheckCurrentTime(p, TimeSpan.FromSeconds(8), "resumed");
                        }),
                    new PlaybackChangerBase(TimeSpan.FromSeconds(1),
                        p => CheckCurrentTime(p, TimeSpan.FromSeconds(9), "resumed"))
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromSeconds(11))
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;
                    playback.TrackNotes = false;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void EventPlayed_MoveToTime_BeyondDuration()
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
                actions: new PlaybackChangerBase[]
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
                    new ReceivedEvent(new NoteOffEvent(), stopAfter + stopPeriod)
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;
                    playback.TrackNotes = false;
                });
        }

        #endregion

        #region Private methods

        private void CheckEventPlayedEvent(
            ICollection<EventToSend> eventsToSend,
            ICollection<ReceivedEvent> expectedPlayedEvents)
        {
            var playbackContext = new PlaybackContext();

            var playedEvents = playbackContext.ReceivedEvents;
            var sentEvents = playbackContext.SentEvents;
            var stopwatch = playbackContext.Stopwatch;
            var tempoMap = playbackContext.TempoMap;

            var eventsForPlayback = GetEventsForPlayback(eventsToSend, tempoMap);

            using (var playback = eventsForPlayback.GetPlayback(tempoMap))
            {
                playback.EventPlayed += (_, e) =>
                {
                    lock (playbackContext.ReceivedEventsLockObject)
                    {
                        playedEvents.Add(new ReceivedEvent(e.Event, stopwatch.Elapsed));
                    }
                };

                stopwatch.Start();
                playback.Start();

                var timeout = TimeSpan.FromTicks(eventsToSend.Sum(e => e.Delay.Ticks)) + SendReceiveUtilities.MaximumEventSendReceiveDelay;
                var playbackStopped = WaitOperations.Wait(() => !playback.IsRunning, timeout);
                Assert.IsTrue(playbackStopped, "Playback is running after completed.");
            }

            CompareReceivedEvents(playedEvents, expectedPlayedEvents.ToList());
        }

        #endregion
    }
}
