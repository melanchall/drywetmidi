using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;

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
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(1000), TempoMap),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)80))
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(1500), TempoMap),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)0))
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(2000), TempoMap),
                },
                actions: Array.Empty<PlaybackAction>(),
                expectedReceivedEvents: new[]
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
            var delay = 30;
            var eventsToSend = TypesProvider
                .GetAllEventTypes()
                .Select(type => type == typeof(UnknownMetaEvent)
                    ? new UnknownMetaEvent(0)
                    : (MidiEvent)Activator.CreateInstance(type, true))
                .OrderBy(e =>
                {
                    var eventType = e.EventType;

                    if (eventType == MidiEventType.NoteOn)
                        return -1;
                    if (eventType == MidiEventType.SetTempo)
                        return 1000;

                    return (int)e.EventType;
                })
                .Select((midiEvent, i) =>
                {
                    if (midiEvent is SetTempoEvent)
                        midiEvent = new SetTempoEvent(500001);

                    if (midiEvent is TimeSignatureEvent)
                        midiEvent = new TimeSignatureEvent(3, 4);

                    return new TimedEvent(midiEvent)
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(delay * (i + 1)), TempoMap);
                })
                .ToArray();

            CheckEventPlayedEvent(
                initialPlaybackObjects: eventsToSend,
                actions: Array.Empty<PlaybackAction>(),
                expectedReceivedEvents: eventsToSend
                    .Select((e, i) => new ReceivedEvent(e.Event, TimeSpan.FromMilliseconds(delay * (i + 1))))
                    .ToArray());
        }

        [Retry(RetriesNumber)]
        [Test]
        public void EventPlayed_InterruptNotesOnStop()
        {
            var noteOnDelay = TimeSpan.Zero;
            var noteOffDelay = TimeSpan.FromSeconds(2);

            var stopAfter = TimeSpan.FromSeconds(1);
            var stopPeriod = TimeSpan.FromMilliseconds(400);

            CheckEventPlayedEvent(
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)noteOnDelay, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)(noteOnDelay + noteOffDelay), TempoMap),
                },
                actions: new PlaybackAction[]
                {
                    new PlaybackAction(stopAfter, p => p.Stop()),
                    new PlaybackAction(stopPeriod, p => p.Start()),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), noteOnDelay),
                    new ReceivedEvent(new NoteOffEvent(), stopAfter),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = false;
                    playback.InterruptNotesOnStop = true;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void EventPlayed_InterruptNotesOnStop_SendNoteOffEventsForNonActiveNotes()
        {
            var noteOnDelay = TimeSpan.Zero;
            var noteOffDelay = TimeSpan.FromSeconds(2);

            var stopAfter = TimeSpan.FromSeconds(1);
            var stopPeriod = TimeSpan.FromMilliseconds(400);

            CheckEventPlayedEvent(
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)noteOnDelay, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)(noteOnDelay + noteOffDelay), TempoMap),
                },
                actions: new PlaybackAction[]
                {
                    new PlaybackAction(stopAfter, p => p.Stop()),
                    new PlaybackAction(stopPeriod, p => p.Start()),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), noteOnDelay),
                    new ReceivedEvent(new NoteOffEvent(), stopAfter),
                    new ReceivedEvent(new NoteOffEvent(), noteOnDelay + stopAfter + stopPeriod + noteOffDelay - stopAfter)
                },
                setupPlayback: playback =>
                {
                    playback.SendNoteOffEventsForNonActiveNotes = true;
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
            var stopAfter = TimeSpan.FromMilliseconds(200);
            var stopPeriod = TimeSpan.FromMilliseconds(700);

            var firstEventTime = TimeSpan.Zero;
            var lastEventTime = TimeSpan.FromSeconds(2);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(200);
            var secondAfterResumeDelay = TimeSpan.FromMilliseconds(500);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(100);

            CheckEventPlayedEvent(
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)firstEventTime, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)(firstEventTime + lastEventTime), TempoMap),
                },
                actions: new PlaybackAction[]
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
                    new PlaybackAction(firstAfterResumeDelay,
                        p => CheckCurrentTime(p, ScaleTimeSpan(firstAfterResumeDelay, speed), "resumed")),
                    new PlaybackAction(secondAfterResumeDelay,
                        p =>
                        {
                            p.MoveToStart();
                            CheckCurrentTime(p, TimeSpan.Zero, "resumed");
                        }),
                    new PlaybackAction(thirdAfterResumeDelay,
                        p => CheckCurrentTime(p, ScaleTimeSpan(thirdAfterResumeDelay, speed), "resumed"))
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), firstEventTime),
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
        public void EventPlayed_MoveToStart_SendNoteOnEventsForActiveNotes(double speed)
        {
            var stopAfter = TimeSpan.FromMilliseconds(200);
            var stopPeriod = TimeSpan.FromMilliseconds(700);

            var firstEventTime = TimeSpan.Zero;
            var lastEventTime = TimeSpan.FromSeconds(2);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(200);
            var secondAfterResumeDelay = TimeSpan.FromMilliseconds(500);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(100);

            CheckEventPlayedEvent(
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)firstEventTime, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)(firstEventTime + lastEventTime), TempoMap),
                },
                actions: new PlaybackAction[]
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
                    new PlaybackAction(firstAfterResumeDelay,
                        p => CheckCurrentTime(p, ScaleTimeSpan(firstAfterResumeDelay, speed), "resumed")),
                    new PlaybackAction(secondAfterResumeDelay,
                        p =>
                        {
                            p.MoveToStart();
                            CheckCurrentTime(p, TimeSpan.Zero, "resumed");
                        }),
                    new PlaybackAction(thirdAfterResumeDelay,
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
                    playback.SendNoteOnEventsForActiveNotes = true;
                    playback.Speed = speed;
                });
        }

        [Retry(RetriesNumber)]
        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(0.5)]
        public void EventPlayed_MoveForward(double speed)
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

            CheckEventPlayedEvent(
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)firstEventTime, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)(firstEventTime + lastEventTime), TempoMap),
                },
                actions: new PlaybackAction[]
                {
                    new PlaybackAction(stopAfter, p =>
                    {
                        p.Stop();
                        p.MoveForward((MetricTimeSpan)stepAfterStop);
                    }),
                    new PlaybackAction(stopPeriod, p =>
                    {
                        p.Start();
                    }),
                    new PlaybackAction(firstAfterResumeDelay,
                        p => CheckCurrentTime(p, ScaleTimeSpan(stopAfter + firstAfterResumeDelay, speed) + stepAfterStop, "resumed")),
                    new PlaybackAction(secondAfterResumeDelay,
                        p =>
                        {
                            p.MoveForward((MetricTimeSpan)stepAfterResumed);
                            CheckCurrentTime(p, ScaleTimeSpan(stopAfter + firstAfterResumeDelay + secondAfterResumeDelay, speed) + stepAfterStop + stepAfterResumed, "resumed");
                        }),
                    new PlaybackAction(thirdAfterResumeDelay,
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

            CheckEventPlayedEvent(
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)TimeSpan.FromSeconds(4), TempoMap),
                },
                actions: new PlaybackAction[]
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
                    new ReceivedEvent(new NoteOffEvent(), stopAfter)
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = false;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void EventPlayed_MoveForward_BeyondPlaybackEnd()
        {
            var stopAfter = TimeSpan.FromMilliseconds(200);
            var stopPeriod = TimeSpan.FromMilliseconds(200);

            var stepAfterStop = TimeSpan.FromMilliseconds(300);

            CheckEventPlayedEvent(
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(400), TempoMap),
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(600), TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(700), TempoMap),
                },
                actions: new PlaybackAction[]
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
                    new ReceivedEvent(new NoteOffEvent(), stopAfter)
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
        public void EventPlayed_MoveBack(double speed)
        {
            var stopAfter = TimeSpan.FromMilliseconds(700);
            var stopPeriod = TimeSpan.FromMilliseconds(200);

            var stepAfterStop = TimeSpan.FromMilliseconds(300);
            ClassicAssert.LessOrEqual(stepAfterStop, ScaleTimeSpan(stopAfter, speed), "Step after stop is invalid.");

            var stepAfterResumed = TimeSpan.FromMilliseconds(100);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(200);
            var secondAfterResumeDelay = TimeSpan.FromMilliseconds(500);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(200);

            var lastEventTime = TimeSpan.FromMilliseconds(3000);
            ClassicAssert.GreaterOrEqual(lastEventTime, ScaleTimeSpan(stopAfter + firstAfterResumeDelay + secondAfterResumeDelay + thirdAfterResumeDelay, speed) - stepAfterStop - stepAfterResumed, "Last event time is invalid.");

            CheckEventPlayedEvent(
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
                        p.MoveBack((MetricTimeSpan)stepAfterStop);
                    }),
                    new PlaybackAction(stopPeriod, p =>
                    {
                        p.Start();
                    }),
                    new PlaybackAction(firstAfterResumeDelay,
                        p => CheckCurrentTime(p, ScaleTimeSpan(stopAfter + firstAfterResumeDelay, speed) - stepAfterStop, "resumed")),
                    new PlaybackAction(secondAfterResumeDelay,
                        p =>
                        {
                            p.MoveBack((MetricTimeSpan)stepAfterResumed);
                            CheckCurrentTime(p, ScaleTimeSpan(stopAfter + firstAfterResumeDelay + secondAfterResumeDelay, speed) - stepAfterStop - stepAfterResumed, "resumed");
                        }),
                    new PlaybackAction(thirdAfterResumeDelay,
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
            var stopAfter = TimeSpan.FromMilliseconds(500);
            var stopPeriod = TimeSpan.FromMilliseconds(600);

            var stepAfterStop = TimeSpan.FromMilliseconds(2000);
            var stepAfterResumed = TimeSpan.FromMilliseconds(500);

            var lastEventTime = TimeSpan.FromSeconds(2);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(200);
            var secondAfterResumeDelay = TimeSpan.FromMilliseconds(400);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(300);

            CheckEventPlayedEvent(
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
                        p.MoveBack((MetricTimeSpan)stepAfterStop);
                    }),
                    new PlaybackAction(stopPeriod, p =>
                    {
                        p.Start();
                        CheckCurrentTime(p, TimeSpan.Zero, "stopped");
                    }),
                    new PlaybackAction(firstAfterResumeDelay,
                        p => CheckCurrentTime(p, firstAfterResumeDelay, "resumed")),
                    new PlaybackAction(secondAfterResumeDelay,
                        p =>
                        {
                            p.MoveBack((MetricTimeSpan)stepAfterResumed);
                            CheckCurrentTime(p, firstAfterResumeDelay + secondAfterResumeDelay - stepAfterResumed, "resumed");
                        }),
                    new PlaybackAction(thirdAfterResumeDelay,
                        p => CheckCurrentTime(p, firstAfterResumeDelay + secondAfterResumeDelay + thirdAfterResumeDelay - stepAfterResumed, "resumed"))
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
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
        public void EventPlayed_MoveBack_BeyondZero_SendNoteOnEventsForActiveNotes()
        {
            var stopAfter = TimeSpan.FromMilliseconds(500);
            var stopPeriod = TimeSpan.FromMilliseconds(600);

            var stepAfterStop = TimeSpan.FromMilliseconds(2000);
            var stepAfterResumed = TimeSpan.FromMilliseconds(500);

            var lastEventTime = TimeSpan.FromSeconds(2);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(200);
            var secondAfterResumeDelay = TimeSpan.FromMilliseconds(400);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(300);

            CheckEventPlayedEvent(
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
                        p.MoveBack((MetricTimeSpan)stepAfterStop);
                    }),
                    new PlaybackAction(stopPeriod, p =>
                    {
                        p.Start();
                        CheckCurrentTime(p, TimeSpan.Zero, "stopped");
                    }),
                    new PlaybackAction(firstAfterResumeDelay,
                        p => CheckCurrentTime(p, firstAfterResumeDelay, "resumed")),
                    new PlaybackAction(secondAfterResumeDelay,
                        p =>
                        {
                            p.MoveBack((MetricTimeSpan)stepAfterResumed);
                            CheckCurrentTime(p, firstAfterResumeDelay + secondAfterResumeDelay - stepAfterResumed, "resumed");
                        }),
                    new PlaybackAction(thirdAfterResumeDelay,
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
                    playback.SendNoteOnEventsForActiveNotes = true;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void EventPlayed_MoveBack_BeyondPlaybackStart()
        {
            CheckEventPlayedEvent(
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
        public void EventPlayed_MoveBack_BeyondPlaybackStart_SendNoteOnEventsForActiveNotes()
        {
            CheckEventPlayedEvent(
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
                    playback.SendNoteOnEventsForActiveNotes = true;
                    playback.PlaybackStart = new MetricTimeSpan(0, 0, 0, 300);
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void EventPlayed_MoveToTime()
        {
            var lastEventTime = TimeSpan.FromMilliseconds(2000);

            var stopAfter = TimeSpan.FromMilliseconds(800);
            var stopPeriod = TimeSpan.FromMilliseconds(400);

            var moveTime1 = TimeSpan.FromMilliseconds(200);
            var moveTime2 = TimeSpan.FromMilliseconds(1600);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(200);
            var secondAfterResumeDelay = TimeSpan.FromMilliseconds(400);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(200);

            CheckEventPlayedEvent(
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
        public void EventPlayed_MoveToTime_BeyondDuration()
        {
            var stopAfter = TimeSpan.FromSeconds(2);
            var stopPeriod = TimeSpan.FromSeconds(2);

            CheckEventPlayedEvent(
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)TimeSpan.FromSeconds(4), TempoMap),
                },
                actions: new PlaybackAction[]
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
                    new ReceivedEvent(new NoteOffEvent(), stopAfter)
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = false;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void EventPlayed_MoveToTime_BeyondPlaybackEnd()
        {
            var stopAfter = TimeSpan.FromMilliseconds(200);
            var stopPeriod = TimeSpan.FromMilliseconds(200);

            CheckEventPlayedEvent(
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(400), TempoMap),
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(600), TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(700), TempoMap),
                },
                actions: new PlaybackAction[]
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
                    new ReceivedEvent(new NoteOffEvent(), stopAfter)
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = false;
                    playback.PlaybackEnd = new MetricTimeSpan(0, 0, 0, 450);
                });
        }

        #endregion

        #region Private methods

        private void CheckEventPlayedEvent(
            ICollection<ITimedObject> initialPlaybackObjects,
            PlaybackAction[] actions,
            ICollection<ReceivedEvent> expectedReceivedEvents,
            Action<Playback> setupPlayback = null)
        {
            var playedEvents = new List<ReceivedEvent>();
            var stopwatch = new Stopwatch();

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: initialPlaybackObjects,
                actions: actions,
                expectedReceivedEvents: expectedReceivedEvents,
                afterStart: _ => stopwatch.Start(),
                setupPlayback: playback =>
                {
                    setupPlayback?.Invoke(playback);
                    playback.EventPlayed += (_, e) => playedEvents.Add(new ReceivedEvent(e.Event, stopwatch.Elapsed));
                });

            CheckReceivedEvents(playedEvents, expectedReceivedEvents.ToList());
        }

        #endregion
    }
}
