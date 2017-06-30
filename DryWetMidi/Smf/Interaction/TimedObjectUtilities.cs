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
        public static TTime TimeAs<TTime>(this ITimedObject obj, TempoMap tempoMap)
            where TTime : ITime
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            if (tempoMap == null)
                throw new ArgumentNullException(nameof(tempoMap));

            return TimeConverter.ConvertTo<TTime>(obj.Time, tempoMap);
        }

        public static IEnumerable<TObject> AtTime<TObject>(this IEnumerable<TObject> objects, long time)
            where TObject : ITimedObject
        {
            if (objects == null)
                throw new ArgumentNullException(nameof(objects));

            if (time < 0)
                throw new ArgumentOutOfRangeException(nameof(time), time, "Time is negative.");

            return objects.Where(o => o.Time == time);
        }

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
