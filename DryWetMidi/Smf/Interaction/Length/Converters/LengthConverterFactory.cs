using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal static class LengthConverterFactory
    {
        #region Fields

        private static readonly ILengthConverter _metricLengthConverter = new MetricLengthConverter();

        #endregion

        #region Methods

        internal static ILengthConverter GetConverter<TLength>()
            where TLength : ILength
        {
            return GetConverter(typeof(TLength));
        }

        internal static ILengthConverter GetConverter(Type lengthType)
        {
            if (lengthType == null)
                throw new ArgumentNullException(nameof(lengthType));

            if (!typeof(ILength).IsAssignableFrom(lengthType))
                throw new ArgumentException($"Length type doesn't implement {nameof(ILength)} interface.", nameof(lengthType));

            if (lengthType == typeof(MetricLength))
                return _metricLengthConverter;

            throw new NotImplementedException($"Converter for {lengthType} is not implemented.");
        }

        #endregion
    }
}
