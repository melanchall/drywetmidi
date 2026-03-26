using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    // TODO: more tests
    [TestFixture]
    public sealed partial class TimedObjectUtilitiesTests
    {
        #region Constants

        private static readonly TempoMap TempoMap = TempoMap.Create(
            new TicksPerQuarterNoteTimeDivision(1000));

        #endregion

        #region Test methods

        #region AtTime

        [Test]
        public void AtTime_EmptyCollection([Values(0, 10)] long time) => AtTime(
            timedObjects: Array.Empty<ITimedObject>(),
            time: time,
            expectedObjects: Array.Empty<ITimedObject>());

        [Test]
        public void AtTime_TimedObjects_Single_1([Values(0, 10)] long time) => AtTime(
            timedObjects: new[]
            {
                GetTimedEvent("A", time),
            },
            time: time,
            expectedObjects: new[]
            {
                GetTimedEvent("A", time),
            });

        [Test]
        public void AtTime_TimedObjects_Single_2([Values(0, 10)] long time) => AtTime(
            timedObjects: new[]
            {
                GetTimedEvent("A", time),
            },
            time: time + 1,
            expectedObjects: Array.Empty<ITimedObject>());

        [Test]
        public void AtTime_TimedObjects_Single_3([Values(1, 10)] long time) => AtTime(
            timedObjects: new[]
            {
                GetTimedEvent("A", time)
            },
            time: time - 1,
            expectedObjects: Array.Empty<ITimedObject>());

        [Test]
        public void AtTime_TimedObjects_1() => AtTime(
            timedObjects: new[]
            {
                GetTimedEvent("A", 10),
                GetTimedEvent("B1", 20),
                null,
                GetTimedEvent("B2", 20),
                GetTimedEvent("C", 30),
                GetTimedEvent("D1", 40),
                GetTimedEvent("D2", 40),
                null,
                GetTimedEvent("D3", 40),
            },
            time: 0,
            expectedObjects: Array.Empty<ITimedObject>());

        [Test]
        public void AtTime_TimedObjects_2() => AtTime(
            timedObjects: new[]
            {
                GetTimedEvent("A", 10),
                GetTimedEvent("B1", 20),
                GetTimedEvent("B2", 20),
                GetTimedEvent("C", 30),
                GetTimedEvent("D1", 40),
                GetTimedEvent("D2", 40),
                GetTimedEvent("D3", 40),
            },
            time: 10,
            expectedObjects: new[]
            {
                GetTimedEvent("A", 10),
            });

        [Test]
        public void AtTime_TimedObjects_3() => AtTime(
            timedObjects: new[]
            {
                GetTimedEvent("A", 10),
                GetTimedEvent("B1", 20),
                null,
                null,
                null,
                GetTimedEvent("B2", 20),
                GetTimedEvent("C", 30),
                GetTimedEvent("D1", 40),
                GetTimedEvent("D2", 40),
                null,
                GetTimedEvent("D3", 40),
            },
            time: 15,
            expectedObjects: Array.Empty<ITimedObject>());

        [Test]
        public void AtTime_TimedObjects_4() => AtTime(
            timedObjects: new[]
            {
                null,
                GetTimedEvent("A", 10),
                GetTimedEvent("B1", 20),
                GetTimedEvent("B2", 20),
                GetTimedEvent("C", 30),
                GetTimedEvent("D1", 40),
                GetTimedEvent("D2", 40),
                GetTimedEvent("D3", 40),
            },
            time: 20,
            expectedObjects: new[]
            {
                GetTimedEvent("B1", 20),
                GetTimedEvent("B2", 20),
            });

        [Test]
        public void AtTime_TimedObjects_5() => AtTime(
            timedObjects: new[]
            {
                GetTimedEvent("A", 10),
                GetTimedEvent("B1", 20),
                GetTimedEvent("B2", 20),
                GetTimedEvent("C", 30),
                GetTimedEvent("D1", 40),
                GetTimedEvent("D2", 40),
                GetTimedEvent("D3", 40),
                null,
            },
            time: 25,
            expectedObjects: Array.Empty<ITimedObject>());

        [Test]
        public void AtTime_TimedObjects_6() => AtTime(
            timedObjects: new[]
            {
                GetTimedEvent("A", 10),
                GetTimedEvent("B1", 20),
                GetTimedEvent("B2", 20),
                null,
                GetTimedEvent("C", 30),
                GetTimedEvent("D1", 40),
                GetTimedEvent("D2", 40),
                GetTimedEvent("D3", 40),
                null,
            },
            time: 30,
            expectedObjects: new[]
            {
                GetTimedEvent("C", 30),
            });

        [Test]
        public void AtTime_TimedObjects_7() => AtTime(
            timedObjects: new[]
            {
                null,
                GetTimedEvent("A", 10),
                GetTimedEvent("B1", 20),
                GetTimedEvent("B2", 20),
                null,
                GetTimedEvent("C", 30),
                GetTimedEvent("D1", 40),
                GetTimedEvent("D2", 40),
                GetTimedEvent("D3", 40),
                null,
            },
            time: 35,
            expectedObjects: Array.Empty<ITimedObject>());

        [Test]
        public void AtTime_TimedObjects_8() => AtTime(
            timedObjects: new[]
            {
                GetTimedEvent("A", 10),
                GetTimedEvent("B1", 20),
                GetTimedEvent("B2", 20),
                GetTimedEvent("C", 30),
                GetTimedEvent("D1", 40),
                GetTimedEvent("D2", 40),
                GetTimedEvent("D3", 40),
            },
            time: 40,
            expectedObjects: new[]
            {
                GetTimedEvent("D1", 40),
                GetTimedEvent("D2", 40),
                GetTimedEvent("D3", 40),
            });

        [Test]
        public void AtTime_TimedObjects_9() => AtTime(
            timedObjects: new[]
            {
                GetTimedEvent("A", 10),
                GetTimedEvent("B1", 20),
                GetTimedEvent("B2", 20),
                GetTimedEvent("C", 30),
                GetTimedEvent("D1", 40),
                null,
                null,
                GetTimedEvent("D2", 40),
                GetTimedEvent("D3", 40),
            },
            time: 50,
            expectedObjects: Array.Empty<ITimedObject>());

        [Test]
        public void AtTime_LengthedObjects_1() => AtTime(
            timedObjects: new[]
            {
                new Note((SevenBitNumber)70, 20, 10),
                new Note((SevenBitNumber)80, 100, 20),
            },
            time: 0,
            expectedObjects: Array.Empty<ITimedObject>());

        [Test]
        public void AtTime_LengthedObjects_2([Values(10, 15)] long time) => AtTime(
            timedObjects: new[]
            {
                new Note((SevenBitNumber)70, 20, 10),
                new Note((SevenBitNumber)80, 100, 20),
            },
            time: time,
            expectedObjects: new[]
            {
                new Note((SevenBitNumber)70, 20, 10),
            });

        [Test]
        public void AtTime_LengthedObjects_3([Values(20, 25, 30)] long time) => AtTime(
            timedObjects: new[]
            {
                new Note((SevenBitNumber)70, 20, 10),
                new Note((SevenBitNumber)80, 100, 20),
            },
            time: time,
            expectedObjects: new[]
            {
                new Note((SevenBitNumber)70, 20, 10),
                new Note((SevenBitNumber)80, 100, 20),
            });

        [Test]
        public void AtTime_LengthedObjects_4([Values(35, 50, 120)] long time) => AtTime(
            timedObjects: new[]
            {
                new Note((SevenBitNumber)70, 20, 10),
                new Note((SevenBitNumber)80, 100, 20),
            },
            time: time,
            expectedObjects: new[]
            {
                new Note((SevenBitNumber)80, 100, 20),
            });

        [Test]
        public void AtTime_LengthedObjects_5() => AtTime(
            timedObjects: new[]
            {
                new Note((SevenBitNumber)70, 20, 10),
                new Note((SevenBitNumber)80, 100, 20),
            },
            time: 120,
            expectedObjects: new[]
            {
                new Note((SevenBitNumber)80, 100, 20),
            });

        [Test]
        public void AtTime_LengthedObjects_6() => AtTime(
            timedObjects: new[]
            {
                new Note((SevenBitNumber)70, 20, 10),
                new Note((SevenBitNumber)80, 100, 20),
            },
            time: 150,
            expectedObjects: Array.Empty<ITimedObject>());

        [Test]
        public void AtTime_MixedObjects_1() => AtTime(
            timedObjects: new ITimedObject[]
            {
                GetTimedEvent("A", 0),
                new Note((SevenBitNumber)70, 20, 10),
                GetTimedEvent("C", 30),
                GetTimedEvent("B", 10),
                new Note((SevenBitNumber)80, 100, 20),
            },
            time: 0,
            expectedObjects: new ITimedObject[]
            {
                GetTimedEvent("A", 0),
            });

        [Test]
        public void AtTime_MixedObjects_2() => AtTime(
            timedObjects: new ITimedObject[]
            {
                GetTimedEvent("A", 0),
                new Note((SevenBitNumber)70, 20, 10),
                GetTimedEvent("C", 30),
                GetTimedEvent("B", 10),
                new Note((SevenBitNumber)80, 100, 20),
            },
            time: 10,
            expectedObjects: new ITimedObject[]
            {
                new Note((SevenBitNumber)70, 20, 10),
                GetTimedEvent("B", 10),
            });

        [Test]
        public void AtTime_MixedObjects_3() => AtTime(
            timedObjects: new ITimedObject[]
            {
                GetTimedEvent("A", 0),
                new Note((SevenBitNumber)70, 20, 10),
                GetTimedEvent("C", 30),
                GetTimedEvent("B", 10),
                new Note((SevenBitNumber)80, 100, 20),
            },
            time: 15,
            expectedObjects: new ITimedObject[]
            {
                new Note((SevenBitNumber)70, 20, 10),
            });

        [Test]
        public void AtTime_MixedObjects_4() => AtTime(
            timedObjects: new ITimedObject[]
            {
                GetTimedEvent("A", 0),
                new Note((SevenBitNumber)70, 20, 10),
                GetTimedEvent("C", 30),
                GetTimedEvent("B", 10),
                new Note((SevenBitNumber)80, 100, 20),
            },
            time: 20,
            expectedObjects: new ITimedObject[]
            {
                new Note((SevenBitNumber)70, 20, 10),
                new Note((SevenBitNumber)80, 100, 20),
            });

        [Test]
        public void AtTime_MixedObjects_5() => AtTime(
            timedObjects: new ITimedObject[]
            {
                GetTimedEvent("A", 0),
                new Note((SevenBitNumber)70, 20, 10),
                null,
                GetTimedEvent("C", 30),
                GetTimedEvent("B", 10),
                null,
                null,
                new Note((SevenBitNumber)80, 100, 20),
                null,
            },
            time: 30,
            expectedObjects: new ITimedObject[]
            {
                new Note((SevenBitNumber)70, 20, 10),
                GetTimedEvent("C", 30),
                new Note((SevenBitNumber)80, 100, 20),
            });

        [Test]
        public void AtTime_MixedObjects_6() => AtTime(
            timedObjects: new ITimedObject[]
            {
                GetTimedEvent("A", 0),
                new Note((SevenBitNumber)70, 20, 10),
                GetTimedEvent("C", 30),
                GetTimedEvent("B", 10),
                new Note((SevenBitNumber)80, 100, 20),
            },
            time: 50,
            expectedObjects: new ITimedObject[]
            {
                new Note((SevenBitNumber)80, 100, 20),
            });

        [Test]
        public void AtTime_MixedObjects_7() => AtTime(
            timedObjects: new ITimedObject[]
            {
                GetTimedEvent("A", 0),
                new Note((SevenBitNumber)70, 20, 10),
                GetTimedEvent("C", 30),
                GetTimedEvent("B", 10),
                new Note((SevenBitNumber)80, 100, 20),
            },
            time: 120,
            expectedObjects: new ITimedObject[]
            {
                new Note((SevenBitNumber)80, 100, 20),
            });

        [Test]
        public void AtTime_MixedObjects_8() => AtTime(
            timedObjects: new ITimedObject[]
            {
                GetTimedEvent("A", 0),
                new Note((SevenBitNumber)70, 20, 10),
                GetTimedEvent("C", 30),
                GetTimedEvent("B", 10),
                new Note((SevenBitNumber)80, 100, 20),
            },
            time: 150,
            expectedObjects: Array.Empty<ITimedObject>());

        [Test]
        public void AtTime_MixedObjects_Metric_1() => AtTime(
            timedObjects: new ITimedObject[]
            {
                GetTimedEventMs("A", 0),
                GetNoteMs(70, 10, 20),
                GetTimedEventMs("C", 30),
                GetTimedEventMs("B", 10),
                GetNoteMs(80, 20, 100),
            },
            time: new MetricTimeSpan(),
            tempoMap: TempoMap,
            expectedObjects: new ITimedObject[]
            {
                GetTimedEventMs("A", 0),
            });

        [Test]
        public void AtTime_MixedObjects_Metric_2() => AtTime(
            timedObjects: new ITimedObject[]
            {
                GetTimedEventMs("A", 0),
                GetNoteMs(70, 10, 20),
                GetTimedEventMs("C", 30),
                GetTimedEventMs("B", 10),
                GetNoteMs(80, 20, 100),
            },
            time: new MetricTimeSpan(0, 0, 0, 10),
            tempoMap: TempoMap,
            expectedObjects: new ITimedObject[]
            {
                GetNoteMs(70, 10, 20),
                GetTimedEventMs("B", 10),
            });

        [Test]
        public void AtTime_MixedObjects_Metric_3() => AtTime(
            timedObjects: new ITimedObject[]
            {
                GetTimedEventMs("A", 0),
                GetNoteMs(70, 10, 20),
                GetTimedEventMs("C", 30),
                GetTimedEventMs("B", 10),
                GetNoteMs(80, 20, 100),
            },
            time: new MetricTimeSpan(0, 0, 0, 15),
            tempoMap: TempoMap,
            expectedObjects: new ITimedObject[]
            {
                GetNoteMs(70, 10, 20),
            });

        [Test]
        public void AtTime_MixedObjects_Metric_4() => AtTime(
            timedObjects: new ITimedObject[]
            {
                GetTimedEventMs("A", 0),
                GetNoteMs(70, 10, 20),
                GetTimedEventMs("C", 30),
                GetTimedEventMs("B", 10),
                GetNoteMs(80, 20, 100),
            },
            time: new MetricTimeSpan(0, 0, 0, 20),
            tempoMap: TempoMap,
            expectedObjects: new ITimedObject[]
            {
                GetNoteMs(70, 10, 20),
                GetNoteMs(80, 20, 100),
            });

        [Test]
        public void AtTime_MixedObjects_Metric_5() => AtTime(
            timedObjects: new ITimedObject[]
            {
                GetTimedEventMs("A", 0),
                null,
                null,
                GetNoteMs(70, 10, 20),
                GetTimedEventMs("C", 30),
                GetTimedEventMs("B", 10),
                null,
                GetNoteMs(80, 20, 100),
                null,
            },
            time: new MetricTimeSpan(0, 0, 0, 30),
            tempoMap: TempoMap,
            expectedObjects: new ITimedObject[]
            {
                GetNoteMs(70, 10, 20),
                GetTimedEventMs("C", 30),
                GetNoteMs(80, 20, 100),
            });

        [Test]
        public void AtTime_MixedObjects_Metric_6() => AtTime(
            timedObjects: new ITimedObject[]
            {
                GetTimedEventMs("A", 0),
                null,
                null,
                GetNoteMs(70, 10, 20),
                GetTimedEventMs("C", 30),
                GetTimedEventMs("B", 10),
                null,
                GetNoteMs(80, 20, 100),
                null,
            },
            time: new MetricTimeSpan(0, 0, 0, 50),
            tempoMap: TempoMap,
            expectedObjects: new ITimedObject[]
            {
                GetNoteMs(80, 20, 100),
            });

        [Test]
        public void AtTime_MixedObjects_Metric_7() => AtTime(
            timedObjects: new ITimedObject[]
            {
                GetTimedEventMs("A", 0),
                null,
                null,
                GetNoteMs(70, 10, 20),
                GetTimedEventMs("C", 30),
                GetTimedEventMs("B", 10),
                null,
                GetNoteMs(80, 20, 100),
                null,
            },
            time: new MetricTimeSpan(0, 0, 0, 120),
            tempoMap: TempoMap,
            expectedObjects: new ITimedObject[]
            {
                GetNoteMs(80, 20, 100),
            });

        [Test]
        public void AtTime_MixedObjects_Metric_8() => AtTime(
            timedObjects: new ITimedObject[]
            {
                GetTimedEventMs("A", 0),
                null,
                null,
                GetNoteMs(70, 10, 20),
                GetTimedEventMs("C", 30),
                GetTimedEventMs("B", 10),
                null,
                GetNoteMs(80, 20, 100),
                null,
            },
            time: new MetricTimeSpan(0, 0, 0, 150),
            tempoMap: TempoMap,
            expectedObjects: Array.Empty<ITimedObject>());

        #endregion

        #region AtTimeRange

        [Test]
        public void AtTimeRange_EmptyCollection([Values(0, 10)] long startTime, [Values(10, 100)] long endTime) => AtTimeRange(
            timedObjects: Array.Empty<ITimedObject>(),
            startTime: startTime,
            endTime: endTime,
            expectedObjects: Array.Empty<ITimedObject>());

        [Test]
        public void AtTimeRange_TimedObjects_Single_1([Values(0, 10)] long startTime, [Values(10, 100)] long endTime) => AtTimeRange(
            timedObjects: new[]
            {
                GetTimedEvent("A", startTime),
            },
            startTime: startTime,
            endTime: endTime,
            expectedObjects: new[]
            {
                GetTimedEvent("A", startTime),
            });

        [Test]
        public void AtTimeRange_TimedObjects_Single_2([Values(0, 10)] long startTime, [Values(10, 100)] long endTime) => AtTimeRange(
            timedObjects: new[]
            {
                GetTimedEvent("A", endTime),
            },
            startTime: startTime,
            endTime: endTime,
            expectedObjects: new[]
            {
                GetTimedEvent("A", endTime),
            });

        [Test]
        public void AtTimeRange_TimedObjects_Single_3([Values(0, 10)] long startTime, [Values(10, 100)] long endTime) => AtTimeRange(
            timedObjects: new[]
            {
                GetTimedEvent("A", startTime + (endTime - startTime) / 2),
            },
            startTime: startTime,
            endTime: endTime,
            expectedObjects: new[]
            {
                GetTimedEvent("A", startTime + (endTime - startTime) / 2),
            });

        [Test]
        public void AtTimeRange_TimedObjects_Single_4([Values(0, 10)] long time) => AtTimeRange(
            timedObjects: new[]
            {
                GetTimedEvent("A", time),
            },
            startTime: time + 1,
            endTime: time + 10,
            expectedObjects: Array.Empty<ITimedObject>());

        [Test]
        public void AtTimeRange_TimedObjects_Single_5([Values(5, 10)] long time) => AtTimeRange(
            timedObjects: new[]
            {
                GetTimedEvent("A", time),
            },
            startTime: time - 5,
            endTime: time - 1,
            expectedObjects: Array.Empty<ITimedObject>());

        [Test]
        public void AtTimeRange_LengthedObjects_Single_1([Values(0, 10)] long objectTime, [Values(5, 10)] long objectLength) => AtTimeRange(
            timedObjects: new[]
            {
                new Note((SevenBitNumber)80, objectLength, objectTime),
                null,
            },
            startTime: 0,
            endTime: 20,
            expectedObjects: new[]
            {
                new Note((SevenBitNumber)80, objectLength, objectTime),
            });

        [Test]
        public void AtTimeRange_LengthedObjects_Single_2([Values(0, 10)] long objectTime, [Values(10, 20)] long objectLength) => AtTimeRange(
            timedObjects: new[]
            {
                new Note((SevenBitNumber)80, objectLength, objectTime),
                null,
            },
            startTime: 10,
            endTime: 40,
            expectedObjects: new[]
            {
                new Note((SevenBitNumber)80, objectLength, objectTime),
            });

        [Test]
        public void AtTimeRange_LengthedObjects_Single_3([Values(30, 40)] int objectTimeMs, [Values(10, 20)] int objectLengthMs) => AtTimeRange(
            timedObjects: new[]
            {
                null,
                GetNoteMs(80, objectLengthMs, objectTimeMs),
                null,
            },
            startTime: new MetricTimeSpan(0, 0, 0, 10),
            endTime: new MetricTimeSpan(0, 0, 0, 40),
            tempoMap: TempoMap,
            expectedObjects: new[]
            {
                GetNoteMs(80, objectLengthMs, objectTimeMs),
            });

        [Test]
        public void AtTimeRange_LengthedObjects_Single_4([Values(0, 10)] long objectTime) => AtTimeRange(
            timedObjects: new[]
            {
                new Note((SevenBitNumber)80, 40, objectTime),
                null,
            },
            startTime: 10,
            endTime: 40,
            expectedObjects: new[]
            {
                new Note((SevenBitNumber)80, 40, objectTime),
            });

        [Test]
        public void AtTimeRange_MixedObjects_Metric_1() => AtTimeRange(
            timedObjects: new ITimedObject[]
            {
                GetTimedEventMs("A", 0),
                GetNoteMs(70, 10, 20),
                GetTimedEventMs("C", 30),
                GetTimedEventMs("B", 10),
                GetNoteMs(80, 20, 100),
            },
            startTime: new MetricTimeSpan(),
            endTime: new MetricTimeSpan(0, 0, 0, 150),
            tempoMap: TempoMap,
            expectedObjects: new ITimedObject[]
            {
                GetTimedEventMs("A", 0),
                GetNoteMs(70, 10, 20),
                GetTimedEventMs("C", 30),
                GetTimedEventMs("B", 10),
                GetNoteMs(80, 20, 100),
            });

        [Test]
        public void AtTimeRange_MixedObjects_Metric_2() => AtTimeRange(
            timedObjects: new ITimedObject[]
            {
                GetTimedEventMs("A", 0),
                GetNoteMs(70, 10, 20),
                GetTimedEventMs("C", 30),
                GetTimedEventMs("B", 10),
                GetNoteMs(80, 20, 100),
            },
            startTime: new MetricTimeSpan(0, 0, 0, 10),
            endTime: new MetricTimeSpan(0, 0, 0, 20),
            tempoMap: TempoMap,
            expectedObjects: new ITimedObject[]
            {
                GetNoteMs(70, 10, 20),
                GetTimedEventMs("B", 10),
                GetNoteMs(80, 20, 100),
            });

        [Test]
        public void AtTimeRange_MixedObjects_Metric_3() => AtTimeRange(
            timedObjects: new ITimedObject[]
            {
                GetTimedEventMs("A", 0),
                GetNoteMs(70, 10, 20),
                GetTimedEventMs("C", 30),
                GetTimedEventMs("B", 10),
                GetNoteMs(80, 20, 100),
            },
            startTime: new MetricTimeSpan(0, 0, 0, 15),
            endTime: new MetricTimeSpan(0, 0, 0, 30),
            tempoMap: TempoMap,
            expectedObjects: new ITimedObject[]
            {
                GetNoteMs(70, 10, 20),
                GetTimedEventMs("C", 30),
                GetNoteMs(80, 20, 100),
            });

        [Test]
        public void AtTimeRange_MixedObjects_Metric_4() => AtTimeRange(
            timedObjects: new ITimedObject[]
            {
                GetTimedEventMs("A", 0),
                GetNoteMs(70, 10, 20),
                GetTimedEventMs("C", 30),
                GetTimedEventMs("B", 10),
                GetNoteMs(80, 20, 100),
            },
            startTime: new MetricTimeSpan(0, 0, 0, 20),
            endTime: new MetricTimeSpan(0, 0, 0, 50),
            tempoMap: TempoMap,
            expectedObjects: new ITimedObject[]
            {
                GetNoteMs(70, 10, 20),
                GetTimedEventMs("C", 30),
                GetNoteMs(80, 20, 100),
            });

        [Test]
        public void AtTimeRange_MixedObjects_Metric_5() => AtTimeRange(
            timedObjects: new ITimedObject[]
            {
                GetTimedEventMs("A", 0),
                null,
                null,
                GetNoteMs(70, 10, 20),
                GetTimedEventMs("C", 30),
                GetTimedEventMs("B", 10),
                null,
                GetNoteMs(80, 20, 100),
                null,
            },
            startTime: new MetricTimeSpan(0, 0, 0, 30),
            endTime: new MetricTimeSpan(0, 0, 0, 40),
            tempoMap: TempoMap,
            expectedObjects: new ITimedObject[]
            {
                GetNoteMs(70, 10, 20),
                GetTimedEventMs("C", 30),
                GetNoteMs(80, 20, 100),
            });

        [Test]
        public void AtTimeRange_MixedObjects_Metric_6() => AtTimeRange(
            timedObjects: new ITimedObject[]
            {
                GetTimedEventMs("A", 0),
                null,
                null,
                GetNoteMs(70, 10, 20),
                GetTimedEventMs("C", 30),
                GetTimedEventMs("B", 10),
                null,
                GetNoteMs(80, 20, 100),
                null,
            },
            startTime: new MetricTimeSpan(0, 0, 0, 50),
            endTime: new MetricTimeSpan(0, 0, 0, 100),
            tempoMap: TempoMap,
            expectedObjects: new ITimedObject[]
            {
                GetNoteMs(80, 20, 100),
            });

        [Test]
        public void AtTimeRange_MixedObjects_Metric_7() => AtTimeRange(
            timedObjects: new ITimedObject[]
            {
                GetTimedEventMs("A", 0),
                null,
                null,
                GetNoteMs(70, 10, 20),
                GetTimedEventMs("C", 30),
                GetTimedEventMs("B", 10),
                null,
                GetNoteMs(80, 20, 100),
                null,
            },
            startTime: new MetricTimeSpan(0, 0, 0, 120),
            endTime: new MetricTimeSpan(0, 0, 0, 130),
            tempoMap: TempoMap,
            expectedObjects: new ITimedObject[]
            {
                GetNoteMs(80, 20, 100),
            });

        [Test]
        public void AtTimeRange_MixedObjects_Metric_8() => AtTimeRange(
            timedObjects: new ITimedObject[]
            {
                GetTimedEventMs("A", 0),
                null,
                null,
                GetNoteMs(70, 10, 20),
                GetTimedEventMs("C", 30),
                GetTimedEventMs("B", 10),
                null,
                GetNoteMs(80, 20, 100),
                null,
            },
            startTime: new MetricTimeSpan(0, 0, 0, 150),
            endTime: new MetricTimeSpan(0, 0, 0, 200),
            tempoMap: TempoMap,
            expectedObjects: Array.Empty<ITimedObject>());

        #endregion

        #endregion

        #region Private methods

        private void AtTime<TObject>(
            IEnumerable<TObject> timedObjects,
            long time,
            ICollection<TObject> expectedObjects)
            where TObject : ITimedObject
        {
            var objectsAtTime = timedObjects.AtTime((MidiTimeSpan)time, TempoMap);

            MidiAsserts.AreEqual(
                expectedObjects.Cast<ITimedObject>(),
                objectsAtTime.Cast<ITimedObject>(),
                "Objects at time are invalid on original collection.");

            objectsAtTime = new SortedLazyCollection<TObject>(timedObjects.OrderBy(obj => obj?.Time ?? -1)).AtTime((MidiTimeSpan)time, TempoMap);

            MidiAsserts.AreEqual(
                expectedObjects.OrderBy(obj => obj.Time).Cast<ITimedObject>(),
                objectsAtTime.Cast<ITimedObject>(),
                "Objects at time are invalid on sorted collection.");
        }

        private void AtTime<TObject>(
            IEnumerable<TObject> timedObjects,
            ITimeSpan time,
            TempoMap tempoMap,
            ICollection<TObject> expectedObjects)
            where TObject : ITimedObject
        {
            var objectsAtTime = timedObjects.AtTime(time, tempoMap);

            MidiAsserts.AreEqual(
                expectedObjects.Cast<ITimedObject>(),
                objectsAtTime.Cast<ITimedObject>(),
                "Objects at time are invalid on original collection.");

            objectsAtTime = new SortedLazyCollection<TObject>(timedObjects.OrderBy(obj => obj?.Time ?? -1)).AtTime(time, tempoMap);

            MidiAsserts.AreEqual(
                expectedObjects.OrderBy(obj => obj.Time).Cast<ITimedObject>(),
                objectsAtTime.Cast<ITimedObject>(),
                "Objects at time are invalid on sorted collection.");
        }

        private void AtTimeRange<TObject>(
            IEnumerable<TObject> timedObjects,
            long startTime,
            long endTime,
            ICollection<TObject> expectedObjects)
            where TObject : ITimedObject
        {
            var objectsAtTime = timedObjects.AtTimeRange((MidiTimeSpan)startTime, (MidiTimeSpan)endTime, TempoMap);

            MidiAsserts.AreEqual(
                expectedObjects.Cast<ITimedObject>(),
                objectsAtTime.Cast<ITimedObject>(),
                "Objects at time are invalid on original collection.");

            objectsAtTime = new SortedLazyCollection<TObject>(timedObjects.OrderBy(obj => obj?.Time ?? -1)).AtTimeRange((MidiTimeSpan)startTime, (MidiTimeSpan)endTime, TempoMap);

            MidiAsserts.AreEqual(
                expectedObjects.OrderBy(obj => obj.Time).Cast<ITimedObject>(),
                objectsAtTime.Cast<ITimedObject>(),
                "Objects at time are invalid on sorted collection.");
        }

        private void AtTimeRange<TObject>(
            IEnumerable<TObject> timedObjects,
            ITimeSpan startTime,
            ITimeSpan endTime,
            TempoMap tempoMap,
            ICollection<TObject> expectedObjects)
            where TObject : ITimedObject
        {
            var objectsAtTime = timedObjects.AtTimeRange(startTime, endTime, tempoMap);
            MidiAsserts.AreEqual(
                expectedObjects.Cast<ITimedObject>(),
                objectsAtTime.Cast<ITimedObject>(),
                "Objects at time are invalid on original collection.");

            objectsAtTime = new SortedLazyCollection<TObject>(timedObjects.OrderBy(obj => obj?.Time ?? -1)).AtTimeRange(startTime, endTime, tempoMap);

            MidiAsserts.AreEqual(
                expectedObjects.OrderBy(obj => obj.Time).Cast<ITimedObject>(),
                objectsAtTime.Cast<ITimedObject>(),
                "Objects at time are invalid on sorted collection.");
        }

        private TimedEvent GetTimedEventMs(string text, int time) => new TimedEvent(new TextEvent(text))
            .SetTime(new MetricTimeSpan(0, 0, 0, time), TempoMap);

        private TimedEvent GetTimedEvent(string text, long time) =>
            new TimedEvent(new TextEvent(text), time);

        private Note GetNoteMs(int noteNumber, int time, int length) => new Note((SevenBitNumber)noteNumber)
            .SetTime(new MetricTimeSpan(0, 0, 0, time), TempoMap)
            .SetLength(new MetricTimeSpan(0, 0, 0, length), TempoMap);

        #endregion
    }
}
