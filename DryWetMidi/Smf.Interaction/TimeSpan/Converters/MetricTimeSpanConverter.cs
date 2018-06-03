using System;
using System.Linq;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal sealed class MetricTimeSpanConverter : ITimeSpanConverter
    {
        #region ITimeSpanConverter

        public ITimeSpan ConvertTo(long timeSpan, long time, TempoMap tempoMap)
        {
            var ticksPerQuarterNoteTimeDivision = tempoMap.TimeDivision as TicksPerQuarterNoteTimeDivision;
            if (ticksPerQuarterNoteTimeDivision == null)
                throw new ArgumentException("Time division is not supported for time span conversion.", nameof(tempoMap));

            var startTimeSpan = TicksToMetricTimeSpan(time, ticksPerQuarterNoteTimeDivision.TicksPerQuarterNote, tempoMap);
            var endTimeSpan = TicksToMetricTimeSpan(time + timeSpan, ticksPerQuarterNoteTimeDivision.TicksPerQuarterNote, tempoMap);

            return endTimeSpan - startTimeSpan;
        }

        public long ConvertFrom(ITimeSpan timeSpan, long time, TempoMap tempoMap)
        {
            var ticksPerQuarterNoteTimeDivision = tempoMap.TimeDivision as TicksPerQuarterNoteTimeDivision;
            if (ticksPerQuarterNoteTimeDivision == null)
                throw new ArgumentException("Time division is not supported for time span conversion.", nameof(tempoMap));

            var startTimeSpan = TicksToMetricTimeSpan(time, ticksPerQuarterNoteTimeDivision.TicksPerQuarterNote, tempoMap);
            var endTimeSpan = startTimeSpan + (MetricTimeSpan)timeSpan;

            return MetricTimeSpanToTicks(endTimeSpan, ticksPerQuarterNoteTimeDivision.TicksPerQuarterNote, tempoMap) - time;
        }

        #endregion

        #region Methods

        private static MetricTimeSpan TicksToMetricTimeSpan(long timeSpan, short ticksPerQuarterNote, TempoMap tempoMap)
        {
            if (timeSpan == 0)
                return new MetricTimeSpan();

            var valuesCache = tempoMap.GetValuesCache<MetricTempoMapValuesCache>();
            var accumulatedMicroseconds = valuesCache.Microseconds.TakeWhile(m => m.Time < timeSpan).LastOrDefault();

            var lastAccumulatedMicroseconds = accumulatedMicroseconds?.Microseconds ?? 0;
            var lastTime = accumulatedMicroseconds?.Time ?? 0;
            var lastMicrosecondsPerTick = accumulatedMicroseconds?.MicrosecondsPerTick ?? valuesCache.DefaultMicrosecondsPerTick;

            return new MetricTimeSpan(RoundMicroseconds(lastAccumulatedMicroseconds + GetMicroseconds(timeSpan - lastTime, lastMicrosecondsPerTick)));
        }

        private static long MetricTimeSpanToTicks(MetricTimeSpan timeSpan, short ticksPerQuarterNote, TempoMap tempoMap)
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

        private static bool IsGreaterOrEqual(double value, long reference)
        {
            const double epsilon = 0.001;
            return value > reference || Math.Abs(value - reference) <= epsilon;
        }

        #endregion
    }
}
