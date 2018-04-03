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

        public IEnumerable<TObject> SplitByGrid(IEnumerable<TObject> objects, IGrid grid, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);
            ThrowIfArgument.IsNull(nameof(grid), grid);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            var lastObjectEndTime = objects.Where(o => o != null)
                                           .Select(o => o.Time + o.Length)
                                           .DefaultIfEmpty()
                                           .Max();
            var times = grid.GetTimes(tempoMap)
                            .TakeWhile(t => t < lastObjectEndTime)
                            .Distinct()
                            .ToList();
            times.Sort();

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
                var tail = CloneObject(obj);

                foreach (var time in intersectedTimes)
                {
                    var parts = SplitObject(tail, time);
                    yield return parts.Item1;

                    tail = parts.Item2;
                }

                yield return tail;
            }
        }

        protected abstract TObject CloneObject(TObject obj);

        protected abstract Tuple<TObject, TObject> SplitObject(TObject obj, long time);

        #endregion
    }
}
