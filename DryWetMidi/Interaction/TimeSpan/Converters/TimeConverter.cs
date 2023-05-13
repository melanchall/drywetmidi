using Melanchall.DryWetMidi.Common;
using System;
using System.ComponentModel;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Provides a way to convert the time of an object from one representation to another.
    /// More info in the <see href="xref:a_time_length">Time and length</see> article.
    /// </summary>
    public static class TimeConverter
    {
        #region Methods

        /// <summary>
        /// Converts time from <see cref="long"/> to the specified time type.
        /// </summary>
        /// <typeparam name="TTimeSpan">Type that will represent the time of an object.</typeparam>
        /// <param name="time">Time to convert.</param>
        /// <param name="tempoMap">Tempo map used to convert <paramref name="time"/>.</param>
        /// <returns>Time as an instance of <typeparamref name="TTimeSpan"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="time"/> is negative.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="tempoMap"/> is <c>null</c>.</exception>
        /// <exception cref="NotSupportedException"><typeparamref name="TTimeSpan"/> is not supported.</exception>
        public static TTimeSpan ConvertTo<TTimeSpan>(long time, TempoMap tempoMap)
            where TTimeSpan : ITimeSpan
        {
            ThrowIfTimeArgument.IsNegative(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return TimeSpanConverter.ConvertTo<TTimeSpan>(time, 0, tempoMap);
        }

        /// <summary>
        /// Converts time from <see cref="long"/> to the specified time type.
        /// </summary>
        /// <param name="time">Time to convert.</param>
        /// <param name="timeType">Type that will represent the time of an object.</param>
        /// <param name="tempoMap">Tempo map used to convert <paramref name="time"/>.</param>
        /// <returns>Time as an instance of time span defined by <paramref name="timeType"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="time"/> is negative.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="tempoMap"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="timeType"/> specified an invalid value.</exception>
        public static ITimeSpan ConvertTo(long time, TimeSpanType timeType, TempoMap tempoMap)
        {
            ThrowIfTimeArgument.IsNegative(nameof(time), time);
            ThrowIfArgument.IsInvalidEnumValue(nameof(timeType), timeType);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return TimeSpanConverter.ConvertTo(time, timeType, 0, tempoMap);
        }

        /// <summary>
        /// Converts time from one time type to another one.
        /// </summary>
        /// <typeparam name="TTimeSpan">Type that will represent the time of an object.</typeparam>
        /// <param name="time">Time to convert.</param>
        /// <param name="tempoMap">Tempo map used to convert <paramref name="time"/>.</param>
        /// <returns>Time as an instance of <typeparamref name="TTimeSpan"/>.</returns>
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
        public static TTimeSpan ConvertTo<TTimeSpan>(ITimeSpan time, TempoMap tempoMap)
            where TTimeSpan : ITimeSpan
        {
            ThrowIfArgument.IsNull(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return TimeSpanConverter.ConvertTo<TTimeSpan>(time, 0, tempoMap);
        }

        /// <summary>
        /// Converts time from one time type to another one.
        /// </summary>
        /// <param name="time">Time to convert.</param>
        /// <param name="timeType">Type that will represent the time of an object.</param>
        /// <param name="tempoMap">Tempo map used to convert <paramref name="time"/>.</param>
        /// <returns>Time as an instance of time span defined by <paramref name="timeType"/>.</returns>
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
        /// <exception cref="InvalidEnumArgumentException"><paramref name="timeType"/> specified an invalid value.</exception>
        public static ITimeSpan ConvertTo(ITimeSpan time, TimeSpanType timeType, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(time), time);
            ThrowIfArgument.IsInvalidEnumValue(nameof(timeType), timeType);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return TimeSpanConverter.ConvertTo(time, timeType, 0, tempoMap);
        }

        /// <summary>
        /// Converts time from one time type to another one.
        /// </summary>
        /// <param name="time">Time to convert.</param>
        /// <param name="timeType">Type to convert time to.</param>
        /// <param name="tempoMap">Tempo map used to convert <paramref name="time"/>.</param>
        /// <returns>Time as an instance of <paramref name="timeType"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="time"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="timeType"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="NotSupportedException"><paramref name="timeType"/> is not supported.</exception>
        public static ITimeSpan ConvertTo(ITimeSpan time, Type timeType, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(timeType), timeType);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return TimeSpanConverter.ConvertTo(time, timeType, 0, tempoMap);
        }

        /// <summary>
        /// Converts time from the specified time type to <see cref="long"/>.
        /// </summary>
        /// <param name="time">Time to convert.</param>
        /// <param name="tempoMap">Tempo map used to convert <paramref name="time"/>.</param>
        /// <returns>Time as <see cref="long"/>.</returns>
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
        public static long ConvertFrom(ITimeSpan time, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return TimeSpanConverter.ConvertFrom(time, 0, tempoMap);
        }

        #endregion
    }
}
