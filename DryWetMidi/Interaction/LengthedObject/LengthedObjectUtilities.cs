using Melanchall.DryWetMidi.Common;
using System;
using System.ComponentModel;

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

        #endregion
    }
}
