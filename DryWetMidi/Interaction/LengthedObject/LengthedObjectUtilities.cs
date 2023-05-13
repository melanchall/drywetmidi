using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Extension methods for objects that implement the <see cref="ILengthedObject"/> interface.
    /// </summary>
    public static class LengthedObjectUtilities
    {
        #region Methods

        /// <summary>
        /// Gets length of an <see cref="ILengthedObject"/> as an instance of type that
        /// implements the <see cref="ITimeSpan"/> interface.
        /// </summary>
        /// <typeparam name="TLength">Type that will represent the length of the <paramref name="obj"/>.</typeparam>
        /// <param name="obj">Object to get length of.</param>
        /// <param name="tempoMap">Tempo map to calculate length of the <paramref name="obj"/>.</param>
        /// <returns>Length of the specified object as an instance of <typeparamref name="TLength"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="obj"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="NotSupportedException"><typeparamref name="TLength"/> is not supported.</exception>
        public static TLength LengthAs<TLength>(this ILengthedObject obj, TempoMap tempoMap)
            where TLength : ITimeSpan
        {
            ThrowIfArgument.IsNull(nameof(obj), obj);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return LengthConverter.ConvertTo<TLength>(obj.Length, obj.Time, tempoMap);
        }

        /// <summary>
        /// Gets length of an <see cref="ILengthedObject"/> as an instance of type defined by the
        /// specified time span type.
        /// </summary>
        /// <param name="obj">Object to get length of.</param>
        /// <param name="lengthType">The type of time span to convert the length of <paramref name="obj"/> to.</param>
        /// <param name="tempoMap">Tempo map to calculate length of the <paramref name="obj"/>.</param>
        /// <returns>Time of the specified object as an instance of time span defined by the
        /// <paramref name="lengthType"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="obj"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="lengthType"/> specified an invalid value.</exception>
        public static ITimeSpan LengthAs(this ILengthedObject obj, TimeSpanType lengthType, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(obj), obj);
            ThrowIfArgument.IsInvalidEnumValue(nameof(lengthType), lengthType);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return LengthConverter.ConvertTo(obj.Length, lengthType, obj.Time, tempoMap);
        }

        /// <summary>
        /// Gets end time of an <see cref="ITimedObject"/> as an instance of type that implements the
        /// <see cref="ITimeSpan"/> interface.
        /// </summary>
        /// <typeparam name="TTime">Type that will represent the end time of the <paramref name="obj"/>.</typeparam>
        /// <param name="obj">Object to get end time of.</param>
        /// <param name="tempoMap">Tempo map to calculate end time of the <paramref name="obj"/>.</param>
        /// <returns>End time of the specified object as an instance of <typeparamref name="TTime"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="obj"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="NotSupportedException"><typeparamref name="TTime"/> is not supported.</exception>
        public static TTime EndTimeAs<TTime>(this ILengthedObject obj, TempoMap tempoMap)
            where TTime : ITimeSpan
        {
            ThrowIfArgument.IsNull(nameof(obj), obj);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return TimeConverter.ConvertTo<TTime>(obj.EndTime, tempoMap);
        }

        /// <summary>
        /// Gets end time of an <see cref="ITimedObject"/> as an instance of time span defined by the
        /// specified time span type.
        /// </summary>
        /// <param name="obj">Object to get end time of.</param>
        /// <param name="timeType">The type of time span to convert the end time of <paramref name="obj"/> to.</param>
        /// <param name="tempoMap">Tempo map to calculate end time of the <paramref name="obj"/>.</param>
        /// <returns>End time of the specified object as an instance of time span defined by the
        /// <paramref name="timeType"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="obj"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="timeType"/> specified an invalid value.</exception>
        public static ITimeSpan EndTimeAs(this ILengthedObject obj, TimeSpanType timeType, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(obj), obj);
            ThrowIfArgument.IsInvalidEnumValue(nameof(timeType), timeType);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return TimeConverter.ConvertTo(obj.EndTime, timeType, tempoMap);
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
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="objects"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description>One of the objects is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
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
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="objects"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description>One of the objects is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
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
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="objects"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="time"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description>One of the objects is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static IEnumerable<TObject> StartAtTime<TObject>(this IEnumerable<TObject> objects, ITimeSpan time, TempoMap tempoMap)
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
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="objects"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="time"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description>One of the objects is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static IEnumerable<TObject> EndAtTime<TObject>(this IEnumerable<TObject> objects, ITimeSpan time, TempoMap tempoMap)
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
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="objects"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description>One of the objects is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="time"/> is negative.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="matchBy"/> specified an invalid value.</exception>
        public static IEnumerable<TObject> AtTime<TObject>(this IEnumerable<TObject> objects, long time, LengthedObjectPart matchBy)
            where TObject : ILengthedObject
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);
            ThrowIfTimeArgument.IsNegative(nameof(time), time);
            ThrowIfArgument.IsInvalidEnumValue(nameof(matchBy), matchBy);

            return objects.Where(o => o != null && IsObjectAtTime(o, time, matchBy));
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
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="objects"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="time"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description>One of the objects is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="matchBy"/> specified an invalid value.</exception>
        public static IEnumerable<TObject> AtTime<TObject>(this IEnumerable<TObject> objects, ITimeSpan time, TempoMap tempoMap, LengthedObjectPart matchBy)
            where TObject : ILengthedObject
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);
            ThrowIfArgument.IsNull(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsInvalidEnumValue(nameof(matchBy), matchBy);

            var convertedTime = TimeConverter.ConvertFrom(time, tempoMap);
            return AtTime(objects, convertedTime, matchBy);
        }

        /// <summary>
        /// Sets length of the specified object in terms of <see cref="ITimeSpan"/>.
        /// </summary>
        /// <typeparam name="TObject">The type of the <paramref name="obj"/>.</typeparam>
        /// <param name="obj">Object to set length of.</param>
        /// <param name="length">New length of the <paramref name="obj"/>.</param>
        /// <param name="tempoMap"><see cref="TempoMap"/> used to calculate new length in ticks.</param>
        /// <returns>The same object the method was called on.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="obj"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="length"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description>One of the objects is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static TObject SetLength<TObject>(this TObject obj, ITimeSpan length, TempoMap tempoMap)
            where TObject : ILengthedObject
        {
            ThrowIfArgument.IsNull(nameof(obj), obj);
            ThrowIfArgument.IsNull(nameof(length), length);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            obj.Length = LengthConverter.ConvertFrom(length, obj.Time, tempoMap);
            return obj;
        }

        /// <summary>
        /// Checks if an <see cref="ILengthedObject"/> is at the specified time.
        /// </summary>
        /// <typeparam name="TObject">Type of an object.</typeparam>
        /// <param name="obj"><see cref="ILengthedObject"/> to check.</param>
        /// <param name="time">Time to check the <paramref name="obj"/>.</param>
        /// <param name="matchBy">Part of the <paramref name="obj"/> which have to be at <paramref name="time"/>.</param>
        /// <returns><c>true</c> if <paramref name="obj"/> is at <paramref name="time"/>; <c>false</c> - otherwise.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="obj"/> is <c>null</c>.</exception>
        private static bool IsObjectAtTime<TObject>(TObject obj, long time, LengthedObjectPart matchBy)
            where TObject : ILengthedObject
        {
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
