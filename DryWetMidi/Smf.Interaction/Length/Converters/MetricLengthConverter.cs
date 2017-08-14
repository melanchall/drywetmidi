using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal sealed class MetricLengthConverter : ILengthConverter
    {
        #region ILengthConverter

        public ILength ConvertTo(long length, long time, TempoMap tempoMap)
        {
            ThrowIfLengthArgument.IsNegative(nameof(length), length);
            ThrowIfTimeArgument.IsNegative(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

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
            ThrowIfArgument.IsNull(nameof(length), length);
            ThrowIfTimeArgument.IsNegative(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            var metricLength = length as MetricLength;
            if (metricLength == null)
                throw new ArgumentException($"Length is not an instance of the {nameof(MetricLength)}.", nameof(length));

            var startTime = TimeConverter.ConvertTo<MetricTime>(time, tempoMap);
            var endTime = new MetricTime(startTime.TotalMicroseconds + metricLength.TotalMicroseconds);

            return TimeConverter.ConvertFrom(endTime, tempoMap) - time;
        }

        public long ConvertFrom(ILength length, ITime time, TempoMap tempoMap)
        {
            return ConvertFrom(length, TimeConverter.ConvertFrom(time, tempoMap), tempoMap);
        }

        #endregion
    }
}
