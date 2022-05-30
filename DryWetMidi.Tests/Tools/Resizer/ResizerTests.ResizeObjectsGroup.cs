using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed partial class ResizerTests
    {
        #region Constants

        private static readonly TempoMap CustomTempoMap = GetTempoMap();
        private static readonly ObjectsFactory ObjectsFactory = new ObjectsFactory(CustomTempoMap);

        #endregion

        #region Test methods

        [Test]
        public void ResizeObjectsGroup_Empty() => ResizeObjectsGroup(
            timedObjects: Array.Empty<ITimedObject>(),
            length: (MidiTimeSpan)100,
            settings: null,
            expectedObjects: Array.Empty<ITimedObject>());

        [Test]
        public void ResizeObjectsGroup_TimedEvents_Single_1() => ResizeObjectsGroup(
            timedObjects: new[]
            {
                new TimedEvent(new TextEvent("A"))
            },
            length: (MidiTimeSpan)100,
            settings: null,
            expectedObjects: new[]
            {
                new TimedEvent(new TextEvent("A"))
            });

        [Test]
        public void ResizeObjectsGroup_TimedEvents_Single_2() => ResizeObjectsGroup(
            timedObjects: new[]
            {
                new TimedEvent(new TextEvent("A"), 20)
            },
            length: (MidiTimeSpan)100,
            settings: null,
            expectedObjects: new[]
            {
                new TimedEvent(new TextEvent("A"), 20)
            });

        [Test]
        public void ResizeObjectsGroup_TimedEvents_Multiple_1() => ResizeObjectsGroup(
            timedObjects: new[]
            {
                new TimedEvent(new TextEvent("A"), 20),
                new TimedEvent(new TextEvent("B"), 20),
            },
            length: (MidiTimeSpan)100,
            settings: null,
            expectedObjects: new[]
            {
                new TimedEvent(new TextEvent("A"), 20),
                new TimedEvent(new TextEvent("B"), 20),
            });

        [Test]
        public void ResizeObjectsGroup_TimedEvents_Multiple_2() => ResizeObjectsGroup(
            timedObjects: new[]
            {
                new TimedEvent(new TextEvent("A"), 20),
                new TimedEvent(new TextEvent("B"), 50),
            },
            length: (MidiTimeSpan)90,
            settings: null,
            expectedObjects: new[]
            {
                new TimedEvent(new TextEvent("A"), 20),
                new TimedEvent(new TextEvent("B"), 110),
            });

        [Test]
        public void ResizeObjectsGroup_TimedEvents_Multiple_3() => ResizeObjectsGroup(
            timedObjects: new[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new TimedEvent(new TextEvent("B"), 50),
            },
            length: (MidiTimeSpan)50,
            settings: null,
            expectedObjects: new[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new TimedEvent(new TextEvent("B"), 50),
            });

        [Test]
        public void ResizeObjectsGroup_TimedEvents_Multiple_4() => ResizeObjectsGroup(
            timedObjects: new[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new TimedEvent(new TextEvent("B"), 50),
            },
            length: (MidiTimeSpan)200,
            settings: null,
            expectedObjects: new[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new TimedEvent(new TextEvent("B"), 200),
            });

        [Test]
        public void ResizeObjectsGroup_Notes_ZeroLength()
        {
            var notes = new[]
            {
                ObjectsFactory.GetNote("0", "0"),
                ObjectsFactory.GetNote("10", "0"),
                ObjectsFactory.GetNote("100", "0"),
            };

            ResizeObjectsGroup(
                timedObjects: notes,
                length: (MidiTimeSpan)1000,
                settings: null,
                tempoMap: CustomTempoMap,
                expectedObjects: ObjectsFactory.WithTimesAndLengths(notes,
                    "0", "0",
                    "100", "0",
                    "1000", "0"));
        }

        [Test]
        public void ResizeObjectsGroup_Notes_Stretch_Midi()
        {
            var notes = new[]
            {
                ObjectsFactory.GetNote("0", "10"),
                ObjectsFactory.GetNote("10", "20"),
                ObjectsFactory.GetNote("100", "400"),
            };

            ResizeObjectsGroup(
                timedObjects: notes,
                length: (MidiTimeSpan)2000,
                settings: null,
                tempoMap: CustomTempoMap,
                expectedObjects: ObjectsFactory.WithTimesAndLengths(notes,
                    "0", "40",
                    "40", "80",
                    "400", "1600"));
        }

        [Test]
        public void ResizeObjectsGroup_Notes_Stretch_Metric()
        {
            var notes = new[]
            {
                ObjectsFactory.GetNote("0:0:0", "0:0:15"),
                ObjectsFactory.GetNote("0:0:10", "0:0:1"),
                ObjectsFactory.GetNote("0:1:0", "0:2:0"),
            };

            ResizeObjectsGroup(
                timedObjects: notes,
                length: new MetricTimeSpan(0, 4, 30),
                settings: new ObjectsGroupResizingSettings
                {
                    DistanceCalculationType = TimeSpanType.Metric
                },
                tempoMap: CustomTempoMap,
                expectedObjects: ObjectsFactory.WithTimesAndLengths(notes,
                    "0:0:0", "0:0:22:500",
                    "0:0:15", "0:0:1:500",
                    "0:1:30", "0:3:0"));
        }

        [Test]
        public void ResizeObjectsGroup_Notes_Shrink_Midi()
        {
            var notes = new[]
            {
                ObjectsFactory.GetNote("0", "10"),
                ObjectsFactory.GetNote("10", "20"),
                ObjectsFactory.GetNote("100", "400"),
            };

            ResizeObjectsGroup(
                timedObjects: notes,
                length: (MidiTimeSpan)100,
                settings: null,
                tempoMap: CustomTempoMap,
                expectedObjects: ObjectsFactory.WithTimesAndLengths(notes,
                    "0", "2",
                    "2", "4",
                    "20", "80"));
        }

        [Test]
        public void ResizeObjectsGroup_Notes_Shrink_Metric()
        {
            var notes = new[]
            {
                ObjectsFactory.GetNote("0:0:0", "0:0:15"),
                ObjectsFactory.GetNote("0:0:10", "0:0:1"),
                ObjectsFactory.GetNote("0:1:0", "0:1:0"),
            };

            ResizeObjectsGroup(
                timedObjects: notes,
                length: new MetricTimeSpan(0, 0, 30),
                settings: new ObjectsGroupResizingSettings
                {
                    DistanceCalculationType = TimeSpanType.Metric
                },
                tempoMap: CustomTempoMap,
                expectedObjects: ObjectsFactory.WithTimesAndLengths(notes,
                    "0:0:0", "0:0:3:750",
                    "0:0:2:500", "0:0:0:250",
                    "0:0:15", "0:0:15"));
        }

        [Test]
        public void ResizeObjectsGroup_ByRatio_Empty() => ResizeObjectsGroup(
            timedObjects: Array.Empty<ITimedObject>(),
            ratio: 2.0,
            settings: null,
            expectedObjects: Array.Empty<ITimedObject>());

        [Test]
        public void ResizeObjectsGroup_ByRatio_TimedEvents_Single_1() => ResizeObjectsGroup(
            timedObjects: new[]
            {
                new TimedEvent(new TextEvent("A"))
            },
            ratio: 2.0,
            settings: null,
            expectedObjects: new[]
            {
                new TimedEvent(new TextEvent("A"))
            });

        [Test]
        public void ResizeObjectsGroup_ByRatio_TimedEvents_Single_2() => ResizeObjectsGroup(
            timedObjects: new[]
            {
                new TimedEvent(new TextEvent("A"), 20)
            },
            ratio: 5.0,
            settings: null,
            expectedObjects: new[]
            {
                new TimedEvent(new TextEvent("A"), 20)
            });

        [Test]
        public void ResizeObjectsGroup_ByRatio_TimedEvents_Multiple_1() => ResizeObjectsGroup(
            timedObjects: new[]
            {
                new TimedEvent(new TextEvent("A"), 20),
                new TimedEvent(new TextEvent("B"), 20),
            },
            ratio: 0.5,
            settings: null,
            expectedObjects: new[]
            {
                new TimedEvent(new TextEvent("A"), 20),
                new TimedEvent(new TextEvent("B"), 20),
            });

        [Test]
        public void ResizeObjectsGroup_ByRatio_TimedEvents_Multiple_2() => ResizeObjectsGroup(
            timedObjects: new[]
            {
                new TimedEvent(new TextEvent("A"), 20),
                new TimedEvent(new TextEvent("B"), 50),
            },
            ratio: 3.0,
            settings: null,
            expectedObjects: new[]
            {
                new TimedEvent(new TextEvent("A"), 20),
                new TimedEvent(new TextEvent("B"), 110),
            });

        [Test]
        public void ResizeObjectsGroup_ByRatio_TimedEvents_Multiple_3() => ResizeObjectsGroup(
            timedObjects: new[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new TimedEvent(new TextEvent("B"), 50),
            },
            ratio: 1.0,
            settings: null,
            expectedObjects: new[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new TimedEvent(new TextEvent("B"), 50),
            });

        [Test]
        public void ResizeObjectsGroup_ByRatio_TimedEvents_Multiple_4() => ResizeObjectsGroup(
            timedObjects: new[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new TimedEvent(new TextEvent("B"), 50),
            },
            ratio: 4.0,
            settings: null,
            expectedObjects: new[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new TimedEvent(new TextEvent("B"), 200),
            });

        [Test]
        public void ResizeObjectsGroup_ByRatio_Notes_ZeroLength()
        {
            var notes = new[]
            {
                ObjectsFactory.GetNote("0", "0"),
                ObjectsFactory.GetNote("10", "0"),
                ObjectsFactory.GetNote("100", "0"),
            };

            ResizeObjectsGroup(
                timedObjects: notes,
                ratio: 10.0,
                settings: null,
                tempoMap: CustomTempoMap,
                expectedObjects: ObjectsFactory.WithTimesAndLengths(notes,
                    "0", "0",
                    "100", "0",
                    "1000", "0"));
        }

        [Test]
        public void ResizeObjectsGroup_ByRatio_Notes_Stretch_Midi()
        {
            var notes = new[]
            {
                ObjectsFactory.GetNote("0", "10"),
                ObjectsFactory.GetNote("10", "20"),
                ObjectsFactory.GetNote("100", "400"),
            };

            ResizeObjectsGroup(
                timedObjects: notes,
                ratio: 4.0,
                settings: null,
                tempoMap: CustomTempoMap,
                expectedObjects: ObjectsFactory.WithTimesAndLengths(notes,
                    "0", "40",
                    "40", "80",
                    "400", "1600"));
        }

        [Test]
        public void ResizeObjectsGroup_ByRatio_Notes_Stretch_Metric()
        {
            var notes = new[]
            {
                ObjectsFactory.GetNote("0:0:0", "0:0:15"),
                ObjectsFactory.GetNote("0:0:10", "0:0:1"),
                ObjectsFactory.GetNote("0:1:0", "0:2:0"),
            };

            ResizeObjectsGroup(
                timedObjects: notes,
                ratio: 1.5,
                settings: new ObjectsGroupResizingSettings
                {
                    DistanceCalculationType = TimeSpanType.Metric
                },
                tempoMap: CustomTempoMap,
                expectedObjects: ObjectsFactory.WithTimesAndLengths(notes,
                    "0:0:0", "0:0:22:500",
                    "0:0:15", "0:0:1:500",
                    "0:1:30", "0:3:0"));
        }

        [Test]
        public void ResizeObjectsGroup_ByRatio_Notes_Shrink_Midi()
        {
            var notes = new[]
            {
                ObjectsFactory.GetNote("0", "10"),
                ObjectsFactory.GetNote("10", "20"),
                ObjectsFactory.GetNote("100", "400"),
            };

            ResizeObjectsGroup(
                timedObjects: notes,
                ratio: 0.2,
                settings: null,
                tempoMap: CustomTempoMap,
                expectedObjects: ObjectsFactory.WithTimesAndLengths(notes,
                    "0", "2",
                    "2", "4",
                    "20", "80"));
        }

        [Test]
        public void ResizeObjectsGroup_ByRatio_Notes_Shrink_Metric()
        {
            var notes = new[]
            {
                ObjectsFactory.GetNote("0:0:0", "0:0:15"),
                ObjectsFactory.GetNote("0:0:10", "0:0:1"),
                ObjectsFactory.GetNote("0:1:0", "0:1:0"),
            };

            ResizeObjectsGroup(
                timedObjects: notes,
                ratio: 0.25,
                settings: new ObjectsGroupResizingSettings
                {
                    DistanceCalculationType = TimeSpanType.Metric
                },
                tempoMap: CustomTempoMap,
                expectedObjects: ObjectsFactory.WithTimesAndLengths(notes,
                    "0:0:0", "0:0:3:750",
                    "0:0:2:500", "0:0:0:250",
                    "0:0:15", "0:0:15"));
        }

        #endregion

        #region Private methods

        private void ResizeObjectsGroup(
            ICollection<ITimedObject> timedObjects,
            ITimeSpan length,
            ObjectsGroupResizingSettings settings,
            ICollection<ITimedObject> expectedObjects) => ResizeObjectsGroup(
                timedObjects,
                length,
                TempoMap.Default,
                settings,
                expectedObjects);

        private void ResizeObjectsGroup(
            ICollection<ITimedObject> timedObjects,
            ITimeSpan length,
            TempoMap tempoMap,
            ObjectsGroupResizingSettings settings,
            ICollection<ITimedObject> expectedObjects)
        {
            timedObjects.ResizeObjectsGroup(length, tempoMap, settings);
            MidiAsserts.AreEqual(expectedObjects, timedObjects, "Invalid objects.");
        }

        private void ResizeObjectsGroup(
            ICollection<ITimedObject> timedObjects,
            double ratio,
            ObjectsGroupResizingSettings settings,
            ICollection<ITimedObject> expectedObjects) => ResizeObjectsGroup(
                timedObjects,
                ratio,
                TempoMap.Default,
                settings,
                expectedObjects);

        private void ResizeObjectsGroup(
            ICollection<ITimedObject> timedObjects,
            double ratio,
            TempoMap tempoMap,
            ObjectsGroupResizingSettings settings,
            ICollection<ITimedObject> expectedObjects)
        {
            timedObjects.ResizeObjectsGroup(ratio, tempoMap, settings);
            MidiAsserts.AreEqual(expectedObjects, timedObjects, "Invalid objects.");
        }

        private static TempoMap GetTempoMap()
        {
            using (var tempoMapManager = new TempoMapManager())
            {
                tempoMapManager.SetTempo(new MetricTimeSpan(0, 0, 1), Tempo.FromBeatsPerMinute(60));
                tempoMapManager.SetTempo(new MetricTimeSpan(0, 0, 10), Tempo.FromBeatsPerMinute(150));
                tempoMapManager.SetTempo(new MetricTimeSpan(0, 0, 50), Tempo.FromBeatsPerMinute(100));

                return tempoMapManager.TempoMap;
            }
        }

        #endregion
    }
}
