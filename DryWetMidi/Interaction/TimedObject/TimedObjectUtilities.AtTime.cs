using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Interaction
{
    // TODO: write about obsolete
    public static partial class TimedObjectUtilities
    {
        #region Methods

        /// <summary>
        /// Gets timed objects that occur at the specified time.
        /// </summary>
        /// <typeparam name="TObject">The type of timed objects to return.</typeparam>
        /// <param name="timedObjects">The collection of timed objects to search.</param>
        /// <param name="time">The time at which to find objects.</param>
        /// <param name="tempoMap">The tempo map used to convert <paramref name="time"/> to MIDI ticks.</param>
        /// <returns>The objects that occur at <paramref name="time"/>.</returns>
        /// <exception cref="ArgumentNullException">An argument is <c>null</c>.</exception>
        /// <example>
        /// <para>
        /// For <c>timedObjects.AtTime(new MidiTimeSpan(120), tempoMap)</c>, a timed event is returned if its
        /// <see cref="ITimedObject.Time"/> is 120. A <see cref="Note"/> is returned if it spans tick 120; for
        /// example, a note starting at tick 100 with length 40 is returned because it ends at tick 140.
        /// </para>
        /// </example>
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

        /// <summary>
        /// Gets timed objects that occur within the specified time range.
        /// </summary>
        /// <typeparam name="TObject">The type of timed objects to return.</typeparam>
        /// <param name="timedObjects">The collection of timed objects to search.</param>
        /// <param name="startTime">The inclusive start of the time range.</param>
        /// <param name="endTime">The inclusive end of the time range.</param>
        /// <param name="tempoMap">The tempo map used to convert the time range to MIDI ticks.</param>
        /// <returns>The objects that occur within the specified time range.</returns>
        /// <exception cref="ArgumentNullException">An argument is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="endTime"/> is earlier than <paramref name="startTime"/>.</exception>
        /// <example>
        /// <para>
        /// For <c>timedObjects.AtTimeRange(new MidiTimeSpan(100), new MidiTimeSpan(200), tempoMap)</c>, a timed event
        /// is returned if its time is between ticks 100 and 200, inclusive. A <see cref="Note"/> is returned if any
        /// part of it overlaps that range; for example, a note starting at tick 80 with length 30 is returned because
        /// it ends at tick 110, within the range. A note ending before tick 100 or starting after tick 200 is not returned.
        /// </para>
        /// </example>
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
