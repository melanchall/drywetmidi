using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed partial class QuantizerTests
    {
        #region Nested classes

        public enum EntryPoint
        {
            Objects,
            TrackChunk,
            TrackChunkCustomQuantizer,
            TrackChunks,
            TrackChunksCustomQuantizer,
            MidiFile,
            MidiFileCustomQuantizer
        }

        public enum InvalidGrid
        {
            NegativeFirstTime,
            NegativeLaterTime,
            RepeatedTime,
            DecreasingTime,
            UnsortedArbitraryGrid,
            DuplicateArbitraryGrid,
            ZeroStep,
            ZeroStepAfterPositiveStep,
            StepRoundedToZero
        }

        public enum BoundaryRange
        {
            MaximumTime,
            LastTwoTicks,
            NegativeLowerBound
        }

        private sealed class TestGrid : IGrid
        {
            private readonly Func<IEnumerable<long>> _getTimes;

            public TestGrid(Func<IEnumerable<long>> getTimes)
            {
                _getTimes = getTimes;
            }

            public IEnumerable<long> GetTimes(TempoMap tempoMap) => _getTimes();
        }

        private sealed class FixedBounds : IBounds
        {
            private readonly long _minimumTime;
            private readonly long _maximumTime;

            public FixedBounds(long minimumTime, long maximumTime)
            {
                _minimumTime = minimumTime;
                _maximumTime = maximumTime;
            }

            public (long MinTime, long MaxTime) GetBounds(long time, TempoMap tempoMap) =>
                (_minimumTime, _maximumTime);
        }

        #endregion

        #region Test methods

        [Test]
        public void Quantize_InvalidGridTimes(
            [Values] EntryPoint entryPoint,
            [Values] InvalidGrid invalidGrid)
        {
            var objects = new ITimedObject[] { new Note((SevenBitNumber)60, 10, 25) };
            var grid = CreateInvalidGrid(invalidGrid);

            ClassicAssert.Throws<InvalidOperationException>(
                () => Quantize(entryPoint, objects, grid),
                $"The {entryPoint} entry point must reject {invalidGrid}.");
        }

        [Test]
        public void Quantize_SinglePassObjects(
            [Values(
                QuantizerTarget.Start,
                QuantizerTarget.End,
                QuantizerTarget.Start | QuantizerTarget.End)] QuantizerTarget target)
        {
            var note = new Note((SevenBitNumber)60, 12, 23);
            var timedEvent = new TimedEvent(new TextEvent("text"), 17);
            var filteredNote = new Note((SevenBitNumber)61, 5, 24);
            var objects = new SinglePassEnumerable<ITimedObject>(
                new ITimedObject[] { note, null, timedEvent, filteredNote });
            var filterCalls = 0;
            var settings = new QuantizingSettings
            {
                Target = target,
                Filter = obj =>
                {
                    filterCalls++;
                    return obj != filteredNote;
                }
            };

            new Quantizer().Quantize(objects, new SteppedGrid((MidiTimeSpan)10), TempoMap.Default, settings);

            var expectedNote = target == QuantizerTarget.Start
                ? new Note((SevenBitNumber)60, 12, 20)
                : target == QuantizerTarget.End
                    ? new Note((SevenBitNumber)60, 12, 18)
                    : new Note((SevenBitNumber)60, 10, 20);
            var expectedEvent = new TimedEvent(new TextEvent("text"), target == QuantizerTarget.End ? 17 : 20);

            MidiAsserts.AreEqual(
                new ITimedObject[] { expectedNote, expectedEvent, new Note((SevenBitNumber)61, 5, 24) },
                new ITimedObject[] { note, timedEvent, filteredNote },
                "Single-pass quantization must update the original selected objects.");
            ClassicAssert.AreEqual(1, objects.EnumerationsCount, "Objects must be enumerated once.");
            ClassicAssert.AreEqual(1, objects.DisposalsCount, "The objects enumerator must be disposed.");
            ClassicAssert.AreEqual(3, filterCalls, "The filter must be evaluated once per non-null object.");
        }

        [Test]
        public void Quantize_SinglePassGrid(
            [Values] EntryPoint entryPoint,
            [Values(0, 1, 4)] int gridPointsCount)
        {
            var sequences = new List<SinglePassEnumerable<long>>();
            var grid = new TestGrid(() =>
            {
                var sequence = new SinglePassEnumerable<long>(
                    Enumerable.Range(0, gridPointsCount).Select(index => (long)index * 10));
                sequences.Add(sequence);
                return sequence;
            });
            var objects = new ITimedObject[] { new Note((SevenBitNumber)60, 5, 23) };
            var expectedNote = new Note((SevenBitNumber)60, 5, gridPointsCount == 0 ? 23 : gridPointsCount == 1 ? 0 : 20);

            var actualEvents = Quantize(entryPoint, objects, grid);

            MidiAsserts.AreEqual(
                new ITimedObject[] { expectedNote }.ToTrackChunk().GetTimedEvents().ToArray(),
                actualEvents,
                "Single-pass grid quantization must produce the expected events.");
            ClassicAssert.AreEqual(1, sequences.Count, "The grid should be requested once for a single track.");
            ClassicAssert.AreEqual(1, sequences[0].EnumerationsCount, "Grid times must be enumerated once.");
            ClassicAssert.AreEqual(1, sequences[0].DisposalsCount, "The grid enumerator must be disposed.");
        }

        [Test]
        public void Quantize_StopsGridAtRequiredTime([Values] EntryPoint entryPoint)
        {
            var objects = new ITimedObject[] { new Note((SevenBitNumber)60, 5, 23) };
            var grid = new TestGrid(GetLimitedGridTimes);

            var actualEvents = Quantize(entryPoint, objects, grid);

            MidiAsserts.AreEqual(
                new ITimedObject[] { new Note((SevenBitNumber)60, 5, 20) }.ToTrackChunk().GetTimedEvents().ToArray(),
                actualEvents,
                "Quantization must not consume grid times after the required upper neighbor.");
        }

        [Test]
        public void Quantize_InvalidGridDisposesEnumerator([Values] EntryPoint entryPoint)
        {
            var sequence = new SinglePassEnumerable<long>(new long[] { 0, 0 });
            var grid = new TestGrid(() => sequence);

            ClassicAssert.Throws<InvalidOperationException>(
                () => Quantize(entryPoint, new ITimedObject[] { new Note((SevenBitNumber)60, 5, 23) }, grid),
                "Invalid grid times must throw.");

            ClassicAssert.AreEqual(1, sequence.EnumerationsCount, "Invalid grid times must be enumerated once.");
            ClassicAssert.AreEqual(1, sequence.DisposalsCount, "The invalid grid enumerator must be disposed.");
        }

        [Test]
        public void Quantize_SinglePassTrackChunks(
            [Values(false, true)] bool customQuantizer)
        {
            var trackChunks = new[]
            {
                new ITimedObject[] { new Note((SevenBitNumber)60, 5, 23) }.ToTrackChunk(),
                new ITimedObject[] { new Note((SevenBitNumber)61, 5, 7) }.ToTrackChunk()
            };
            var sequence = new SinglePassEnumerable<TrackChunk>(
                new[] { trackChunks[0], null, trackChunks[1] });
            var grid = new SteppedGrid((MidiTimeSpan)10);

            if (customQuantizer)
                sequence.QuantizeObjects(new Quantizer(), ObjectType.Note, grid, TempoMap.Default);
            else
                sequence.QuantizeObjects(ObjectType.Note, grid, TempoMap.Default);

            MidiAsserts.AreEqual(
                new[]
                {
                    new ITimedObject[] { new Note((SevenBitNumber)60, 5, 20) }.ToTrackChunk(),
                    new ITimedObject[] { new Note((SevenBitNumber)61, 5, 10) }.ToTrackChunk()
                },
                trackChunks,
                true,
                "Single-pass track collections must update every non-null track.");
            ClassicAssert.AreEqual(1, sequence.EnumerationsCount, "Tracks must be enumerated once.");
            ClassicAssert.AreEqual(1, sequence.DisposalsCount, "The tracks enumerator must be disposed.");
        }

        [Test]
        public void Quantize_LargeRandomizationRange(
            [Values] EntryPoint entryPoint,
            [Values(
                QuantizerTarget.Start,
                QuantizerTarget.End,
                QuantizerTarget.Start | QuantizerTarget.End)] QuantizerTarget target,
            [Values((long)int.MaxValue, (long)uint.MaxValue, long.MaxValue)] long maximumTime)
        {
            var settings = new QuantizingSettings
            {
                Target = target,
                QuantizingLevel = 0,
                FixOppositeEnd = true,
                QuantizingBeyondFixedEndPolicy = QuantizingBeyondFixedEndPolicy.CollapseAndMove,
                RandomizingSettings = new RandomizingSettings
                {
                    Bounds = new FixedBounds(0, maximumTime)
                }
            };
            var objects = new ITimedObject[] { new Note((SevenBitNumber)60, 0, 100) };
            var grid = new ArbitraryGrid((MidiTimeSpan)100);

            var actualEvents = Quantize(entryPoint, objects, grid, settings);

            ClassicAssert.AreEqual(2, actualEvents.Length, "Randomization must preserve paired note events.");

            foreach (var timedEvent in actualEvents)
            {
                ClassicAssert.GreaterOrEqual(timedEvent.Time, 0, "Randomized time must be nonnegative.");
                ClassicAssert.LessOrEqual(timedEvent.Time, maximumTime, "Randomized time must stay within bounds.");
            }
        }

        [Test]
        public void Quantize_LargeConstantRandomizationRange(
            [Values] EntryPoint entryPoint,
            [Values(QuantizerTarget.Start, QuantizerTarget.End)] QuantizerTarget target,
            [Values((long)int.MaxValue, (long)uint.MaxValue, long.MaxValue)] long maximumTime)
        {
            var settings = new QuantizingSettings
            {
                Target = target,
                QuantizingLevel = 0,
                RandomizingSettings = new RandomizingSettings
                {
                    Bounds = new ConstantBounds((MidiTimeSpan)100, (MidiTimeSpan)(maximumTime - 100))
                }
            };

            var actualEvents = Quantize(
                entryPoint,
                new ITimedObject[] { new Note((SevenBitNumber)60, 0, 100) },
                new ArbitraryGrid((MidiTimeSpan)100),
                settings);

            ClassicAssert.AreEqual(2, actualEvents.Length, "Randomization must preserve paired note events.");

            foreach (var timedEvent in actualEvents)
            {
                ClassicAssert.GreaterOrEqual(timedEvent.Time, 0, "Randomized time must be nonnegative.");
                ClassicAssert.LessOrEqual(timedEvent.Time, maximumTime, "Randomized time must stay within constant bounds.");
            }
        }

        [Test]
        public void Quantize_RandomizationUsesWholeLargeRange([Values] EntryPoint entryPoint)
        {
            var objects = Enumerable.Range(0, 64)
                .Select(index => (ITimedObject)new TimedEvent(new TextEvent(index.ToString()), 100))
                .ToArray();
            var settings = new QuantizingSettings
            {
                QuantizingLevel = 0,
                RandomizingSettings = new RandomizingSettings
                {
                    Bounds = new FixedBounds(0, (long)uint.MaxValue + 100)
                }
            };

            var actualEvents = Quantize(entryPoint, objects, new ArbitraryGrid((MidiTimeSpan)100), settings);

            ClassicAssert.IsTrue(
                actualEvents.Any(timedEvent => timedEvent.Time > int.MaxValue),
                "Sampling must not truncate a large range to its low 32 bits.");
        }

        [Test]
        public void Quantize_RandomizationBoundaryRange(
            [Values] BoundaryRange boundaryRange,
            [Values] EntryPoint entryPoint)
        {
            var minimumTime = boundaryRange == BoundaryRange.MaximumTime
                ? long.MaxValue
                : boundaryRange == BoundaryRange.LastTwoTicks ? long.MaxValue - 1 : -10;
            var maximumTime = boundaryRange == BoundaryRange.NegativeLowerBound ? 0 : long.MaxValue;
            var settings = new QuantizingSettings
            {
                QuantizingLevel = 0,
                RandomizingSettings = new RandomizingSettings
                {
                    Bounds = new FixedBounds(minimumTime, maximumTime)
                }
            };

            var actualEvents = Quantize(
                entryPoint,
                new ITimedObject[] { new TimedEvent(new TextEvent("text"), 100) },
                new ArbitraryGrid((MidiTimeSpan)100),
                settings);

            ClassicAssert.GreaterOrEqual(actualEvents[0].Time, Math.Max(0, minimumTime), "The lower bound must be respected.");
            ClassicAssert.LessOrEqual(actualEvents[0].Time, maximumTime, "The upper bound must be respected.");
        }

        [Test]
        public void Quantize_InvalidRandomizationBounds(
            [Values] EntryPoint entryPoint,
            [Values(false, true)] bool entirelyNegative)
        {
            var settings = new QuantizingSettings
            {
                RandomizingSettings = new RandomizingSettings
                {
                    Bounds = entirelyNegative ? new FixedBounds(-10, -1) : new FixedBounds(20, 10)
                }
            };

            ClassicAssert.Throws<InvalidOperationException>(
                () => Quantize(
                    entryPoint,
                    new ITimedObject[] { new TimedEvent(new TextEvent("text"), 100) },
                    new ArbitraryGrid((MidiTimeSpan)100),
                    settings),
                "Randomization bounds must contain a nonnegative time in ascending order.");
        }

        #endregion

        #region Private methods

        private static TimedEvent[] Quantize(
            EntryPoint entryPoint,
            ITimedObject[] objects,
            IGrid grid,
            QuantizingSettings settings = null)
        {
            if (entryPoint == EntryPoint.Objects)
            {
                new Quantizer().Quantize(objects, grid, TempoMap.Default, settings);
                return objects.ToTrackChunk().GetTimedEvents().ToArray();
            }

            var trackChunk = objects.ToTrackChunk();
            var objectType = objects.All(obj => obj is Note) ? ObjectType.Note : ObjectType.TimedEvent;

            switch (entryPoint)
            {
                case EntryPoint.TrackChunk:
                    trackChunk.QuantizeObjects(objectType, grid, TempoMap.Default, settings);
                    break;
                case EntryPoint.TrackChunkCustomQuantizer:
                    trackChunk.QuantizeObjects(new Quantizer(), objectType, grid, TempoMap.Default, settings);
                    break;
                case EntryPoint.TrackChunks:
                    new[] { trackChunk }.QuantizeObjects(objectType, grid, TempoMap.Default, settings);
                    break;
                case EntryPoint.TrackChunksCustomQuantizer:
                    new[] { trackChunk }.QuantizeObjects(new Quantizer(), objectType, grid, TempoMap.Default, settings);
                    break;
                case EntryPoint.MidiFile:
                    new MidiFile(trackChunk).QuantizeObjects(objectType, grid, settings);
                    break;
                case EntryPoint.MidiFileCustomQuantizer:
                    new MidiFile(trackChunk).QuantizeObjects(new Quantizer(), objectType, grid, settings);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(entryPoint));
            }

            return trackChunk.GetTimedEvents().ToArray();
        }

        private static IGrid CreateInvalidGrid(InvalidGrid invalidGrid)
        {
            switch (invalidGrid)
            {
                case InvalidGrid.NegativeFirstTime:
                    return new TestGrid(() => new long[] { -1, 40 });
                case InvalidGrid.NegativeLaterTime:
                    return new TestGrid(() => new long[] { 0, -1, 40 });
                case InvalidGrid.RepeatedTime:
                    return new TestGrid(() => new long[] { 0, 10, 10, 40 });
                case InvalidGrid.DecreasingTime:
                    return new TestGrid(() => new long[] { 0, 20, 10, 40 });
                case InvalidGrid.UnsortedArbitraryGrid:
                    return new ArbitraryGrid((MidiTimeSpan)0, (MidiTimeSpan)20, (MidiTimeSpan)10, (MidiTimeSpan)40);
                case InvalidGrid.DuplicateArbitraryGrid:
                    return new ArbitraryGrid((MidiTimeSpan)0, (MidiTimeSpan)10, (MidiTimeSpan)10, (MidiTimeSpan)40);
                case InvalidGrid.ZeroStep:
                    return BoundGridEnumeration(new SteppedGrid((MidiTimeSpan)0));
                case InvalidGrid.ZeroStepAfterPositiveStep:
                    return BoundGridEnumeration(new SteppedGrid(new ITimeSpan[] { (MidiTimeSpan)10, (MidiTimeSpan)0 }));
                case InvalidGrid.StepRoundedToZero:
                    return BoundGridEnumeration(new SteppedGrid(new MusicalTimeSpan(1, 1000000)));
                default:
                    throw new ArgumentOutOfRangeException(nameof(invalidGrid));
            }
        }

        private static IGrid BoundGridEnumeration(IGrid grid) =>
            new TestGrid(() => EnumerateBoundedGrid(grid));

        private static IEnumerable<long> EnumerateBoundedGrid(IGrid grid)
        {
            using (var enumerator = grid.GetTimes(TempoMap.Default).GetEnumerator())
            {
                for (var index = 0; index < 4 && enumerator.MoveNext(); index++)
                {
                    yield return enumerator.Current;
                }
            }

            throw new InvalidDataException("The quantizer failed to reject a non-advancing grid before the test limit.");
        }

        private static IEnumerable<long> GetLimitedGridTimes()
        {
            yield return 0;
            yield return 10;
            yield return 20;
            yield return 30;

            throw new InvalidDataException("The quantizer read beyond the required grid prefix.");
        }

        #endregion
    }
}
