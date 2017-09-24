using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

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
            public TimeSpansOperationInfo(ITimeSpan timeSpan1, ITimeSpan timeSpan2, ITimeSpan expectedTimeSpan = null)
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

        //protected abstract IEnumerable<TimeSpansOperationInfo> TimeSpansToSubtract { get; }

        //protected abstract IEnumerable<TimeSpanAndDoubleOperationInfo> TimeSpansToMultiply { get; }

        //protected abstract IEnumerable<TimeSpanAndDoubleOperationInfo> TimeSpansToDivide { get; }

        #endregion

        #region Test methods

        [TestMethod]
        public void Parse()
        {
            foreach (var timeSpan in TimeSpansToParse)
            {
                var input = timeSpan.Input;
                var expectedTimeSpan = timeSpan.ExpectedTimeSpan;

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
        }

        [TestMethod]
        public void Add_TimeTime()
        {
            foreach (var timeSpan in TimeSpansToAdd.Where(ts => ts.ExpectedTimeSpan == null))
            {
                Assert.ThrowsException<ArgumentException>(() => timeSpan.TimeSpan1.Add(timeSpan.TimeSpan2, MathOperationMode.TimeTime));
            }
        }

        [TestMethod]
        public void Add_TimeLength()
        {
            foreach (var timeSpan in TimeSpansToAdd)
            {
                Add(timeSpan.TimeSpan1, timeSpan.TimeSpan2, timeSpan.ExpectedTimeSpan, MathOperationMode.TimeLength);
            }
        }

        [TestMethod]
        public void Add_LengthLength()
        {
            foreach (var timeSpan in TimeSpansToAdd)
            {
                Add(timeSpan.TimeSpan1, timeSpan.TimeSpan2, timeSpan.ExpectedTimeSpan, MathOperationMode.LengthLength);
            }
        }

        #endregion

        #region Private methods

        private static void Add(ITimeSpan timeSpan1, ITimeSpan timeSpan2, ITimeSpan expectedTimeSpan, MathOperationMode operationMode)
        {
            if (expectedTimeSpan != null)
            {
                Assert.AreEqual(expectedTimeSpan, timeSpan1.Add(timeSpan2, operationMode));
                return;
            }

            //

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
            const int tempoChangesCount = 2;
            const int timeSignatureChangesCount = 10;

            using (var tempoMapManager = new TempoMapManager())
            {
                var ticksPerQuarterNote = ((TicksPerQuarterNoteTimeDivision)tempoMapManager.TempoMap.TimeDivision).TicksPerQuarterNote;

                for (var i = 0; i < tempoChangesCount; i++)
                {
                    tempoMapManager.SetTempo(i * 2000 + 100, new Tempo((i + 1) * ticksPerQuarterNote * 1000));
                }

                //for (var i = 0; i < timeSignatureChangesCount; i++)
                //{
                //    tempoMapManager.SetTimeSignature(i * 2500 + 50, new TimeSignature(i + 1, 8));
                //}

                return tempoMapManager.TempoMap;
            }
        }

        #endregion
    }
}
