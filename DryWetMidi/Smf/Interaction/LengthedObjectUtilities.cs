using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Extension methods for objects that implement the <see cref="ILengthedObject"/> interface.
    /// </summary>
    public static class LengthedObjectUtilities
    {
        #region Methods

        /// <summary>
        /// Gets length of an <see cref="ILengthedObject"/> as an instance of type that
        /// implements the <see cref="ILength"/> interface.
        /// </summary>
        /// <typeparam name="TLength">Type that will represent the length of the <paramref name="obj"/>.</typeparam>
        /// <param name="obj">Object to get length of.</param>
        /// <param name="tempoMap">Tempo map to calculate length of the <paramref name="obj"/>.</param>
        /// <returns>Length of the specified object as an instance of <typeparamref name="TLength"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="obj"/> is null. -or-
        /// <paramref name="tempoMap"/> is null.</exception>
        /// <exception cref="NotSupportedException"><typeparamref name="TLength"/> is not supported.</exception>
        public static TLength LengthAs<TLength>(this ILengthedObject obj, TempoMap tempoMap)
            where TLength : ILength
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            if (tempoMap == null)
                throw new ArgumentNullException(nameof(tempoMap));

            return LengthConverter.ConvertTo<TLength>(obj.Length, obj.Time, tempoMap);
        }

        /// <summary>
        /// Filters collection of <see cref="ILengthedObject"/> to return objects that start at the specified time.
        /// </summary>
        /// <typeparam name="TObject">The type of the elements of <paramref name="objects"/>.</typeparam>
        /// <param name="objects">A collection to filter.</param>
        /// <param name="time">Start time to filter objects by.</param>
        /// <returns>A collection that contains objects from the input sequence that start at the specified time.</returns>
        /// <remarks>
        /// Note that changes made on the objects returned by this method will not be saved to an underlying
        /// data source (events collection, track chunk, file). To change properties of lengthed objects and
        /// save them you need to use a manager appropriate for an object's type.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="objects"/> is null. -or- One of the objects is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="time"/> is negative.</exception>
        public static IEnumerable<TObject> StartAtTime<TObject>(this IEnumerable<TObject> objects, long time)
            where TObject : ILengthedObject
        {
            return AtTime(objects, time, LengthedObjectPart.Start);
        }

        /// <summary>
        /// Filters collection of <see cref="ILengthedObject"/> to return objects that end at the specified time.
        /// </summary>
        /// <typeparam name="TObject">The type of the elements of <paramref name="objects"/>.</typeparam>
        /// <param name="objects">A collection to filter.</param>
        /// <param name="time">End time to filter objects by.</param>
        /// <returns>A collection that contains objects from the input sequence that end at the specified time.</returns>
        /// <remarks>
        /// Note that changes made on the objects returned by this method will not be saved to an underlying
        /// data source (events collection, track chunk, file). To change properties of lengthed objects and
        /// save them you need to use a manager appropriate for an object's type.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="objects"/> is null. -or- One of the objects is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="time"/> is negative.</exception>
        public static IEnumerable<TObject> EndAtTime<TObject>(this IEnumerable<TObject> objects, long time)
            where TObject : ILengthedObject
        {
            return AtTime(objects, time, LengthedObjectPart.End);
        }

        /// <summary>
        /// Filters collection of <see cref="ILengthedObject"/> to return objects that start at the specified time.
        /// </summary>
        /// <typeparam name="TObject">The type of the elements of <paramref name="objects"/>.</typeparam>
        /// <param name="objects">A collection to filter.</param>
        /// <param name="time">Start time to filter objects by.</param>
        /// <param name="tempoMap">Tempo map to filter <paramref name="objects"/> by <paramref name="time"/>.</param>
        /// <returns>A collection that contains objects from the input sequence that start at the specified time.</returns>
        /// <remarks>
        /// Note that changes made on the objects returned by this method will not be saved to an underlying
        /// data source (events collection, track chunk, file). To change properties of lengthed objects and
        /// save them you need to use a manager appropriate for an object's type.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="objects"/> is null. -or- <paramref name="time"/> is null. -or-
        /// <paramref name="tempoMap"/> is null. -or- One of the objects is null.</exception>
        public static IEnumerable<TObject> StartAtTime<TObject>(this IEnumerable<TObject> objects, ITime time, TempoMap tempoMap)
            where TObject : ILengthedObject
        {
            return AtTime(objects, time, tempoMap, LengthedObjectPart.Start);
        }

        /// <summary>
        /// Filters collection of <see cref="ILengthedObject"/> to return objects that end at the specified time.
        /// </summary>
        /// <typeparam name="TObject">The type of the elements of <paramref name="objects"/>.</typeparam>
        /// <param name="objects">A collection to filter.</param>
        /// <param name="time">End time to filter objects by.</param>
        /// <param name="tempoMap">Tempo map to filter <paramref name="objects"/> by <paramref name="time"/>.</param>
        /// <returns>A collection that contains objects from the input sequence that end at the specified time.</returns>
        /// <remarks>
        /// Note that changes made on the objects returned by this method will not be saved to an underlying
        /// data source (events collection, track chunk, file). To change properties of lengthed objects and
        /// save them you need to use a manager appropriate for an object's type.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="objects"/> is null. -or- <paramref name="time"/> is null. -or-
        /// <paramref name="tempoMap"/> is null. -or- One of the objects is null.</exception>
        public static IEnumerable<TObject> EndAtTime<TObject>(this IEnumerable<TObject> objects, ITime time, TempoMap tempoMap)
            where TObject : ILengthedObject
        {
            return AtTime(objects, time, tempoMap, LengthedObjectPart.End);
        }

        /// <summary>
        /// Filters collection of <see cref="ILengthedObject"/> to return objects at the specified time.
        /// </summary>
        /// <typeparam name="TObject">The type of the elements of <paramref name="objects"/>.</typeparam>
        /// <param name="objects">A collection to filter.</param>
        /// <param name="time">Time to filter objects by.</param>
        /// <param name="matchBy">Part of an object which have to be at <paramref name="time"/>.</param>
        /// <returns>A collection that contains objects from the input sequence that are at the specified time.</returns>
        /// <remarks>
        /// Note that changes made on the objects returned by this method will not be saved to an underlying
        /// data source (events collection, track chunk, file). To change properties of lengthed objects and
        /// save them you need to use a manager appropriate for an object's type.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="objects"/> is null. -or- One of the objects is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="time"/> is negative.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="matchBy"/> specified an invalid value.</exception>
        public static IEnumerable<TObject> AtTime<TObject>(this IEnumerable<TObject> objects, long time, LengthedObjectPart matchBy)
            where TObject : ILengthedObject
        {
            if (objects == null)
                throw new ArgumentNullException(nameof(objects));

            if (time < 0)
                throw new ArgumentOutOfRangeException(nameof(time), time, "Time is negative.");

            if (!Enum.IsDefined(typeof(LengthedObjectPart), matchBy))
                throw new InvalidEnumArgumentException(nameof(matchBy), (int)matchBy, typeof(LengthedObjectPart));

            return objects.Where(o => IsObjectAtTime(o, time, matchBy));
        }

        /// <summary>
        /// Filters collection of <see cref="ILengthedObject"/> to return objects at the specified time.
        /// </summary>
        /// <typeparam name="TObject">The type of the elements of <paramref name="objects"/>.</typeparam>
        /// <param name="objects">A collection to filter.</param>
        /// <param name="time">Time to filter objects by.</param>
        /// <param name="tempoMap">Tempo map to filter <paramref name="objects"/> by <paramref name="time"/>.</param>
        /// <param name="matchBy">Part of an object which have to be at <paramref name="time"/>.</param>
        /// <returns>A collection that contains objects from the input sequence that are at the specified time.</returns>
        /// <remarks>
        /// Note that changes made on the objects returned by this method will not be saved to an underlying
        /// data source (events collection, track chunk, file). To change properties of lengthed objects and
        /// save them you need to use a manager appropriate for an object's type.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="objects"/> is null. -or- <paramref name="time"/> is null. -or-
        /// <paramref name="tempoMap"/> is null. -or- One of the objects is null.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="matchBy"/> specified an invalid value.</exception>
        public static IEnumerable<TObject> AtTime<TObject>(this IEnumerable<TObject> objects, ITime time, TempoMap tempoMap, LengthedObjectPart matchBy)
            where TObject : ILengthedObject
        {
            if (objects == null)
                throw new ArgumentNullException(nameof(objects));

            if (time == null)
                throw new ArgumentNullException(nameof(time));

            if (tempoMap == null)
                throw new ArgumentNullException(nameof(tempoMap));

            if (!Enum.IsDefined(typeof(LengthedObjectPart), matchBy))
                throw new InvalidEnumArgumentException(nameof(matchBy), (int)matchBy, typeof(LengthedObjectPart));

            var convertedTime = TimeConverter.ConvertFrom(time, tempoMap);
            return AtTime(objects, convertedTime, matchBy);
        }

        /// <summary>
        /// Checks if an <see cref="ILengthedObject"/> is at the specified time.
        /// </summary>
        /// <typeparam name="TObject">Type of an object.</typeparam>
        /// <param name="obj"><see cref="ILengthedObject"/> to check.</param>
        /// <param name="time">Time to check the <paramref name="obj"/>.</param>
        /// <param name="matchBy">Part of the <paramref name="obj"/> which have to be at <paramref name="time"/>.</param>
        /// <returns>true if <paramref name="obj"/> is at <paramref name="time"/>; false - otherwise.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="obj"/> is null.</exception>
        private static bool IsObjectAtTime<TObject>(TObject obj, long time, LengthedObjectPart matchBy)
            where TObject : ILengthedObject
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var startTime = obj.Time;
            if (startTime == time && (matchBy == LengthedObjectPart.Start || matchBy == LengthedObjectPart.Entire))
                return true;

            var endTime = startTime + obj.Length;
            if (endTime == time && (matchBy == LengthedObjectPart.End || matchBy == LengthedObjectPart.Entire))
                return true;

            return matchBy == LengthedObjectPart.Entire && time >= startTime && time <= endTime;
        }

        #endregion
    }
}
