using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed partial class SplitterTests
    {
        #region Test methods

        [Test]
        public void SplitObjectsByGrid_Notes_EmptyCollection()
        {
            SplitByGrid_ClonesExpected(inputObjects: Enumerable.Empty<Note>(),
                                       gridStart: (MidiTimeSpan)0,
                                       gridSteps: new[] { MusicalTimeSpan.Whole },
                                       tempoMap: TempoMap.Default);
        }

        [Test]
        public void SplitObjectsByGrid_Notes_Nulls()
        {
            SplitByGrid_ClonesExpected(inputObjects: new[] { default(Note), default(Note) },
                                       gridStart: (MidiTimeSpan)0,
                                       gridSteps: new[] { MusicalTimeSpan.Whole },
                                       tempoMap: TempoMap.Default);
        }

        [Test]
        public void SplitObjectsByGrid_Notes_ZeroLength()
        {
            SplitByGrid_ClonesExpected(inputObjects: CreateInputNotes(0),
                                       gridStart: (MidiTimeSpan)0,
                                       gridSteps: new[] { MusicalTimeSpan.Whole },
                                       tempoMap: TempoMap.Default);
        }

        [Test]
        public void SplitObjectsByGrid_Notes_NoSteps()
        {
            SplitByGrid_ClonesExpected(inputObjects: CreateInputNotes(100),
                                       gridStart: (MidiTimeSpan)0,
                                       gridSteps: Enumerable.Empty<ITimeSpan>(),
                                       tempoMap: TempoMap.Default);
        }

        [Test]
        public void SplitObjectsByGrid_Notes_OneStep_SinglePart_FromZero_Midi()
        {
            SplitByGrid_OneStep_SinglePart(gridStart: (MidiTimeSpan)0,
                                           step: (MidiTimeSpan)100,
                                           tempoMap: TempoMap.Default,
                                           methods: new NoteMethods());
        }

        [Test]
        public void SplitObjectsByGrid_Notes_OneStep_SinglePart_AwayFromZero_Midi()
        {
            SplitByGrid_OneStep_SinglePart(gridStart: (MidiTimeSpan)100,
                                           step: (MidiTimeSpan)123,
                                           tempoMap: TempoMap.Default,
                                           methods: new NoteMethods());
        }

        [Test]
        public void SplitObjectsByGrid_Notes_OneStep_SinglePart_FromZero_Metric()
        {
            SplitByGrid_OneStep_SinglePart(gridStart: new MetricTimeSpan(),
                                           step: new MetricTimeSpan(0, 0, 2),
                                           tempoMap: TempoMap.Default,
                                           methods: new NoteMethods());
        }

        [Test]
        public void SplitObjectsByGrid_Notes_OneStep_SinglePart_AwayFromZero_Metric()
        {
            SplitByGrid_OneStep_SinglePart(gridStart: new MetricTimeSpan(0, 0, 1),
                                           step: new MetricTimeSpan(0, 0, 0, 123),
                                           tempoMap: TempoMap.Default,
                                           methods: new NoteMethods());
        }

        [Test]
        public void SplitObjectsByGrid_Notes_OneStep_SinglePart_FromZero_Musical()
        {
            SplitByGrid_OneStep_SinglePart(gridStart: new MusicalTimeSpan(),
                                           step: MusicalTimeSpan.Whole,
                                           tempoMap: TempoMap.Default,
                                           methods: new NoteMethods());
        }

        [Test]
        public void SplitObjectsByGrid_Notes_OneStep_SinglePart_AwayFromZero_Musical()
        {
            SplitByGrid_OneStep_SinglePart(gridStart: new MusicalTimeSpan(5, 17),
                                           step: new MusicalTimeSpan(3, 67),
                                           tempoMap: TempoMap.Default,
                                           methods: new NoteMethods());
        }

        [Test]
        public void SplitObjectsByGrid_Notes_MultipleSteps_FromZero_Midi()
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

            var methods = new NoteMethods();
            var obj1 = methods.Create(0, 200);
            var obj2 = methods.Create(100, 240);

            SplitByGrid_MultipleSteps(
                new[] { obj1, obj2 },
                (MidiTimeSpan)0,
                new[] { (MidiTimeSpan)100, (MidiTimeSpan)67 },
                new Dictionary<Note, IEnumerable<TimeAndLength>>
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
        public void SplitObjectsByGrid_Notes_MultipleSteps_AwayFromZero_Midi()
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

            var methods = new NoteMethods();
            var obj1 = methods.Create(0, 200);
            var obj2 = methods.Create(100, 240);

            SplitByGrid_MultipleSteps(
                inputObjects: new[] { obj1, obj2 },
                gridStart: (MidiTimeSpan)50,
                gridSteps: new[] { (MidiTimeSpan)100, (MidiTimeSpan)67 },
                expectedParts: new Dictionary<Note, IEnumerable<TimeAndLength>>
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
        public void SplitObjectsByGrid_Notes_MultipleSteps_FromZero_Metric()
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

            var methods = new NoteMethods();
            var obj1 = methods.Create(new MetricTimeSpan(), new MetricTimeSpan(0, 0, 1, 500), tempoMap);
            var obj2 = methods.Create(new MetricTimeSpan(0, 0, 1, 200), new MetricTimeSpan(0, 0, 2, 200), tempoMap);

            SplitByGrid_MultipleSteps(
                inputObjects: new[] { obj1, obj2 },
                gridStart: new MetricTimeSpan(),
                gridSteps: new[]
                {
                    new MetricTimeSpan(0, 0, 1),
                    new MetricTimeSpan(0, 0, 0, 350)
                },
                expectedParts: new Dictionary<Note, IEnumerable<TimeAndLength>>
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
        public void SplitObjectsByGrid_Notes_MultipleSteps_AwayFromZero_Metric()
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

            var methods = new NoteMethods();
            var obj1 = methods.Create(new MetricTimeSpan(),
                                      new MetricTimeSpan(0, 0, 1, 500),
                                      tempoMap);
            var obj2 = methods.Create(new MetricTimeSpan(0, 0, 1, 150),
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
                expectedParts: new Dictionary<Note, IEnumerable<TimeAndLength>>
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
        public void SplitObjectsByGrid_Notes_MultipleSteps_FromZero_Musical()
        {
            var tempoMap = TempoMap.Default;

            var methods = new NoteMethods();
            var obj1 = methods.Create(new MusicalTimeSpan(), MusicalTimeSpan.Whole, tempoMap);
            var obj2 = methods.Create(new MusicalTimeSpan(5, 8), 10 * MusicalTimeSpan.Eighth, tempoMap);

            SplitByGrid_MultipleSteps(
                inputObjects: new[] { obj1, obj2 },
                gridStart: new MusicalTimeSpan(),
                gridSteps: new[]
                {
                    MusicalTimeSpan.Eighth,
                    MusicalTimeSpan.Whole
                },
                expectedParts: new Dictionary<Note, IEnumerable<TimeAndLength>>
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
        public void SplitObjectsByGrid_Notes_MultipleSteps_AwayFromZero_Musical()
        {
            var tempoMap = TempoMap.Default;

            var methods = new NoteMethods();
            var obj1 = methods.Create(new MusicalTimeSpan(), MusicalTimeSpan.Whole, tempoMap);
            var obj2 = methods.Create(new MusicalTimeSpan(5, 8), 10 * MusicalTimeSpan.Eighth, tempoMap);

            SplitByGrid_MultipleSteps(
                inputObjects: new[] { obj1, obj2 },
                gridStart: MusicalTimeSpan.Half,
                gridSteps: new[]
                {
                    MusicalTimeSpan.Eighth,
                    MusicalTimeSpan.Whole
                },
                expectedParts: new Dictionary<Note, IEnumerable<TimeAndLength>>
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
        public void SplitObjectsByGrid_Notes_MultipleSteps_FromZero_Mixed()
        {
            var tempoMap = TempoMap.Default;

            var methods = new NoteMethods();
            var obj1 = methods.Create(new MidiTimeSpan(), MusicalTimeSpan.Whole, tempoMap);
            var obj2 = methods.Create(new MetricTimeSpan(0, 0, 0, 210), new BarBeatTicksTimeSpan(1, 1), tempoMap);

            var step1 = MusicalTimeSpan.ThirtySecond;
            var step2 = new MetricTimeSpan(0, 0, 1);

            SplitByGrid_MultipleSteps(
                inputObjects: new[] { obj1, obj2 },
                gridStart: (MidiTimeSpan)0,
                gridSteps: new ITimeSpan[] { step1, step2 },
                expectedParts: new Dictionary<Note, IEnumerable<TimeAndLength>>
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

        [Test]
        public void SplitObjectsByGrid_Chords_EmptyCollection()
        {
            SplitByGrid_ClonesExpected(inputObjects: Enumerable.Empty<Chord>(),
                                       gridStart: (MidiTimeSpan)0,
                                       gridSteps: new[] { MusicalTimeSpan.Whole },
                                       tempoMap: TempoMap.Default);
        }

        [Test]
        public void SplitObjectsByGrid_Chords_Nulls()
        {
            SplitByGrid_ClonesExpected(inputObjects: new[] { default(Chord), default(Chord) },
                                       gridStart: (MidiTimeSpan)0,
                                       gridSteps: new[] { MusicalTimeSpan.Whole },
                                       tempoMap: TempoMap.Default);
        }

        [Test]
        public void SplitObjectsByGrid_Chords_ZeroLength()
        {
            SplitByGrid_ClonesExpected(inputObjects: CreateInputChords(0),
                                       gridStart: (MidiTimeSpan)0,
                                       gridSteps: new[] { MusicalTimeSpan.Whole },
                                       tempoMap: TempoMap.Default);
        }

        [Test]
        public void SplitObjectsByGrid_Chords_NoSteps()
        {
            SplitByGrid_ClonesExpected(inputObjects: CreateInputChords(100),
                                       gridStart: (MidiTimeSpan)0,
                                       gridSteps: Enumerable.Empty<ITimeSpan>(),
                                       tempoMap: TempoMap.Default);
        }

        [Test]
        public void SplitObjectsByGrid_Chords_OneStep_SinglePart_FromZero_Midi()
        {
            SplitByGrid_OneStep_SinglePart(gridStart: (MidiTimeSpan)0,
                                           step: (MidiTimeSpan)100,
                                           tempoMap: TempoMap.Default,
                                           methods: new ChordMethods());
        }

        [Test]
        public void SplitObjectsByGrid_Chords_OneStep_SinglePart_AwayFromZero_Midi()
        {
            SplitByGrid_OneStep_SinglePart(gridStart: (MidiTimeSpan)100,
                                           step: (MidiTimeSpan)123,
                                           tempoMap: TempoMap.Default,
                                           methods: new ChordMethods());
        }

        [Test]
        public void SplitObjectsByGrid_Chords_OneStep_SinglePart_FromZero_Metric()
        {
            SplitByGrid_OneStep_SinglePart(gridStart: new MetricTimeSpan(),
                                           step: new MetricTimeSpan(0, 0, 2),
                                           tempoMap: TempoMap.Default,
                                           methods: new ChordMethods());
        }

        [Test]
        public void SplitObjectsByGrid_Chords_OneStep_SinglePart_AwayFromZero_Metric()
        {
            SplitByGrid_OneStep_SinglePart(gridStart: new MetricTimeSpan(0, 0, 1),
                                           step: new MetricTimeSpan(0, 0, 0, 123),
                                           tempoMap: TempoMap.Default,
                                           methods: new ChordMethods());
        }

        [Test]
        public void SplitObjectsByGrid_Chords_OneStep_SinglePart_FromZero_Musical()
        {
            SplitByGrid_OneStep_SinglePart(gridStart: new MusicalTimeSpan(),
                                           step: MusicalTimeSpan.Whole,
                                           tempoMap: TempoMap.Default,
                                           methods: new ChordMethods());
        }

        [Test]
        public void SplitObjectsByGrid_Chords_OneStep_SinglePart_AwayFromZero_Musical()
        {
            SplitByGrid_OneStep_SinglePart(gridStart: new MusicalTimeSpan(5, 17),
                                           step: new MusicalTimeSpan(3, 67),
                                           tempoMap: TempoMap.Default,
                                           methods: new ChordMethods());
        }

        [Test]
        public void SplitObjectsByGrid_Chords_MultipleSteps_FromZero_Midi()
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

            var methods = new ChordMethods();
            var obj1 = methods.Create(0, 200);
            var obj2 = methods.Create(100, 240);

            SplitByGrid_MultipleSteps(
                new[] { obj1, obj2 },
                (MidiTimeSpan)0,
                new[] { (MidiTimeSpan)100, (MidiTimeSpan)67 },
                new Dictionary<Chord, IEnumerable<TimeAndLength>>
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
        public void SplitObjectsByGrid_Chords_MultipleSteps_AwayFromZero_Midi()
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

            var methods = new ChordMethods();
            var obj1 = methods.Create(0, 200);
            var obj2 = methods.Create(100, 240);

            SplitByGrid_MultipleSteps(
                inputObjects: new[] { obj1, obj2 },
                gridStart: (MidiTimeSpan)50,
                gridSteps: new[] { (MidiTimeSpan)100, (MidiTimeSpan)67 },
                expectedParts: new Dictionary<Chord, IEnumerable<TimeAndLength>>
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
        public void SplitObjectsByGrid_Chords_MultipleSteps_FromZero_Metric()
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

            var methods = new ChordMethods();
            var obj1 = methods.Create(new MetricTimeSpan(), new MetricTimeSpan(0, 0, 1, 500), tempoMap);
            var obj2 = methods.Create(new MetricTimeSpan(0, 0, 1, 200), new MetricTimeSpan(0, 0, 2, 200), tempoMap);

            SplitByGrid_MultipleSteps(
                inputObjects: new[] { obj1, obj2 },
                gridStart: new MetricTimeSpan(),
                gridSteps: new[]
                {
                    new MetricTimeSpan(0, 0, 1),
                    new MetricTimeSpan(0, 0, 0, 350)
                },
                expectedParts: new Dictionary<Chord, IEnumerable<TimeAndLength>>
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
        public void SplitObjectsByGrid_Chords_MultipleSteps_AwayFromZero_Metric()
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

            var methods = new ChordMethods();
            var obj1 = methods.Create(new MetricTimeSpan(),
                                      new MetricTimeSpan(0, 0, 1, 500),
                                      tempoMap);
            var obj2 = methods.Create(new MetricTimeSpan(0, 0, 1, 150),
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
                expectedParts: new Dictionary<Chord, IEnumerable<TimeAndLength>>
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
        public void SplitObjectsByGrid_Chords_MultipleSteps_FromZero_Musical()
        {
            var tempoMap = TempoMap.Default;

            var methods = new ChordMethods();
            var obj1 = methods.Create(new MusicalTimeSpan(), MusicalTimeSpan.Whole, tempoMap);
            var obj2 = methods.Create(new MusicalTimeSpan(5, 8), 10 * MusicalTimeSpan.Eighth, tempoMap);

            SplitByGrid_MultipleSteps(
                inputObjects: new[] { obj1, obj2 },
                gridStart: new MusicalTimeSpan(),
                gridSteps: new[]
                {
                    MusicalTimeSpan.Eighth,
                    MusicalTimeSpan.Whole
                },
                expectedParts: new Dictionary<Chord, IEnumerable<TimeAndLength>>
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
        public void SplitObjectsByGrid_Chords_MultipleSteps_AwayFromZero_Musical()
        {
            var tempoMap = TempoMap.Default;

            var methods = new ChordMethods();
            var obj1 = methods.Create(new MusicalTimeSpan(), MusicalTimeSpan.Whole, tempoMap);
            var obj2 = methods.Create(new MusicalTimeSpan(5, 8), 10 * MusicalTimeSpan.Eighth, tempoMap);

            SplitByGrid_MultipleSteps(
                inputObjects: new[] { obj1, obj2 },
                gridStart: MusicalTimeSpan.Half,
                gridSteps: new[]
                {
                    MusicalTimeSpan.Eighth,
                    MusicalTimeSpan.Whole
                },
                expectedParts: new Dictionary<Chord, IEnumerable<TimeAndLength>>
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
        public void SplitObjectsByGrid_Chords_MultipleSteps_FromZero_Mixed()
        {
            var tempoMap = TempoMap.Default;

            var methods = new ChordMethods();
            var obj1 = methods.Create(new MidiTimeSpan(), MusicalTimeSpan.Whole, tempoMap);
            var obj2 = methods.Create(new MetricTimeSpan(0, 0, 0, 210), new BarBeatTicksTimeSpan(1, 1), tempoMap);

            var step1 = MusicalTimeSpan.ThirtySecond;
            var step2 = new MetricTimeSpan(0, 0, 1);

            SplitByGrid_MultipleSteps(
                inputObjects: new[] { obj1, obj2 },
                gridStart: (MidiTimeSpan)0,
                gridSteps: new ITimeSpan[] { step1, step2 },
                expectedParts: new Dictionary<Chord, IEnumerable<TimeAndLength>>
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
    }
}
