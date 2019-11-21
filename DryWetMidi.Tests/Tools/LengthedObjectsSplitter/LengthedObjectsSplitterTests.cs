using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    public abstract class LengthedObjectsSplitterTests<TObject> : LengthedObjectsToolTests<TObject>
        where TObject : ILengthedObject
    {
        #region Nested classes

        private sealed class SplitData
        {
            #region Constructor

            public SplitData(IEnumerable<TObject> inputObjects, IEnumerable<TObject> expectedObjects, IEnumerable<TObject> actualObjects)
            {
                InputObjects = inputObjects;
                ExpectedObjects = expectedObjects;
                ActualObjects = actualObjects;
            }

            #endregion

            #region Properties

            public IEnumerable<TObject> InputObjects { get; }

            public IEnumerable<TObject> ExpectedObjects { get; }

            public IEnumerable<TObject> ActualObjects { get; }

            #endregion
        }

        #endregion

        #region Constants

        private static readonly Dictionary<Type, TimeSpanType> TimeSpanTypes = new Dictionary<Type, TimeSpanType>
        {
            [typeof(MidiTimeSpan)] = TimeSpanType.Midi,
            [typeof(MetricTimeSpan)] = TimeSpanType.Metric,
            [typeof(MusicalTimeSpan)] = TimeSpanType.Musical,
            [typeof(BarBeatTicksTimeSpan)] = TimeSpanType.BarBeatTicks,
            [typeof(BarBeatFractionTimeSpan)] = TimeSpanType.BarBeatFraction
        };

        #endregion

        #region Constructor

        public LengthedObjectsSplitterTests(LengthedObjectMethods<TObject> methods, LengthedObjectsSplitter<TObject> splitter)
            : base(methods)
        {
            Splitter = splitter;
        }

        #endregion

        #region Properties

        protected LengthedObjectsSplitter<TObject> Splitter { get; }

        #endregion

        #region Test methods

        #region SplitByStep

        [Test]
        [Description("Split empty collection by step.")]
        public void SplitByStep_EmptyCollection()
        {
            var tempoMap = TempoMap.Default;

            var inputObjects = Enumerable.Empty<TObject>();
            var step = (MidiTimeSpan)100;
            var expectedObjects = Enumerable.Empty<TObject>();
            var actualObjects = Splitter.SplitByStep(inputObjects, step, tempoMap);

            ObjectMethods.AssertCollectionsAreEqual(expectedObjects, actualObjects);
        }

        [Test]
        [Description("Split null objects by step.")]
        public void SplitByStep_Nulls()
        {
            var tempoMap = TempoMap.Default;

            var inputObjects = new[] { default(TObject), default(TObject) };
            var step = (MidiTimeSpan)100;
            var expectedObjects = new[] { default(TObject), default(TObject) };
            var actualObjects = Splitter.SplitByStep(inputObjects, step, tempoMap);

            ObjectMethods.AssertCollectionsAreEqual(expectedObjects, actualObjects);
        }

        [Test]
        [Description("Split collection by zero step.")]
        public void SplitByStep_ZeroStep()
        {
            var tempoMap = TempoMap.Default;

            var inputObjects = CreateInputObjects(100);
            var step = (MidiTimeSpan)0;

            Assert.Throws<InvalidOperationException>(() => Splitter.SplitByStep(inputObjects, step, tempoMap).ToArray());
        }

        [Test]
        [Description("Split collection by step greater than length of any object in the collection.")]
        public void SplitByStep_BigStep()
        {
            var tempoMap = TempoMap.Default;

            var inputObjects = CreateInputObjects(100);
            var step = 1000L;
            var expectedObjects = inputObjects.Select(o => ObjectMethods.Clone(o));
            var actualObjects = Splitter.SplitByStep(inputObjects, (MidiTimeSpan)step, tempoMap);

            ObjectMethods.AssertCollectionsAreEqual(expectedObjects, actualObjects);
        }

        [Test]
        [Description("Split collection by step that equals to length of an object in the collection.")]
        public void SplitByStep_StepEqualObjectsLength()
        {
            var tempoMap = TempoMap.Default;

            var step = 1000L;
            var inputObjects = CreateInputObjects(step);
            var expectedObjects = inputObjects.Select(o => ObjectMethods.Clone(o));
            var actualObjects = Splitter.SplitByStep(inputObjects, (MidiTimeSpan)step, tempoMap);

            ObjectMethods.AssertCollectionsAreEqual(expectedObjects, actualObjects);
        }

        [Test]
        [Description("Split collection by a step that results to parts of equal length.")]
        public void SplitByStep_EqualDivision()
        {
            var tempoMap = TempoMap.Default;

            var partsNumber = 10;
            var step = 1000L;
            var inputObjects = CreateInputObjects(step * partsNumber).ToArray();
            var expectedObjects = inputObjects.SelectMany(o => Split(o, Enumerable.Range(1, partsNumber - 1).Select(i => o.Time + step * i)));
            var actualObjects = Splitter.SplitByStep(inputObjects, (MidiTimeSpan)step, tempoMap).ToArray();

            Assert.AreEqual(inputObjects.Length * partsNumber,
                            actualObjects.Length,
                            "Parts count is invalid.");
            Assert.IsTrue(actualObjects.All(o => o.Length == step),
                          "Length of some objects doesn't equal to the step.");
            ObjectMethods.AssertCollectionsAreEqual(expectedObjects, actualObjects);
        }

        [Test]
        [Description("Split collection by a step that results to parts of unequal length (short last part).")]
        public void SplitByStep_UnequalDivision()
        {
            var tempoMap = TempoMap.Default;

            var partsNumber = 10;
            var step = 1000L;
            var inputObjects = CreateInputObjects(step * (partsNumber - 1) + step / 2).ToArray();
            var expectedObjects = inputObjects.SelectMany(o => Split(o, Enumerable.Range(1, partsNumber - 1).Select(i => o.Time + step * i)));
            var actualObjects = Splitter.SplitByStep(inputObjects, (MidiTimeSpan)step, tempoMap).ToArray();

            Assert.AreEqual(inputObjects.Length * partsNumber,
                            actualObjects.Length,
                            "Parts count is invalid.");
            Assert.IsTrue(Enumerable.Range(0, inputObjects.Length)
                                    .SelectMany(i => actualObjects.Skip(partsNumber * i)
                                                                  .Take(partsNumber - 1))
                                    .All(o => o.Length == step),
                          "Length of some objects (except the last one) doesn't equal to the step.");
            Assert.IsTrue(Enumerable.Range(0, inputObjects.Length)
                                    .All(i => actualObjects.Skip(partsNumber * i)
                                                           .Take(partsNumber)
                                                           .Last()
                                                           .Length < step),
                          "Last object's length is not less than the step.");
            ObjectMethods.AssertCollectionsAreEqual(expectedObjects, actualObjects);
        }

        #endregion

        #region SplitByPartsNumber

        [Test]
        [Description("Split empty collection by parts number.")]
        public void SplitByPartsNumber_EmptyCollection()
        {
            var tempoMap = TempoMap.Default;

            var inputObjects = Enumerable.Empty<TObject>();
            var partsNumber = 100;
            var expectedObjects = Enumerable.Empty<TObject>();
            var actualObjects = Splitter.SplitByPartsNumber(inputObjects, partsNumber, TimeSpanType.Midi, tempoMap);

            ObjectMethods.AssertCollectionsAreEqual(expectedObjects, actualObjects);
        }

        [Test]
        [Description("Split null objects by parts number.")]
        public void SplitByPartsNumber_Nulls()
        {
            var tempoMap = TempoMap.Default;

            var inputObjects = new[] { default(TObject), default(TObject) };
            var partsNumber = 100;
            var expectedObjects = new[] { default(TObject), default(TObject) };
            var actualObjects = Splitter.SplitByPartsNumber(inputObjects, partsNumber, TimeSpanType.Midi, tempoMap);

            ObjectMethods.AssertCollectionsAreEqual(expectedObjects, actualObjects);
        }

        [Test]
        [Description("Split objects into one part.")]
        public void SplitByPartsNumber_OnePart()
        {
            var tempoMap = TempoMap.Default;

            var inputObjects = CreateInputObjects(100);
            var partsNumber = 1;
            var expectedObjects = inputObjects.Select(o => ObjectMethods.Clone(o));
            var actualObjects = Splitter.SplitByPartsNumber(inputObjects, partsNumber, TimeSpanType.Midi, tempoMap);

            ObjectMethods.AssertCollectionsAreEqual(expectedObjects, actualObjects);
        }

        [Test]
        [Description("Split objects of zero length by parts number.")]
        public void SplitByPartsNumber_ZeroLength()
        {
            var tempoMap = TempoMap.Default;

            var inputObjects = CreateInputObjects(0);
            var partsNumber = 10;
            var expectedObjects = inputObjects.SelectMany(o => Enumerable.Range(0, partsNumber).Select(i => ObjectMethods.Clone(o)));
            var actualObjects = Splitter.SplitByPartsNumber(inputObjects, partsNumber, TimeSpanType.Midi, tempoMap).ToArray();

            ObjectMethods.AssertCollectionsAreEqual(expectedObjects, actualObjects);
        }

        [Test]
        [Description("Split objects by parts number using MIDI length measurement resulting into parts of equal length.")]
        public void SplitByPartsNumber_Midi_EqualDivision()
        {
            SplitByPartsNumber_EqualDivision(objectsLength: 1230,
                                             partsNumber: 123,
                                             lengthType: TimeSpanType.Midi,
                                             tempoMap: TempoMap.Default);
        }

        [Test]
        [Description("Split objects by parts number using MIDI length measurement resulting into parts of unequal length.")]
        public void SplitByPartsNumber_Midi_UnequalDivision()
        {
            SplitByPartsNumber_UnequalDivision(objectsLength: 1234,
                                               partsNumber: 33,
                                               lengthType: TimeSpanType.Midi,
                                               tempoMap: TempoMap.Default);
        }

        [Test]
        [Description("Split objects by parts number using metric length measurement resulting into parts of equal length.")]
        public void SplitByPartsNumber_Metric_EqualDivision()
        {
            SplitByPartsNumber_EqualDivision(objectsLength: 1230,
                                             partsNumber: 123,
                                             lengthType: TimeSpanType.Metric,
                                             tempoMap: TempoMap.Default);
        }

        [Test]
        [Description("Split objects by parts number using metric length measurement resulting into parts of unequal length.")]
        public void SplitByPartsNumber_Metric_UnequalDivision()
        {
            SplitByPartsNumber_UnequalDivision(objectsLength: 1234,
                                               partsNumber: 33,
                                               lengthType: TimeSpanType.Metric,
                                               tempoMap: TempoMap.Default);
        }

        [Test]
        [Description("Split objects by parts number using musical length measurement resulting into parts of equal length.")]
        public void SplitByPartsNumber_Musical_EqualDivision()
        {
            SplitByPartsNumber_EqualDivision(objectsLength: 1230,
                                             partsNumber: 123,
                                             lengthType: TimeSpanType.Musical,
                                             tempoMap: TempoMap.Default);
        }

        [Test]
        [Description("Split objects by parts number using musical length measurement resulting into parts of unequal length.")]
        public void SplitByPartsNumber_Musical_UnequalDivision()
        {
            SplitByPartsNumber_UnequalDivision(objectsLength: 1234,
                                               partsNumber: 33,
                                               lengthType: TimeSpanType.Musical,
                                               tempoMap: TempoMap.Default);
        }

        #endregion

        #region SplitByGrid

        [Test]
        [Description("Split empty collection by grid.")]
        public void SplitByGrid_EmptyCollection()
        {
            SplitByGrid_ClonesExpected(inputObjects: Enumerable.Empty<TObject>(),
                                       gridStart: (MidiTimeSpan)0,
                                       gridSteps: new[] { MusicalTimeSpan.Whole },
                                       tempoMap: TempoMap.Default);
        }

        [Test]
        [Description("Split null objects by grid.")]
        public void SplitByGrid_Nulls()
        {
            SplitByGrid_ClonesExpected(inputObjects: new[] { default(TObject), default(TObject) },
                                       gridStart: (MidiTimeSpan)0,
                                       gridSteps: new[] { MusicalTimeSpan.Whole },
                                       tempoMap: TempoMap.Default);
        }

        [Test]
        [Description("Split objects of zero length by grid.")]
        public void SplitByGrid_ZeroLength()
        {
            SplitByGrid_ClonesExpected(inputObjects: CreateInputObjects(0),
                                       gridStart: (MidiTimeSpan)0,
                                       gridSteps: new[] { MusicalTimeSpan.Whole },
                                       tempoMap: TempoMap.Default);
        }

        [Test]
        [Description("Split objects by grid with empty steps collection.")]
        public void SplitByGrid_NoSteps()
        {
            SplitByGrid_ClonesExpected(inputObjects: CreateInputObjects(100),
                                       gridStart: (MidiTimeSpan)0,
                                       gridSteps: Enumerable.Empty<ITimeSpan>(),
                                       tempoMap: TempoMap.Default);
        }

        [Test]
        [Description("Split objects by grid of single MIDI step starting from zero and resulting in single part.")]
        public void SplitByGrid_OneStep_SinglePart_FromZero_Midi()
        {
            SplitByGrid_OneStep_SinglePart(gridStart: (MidiTimeSpan)0,
                                           step: (MidiTimeSpan)100,
                                           tempoMap: TempoMap.Default);
        }

        [Test]
        [Description("Split objects by grid of single MIDI step starting away from zero and resulting in single part.")]
        public void SplitByGrid_OneStep_SinglePart_AwayFromZero_Midi()
        {
            SplitByGrid_OneStep_SinglePart(gridStart: (MidiTimeSpan)100,
                                           step: (MidiTimeSpan)123,
                                           tempoMap: TempoMap.Default);
        }

        [Test]
        [Description("Split objects by grid of single metric step starting from zero and resulting in single part.")]
        public void SplitByGrid_OneStep_SinglePart_FromZero_Metric()
        {
            SplitByGrid_OneStep_SinglePart(gridStart: new MetricTimeSpan(),
                                           step: new MetricTimeSpan(0, 0, 2),
                                           tempoMap: TempoMap.Default);
        }

        [Test]
        [Description("Split objects by grid of single metric step starting away from zero and resulting in single part.")]
        public void SplitByGrid_OneStep_SinglePart_AwayFromZero_Metric()
        {
            SplitByGrid_OneStep_SinglePart(gridStart: new MetricTimeSpan(0, 0, 1),
                                           step: new MetricTimeSpan(0, 0, 0, 123),
                                           tempoMap: TempoMap.Default);
        }

        [Test]
        [Description("Split objects by grid of single musical step starting from zero and resulting in single part.")]
        public void SplitByGrid_OneStep_SinglePart_FromZero_Musical()
        {
            SplitByGrid_OneStep_SinglePart(gridStart: new MusicalTimeSpan(),
                                           step: MusicalTimeSpan.Whole,
                                           tempoMap: TempoMap.Default);
        }

        [Test]
        [Description("Split objects by grid of single musical step starting away from zero and resulting in single part.")]
        public void SplitByGrid_OneStep_SinglePart_AwayFromZero_Musical()
        {
            SplitByGrid_OneStep_SinglePart(gridStart: new MusicalTimeSpan(5, 17),
                                           step: new MusicalTimeSpan(3, 67),
                                           tempoMap: TempoMap.Default);
        }

        [Test]
        [Description("Split objects by grid of multiple MIDI steps starting from zero.")]
        public void SplitByGrid_MultipleSteps_FromZero_Midi()
        {
            //        200
            // │####### #### ##│
            // .                240
            // .       │####║#######║####║#│
            // .       .    .  |    .      |
            // .       .    . 200   .     340
            // .       .    .  |    .      |
            // |=======|====|=======|====|=======|
            // |  100  | 67 |  100  | 67 |  100  |
            // |       |    |       |    |       |
            // 0      100  167     267  334     434

            var obj1 = ObjectMethods.Create(0, 200);
            var obj2 = ObjectMethods.Create(100, 240);

            SplitByGrid_MultipleSteps(
                new[] { obj1, obj2 },
                (MidiTimeSpan)0,
                new[] { (MidiTimeSpan)100, (MidiTimeSpan)67 },
                new Dictionary<TObject, IEnumerable<TimeAndLength>>
                {
                    [obj1] = new[]
                    {
                        new TimeAndLength((MidiTimeSpan)0, (MidiTimeSpan)100),
                        new TimeAndLength((MidiTimeSpan)100, (MidiTimeSpan)67),
                        new TimeAndLength((MidiTimeSpan)167, (MidiTimeSpan)33)
                    },
                    [obj2] = new[]
                    {
                        new TimeAndLength((MidiTimeSpan)100, (MidiTimeSpan)67),
                        new TimeAndLength((MidiTimeSpan)167, (MidiTimeSpan)100),
                        new TimeAndLength((MidiTimeSpan)267, (MidiTimeSpan)67),
                        new TimeAndLength((MidiTimeSpan)334, (MidiTimeSpan)6)
                    }
                },
                TempoMap.Default);
        }

        [Test]
        [Description("Split objects by grid of multiple MIDI steps starting away from zero.")]
        public void SplitByGrid_MultipleSteps_AwayFromZero_Midi()
        {
            //        200
            // │####║#######║##│
            // .    .           240
            // .    .  │####║####║#######║#│
            // .    .  |    .  | .       . |
            // .    . 100   . 200.       .340
            // .    .  |    .  | .       . |
            // .    |=======|====|=======|====|
            // .    |  100  | 67 |  100  | 67 |
            // |    |       |    |       |    |
            // 0   50      150  217     317  384

            var obj1 = ObjectMethods.Create(0, 200);
            var obj2 = ObjectMethods.Create(100, 240);

            SplitByGrid_MultipleSteps(
                inputObjects: new[] { obj1, obj2 },
                gridStart: (MidiTimeSpan)50,
                gridSteps: new[] { (MidiTimeSpan)100, (MidiTimeSpan)67 },
                expectedParts: new Dictionary<TObject, IEnumerable<TimeAndLength>>
                {
                    [obj1] = new[]
                    {
                        new TimeAndLength((MidiTimeSpan)0, (MidiTimeSpan)50),
                        new TimeAndLength((MidiTimeSpan)50, (MidiTimeSpan)100),
                        new TimeAndLength((MidiTimeSpan)150, (MidiTimeSpan)50)
                    },
                    [obj2] = new[]
                    {
                        new TimeAndLength((MidiTimeSpan)100, (MidiTimeSpan)50),
                        new TimeAndLength((MidiTimeSpan)150, (MidiTimeSpan)67),
                        new TimeAndLength((MidiTimeSpan)217, (MidiTimeSpan)100),
                        new TimeAndLength((MidiTimeSpan)317, (MidiTimeSpan)23)
                    },
                },
                tempoMap: TempoMap.Default);
        }

        [Test]
        [Description("Split objects by grid of multiple metric steps starting from zero.")]
        public void SplitByGrid_MultipleSteps_FromZero_Metric()
        {
            //              0:0:1.500
            // │#################║###########║###│
            // .                 .                     0:0:2.200
            // .                 .  │########║#################║###########║########│
            // .                 .  |        .   |             .           .        |
            // .                0:0:1.200    0:0:1.500         .           .    0:0:3.400
            // .                 .  |        .   |             .           .        |
            // |=================|===========|=================|===========|=================|
            // |      0:0:1      | 0:0:0.350 |      0:0:1      | 0:0:0.350 |      0:0:1      |
            // |                 |           |                 |           |                 |
            // 0               0:0:1     0:0:1.350         0:0:2.350   0:0:2.700         0:0:3.700

            var tempoMap = TempoMap.Default;

            var obj1 = ObjectMethods.Create(new MetricTimeSpan(), new MetricTimeSpan(0, 0, 1, 500), tempoMap);
            var obj2 = ObjectMethods.Create(new MetricTimeSpan(0, 0, 1, 200), new MetricTimeSpan(0, 0, 2, 200), tempoMap);

            SplitByGrid_MultipleSteps(
                inputObjects: new[] { obj1, obj2 },
                gridStart: new MetricTimeSpan(),
                gridSteps: new[]
                {
                    new MetricTimeSpan(0, 0, 1),
                    new MetricTimeSpan(0, 0, 0, 350)
                },
                expectedParts: new Dictionary<TObject, IEnumerable<TimeAndLength>>
                {
                    [obj1] = new[]
                    {
                        new TimeAndLength(new MetricTimeSpan(), new MetricTimeSpan(0, 0, 1)),
                        new TimeAndLength(new MetricTimeSpan(0, 0, 1), new MetricTimeSpan(0, 0, 0, 350)),
                        new TimeAndLength(new MetricTimeSpan(0, 0, 1, 350), new MetricTimeSpan(0, 0, 0, 150))
                    },
                    [obj2] = new[]
                    {
                        new TimeAndLength(new MetricTimeSpan(0, 0, 1, 200), new MetricTimeSpan(0, 0, 0, 150)),
                        new TimeAndLength(new MetricTimeSpan(0, 0, 1, 350), new MetricTimeSpan(0, 0, 1)),
                        new TimeAndLength(new MetricTimeSpan(0, 0, 2, 350), new MetricTimeSpan(0, 0, 0, 350)),
                        new TimeAndLength(new MetricTimeSpan(0, 0, 2, 700), new MetricTimeSpan(0, 0, 0, 700))
                    }
                },
                tempoMap: tempoMap);
        }

        [Test]
        [Description("Split objects by grid of multiple metric steps starting from zero.")]
        public void SplitByGrid_MultipleSteps_AwayFromZero_Metric()
        {
            //              0:0:1.500
            // │######║#################║########│
            // .      .                                0:0:2.250
            // .      .             │###║###########║#################║###########║#│
            // .      .             |   .        |  .                 .           . |
            // .      .         0:0:1.150    0:0:1.500                .         0:0:3.400
            // .      .             |   .        |  .                 .           . |
            // .      |=================|===========|=================|===========|=================|
            // .      |      0:0:1      | 0:0:0.350 |      0:0:1      | 0:0:0.350 |      0:0:1      |
            // |      |                 |           |                 |           |                 |
            // 0  0:0:0.300         0:0:1.300   0:0:1.650         0:0:2.650     0:0:3             0:0:4

            var tempoMap = TempoMap.Default;

            var obj1 = ObjectMethods.Create(new MetricTimeSpan(),
                                      new MetricTimeSpan(0, 0, 1, 500),
                                      tempoMap);
            var obj2 = ObjectMethods.Create(new MetricTimeSpan(0, 0, 1, 150),
                                      new MetricTimeSpan(0, 0, 2, 250),
                                      tempoMap);

            SplitByGrid_MultipleSteps(
                inputObjects: new[] { obj1, obj2 },
                gridStart: new MetricTimeSpan(0, 0, 0, 300),
                gridSteps: new[]
                {
                    new MetricTimeSpan(0, 0, 1),
                    new MetricTimeSpan(0, 0, 0, 350)
                },
                expectedParts: new Dictionary<TObject, IEnumerable<TimeAndLength>>
                {
                    [obj1] = new[]
                    {
                        new TimeAndLength(new MetricTimeSpan(), new MetricTimeSpan(0, 0, 0, 300)),
                        new TimeAndLength(new MetricTimeSpan(0, 0, 0, 300), new MetricTimeSpan(0, 0, 1)),
                        new TimeAndLength(new MetricTimeSpan(0, 0, 1, 300), new MetricTimeSpan(0, 0, 0, 200))
                    },
                    [obj2] = new[]
                    {
                        new TimeAndLength(new MetricTimeSpan(0, 0, 1, 150), new MetricTimeSpan(0, 0, 0, 150)),
                        new TimeAndLength(new MetricTimeSpan(0, 0, 1, 300), new MetricTimeSpan(0, 0, 0, 350)),
                        new TimeAndLength(new MetricTimeSpan(0, 0, 1, 650), new MetricTimeSpan(0, 0, 1)),
                        new TimeAndLength(new MetricTimeSpan(0, 0, 2, 650), new MetricTimeSpan(0, 0, 0, 350)),
                        new TimeAndLength(new MetricTimeSpan(0, 0, 3), new MetricTimeSpan(0, 0, 0, 400))
                    }
                },
                tempoMap: tempoMap);
        }

        [Test]
        [Description("Split objects by grid of multiple musical steps starting from zero.")]
        public void SplitByGrid_MultipleSteps_FromZero_Musical()
        {
            var tempoMap = TempoMap.Default;

            var obj1 = ObjectMethods.Create(new MusicalTimeSpan(), MusicalTimeSpan.Whole, tempoMap);
            var obj2 = ObjectMethods.Create(new MusicalTimeSpan(5, 8), 10 * MusicalTimeSpan.Eighth, tempoMap);

            SplitByGrid_MultipleSteps(
                inputObjects: new[] { obj1, obj2 },
                gridStart: new MusicalTimeSpan(),
                gridSteps: new[]
                {
                    MusicalTimeSpan.Eighth,
                    MusicalTimeSpan.Whole
                },
                expectedParts: new Dictionary<TObject, IEnumerable<TimeAndLength>>
                {
                    [obj1] = new[]
                    {
                        new TimeAndLength(new MusicalTimeSpan(), MusicalTimeSpan.Eighth),
                        new TimeAndLength(MusicalTimeSpan.Eighth, 7 * MusicalTimeSpan.Eighth)
                    },
                    [obj2] = new[]
                    {
                        new TimeAndLength(new MusicalTimeSpan(5, 8), MusicalTimeSpan.Half),
                        new TimeAndLength(9 * MusicalTimeSpan.Eighth, MusicalTimeSpan.Eighth),
                        new TimeAndLength(10 * MusicalTimeSpan.Eighth, new MusicalTimeSpan(5, 8))
                    }
                },
                tempoMap: tempoMap);
        }

        [Test]
        [Description("Split objects by grid of multiple musical steps starting away from zero.")]
        public void SplitByGrid_MultipleSteps_AwayFromZero_Musical()
        {
            var tempoMap = TempoMap.Default;

            var obj1 = ObjectMethods.Create(new MusicalTimeSpan(), MusicalTimeSpan.Whole, tempoMap);
            var obj2 = ObjectMethods.Create(new MusicalTimeSpan(5, 8), 10 * MusicalTimeSpan.Eighth, tempoMap);

            SplitByGrid_MultipleSteps(
                inputObjects: new[] { obj1, obj2 },
                gridStart: MusicalTimeSpan.Half,
                gridSteps: new[]
                {
                    MusicalTimeSpan.Eighth,
                    MusicalTimeSpan.Whole
                },
                expectedParts: new Dictionary<TObject, IEnumerable<TimeAndLength>>
                {
                    [obj1] = new[]
                    {
                        new TimeAndLength(new MusicalTimeSpan(), MusicalTimeSpan.Half),
                        new TimeAndLength(MusicalTimeSpan.Half, MusicalTimeSpan.Eighth),
                        new TimeAndLength(MusicalTimeSpan.Half + MusicalTimeSpan.Eighth, 3 * MusicalTimeSpan.Eighth)
                    },
                    [obj2] = new[]
                    {
                        new TimeAndLength(new MusicalTimeSpan(5, 8), MusicalTimeSpan.Whole),
                        new TimeAndLength(13 * MusicalTimeSpan.Eighth, MusicalTimeSpan.Eighth),
                        new TimeAndLength(14 * MusicalTimeSpan.Eighth, MusicalTimeSpan.Eighth)
                    }
                },
                tempoMap: tempoMap);
        }

        [Test]
        [Description("Split objects by grid of multiple steps of different types starting from zero.")]
        public void SplitByGrid_MultipleSteps_FromZero_Mixed()
        {
            var tempoMap = TempoMap.Default;

            var obj1 = ObjectMethods.Create(new MidiTimeSpan(), MusicalTimeSpan.Whole, tempoMap);
            var obj2 = ObjectMethods.Create(new MetricTimeSpan(0, 0, 0, 210), new BarBeatTicksTimeSpan(1, 1), tempoMap);

            var step1 = MusicalTimeSpan.ThirtySecond;
            var step2 = new MetricTimeSpan(0, 0, 1);

            SplitByGrid_MultipleSteps(
                inputObjects: new[] { obj1, obj2 },
                gridStart: (MidiTimeSpan)0,
                gridSteps: new ITimeSpan[] { step1, step2 },
                expectedParts: new Dictionary<TObject, IEnumerable<TimeAndLength>>
                {
                    [obj1] = new[]
                    {
                        new TimeAndLength(new MusicalTimeSpan(), step1),
                        new TimeAndLength(step1, step2),
                        new TimeAndLength(step1.Add(step2, TimeSpanMode.TimeLength), step1),
                        new TimeAndLength((2 * step1).Add(step2, TimeSpanMode.TimeLength),
                                          (MusicalTimeSpan.Whole - 2 * step1).Subtract(step2, TimeSpanMode.LengthLength))
                    },
                    [obj2] = new[]
                    {
                        new TimeAndLength(new MetricTimeSpan(0, 0, 0, 210),
                                          step1.Add(step2, TimeSpanMode.TimeLength).Subtract(new MetricTimeSpan(0, 0, 0, 210), TimeSpanMode.LengthLength)),
                        new TimeAndLength(step1.Add(step2, TimeSpanMode.TimeLength), step1),
                        new TimeAndLength((2 * step1).Add(step2, TimeSpanMode.TimeLength), step2),
                        new TimeAndLength((2 * step1).Add(step2 + step2, TimeSpanMode.TimeLength), step1),
                        new TimeAndLength((3 * step1).Add(step2 + step2, TimeSpanMode.TimeLength),
                                          new MetricTimeSpan(0, 0, 0, 210).Add(new BarBeatTicksTimeSpan(1, 1), TimeSpanMode.TimeLength).Subtract((3 * step1).Add(step2 + step2, TimeSpanMode.TimeLength), TimeSpanMode.TimeTime)),
                    }
                },
                tempoMap: tempoMap);
        }

        #endregion

        #region SplitAtDistance

        [TestCase(LengthedObjectTarget.Start)]
        [TestCase(LengthedObjectTarget.End)]
        public void SplitAtDistance_EmptyCollection(LengthedObjectTarget from)
        {
            var tempoMap = TempoMap.Default;

            var inputObjects = Enumerable.Empty<TObject>();
            var distance = (MidiTimeSpan)100;
            var expectedObjects = Enumerable.Empty<TObject>();
            var actualObjects = Splitter.SplitAtDistance(inputObjects, distance, from, tempoMap);

            ObjectMethods.AssertCollectionsAreEqual(expectedObjects, actualObjects);
        }

        [TestCase(LengthedObjectTarget.Start)]
        [TestCase(LengthedObjectTarget.End)]
        public void SplitAtDistance_Nulls(LengthedObjectTarget from)
        {
            var tempoMap = TempoMap.Default;

            var inputObjects = new[] { default(TObject), default(TObject) };
            var distance = (MidiTimeSpan)100;
            var expectedObjects = new[] { default(TObject), default(TObject) };
            var actualObjects = Splitter.SplitAtDistance(inputObjects, distance, from, tempoMap);

            ObjectMethods.AssertCollectionsAreEqual(expectedObjects, actualObjects);
        }

        [TestCase(LengthedObjectTarget.Start)]
        [TestCase(LengthedObjectTarget.End)]
        public void SplitAtDistance_ZeroDistance(LengthedObjectTarget from)
        {
            var tempoMap = TempoMap.Default;

            var inputObjects = CreateInputObjects(1000);
            var distance = (MidiTimeSpan)0;
            var expectedObjects = inputObjects.Select(o => ObjectMethods.Clone(o));
            var actualObjects = Splitter.SplitAtDistance(inputObjects, distance, from, tempoMap);

            ObjectMethods.AssertCollectionsAreEqual(expectedObjects, actualObjects);
        }

        [TestCase(LengthedObjectTarget.Start)]
        [TestCase(LengthedObjectTarget.End)]
        public void SplitAtDistance_BigDistance(LengthedObjectTarget from)
        {
            var tempoMap = TempoMap.Default;

            var inputObjects = CreateInputObjects(1000);
            var distance = (MidiTimeSpan)1000;
            var expectedObjects = inputObjects.Select(o => ObjectMethods.Clone(o));
            var actualObjects = Splitter.SplitAtDistance(inputObjects, distance, from, tempoMap);

            ObjectMethods.AssertCollectionsAreEqual(expectedObjects, actualObjects);
        }

        [Test]
        public void SplitAtDistance_Start()
        {
            var tempoMap = TempoMap.Default;

            var inputObjects = CreateInputObjects(1000);
            var distance = (MidiTimeSpan)10;
            var expectedObjects = inputObjects.SelectMany(o => Split(o, new[] { o.Time + distance }));
            var actualObjects = Splitter.SplitAtDistance(inputObjects, distance, LengthedObjectTarget.Start, tempoMap);

            ObjectMethods.AssertCollectionsAreEqual(expectedObjects, actualObjects);
        }

        [Test]
        public void SplitAtDistance_End()
        {
            var tempoMap = TempoMap.Default;

            var inputObjects = CreateInputObjects(1000);
            var distance = (MidiTimeSpan)10;
            var expectedObjects = inputObjects.SelectMany(o => Split(o, new[] { o.Time + o.Length - distance }));
            var actualObjects = Splitter.SplitAtDistance(inputObjects, distance, LengthedObjectTarget.End, tempoMap);

            ObjectMethods.AssertCollectionsAreEqual(expectedObjects, actualObjects);
        }

        [TestCase(LengthedObjectTarget.Start)]
        [TestCase(LengthedObjectTarget.End)]
        public void SplitAtDistance_ByRatio_EmptyCollection(LengthedObjectTarget from)
        {
            var tempoMap = TempoMap.Default;

            var inputObjects = Enumerable.Empty<TObject>();
            var ratio = 0.5;
            var expectedObjects = Enumerable.Empty<TObject>();
            var actualObjects = Splitter.SplitAtDistance(inputObjects, ratio, TimeSpanType.Midi, from, tempoMap);

            ObjectMethods.AssertCollectionsAreEqual(expectedObjects, actualObjects);
        }

        [TestCase(LengthedObjectTarget.Start)]
        [TestCase(LengthedObjectTarget.End)]
        public void SplitAtDistance_ByRatio_Nulls(LengthedObjectTarget from)
        {
            var tempoMap = TempoMap.Default;

            var inputObjects = new[] { default(TObject), default(TObject) };
            var ratio = 0.5;
            var expectedObjects = new[] { default(TObject), default(TObject) };
            var actualObjects = Splitter.SplitAtDistance(inputObjects, ratio, TimeSpanType.Midi, from, tempoMap);

            ObjectMethods.AssertCollectionsAreEqual(expectedObjects, actualObjects);
        }

        [TestCase(LengthedObjectTarget.Start)]
        [TestCase(LengthedObjectTarget.End)]
        public void SplitAtDistance_ByRatio_ZeroRatio(LengthedObjectTarget from)
        {
            var tempoMap = TempoMap.Default;

            var inputObjects = CreateInputObjects(1000);
            var ratio = 0.0;
            var expectedObjects = inputObjects.Select(o => ObjectMethods.Clone(o));
            var actualObjects = Splitter.SplitAtDistance(inputObjects, ratio, TimeSpanType.Midi, from, tempoMap);

            ObjectMethods.AssertCollectionsAreEqual(expectedObjects, actualObjects);
        }

        [TestCase(LengthedObjectTarget.Start)]
        [TestCase(LengthedObjectTarget.End)]
        public void SplitAtDistance_FullLengthRatio(LengthedObjectTarget from)
        {
            var tempoMap = TempoMap.Default;

            var inputObjects = CreateInputObjects(1000);
            var ratio = 1.0;
            var expectedObjects = inputObjects.Select(o => ObjectMethods.Clone(o));
            var actualObjects = Splitter.SplitAtDistance(inputObjects, ratio, TimeSpanType.Midi, from, tempoMap);

            ObjectMethods.AssertCollectionsAreEqual(expectedObjects, actualObjects);
        }

        [Test]
        public void SplitAtDistance_ByRatio_Start()
        {
            var tempoMap = TempoMap.Default;

            var inputObjects = CreateInputObjects(1000);
            var ratio = 0.1;
            var expectedObjects = inputObjects.SelectMany(o => Split(o, new[] { o.Time + 100 }));
            var actualObjects = Splitter.SplitAtDistance(inputObjects, ratio, TimeSpanType.Midi, LengthedObjectTarget.Start, tempoMap);

            ObjectMethods.AssertCollectionsAreEqual(expectedObjects, actualObjects);
        }

        [Test]
        public void SplitAtDistance_ByRatio_End()
        {
            var tempoMap = TempoMap.Default;

            var inputObjects = CreateInputObjects(1000);
            var ratio = 0.1;
            var expectedObjects = inputObjects.SelectMany(o => Split(o, new[] { o.Time + o.Length - 100 }));
            var actualObjects = Splitter.SplitAtDistance(inputObjects, ratio, TimeSpanType.Midi, LengthedObjectTarget.End, tempoMap);

            ObjectMethods.AssertCollectionsAreEqual(expectedObjects, actualObjects);
        }

        #endregion

        #endregion

        #region Protected methods

        protected abstract IEnumerable<TObject> CreateInputObjects(long length);

        protected abstract SplittedLengthedObject<TObject> SplitObject(TObject obj, long time);

        #endregion

        #region Private methods

        private void SplitByGrid_MultipleSteps(IEnumerable<TObject> inputObjects,
                                               ITimeSpan gridStart,
                                               IEnumerable<ITimeSpan> gridSteps,
                                               Dictionary<TObject, IEnumerable<TimeAndLength>> expectedParts,
                                               TempoMap tempoMap)
        {
            var expectedObjects = expectedParts
                .SelectMany(p => p.Value.Select(tl => CloneAndChangeTimeAndLength(
                    p.Key,
                    TimeConverter.ConvertFrom(tl.Time, tempoMap),
                    LengthConverter.ConvertFrom(tl.Length, tl.Time, tempoMap))))
                .ToArray();

            var actualObjects = Splitter.SplitByGrid(inputObjects, new SteppedGrid(gridStart, gridSteps), tempoMap).ToArray();

            ObjectMethods.AssertCollectionsAreEqual(expectedObjects, actualObjects);
        }

        private void SplitByGrid_OneStep_SinglePart(ITimeSpan gridStart, ITimeSpan step, TempoMap tempoMap)
        {
            var gridSteps = new[] { step };

            var firstTime = TimeConverter.ConvertFrom(gridStart, tempoMap);

            long secondTime = firstTime;
            for (int i = 0; i < 5; i++)
            {
                secondTime += LengthConverter.ConvertFrom(step, secondTime, tempoMap);
            }

            var inputObjects = new[]
            {
                ObjectMethods.Create(firstTime, LengthConverter.ConvertFrom(step, firstTime, tempoMap)),
                ObjectMethods.Create(secondTime, LengthConverter.ConvertFrom(step, secondTime, tempoMap))
            };

            var data = SplitByGrid_ClonesExpected(inputObjects, gridStart, gridSteps, tempoMap);

            var stepType = TimeSpanTypes[step.GetType()];
            var partLength = data.ActualObjects.First().LengthAs(stepType, tempoMap);
            Assert.IsTrue(data.ActualObjects.All(o => o.LengthAs(stepType, tempoMap).Equals(partLength)),
                          $"Objects have different length measured as {stepType}.");
        }

        private SplitData SplitByGrid_ClonesExpected(IEnumerable<TObject> inputObjects, ITimeSpan gridStart, IEnumerable<ITimeSpan> gridSteps, TempoMap tempoMap)
        {
            var expectedObjects = inputObjects.Select(o => o == null ? default(TObject) : ObjectMethods.Clone(o)).ToArray();
            var actualObjects = Splitter.SplitByGrid(inputObjects, new SteppedGrid(gridStart, gridSteps), tempoMap).ToArray();

            ObjectMethods.AssertCollectionsAreEqual(expectedObjects, actualObjects);

            return new SplitData(inputObjects, expectedObjects, actualObjects);
        }

        private void SplitByPartsNumber_EqualDivision(long objectsLength, int partsNumber, TimeSpanType lengthType, TempoMap tempoMap)
        {
            var data = SplitByPartsNumber(objectsLength, partsNumber, lengthType, tempoMap);

            var partLength = data.ActualObjects.First().Length;
            Assert.IsTrue(data.ActualObjects.All(o => o.Length == partLength), "Objects don't have the same length.");
        }

        private void SplitByPartsNumber_UnequalDivision(long objectsLength, int partsNumber, TimeSpanType lengthType, TempoMap tempoMap)
        {
            var data = SplitByPartsNumber(objectsLength, partsNumber, lengthType, tempoMap);

            var partLength = data.ActualObjects.First().Length;
            Assert.IsTrue(!data.ActualObjects.All(o => o.Length == partLength), "Objects have the same length.");
        }

        private SplitData SplitByPartsNumber(long objectsLength, int partsNumber, TimeSpanType lengthType, TempoMap tempoMap)
        {
            var inputObjects = CreateInputObjects(objectsLength).ToArray();
            var expectedObjects = GetExpectedObjectsByPartsNumber(inputObjects, partsNumber).ToArray();
            var actualObjects = Splitter.SplitByPartsNumber(inputObjects, partsNumber, lengthType, tempoMap).ToArray();

            Assert.AreEqual(inputObjects.Length * partsNumber,
                            actualObjects.Length,
                            "Parts count is invalid.");
            ObjectMethods.AssertCollectionsAreEqual(expectedObjects, actualObjects);

            return new SplitData(inputObjects, expectedObjects, actualObjects);
        }

        private IEnumerable<TObject> GetExpectedObjectsByPartsNumber(IEnumerable<TObject> inputObjects, int partsNumber)
        {
            foreach (var obj in inputObjects)
            {
                var length = obj.Length;
                var time = obj.Time;

                var times = new List<long>();

                for (var partsRemaining = partsNumber; partsRemaining > 1; partsRemaining--)
                {
                    var partLength = (long)Math.Round(length / (double)partsRemaining, MidpointRounding.AwayFromZero);
                    time += partLength;
                    times.Add(time);
                    length -= partLength;
                }

                foreach (var result in Split(obj, times))
                {
                    yield return result;
                }
            }
        }

        private IEnumerable<TObject> Split(TObject obj, IEnumerable<long> times)
        {
            var tail = ObjectMethods.Clone(obj);

            foreach (var time in times.OrderBy(t => t))
            {
                var parts = SplitObject(tail, time);
                yield return parts.LeftPart;

                tail = parts.RightPart;
            }

            yield return tail;
        }

        private TObject CloneAndChangeTimeAndLength(TObject obj, long time, long length)
        {
            var result = ObjectMethods.Clone(obj);
            ObjectMethods.SetTime(result, time);
            ObjectMethods.SetLength(result, length);
            return result;
        }

        #endregion
    }
}
