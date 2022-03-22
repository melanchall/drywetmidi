using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    // TODO: randomize end
    public abstract partial class LengthedObjectsQuantizerTests<TObject, TSettings>
    {
        #region Nested classes

        private sealed class TimeBounds
        {
            #region Constructor

            public TimeBounds(ITimeSpan minTime, ITimeSpan maxTime)
            {
                MinTime = minTime;
                MaxTime = maxTime;
            }

            #endregion

            #region Properties

            public ITimeSpan MinTime { get; }

            public ITimeSpan MaxTime { get; }

            #endregion

            #region Overrides

            public override string ToString()
            {
                return $"[{MinTime}; {MaxTime}]";
            }

            #endregion
        }

        #endregion

        #region Constants

        private const int RepeatRandomizationCount = 10000;

        #endregion

        #region Test methods

        [Test]
        public void Randomize_Start_EmptyCollection()
        {
            var tempoMap = TempoMap.Default;

            Randomize_Start(
                Enumerable.Empty<TObject>(),
                new ConstantBounds((MidiTimeSpan)123),
                Enumerable.Empty<TimeBounds>(),
                tempoMap);
        }

        [Test]
        public void Randomize_Start_Nulls()
        {
            var tempoMap = TempoMap.Default;

            Randomize_Start(
                new[] { default(TObject), default(TObject) },
                new ConstantBounds((MidiTimeSpan)123),
                new[]
                {
                    new TimeBounds(null, null),
                    new TimeBounds(null, null)
                },
                tempoMap);
        }

        [Test]
        public void Randomize_Start_Constant_Zero()
        {
            var tempoMap = TempoMap.Default;

            Randomize_Start(
                new[]
                {
                    ObjectMethods.Create(1000, 1000),
                    ObjectMethods.Create(0, 10000),
                },
                new ConstantBounds((MidiTimeSpan)0),
                new[]
                {
                    new TimeBounds((MidiTimeSpan)1000, (MidiTimeSpan)1000),
                    new TimeBounds((MidiTimeSpan)0, (MidiTimeSpan)0)
                },
                tempoMap,
                checkUnchangedTimeObjectsPercent: false);
        }

        [Test]
        public void Randomize_Start_Constant_SizeGreaterThanTime_Midi()
        {
            var tempoMap = TempoMap.Default;

            Randomize_Start(
                new[]
                {
                    ObjectMethods.Create(1000, 1000),
                    ObjectMethods.Create(0, 10000),
                },
                new ConstantBounds((MidiTimeSpan)10000),
                new[]
                {
                    new TimeBounds((MidiTimeSpan)0, (MidiTimeSpan)11000),
                    new TimeBounds((MidiTimeSpan)0, (MidiTimeSpan)10000)
                },
                tempoMap);
        }

        [Test]
        public void Randomize_Start_Constant_SizeGreaterThanTime_Metric()
        {
            var tempoMap = TempoMap.Default;

            Randomize_Start(
                new[]
                {
                    ObjectMethods.Create(new MetricTimeSpan(0, 1, 23), (MidiTimeSpan)1000, tempoMap),
                    ObjectMethods.Create(0, 10000),
                },
                new ConstantBounds(new MetricTimeSpan(0, 2, 0)),
                new[]
                {
                    new TimeBounds((MidiTimeSpan)0, new MetricTimeSpan(0, 3, 23)),
                    new TimeBounds((MidiTimeSpan)0, new MetricTimeSpan(0, 2, 0))
                },
                tempoMap);
        }

        [Test]
        public void Randomize_Start_Filter()
        {
            var tempoMap = TempoMap.Default;

            Randomize_Start(
                new[]
                {
                    ObjectMethods.Create(1000, 1000),
                    ObjectMethods.Create(0, 10000),
                    ObjectMethods.Create(20, 30)
                },
                new ConstantBounds((MidiTimeSpan)10000),
                new[]
                {
                    new TimeBounds((MidiTimeSpan)0, (MidiTimeSpan)11000),
                    new TimeBounds((MidiTimeSpan)0, (MidiTimeSpan)10000),
                    new TimeBounds((MidiTimeSpan)20, (MidiTimeSpan)20)
                },
                tempoMap,
                filter: o => o.Time != 20);
        }

        #endregion

        #region Private methods

        private void Randomize_Start(
            IEnumerable<TObject> actualObjects,
            IBounds bounds,
            IEnumerable<TimeBounds> expectedBounds,
            TempoMap tempoMap,
            Predicate<TObject> filter = null,
            bool checkUnchangedTimeObjectsPercent = true)
        {
            for (int i = 0; i < RepeatRandomizationCount; i++)
            {
                var clonedActualObjects = actualObjects.Select(o => o != null ? ObjectMethods.Clone(o) : default(TObject)).ToList();
                Randomize(
                    LengthedObjectTarget.Start,
                    clonedActualObjects,
                    bounds,
                    expectedBounds,
                    tempoMap,
                    filter,
                    checkUnchangedTimeObjectsPercent);
            }
        }

        private void Randomize(
            LengthedObjectTarget target,
            IEnumerable<TObject> actualObjects,
            IBounds bounds,
            IEnumerable<TimeBounds> expectedBounds,
            TempoMap tempoMap,
            Predicate<TObject> filter = null,
            bool checkUnchangedTimeObjectsPercent = true)
        {
            var objectsBounds = actualObjects.Zip(expectedBounds, (o, b) => new
            {
                Object = o,
                OldTime = o?.Time ?? 0,
                Bounds = b
            }).ToArray();

            Quantizer.Quantize(
                actualObjects,
                new ArbitraryGrid(),
                tempoMap,
                new TSettings
                {
                    QuantizingTarget = target,
                    Filter = filter,
                    RandomizingSettings = new RandomizingSettings
                    {
                        Bounds = bounds
                    }
                });

            foreach (var objectBounds in objectsBounds)
            {
                var time = objectBounds.Object?.Time;
                var timeBounds = objectBounds.Bounds;

                if (time == null)
                {
                    Assert.IsNull(timeBounds.MinTime, "Min time is not null for null object.");
                    Assert.IsNull(timeBounds.MaxTime, "Max time is not null for null object.");
                    continue;
                }

                var minTime = TimeConverter.ConvertFrom(timeBounds.MinTime, tempoMap);
                var maxTime = TimeConverter.ConvertFrom(timeBounds.MaxTime, tempoMap);

                Assert.IsTrue(
                    time >= minTime && time <= maxTime,
                    $"Object's time {time} is not in {timeBounds} [{minTime}; {maxTime}] range.");
            }

            if (checkUnchangedTimeObjectsPercent)
            {
                var allObjectsCount = objectsBounds.Count(b => b.Object != null);
                var unchangedTimeObjectsCount = objectsBounds.Count(b => b.Object != null && b.Object.Time == b.OldTime);
                Assert.Less(
                    unchangedTimeObjectsCount / (double)allObjectsCount,
                    1,
                    "Too high precent of objects with unchanged time.");
            }
        }

        #endregion
    }
}
