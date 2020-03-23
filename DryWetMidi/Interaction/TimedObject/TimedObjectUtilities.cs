using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Extension methods for objects that implement the <see cref="ITimedObject"/> interface.
    /// </summary>
    public static class TimedObjectUtilities
    {
        #region Methods

        /// <summary>
        /// Gets time of an <see cref="ITimedObject"/> as an instance of type that implements the
        /// <see cref="ITimeSpan"/> interface.
        /// </summary>
        /// <typeparam name="TTime">Type that will represent the time of the <paramref name="obj"/>.</typeparam>
        /// <param name="obj">Object to get time of.</param>
        /// <param name="tempoMap">Tempo map to calculate time of the <paramref name="obj"/>.</param>
        /// <returns>Time of the specified object as an instance of <typeparamref name="TTime"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
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
        public static TTime TimeAs<TTime>(this ITimedObject obj, TempoMap tempoMap)
            where TTime : ITimeSpan
        {
            ThrowIfArgument.IsNull(nameof(obj), obj);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return TimeConverter.ConvertTo<TTime>(obj.Time, tempoMap);
        }

        /// <summary>
        /// Gets time of an <see cref="ITimedObject"/> as an instance of time span defined by the
        /// specified time span type.
        /// </summary>
        /// <param name="obj">Object to get time of.</param>
        /// <param name="timeType">The type of time span to convert the time of <paramref name="obj"/> to.</param>
        /// <param name="tempoMap">Tempo map to calculate time of the <paramref name="obj"/>.</param>
        /// <returns>Time of the specified object as an instance of time span defined by the
        /// <paramref name="timeType"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
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
        public static ITimeSpan TimeAs(this ITimedObject obj, TimeSpanType timeType, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(obj), obj);
            ThrowIfArgument.IsInvalidEnumValue(nameof(timeType), timeType);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return TimeConverter.ConvertTo(obj.Time, timeType, tempoMap);
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
        /// <exception cref="ArgumentNullException"><paramref name="objects"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="time"/> is negative.</exception>
        public static IEnumerable<TObject> AtTime<TObject>(this IEnumerable<TObject> objects, long time)
            where TObject : ITimedObject
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);
            ThrowIfTimeArgument.IsNegative(nameof(time), time);

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
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
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
        /// </list>
        /// </exception>
        public static IEnumerable<TObject> AtTime<TObject>(this IEnumerable<TObject> objects, ITimeSpan time, TempoMap tempoMap)
            where TObject : ITimedObject
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);
            ThrowIfArgument.IsNull(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            var convertedTime = TimeConverter.ConvertFrom(time, tempoMap);
            return AtTime(objects, convertedTime);
        }

        #endregion
    }
}
