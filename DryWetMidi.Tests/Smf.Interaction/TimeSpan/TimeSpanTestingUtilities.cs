using Melanchall.DryWetMidi.Smf.Interaction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction
{
    internal static class TimeSpanTestingUtilities
    {
        #region Methods

        public static void TryParse(string input, ITimeSpan expectedTimeSpan)
        {
            TimeSpanUtilities.TryParse(input, out var actualTimeSpan);
            Assert.AreEqual(expectedTimeSpan, actualTimeSpan);
        }

        public static void ParseToString(ITimeSpan expectedTimeSpan)
        {
            Assert.AreEqual(expectedTimeSpan, TimeSpanUtilities.Parse(expectedTimeSpan.ToString()));
        }

        public static void Add_TimeTime(ITimeSpan timeSpan1, ITimeSpan timeSpan2)
        {
            Assert.ThrowsException<ArgumentException>(() => timeSpan1.Add(timeSpan2, MathOperationMode.TimeTime));
        }

        public static void Add_TimeLength(ITimeSpan timeSpan1, ITimeSpan timeSpan2)
        {
            Add(timeSpan1, timeSpan2, MathOperationMode.TimeLength);
        }

        public static void Add_LengthLength(ITimeSpan timeSpan1, ITimeSpan timeSpan2)
        {
            Add(timeSpan1, timeSpan2, MathOperationMode.LengthLength);
        }

        private static void Add(ITimeSpan timeSpan1, ITimeSpan timeSpan2, MathOperationMode operationMode)
        {
            var mathTimeSpan = timeSpan1.Add(timeSpan2, operationMode) as MathTimeSpan;

            Assert.IsTrue(mathTimeSpan != null &&
                          mathTimeSpan.TimeSpan1.Equals(timeSpan1) &&
                          mathTimeSpan.TimeSpan2.Equals(timeSpan2) &&
                          mathTimeSpan.Operation == MathOperation.Add &&
                          mathTimeSpan.OperationMode == operationMode,
                          "Result is not a math time span.");

            switch (operationMode)
            {
                case MathOperationMode.TimeLength:
                    Add_TimeLength_Tests(timeSpan1, timeSpan2, 100, TempoMap.Default);
                    break;

                case MathOperationMode.LengthLength:
                    // TODO
                    break;
            }
        }

        private static void Add_TimeLength_Tests(ITimeSpan timeSpan1, ITimeSpan timeSpan2, long time, TempoMap tempoMap)
        {
            var timeSpan = timeSpan1.Add(timeSpan2, MathOperationMode.TimeLength);

            //

            var timeSpanMunusLength = timeSpan.Subtract(timeSpan2, MathOperationMode.TimeLength);

            Assert.AreEqual(TimeConverter2.ConvertFrom(timeSpan1, tempoMap),
                            TimeConverter2.ConvertFrom(timeSpanMunusLength, tempoMap),
                            "(t + l) - l != t");

            Assert.AreEqual(timeSpan1,
                            TimeConverter2.ConvertTo(timeSpanMunusLength, timeSpan1.GetType(), tempoMap),
                            "[(t + l) - l]_t != t");

            //

            var timeSpanMunusTime = timeSpan.Subtract(timeSpan1, MathOperationMode.TimeTime);

            Assert.AreEqual(LengthConverter2.ConvertFrom(timeSpan2, time, tempoMap),
                            LengthConverter2.ConvertFrom(timeSpanMunusTime, time, tempoMap),
                            "(t + l) - t != l");

            Assert.AreEqual(timeSpan2,
                            LengthConverter2.ConvertTo(timeSpanMunusTime, timeSpan2.GetType(), time, tempoMap),
                            "[(t + l) - t]_l != l");
        }

        #endregion
    }
}
