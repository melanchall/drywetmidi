using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal static class TimeConverterFactory
    {
        #region Fields

        private static readonly ITimeConverter _metricTimeConverter = new MetricTimeConverter();
        private static readonly ITimeConverter _musicalTimeConverter = new MusicalTimeConverter();

        #endregion

        #region Methods

        internal static ITimeConverter GetConverter<TTime>()
            where TTime : ITime
        {
            return GetConverter(typeof(TTime));
        }

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
