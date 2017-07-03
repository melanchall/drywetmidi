using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal sealed class MetricLengthConverter : ILengthConverter
    {
        #region ILengthConverter

        public ILength ConvertTo(long length, long time, TempoMap tempoMap)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "Length is negative.");

            if (time < 0)
                throw new ArgumentOutOfRangeException(nameof(time), time, "Time is negative.");

            if (tempoMap == null)
                throw new ArgumentNullException(nameof(tempoMap));

            //

            var startTime = TimeConverter.ConvertTo<MetricTime>(time, tempoMap);
            var endTime = TimeConverter.ConvertTo<MetricTime>(time + length, tempoMap);

            return new MetricLength(endTime - startTime);
        }

        public ILength ConvertTo(long length, ITime time, TempoMap tempoMap)
        {
            return ConvertTo(length, TimeConverter.ConvertFrom(time, tempoMap), tempoMap);
        }

        public long ConvertFrom(ILength length, long time, TempoMap tempoMap)
        {
            if (length == null)
                throw new ArgumentNullException(nameof(length));

            if (time < 0)
                throw new ArgumentOutOfRangeException(nameof(time), time, "Time is negative.");

            if (tempoMap == null)
                throw new ArgumentNullException(nameof(tempoMap));

            var metricLength = length as MetricLength;
            if (metricLength == null)
                throw new ArgumentException("Length is not a metric length.", nameof(length));

            var startTime = TimeConverter.ConvertTo<MetricTime>(time, tempoMap);
            var endTime = new MetricTime(startTime.TotalMicroseconds + metricLength.TotalMicroseconds);

            return TimeConverter.ConvertFrom(endTime - startTime, tempoMap);
        }

        public long ConvertFrom(ILength length, ITime time, TempoMap tempoMap)
        {
            return ConvertFrom(length, TimeConverter.ConvertFrom(time, tempoMap), tempoMap);
        }

        #endregion
    }
}
