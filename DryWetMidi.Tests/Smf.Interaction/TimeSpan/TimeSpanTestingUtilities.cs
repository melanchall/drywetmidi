using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction
{
    internal static class TimeSpanTestingUtilities
    {
        #region Constants

        private static readonly IEnumerable<TempoMap> TempoMaps = new[]
        {
            GetDefaultTempoMap(),
            GetSimpleTempoMap(),
            GetComplexTempoMap()
        };

        #endregion

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

            foreach (var tempoMap in TempoMaps)
            {
                for (var i = 0; i < 20; i++)
                {
                    switch (operationMode)
                    {
                        case MathOperationMode.TimeLength:
                            Assert.AreEqual(timeSpan1,
                                            TimeConverter2.ConvertTo(mathTimeSpan.Subtract(timeSpan2, MathOperationMode.TimeLength), timeSpan1.GetType(), tempoMap),
                                            $"(t + l) - l != t");
                            break;

                        case MathOperationMode.LengthLength:
                            var time = i * 1000;
                            Assert.AreEqual(timeSpan1,
                                            LengthConverter2.ConvertTo(mathTimeSpan.Subtract(timeSpan2, MathOperationMode.LengthLength), timeSpan1.GetType(), time, tempoMap),
                                            $"(l1 + l2) - l2 != l1 | time = {time}");
                            break;
                    }
                }
            }
        }

        private static TempoMap GetDefaultTempoMap()
        {
            return TempoMap.Default;
        }

        private static TempoMap GetSimpleTempoMap()
        {
            return TempoMap.Create(Tempo.FromBeatsPerMinute(200), new TimeSignature(5, 8));
        }

        private static TempoMap GetComplexTempoMap()
        {
            const int tempoChangesCount = 10;
            const int timeSignatureChangesCount = 10;

            using (var tempoMapManager = new TempoMapManager())
            {
                var ticksPerQuarterNote = ((TicksPerQuarterNoteTimeDivision)tempoMapManager.TempoMap.TimeDivision).TicksPerQuarterNote;

                for (var i = 0; i < tempoChangesCount; i++)
                {
                    tempoMapManager.SetTempo(i * 2000 + 100, new Tempo((i + 1) * ticksPerQuarterNote * 1000));
                }

                for (var i = 0; i < timeSignatureChangesCount; i++)
                {
                    tempoMapManager.SetTimeSignature(i * 2500 + 50, new TimeSignature(i + 1, 8));
                }

                return tempoMapManager.TempoMap;
            }
        }

        #endregion
    }
}
