using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public abstract class LengthedObjectsSplitter<TObject>
        where TObject : ILengthedObject
    {
        #region Methods

        public IEnumerable<TObject> SplitByStep(IEnumerable<TObject> objects, ITimeSpan step, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);
            ThrowIfArgument.IsNull(nameof(step), step);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            foreach (var obj in objects)
            {
                if (obj == null)
                {
                    yield return default(TObject);
                    continue;
                }

                if (obj.Length == 0)
                {
                    yield return CloneObject(obj);
                    continue;
                }

                var startTime = obj.Time;
                var endTime = startTime + obj.Length;

                var time = startTime;
                var tail = CloneObject(obj);

                while (time < endTime && tail != null)
                {
                    var convertedStep = LengthConverter.ConvertFrom(step, time, tempoMap);
                    if (convertedStep == 0)
                        throw new InvalidOperationException("Step is too small.");

                    time += convertedStep;

                    var parts = SplitObject(tail, time);
                    yield return parts.Item1;

                    tail = parts.Item2;
                }
            }
        }

        public IEnumerable<TObject> SplitByPartsNumber(IEnumerable<TObject> objects, int partsNumber, TimeSpanType lengthType, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);
            ThrowIfArgument.IsNonpositive(nameof(partsNumber), partsNumber, "Parts number is zero or negative.");
            ThrowIfArgument.IsInvalidEnumValue(nameof(lengthType), lengthType);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            foreach (var obj in objects)
            {
                if (obj == null)
                {
                    yield return default(TObject);
                    continue;
                }

                if (partsNumber == 1)
                {
                    yield return CloneObject(obj);
                    continue;
                }

                if (obj.Length == 0)
                {
                    foreach (var i in Enumerable.Range(0, partsNumber))
                    {
                        yield return CloneObject(obj);
                    }

                    continue;
                }

                var time = obj.Time;
                var tail = CloneObject(obj);

                for (int partsRemaining = partsNumber; partsRemaining > 1 && tail != null; partsRemaining--)
                {
                    var length = tail.LengthAs(lengthType, tempoMap);
                    var partLength = length.Divide(partsRemaining);

                    time += LengthConverter.ConvertFrom(partLength, time, tempoMap);

                    var parts = SplitObject(tail, time);
                    yield return parts.Item1;

                    tail = parts.Item2;
                }

                if (tail != null)
                    yield return tail;
            }
        }

        public IEnumerable<TObject> SplitByGrid(IEnumerable<TObject> objects, ITimeSpan gridStart, IEnumerable<ITimeSpan> gridSteps, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);
            ThrowIfArgument.IsNull(nameof(gridStart), gridStart);
            ThrowIfArgument.IsNull(nameof(gridSteps), gridSteps);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            // Calculate grid times

            var lastObjectEndTime = objects.Where(o => o != null)
                                           .Select(o => o.Time + o.Length)
                                           .DefaultIfEmpty()
                                           .Max();
            var times = new List<long>();

            if (gridSteps.Any())
            {
                var time = TimeConverter.ConvertFrom(gridStart, tempoMap);
                times.Add(time);

                while (time < lastObjectEndTime)
                {
                    foreach (var step in gridSteps)
                    {
                        if (time >= lastObjectEndTime)
                            break;

                        time += LengthConverter.ConvertFrom(step, time, tempoMap);
                        times.Add(time);
                    }
                }
            }

            // Split

            return SplitByTimes(objects, times);
        }

        public IEnumerable<TObject> SplitByTimes(IEnumerable<TObject> objects, IEnumerable<ITimeSpan> times, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);
            ThrowIfArgument.IsNull(nameof(times), times);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return SplitByTimes(objects, times.Where(t => t != null).Select(t => TimeConverter.ConvertFrom(t, tempoMap)));
        }

        private IEnumerable<TObject> SplitByTimes(IEnumerable<TObject> objects, IEnumerable<long> times)
        {
            times = times.OrderBy(t => t).ToArray();

            foreach (var obj in objects)
            {
                if (obj == null)
                {
                    yield return default(TObject);
                    continue;
                }

                var startTime = obj.Time;
                var endTime = startTime + obj.Length;

                var intersectedTimes = times.SkipWhile(t => t <= startTime).TakeWhile(t => t < endTime);

                foreach (var result in SplitByTimes(obj, intersectedTimes))
                {
                    yield return result;
                }
            }
        }

        private IEnumerable<TObject> SplitByTimes(TObject obj, IEnumerable<long> times)
        {
            var tail = CloneObject(obj);

            foreach (var time in times.OrderBy(t => t))
            {
                var parts = SplitObject(tail, time);
                yield return parts.Item1;

                tail = parts.Item2;
            }

            yield return tail;
        }

        protected abstract TObject CloneObject(TObject obj);

        protected abstract Tuple<TObject, TObject> SplitObject(TObject obj, long time);

        #endregion
    }
}
