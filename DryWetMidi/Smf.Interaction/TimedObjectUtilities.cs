using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Extension methods for objects that implement the <see cref="ITimedObject"/> interface.
    /// </summary>
    public static class TimedObjectUtilities
    {
        #region Methods

        /// <summary>
        /// Gets time of an <see cref="ITimedObject"/> as an instance of type that implements the
        /// <see cref="ITime"/> interface.
        /// </summary>
        /// <typeparam name="TTime">Type that will represent the time of the <paramref name="obj"/>.</typeparam>
        /// <param name="obj">Object to get time of.</param>
        /// <param name="tempoMap">Tempo map to calculate time of the <paramref name="obj"/>.</param>
        /// <returns>Time of the specified object as an instance of <typeparamref name="TTime"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="obj"/> is null. -or-
        /// <paramref name="tempoMap"/> is null.</exception>
        /// <exception cref="NotSupportedException"><typeparamref name="TTime"/> is not supported.</exception>
        public static TTime TimeAs<TTime>(this ITimedObject obj, TempoMap tempoMap)
            where TTime : ITime
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            if (tempoMap == null)
                throw new ArgumentNullException(nameof(tempoMap));

            return TimeConverter.ConvertTo<TTime>(obj.Time, tempoMap);
        }

        /// <summary>
        /// Filters collection of <see cref="ITimedObject"/> to return objects at the specified time.
        /// </summary>
        /// <typeparam name="TObject">The type of the elements of <paramref name="objects"/>.</typeparam>
        /// <param name="objects">A collection to filter.</param>
        /// <param name="time">Time to filter objects by.</param>
        /// <returns>A collection that contains objects from the input sequence that are at the specified time.</returns>
        /// <remarks>
        /// Note that changes made on the objects returned by this method will not be saved to an underlying
        /// data source (events collection, track chunk, file). To change properties of timed objects and
        /// save them you need to use a manager appropriate for an object's type.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="objects"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="time"/> is negative.</exception>
        public static IEnumerable<TObject> AtTime<TObject>(this IEnumerable<TObject> objects, long time)
            where TObject : ITimedObject
        {
            if (objects == null)
                throw new ArgumentNullException(nameof(objects));

            if (time < 0)
                throw new ArgumentOutOfRangeException(nameof(time), time, "Time is negative.");

            return objects.Where(o => o.Time == time);
        }

        /// <summary>
        /// Filters collection of <see cref="ITimedObject"/> to return objects at the specified time.
        /// </summary>
        /// <typeparam name="TObject">The type of the elements of <paramref name="objects"/>.</typeparam>
        /// <param name="objects">A collection to filter.</param>
        /// <param name="time">Time to filter objects by.</param>
        /// <param name="tempoMap">Tempo map to filter <paramref name="objects"/> by <paramref name="time"/>.</param>
        /// <returns>A collection that contains objects from the input sequence that are at the specified time.</returns>
        /// <remarks>
        /// Note that changes made on the objects returned by this method will not be saved to an underlying
        /// data source (events collection, track chunk, file). To change properties of timed objects and
        /// save them you need to use a manager appropriate for an object's type.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="objects"/> is null. -or- <paramref name="time"/> is null. -or-
        /// <paramref name="tempoMap"/> is null.</exception>
        public static IEnumerable<TObject> AtTime<TObject>(this IEnumerable<TObject> objects, ITime time, TempoMap tempoMap)
            where TObject : ITimedObject
        {
            if (objects == null)
                throw new ArgumentNullException(nameof(objects));

            if (time == null)
                throw new ArgumentNullException(nameof(time));

            if (tempoMap == null)
                throw new ArgumentNullException(nameof(tempoMap));

            var convertedTime = TimeConverter.ConvertFrom(time, tempoMap);
            return AtTime(objects, convertedTime);
        }

        #endregion
    }
}
