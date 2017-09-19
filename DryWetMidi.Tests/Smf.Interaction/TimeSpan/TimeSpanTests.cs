using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction
{
    public abstract class TimeSpanTests
    {
        #region Nested classes

        protected sealed class TimeSpanParseInfo
        {
            public TimeSpanParseInfo(string input, ITimeSpan expectedTimeSpan)
            {
                Input = input;
                ExpectedTimeSpan = expectedTimeSpan;
            }

            public string Input { get; }

            public ITimeSpan ExpectedTimeSpan { get; }
        }

        protected abstract class TimeSpanOperationInfo
        {
            public TimeSpanOperationInfo(ITimeSpan expectedTimeSpan)
            {
                ExpectedTimeSpan = expectedTimeSpan;
            }

            public ITimeSpan ExpectedTimeSpan { get; }
        }

        protected sealed class TimeSpansOperationInfo : TimeSpanOperationInfo
        {
            public TimeSpansOperationInfo(ITimeSpan timeSpan1, ITimeSpan timeSpan2, ITimeSpan expectedTimeSpan)
                : base(expectedTimeSpan)
            {
                TimeSpan1 = timeSpan1;
                TimeSpan2 = timeSpan2;
            }

            public ITimeSpan TimeSpan1 { get; }

            public ITimeSpan TimeSpan2 { get; }
        }

        protected sealed class TimeSpanAndDoubleOperationInfo : TimeSpanOperationInfo
        {
            public TimeSpanAndDoubleOperationInfo(ITimeSpan timeSpan, double number, ITimeSpan expectedTimeSpan)
                : base(expectedTimeSpan)
            {
                TimeSpan = timeSpan;
                Number = number;
            }

            public ITimeSpan TimeSpan { get; }

            public double Number { get; }
        }

        #endregion

        #region Constants

        private static readonly IEnumerable<TempoMap> TempoMaps = new[]
        {
            GetDefaultTempoMap(),
            GetSimpleTempoMap(),
            GetComplexTempoMap()
        };

        #endregion

        #region Properties

        protected abstract IEnumerable<TimeSpanParseInfo> TimeSpansToParse { get; }

        protected abstract IEnumerable<TimeSpansOperationInfo> TimeSpansToAdd { get; }

        protected abstract IEnumerable<TimeSpansOperationInfo> TimeSpansToSubtract { get; }

        protected abstract IEnumerable<TimeSpanAndDoubleOperationInfo> TimeSpansToMultiply { get; }

        protected abstract IEnumerable<TimeSpanAndDoubleOperationInfo> TimeSpansToDivide { get; }

        #endregion

        #region Test methods

        [TestMethod]
        [Description("Test TryParse method.")]
        public void TryParse()
        {
            foreach (var timeSpan in TimeSpansToParse)
            {
                TimeSpanUtilities.TryParse(timeSpan.Input, out var actualTimeSpan);
                Assert.AreEqual(timeSpan.ExpectedTimeSpan,
                                actualTimeSpan,
                                $"TryParse gave invalid result for '{timeSpan.Input}'.");
            }
        }

        [TestMethod]
        [Description("Test Parse method.")]
        public void Parse()
        {
            foreach (var timeSpan in TimeSpansToParse)
            {
                Assert.AreEqual(timeSpan.ExpectedTimeSpan,
                                TimeSpanUtilities.Parse(timeSpan.Input),
                                $"Parse gave invalid result for '{timeSpan.Input}'.");
            }
        }

        [TestMethod]
        [Description("Test Parse method passing string representation of an expected time span obtained via ToString.")]
        public void ParseToString()
        {
            foreach (var timeSpan in TimeSpansToParse)
            {
                Assert.AreEqual(timeSpan.ExpectedTimeSpan,
                                TimeSpanUtilities.Parse(timeSpan.ExpectedTimeSpan.ToString()),
                                $"Parse gave invalid result for string representation of {timeSpan.ExpectedTimeSpan}.");
            }
        }

        #endregion

        #region Private methods

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
