using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Provides a way to get <see cref="ILengthConverter"/> for a length type.
    /// </summary>
    internal static class LengthConverterFactory
    {
        #region Fields
        
        private static readonly Dictionary<Type, ILengthConverter> _converters = new Dictionary<Type, ILengthConverter>
        {
            [typeof(MetricLength)] = new MetricLengthConverter(),
            [typeof(MusicalLength)] = new MusicalLengthConverter(),
            [typeof(MathLength)] = new MathLengthConverter(),
        };

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
        /// <exception cref="NotSupportedException"><paramref name="lengthType"/> is not supported.</exception>
        internal static ILengthConverter GetConverter(Type lengthType)
        {
            ILengthConverter converter;
            if (_converters.TryGetValue(lengthType, out converter))
                return converter;

            throw new NotSupportedException($"Converter for {lengthType} is not supported.");
        }

        #endregion
    }
}
