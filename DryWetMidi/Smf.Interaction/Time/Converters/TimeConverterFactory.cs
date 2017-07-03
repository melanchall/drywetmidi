using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Provides a way to get <see cref="ITimeConverter"/> for a time type.
    /// </summary>
    internal static class TimeConverterFactory
    {
        #region Fields

        private static readonly ITimeConverter _metricTimeConverter = new MetricTimeConverter();
        private static readonly ITimeConverter _musicalTimeConverter = new MusicalTimeConverter();

        #endregion

        #region Methods

        /// <summary>
        /// Gets converter that can be used to convert time of an object from <see cref="long"/>
        /// to the specified time type and vice versa.
        /// </summary>
        /// <typeparam name="TTime">Type that will represent the time of an object.</typeparam>
        /// <returns>Converter to convert time between <see cref="long"/> and <typeparamref name="TTime"/>.</returns>
        /// <exception cref="NotSupportedException"><typeparamref name="TTime"/> is not supported.</exception>
        internal static ITimeConverter GetConverter<TTime>()
            where TTime : ITime
        {
            return GetConverter(typeof(TTime));
        }

        /// <summary>
        /// Gets converter that can be used to convert time of an object from <see cref="long"/>
        /// to the specified time type and vice versa.
        /// </summary>
        /// <param name="timeType">Type of an object's time to get converter for.</param>
        /// <returns>Converter to convert time between <see cref="long"/> and <paramref name="timeType"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="timeType"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="timeType"/> doesn't implement <see cref="ITime"/>.</exception>
        /// <exception cref="NotSupportedException"><paramref name="timeType"/> is not supported.</exception>
        internal static ITimeConverter GetConverter(Type timeType)
        {
            if (timeType == null)
                throw new ArgumentNullException(nameof(timeType));

            if (!typeof(ITime).IsAssignableFrom(timeType))
                throw new ArgumentException($"Time type doesn't implement {nameof(ITime)} interface.", nameof(timeType));

            if (timeType == typeof(MetricTime))
                return _metricTimeConverter;
            else if (timeType == typeof(MusicalTime))
                return _musicalTimeConverter;

            throw new NotSupportedException($"Converter for {timeType} is not supported.");
        }

        #endregion
    }
}
