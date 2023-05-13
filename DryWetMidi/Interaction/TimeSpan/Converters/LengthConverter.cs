using Melanchall.DryWetMidi.Common;
using System;
using System.ComponentModel;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Provides a way to convert the length of an object from one representation to another.
    /// More info in the <see href="xref:a_time_length">Time and length</see> article.
    /// </summary>
    public static class LengthConverter
    {
        #region Methods

        /// <summary>
        /// Converts length from <see cref="long"/> to the specified length type.
        /// </summary>
        /// <typeparam name="TTimeSpan">Type that will represent the length of an object.</typeparam>
        /// <param name="length">Length to convert.</param>
        /// <param name="time">Start time of an object to convert length of.</param>
        /// <param name="tempoMap">Tempo map used to convert <paramref name="length"/>.</param>
        /// <returns>Length as an instance of <typeparamref name="TTimeSpan"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="length"/> is negative.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="time"/> is negative.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="tempoMap"/> is <c>null</c>.</exception>
        /// <exception cref="NotSupportedException"><typeparamref name="TTimeSpan"/> is not supported.</exception>
        public static TTimeSpan ConvertTo<TTimeSpan>(long length, long time, TempoMap tempoMap)
            where TTimeSpan : ITimeSpan
        {
            ThrowIfLengthArgument.IsNegative(nameof(length), length);
            ThrowIfTimeArgument.IsNegative(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return TimeSpanConverter.ConvertTo<TTimeSpan>(length, time, tempoMap);
        }

        /// <summary>
        /// Converts length from <see cref="long"/> to the specified length type.
        /// </summary>
        /// <param name="length">Length to convert.</param>
        /// <param name="lengthType">Type that will represent the length of an object.</param>
        /// <param name="time">Start time of an object to convert length of.</param>
        /// <param name="tempoMap">Tempo map used to convert <paramref name="length"/>.</param>
        /// <returns>Length as an instance of time span defined by <paramref name="lengthType"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="length"/> is negative.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="time"/> is negative.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="tempoMap"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="lengthType"/> specified an invalid value.</exception>
        public static ITimeSpan ConvertTo(long length, TimeSpanType lengthType, long time, TempoMap tempoMap)
        {
            ThrowIfLengthArgument.IsNegative(nameof(length), length);
            ThrowIfArgument.IsInvalidEnumValue(nameof(lengthType), lengthType);
            ThrowIfTimeArgument.IsNegative(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return TimeSpanConverter.ConvertTo(length, lengthType, time, tempoMap);
        }

        /// <summary>
        /// Converts length from <see cref="long"/> to the specified length type.
        /// </summary>
        /// <typeparam name="TTimeSpan">Type that will represent the length of an object.</typeparam>
        /// <param name="length">Length to convert.</param>
        /// <param name="time">Start time of an object to convert length of.</param>
        /// <param name="tempoMap">Tempo map used to convert <paramref name="length"/>.</param>
        /// <returns>Length as an instance of <typeparamref name="TTimeSpan"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is negative.</exception>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="time"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="NotSupportedException"><typeparamref name="TTimeSpan"/> is not supported.</exception>
        public static TTimeSpan ConvertTo<TTimeSpan>(long length, ITimeSpan time, TempoMap tempoMap)
            where TTimeSpan : ITimeSpan
        {
            ThrowIfLengthArgument.IsNegative(nameof(length), length);
            ThrowIfArgument.IsNull(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return TimeSpanConverter.ConvertTo<TTimeSpan>(length, TimeConverter.ConvertFrom(time, tempoMap), tempoMap);
        }

        /// <summary>
        /// Converts length from <see cref="long"/> to the specified length type.
        /// </summary>
        /// <param name="length">Length to convert.</param>
        /// <param name="lengthType">Type that will represent the length of an object.</param>
        /// <param name="time">Start time of an object to convert length of.</param>
        /// <param name="tempoMap">Tempo map used to convert <paramref name="length"/>.</param>
        /// <returns>Length as an instance of time span defined by <paramref name="lengthType"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is negative.</exception>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="time"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="lengthType"/> specified an invalid value.</exception>
        public static ITimeSpan ConvertTo(long length, TimeSpanType lengthType, ITimeSpan time, TempoMap tempoMap)
        {
            ThrowIfLengthArgument.IsNegative(nameof(length), length);
            ThrowIfArgument.IsInvalidEnumValue(nameof(lengthType), lengthType);
            ThrowIfArgument.IsNull(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return TimeSpanConverter.ConvertTo(length, lengthType, TimeConverter.ConvertFrom(time, tempoMap), tempoMap);
        }

        /// <summary>
        /// Converts length from one length type to another one.
        /// </summary>
        /// <typeparam name="TTimeSpan">Type that will represent the length of an object.</typeparam>
        /// <param name="length">Length to convert.</param>
        /// <param name="time">Start time of an object to convert length of.</param>
        /// <param name="tempoMap">Tempo map used to convert <paramref name="length"/>.</param>
        /// <returns>Length as an instance of <typeparamref name="TTimeSpan"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="time"/> is negative.</exception>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="length"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="NotSupportedException"><typeparamref name="TTimeSpan"/> is not supported.</exception>
        public static TTimeSpan ConvertTo<TTimeSpan>(ITimeSpan length, long time, TempoMap tempoMap)
            where TTimeSpan : ITimeSpan
        {
            ThrowIfArgument.IsNull(nameof(length), length);
            ThrowIfTimeArgument.IsNegative(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return TimeSpanConverter.ConvertTo<TTimeSpan>(length, time, tempoMap);
        }

        /// <summary>
        /// Converts length from one length type to another one.
        /// </summary>
        /// <param name="length">Length to convert.</param>
        /// <param name="lengthType">Type that will represent the length of an object.</param>
        /// <param name="time">Start time of an object to convert length of.</param>
        /// <param name="tempoMap">Tempo map used to convert <paramref name="length"/>.</param>
        /// <returns>Length as an instance of time span defined by <paramref name="lengthType"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="time"/> is negative.</exception>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="length"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="lengthType"/> specified an invalid value.</exception>
        public static ITimeSpan ConvertTo(ITimeSpan length, TimeSpanType lengthType, long time, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(length), length);
            ThrowIfArgument.IsInvalidEnumValue(nameof(lengthType), lengthType);
            ThrowIfTimeArgument.IsNegative(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return TimeSpanConverter.ConvertTo(length, lengthType, time, tempoMap);
        }

        /// <summary>
        /// Converts length from one length type to another one.
        /// </summary>
        /// <typeparam name="TTimeSpan">Type that will represent the length of an object.</typeparam>
        /// <param name="length">Length to convert.</param>
        /// <param name="time">Start time of an object to convert length of.</param>
        /// <param name="tempoMap">Tempo map used to convert <paramref name="length"/>.</param>
        /// <returns>Length as an instance of <typeparamref name="TTimeSpan"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="length"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="time"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="NotSupportedException"><typeparamref name="TTimeSpan"/> is not supported.</exception>
        public static TTimeSpan ConvertTo<TTimeSpan>(ITimeSpan length, ITimeSpan time, TempoMap tempoMap)
            where TTimeSpan : ITimeSpan
        {
            ThrowIfArgument.IsNull(nameof(length), length);
            ThrowIfArgument.IsNull(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return TimeSpanConverter.ConvertTo<TTimeSpan>(length, TimeConverter.ConvertFrom(time, tempoMap), tempoMap);
        }

        /// <summary>
        /// Converts length from one length type to another one.
        /// </summary>
        /// <param name="length">Length to convert.</param>
        /// <param name="lengthType">Type that will represent the length of an object.</param>
        /// <param name="time">Start time of an object to convert length of.</param>
        /// <param name="tempoMap">Tempo map used to convert <paramref name="length"/>.</param>
        /// <returns>Length as an instance of time span defined by <paramref name="lengthType"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="length"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="time"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="lengthType"/> specified an invalid value.</exception>
        public static ITimeSpan ConvertTo(ITimeSpan length, TimeSpanType lengthType, ITimeSpan time, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(length), length);
            ThrowIfArgument.IsInvalidEnumValue(nameof(lengthType), lengthType);
            ThrowIfArgument.IsNull(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return TimeSpanConverter.ConvertTo(length, lengthType, TimeConverter.ConvertFrom(time, tempoMap), tempoMap);
        }

        /// <summary>
        /// Converts length from one length type to another one.
        /// </summary>
        /// <param name="length">Length to convert.</param>
        /// <param name="lengthType">Type to convert <paramref name="length"/> to.</param>
        /// <param name="time">Start time of an object to convert length of.</param>
        /// <param name="tempoMap">Tempo map used to convert <paramref name="length"/>.</param>
        /// <returns>Length as an instance of <paramref name="lengthType"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="length"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="lengthType"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="time"/> is negative.</exception>
        /// <exception cref="NotSupportedException"><paramref name="lengthType"/> is not supported.</exception>
        public static ITimeSpan ConvertTo(ITimeSpan length, Type lengthType, long time, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(length), length);
            ThrowIfArgument.IsNull(nameof(lengthType), lengthType);
            ThrowIfTimeArgument.IsNegative(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return TimeSpanConverter.ConvertTo(length, lengthType, time, tempoMap);
        }

        /// <summary>
        /// Converts length from one length type to another one.
        /// </summary>
        /// <param name="length">Length to convert.</param>
        /// <param name="lengthType">Type to convert <paramref name="length"/> to.</param>
        /// <param name="time">Start time of an object to convert length of.</param>
        /// <param name="tempoMap">Tempo map used to convert <paramref name="length"/>.</param>
        /// <returns>Length as an instance of <paramref name="lengthType"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="length"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="lengthType"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="time"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="NotSupportedException"><paramref name="lengthType"/> is not supported.</exception>
        public static ITimeSpan ConvertTo(ITimeSpan length, Type lengthType, ITimeSpan time, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(length), length);
            ThrowIfArgument.IsNull(nameof(lengthType), lengthType);
            ThrowIfArgument.IsNull(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return TimeSpanConverter.ConvertTo(length, lengthType, TimeConverter.ConvertFrom(time, tempoMap), tempoMap);
        }

        /// <summary>
        /// Converts length from the specified length type to <see cref="long"/>.
        /// </summary>
        /// <param name="length">Length to convert.</param>
        /// <param name="time">Start time of an object to convert length of.</param>
        /// <param name="tempoMap">Tempo map used to convert <paramref name="length"/>.</param>
        /// <returns>Length as <see cref="long"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="time"/> is negative.</exception>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="length"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static long ConvertFrom(ITimeSpan length, long time, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(length), length);
            ThrowIfTimeArgument.IsNegative(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return TimeSpanConverter.ConvertFrom(length, time, tempoMap);
        }

        /// <summary>
        /// Converts length from the specified length type to <see cref="long"/>.
        /// </summary>
        /// <param name="length">Length to convert.</param>
        /// <param name="time">Start time of an object to convert length of.</param>
        /// <param name="tempoMap">Tempo map used to convert <paramref name="length"/>.</param>
        /// <returns>Length as <see cref="long"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="length"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="time"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static long ConvertFrom(ITimeSpan length, ITimeSpan time, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(length), length);
            ThrowIfArgument.IsNull(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return TimeSpanConverter.ConvertFrom(length, TimeConverter.ConvertFrom(time, tempoMap), tempoMap);
        }

        #endregion
    }
}
