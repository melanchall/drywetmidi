using Melanchall.DryWetMidi.Common;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Interaction
{
    // TODO: write about obsolete
    public static partial class TimedObjectUtilities
    {
        #region Methods

        public static IEnumerable<TObject> AtTime<TObject>(
            this IEnumerable<TObject?> timedObjects,
            ITimeSpan time,
            TempoMap tempoMap)
            where TObject : ITimedObject
        {
            ThrowIfArgument.IsNull(nameof(timedObjects), timedObjects);
            ThrowIfArgument.IsNull(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return timedObjects.AtTimeRange(time, time, tempoMap);
        }

        public static IEnumerable<TObject> AtTimeRange<TObject>(
            this IEnumerable<TObject?> timedObjects,
            ITimeSpan startTime,
            ITimeSpan endTime,
            TempoMap tempoMap)
            where TObject : ITimedObject
        {
            ThrowIfArgument.IsNull(nameof(timedObjects), timedObjects);
            ThrowIfArgument.IsNull(nameof(startTime), startTime);
            ThrowIfArgument.IsNull(nameof(endTime), endTime);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            var startTimeInTicks = TimeConverter.ConvertFrom(startTime, tempoMap);
            var endTimeInTicks = TimeConverter.ConvertFrom(endTime, tempoMap);

            ThrowIfArgument.IsLessThan(nameof(endTime), endTimeInTicks, startTimeInTicks, "End time is less than start time.");

            if (timedObjects is ISortedCollection)
            {
                foreach (var obj in timedObjects)
                {
                    if (obj == null)
                        continue;

                    if (obj.Time > endTimeInTicks)
                        break;

                    if (IsObjectAtTimeRange(obj, startTimeInTicks, endTimeInTicks))
                        yield return obj;
                }

                yield break;
            }

            var result = timedObjects.Where(obj =>
                obj != null && IsObjectAtTimeRange(obj, startTimeInTicks, endTimeInTicks));

            foreach (var obj in result.OfType<TObject>())
                yield return obj;
        }

        private static bool IsObjectAtTimeRange(
            ITimedObject timedObject,
            long startTime,
            long endTime)
        {
            if (timedObject is ILengthedObject lengthedObject)
                return lengthedObject.Time <= endTime && startTime <= lengthedObject.EndTime;

            return timedObject.Time >= startTime && timedObject.Time <= endTime;
        }

        #endregion
    }
}
