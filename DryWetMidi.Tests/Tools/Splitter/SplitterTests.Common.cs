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
    [TestFixture]
    public sealed partial class SplitterTests
    {
        #region Nested classes

        private sealed class SplitData
        {
            #region Constructor

            public SplitData(
                IEnumerable<ILengthedObject> inputObjects,
                IEnumerable<ILengthedObject> expectedObjects,
                IEnumerable<ILengthedObject> actualObjects)
            {
                InputObjects = inputObjects;
                ExpectedObjects = expectedObjects;
                ActualObjects = actualObjects;
            }

            #endregion

            #region Properties

            public IEnumerable<ILengthedObject> InputObjects { get; }

            public IEnumerable<ILengthedObject> ExpectedObjects { get; }

            public IEnumerable<ILengthedObject> ActualObjects { get; }

            #endregion
        }

        #endregion

        #region Constants

        private static readonly Dictionary<Type, TimeSpanType> TimeSpanTypes = new Dictionary<Type, TimeSpanType>
        {
            [typeof(MidiTimeSpan)] = TimeSpanType.Midi,
            [typeof(MetricTimeSpan)] = TimeSpanType.Metric,
            [typeof(MusicalTimeSpan)] = TimeSpanType.Musical,
            [typeof(BarBeatTicksTimeSpan)] = TimeSpanType.BarBeatTicks,
            [typeof(BarBeatFractionTimeSpan)] = TimeSpanType.BarBeatFraction
        };

        #endregion

        #region Fields

        private readonly ObjectsFactory _factory = new ObjectsFactory(TempoMap.Default);

        #endregion

        #region Private methods

        private static void CompareTimedEvents(
            IEnumerable<TimedEvent> actualTimedEvents,
            IEnumerable<TimedEvent> expectedTimedEvents,
            string message)
        {
            MidiAsserts.AreEqual(
                expectedTimedEvents,
                actualTimedEvents,
                false,
                0,
                message);
        }

        private static void CompareTimedEventsSplittingByGrid(
            TimedEvent[][] inputTimedEvents,
            IGrid grid,
            SliceMidiFileSettings settings,
            TimedEvent[][] outputTimedEvents)
        {
            var trackChunks = inputTimedEvents.Select(e => e.ToTrackChunk());
            var midiFile = new MidiFile(trackChunks);

            var newFiles = midiFile.SplitByGrid(grid, settings).ToList();
            Assert.AreEqual(outputTimedEvents.Length, newFiles.Count, "New files count is invalid.");

            for (var i = 0; i < outputTimedEvents.Length; i++)
            {
                CompareTimedEvents(
                    newFiles[i].GetTimedEvents(),
                    outputTimedEvents[i],
                    $"File {i} contains invalid events.");
            }
        }

        private Note[] CreateInputNotes(long length)
        {
            return new[]
            {
                new Note((SevenBitNumber)100, length),
                new Note((SevenBitNumber)110, length, 200),
            };
        }

        private Chord[] CreateInputChords(long length)
        {
            return length == 0
                ? new[]
                {
                    new Chord(new Note((SevenBitNumber)100),
                              new Note((SevenBitNumber)23)),
                    new Chord(new Note((SevenBitNumber)1),
                              new Note((SevenBitNumber)2),
                              new Note((SevenBitNumber)3))
                }
                : new[]
                {
                    new Chord(new Note((SevenBitNumber)100, length - 10),
                              new Note((SevenBitNumber)23, length - 10, 10)),
                    new Chord(new Note((SevenBitNumber)1, length - 1, 10),
                              new Note((SevenBitNumber)2, length - 5, 11),
                              new Note((SevenBitNumber)3, length - 10, 20))
                };
        }

        private void SplitByGrid_MultipleSteps<TObject>(
            IEnumerable<TObject> inputObjects,
            ITimeSpan gridStart,
            IEnumerable<ITimeSpan> gridSteps,
            Dictionary<TObject, IEnumerable<TimeAndLength>> expectedParts,
            TempoMap tempoMap)
            where TObject : ILengthedObject
        {
            var expectedObjects = expectedParts
                .SelectMany(p => p.Value.Select(tl => CloneAndChangeTimeAndLength(
                    p.Key,
                    TimeConverter.ConvertFrom(tl.Time, tempoMap),
                    LengthConverter.ConvertFrom(tl.Length, tl.Time, tempoMap))))
                .ToArray();

            var actualObjects = Splitter.SplitObjectsByGrid(inputObjects.OfType<ILengthedObject>(), new SteppedGrid(gridStart, gridSteps), tempoMap).ToArray();

            MidiAsserts.AreEqual(
                expectedObjects.OfType<ITimedObject>(),
                actualObjects.OfType<ITimedObject>(),
                true,
                0,
                "Objects are invalid.");
        }

        private void SplitByGrid_OneStep_SinglePart<TObject>(
            ITimeSpan gridStart,
            ITimeSpan step,
            TempoMap tempoMap,
            LengthedObjectMethods<TObject> methods)
            where TObject : ILengthedObject
        {
            var gridSteps = new[] { step };

            var firstTime = TimeConverter.ConvertFrom(gridStart, tempoMap);

            long secondTime = firstTime;
            for (int i = 0; i < 5; i++)
            {
                secondTime += LengthConverter.ConvertFrom(step, secondTime, tempoMap);
            }

            var inputObjects = new[]
            {
                methods.Create(firstTime, LengthConverter.ConvertFrom(step, firstTime, tempoMap)),
                methods.Create(secondTime, LengthConverter.ConvertFrom(step, secondTime, tempoMap))
            };

            var data = SplitByGrid_ClonesExpected(inputObjects.OfType<ILengthedObject>(), gridStart, gridSteps, tempoMap);

            var stepType = TimeSpanTypes[step.GetType()];
            var partLength = data.ActualObjects.First().LengthAs(stepType, tempoMap);
            Assert.IsTrue(data.ActualObjects.All(o => o.LengthAs(stepType, tempoMap).Equals(partLength)),
                          $"Objects have different length measured as {stepType}.");
        }

        private SplitData SplitByGrid_ClonesExpected(
            IEnumerable<ILengthedObject> inputObjects,
            ITimeSpan gridStart,
            IEnumerable<ITimeSpan> gridSteps,
            TempoMap tempoMap)
        {
            var expectedObjects = inputObjects.Select(o => o == null ? default(ILengthedObject) : (ILengthedObject)o.Clone()).ToArray();
            var actualObjects = Splitter
                .SplitObjectsByGrid(inputObjects.OfType<ILengthedObject>(), new SteppedGrid(gridStart, gridSteps), tempoMap)
                .ToArray();

            MidiAsserts.AreEqual(
                expectedObjects.OfType<ITimedObject>(),
                actualObjects.OfType<ITimedObject>(),
                true,
                0,
                "Objects are invalid.");

            return new SplitData(inputObjects, expectedObjects, actualObjects);
        }

        private IEnumerable<ILengthedObject> Split(ILengthedObject obj, IEnumerable<long> times)
        {
            var tail = (ILengthedObject)obj.Clone();

            foreach (var time in times.OrderBy(t => t))
            {
                var parts = tail.Split(time);
                yield return parts.LeftPart;

                tail = parts.RightPart;
            }

            yield return tail;
        }

        private ILengthedObject CloneAndChangeTimeAndLength(ILengthedObject obj, long time, long length)
        {
            var result = (ILengthedObject)obj.Clone();
            result.Time = time;
            result.Length = length;
            return result;
        }

        #endregion
    }
}
