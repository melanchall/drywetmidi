using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tools
{
    public static partial class Resizer
    {
        #region Methods

        /// <summary>
        /// Resizes group of objects to the specified length treating all objects as a single one. More info in the
        /// <see href="xref:a_resizer">Resizer</see> article.
        /// </summary>
        /// <param name="objects">Objects to resize.</param>
        /// <param name="length">New length of the objects collection.</param>
        /// <param name="tempoMap">Tempo map used to calculate length.</param>
        /// <param name="settings">Settings according to which objects should be resized.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="objects"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="length"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentException"><see cref="TimeSpanType.BarBeatTicks"/> or <see cref="TimeSpanType.BarBeatFraction"/>
        /// is used for <see cref="ObjectsGroupResizingSettings.DistanceCalculationType"/> of the <paramref name="settings"/>
        /// which is unsupported.</exception>
        public static void ResizeObjectsGroup(
            this IEnumerable<ITimedObject> objects,
            ITimeSpan length,
            TempoMap tempoMap,
            ObjectsGroupResizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);
            ThrowIfArgument.IsNull(nameof(length), length);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            settings = settings ?? new ObjectsGroupResizingSettings();

            var distanceCalculationType = settings.DistanceCalculationType;
            if (distanceCalculationType == TimeSpanType.BarBeatTicks || distanceCalculationType == TimeSpanType.BarBeatFraction)
                throw new ArgumentException("Bar/beat distance calculation type is not supported.", nameof(settings));

            var notNullObjects = objects.Where(obj => obj != null);
            if (!notNullObjects.Any())
                return;

            //

            var minTime = long.MaxValue;
            var maxEndTime = 0L;

            foreach (var obj in notNullObjects)
            {
                var time = obj.Time;
                var endTime = time + ((obj as ILengthedObject)?.Length ?? 0);

                minTime = Math.Min(minTime, time);
                maxEndTime = Math.Max(maxEndTime, endTime);
            }

            var totalLength = maxEndTime - minTime;

            //

            var oldLength = LengthConverter.ConvertTo(totalLength, distanceCalculationType, minTime, tempoMap);
            if (oldLength.IsZeroTimeSpan())
            {
                ResizeZeroLengthObjectsGroup(notNullObjects, length, tempoMap);
                return;
            }

            var newLength = LengthConverter.ConvertTo(length, distanceCalculationType, minTime, tempoMap);
            var ratio = TimeSpanUtilities.Divide(newLength, oldLength);

            var startTime = TimeConverter.ConvertTo(minTime, distanceCalculationType, tempoMap);

            ResizeObjectsGroupByRatio(notNullObjects, ratio, distanceCalculationType, tempoMap, startTime);
        }

        /// <summary>
        /// Resizes group of objects by the specified ratio treating all objects as a single object. More info in the
        /// <see href="xref:a_resizer">Resizer</see> article.
        /// </summary>
        /// <param name="objects">Objects to resize.</param>
        /// <param name="ratio">Ratio to resize objects by.</param>
        /// <param name="tempoMap">Tempo map used to calculate length.</param>
        /// <param name="settings">Settings according to which objects should be resized.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="objects"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="ratio"/> is negative.</exception>
        /// <exception cref="ArgumentException"><see cref="TimeSpanType.BarBeatTicks"/> or <see cref="TimeSpanType.BarBeatFraction"/>
        /// is used for <see cref="ObjectsGroupResizingSettings.DistanceCalculationType"/> of the <paramref name="settings"/>
        /// which is unsupported.</exception>
        public static void ResizeObjectsGroup(
            this IEnumerable<ITimedObject> objects,
            double ratio,
            TempoMap tempoMap,
            ObjectsGroupResizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);
            ThrowIfArgument.IsNegative(nameof(ratio), ratio, "Ratio is negative");
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            settings = settings ?? new ObjectsGroupResizingSettings();

            var distanceCalculationType = settings.DistanceCalculationType;
            if (distanceCalculationType == TimeSpanType.BarBeatTicks || distanceCalculationType == TimeSpanType.BarBeatFraction)
                throw new ArgumentException("BarBeat distance calculation type is not supported.", nameof(distanceCalculationType));

            var notNullObjects = objects.Where(obj => obj != null);
            if (!notNullObjects.Any())
                return;

            //

            var minStartTime = notNullObjects.Select(n => n.Time).Min();
            var startTime = TimeConverter.ConvertTo(minStartTime, distanceCalculationType, tempoMap);

            ResizeObjectsGroupByRatio(notNullObjects, ratio, distanceCalculationType, tempoMap, startTime);
        }

        private static void ResizeObjectsGroupByRatio(
            IEnumerable<ITimedObject> objects,
            double ratio,
            TimeSpanType distanceCalculationType,
            TempoMap tempoMap,
            ITimeSpan startTime)
        {
            foreach (var obj in objects)
            {
                var lengthedObject = obj as ILengthedObject;

                var length = lengthedObject?.LengthAs(distanceCalculationType, tempoMap);
                var time = obj.TimeAs(distanceCalculationType, tempoMap);

                var scaledShiftFromStart = time.Subtract(startTime, TimeSpanMode.TimeTime).Multiply(ratio);
                obj.Time = TimeConverter.ConvertFrom(startTime.Add(scaledShiftFromStart, TimeSpanMode.TimeLength), tempoMap);

                if (lengthedObject != null)
                {
                    var scaledLength = length.Multiply(ratio);
                    lengthedObject.Length = LengthConverter.ConvertFrom(scaledLength, obj.Time, tempoMap);
                }
            }
        }

        private static void ResizeZeroLengthObjectsGroup(
            IEnumerable<ITimedObject> objects,
            ITimeSpan length,
            TempoMap tempoMap)
        {
            foreach (var obj in objects)
            {
                var lengthedObject = obj as ILengthedObject;
                if (lengthedObject == null)
                    continue;

                lengthedObject.SetLength(length, tempoMap);
            }
        }

        #endregion
    }
}
