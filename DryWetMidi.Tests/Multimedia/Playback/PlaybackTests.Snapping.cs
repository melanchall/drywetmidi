using System;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed partial class PlaybackTests
    {
        #region Test methods

        [Test]
        public void Snapping_AddSnapPoint_WithoutData()
        {
            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(0), TempoMap),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(100, p =>
                    {
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(100), "Aa");
                        p.MoveToFirstSnapPoint();
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(200), "Ab");
                    }),
                    new PlaybackAction(200, p =>
                    {
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(400), "Ba");
                        p.MoveToPreviousSnapPoint();
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(200), "Bb");
                    }),
                    new PlaybackAction(100, p =>
                    {
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(300), "Ca");
                        p.MoveToNextSnapPoint();
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(300), "Cb");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.FromMilliseconds(0)),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(600)),
                },
                setupPlayback: playback =>
                {
                    ClassicAssert.IsInstanceOf<SnapPoint>(playback.AddSnapPoint(new MetricTimeSpan(0, 0, 0, 200)));
                });
        }

        [Test]
        public void Snapping_AddSnapPoint_WithData()
        {
            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(0), TempoMap),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(100, p =>
                    {
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(100), "Aa");
                        p.MoveToFirstSnapPoint("X");
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(200), "Ab");
                    }),
                    new PlaybackAction(200, p =>
                    {
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(400), "Ba");
                        p.MoveToPreviousSnapPoint("X");
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(200), "Bb");
                    }),
                    new PlaybackAction(100, p =>
                    {
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(300), "Ca");
                        p.MoveToNextSnapPoint("X");
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(300), "Cb");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.FromMilliseconds(0)),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(600)),
                },
                setupPlayback: playback =>
                {
                    var snapPoint = playback.AddSnapPoint(new MetricTimeSpan(0, 0, 0, 200), "X");
                    ClassicAssert.IsInstanceOf<SnapPoint<string>>(snapPoint, "Invalid snap point's type.");
                    ClassicAssert.AreEqual("X", snapPoint.Data, "Invalid snap point's data.");
                });
        }

        [Test]
        public void Snapping_RemoveSnapPoint_WithoutData()
        {
            SnapPoint snapPoint = null;

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(0), TempoMap),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(100, p =>
                    {
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(100), "Aa");
                        ClassicAssert.IsTrue(p.MoveToFirstSnapPoint(), "Failed to move to first snap point.");
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(200), "Ab");
                    }),
                    new PlaybackAction(200, p =>
                    {
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(400), "Ba");
                        ClassicAssert.IsTrue(p.MoveToPreviousSnapPoint(), "Failed to move to previous snap point.");
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(200), "Bb");
                    }),
                    new PlaybackAction(100, p =>
                    {
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(300), "Ca");
                        ClassicAssert.IsFalse(p.MoveToNextSnapPoint(), "Successfully moved to next snap point.");
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(300), "Cb");
                    }),
                    new PlaybackAction(100, p =>
                    {
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(400), "Da");
                        p.RemoveSnapPoint(snapPoint);
                        ClassicAssert.IsFalse(p.MoveToPreviousSnapPoint(), "Successfully moved to previous snap point after deletion.");
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(400), "Db");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.FromMilliseconds(0)),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(600)),
                },
                setupPlayback: playback =>
                {
                    ClassicAssert.IsInstanceOf<SnapPoint>(snapPoint = playback.AddSnapPoint(new MetricTimeSpan(0, 0, 0, 200)));
                });
        }

        [Test]
        public void Snapping_RemoveSnapPoint_WithData()
        {
            SnapPoint<string> snapPoint = null;

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(0), TempoMap),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(100, p =>
                    {
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(100), "Aa");
                        ClassicAssert.IsTrue(p.MoveToFirstSnapPoint("X"), "Failed to move to first snap point.");
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(200), "Ab");
                    }),
                    new PlaybackAction(200, p =>
                    {
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(400), "Ba");
                        ClassicAssert.IsTrue(p.MoveToPreviousSnapPoint("X"), "Failed to move to previous snap point.");
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(200), "Bb");
                    }),
                    new PlaybackAction(100, p =>
                    {
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(300), "Ca");
                        ClassicAssert.IsFalse(p.MoveToNextSnapPoint("X"), "Successfully moved to next snap point.");
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(300), "Cb");
                    }),
                    new PlaybackAction(100, p =>
                    {
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(400), "Da");
                        p.RemoveSnapPoint(snapPoint);
                        ClassicAssert.IsFalse(p.MoveToPreviousSnapPoint("X"), "Successfully moved to previous snap point after deletion.");
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(400), "Db");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.FromMilliseconds(0)),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(600)),
                },
                setupPlayback: playback =>
                {
                    ClassicAssert.IsInstanceOf<SnapPoint<string>>(snapPoint = playback.AddSnapPoint(new MetricTimeSpan(0, 0, 0, 200), "X"));
                });
        }

        [Test]
        public void Snapping_RemoveSnapPointsByData()
        {
            SnapPoint snapPoint1 = null;
            SnapPoint snapPoint2 = null;
            SnapPoint snapPoint3 = null;

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(150, p =>
                    {
                        ClassicAssert.IsTrue(p.MoveToPreviousSnapPoint(), "Failed to move to previous snap point.");
                        CheckCurrentTime(p, snapPoint2.Time, "B");
                    }),
                    new PlaybackAction(100, p =>
                    {
                        p.RemoveSnapPointsByData((string data) => data.StartsWith("Data"));
                        ClassicAssert.IsTrue(p.MoveToFirstSnapPoint(), "Failed to move to first snap point.");
                        ClassicAssert.IsFalse(p.MoveToFirstSnapPoint("DataX"), "Moved to first data snap point.");
                    }),
                },
                afterStart: playback =>
                {
                    ClassicAssert.IsTrue(playback.MoveToNextSnapPoint("DataX"), "Failed to move to next snap point.");
                    CheckCurrentTime(playback, snapPoint1.Time, "A");
                },
                setupPlayback: playback =>
                {
                    snapPoint1 = playback.AddSnapPoint(new MetricTimeSpan(0, 0, 0, 100), "DataX");
                    snapPoint2 = playback.AddSnapPoint(new MetricTimeSpan(0, 0, 0, 200), "DataY");
                    snapPoint3 = playback.AddSnapPoint(new MetricTimeSpan(0, 0, 0, 300), "Something");
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.FromMilliseconds(0)),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(450)),
                });
        }

        [Test]
        public void Snapping_RemoveAllSnapPoints()
        {
            SnapPoint snapPoint1 = null;
            SnapPoint snapPoint2 = null;
            SnapPoint snapPoint3 = null;

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(150, p =>
                    {
                        ClassicAssert.IsTrue(p.MoveToPreviousSnapPoint(), "Failed to move to previous snap point.");
                        CheckCurrentTime(p, snapPoint2.Time, "B");
                    }),
                    new PlaybackAction(100, p =>
                    {
                        p.RemoveAllSnapPoints();
                        ClassicAssert.IsFalse(p.MoveToFirstSnapPoint(), "Failed to move to first snap point.");
                        ClassicAssert.IsFalse(p.MoveToFirstSnapPoint("DataX"), "Moved to first data snap point.");
                    }),
                },
                afterStart: playback =>
                {
                    ClassicAssert.IsTrue(playback.MoveToNextSnapPoint("DataX"), "Failed to move to next snap point.");
                    CheckCurrentTime(playback, snapPoint1.Time, "A");
                },
                setupPlayback: playback =>
                {
                    snapPoint1 = playback.AddSnapPoint(new MetricTimeSpan(0, 0, 0, 100), "DataX");
                    snapPoint2 = playback.AddSnapPoint(new MetricTimeSpan(0, 0, 0, 200), "DataY");
                    snapPoint3 = playback.AddSnapPoint(new MetricTimeSpan(0, 0, 0, 300), "Something");
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.FromMilliseconds(0)),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(450)),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void Snapping_MoveToSnapPoint_1()
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

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)endTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(stopAfter, p =>
                    {
                        p.Stop();
                        p.MoveToSnapPoint(snapPoint1);
                    }),
                    new PlaybackAction(stopPeriod, p =>
                    {
                        p.Start();
                        CheckCurrentTime(p, snapPointTime1, "stopped");
                    }),
                    new PlaybackAction(firstAfterResumeDelay,
                        p => CheckCurrentTime(p, snapPointTime1 + firstAfterResumeDelay, "resumed")),
                    new PlaybackAction(secondAfterResumeDelay, p =>
                    {
                        p.MoveToSnapPoint(snapPoint2);
                        CheckCurrentTime(p, snapPointTime2, "resumed");
                    }),
                    new PlaybackAction(thirdAfterResumeDelay,
                        p => CheckCurrentTime(p, snapPointTime2 + thirdAfterResumeDelay, "resumed")),
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;

                    snapPoint1 = playback.AddSnapPoint((MetricTimeSpan)snapPointTime1, "Data1");
                    snapPoint2 = playback.AddSnapPoint((MetricTimeSpan)snapPointTime2, "Data2");
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(), endTime + stopPeriod + (stopAfter - snapPointTime1) - (snapPointTime2 - (snapPointTime1 + firstAfterResumeDelay + secondAfterResumeDelay))),
                });
        }

        [Test]
        public void Snapping_MoveToSnapPoint_2()
        {
            SnapPoint snapPoint = null;

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(100, p =>
                    {
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(100), "Aa");
                        ClassicAssert.IsTrue(p.MoveToSnapPoint(snapPoint), "Failed to move to snap point.");
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(200), "Ab");
                    }),
                    new PlaybackAction(200, p =>
                    {
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(400), "Ba");
                        snapPoint.IsEnabled = false;
                        ClassicAssert.IsFalse(p.MoveToSnapPoint(snapPoint), "Position changed to disabled snap point's time.");
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(400), "Bb");
                    }),
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;

                    snapPoint = playback.AddSnapPoint(new MetricTimeSpan(0, 0, 0, 200));
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(400)),
                });
        }

        [Test]
        public void Snapping_MoveToFirstSnapPoint_1()
        {
            var snapPoint1Time = new MetricTimeSpan(0, 0, 0, 200);
            var snapPoint2Time = new MetricTimeSpan(0, 0, 0, 400);

            SnapPoint snapPoint1 = null;
            SnapPoint snapPoint2 = null;

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(100, p =>
                    {
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(100), "Aa");
                        ClassicAssert.IsTrue(p.MoveToFirstSnapPoint(), "Failed to move to first snap point.");
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(200), "Ab");
                    }),
                    new PlaybackAction(50, p =>
                    {
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(250), "Ba");
                        ClassicAssert.IsTrue(p.MoveToFirstSnapPoint(), "Failed to move to first snap point again.");
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(200), "Bb");
                    }),
                    new PlaybackAction(100, p =>
                    {
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(300), "Ca");
                        snapPoint1.IsEnabled = false;
                        ClassicAssert.IsTrue(p.MoveToFirstSnapPoint(), "Failed to move to first snap point after first one disabled.");
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(400), "Cb");
                    }),
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;

                    snapPoint1 = playback.AddSnapPoint(snapPoint1Time);
                    snapPoint2 = playback.AddSnapPoint(snapPoint2Time);
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(350)),
                });
        }

        [Test]
        public void Snapping_MoveToFirstSnapPoint_2()
        {
            var snapPoint1Time = new MetricTimeSpan();
            var snapPoint2Time = new MetricTimeSpan(0, 0, 0, 200);

            SnapPoint snapPoint1 = null;
            SnapPoint snapPoint2 = null;

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(100, p =>
                    {
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(100), "Aa");
                        ClassicAssert.IsTrue(p.MoveToFirstSnapPoint(), "Failed to move to first snap point.");
                        CheckCurrentTime(p, snapPoint1Time, "Ab");
                    }),
                    new PlaybackAction(50, p =>
                    {
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(50), "Ba");
                        ClassicAssert.IsTrue(p.MoveToFirstSnapPoint(), "Failed to move to first snap point again.");
                        CheckCurrentTime(p, snapPoint1Time, "Bb");
                    }),
                    new PlaybackAction(100, p =>
                    {
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(100), "Ca");
                        snapPoint1.IsEnabled = false;
                        ClassicAssert.IsTrue(p.MoveToFirstSnapPoint(), "Failed to move to first snap point after first one disabled.");
                        CheckCurrentTime(p, snapPoint2Time, "Cb");
                    }),
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;

                    snapPoint1 = playback.AddSnapPoint(snapPoint1Time);
                    snapPoint2 = playback.AddSnapPoint(snapPoint2Time);
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(550)),
                });
        }

        [Test]
        public void Snapping_MoveToFirstSnapPoint_3()
        {
            var snapPoint1Time = new MetricTimeSpan(0, 0, 0, 100);
            var snapPoint2Time = new MetricTimeSpan(0, 0, 0, 200);
            var snapPoint3Time = new MetricTimeSpan(0, 0, 0, 300);

            SnapPoint<string> snapPoint1 = null;
            SnapPoint<string> snapPoint2 = null;
            SnapPoint<string> snapPoint3 = null;

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(50, p =>
                    {
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(50), "Aa");
                        ClassicAssert.IsTrue(p.MoveToFirstSnapPoint("Y"), "Failed to move to first snap point.");
                        CheckCurrentTime(p, snapPoint2Time, "Ab");
                    }),
                    new PlaybackAction(100, p =>
                    {
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(300), "Ba");
                        ClassicAssert.IsTrue(p.MoveToFirstSnapPoint("Y"), "Failed to move to first snap point again.");
                        CheckCurrentTime(p, snapPoint2Time, "Bb");
                    }),
                    new PlaybackAction(100, p =>
                    {
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(300), "Ca");
                        snapPoint2.IsEnabled = false;
                        ClassicAssert.IsFalse(p.MoveToFirstSnapPoint("Y"), "Failed to move to first snap point after first one disabled.");
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(300), "Cb");
                    }),
                    new PlaybackAction(50, p =>
                    {
                        p.MoveToTime(new MetricTimeSpan(0, 0, 0, 400));
                        ClassicAssert.IsTrue(p.MoveToFirstSnapPoint("Z"), "Failed to move to first enabled snap point.");
                        CheckCurrentTime(p, snapPoint3Time, "Db");
                    }),
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;

                    snapPoint1 = playback.AddSnapPoint(snapPoint1Time, "X");
                    snapPoint2 = playback.AddSnapPoint(snapPoint2Time, "Y");
                    snapPoint3 = playback.AddSnapPoint(snapPoint3Time, "Z");
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(500)),
                });
        }

        [Test]
        public void Snapping_MoveToFirstSnapPoint_4()
        {
            var snapPoint1Time = new MetricTimeSpan(0, 0, 0, 100);
            var snapPoint2Time = new MetricTimeSpan(0, 0, 0, 0);
            var snapPoint3Time = new MetricTimeSpan(0, 0, 0, 300);

            SnapPoint<string> snapPoint1 = null;
            SnapPoint<string> snapPoint2 = null;
            SnapPoint<string> snapPoint3 = null;

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(50, p =>
                    {
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(50), "Aa");
                        ClassicAssert.IsTrue(p.MoveToFirstSnapPoint("Y"), "Failed to move to first snap point.");
                        CheckCurrentTime(p, snapPoint2Time, "Ab");
                    }),
                    new PlaybackAction(100, p =>
                    {
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(100), "Ba");
                        ClassicAssert.IsTrue(p.MoveToFirstSnapPoint("Y"), "Failed to move to first snap point again.");
                        CheckCurrentTime(p, snapPoint2Time, "Bb");
                    }),
                    new PlaybackAction(100, p =>
                    {
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(100), "Ca");
                        snapPoint2.IsEnabled = false;
                        ClassicAssert.IsFalse(p.MoveToFirstSnapPoint("Y"), "Failed to move to first snap point after first one disabled.");
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(100), "Cb");
                    }),
                    new PlaybackAction(50, p =>
                    {
                        p.MoveToTime(new MetricTimeSpan(0, 0, 0, 400));
                        ClassicAssert.IsTrue(p.MoveToFirstSnapPoint("Z"), "Failed to move to first enabled snap point.");
                        CheckCurrentTime(p, snapPoint3Time, "Db");
                    }),
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;

                    snapPoint1 = playback.AddSnapPoint(snapPoint1Time, "X");
                    snapPoint2 = playback.AddSnapPoint(snapPoint2Time, "Y");
                    snapPoint3 = playback.AddSnapPoint(snapPoint3Time, "Z");
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(500)),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void Snapping_MoveToPreviousSnapPoint_1()
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

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new MarkerEvent("A"))
                        .SetTime((MetricTimeSpan)snapPointTime1, TempoMap),
                    new TimedEvent(new MarkerEvent("B"))
                        .SetTime((MetricTimeSpan)snapPointTime2, TempoMap),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)endTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(stopAfter, p =>
                    {
                        p.Stop();
                        p.MoveToPreviousSnapPoint(snapPointsGroup);
                    }),
                    new PlaybackAction(stopPeriod, p =>
                    {
                        p.Start();
                        CheckCurrentTime(p, snapPointTime1, "stopped");
                    }),
                    new PlaybackAction(firstAfterResumeDelay,
                        p => CheckCurrentTime(p, snapPointTime1 + firstAfterResumeDelay, "resumed")),
                    new PlaybackAction(secondAfterResumeDelay, p =>
                    {
                        p.MoveToPreviousSnapPoint(snapPointsGroup);
                        CheckCurrentTime(p, snapPointTime2, "resumed");
                    }),
                    new PlaybackAction(thirdAfterResumeDelay,
                        p => CheckCurrentTime(p, snapPointTime2 + thirdAfterResumeDelay, "resumed")),
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;

                    snapPointsGroup = playback.SnapToEvents(e => e.EventType == MidiEventType.Marker);
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.FromMilliseconds(0)),
                    new ReceivedEvent(new MarkerEvent("A"), TimeSpan.FromMilliseconds(1000)),
                    new ReceivedEvent(new MarkerEvent("A"), TimeSpan.FromMilliseconds(3000)),
                    new ReceivedEvent(new MarkerEvent("B"), TimeSpan.FromMilliseconds(4100)),
                    new ReceivedEvent(new MarkerEvent("B"), TimeSpan.FromMilliseconds(4600)),
                    new ReceivedEvent(new NoteOffEvent(), endTime + stopPeriod + (stopAfter - snapPointTime1) + (snapPointTime1 + firstAfterResumeDelay + secondAfterResumeDelay - snapPointTime2)),
                });
        }

        [Test]
        public void Snapping_MoveToPreviousSnapPoint_ByGroup_CheckReturnValue()
        {
            SnapPointsGroup snapPointsGroup = null;

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new MarkerEvent("A"))
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(100), TempoMap),
                    new TimedEvent(new MarkerEvent("B"))
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(400), TempoMap),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(50, p =>
                    {
                        p.Stop();
                        p.MoveToTime(p.GetDuration<MetricTimeSpan>());

                        ClassicAssert.IsTrue(p.MoveToPreviousSnapPoint(snapPointsGroup), "Failed to move to first previous snap point.");
                        ClassicAssert.IsTrue(p.MoveToPreviousSnapPoint(snapPointsGroup), "Failed to move to second previous snap point.");
                        ClassicAssert.IsFalse(p.MoveToPreviousSnapPoint(snapPointsGroup), "Position changed beyond first snap point of the group.");

                        p.Start();
                    }),
                    new PlaybackAction(100, p =>
                    {
                        snapPointsGroup.IsEnabled = false;
                        p.MoveToTime(p.GetDuration<MetricTimeSpan>());
                        ClassicAssert.IsFalse(
                            p.MoveToPreviousSnapPoint(snapPointsGroup),
                            "Position changed to the time of a snap point within disabled snap point group.");
                    }),
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;

                    snapPointsGroup = playback.SnapToEvents(e => e.EventType == MidiEventType.Marker);
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.FromMilliseconds(0)),
                    // 50 is correct value, but on macOS tick generator restart requires more time
                    new ReceivedEvent(new MarkerEvent("A"), TimeSpan.FromMilliseconds(55)),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(150)),
                });
        }

        [Test]
        public void Snapping_MoveToPreviousSnapPoint_ByGroup_CheckReturnValue_SendNoteOffEventsForNonActiveNotes()
        {
            SnapPointsGroup snapPointsGroup = null;

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new MarkerEvent("A"))
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(100), TempoMap),
                    new TimedEvent(new MarkerEvent("B"))
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(400), TempoMap),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(50, p =>
                    {
                        p.Stop();
                        p.MoveToTime(p.GetDuration<MetricTimeSpan>());

                        ClassicAssert.IsTrue(p.MoveToPreviousSnapPoint(snapPointsGroup), "Failed to move to first previous snap point.");
                        ClassicAssert.IsTrue(p.MoveToPreviousSnapPoint(snapPointsGroup), "Failed to move to second previous snap point.");
                        ClassicAssert.IsFalse(p.MoveToPreviousSnapPoint(snapPointsGroup), "Position changed beyond first snap point of the group.");

                        p.Start();
                    }),
                    new PlaybackAction(100, p =>
                    {
                        snapPointsGroup.IsEnabled = false;
                        p.MoveToTime(p.GetDuration<MetricTimeSpan>());
                        ClassicAssert.IsFalse(
                            p.MoveToPreviousSnapPoint(snapPointsGroup),
                            "Position changed to the time of a snap point within disabled snap point group.");
                    }),
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;
                    playback.SendNoteOffEventsForNonActiveNotes = true;

                    snapPointsGroup = playback.SnapToEvents(e => e.EventType == MidiEventType.Marker);
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.FromMilliseconds(0)),
                    // 50 is correct value, but on macOS tick generator restart requires more time
                    new ReceivedEvent(new MarkerEvent("A"), TimeSpan.FromMilliseconds(55)),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(150)),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(155)),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void Snapping_MoveToPreviousSnapPoint_Global()
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

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new MarkerEvent("A"))
                        .SetTime((MetricTimeSpan)snapPointTime1, TempoMap),
                    new TimedEvent(new MarkerEvent("B"))
                        .SetTime((MetricTimeSpan)snapPointTime2, TempoMap),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)endTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(stopAfter, p =>
                    {
                        p.Stop();
                        p.MoveToPreviousSnapPoint();
                    }),
                    new PlaybackAction(stopPeriod, p =>
                    {
                        p.Start();
                        CheckCurrentTime(p, snapPointTime1, "stopped");
                    }),
                    new PlaybackAction(firstAfterResumeDelay,
                        p => CheckCurrentTime(p, snapPointTime1 + firstAfterResumeDelay, "resumed")),
                    new PlaybackAction(secondAfterResumeDelay, p =>
                    {
                        p.MoveToPreviousSnapPoint();
                        CheckCurrentTime(p, snapPointTime3, "resumed");
                    }),
                    new PlaybackAction(thirdAfterResumeDelay,
                        p => CheckCurrentTime(p, snapPointTime3 + thirdAfterResumeDelay, "resumed")),
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;

                    playback.SnapToEvents(e => e.EventType == MidiEventType.Marker);
                    playback.AddSnapPoint((MetricTimeSpan)snapPointTime3, "Data");
                    var snapPoint = playback.AddSnapPoint(new MetricTimeSpan(0, 0, 0, 2400), "Data");
                    snapPoint.IsEnabled = false;
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new ReceivedEvent(new MarkerEvent("A"), TimeSpan.FromMilliseconds(1000)),
                    new ReceivedEvent(new MarkerEvent("A"), TimeSpan.FromMilliseconds(3000)),
                    new ReceivedEvent(new MarkerEvent("B"), TimeSpan.FromMilliseconds(4100)),
                    new ReceivedEvent(new NoteOffEvent(), endTime + stopPeriod + (stopAfter - snapPointTime1) + (snapPointTime1 + firstAfterResumeDelay + secondAfterResumeDelay - snapPointTime3)),
                });
        }

        [Test]
        public void Snapping_MoveToPreviousSnapPoint_Global_CheckReturnValue()
        {
            SnapPoint snapPoint1 = null;
            SnapPoint snapPoint2 = null;

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(700), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(50, p =>
                    {
                        p.Stop();

                        p.MoveToTime(p.GetDuration<MetricTimeSpan>());
                        ClassicAssert.IsTrue(p.MoveToPreviousSnapPoint(), "Failed to move to first previous snap point.");
                        ClassicAssert.IsTrue(p.MoveToPreviousSnapPoint(), "Failed to move to second previous snap point.");
                        ClassicAssert.IsFalse(p.MoveToPreviousSnapPoint(), "Position changed beyond first snap point.");

                        p.Start();
                    }),
                    new PlaybackAction(100, p =>
                    {
                        p.Stop();

                        snapPoint2.IsEnabled = false;
                        p.MoveToTime(p.GetDuration<MetricTimeSpan>());
                        ClassicAssert.IsTrue(p.MoveToPreviousSnapPoint(), "Failed to move to second previous snap point.");
                        ClassicAssert.IsFalse(p.MoveToPreviousSnapPoint(), "Position changed beyond first snap point.");

                        p.Start();
                    }),
                    new PlaybackAction(100, p =>
                    {
                        p.Stop();

                        snapPoint1.IsEnabled = false;
                        p.MoveToTime(p.GetDuration<MetricTimeSpan>());
                        ClassicAssert.IsFalse(p.MoveToPreviousSnapPoint(), "Position changed without any snap point enabled.");

                        p.Start();
                    }),
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;

                    snapPoint1 = playback.AddSnapPoint(new MetricTimeSpan(0, 0, 0, 100));
                    snapPoint2 = playback.AddSnapPoint(new MetricTimeSpan(0, 0, 0, 500));
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.FromMilliseconds(0)),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(250)),
                });
        }

        [Test]
        public void Snapping_MoveToPreviousSnapPoint_Global_CheckReturnValue_SendNoteOffEventsForNonActiveNotes()
        {
            SnapPoint snapPoint1 = null;
            SnapPoint snapPoint2 = null;

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(700), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(50, p =>
                    {
                        p.Stop();

                        p.MoveToTime(p.GetDuration<MetricTimeSpan>());
                        ClassicAssert.IsTrue(p.MoveToPreviousSnapPoint(), "Failed to move to first previous snap point.");
                        ClassicAssert.IsTrue(p.MoveToPreviousSnapPoint(), "Failed to move to second previous snap point.");
                        ClassicAssert.IsFalse(p.MoveToPreviousSnapPoint(), "Position changed beyond first snap point.");

                        p.Start();
                    }),
                    new PlaybackAction(100, p =>
                    {
                        p.Stop();

                        snapPoint2.IsEnabled = false;
                        p.MoveToTime(p.GetDuration<MetricTimeSpan>());
                        ClassicAssert.IsTrue(p.MoveToPreviousSnapPoint(), "Failed to move to second previous snap point.");
                        ClassicAssert.IsFalse(p.MoveToPreviousSnapPoint(), "Position changed beyond first snap point.");

                        p.Start();
                    }),
                    new PlaybackAction(100, p =>
                    {
                        p.Stop();

                        snapPoint1.IsEnabled = false;
                        p.MoveToTime(p.GetDuration<MetricTimeSpan>());
                        ClassicAssert.IsFalse(p.MoveToPreviousSnapPoint(), "Position changed without any snap point enabled.");

                        p.Start();
                    }),
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;
                    playback.SendNoteOffEventsForNonActiveNotes = true;

                    snapPoint1 = playback.AddSnapPoint(new MetricTimeSpan(0, 0, 0, 100));
                    snapPoint2 = playback.AddSnapPoint(new MetricTimeSpan(0, 0, 0, 500));
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.FromMilliseconds(0)),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(250)),
                    // 250 is correct value, but on macOS tick generator restart requires more time
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(255)),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void Snapping_MoveToPreviousSnapPoint_ByData()
        {
            var stopAfter = TimeSpan.FromSeconds(2);
            var stopPeriod = TimeSpan.FromSeconds(1);

            var snapPointTime1 = TimeSpan.FromSeconds(1);
            var snapPointTime2 = TimeSpan.FromMilliseconds(1500);
            var snapPointTime3 = TimeSpan.FromMilliseconds(2300);

            var firstAfterResumeDelay = TimeSpan.FromSeconds(1);
            var secondAfterResumeDelay = TimeSpan.FromMilliseconds(600);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(200);

            var endTime = TimeSpan.FromSeconds(4);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)endTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(stopAfter, p =>
                    {
                        p.Stop();
                        p.MoveToPreviousSnapPoint("X");
                    }),
                    new PlaybackAction(stopPeriod, p =>
                    {
                        p.Start();
                        CheckCurrentTime(p, snapPointTime1, "stopped");
                    }),
                    new PlaybackAction(firstAfterResumeDelay,
                        p => CheckCurrentTime(p, snapPointTime1 + firstAfterResumeDelay, "resumed")),
                    new PlaybackAction(secondAfterResumeDelay, p =>
                    {
                        p.MoveToPreviousSnapPoint("X");
                        CheckCurrentTime(p, snapPointTime3, "resumed");
                    }),
                    new PlaybackAction(thirdAfterResumeDelay,
                        p => CheckCurrentTime(p, snapPointTime3 + thirdAfterResumeDelay, "resumed")),
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;

                    playback.AddSnapPoint((MetricTimeSpan)snapPointTime1, "X");
                    playback.AddSnapPoint((MetricTimeSpan)snapPointTime2, "Y");
                    playback.AddSnapPoint((MetricTimeSpan)snapPointTime3, "X");
                    playback.AddSnapPoint(new MetricTimeSpan(0, 0, 0, 2400), "Y");
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(), endTime + stopPeriod + (stopAfter - snapPointTime1) + (snapPointTime1 + firstAfterResumeDelay + secondAfterResumeDelay - snapPointTime3)),
                });
        }

        [Test]
        public void Snapping_MoveToPreviousSnapPoint_ByData_CheckReturnValue()
        {
            SnapPoint snapPoint1 = null;
            SnapPoint snapPoint2 = null;

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(700), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(50, p =>
                    {
                        p.Stop();

                        p.MoveToTime(p.GetDuration<MetricTimeSpan>());
                        ClassicAssert.IsTrue(p.MoveToPreviousSnapPoint("X"), "Failed to move to previous snap point.");
                        ClassicAssert.IsFalse(p.MoveToPreviousSnapPoint("X"), "Position changed beyond first snap point.");

                        p.Start();
                    }),
                    new PlaybackAction(100, p =>
                    {
                        p.Stop();

                        snapPoint1.IsEnabled = false;
                        p.MoveToTime(p.GetDuration<MetricTimeSpan>());
                        ClassicAssert.IsFalse(p.MoveToPreviousSnapPoint("X"), "Position changed without any snap point enabled.");

                        p.Start();
                    }),
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;

                    snapPoint1 = playback.AddSnapPoint(new MetricTimeSpan(0, 0, 0, 100), "X");
                    snapPoint2 = playback.AddSnapPoint(new MetricTimeSpan(0, 0, 0, 500), "Y");
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.FromMilliseconds(0)),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(150)),
                });
        }

        [Test]
        public void Snapping_MoveToPreviousSnapPoint_ByData_CheckReturnValue_SendNoteOffEventsForNonActiveNotes()
        {
            SnapPoint snapPoint1 = null;
            SnapPoint snapPoint2 = null;

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(700), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(50, p =>
                    {
                        p.Stop();

                        p.MoveToTime(p.GetDuration<MetricTimeSpan>());
                        ClassicAssert.IsTrue(p.MoveToPreviousSnapPoint("X"), "Failed to move to previous snap point.");
                        ClassicAssert.IsFalse(p.MoveToPreviousSnapPoint("X"), "Position changed beyond first snap point.");

                        p.Start();
                    }),
                    new PlaybackAction(100, p =>
                    {
                        p.Stop();

                        snapPoint1.IsEnabled = false;
                        p.MoveToTime(p.GetDuration<MetricTimeSpan>());
                        ClassicAssert.IsFalse(p.MoveToPreviousSnapPoint("X"), "Position changed without any snap point enabled.");

                        p.Start();
                    }),
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;
                    playback.SendNoteOffEventsForNonActiveNotes = true;

                    snapPoint1 = playback.AddSnapPoint(new MetricTimeSpan(0, 0, 0, 100), "X");
                    snapPoint2 = playback.AddSnapPoint(new MetricTimeSpan(0, 0, 0, 500), "Y");
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.FromMilliseconds(0)),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(150)),
                    // 150 is correct value, but on macOS tick generator restart requires more time
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(155)),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void Snapping_MoveToNextSnapPoint_ByGroup()
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

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new MarkerEvent("A"))
                        .SetTime((MetricTimeSpan)snapPointTime1, TempoMap),
                    new TimedEvent(new MarkerEvent("B"))
                        .SetTime((MetricTimeSpan)snapPointTime2, TempoMap),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)endTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(stopAfter, p =>
                    {
                        p.Stop();
                        p.MoveToNextSnapPoint(snapPointsGroup);
                    }),
                    new PlaybackAction(stopPeriod, p =>
                    {
                        p.Start();
                        CheckCurrentTime(p, snapPointTime1, "stopped");
                    }),
                    new PlaybackAction(firstAfterResumeDelay,
                        p => CheckCurrentTime(p, snapPointTime1 + firstAfterResumeDelay, "resumed")),
                    new PlaybackAction(secondAfterResumeDelay, p =>
                    {
                        p.MoveToNextSnapPoint(snapPointsGroup);
                        CheckCurrentTime(p, snapPointTime2, "resumed");
                    }),
                    new PlaybackAction(thirdAfterResumeDelay,
                        p => CheckCurrentTime(p, snapPointTime2 + thirdAfterResumeDelay, "resumed")),
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;

                    snapPointsGroup = playback.SnapToEvents(e => e.EventType == MidiEventType.Marker);
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new ReceivedEvent(new MarkerEvent("A"), TimeSpan.FromMilliseconds(2100)),
                    new ReceivedEvent(new MarkerEvent("B"), TimeSpan.FromMilliseconds(3000)),
                    new ReceivedEvent(new NoteOffEvent(), endTime + stopPeriod - (snapPointTime1 - stopAfter) - (snapPointTime2 - (snapPointTime1 + firstAfterResumeDelay + secondAfterResumeDelay))),
                });
        }

        [Test]
        public void Snapping_MoveToNextSnapPoint_ByGroup_CheckReturnValue()
        {
            SnapPointsGroup snapPointsGroup = null;

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new MarkerEvent("A"))
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(100), TempoMap),
                    new TimedEvent(new MarkerEvent("B"))
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(400), TempoMap),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(50, p =>
                    {
                        ClassicAssert.IsTrue(p.MoveToNextSnapPoint(snapPointsGroup), "Failed to move to first next snap point.");
                        ClassicAssert.IsTrue(p.MoveToNextSnapPoint(snapPointsGroup), "Failed to move to second next snap point.");
                        ClassicAssert.IsFalse(p.MoveToNextSnapPoint(snapPointsGroup), "Position changed beyond last snap point of the group.");
                    }),
                    new PlaybackAction(50, p =>
                    {
                        snapPointsGroup.IsEnabled = false;
                        p.MoveToStart();
                        ClassicAssert.IsFalse(
                            p.MoveToNextSnapPoint(snapPointsGroup),
                            "Position changed to the time of a snap point within disabled snap point group.");
                    }),
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;

                    snapPointsGroup = playback.SnapToEvents(e => e.EventType == MidiEventType.Marker);
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.FromMilliseconds(0)),
                    new ReceivedEvent(new MarkerEvent("B"), TimeSpan.FromMilliseconds(50)),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new MarkerEvent("A"), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new MarkerEvent("B"), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(600)),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void Snapping_MoveToNextSnapPoint_Global()
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

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new MarkerEvent("A"))
                        .SetTime((MetricTimeSpan)snapPointTime1, TempoMap),
                    new TimedEvent(new MarkerEvent("B"))
                        .SetTime((MetricTimeSpan)snapPointTime2, TempoMap),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)endTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(stopAfter, p =>
                    {
                        p.Stop();
                        p.MoveToNextSnapPoint();
                    }),
                    new PlaybackAction(stopPeriod, p =>
                    {
                        p.Start();
                        CheckCurrentTime(p, snapPointTime1, "stopped");
                    }),
                    new PlaybackAction(firstAfterResumeDelay,
                        p => CheckCurrentTime(p, snapPointTime1 + firstAfterResumeDelay, "resumed")),
                    new PlaybackAction(secondAfterResumeDelay, p =>
                    {
                        p.MoveToNextSnapPoint();
                        CheckCurrentTime(p, snapPointTime3, "resumed");
                    }),
                    new PlaybackAction(thirdAfterResumeDelay,
                        p => CheckCurrentTime(p, snapPointTime3 + thirdAfterResumeDelay, "resumed")),
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;

                    playback.SnapToEvents(e => e.EventType == MidiEventType.Marker);
                    playback.AddSnapPoint((MetricTimeSpan)snapPointTime3, "Data");
                    var snapPoint = playback.AddSnapPoint(new MetricTimeSpan(0, 0, 0, 1100), "Data");
                    snapPoint.IsEnabled = false;
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new ReceivedEvent(new MarkerEvent("A"), TimeSpan.FromMilliseconds(2100)),
                    new ReceivedEvent(new MarkerEvent("B"), TimeSpan.FromMilliseconds(3100)),
                    new ReceivedEvent(new NoteOffEvent(), endTime + stopPeriod - (snapPointTime1 - stopAfter) - (snapPointTime3 - (snapPointTime1 + firstAfterResumeDelay + secondAfterResumeDelay))),
                });
        }

        [Test]
        public void Snapping_MoveToNextSnapPoint_Global_CheckReturnValue()
        {
            SnapPoint snapPoint1 = null;
            SnapPoint snapPoint2 = null;

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(700), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(50, p =>
                    {
                        ClassicAssert.IsTrue(p.MoveToNextSnapPoint(), "Failed to move to first next snap point.");
                        ClassicAssert.IsTrue(p.MoveToNextSnapPoint(), "Failed to move to second next snap point.");
                        ClassicAssert.IsFalse(p.MoveToNextSnapPoint(), "Position changed beyond last snap point.");
                    }),
                    new PlaybackAction(100, p =>
                    {
                        snapPoint2.IsEnabled = false;
                        p.MoveToStart();
                        ClassicAssert.IsTrue(p.MoveToNextSnapPoint(), "Failed to move to second next snap point.");
                        ClassicAssert.IsFalse(p.MoveToNextSnapPoint(), "Position changed beyond last snap point.");
                    }),
                    new PlaybackAction(100, p =>
                    {
                        snapPoint1.IsEnabled = false;
                        p.MoveToStart();
                        ClassicAssert.IsFalse(p.MoveToNextSnapPoint(), "Position changed without any snap point enabled.");
                    }),
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;

                    snapPoint1 = playback.AddSnapPoint(new MetricTimeSpan(0, 0, 0, 100));
                    snapPoint2 = playback.AddSnapPoint(new MetricTimeSpan(0, 0, 0, 500));
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.FromMilliseconds(0)),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(150)),
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.FromMilliseconds(150)),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(250)),
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.FromMilliseconds(250)),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(950)),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void Snapping_MoveToNextSnapPoint_ByData()
        {
            var stopAfter = TimeSpan.FromSeconds(1);
            var stopPeriod = TimeSpan.FromMilliseconds(1100);

            var snapPointTime1 = TimeSpan.FromMilliseconds(1200);
            var snapPointTime2 = TimeSpan.FromSeconds(2000);
            var snapPointTime3 = TimeSpan.FromMilliseconds(2900);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(300);
            var secondAfterResumeDelay = TimeSpan.FromMilliseconds(600);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(200);

            var endTime = TimeSpan.FromSeconds(4);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)endTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(stopAfter, p =>
                    {
                        p.Stop();
                        p.MoveToNextSnapPoint("X");
                    }),
                    new PlaybackAction(stopPeriod, p =>
                    {
                        p.Start();
                        CheckCurrentTime(p, snapPointTime1, "stopped");
                    }),
                    new PlaybackAction(firstAfterResumeDelay,
                        p => CheckCurrentTime(p, snapPointTime1 + firstAfterResumeDelay, "resumed")),
                    new PlaybackAction(secondAfterResumeDelay, p =>
                    {
                        p.MoveToNextSnapPoint("X");
                        CheckCurrentTime(p, snapPointTime3, "resumed");
                    }),
                    new PlaybackAction(thirdAfterResumeDelay,
                        p => CheckCurrentTime(p, snapPointTime3 + thirdAfterResumeDelay, "resumed")),
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;

                    playback.AddSnapPoint((MetricTimeSpan)snapPointTime1, "X");
                    playback.AddSnapPoint((MetricTimeSpan)snapPointTime2, "Y");
                    playback.AddSnapPoint((MetricTimeSpan)snapPointTime3, "X");
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(), endTime + stopPeriod - (snapPointTime1 - stopAfter) - (snapPointTime3 - (snapPointTime1 + firstAfterResumeDelay + secondAfterResumeDelay))),
                });
        }

        [Test]
        public void Snapping_MoveToNextSnapPoint_ByData_CheckReturnValue()
        {
            SnapPoint snapPoint1 = null;
            SnapPoint snapPoint2 = null;

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(700), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(50, p =>
                    {
                        ClassicAssert.IsTrue(p.MoveToNextSnapPoint("X"), "Failed to move to next snap point.");
                        ClassicAssert.IsFalse(p.MoveToNextSnapPoint("X"), "Position changed beyond last snap point.");
                    }),
                    new PlaybackAction(100, p =>
                    {
                        snapPoint2.IsEnabled = false;
                        p.MoveToStart();
                        ClassicAssert.IsFalse(p.MoveToNextSnapPoint("X"), "Position changed without any snap point enabled.");
                    }),
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;

                    snapPoint1 = playback.AddSnapPoint(new MetricTimeSpan(0, 0, 0, 100), "Y");
                    snapPoint2 = playback.AddSnapPoint(new MetricTimeSpan(0, 0, 0, 500), "X");
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.FromMilliseconds(0)),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(150)),
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.FromMilliseconds(150)),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(850)),
                });
        }

        [Test]
        public void Snapping_MoveToNextSnapPoint_ByData_CheckReturnValue_AtZero()
        {
            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(700), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(50, p =>
                    {
                        ClassicAssert.IsFalse(p.MoveToNextSnapPoint("X"), "Position changed beyond last snap point.");
                    }),
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;

                    playback.AddSnapPoint(new MetricTimeSpan(), "X");
                    ClassicAssert.IsTrue(playback.MoveToNextSnapPoint("X"), "Failed to move to next snap point.");
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.FromMilliseconds(0)),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(700)),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void Snapping_SnapToEvents()
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

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)100))
                        .SetTime((MetricTimeSpan)snapPointTime1, TempoMap),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)10, (SevenBitNumber)100))
                        .SetTime((MetricTimeSpan)snapPointTime2, TempoMap),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)100))
                        .SetTime((MetricTimeSpan)(snapPointTime2 + TimeSpan.FromSeconds(1)), TempoMap),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)10, (SevenBitNumber)100))
                        .SetTime((MetricTimeSpan)(snapPointTime2 + TimeSpan.FromMilliseconds(1200)), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(stopAfter, p =>
                    {
                        p.Stop();
                        p.MoveToPreviousSnapPoint(snapPointsGroup);
                    }),
                    new PlaybackAction(stopPeriod, p =>
                    {
                        p.Start();
                        CheckCurrentTime(p, snapPointTime1, "stopped");
                    }),
                    new PlaybackAction(firstAfterResumeDelay,
                        p => CheckCurrentTime(p, snapPointTime1 + firstAfterResumeDelay, "resumed")),
                    new PlaybackAction(secondAfterResumeDelay, p =>
                    {
                        p.MoveToNextSnapPoint(snapPointsGroup);
                        CheckCurrentTime(p, snapPointTime2, "resumed");
                    }),
                    new PlaybackAction(thirdAfterResumeDelay,
                        p => CheckCurrentTime(p, snapPointTime2 + thirdAfterResumeDelay, "resumed")),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = false;
                    playback.InterruptNotesOnStop = false;
                    playback.SendNoteOnEventsForActiveNotes = true;

                    snapPointsGroup = playback.SnapToEvents(e => e.EventType == MidiEventType.NoteOn);
                    var snapPointsGroup2 = playback.SnapToEvents(e => e.EventType == MidiEventType.NoteOn);
                    ClassicAssert.AreNotSame(snapPointsGroup, snapPointsGroup2, "Snapping to notes starts creates new snap points group.");
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)100), snapPointTime1),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)100), stopAfter + stopPeriod),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)10, (SevenBitNumber)100), stopAfter + stopPeriod + firstAfterResumeDelay + secondAfterResumeDelay),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)100), stopAfter + stopPeriod + firstAfterResumeDelay + secondAfterResumeDelay + TimeSpan.FromSeconds(1)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)10, (SevenBitNumber)100), endTime + stopPeriod + (stopAfter - snapPointTime1) - (snapPointTime2 - (snapPointTime1 + firstAfterResumeDelay + secondAfterResumeDelay))),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void Snapping_MoveToSnapPoint_BeyondDuration()
        {
            SnapPoint snapPointBeyondDuration = null;

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(100, p =>
                    {
                        ClassicAssert.IsTrue(p.MoveToNextSnapPoint(), "Failed to move to snap point.");
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(200), "A");
                        ClassicAssert.IsFalse(p.MoveToNextSnapPoint(), "Moved to snap point beyond duration.");
                    }),
                    new PlaybackAction(100, p =>
                    {
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(300), "B");
                        ClassicAssert.IsFalse(p.MoveToSnapPoint(snapPointBeyondDuration), "Moved to snap point beyond duration again.");
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(300), "C");
                    }),
                },
                setupPlayback: playback =>
                {
                    playback.AddSnapPoint(new MetricTimeSpan(0, 0, 0, 200));
                    snapPointBeyondDuration = playback.AddSnapPoint(new MetricTimeSpan(0, 0, 0, 700));
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(400)),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void Snapping_MoveToSnapPoint_BeyondPlaybackEnd()
        {
            SnapPoint snapPointBeyondPlaybackEnd = null;

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap),
                    new TimedEvent(new NoteOnEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(700), TempoMap),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(900), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(100, p =>
                    {
                        ClassicAssert.IsTrue(p.MoveToNextSnapPoint(), "Failed to move to snap point.");
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(200), "A");
                        ClassicAssert.IsFalse(p.MoveToNextSnapPoint(), "Moved to snap point beyond duration.");
                    }),
                    new PlaybackAction(100, p =>
                    {
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(300), "B");
                        ClassicAssert.IsFalse(p.MoveToSnapPoint(snapPointBeyondPlaybackEnd), "Moved to snap point beyond duration again.");
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(300), "C");
                    }),
                },
                setupPlayback: playback =>
                {
                    playback.PlaybackEnd = new MetricTimeSpan(0, 0, 0, 600);

                    playback.AddSnapPoint(new MetricTimeSpan(0, 0, 0, 200));
                    snapPointBeyondPlaybackEnd = playback.AddSnapPoint(new MetricTimeSpan(0, 0, 0, 650));
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(400)),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void Snapping_MoveToSnapPoint_BeyondPlaybackStart()
        {
            SnapPoint snapPointBeyondStart = null;

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new TextEvent("A"))
                        .SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteOnEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(200), TempoMap),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(600), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(200, p =>
                    {
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(400), "A");
                        p.Stop();
                        ClassicAssert.IsTrue(p.MoveToPreviousSnapPoint(), "Failed to move to snap point.");
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(300), "B");
                        ClassicAssert.IsFalse(p.MoveToPreviousSnapPoint(), "Moved to snap point beyond start.");
                        p.Start();
                    }),
                    new PlaybackAction(100, p =>
                    {
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(400), "C");
                        ClassicAssert.IsFalse(p.MoveToSnapPoint(snapPointBeyondStart), "Moved to snap point beyond start again.");
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(400), "D");
                    }),
                },
                setupPlayback: playback =>
                {
                    playback.AddSnapPoint(new MetricTimeSpan(0, 0, 0, 300));
                    snapPointBeyondStart = playback.AddSnapPoint(new MetricTimeSpan(0, 0, 0, 100));

                    playback.PlaybackStart = new MetricTimeSpan(0, 0, 0, 200);
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(500)),
                });
        }

        #endregion
    }
}
