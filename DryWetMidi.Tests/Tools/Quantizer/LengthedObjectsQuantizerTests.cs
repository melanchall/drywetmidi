using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Smf.Interaction;
using Melanchall.DryWetMidi.Tests.Common;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    public abstract class LengthedObjectsQuantizerTests<TObject, TSettings> : LengthedObjectsToolTests<TObject>
        where TObject : ILengthedObject
        where TSettings : LengthedObjectsQuantizingSettings, new()
    {
        #region Constructor

        public LengthedObjectsQuantizerTests(LengthedObjectMethods<TObject> methods, LengthedObjectsQuantizer<TObject, TSettings> quantizer)
            : base(methods)
        {
            Quantizer = quantizer;
        }

        #endregion

        #region Properties

        protected LengthedObjectsQuantizer<TObject, TSettings> Quantizer { get; }

        #endregion

        #region Test methods

        [Test]
        [Description("Quantize start times of empty collection.")]
        public void Quantize_Start_EmptyCollection()
        {
            var tempoMap = TempoMap.Default;

            Quantize_Start_DontFixEnd(
                Enumerable.Empty<TObject>(),
                new SteppedGrid(MusicalTimeSpan.Eighth),
                Enumerable.Empty<ITimeSpan>(),
                tempoMap);
        }

        [Test]
        [Description("Quantize start times of nulls.")]
        public void Quantize_Start_Nulls()
        {
            var tempoMap = TempoMap.Default;

            Quantize_Start_DontFixEnd(
                new[] { default(TObject), default(TObject) },
                new SteppedGrid(MusicalTimeSpan.Eighth),
                new ITimeSpan[] { null, null },
                tempoMap);
        }

        [Test]
        [Description("Quantize start times by grid of one step starting from zero.")]
        public void Quantize_Start_OneStep_FromZero()
        {
            var tempoMap = TempoMap.Default;

            Quantize_Start_DontFixEnd(
                new[]
                {
                    Methods.Create((MidiTimeSpan)0, MusicalTimeSpan.Whole, tempoMap),
                    Methods.Create(MusicalTimeSpan.Quarter + MusicalTimeSpan.Sixteenth, MusicalTimeSpan.Half, tempoMap),
                    Methods.Create(MusicalTimeSpan.Eighth, (MidiTimeSpan)123, tempoMap)
                },
                new SteppedGrid(MusicalTimeSpan.Eighth),
                new ITimeSpan[]
                {
                    (MidiTimeSpan)0,
                    MusicalTimeSpan.Quarter,
                    MusicalTimeSpan.Eighth
                },
                tempoMap);
        }

        [Test]
        [Description("Quantize start times by grid of one step starting away from zero.")]
        public void Quantize_Start_OneStep_AwayFromZero()
        {
            var tempoMap = TempoMap.Default;

            Quantize_Start_DontFixEnd(
                new[]
                {
                    Methods.Create((MidiTimeSpan)0, MusicalTimeSpan.Whole, tempoMap),
                    Methods.Create(MusicalTimeSpan.Whole + MusicalTimeSpan.Sixteenth, MusicalTimeSpan.Half, tempoMap),
                    Methods.Create(MusicalTimeSpan.Whole.SingleDotted(), (MidiTimeSpan)123, tempoMap)
                },
                new SteppedGrid(MusicalTimeSpan.Whole, MusicalTimeSpan.Eighth),
                new[]
                {
                    MusicalTimeSpan.Whole,
                    MusicalTimeSpan.Whole,
                    MusicalTimeSpan.Whole.SingleDotted()
                },
                tempoMap);
        }

        [Test]
        [Description("Quantize start times by grid of one step keeping end times untouched.")]
        public void Quantize_Start_OneStep_FixEnd()
        {
            var tempoMap = TempoMap.Default;

            Quantize_Start_FixEnd(
                new[]
                {
                    Methods.Create((MidiTimeSpan)0, MusicalTimeSpan.Whole, tempoMap),
                    Methods.Create(MusicalTimeSpan.Eighth + MusicalTimeSpan.ThirtySecond, 3 * MusicalTimeSpan.ThirtySecond, tempoMap),
                    Methods.Create(2 * MusicalTimeSpan.Eighth + MusicalTimeSpan.ThirtySecond, MusicalTimeSpan.Whole, tempoMap),
                    Methods.Create(MusicalTimeSpan.Whole.SingleDotted(), MusicalTimeSpan.Whole, tempoMap)
                },
                new SteppedGrid(MusicalTimeSpan.Eighth, MusicalTimeSpan.Eighth),
                new[]
                {
                    new TimeAndLength(MusicalTimeSpan.Eighth, 7 * MusicalTimeSpan.Eighth),
                    new TimeAndLength(MusicalTimeSpan.Eighth, MusicalTimeSpan.Eighth),
                    new TimeAndLength(2 * MusicalTimeSpan.Eighth, MusicalTimeSpan.Whole + MusicalTimeSpan.ThirtySecond),
                    new TimeAndLength(MusicalTimeSpan.Whole.SingleDotted(), MusicalTimeSpan.Whole)
                },
                tempoMap);
        }

        [Test]
        [Description("Quantize start times by grid of multiple steps starting from zero.")]
        public void Quantize_Start_MultipleSteps_FromZero()
        {
            var tempoMap = TempoMap.Default;

            Quantize_Start_DontFixEnd(
                new[]
                {
                    Methods.Create((MidiTimeSpan)0, MusicalTimeSpan.Whole, tempoMap),
                    Methods.Create(MusicalTimeSpan.Eighth + MusicalTimeSpan.ThirtySecond, MusicalTimeSpan.ThirtySecond, tempoMap),
                    Methods.Create(MusicalTimeSpan.Whole + MusicalTimeSpan.Quarter + MusicalTimeSpan.ThirtySecond, MusicalTimeSpan.Half, tempoMap),
                },
                new SteppedGrid(new[] { MusicalTimeSpan.Eighth, MusicalTimeSpan.Quarter }),
                new ITimeSpan[]
                {
                    (MidiTimeSpan)0,
                    MusicalTimeSpan.Eighth,
                    MusicalTimeSpan.Whole + MusicalTimeSpan.Quarter
                },
                tempoMap);
        }

        [Test]
        [Description("Quantize start times by grid of multiple steps starting away from zero.")]
        public void Quantize_Start_MultipleSteps_AwayFromZero()
        {
            var tempoMap = TempoMap.Default;

            Quantize_Start_DontFixEnd(
                new[]
                {
                    Methods.Create((MidiTimeSpan)0, MusicalTimeSpan.Whole, tempoMap),
                    Methods.Create(MusicalTimeSpan.ThirtySecond, MusicalTimeSpan.Whole, tempoMap),
                    Methods.Create(MusicalTimeSpan.Eighth, MusicalTimeSpan.ThirtySecond, tempoMap),
                    Methods.Create(MusicalTimeSpan.Whole + MusicalTimeSpan.Quarter, MusicalTimeSpan.Half, tempoMap),
                },
                new SteppedGrid(MusicalTimeSpan.ThirtySecond, new[] { MusicalTimeSpan.Eighth, MusicalTimeSpan.Quarter }),
                new ITimeSpan[]
                {
                    MusicalTimeSpan.ThirtySecond,
                    MusicalTimeSpan.ThirtySecond,
                    MusicalTimeSpan.Eighth + MusicalTimeSpan.ThirtySecond,
                    MusicalTimeSpan.Whole + MusicalTimeSpan.Quarter + MusicalTimeSpan.ThirtySecond
                },
                tempoMap);
        }

        [Test]
        [Description("Quantize start times by grid of multiple steps keeping end times untouched.")]
        public void Quantize_Start_MultipleSteps_FixEnd()
        {
            var tempoMap = TempoMap.Default;

            Quantize_Start_FixEnd(
                new[]
                {
                    Methods.Create((MidiTimeSpan)0, MusicalTimeSpan.Whole, tempoMap),
                    Methods.Create(MusicalTimeSpan.ThirtySecond, MusicalTimeSpan.Whole, tempoMap),
                    Methods.Create(MusicalTimeSpan.Eighth, MusicalTimeSpan.ThirtySecond, tempoMap),
                    Methods.Create(MusicalTimeSpan.Whole + MusicalTimeSpan.Quarter, MusicalTimeSpan.Half, tempoMap),
                },
                new SteppedGrid(MusicalTimeSpan.ThirtySecond, new[] { MusicalTimeSpan.Eighth, MusicalTimeSpan.Quarter }),
                new[]
                {
                    new TimeAndLength(MusicalTimeSpan.ThirtySecond, MusicalTimeSpan.Whole - MusicalTimeSpan.ThirtySecond),
                    new TimeAndLength(MusicalTimeSpan.ThirtySecond, MusicalTimeSpan.Whole),
                    new TimeAndLength(MusicalTimeSpan.Eighth + MusicalTimeSpan.ThirtySecond, new MusicalTimeSpan()),
                    new TimeAndLength(MusicalTimeSpan.Whole + MusicalTimeSpan.Quarter + MusicalTimeSpan.ThirtySecond, MusicalTimeSpan.Half - MusicalTimeSpan.ThirtySecond)
                },
                tempoMap);
        }

        [Test]
        [Description("Quantize start times that go beyond fixed ends: skip object.")]
        public void Quantize_Start_QuantizingBeyondFixedEnd_Skip()
        {
            var tempoMap = TempoMap.Default;

            Quantize_Start_QuantizingBeyondFixedEnd(
                new[]
                {
                    // The case
                    Methods.Create(MusicalTimeSpan.Quarter.SingleDotted(), MusicalTimeSpan.ThirtySecond, tempoMap),
                    // Already quantized
                    Methods.Create(MusicalTimeSpan.Half, MusicalTimeSpan.Whole, tempoMap),
                    // Regular case
                    Methods.Create(MusicalTimeSpan.Quarter.SingleDotted(), MusicalTimeSpan.Quarter, tempoMap)
                },
                new SteppedGrid(MusicalTimeSpan.Half),
                new[]
                {
                    new TimeAndLength(MusicalTimeSpan.Quarter.SingleDotted(), MusicalTimeSpan.ThirtySecond),
                    new TimeAndLength(MusicalTimeSpan.Half, MusicalTimeSpan.Whole),
                    new TimeAndLength(MusicalTimeSpan.Half, MusicalTimeSpan.Eighth)
                },
                tempoMap,
                QuantizingBeyondFixedEndPolicy.Skip);
        }

        [Test]
        [Description("Quantize start times that go beyond fixed ends: abort quantizing.")]
        public void Quantize_Start_QuantizingBeyondFixedEnd_Abort()
        {
            var tempoMap = TempoMap.Default;

            Assert.Throws<InvalidOperationException>(() => Quantize_Start_QuantizingBeyondFixedEnd(
                new[]
                {
                    Methods.Create(MusicalTimeSpan.Quarter.SingleDotted(), MusicalTimeSpan.ThirtySecond, tempoMap)
                },
                new SteppedGrid(MusicalTimeSpan.Half),
                new[]
                {
                    new TimeAndLength(MusicalTimeSpan.Quarter.SingleDotted(), MusicalTimeSpan.ThirtySecond)
                },
                tempoMap,
                QuantizingBeyondFixedEndPolicy.Abort));
        }

        [Test]
        [Description("Quantize start times that go beyond fixed ends: swap ends.")]
        public void Quantize_Start_QuantizingBeyondFixedEnd_SwapEnds()
        {
            var tempoMap = TempoMap.Default;

            Quantize_Start_QuantizingBeyondFixedEnd(
                new[]
                {
                    // The case
                    Methods.Create(MusicalTimeSpan.Quarter.SingleDotted(), MusicalTimeSpan.ThirtySecond, tempoMap),
                    // Already quantized
                    Methods.Create(MusicalTimeSpan.Half, MusicalTimeSpan.Whole, tempoMap),
                    // Regular case
                    Methods.Create(MusicalTimeSpan.Quarter.SingleDotted(), MusicalTimeSpan.Quarter, tempoMap)
                },
                new SteppedGrid(MusicalTimeSpan.Half),
                new[]
                {
                    new TimeAndLength(MusicalTimeSpan.Quarter.SingleDotted() + MusicalTimeSpan.ThirtySecond, MusicalTimeSpan.Eighth - MusicalTimeSpan.ThirtySecond),
                    new TimeAndLength(MusicalTimeSpan.Half, MusicalTimeSpan.Whole),
                    new TimeAndLength(MusicalTimeSpan.Half, MusicalTimeSpan.Eighth)
                },
                tempoMap,
                QuantizingBeyondFixedEndPolicy.SwapEnds);
        }

        [Test]
        [Description("Quantize start times that go beyond fixed ends: collapse object and fix at end.")]
        public void Quantize_Start_QuantizingBeyondFixedEnd_CollapseAndFix()
        {
            var tempoMap = TempoMap.Default;

            Quantize_Start_QuantizingBeyondFixedEnd(
                new[]
                {
                    // The case
                    Methods.Create(MusicalTimeSpan.Quarter.SingleDotted(), MusicalTimeSpan.ThirtySecond, tempoMap),
                    // Already quantized
                    Methods.Create(MusicalTimeSpan.Half, MusicalTimeSpan.Whole, tempoMap),
                    // Regular case
                    Methods.Create(MusicalTimeSpan.Quarter.SingleDotted(), MusicalTimeSpan.Quarter, tempoMap)
                },
                new SteppedGrid(MusicalTimeSpan.Half),
                new[]
                {
                    new TimeAndLength(MusicalTimeSpan.Quarter.SingleDotted() + MusicalTimeSpan.ThirtySecond, new MusicalTimeSpan()),
                    new TimeAndLength(MusicalTimeSpan.Half, MusicalTimeSpan.Whole),
                    new TimeAndLength(MusicalTimeSpan.Half, MusicalTimeSpan.Eighth)
                },
                tempoMap,
                QuantizingBeyondFixedEndPolicy.CollapseAndFix);
        }

        [Test]
        [Description("Quantize start times that go beyond fixed ends: collapse object and move to grid point.")]
        public void Quantize_Start_QuantizingBeyondFixedEnd_CollapseAndMove()
        {
            var tempoMap = TempoMap.Default;

            Quantize_Start_QuantizingBeyondFixedEnd(
                new[]
                {
                    // The case
                    Methods.Create(MusicalTimeSpan.Quarter.SingleDotted(), MusicalTimeSpan.ThirtySecond, tempoMap),
                    // Already quantized
                    Methods.Create(MusicalTimeSpan.Half, MusicalTimeSpan.Whole, tempoMap),
                    // Regular case
                    Methods.Create(MusicalTimeSpan.Quarter.SingleDotted(), MusicalTimeSpan.Quarter, tempoMap)
                },
                new SteppedGrid(MusicalTimeSpan.Half),
                new[]
                {
                    new TimeAndLength(MusicalTimeSpan.Half, new MusicalTimeSpan()),
                    new TimeAndLength(MusicalTimeSpan.Half, MusicalTimeSpan.Whole),
                    new TimeAndLength(MusicalTimeSpan.Half, MusicalTimeSpan.Eighth)
                },
                tempoMap,
                QuantizingBeyondFixedEndPolicy.CollapseAndMove);
        }

        [Test]
        [Description("Quantize start times using metric distance calculation.")]
        public void Quantize_Start_MetricDistance()
        {
            TempoMap tempoMap = null;

            using (var tempoMapManager = new TempoMapManager())
            {
                tempoMapManager.SetTempo(0, new Tempo(100000));
                tempoMapManager.SetTempo(new MetricTimeSpan(0, 0, 1, 500),
                                         new Tempo(40000));

                tempoMap = tempoMapManager.TempoMap;
            }

            Quantize_Start_DontFixEnd_CustomDistanceType(
                new[]
                {
                    // The case
                    Methods.Create(new MetricTimeSpan(0, 0, 1, 600), new MetricTimeSpan(0, 0, 2), tempoMap),
                    // Already quantized
                    Methods.Create(new MetricTimeSpan(0, 0, 1), new MetricTimeSpan(0, 0, 2), tempoMap)
                },
                new SteppedGrid(new MetricTimeSpan(0, 0, 1)),
                new[]
                {
                    new MetricTimeSpan(0, 0, 2),
                    new MetricTimeSpan(0, 0, 1)
                },
                tempoMap,
                TimeSpanType.Metric);
        }

        [Test]
        [Description("Quantize start times using metric length hint.")]
        public void Quantize_Start_MetricLength()
        {
            TempoMap tempoMap = null;

            using (var tempoMapManager = new TempoMapManager())
            {
                tempoMapManager.SetTempo(0, new Tempo(100000));
                tempoMapManager.SetTempo(new MetricTimeSpan(0, 0, 2, 100),
                                         new Tempo(40000));

                tempoMap = tempoMapManager.TempoMap;
            }

            Quantize_Start_DontFixEnd_CustomLengthType(
                new[]
                {
                    // The case
                    Methods.Create(new MetricTimeSpan(0, 0, 1, 600), new MetricTimeSpan(0, 0, 2), tempoMap),
                    // The case in presence of no tempo change
                    Methods.Create(new MetricTimeSpan(0, 0, 0, 100), new MetricTimeSpan(0, 0, 1), tempoMap),
                    // Already quantized
                    Methods.Create(new MetricTimeSpan(0, 0, 1), new MetricTimeSpan(0, 0, 2), tempoMap)
                },
                new SteppedGrid(new MetricTimeSpan(0, 0, 1)),
                new[]
                {
                    new TimeAndLength(new MetricTimeSpan(0, 0, 2), new MetricTimeSpan(0, 0, 2)),
                    new TimeAndLength(new MetricTimeSpan(), new MetricTimeSpan(0, 0, 1)),
                    new TimeAndLength(new MetricTimeSpan(0, 0, 1), new MetricTimeSpan(0, 0, 2))
                },
                tempoMap,
                TimeSpanType.Metric);
        }

        [Test]
        [Description("Quantize end times of empty collection.")]
        public void Quantize_End_EmptyCollection()
        {
            var tempoMap = TempoMap.Default;

            Quantize_End_DontFixStart(
                Enumerable.Empty<TObject>(),
                new SteppedGrid(MusicalTimeSpan.Eighth),
                Enumerable.Empty<ITimeSpan>(),
                tempoMap);
        }

        [Test]
        [Description("Quantize end times of nulls.")]
        public void Quantize_End_Nulls()
        {
            var tempoMap = TempoMap.Default;

            Quantize_End_DontFixStart(
                new[] { default(TObject), default(TObject) },
                new SteppedGrid(MusicalTimeSpan.Eighth),
                new ITimeSpan[] { null, null },
                tempoMap);
        }

        [Test]
        [Description("Quantize end times by grid of one step starting from zero.")]
        public void Quantize_End_OneStep_FromZero()
        {
            var tempoMap = TempoMap.Default;

            Quantize_End_DontFixStart(
                new[]
                {
                    Methods.Create((MidiTimeSpan)0, MusicalTimeSpan.Whole, tempoMap),
                    Methods.Create(MusicalTimeSpan.Quarter + MusicalTimeSpan.Sixteenth, MusicalTimeSpan.Half, tempoMap),
                    Methods.Create(MusicalTimeSpan.Eighth, MusicalTimeSpan.Whole, tempoMap)
                },
                new SteppedGrid(MusicalTimeSpan.Eighth),
                new ITimeSpan[]
                {
                    (MidiTimeSpan)0,
                    MusicalTimeSpan.Quarter,
                    MusicalTimeSpan.Eighth
                },
                tempoMap);
        }

        [Test]
        [Description("Quantize end times by grid of one step starting away from zero.")]
        public void Quantize_End_OneStep_AwayFromZero()
        {
            var tempoMap = TempoMap.Default;

            Quantize_End_DontFixStart(
                new[]
                {
                    Methods.Create(MusicalTimeSpan.Quarter, MusicalTimeSpan.Half.SingleDotted() + MusicalTimeSpan.ThirtySecond, tempoMap),
                    Methods.Create(MusicalTimeSpan.Whole + MusicalTimeSpan.Sixteenth, MusicalTimeSpan.Half, tempoMap),
                    Methods.Create(MusicalTimeSpan.Whole.SingleDotted(), MusicalTimeSpan.Half, tempoMap)
                },
                new SteppedGrid(MusicalTimeSpan.Whole, MusicalTimeSpan.Eighth),
                new ITimeSpan[]
                {
                    MusicalTimeSpan.Quarter - MusicalTimeSpan.ThirtySecond,
                    MusicalTimeSpan.Whole,
                    MusicalTimeSpan.Whole.SingleDotted()
                },
                tempoMap);
        }

        [Test]
        [Description("Quantize end times by grid of one step keeping start times untouched.")]
        public void Quantize_End_OneStep_FixStart()
        {
            var tempoMap = TempoMap.Default;

            Quantize_End_FixStart(
                new[]
                {
                    // Increase length
                    Methods.Create((MidiTimeSpan)0, MusicalTimeSpan.Whole, tempoMap),
                    // Already quantized
                    Methods.Create(MusicalTimeSpan.Eighth + MusicalTimeSpan.Half, MusicalTimeSpan.Whole, tempoMap),
                    // Decrease length
                    Methods.Create(MusicalTimeSpan.Sixteenth + MusicalTimeSpan.Quarter, MusicalTimeSpan.Half, tempoMap)
                },
                new SteppedGrid(MusicalTimeSpan.Eighth, MusicalTimeSpan.Half),
                new[]
                {
                    new TimeAndLength((MidiTimeSpan)0, MusicalTimeSpan.Whole + MusicalTimeSpan.Eighth),
                    new TimeAndLength(MusicalTimeSpan.Eighth + MusicalTimeSpan.Half, MusicalTimeSpan.Whole),
                    new TimeAndLength(MusicalTimeSpan.Sixteenth + MusicalTimeSpan.Quarter, MusicalTimeSpan.Half + MusicalTimeSpan.Eighth - MusicalTimeSpan.Sixteenth - MusicalTimeSpan.Quarter)
                },
                tempoMap);
        }

        [Test]
        [Description("Quantize end times by grid of multiple steps starting from zero.")]
        public void Quantize_End_MultipleSteps_FromZero()
        {
            var tempoMap = TempoMap.Default;

            Quantize_End_DontFixStart(
                new[]
                {
                    // Already quantized
                    Methods.Create((MidiTimeSpan)0, MusicalTimeSpan.Whole - MusicalTimeSpan.Eighth, tempoMap),
                    // Move forward
                    Methods.Create(MusicalTimeSpan.Eighth + MusicalTimeSpan.Whole + 3 * MusicalTimeSpan.ThirtySecond, MusicalTimeSpan.Whole, tempoMap),
                    // Move back
                    Methods.Create(MusicalTimeSpan.Eighth + MusicalTimeSpan.Whole + MusicalTimeSpan.ThirtySecond, MusicalTimeSpan.Half, tempoMap),
                },
                new SteppedGrid(new[] { MusicalTimeSpan.Eighth, MusicalTimeSpan.Quarter }),
                new ITimeSpan[]
                {
                    (MidiTimeSpan)0,
                    MusicalTimeSpan.Whole + MusicalTimeSpan.Quarter,
                    MusicalTimeSpan.Eighth + MusicalTimeSpan.Whole
                },
                tempoMap);
        }

        [Test]
        [Description("Quantize end times by grid of multiple steps starting away from zero.")]
        public void Quantize_End_MultipleSteps_AwayFromZero()
        {
            var tempoMap = TempoMap.Default;

            Quantize_End_DontFixStart(
                new[]
                {
                    // Already quantized
                    Methods.Create(MusicalTimeSpan.ThirtySecond, MusicalTimeSpan.Whole - MusicalTimeSpan.Eighth, tempoMap),
                    // Move forward
                    Methods.Create((MidiTimeSpan)0, MusicalTimeSpan.Whole + 3 * MusicalTimeSpan.ThirtySecond, tempoMap),
                    // Move back
                    Methods.Create(MusicalTimeSpan.Eighth, MusicalTimeSpan.Whole - MusicalTimeSpan.Eighth, tempoMap),
                },
                new SteppedGrid(MusicalTimeSpan.ThirtySecond, new[] { MusicalTimeSpan.Eighth, MusicalTimeSpan.Quarter }),
                new ITimeSpan[]
                {
                    MusicalTimeSpan.ThirtySecond,
                    MusicalTimeSpan.Sixteenth,
                    MusicalTimeSpan.ThirtySecond
                },
                tempoMap);
        }

        [Test]
        [Description("Quantize end times by grid of multiple steps keeping start times untouched.")]
        public void Quantize_End_MultipleSteps_FixStart()
        {
            var tempoMap = TempoMap.Default;

            Quantize_End_FixStart(
                new[]
                {
                    // Already quantized
                    Methods.Create(MusicalTimeSpan.ThirtySecond, MusicalTimeSpan.Whole - MusicalTimeSpan.Eighth, tempoMap),
                    // Move forward
                    Methods.Create((MidiTimeSpan)0, MusicalTimeSpan.Whole + 3 * MusicalTimeSpan.ThirtySecond, tempoMap),
                    // Move back
                    Methods.Create(MusicalTimeSpan.Eighth, MusicalTimeSpan.Whole - MusicalTimeSpan.Eighth, tempoMap),
                },
                new SteppedGrid(MusicalTimeSpan.ThirtySecond, new[] { MusicalTimeSpan.Eighth, MusicalTimeSpan.Quarter }),
                new[]
                {
                    new TimeAndLength(MusicalTimeSpan.ThirtySecond, MusicalTimeSpan.Whole - MusicalTimeSpan.Eighth),
                    new TimeAndLength((MidiTimeSpan)0, MusicalTimeSpan.Whole + 5 * MusicalTimeSpan.ThirtySecond),
                    new TimeAndLength(MusicalTimeSpan.Eighth, MusicalTimeSpan.Whole - MusicalTimeSpan.Eighth - 3 * MusicalTimeSpan.ThirtySecond),
                },
                tempoMap);
        }

        [Test]
        [Description("Quantize end times that leads to object goes beyond zero: skip object.")]
        public void Quantize_End_QuantizingBeyondZero_Skip()
        {
            var tempoMap = TempoMap.Default;

            Quantize_End_QuantizingBeyondZero(
                new[]
                {
                    // The case
                    Methods.Create((MidiTimeSpan)0, MusicalTimeSpan.Quarter + MusicalTimeSpan.Sixteenth, tempoMap),
                    // Quantized exactly at zero
                    Methods.Create(MusicalTimeSpan.ThirtySecond, MusicalTimeSpan.Quarter, tempoMap),
                    // Already quantized
                    Methods.Create(MusicalTimeSpan.ThirtySecond, MusicalTimeSpan.Quarter - MusicalTimeSpan.ThirtySecond, tempoMap),
                    // Regular case
                    Methods.Create(MusicalTimeSpan.ThirtySecond, MusicalTimeSpan.Quarter.SingleDotted(), tempoMap)
                },
                new SteppedGrid(MusicalTimeSpan.Quarter),
                new[]
                {
                    new TimeAndLength((MidiTimeSpan)0, MusicalTimeSpan.Quarter + MusicalTimeSpan.Sixteenth),
                    new TimeAndLength((MidiTimeSpan)0, MusicalTimeSpan.Quarter),
                    new TimeAndLength(MusicalTimeSpan.ThirtySecond, MusicalTimeSpan.Quarter - MusicalTimeSpan.ThirtySecond),
                    new TimeAndLength(MusicalTimeSpan.Eighth, MusicalTimeSpan.Quarter.SingleDotted())
                },
                tempoMap,
                QuantizingBeyondZeroPolicy.Skip);
        }

        [Test]
        [Description("Quantize end times that leads to object goes beyond zero: abort quantizing.")]
        public void Quantize_End_QuantizingBeyondZero_Abort()
        {
            var tempoMap = TempoMap.Default;

            Assert.Throws<InvalidOperationException>(() => Quantize_End_QuantizingBeyondZero(
                new[]
                {
                    Methods.Create((MidiTimeSpan)0, MusicalTimeSpan.Quarter + MusicalTimeSpan.Sixteenth, tempoMap)
                },
                new SteppedGrid(MusicalTimeSpan.Quarter),
                new[]
                {
                    new TimeAndLength((MidiTimeSpan)0, MusicalTimeSpan.Quarter + MusicalTimeSpan.Sixteenth)
                },
                tempoMap,
                QuantizingBeyondZeroPolicy.Abort));
        }

        [Test]
        [Description("Quantize end times that leads to object goes beyond zero: fix start time at zero and shrink object.")]
        public void Quantize_End_QuantizingBeyondZero_FixAtZero()
        {
            var tempoMap = TempoMap.Default;

            Quantize_End_QuantizingBeyondZero(
                new[]
                {
                    // The case
                    Methods.Create((MidiTimeSpan)0, MusicalTimeSpan.Quarter + MusicalTimeSpan.Sixteenth, tempoMap),
                    // Quantized exactly at zero
                    Methods.Create(MusicalTimeSpan.ThirtySecond, MusicalTimeSpan.Half, tempoMap),
                    // Already quantized
                    Methods.Create(MusicalTimeSpan.ThirtySecond, MusicalTimeSpan.Quarter - MusicalTimeSpan.ThirtySecond, tempoMap),
                    // Regular case
                    Methods.Create(MusicalTimeSpan.ThirtySecond, MusicalTimeSpan.Quarter.SingleDotted(), tempoMap)
                },
                new SteppedGrid(MusicalTimeSpan.Quarter),
                new[]
                {
                    new TimeAndLength((MidiTimeSpan)0, MusicalTimeSpan.Quarter),
                    new TimeAndLength((MidiTimeSpan)0, MusicalTimeSpan.Half),
                    new TimeAndLength(MusicalTimeSpan.ThirtySecond, MusicalTimeSpan.Quarter - MusicalTimeSpan.ThirtySecond),
                    new TimeAndLength(MusicalTimeSpan.Eighth, MusicalTimeSpan.Quarter.SingleDotted())
                },
                tempoMap,
                QuantizingBeyondZeroPolicy.FixAtZero);
        }

        [Test]
        [Description("Quantize end times that leads to object goes beyond zero: use next grid point.")]
        public void Quantize_End_QuantizingBeyondZero_UseNextGridPoint()
        {
            var tempoMap = TempoMap.Default;

            Quantize_End_QuantizingBeyondZero(
                new[]
                {
                    // The case
                    Methods.Create((MidiTimeSpan)0, MusicalTimeSpan.Half + MusicalTimeSpan.Sixteenth, tempoMap),
                    // Quantized exactly at zero
                    Methods.Create(MusicalTimeSpan.ThirtySecond, MusicalTimeSpan.Half, tempoMap),
                    // Already quantized
                    Methods.Create(MusicalTimeSpan.Half, MusicalTimeSpan.Whole, tempoMap),
                    // Regular case
                    Methods.Create(MusicalTimeSpan.ThirtySecond, MusicalTimeSpan.Half.SingleDotted(), tempoMap)
                },
                new SteppedGrid(MusicalTimeSpan.Half),
                new[]
                {
                    new TimeAndLength(MusicalTimeSpan.Half - MusicalTimeSpan.Sixteenth, MusicalTimeSpan.Half + MusicalTimeSpan.Sixteenth),
                    new TimeAndLength((MidiTimeSpan)0, MusicalTimeSpan.Half),
                    new TimeAndLength(MusicalTimeSpan.Half, MusicalTimeSpan.Whole),
                    new TimeAndLength(MusicalTimeSpan.Quarter, MusicalTimeSpan.Half.SingleDotted())
                },
                tempoMap,
                QuantizingBeyondZeroPolicy.UseNextGridPoint);
        }

        [Test]
        [Description("Quantize end times that go beyond fixed starts: skip object.")]
        public void Quantize_End_QuantizingBeyondFixedEnd_Skip()
        {
            var tempoMap = TempoMap.Default;

            Quantize_End_QuantizingBeyondFixedEnd(
                new[]
                {
                    // The case
                    Methods.Create(MusicalTimeSpan.Half + MusicalTimeSpan.Sixteenth, MusicalTimeSpan.ThirtySecond, tempoMap),
                    // Already quantized
                    Methods.Create(MusicalTimeSpan.Half, MusicalTimeSpan.Whole, tempoMap),
                    // Regular case
                    Methods.Create(MusicalTimeSpan.ThirtySecond, MusicalTimeSpan.Half.SingleDotted(), tempoMap)
                },
                new SteppedGrid(MusicalTimeSpan.Half),
                new[]
                {
                    new TimeAndLength(MusicalTimeSpan.Half + MusicalTimeSpan.Sixteenth, MusicalTimeSpan.ThirtySecond),
                    new TimeAndLength(MusicalTimeSpan.Half, MusicalTimeSpan.Whole),
                    new TimeAndLength(MusicalTimeSpan.ThirtySecond, MusicalTimeSpan.Whole - MusicalTimeSpan.ThirtySecond)
                },
                tempoMap,
                QuantizingBeyondFixedEndPolicy.Skip);
        }

        [Test]
        [Description("Quantize end times that go beyond fixed starts: abort quantizing.")]
        public void Quantize_End_QuantizingBeyondFixedEnd_Abort()
        {
            var tempoMap = TempoMap.Default;

            Assert.Throws<InvalidOperationException>(() => Quantize_End_QuantizingBeyondFixedEnd(
                new[]
                {
                    Methods.Create(MusicalTimeSpan.Half + MusicalTimeSpan.Sixteenth, MusicalTimeSpan.ThirtySecond, tempoMap)
                },
                new SteppedGrid(MusicalTimeSpan.Half),
                new[]
                {
                    new TimeAndLength(MusicalTimeSpan.Half + MusicalTimeSpan.Sixteenth, MusicalTimeSpan.ThirtySecond)
                },
                tempoMap,
                QuantizingBeyondFixedEndPolicy.Abort));
        }

        [Test]
        [Description("Quantize end times that go beyond fixed starts: swap ends.")]
        public void Quantize_End_QuantizingBeyondFixedEnd_SwapEnds()
        {
            var tempoMap = TempoMap.Default;

            Quantize_End_QuantizingBeyondFixedEnd(
                new[]
                {
                    // The case
                    Methods.Create(MusicalTimeSpan.Half + MusicalTimeSpan.Sixteenth, MusicalTimeSpan.ThirtySecond, tempoMap),
                    // Already quantized
                    Methods.Create(MusicalTimeSpan.Half, MusicalTimeSpan.Whole, tempoMap),
                    // Regular case
                    Methods.Create(MusicalTimeSpan.ThirtySecond, MusicalTimeSpan.Half.SingleDotted(), tempoMap)
                },
                new SteppedGrid(MusicalTimeSpan.Half),
                new[]
                {
                    new TimeAndLength(MusicalTimeSpan.Half, MusicalTimeSpan.Sixteenth),
                    new TimeAndLength(MusicalTimeSpan.Half, MusicalTimeSpan.Whole),
                    new TimeAndLength(MusicalTimeSpan.ThirtySecond, MusicalTimeSpan.Whole - MusicalTimeSpan.ThirtySecond)
                },
                tempoMap,
                QuantizingBeyondFixedEndPolicy.SwapEnds);
        }

        [Test]
        [Description("Quantize end times that go beyond fixed starts: collapse object and fix at start.")]
        public void Quantize_End_QuantizingBeyondFixedEnd_CollapseAndFix()
        {
            var tempoMap = TempoMap.Default;

            Quantize_End_QuantizingBeyondFixedEnd(
                new[]
                {
                    // The case
                    Methods.Create(MusicalTimeSpan.Half + MusicalTimeSpan.Sixteenth, MusicalTimeSpan.ThirtySecond, tempoMap),
                    // Already quantized
                    Methods.Create(MusicalTimeSpan.Half, MusicalTimeSpan.Whole, tempoMap),
                    // Regular case
                    Methods.Create(MusicalTimeSpan.ThirtySecond, MusicalTimeSpan.Half.SingleDotted(), tempoMap)
                },
                new SteppedGrid(MusicalTimeSpan.Half),
                new[]
                {
                    new TimeAndLength(MusicalTimeSpan.Half + MusicalTimeSpan.Sixteenth, new MusicalTimeSpan()),
                    new TimeAndLength(MusicalTimeSpan.Half, MusicalTimeSpan.Whole),
                    new TimeAndLength(MusicalTimeSpan.ThirtySecond, MusicalTimeSpan.Whole - MusicalTimeSpan.ThirtySecond)
                },
                tempoMap,
                QuantizingBeyondFixedEndPolicy.CollapseAndFix);
        }

        [Test]
        [Description("Quantize end times that go beyond fixed starts: collapse object and move to grid point.")]
        public void Quantize_End_QuantizingBeyondFixedEnd_CollapseAndMove()
        {
            var tempoMap = TempoMap.Default;

            Quantize_End_QuantizingBeyondFixedEnd(
                new[]
                {
                    // The case
                    Methods.Create(MusicalTimeSpan.Half + MusicalTimeSpan.Sixteenth, MusicalTimeSpan.ThirtySecond, tempoMap),
                    // Already quantized
                    Methods.Create(MusicalTimeSpan.Half, MusicalTimeSpan.Whole, tempoMap),
                    // Regular case
                    Methods.Create(MusicalTimeSpan.ThirtySecond, MusicalTimeSpan.Half.SingleDotted(), tempoMap)
                },
                new SteppedGrid(MusicalTimeSpan.Half),
                new[]
                {
                    new TimeAndLength(MusicalTimeSpan.Half, new MusicalTimeSpan()),
                    new TimeAndLength(MusicalTimeSpan.Half, MusicalTimeSpan.Whole),
                    new TimeAndLength(MusicalTimeSpan.ThirtySecond, MusicalTimeSpan.Whole - MusicalTimeSpan.ThirtySecond)
                },
                tempoMap,
                QuantizingBeyondFixedEndPolicy.CollapseAndMove);
        }

        #endregion

        #region Private methods

        private void Quantize_Start_DontFixEnd(IEnumerable<TObject> actualObjects, IGrid grid, IEnumerable<ITimeSpan> expectedTimes, TempoMap tempoMap)
        {
            Quantize_Start(actualObjects, grid, GetExpectedTimesAndLengths(actualObjects, expectedTimes), false, tempoMap);
        }

        private void Quantize_Start_DontFixEnd_CustomDistanceType(IEnumerable<TObject> actualObjects, IGrid grid, IEnumerable<ITimeSpan> expectedTimes, TempoMap tempoMap, TimeSpanType distanceType)
        {
            Quantize_Start(actualObjects, grid, GetExpectedTimesAndLengths(actualObjects, expectedTimes), false, tempoMap, distanceType: distanceType);
        }

        private void Quantize_Start_DontFixEnd_CustomLengthType(IEnumerable<TObject> actualObjects, IGrid grid, IEnumerable<TimeAndLength> expectedTimes, TempoMap tempoMap, TimeSpanType lengthType)
        {
            Quantize_Start(actualObjects, grid, expectedTimes, false, tempoMap, lengthType: lengthType);
        }

        private void Quantize_Start_FixEnd(IEnumerable<TObject> actualObjects, IGrid grid, IEnumerable<TimeAndLength> expectedTimes, TempoMap tempoMap)
        {
            Quantize_Start(actualObjects, grid, expectedTimes, true, tempoMap);
        }

        private void Quantize_Start_QuantizingBeyondFixedEnd(IEnumerable<TObject> actualObjects, IGrid grid, IEnumerable<TimeAndLength> expectedTimes, TempoMap tempoMap, QuantizingBeyondFixedEndPolicy policy)
        {
            Quantize_Start(actualObjects, grid, expectedTimes, true, tempoMap, policy);
        }

        private void Quantize_End_DontFixStart(IEnumerable<TObject> actualObjects, IGrid grid, IEnumerable<ITimeSpan> expectedTimes, TempoMap tempoMap)
        {
            Quantize_End(actualObjects, grid, GetExpectedTimesAndLengths(actualObjects, expectedTimes), false, tempoMap);
        }

        private void Quantize_End_FixStart(IEnumerable<TObject> actualObjects, IGrid grid, IEnumerable<TimeAndLength> expectedTimes, TempoMap tempoMap)
        {
            Quantize_End(actualObjects, grid, expectedTimes, true, tempoMap);
        }

        private void Quantize_End_QuantizingBeyondZero(IEnumerable<TObject> actualObjects, IGrid grid, IEnumerable<TimeAndLength> expectedTimes, TempoMap tempoMap, QuantizingBeyondZeroPolicy policy)
        {
            Quantize_End(actualObjects, grid, expectedTimes, false, tempoMap, policy);
        }

        private void Quantize_End_QuantizingBeyondFixedEnd(IEnumerable<TObject> actualObjects, IGrid grid, IEnumerable<TimeAndLength> expectedTimes, TempoMap tempoMap, QuantizingBeyondFixedEndPolicy policy)
        {
            Quantize_End(actualObjects, grid, expectedTimes, true, tempoMap, default(QuantizingBeyondZeroPolicy), policy);
        }

        private void Quantize_Start(IEnumerable<TObject> actualObjects,
                                    IGrid grid,
                                    IEnumerable<TimeAndLength> expectedTimesAndLengths,
                                    bool fixEnd,
                                    TempoMap tempoMap,
                                    QuantizingBeyondFixedEndPolicy policy = default(QuantizingBeyondFixedEndPolicy),
                                    TimeSpanType distanceType = TimeSpanType.Midi,
                                    TimeSpanType lengthType = TimeSpanType.Midi)
        {
            var expectedObjects = GetExpectedObjects(actualObjects, expectedTimesAndLengths, tempoMap);

            var settings = new TSettings
            {
                QuantizingTarget = LengthedObjectTarget.Start,
                FixOppositeEnd = fixEnd,
                QuantizingBeyondFixedEndPolicy = policy,
                DistanceType = distanceType,
                LengthType = lengthType
            };

            Quantizer.Quantize(actualObjects, grid, tempoMap, settings);

            Methods.AssertCollectionsAreEqual(expectedObjects, actualObjects);
        }

        private void Quantize_End(IEnumerable<TObject> actualObjects,
                                  IGrid grid,
                                  IEnumerable<TimeAndLength> expectedTimesAndLengths,
                                  bool fixStart,
                                  TempoMap tempoMap,
                                  QuantizingBeyondZeroPolicy quantizingBeyondZeroPolicy = default(QuantizingBeyondZeroPolicy),
                                  QuantizingBeyondFixedEndPolicy quantizingBeyondFixedEndPolicy = default(QuantizingBeyondFixedEndPolicy),
                                  TimeSpanType distanceType = TimeSpanType.Midi,
                                  TimeSpanType lengthType = TimeSpanType.Midi)
        {
            var expectedObjects = GetExpectedObjects(actualObjects, expectedTimesAndLengths, tempoMap);

            var settings = new TSettings
            {
                QuantizingTarget = LengthedObjectTarget.End,
                FixOppositeEnd = fixStart,
                QuantizingBeyondZeroPolicy = quantizingBeyondZeroPolicy,
                QuantizingBeyondFixedEndPolicy = quantizingBeyondFixedEndPolicy,
                DistanceType = distanceType,
                LengthType = lengthType
            };

            Quantizer.Quantize(actualObjects, grid, tempoMap, settings);

            Methods.AssertCollectionsAreEqual(expectedObjects, actualObjects);
        }

        private IEnumerable<TimeAndLength> GetExpectedTimesAndLengths(IEnumerable<TObject> actualObjects, IEnumerable<ITimeSpan> expectedTimes)
        {
            return actualObjects.Zip(expectedTimes, (obj, time) => new { Object = obj, Time = time })
                                .Select(ot => ot.Object != null ? new TimeAndLength(ot.Time, (MidiTimeSpan)ot.Object.Length) : null);
        }

        private IEnumerable<TObject> GetExpectedObjects(IEnumerable<TObject> actualObjects, IEnumerable<TimeAndLength> expectedTimesAndLengths, TempoMap tempoMap)
        {
            var descriptors = actualObjects.Zip(expectedTimesAndLengths, (obj, timeAndLength) => new { Object = obj != null ? Methods.Clone(obj) : default(TObject), TimeAndLength = timeAndLength }).ToList();

            foreach (var descriptor in descriptors)
            {
                var obj = descriptor.Object;
                var timeAndLength = descriptor.TimeAndLength;
                if (obj == null || timeAndLength == null)
                    continue;

                Methods.SetTime(obj, timeAndLength.Time, tempoMap);
                Methods.SetLength(obj, timeAndLength.Length, timeAndLength.Time, tempoMap);
            }

            return descriptors.Select(d => d.Object).ToList();
        }

        #endregion
    }
}
