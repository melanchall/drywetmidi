using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections;
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

        public enum RegressionSliceOperation
        {
            Grid,
            Take,
            Skip,
            Cut,
            TakeFromStart
        }

        public enum RegressionObjectContainer
        {
            Objects,
            TrackChunk,
            TrackChunks,
            MidiFile
        }

        public enum RegressionGridType
        {
            Arbitrary,
            Custom,
            Stepped
        }

        public enum RegressionCompoundState
        {
            BankProgram,
            RegisteredParameter,
            NonRegisteredParameter,
            MpeConfiguration,
            RelativeParameter,
            FourteenBitController
        }

        private sealed class RegressionGrid : IGrid
        {
            private readonly IEnumerable<long> _times;

            public RegressionGrid(IEnumerable<long> times)
            {
                _times = times;
            }

            public IEnumerable<long> GetTimes(TempoMap tempoMap) => _times;
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
            ClassicAssert.AreEqual(outputTimedEvents.Length, newFiles.Count, "New files count is invalid.");

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
            ClassicAssert.IsTrue(data.ActualObjects.All(o => o.LengthAs(stepType, tempoMap).Equals(partLength)),
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

            return new SplitData(inputObjects, expectedObjects, actualObjects.Cast<ILengthedObject>());
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

        private static MidiFile SliceRegressionFile(
            MidiFile midiFile,
            RegressionSliceOperation operation,
            SliceMidiFileSettings settings)
        {
            switch (operation)
            {
                case RegressionSliceOperation.Grid:
                    return midiFile.SplitByGrid(new SteppedGrid((MidiTimeSpan)100), settings).Skip(1).First();
                case RegressionSliceOperation.Take:
                    return midiFile.TakePart((MidiTimeSpan)100, (MidiTimeSpan)100, settings);
                case RegressionSliceOperation.Skip:
                    return midiFile.SkipPart((MidiTimeSpan)100, settings);
                case RegressionSliceOperation.Cut:
                    return midiFile.CutPart((MidiTimeSpan)100, (MidiTimeSpan)100, settings);
                case RegressionSliceOperation.TakeFromStart:
                    return midiFile.TakePart((MidiTimeSpan)100, settings);
                default:
                    throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }

        private static void SplitRegressionObjects(
            IEnumerable<ITimedObject> objects,
            RegressionObjectContainer container,
            IGrid grid)
        {
            if (container == RegressionObjectContainer.Objects)
            {
                objects.SplitObjectsByGrid(grid, TempoMap.Default).ToArray();
                return;
            }

            var trackChunk = objects.ToTrackChunk();

            switch (container)
            {
                case RegressionObjectContainer.TrackChunk:
                    trackChunk.SplitObjectsByGrid(ObjectType.Note, grid, TempoMap.Default);
                    break;
                case RegressionObjectContainer.TrackChunks:
                    new[] { trackChunk }.SplitObjectsByGrid(ObjectType.Note, grid, TempoMap.Default);
                    break;
                case RegressionObjectContainer.MidiFile:
                    new MidiFile(trackChunk).SplitObjectsByGrid(ObjectType.Note, grid);
                    break;
            }
        }

        private static IEnumerable<ITimedObject> EnumerateRegressionObjectsLazily()
        {
            yield return new Note((SevenBitNumber)60, 100);
            throw new InvalidOperationException("Splitting read the next object too early.");
        }

        private static IEnumerable<long> EnumerateRegressionGridLazily()
        {
            yield return 50;
            throw new InvalidOperationException("Splitting read the next grid point too early.");
        }

        private static TimedEvent RegressionControlChange(ControlName controlName, int value, long time)
        {
            return new TimedEvent(
                new ControlChangeEvent((SevenBitNumber)(byte)controlName, (SevenBitNumber)value),
                time);
        }

        private static TimedEvent[] GetRegressionCompoundHistory(RegressionCompoundState state)
        {
            if (state == RegressionCompoundState.BankProgram)
            {
                return new[]
                {
                    RegressionControlChange(ControlName.BankSelect, 1, 0),
                    RegressionControlChange(ControlName.LsbForBankSelect, 2, 1),
                    new TimedEvent(new ProgramChangeEvent((SevenBitNumber)10), 2),
                    RegressionControlChange(ControlName.BankSelect, 3, 3)
                };
            }

            if (state == RegressionCompoundState.FourteenBitController)
            {
                return new[]
                {
                    RegressionControlChange(ControlName.Modulation, 1, 0),
                    RegressionControlChange(ControlName.LsbForModulation, 2, 1),
                    RegressionControlChange(ControlName.Modulation, 3, 2),
                    RegressionControlChange(ControlName.LsbForModulation, 4, 3)
                };
            }

            var parameterMsb = state == RegressionCompoundState.NonRegisteredParameter
                ? ControlName.NonRegisteredParameterNumberMsb
                : ControlName.RegisteredParameterNumberMsb;
            var parameterLsb = state == RegressionCompoundState.NonRegisteredParameter
                ? ControlName.NonRegisteredParameterNumberLsb
                : ControlName.RegisteredParameterNumberLsb;
            var parameterNumber = state == RegressionCompoundState.MpeConfiguration ? 6 : 0;
            var history = new List<TimedEvent>
            {
                RegressionControlChange(parameterMsb, 0, 0),
                RegressionControlChange(parameterLsb, parameterNumber, 1)
            };

            if (state != RegressionCompoundState.RelativeParameter)
            {
                history.Add(RegressionControlChange(ControlName.DataEntryMsb, 2, 2));
                history.Add(RegressionControlChange(ControlName.LsbForDataEntry, 3, 3));
            }

            history.Add(RegressionControlChange(ControlName.DataIncrement, 0, 4));
            history.Add(RegressionControlChange(ControlName.DataIncrement, 0, 5));
            history.Add(RegressionControlChange(ControlName.DataDecrement, 0, 6));

            if (state != RegressionCompoundState.RelativeParameter)
            {
                history.Add(RegressionControlChange(parameterLsb, parameterNumber + 1, 7));
                history.Add(RegressionControlChange(ControlName.DataEntryMsb, 64, 8));
                history.Add(RegressionControlChange(parameterMsb, 127, 9));
                history.Add(RegressionControlChange(parameterLsb, 127, 10));
            }

            return history.ToArray();
        }

        #endregion
    }
}
