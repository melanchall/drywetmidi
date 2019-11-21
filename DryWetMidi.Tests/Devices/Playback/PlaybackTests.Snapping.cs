using System;
using System.Linq;
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
        #region Test methods

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
    }
}
