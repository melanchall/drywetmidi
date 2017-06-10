using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal static class TimeConverterFactory
    {
        #region Fields

        private static readonly ITimeConverter _metricTimeConverter = new MetricTimeConverter();

        #endregion

        #region Methods

        internal static ITimeConverter GetConverter<TTime>()
            where TTime : ITime
        {
            if (typeof(TTime) == typeof(MetricTime))
                return _metricTimeConverter;

            throw new NotImplementedException($"Converter for {typeof(TTime)} is not implemented.");
        }

        internal static ITimeConverter GetConverter(Type timeType)
        {
            if (timeType == null)
                throw new ArgumentNullException(nameof(timeType));

            if (!typeof(ITime).IsAssignableFrom(timeType))
                throw new ArgumentException($"Time type doesn't implement {nameof(ITime)} interface.", nameof(timeType));

            if (timeType == typeof(MetricTime))
                return _metricTimeConverter;

            throw new NotImplementedException($"Converter for {timeType} is not implemented.");
        }

        #endregion
    }
}
