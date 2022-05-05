using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    // TODO: math time span
    [TestFixture]
    public sealed partial class RepeaterTests
    {
        #region Constants

        private static readonly int[] RepeatsNumbers = { 1, 10 };

        private static readonly object[] RoundUpStepData_ShiftByMaxTime =
            GetRoundUpStepData_ShiftByMaxTime(RepeatsNumbers, new Dictionary<string, (string EventTime, string ExpectedShift)[]>
            {
                // Midi

                ["5"] = new[]
                {
                    ("3", "5"),
                    ("5", "5"),
                    ("7", "10"),
                    ("10", "10")
                },
                ["0"] = new[]
                {
                    ("3", "3"),
                    ("5", "5"),
                    ("7", "7"),
                    ("10", "10")
                },

                // Musical

                ["5/8"] = new[]
                {
                    ("3/8", "5/8"),
                    ("5/8", "5/8"),
                    ("7/8", "10/8"),
                    ("10/8", "10/8")
                },

                // Metric

                ["1s 500ms"] = new[]
                {
                    ("1s", "1s 500ms"),
                    ("1s 500ms", "1s 500ms"),
                    ("2s", "3s"),
                    ("3s", "3s")
                },
                ["123ms"] = new[]
                {
                    ("122ms", "123ms"),
                    ("121ms", "123ms"),
                    ("120ms", "123ms"),
                },
                ["0:0:0"] = new[]
                {
                    ("1s", "1s"),
                    ("1s 500ms", "1s 500ms"),
                    ("2s", "2s"),
                    ("3s", "3s")
                },

                // BarBeatTicks

                ["0.0.8"] = new[]
                {
                    ("0.0.6", "0.0.8"),
                    ("0.0.8", "0.0.8"),
                    ("0.0.10", "0.0.16"),
                    ("0.0.16", "0.0.16")
                },
                ["0.8.0"] = new[]
                {
                    ("0.6.0", "0.8.0"),
                    ("0.8.0", "0.8.0"),
                    ("0.10.0", "0.16.0"),
                    ("0.16.0", "0.16.0")
                },
                ["8.0.0"] = new[]
                {
                    ("6.0.0", "8.0.0"),
                    ("8.0.0", "8.0.0"),
                    ("10.0.0", "16.0.0"),
                    ("16.0.0", "16.0.0")
                },
                ["1.0.0"] = new[]
                {
                    ("0.2.6", "1.0.0")
                },

                // BarBeatFraction

                ["0_0.5"] = new[]
                {
                    ("0_0.3", "0_0.5"),
                    ("0_0.5", "0_0.5"),
                    ("0_0.7", "0_1.0"),
                    ("0_1.0", "0_1.0")
                },
                ["5_0"] = new[]
                {
                    ("3_0", "5_0"),
                    ("5_0", "5_0"),
                    ("7_0", "10_0"),
                    ("10_0", "10_0")
                },
                ["0_0.05"] = new[]
                {
                    ("0_0.03", "0_0.05"),
                    ("0_0.05", "0_0.05"),
                    ("0_0.07", "0_0.10"),
                    ("0_0.10", "0_0.10")
                },
                ["0_0.0"] = new[]
                {
                    ("0_0.03", "0_0.03"),
                    ("0_0.05", "0_0.05"),
                    ("0_0.07", "0_0.07"),
                    ("0_0.10", "0_0.10")
                }
            });

        private static readonly object[] RoundUpStepData_ShiftByFixedValue =
            GetRoundUpStepData_ShiftByFixedValue(RepeatsNumbers, new Dictionary<(string Shift, string ShiftStep), (string EventTime, string ExpectedShift)[]>
            {
                // Midi

                [("3", "5")] = new[]
                {
                    ("3", "5"),
                    ("5", "5")
                },
                [("7", "10")] = new[]
                {
                    ("7", "10"),
                    ("10", "10")
                }
            });

        #endregion

        #region Test methods

        [Test]
        public void CheckRepeat_SingleCollection_EmptyCollection([Values(1, 10)] int repeatsNumber) => CheckRepeat(
            inputObjects: Array.Empty<ITimedObject>(),
            repeatsNumber: repeatsNumber,
            settings: null,
            expectedObjects: Array.Empty<ITimedObject>());

        [Test]
        public void CheckRepeat_SingleCollection_SingleTimedEvent_DefaultSettings([Values(0, 10)] long eventTime, [Values(1, 10)] int repeatsNumber) => CheckRepeat(
            inputObjects: new[]
            {
                new TimedEvent(new TextEvent("A"), eventTime),
            },
            repeatsNumber: repeatsNumber,
            settings: null,
            expectedObjects: Enumerable
                .Range(0, repeatsNumber + 1)
                .Select(i => new TimedEvent(new TextEvent("A"), eventTime + eventTime * i))
                .ToArray());

        [Test]
        public void CheckRepeat_SingleCollection_SingleTimedEvent_NoShift([Values(0, 10)] long eventTime, [Values(1, 10)] int repeatsNumber) => CheckRepeat(
            inputObjects: new[]
            {
                new TimedEvent(new TextEvent("A"), eventTime),
            },
            repeatsNumber: repeatsNumber,
            settings: new RepeatingSettings
            {
                ShiftPolicy = ShiftPolicy.None
            },
            expectedObjects: Enumerable
                .Range(0, repeatsNumber + 1)
                .Select(_ => new TimedEvent(new TextEvent("A"), eventTime))
                .ToArray());

        [Test]
        public void CheckRepeat_SingleCollection_SingleTimedEvent_ShiftByMaxTime([Values(0, 10)] long eventTime, [Values(1, 10)] int repeatsNumber) => CheckRepeat(
            inputObjects: new[]
            {
                new TimedEvent(new TextEvent("A"), eventTime),
            },
            repeatsNumber: repeatsNumber,
            settings: new RepeatingSettings
            {
                ShiftPolicy = ShiftPolicy.ShiftByMaxTime
            },
            expectedObjects: Enumerable
                .Range(0, repeatsNumber + 1)
                .Select(i => new TimedEvent(new TextEvent("A"), eventTime + eventTime * i))
                .ToArray());

        [Test]
        public void CheckRepeat_SingleCollection_SingleTimedEvent_ShiftByFixedValue([Values(0, 10)] long eventTime, [Values(1, 10)] int repeatsNumber, [Values(0, 10)] long shift) => CheckRepeat(
            inputObjects: new[]
            {
                new TimedEvent(new TextEvent("A"), eventTime),
            },
            repeatsNumber: repeatsNumber,
            settings: new RepeatingSettings
            {
                ShiftPolicy = ShiftPolicy.ShiftByFixedValue,
                Shift = (MidiTimeSpan)shift
            },
            expectedObjects: Enumerable
                .Range(0, repeatsNumber + 1)
                .Select(i => new TimedEvent(new TextEvent("A"), eventTime + shift * i))
                .ToArray());

        [TestCaseSource(nameof(RoundUpStepData_ShiftByMaxTime))]
        public void CheckRepeat_SingleCollection_SingleTimedEvent_ShiftByMaxTime_Step(
            string eventTime,
            int repeatsNumber,
            string step,
            string expectedShift)
        {
            var tempoMap = TempoMap.Default;

            var eventTimeTicks = TimeConverter.ConvertFrom(TimeSpanUtilities.Parse(eventTime), tempoMap);
            var expectedShiftTicks = TimeConverter.ConvertFrom(TimeSpanUtilities.Parse(expectedShift), tempoMap);

            CheckRepeat(
                inputObjects: new[]
                {
                    new TimedEvent(new TextEvent("A"), eventTimeTicks),
                },
                repeatsNumber: repeatsNumber,
                settings: new RepeatingSettings
                {
                    ShiftPolicy = ShiftPolicy.ShiftByMaxTime,
                    ShiftRoundingPolicy = TimeSpanRoundingPolicy.RoundUp,
                    ShiftRoundingStep = TimeSpanUtilities.Parse(step)
                },
                expectedObjects: Enumerable
                    .Range(0, repeatsNumber + 1)
                    .Select(i => new TimedEvent(
                        new TextEvent("A"),
                        eventTimeTicks + expectedShiftTicks * i))
                    .ToArray());
        }

        [TestCaseSource(nameof(RoundUpStepData_ShiftByFixedValue))]
        public void CheckRepeat_SingleCollection_SingleTimedEvent_ShiftByFixedValue_Step(
            string eventTime,
            int repeatsNumber,
            string shift,
            string step,
            string expectedShift)
        {
            var tempoMap = TempoMap.Default;

            var eventTimeTicks = TimeConverter.ConvertFrom(TimeSpanUtilities.Parse(eventTime), tempoMap);
            var expectedShiftTicks = TimeConverter.ConvertFrom(TimeSpanUtilities.Parse(expectedShift), tempoMap);

            CheckRepeat(
                inputObjects: new[]
                {
                    new TimedEvent(new TextEvent("A"), eventTimeTicks),
                },
                repeatsNumber: repeatsNumber,
                settings: new RepeatingSettings
                {
                    ShiftPolicy = ShiftPolicy.ShiftByFixedValue,
                    Shift = TimeSpanUtilities.Parse(shift),
                    ShiftRoundingStep = TimeSpanUtilities.Parse(step),
                    ShiftRoundingPolicy = TimeSpanRoundingPolicy.RoundUp
                },
                expectedObjects: Enumerable
                    .Range(0, repeatsNumber + 1)
                    .Select(i => new TimedEvent(
                        new TextEvent("A"),
                        eventTimeTicks + expectedShiftTicks * i))
                    .ToArray());
        }

        [Test]
        public void CheckRepeat_SingleCollection_MultipleTimedEvents_NoShift([Values(1, 10)] long eventTime, [Values(1, 10)] int repeatsNumber) => CheckRepeat(
            inputObjects: new[]
            {
                new TimedEvent(new TextEvent("A"), eventTime),
                new TimedEvent(new ProgramChangeEvent(), eventTime - 1),
            },
            repeatsNumber: repeatsNumber,
            settings: new RepeatingSettings
            {
                ShiftPolicy = ShiftPolicy.None
            },
            expectedObjects: Enumerable
                .Range(0, repeatsNumber + 1)
                .SelectMany(_ => new[]
                {
                    new TimedEvent(new TextEvent("A"), eventTime),
                    new TimedEvent(new ProgramChangeEvent(), eventTime - 1)
                })
                .ToArray());

        [Test]
        public void CheckRepeat_SingleCollection_MultipleTimedEvents_ShiftByMaxTime([Values(1, 10)] long eventTime, [Values(1, 10)] int repeatsNumber) => CheckRepeat(
            inputObjects: new[]
            {
                new TimedEvent(new TextEvent("A"), eventTime),
                new TimedEvent(new ProgramChangeEvent(), eventTime - 1),
            },
            repeatsNumber: repeatsNumber,
            settings: new RepeatingSettings
            {
                ShiftPolicy = ShiftPolicy.ShiftByMaxTime
            },
            expectedObjects: Enumerable
                .Range(0, repeatsNumber + 1)
                .SelectMany(i => new[]
                {
                    new TimedEvent(new TextEvent("A"), eventTime + eventTime * i),
                    new TimedEvent(new ProgramChangeEvent(), eventTime - 1 + eventTime * i)
                })
                .ToArray());

        [Test]
        public void CheckRepeat_SingleCollection_MultipleTimedEvents_ShiftByFixedValue([Values(1, 10)] long eventTime, [Values(1, 10)] int repeatsNumber, [Values(0, 10)] long shift) => CheckRepeat(
            inputObjects: new[]
            {
                new TimedEvent(new TextEvent("A"), eventTime),
                new TimedEvent(new ProgramChangeEvent(), eventTime - 1),
            },
            repeatsNumber: repeatsNumber,
            settings: new RepeatingSettings
            {
                ShiftPolicy = ShiftPolicy.ShiftByFixedValue,
                Shift = (MidiTimeSpan)shift
            },
            expectedObjects: Enumerable
                .Range(0, repeatsNumber + 1)
                .SelectMany(i => new[]
                {
                    new TimedEvent(new TextEvent("A"), eventTime + shift * i),
                    new TimedEvent(new ProgramChangeEvent(), eventTime - 1 + shift * i)
                })
                .ToArray());

        [TestCaseSource(nameof(RoundUpStepData_ShiftByMaxTime))]
        public void CheckRepeat_SingleCollection_MultipleTimedEvents_ShiftByMaxTime_Step(
            string eventTime,
            int repeatsNumber,
            string step,
            string expectedShift)
        {
            var tempoMap = TempoMap.Default;

            var eventTimeTicks = TimeConverter.ConvertFrom(TimeSpanUtilities.Parse(eventTime), tempoMap);
            var expectedShiftTicks = TimeConverter.ConvertFrom(TimeSpanUtilities.Parse(expectedShift), tempoMap);

            CheckRepeat(
                inputObjects: new[]
                {
                    new TimedEvent(new TextEvent("A"), eventTimeTicks),
                    new TimedEvent(new ProgramChangeEvent(), eventTimeTicks - 1),
                },
                repeatsNumber: repeatsNumber,
                settings: new RepeatingSettings
                {
                    ShiftPolicy = ShiftPolicy.ShiftByMaxTime,
                    ShiftRoundingPolicy = TimeSpanRoundingPolicy.RoundUp,
                    ShiftRoundingStep = TimeSpanUtilities.Parse(step)
                },
                expectedObjects: Enumerable
                    .Range(0, repeatsNumber + 1)
                    .SelectMany(i => new[]
                    {
                        new TimedEvent(new TextEvent("A"), eventTimeTicks + expectedShiftTicks * i),
                        new TimedEvent(new ProgramChangeEvent(), eventTimeTicks - 1 + expectedShiftTicks * i)
                    })
                    .ToArray());
        }

        [Test]
        public void CheckRepeat_SingleCollection_SaveTempoMap_1() => CheckRepeat(
            inputObjects: new[]
            {
                new TimedEvent(new TextEvent("A"), 10),
                new TimedEvent(new SetTempoEvent(200000), 100),
                new TimedEvent(new TimeSignatureEvent(3, 8), 110),
                new TimedEvent(new ProgramChangeEvent(), 120),
            },
            repeatsNumber: 1,
            settings: new RepeatingSettings
            {
                SaveTempoMap = true
            },
            expectedObjects: new[]
            {
                new TimedEvent(new TextEvent("A"), 10),
                new TimedEvent(new SetTempoEvent(200000), 100),
                new TimedEvent(new TimeSignatureEvent(3, 8), 110),
                new TimedEvent(new ProgramChangeEvent(), 120),

                new TimedEvent(new TimeSignatureEvent(), 120),
                new TimedEvent(new SetTempoEvent(), 120),
                new TimedEvent(new TextEvent("A"), 130),
                new TimedEvent(new SetTempoEvent(200000), 220),
                new TimedEvent(new TimeSignatureEvent(3, 8), 230),
                new TimedEvent(new ProgramChangeEvent(), 240),
            });

        [Test]
        public void CheckRepeat_SingleCollection_SaveTempoMap_2() => CheckRepeat(
            inputObjects: new[]
            {
                new TimedEvent(new TextEvent("A"), 10),
                new TimedEvent(new SetTempoEvent(200000), 100),
                new TimedEvent(new TimeSignatureEvent(3, 8), 110),
                new TimedEvent(new ProgramChangeEvent(), 120),
            },
            repeatsNumber: 2,
            settings: new RepeatingSettings
            {
                SaveTempoMap = true
            },
            expectedObjects: new[]
            {
                new TimedEvent(new TextEvent("A"), 10),
                new TimedEvent(new SetTempoEvent(200000), 100),
                new TimedEvent(new TimeSignatureEvent(3, 8), 110),
                new TimedEvent(new ProgramChangeEvent(), 120),

                new TimedEvent(new TimeSignatureEvent(), 120),
                new TimedEvent(new SetTempoEvent(), 120),
                new TimedEvent(new TextEvent("A"), 130),
                new TimedEvent(new SetTempoEvent(200000), 220),
                new TimedEvent(new TimeSignatureEvent(3, 8), 230),
                new TimedEvent(new ProgramChangeEvent(), 240),

                new TimedEvent(new TimeSignatureEvent(), 240),
                new TimedEvent(new SetTempoEvent(), 240),
                new TimedEvent(new TextEvent("A"), 250),
                new TimedEvent(new SetTempoEvent(200000), 340),
                new TimedEvent(new TimeSignatureEvent(3, 8), 350),
                new TimedEvent(new ProgramChangeEvent(), 360),
            });

        [Test]
        public void CheckRepeat_SingleCollection_DontSaveTempoMap() => CheckRepeat(
            inputObjects: new[]
            {
                new TimedEvent(new TextEvent("A"), 10),
                new TimedEvent(new SetTempoEvent(200000), 100),
                new TimedEvent(new TimeSignatureEvent(3, 8), 110),
                new TimedEvent(new ProgramChangeEvent(), 120),
            },
            repeatsNumber: 1,
            settings: new RepeatingSettings
            {
                SaveTempoMap = false
            },
            expectedObjects: new[]
            {
                new TimedEvent(new TextEvent("A"), 10),
                new TimedEvent(new SetTempoEvent(200000), 100),
                new TimedEvent(new TimeSignatureEvent(3, 8), 110),
                new TimedEvent(new ProgramChangeEvent(), 120),

                new TimedEvent(new TextEvent("A"), 130),
                new TimedEvent(new SetTempoEvent(200000), 220),
                new TimedEvent(new TimeSignatureEvent(3, 8), 230),
                new TimedEvent(new ProgramChangeEvent(), 240),
            });

        [Test]
        public void CheckRepeat_SingleCollection_NotesAndTimedEvents() => CheckRepeat(
            inputObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 10),
                new Note((SevenBitNumber)70, 100, 105),
                new TimedEvent(new ProgramChangeEvent(), 120),
                new Note((SevenBitNumber)40, 30, 140),
                new Note((SevenBitNumber)50, 30, 150),
            },
            repeatsNumber: 1,
            settings: null,
            expectedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 10),
                new Note((SevenBitNumber)70, 100, 105),
                new TimedEvent(new ProgramChangeEvent(), 120),
                new Note((SevenBitNumber)40, 30, 140),
                new Note((SevenBitNumber)50, 30, 150),

                new TimedEvent(new TextEvent("A"), 215),
                new Note((SevenBitNumber)70, 100, 310),
                new TimedEvent(new ProgramChangeEvent(), 325),
                new Note((SevenBitNumber)40, 30, 345),
                new Note((SevenBitNumber)50, 30, 355),
            });

        [Test]
        public void CheckRepeat_SingleCollection_Nulls() => CheckRepeat(
            inputObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 10),
                null
            },
            repeatsNumber: 1,
            settings: null,
            expectedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 10),
                new TimedEvent(new TextEvent("A"), 20),
            });

        [Test]
        public void CheckRepeat_NonPositiveRepeatsNumber([Values(0, -1)] int repeatsNumber)
        {
            var repeater = new Repeater();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => repeater.Repeat(Array.Empty<ITimedObject>(), repeatsNumber, TempoMap.Default),
                "No desired exception on timed objects.");

            Assert.Throws<ArgumentOutOfRangeException>(
                () => repeater.Repeat(new TrackChunk(), repeatsNumber, TempoMap.Default),
                "No desired exception on track chunk.");

            Assert.Throws<ArgumentOutOfRangeException>(
                () => repeater.Repeat(new[] { new TrackChunk() }, repeatsNumber, TempoMap.Default),
                "No desired exception on track chunks.");

            Assert.Throws<ArgumentOutOfRangeException>(
                () => repeater.Repeat(new MidiFile(), repeatsNumber),
                "No desired exception on file.");
        }

        [Test]
        public void CheckRepeat_NullSource()
        {
            var repeater = new Repeater();

            Assert.Throws<ArgumentNullException>(
                () => repeater.Repeat(default(IEnumerable<ITimedObject>), 1, TempoMap.Default),
                "No desired exception on timed objects.");

            Assert.Throws<ArgumentNullException>(
                () => repeater.Repeat(default(TrackChunk), 1, TempoMap.Default),
                "No desired exception on track chunk.");

            Assert.Throws<ArgumentNullException>(
                () => repeater.Repeat(default(IEnumerable<TrackChunk>), 1, TempoMap.Default),
                "No desired exception on track chunks.");

            Assert.Throws<ArgumentNullException>(
                () => repeater.Repeat(default(MidiFile), 1),
                "No desired exception on file.");
        }

        [Test]
        public void CheckRepeat_NullTempoMap()
        {
            var repeater = new Repeater();

            Assert.Throws<ArgumentNullException>(
                () => repeater.Repeat(Array.Empty<ITimedObject>(), 1, null),
                "No desired exception on timed objects.");

            Assert.Throws<ArgumentNullException>(
                () => repeater.Repeat(new TrackChunk(), 1, null),
                "No desired exception on track chunk.");

            Assert.Throws<ArgumentNullException>(
                () => repeater.Repeat(new[] { new TrackChunk() }, 1, null),
                "No desired exception on track chunks.");
        }

        #endregion

        #region Private methods

        private void CheckRepeat(
            ICollection<ITimedObject> inputObjects,
            int repeatsNumber,
            RepeatingSettings settings,
            ICollection<ITimedObject> expectedObjects) =>
            CheckRepeat(
                inputObjects,
                repeatsNumber,
                TempoMap.Default,
                settings,
                expectedObjects);

        private void CheckRepeat(
            ICollection<ITimedObject> inputObjects,
            int repeatsNumber,
            TempoMap tempoMap,
            RepeatingSettings settings,
            ICollection<ITimedObject> expectedObjects)
        {
            inputObjects = inputObjects.OrderBy(obj => obj?.Time ?? 0).ToArray();
            expectedObjects = expectedObjects.OrderBy(obj => obj.Time).ToArray();

            //

            var actualObjects = inputObjects.Repeat(repeatsNumber, tempoMap, settings).OrderBy(obj => obj.Time).ToArray();
            MidiAsserts.AreEqual(expectedObjects, actualObjects.OrderBy(obj => obj.Time), "Invalid result objects collection.");

            Assert.IsFalse(
                inputObjects.Any(obj => actualObjects.Contains(obj)),
                "Some result objects refer to source one(s).");
            Assert.AreEqual(actualObjects.Length, actualObjects.Distinct().Count(), "Some objects are not unique.");

            //

            var inputTrackChunk = inputObjects.ToTrackChunk();
            var actualTrackChunk = inputTrackChunk.Repeat(repeatsNumber, tempoMap, settings);
            MidiAsserts.AreEqual(expectedObjects.ToTrackChunk(), actualTrackChunk, true, "Invalid result track chunk.");
            Assert.AreNotSame(inputTrackChunk, actualTrackChunk, "Result track chunk refers to the input one.");

            //

            var inputTrackChunks = new[] { inputObjects.ToTrackChunk() };
            var actualTrackChunks = inputTrackChunks.Repeat(repeatsNumber, tempoMap, settings);
            MidiAsserts.AreEqual(new[] { expectedObjects.ToTrackChunk() }, actualTrackChunks, true, "Invalid result track chunks.");
            Assert.AreNotSame(inputTrackChunks.First(), actualTrackChunks.First(), "Result track chunks refers to the input ones.");

            //

            var inputFile = inputObjects.ToFile();
            var actualFile = inputFile.Repeat(repeatsNumber, settings);
            MidiAsserts.AreEqual(expectedObjects.ToFile(), actualFile, true, "Invalid result file.");
            Assert.AreNotSame(inputFile, actualFile, "Result file refers to the input one.");
        }

        private static object[] GetRoundUpStepData_ShiftByMaxTime(
            int[] repeatsNumbers,
            Dictionary<string, (string EventTime, string ExpectedShift)[]> shifts) =>
            repeatsNumbers
                .SelectMany(repeatsNumber => shifts.SelectMany(shift => shift.Value.Select(s => new object[]
                {
                    s.EventTime,
                    repeatsNumber,
                    shift.Key,
                    s.ExpectedShift
                })))
                .ToArray();

        private static object[] GetRoundUpStepData_ShiftByFixedValue(
            int[] repeatsNumbers,
            Dictionary<(string Shift, string ShiftStep), (string EventTime, string ExpectedShift)[]> shifts) =>
            repeatsNumbers
                .SelectMany(repeatsNumber => shifts.SelectMany(shift => shift.Value.Select(s => new object[]
                {
                    s.EventTime,
                    repeatsNumber,
                    shift.Key.Shift,
                    shift.Key.ShiftStep,
                    s.ExpectedShift
                })))
                .ToArray();

        #endregion
    }
}
