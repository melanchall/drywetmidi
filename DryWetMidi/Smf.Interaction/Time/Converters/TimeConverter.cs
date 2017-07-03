using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Provides a way to convert the time of an object from one representation to another.
    /// </summary>
    public static class TimeConverter
    {
        #region Methods

        /// <summary>
        /// Converts time from <see cref="long"/> to the specified time type.
        /// </summary>
        /// <typeparam name="TTime">Type that will represent the time of an object.</typeparam>
        /// <param name="time">Time to convert.</param>
        /// <param name="tempoMap">Tempo map used to convert <paramref name="time"/>.</param>
        /// <returns>Time as an instance of <typeparamref name="TTime"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="time"/> is negative.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="tempoMap"/> is null.</exception>
        /// <exception cref="NotSupportedException"><typeparamref name="TTime"/> is not supported.</exception>
        public static TTime ConvertTo<TTime>(long time, TempoMap tempoMap)
            where TTime : ITime
        {
            if (time < 0)
                throw new ArgumentOutOfRangeException(nameof(time), time, "Time is negative.");

            if (tempoMap == null)
                throw new ArgumentNullException(nameof(tempoMap));

            return (TTime)GetConverter<TTime>().ConvertTo(time, tempoMap);
        }

        /// <summary>
        /// Converts time from one time type to another one.
        /// </summary>
        /// <typeparam name="TTime">Type that will represent the time of an object.</typeparam>
        /// <param name="time">Time to convert.</param>
        /// <param name="tempoMap">Tempo map used to convert <paramref name="time"/>.</param>
        /// <returns>Time as an instance of <typeparamref name="TTime"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="time"/> is null. -or-
        /// <paramref name="tempoMap"/> is null.</exception>
        /// <exception cref="NotSupportedException"><typeparamref name="TTime"/> is not supported.</exception>
        public static TTime ConvertTo<TTime>(ITime time, TempoMap tempoMap)
            where TTime : ITime
        {
            if (time == null)
                throw new ArgumentNullException(nameof(time));

            if (tempoMap == null)
                throw new ArgumentNullException(nameof(tempoMap));

            return ConvertTo<TTime>(ConvertFrom(time, tempoMap), tempoMap);
        }

        /// <summary>
        /// Converts time from the specified time type to <see cref="long"/>.
        /// </summary>
        /// <param name="time">Time to convert.</param>
        /// <param name="tempoMap">Tempo map used to convert <paramref name="time"/>.</param>
        /// <returns>Time as <see cref="long"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="time"/> is null. -or-
        /// <paramref name="tempoMap"/> is null.</exception>
        public static long ConvertFrom(ITime time, TempoMap tempoMap)
        {
            if (time == null)
                throw new ArgumentNullException(nameof(time));

            if (tempoMap == null)
                throw new ArgumentNullException(nameof(tempoMap));

            return TimeConverterFactory.GetConverter(time.GetType())
                                       .ConvertFrom(time, tempoMap);
        }

        /// <summary>
        /// Gets converter that can be used to convert time of an object from <see cref="long"/>
        /// to the specified time type and vice versa.
        /// </summary>
        /// <typeparam name="TTime">Type that will represent the time of an object.</typeparam>
        /// <returns>Converter to convert time between <see cref="long"/> and <typeparamref name="TTime"/>.</returns>
        /// <exception cref="NotSupportedException"><typeparamref name="TTime"/> is not supported.</exception>
        public static ITimeConverter GetConverter<TTime>()
            where TTime : ITime
        {
            return TimeConverterFactory.GetConverter<TTime>();
        }

        #endregion
    }
}
