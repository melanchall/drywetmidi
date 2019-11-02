using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    internal static class TimeSpanTestUtilities
    {
        #region Nested classes

        private sealed class MetricTimeSpanEqualityComparer : IEqualityComparer<ITimeSpan>
        {
            #region Fields

            private readonly long _tolerance;

            #endregion

            #region Constructor

            public MetricTimeSpanEqualityComparer(long toleranceInMicroseconds)
            {
                _tolerance = toleranceInMicroseconds;
            }

            #endregion

            #region IEqualityComparer<ITimeSpan>

            public bool Equals(ITimeSpan x, ITimeSpan y)
            {
                var metricX = (MetricTimeSpan)x;
                var metricY = (MetricTimeSpan)y;

                return Math.Abs(metricX.TotalMicroseconds - metricY.TotalMicroseconds) <= _tolerance;
            }

            public int GetHashCode(ITimeSpan obj)
            {
                return obj.GetHashCode();
            }

            #endregion
        }

        private sealed class MidiTimeSpanEqualityComparer : IEqualityComparer<ITimeSpan>
        {
            #region Fields

            private readonly long _tolerance;

            #endregion

            #region Constructor

            public MidiTimeSpanEqualityComparer(long toleranceInTicks)
            {
                _tolerance = toleranceInTicks;
            }

            #endregion

            #region IEqualityComparer<ITimeSpan>

            public bool Equals(ITimeSpan x, ITimeSpan y)
            {
                var midiX = (MidiTimeSpan)x;
                var midiY = (MidiTimeSpan)y;

                return Math.Abs(midiX.TimeSpan - midiY.TimeSpan) <= _tolerance;
            }

            public int GetHashCode(ITimeSpan obj)
            {
                return obj.GetHashCode();
            }

            #endregion
        }

        private sealed class BarBeatFractionTimeSpanEqualityComparer : IEqualityComparer<ITimeSpan>
        {
            #region Fields

            private readonly double _fractionalBeatsTolerance;

            #endregion

            #region Constructor

            public BarBeatFractionTimeSpanEqualityComparer(double fractionalBeatsTolerance)
            {
                _fractionalBeatsTolerance = fractionalBeatsTolerance;
            }

            #endregion

            #region IEqualityComparer<ITimeSpan>

            public bool Equals(ITimeSpan x, ITimeSpan y)
            {
                var xTimeSpan = (BarBeatFractionTimeSpan)x;
                var yTimeSpan = (BarBeatFractionTimeSpan)y;

                return xTimeSpan.Bars == yTimeSpan.Bars &&
                       Math.Abs(xTimeSpan.Beats - yTimeSpan.Beats) <= _fractionalBeatsTolerance;
            }

            public int GetHashCode(ITimeSpan obj)
            {
                return obj.GetHashCode();
            }

            #endregion
        }

        #endregion

        #region Constants

        public const short TicksPerQuarterNote = 480;

        public static readonly TempoMap DefaultTempoMap = GenerateDefaultTempoMap();
        public static readonly TempoMap SimpleTempoMap = GenerateSimpleTempoMap();
        public static readonly TempoMap ComplexTempoMap = GenerateComplexTempoMap();

        // TODO: find a way to decrease this constant
        private const long MetricTimeSpanEqualityTolerance = 500; // μs
        private const long MidiTimeSpanEqualityTolerance = 1; // ticks
        private const double BarBeatFractionTimeSpanEqualityTolerance = 0.001;

        private static readonly Dictionary<Type, IEqualityComparer<ITimeSpan>> TimeSpanComparers =
            new Dictionary<Type, IEqualityComparer<ITimeSpan>>
            {
                [typeof(MetricTimeSpan)] = new MetricTimeSpanEqualityComparer(MetricTimeSpanEqualityTolerance),
                [typeof(MidiTimeSpan)] = new MidiTimeSpanEqualityComparer(MidiTimeSpanEqualityTolerance),
                [typeof(BarBeatFractionTimeSpan)] = new BarBeatFractionTimeSpanEqualityComparer(BarBeatFractionTimeSpanEqualityTolerance)
            };

        #endregion

        #region Methods

        public static void TestClone(ITimeSpan timeSpan)
        {
            var clone = timeSpan.Clone();

            Assert.AreEqual(timeSpan, clone, "Clone time span doesn't equal to the original one.");
            Assert.IsFalse(ReferenceEquals(timeSpan, clone), "Clone time span is the same instance as the original one.");
        }

        public static void TestConversion<TTimeSpan>(TTimeSpan timeSpan, ITimeSpan referenceTimeSpan, ITimeSpan time, TempoMap tempoMap)
            where TTimeSpan : ITimeSpan
        {
            time = time ?? new MidiTimeSpan();

            var ticks = LengthConverter.ConvertFrom(timeSpan, time, tempoMap);
            AreEqual(timeSpan,
                     LengthConverter.ConvertTo<TTimeSpan>(ticks, time, tempoMap),
                     "Cyclic conversion failed.");

            AreEqual(timeSpan,
                     LengthConverter.ConvertTo<TTimeSpan>(referenceTimeSpan, time, tempoMap),
                     "ConvertTo failed.");

            Assert.AreEqual(LengthConverter.ConvertFrom(referenceTimeSpan, time, tempoMap),
                            ticks,
                            "ConvertFrom failed.");
        }

        public static void Parse(string input, ITimeSpan expectedTimeSpan)
        {
            TimeSpanUtilities.TryParse(input, out var actualTimeSpan);
            Assert.AreEqual(expectedTimeSpan,
                            actualTimeSpan,
                            $"TryParse: incorrect result for '{input}'.");

            actualTimeSpan = TimeSpanUtilities.Parse(input);
            Assert.AreEqual(expectedTimeSpan,
                            actualTimeSpan,
                            $"Parse: incorrect result for '{input}'.");

            Assert.AreEqual(expectedTimeSpan,
                            TimeSpanUtilities.Parse(expectedTimeSpan.ToString()),
                            $"Parse: string representation was not parsed to the original time span for '{input}'.");
        }

        public static void ParseInvalidInput(string input)
        {
            Assert.Throws<FormatException>(() => TimeSpanUtilities.Parse(input));
        }

        public static void Add_SameType<TTimeSpan>(TTimeSpan timeSpan1, TTimeSpan timeSpan2, TTimeSpan expectedTimeSpan)
            where TTimeSpan : ITimeSpan
        {
            foreach (TimeSpanMode mode in Enum.GetValues(typeof(TimeSpanMode)))
            {
                AreEqual(expectedTimeSpan,
                         timeSpan1.Add(timeSpan2, mode),
                         $"{timeSpan1} + {timeSpan2} != {expectedTimeSpan} for {mode} mode.");
            }
        }

        public static void Add_TimeTime(ITimeSpan timeSpan1, ITimeSpan timeSpan2)
        {
            Assert.Throws<ArgumentException>(() => timeSpan1.Add(timeSpan2, TimeSpanMode.TimeTime));
        }

        public static void Add_TimeLength(ITimeSpan timeSpan1, ITimeSpan timeSpan2, TempoMap tempoMap)
        {
            var mathTimeSpan = CheckMathTimeSpan(timeSpan1, timeSpan2, MathOperation.Add, TimeSpanMode.TimeLength);

            AreEqual(timeSpan1,
                     TimeConverter.ConvertTo(mathTimeSpan.Subtract(timeSpan2, TimeSpanMode.TimeLength),
                                              timeSpan1.GetType(),
                                              tempoMap),
                     $"({timeSpan1} + {timeSpan2}) - {timeSpan2} != {timeSpan1}.");
        }

        public static void Add_LengthLength(ITimeSpan timeSpan1, ITimeSpan timeSpan2, TempoMap tempoMap, long time)
        {
            var mathTimeSpan = CheckMathTimeSpan(timeSpan1, timeSpan2, MathOperation.Add, TimeSpanMode.LengthLength);

            AreEqual(timeSpan1,
                     LengthConverter.ConvertTo(mathTimeSpan.Subtract(timeSpan2, TimeSpanMode.LengthLength),
                                                timeSpan1.GetType(),
                                                time,
                                                tempoMap),
                     $"({timeSpan1} + {timeSpan2}) - {timeSpan2} != {timeSpan1} at time of {time}.");
        }

        public static void Subtract_SameType<TTimeSpan>(TTimeSpan timeSpan1, TTimeSpan timeSpan2, TTimeSpan expectedTimeSpan)
            where TTimeSpan : ITimeSpan
        {
            foreach (TimeSpanMode mode in Enum.GetValues(typeof(TimeSpanMode)))
            {
                AreEqual(expectedTimeSpan,
                         timeSpan1.Subtract(timeSpan2, mode),
                         $"{timeSpan1} - {timeSpan2} != {expectedTimeSpan} for {mode} mode.");
            }
        }

        public static void Subtract_TimeTime(ITimeSpan timeSpan1, ITimeSpan timeSpan2, TempoMap tempoMap)
        {
            var mathTimeSpan = CheckMathTimeSpan(timeSpan1, timeSpan2, MathOperation.Subtract, TimeSpanMode.TimeTime);

            AreEqual(timeSpan1,
                     TimeConverter.ConvertTo(timeSpan2.Add(mathTimeSpan, TimeSpanMode.TimeLength),
                                              timeSpan1.GetType(),
                                              tempoMap),
                     $"{timeSpan2} + ({timeSpan1} - {timeSpan2}) != {timeSpan1}.");
        }

        public static void Subtract_TimeLength(ITimeSpan timeSpan1, ITimeSpan timeSpan2, TempoMap tempoMap)
        {
            var mathTimeSpan = CheckMathTimeSpan(timeSpan1, timeSpan2, MathOperation.Subtract, TimeSpanMode.TimeLength);

            AreEqual(timeSpan1,
                     TimeConverter.ConvertTo(mathTimeSpan.Add(timeSpan2, TimeSpanMode.TimeLength),
                                              timeSpan1.GetType(),
                                              tempoMap),
                     $"({timeSpan1} - {timeSpan2}) + {timeSpan2} != {timeSpan1}.");
        }

        public static void Subtract_LengthLength(ITimeSpan timeSpan1, ITimeSpan timeSpan2, TempoMap tempoMap, long time)
        {
            var mathTimeSpan = CheckMathTimeSpan(timeSpan1, timeSpan2, MathOperation.Subtract, TimeSpanMode.LengthLength);

            AreEqual(timeSpan1,
                     LengthConverter.ConvertTo(mathTimeSpan.Add(timeSpan2, TimeSpanMode.LengthLength),
                                                timeSpan1.GetType(),
                                                time,
                                                tempoMap),
                     $"({timeSpan1} - {timeSpan2}) + {timeSpan2} != {timeSpan1}.");
        }

        public static void AreEqual(ITimeSpan expectedTimeSpan, ITimeSpan actualTimeSpan, string message)
        {
            var expectedTimeSpanType = expectedTimeSpan.GetType();
            var actualTimeSpanType = actualTimeSpan.GetType();

            //

            Assert.AreEqual(expectedTimeSpanType,
                            actualTimeSpanType,
                            $"Type of {expectedTimeSpan} isn't equal to the type of {actualTimeSpan}.");

            //

            if (!TimeSpanComparers.TryGetValue(expectedTimeSpanType, out var comparer))
                comparer = EqualityComparer<ITimeSpan>.Default;

            Assert.IsTrue(comparer.Equals(expectedTimeSpan, actualTimeSpan),
                          $"Time spans are not equal. Expected: {expectedTimeSpan}. Actual: {actualTimeSpan}. {message}");
        }

        private static MathTimeSpan CheckMathTimeSpan(ITimeSpan timeSpan1, ITimeSpan timeSpan2, MathOperation operation, TimeSpanMode mode)
        {
            var mathTimeSpan = (operation == MathOperation.Add
                ? timeSpan1.Add(timeSpan2, mode)
                : timeSpan1.Subtract(timeSpan2, mode)) as MathTimeSpan;

            Assert.IsTrue(mathTimeSpan != null &&
                          mathTimeSpan.TimeSpan1.Equals(timeSpan1) &&
                          mathTimeSpan.TimeSpan2.Equals(timeSpan2) &&
                          mathTimeSpan.Operation == operation &&
                          mathTimeSpan.Mode == mode,
                          "Result is not a math time span.");

            return mathTimeSpan;
        }

        private static TempoMap GenerateDefaultTempoMap()
        {
            // 4/4
            //  |----+----+----+----|----+----+----+----|----+----+----+----|
            //  0                   1                   2                   3

            using (var tempoMapManager = new TempoMapManager(new TicksPerQuarterNoteTimeDivision(TicksPerQuarterNote)))
            {
                return tempoMapManager.TempoMap;
            }
        }

        private static TempoMap GenerateSimpleTempoMap()
        {
            // 4/4                 5/8            5/16
            //  |----+----+----+----|--+--+--+--+--|-+-+-+-+-|
            //  0                   1              2         3

            using (var tempoMapManager = new TempoMapManager(new TicksPerQuarterNoteTimeDivision(TicksPerQuarterNote)))
            {
                tempoMapManager.SetTimeSignature(MusicalTimeSpan.Whole, new TimeSignature(5, 8));
                tempoMapManager.SetTimeSignature(MusicalTimeSpan.Whole + 5 * MusicalTimeSpan.Eighth, new TimeSignature(5, 16));

                tempoMapManager.SetTempo(new MetricTimeSpan(0, 0, 10), Tempo.FromMillisecondsPerQuarterNote(300));
                tempoMapManager.SetTempo(new MetricTimeSpan(0, 1, 30), Tempo.FromMillisecondsPerQuarterNote(600));

                return tempoMapManager.TempoMap;
            }
        }

        private static TempoMap GenerateComplexTempoMap()
        {
            // 4/4                                     5/8            5/16                          5/8
            //  |----+----+----+----|----+----+----+----|--+--+--+--+--|-+-+-+-+-|-+-+-+-+-|-+-+-+-+-|--+--+--+--+--|
            //  0                   1                   2              3         4         5         6              7

            var steps = new[]
            {
                Tuple.Create(2 * MusicalTimeSpan.Whole, new TimeSignature(5, 8)),
                Tuple.Create(5 * MusicalTimeSpan.Eighth, new TimeSignature(5, 16)),
                Tuple.Create(15 * MusicalTimeSpan.Sixteenth, new TimeSignature(5, 8)),
            };

            using (var tempoMapManager = new TempoMapManager(new TicksPerQuarterNoteTimeDivision(TicksPerQuarterNote)))
            {
                var time = new MusicalTimeSpan();

                foreach (var step in steps)
                {
                    time += step.Item1;
                    tempoMapManager.SetTimeSignature(time, step.Item2);
                }

                tempoMapManager.SetTempo(new MetricTimeSpan(0, 0, 10), Tempo.FromMillisecondsPerQuarterNote(300));
                tempoMapManager.SetTempo(new MetricTimeSpan(0, 1, 30), Tempo.FromMillisecondsPerQuarterNote(600));
                tempoMapManager.SetTempo(new MetricTimeSpan(0, 1, 31), Tempo.FromMillisecondsPerQuarterNote(640));

                return tempoMapManager.TempoMap;
            }
        }

        #endregion
    }
}
