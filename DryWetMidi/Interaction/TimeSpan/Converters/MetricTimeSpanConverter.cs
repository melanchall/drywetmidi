using System;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class MetricTimeSpanConverter : ITimeSpanConverter
    {
        #region ITimeSpanConverter

        public ITimeSpan ConvertTo(long timeSpan, long time, TempoMap tempoMap)
        {
            var ticksPerQuarterNoteTimeDivision = tempoMap.TimeDivision as TicksPerQuarterNoteTimeDivision;
            if (ticksPerQuarterNoteTimeDivision == null)
                throw new ArgumentException("Time division is not supported for time span conversion.", nameof(tempoMap));

            if (timeSpan == 0)
                return new MetricTimeSpan();

            var startTimeSpan = TicksToMetricTimeSpan(time, tempoMap);
            var endTimeSpan = TicksToMetricTimeSpan(time + timeSpan, tempoMap);

            return endTimeSpan - startTimeSpan;
        }

        public long ConvertFrom(ITimeSpan timeSpan, long time, TempoMap tempoMap)
        {
            var ticksPerQuarterNoteTimeDivision = tempoMap.TimeDivision as TicksPerQuarterNoteTimeDivision;
            if (ticksPerQuarterNoteTimeDivision == null)
                throw new ArgumentException("Time division is not supported for time span conversion.", nameof(tempoMap));

            var metricTimeSpan = (MetricTimeSpan)timeSpan;
            if ((TimeSpan)metricTimeSpan == TimeSpan.Zero)
                return 0;

            var startTimeSpan = TicksToMetricTimeSpan(time, tempoMap);
            var endTimeSpan = startTimeSpan + metricTimeSpan;

            return MetricTimeSpanToTicks(endTimeSpan, tempoMap) - time;
        }

        #endregion

        #region Methods

        private static MetricTimeSpan TicksToMetricTimeSpan(long timeSpan, TempoMap tempoMap)
        {
            if (timeSpan == 0)
                return new MetricTimeSpan();

            var valuesCache = tempoMap.GetValuesCache<MetricTempoMapValuesCache>();
            var accumulatedMicroseconds = MathUtilities.GetLastElementBelowThreshold(valuesCache.Microseconds, timeSpan, m => m.Time);

            var lastAccumulatedMicroseconds = accumulatedMicroseconds?.Microseconds ?? 0;
            var lastTime = accumulatedMicroseconds?.Time ?? 0;
            var lastMicrosecondsPerTick = accumulatedMicroseconds?.MicrosecondsPerTick ?? valuesCache.DefaultMicrosecondsPerTick;

            var totalMicroseconds = lastAccumulatedMicroseconds + GetMicroseconds(timeSpan - lastTime, lastMicrosecondsPerTick);
            if (totalMicroseconds > long.MaxValue)
                throw new InvalidOperationException("Time span is too big.");

            return new MetricTimeSpan(RoundMicroseconds(totalMicroseconds));
        }

        private static long MetricTimeSpanToTicks(MetricTimeSpan timeSpan, TempoMap tempoMap)
        {
            var timeMicroseconds = timeSpan.TotalMicroseconds;
            if (timeMicroseconds == 0)
                return 0;

            var valuesCache = tempoMap.GetValuesCache<MetricTempoMapValuesCache>();
            var accumulatedMicroseconds = valuesCache.Microseconds.TakeWhile(m => m.Microseconds < timeMicroseconds).LastOrDefault();

            var lastAccumulatedMicroseconds = accumulatedMicroseconds?.Microseconds ?? 0;
            var lastTime = accumulatedMicroseconds?.Time ?? 0;
            var lastTicksPerMicrosecond = accumulatedMicroseconds?.TicksPerMicrosecond ?? valuesCache.DefaultTicksPerMicrosecond;

            return RoundMicroseconds(lastTime + (timeMicroseconds - lastAccumulatedMicroseconds) * lastTicksPerMicrosecond);
        }

        private static double GetMicroseconds(long time, double microsecondsPerTick)
        {
            return time * microsecondsPerTick;
        }

        private static long RoundMicroseconds(double microseconds)
        {
            return MathUtilities.RoundToLong(microseconds);
        }

        #endregion
    }
}
