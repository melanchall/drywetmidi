using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Provides a way to get <see cref="ILengthConverter"/> for a length type.
    /// </summary>
    internal static class LengthConverterFactory
    {
        #region Fields

        private static readonly ILengthConverter _metricLengthConverter = new MetricLengthConverter();

        #endregion

        #region Methods

        /// <summary>
        /// Gets converter that can be used to convert length of an object from <see cref="long"/>
        /// to the specified length type and vice versa.
        /// </summary>
        /// <typeparam name="TLength">Type that will represent the length of an object.</typeparam>
        /// <returns>Converter to convert length between <see cref="long"/> and <typeparamref name="TLength"/>.</returns>
        /// <exception cref="NotSupportedException"><typeparamref name="TLength"/> is not supported.</exception>
        internal static ILengthConverter GetConverter<TLength>()
            where TLength : ILength
        {
            return GetConverter(typeof(TLength));
        }

        /// <summary>
        /// Gets converter that can be used to convert length of an object from <see cref="long"/>
        /// to the specified length type and vice versa.
        /// </summary>
        /// <param name="lengthType">Type of an object's length to get converter for.</param>
        /// <returns>Converter to convert length between <see cref="long"/> and <paramref name="lengthType"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="lengthType"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="lengthType"/> doesn't implement <see cref="ILength"/>.</exception>
        /// <exception cref="NotSupportedException"><paramref name="lengthType"/> is not supported.</exception>
        internal static ILengthConverter GetConverter(Type lengthType)
        {
            if (lengthType == null)
                throw new ArgumentNullException(nameof(lengthType));

            if (!typeof(ILength).IsAssignableFrom(lengthType))
                throw new ArgumentException($"Length type doesn't implement {nameof(ILength)} interface.", nameof(lengthType));

            if (lengthType == typeof(MetricLength))
                return _metricLengthConverter;

            throw new NotSupportedException($"Converter for {lengthType} is not supported.");
        }

        #endregion
    }
}
