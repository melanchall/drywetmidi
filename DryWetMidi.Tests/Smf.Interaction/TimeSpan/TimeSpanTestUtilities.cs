using Melanchall.DryWetMidi.Smf.Interaction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction
{
    internal static class TimeSpanTestUtilities
    {
        #region Nested classes

        private sealed class MetricTimeSpanEqualityComparer : IEqualityComparer<ITimeSpan>
        {
            #region Fields

            private readonly long _toleranceInMicroseconds;

            #endregion

            #region Constructor

            public MetricTimeSpanEqualityComparer(long toleranceInMicroseconds)
            {
                _toleranceInMicroseconds = toleranceInMicroseconds;
            }

            #endregion

            #region IEqualityComparer<ITimeSpan>

            public bool Equals(ITimeSpan x, ITimeSpan y)
            {
                var metricTimeSpan1 = (MetricTimeSpan)x;
                var metricTimeSpan2 = (MetricTimeSpan)y;

                return Math.Abs(metricTimeSpan1.TotalMicroseconds - metricTimeSpan2.TotalMicroseconds) < _toleranceInMicroseconds;
            }

            public int GetHashCode(ITimeSpan obj)
            {
                return obj.GetHashCode();
            }

            #endregion
        }

        #endregion

        #region Constants

        private const long MetricTimeSpanEqualityTolerance = 500; // μs

        private static readonly Dictionary<Type, IEqualityComparer<ITimeSpan>> TimeSpanComparers =
            new Dictionary<Type, IEqualityComparer<ITimeSpan>>
            {
                [typeof(MetricTimeSpan)] = new MetricTimeSpanEqualityComparer(MetricTimeSpanEqualityTolerance)
            };

        #endregion

        #region Methods

        public static void Parse(string input, ITimeSpan expectedTimeSpan)
        {
            TimeSpanUtilities.TryParse(input, out var actualTimeSpan);
            Assert.AreEqual(expectedTimeSpan,
                            actualTimeSpan,
                            "TryParse: incorrect result.");

            actualTimeSpan = TimeSpanUtilities.Parse(input);
            Assert.AreEqual(expectedTimeSpan,
                            actualTimeSpan,
                            "Parse: incorrect result.");

            Assert.AreEqual(expectedTimeSpan,
                            TimeSpanUtilities.Parse(expectedTimeSpan.ToString()),
                            "Parse: string representation was not parsed to the original time span.");
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
            Assert.ThrowsException<ArgumentException>(() => timeSpan1.Add(timeSpan2, TimeSpanMode.TimeTime));
        }

        public static void Add_TimeLength(ITimeSpan timeSpan1, ITimeSpan timeSpan2, TempoMap tempoMap)
        {
            var mathTimeSpan = CheckMathTimeSpan(timeSpan1, timeSpan2, MathOperation.Add, TimeSpanMode.TimeLength);

            AreEqual(timeSpan1,
                     TimeConverter2.ConvertTo(mathTimeSpan.Subtract(timeSpan2, TimeSpanMode.TimeLength),
                                              timeSpan1.GetType(),
                                              tempoMap),
                     $"({timeSpan1} + {timeSpan2}) - {timeSpan2} != {timeSpan1}.");
        }

        public static void Add_LengthLength(ITimeSpan timeSpan1, ITimeSpan timeSpan2, TempoMap tempoMap, long time)
        {
            var mathTimeSpan = CheckMathTimeSpan(timeSpan1, timeSpan2, MathOperation.Add, TimeSpanMode.LengthLength);

            AreEqual(timeSpan1,
                     LengthConverter2.ConvertTo(mathTimeSpan.Subtract(timeSpan2, TimeSpanMode.LengthLength),
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
                     TimeConverter2.ConvertTo(timeSpan2.Add(mathTimeSpan, TimeSpanMode.TimeLength),
                                              timeSpan1.GetType(),
                                              tempoMap),
                     $"{timeSpan2} + ({timeSpan1} - {timeSpan2}) != {timeSpan1}.");
        }

        public static void Subtract_TimeLength(ITimeSpan timeSpan1, ITimeSpan timeSpan2, TempoMap tempoMap)
        {
            var mathTimeSpan = CheckMathTimeSpan(timeSpan1, timeSpan2, MathOperation.Subtract, TimeSpanMode.TimeLength);

            AreEqual(timeSpan1,
                     TimeConverter2.ConvertTo(mathTimeSpan.Add(timeSpan2, TimeSpanMode.TimeLength),
                                              timeSpan1.GetType(),
                                              tempoMap),
                     $"({timeSpan1} - {timeSpan2}) + {timeSpan2} != {timeSpan1}.");
        }

        public static void Subtract_LengthLength(ITimeSpan timeSpan1, ITimeSpan timeSpan2, TempoMap tempoMap, long time)
        {
            var mathTimeSpan = CheckMathTimeSpan(timeSpan1, timeSpan2, MathOperation.Subtract, TimeSpanMode.LengthLength);

            AreEqual(timeSpan1,
                     LengthConverter2.ConvertTo(mathTimeSpan.Add(timeSpan2, TimeSpanMode.LengthLength),
                                                timeSpan1.GetType(),
                                                time,
                                                tempoMap),
                     $"({timeSpan1} - {timeSpan2}) + {timeSpan2} != {timeSpan1}.");
        }

        private static void AreEqual(ITimeSpan expectedTimeSpan, ITimeSpan actualTimeSpan, string message)
        {
            var expectedTimeSpanType = expectedTimeSpan.GetType();
            var actualTimeSpanType = actualTimeSpan.GetType();

            //

            Assert.IsTrue(expectedTimeSpanType == actualTimeSpanType,
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

        #endregion
    }
}
