using Melanchall.DryWetMidi.Smf.Interaction;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction
{
    internal static class TimeSpanTestUtilities
    {
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

        public static void Add_SameType<TTimeSpan>(TTimeSpan timeSpan1, TTimeSpan timeSpan2, TTimeSpan expectedTimeSpan, TimeSpanMode operationMode)
            where TTimeSpan : ITimeSpan
        {
            Assert.AreEqual(expectedTimeSpan,
                            timeSpan1.Add(timeSpan2, operationMode),
                            $"{timeSpan1} + {timeSpan2} != {expectedTimeSpan}");
        }

        public static void Add_TimeLength(ITimeSpan timeSpan1, ITimeSpan timeSpan2, TempoMap tempoMap)
        {
            var mathTimeSpan = CheckMathTimeSpan(timeSpan1, timeSpan2, MathOperation.Add, TimeSpanMode.TimeLength);

            Assert.AreEqual(timeSpan1,
                            TimeConverter2.ConvertTo(mathTimeSpan.Subtract(timeSpan2, TimeSpanMode.TimeLength),
                                                     timeSpan1.GetType(),
                                                     tempoMap),
                            $"({timeSpan1} + {timeSpan2}) - {timeSpan2} != {timeSpan1}");
        }

        public static void Add_LengthLength(ITimeSpan timeSpan1, ITimeSpan timeSpan2, TempoMap tempoMap, long time)
        {
            var mathTimeSpan = CheckMathTimeSpan(timeSpan1, timeSpan2, MathOperation.Add, TimeSpanMode.LengthLength);

            Assert.AreEqual(timeSpan1,
                            LengthConverter2.ConvertTo(mathTimeSpan.Subtract(timeSpan2, TimeSpanMode.LengthLength),
                                                       timeSpan1.GetType(),
                                                       time,
                                                       tempoMap),
                            $"({timeSpan1} + {timeSpan2}) - {timeSpan2} != {timeSpan1} at time of {time}");
        }

        public static void Subtract_SameType<TTimeSpan>(TTimeSpan timeSpan1, TTimeSpan timeSpan2, TTimeSpan expectedTimeSpan, TimeSpanMode operationMode)
            where TTimeSpan : ITimeSpan
        {
            Assert.AreEqual(expectedTimeSpan,
                            timeSpan1.Subtract(timeSpan2, operationMode),
                            $"{timeSpan1} - {timeSpan2} != {expectedTimeSpan}");
        }

        private static MathTimeSpan CheckMathTimeSpan(ITimeSpan timeSpan1, ITimeSpan timeSpan2, MathOperation operation, TimeSpanMode mode)
        {
            var mathTimeSpan = timeSpan1.Add(timeSpan2, mode) as MathTimeSpan;

            Assert.IsTrue(mathTimeSpan != null &&
                          mathTimeSpan.TimeSpan1.Equals(timeSpan1) &&
                          mathTimeSpan.TimeSpan2.Equals(timeSpan2) &&
                          mathTimeSpan.Operation == operation &&
                          mathTimeSpan.Mode == mode,
                          "Result is not a math time span.");

            return mathTimeSpan;
        }
    }
}
