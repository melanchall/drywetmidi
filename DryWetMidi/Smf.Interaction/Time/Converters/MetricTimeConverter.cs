using Melanchall.DryWetMidi.Common;
using System;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal sealed class MetricTimeConverter : ITimeConverter
    {
        #region ITimeConverter

        public ITime ConvertTo(long time, TempoMap tempoMap)
        {
            ThrowIfTimeArgument.IsNegative(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            var ticksPerQuarterNoteTimeDivision = tempoMap.TimeDivision as TicksPerQuarterNoteTimeDivision;
            if (ticksPerQuarterNoteTimeDivision != null)
                return ConvertToByTicksPerQuarterNote(time, ticksPerQuarterNoteTimeDivision.TicksPerQuarterNote, tempoMap);

            ThrowIfTimeDivision.IsNotSupportedForTimeConversion(tempoMap.TimeDivision);
            return null;
        }

        public long ConvertFrom(ITime time, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            var metricTime = time as MetricTime;
            if (metricTime == null)
                throw new ArgumentException($"Time is not an instance of the {nameof(MetricTime)}.", nameof(time));

            var ticksPerQuarterNoteTimeDivision = tempoMap.TimeDivision as TicksPerQuarterNoteTimeDivision;
            if (ticksPerQuarterNoteTimeDivision != null)
                return ConvertFromByTicksPerQuarterNote(metricTime, ticksPerQuarterNoteTimeDivision.TicksPerQuarterNote, tempoMap);

            ThrowIfTimeDivision.IsNotSupportedForTimeConversion(tempoMap.TimeDivision);
            return 0;
        }

        #endregion

        #region Methods

        private static MetricTime ConvertToByTicksPerQuarterNote(long time, short ticksPerQuarterNote, TempoMap tempoMap)
        {
            ThrowIfTimeArgument.IsNegative(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            //

            var tempoLine = tempoMap.Tempo;
            var tempoChanges = tempoLine.Values.Where(v => v.Time < time);
            if (!tempoChanges.Any())
                return new MetricTime(RoundMicroseconds(GetMicroseconds(time, Tempo.Default, ticksPerQuarterNote)));

            //

            var accumulatedMicroseconds = 0d;
            var lastTime = 0L;
            var lastTempo = Tempo.Default;

            foreach (var tempoChange in tempoChanges.Concat(new[] { new ValueChange<Tempo>(time, tempoLine.AtTime(time)) }))
            {
                var tempoChangeTime = tempoChange.Time;

                accumulatedMicroseconds += GetMicroseconds(tempoChangeTime - lastTime, lastTempo, ticksPerQuarterNote);
                lastTempo = tempoChange.Value;
                lastTime = tempoChangeTime;
            }

            return new MetricTime(RoundMicroseconds(accumulatedMicroseconds));
        }

        private static long ConvertFromByTicksPerQuarterNote(MetricTime time, short ticksPerQuarterNote, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            //

            var timeMicroseconds = time.TotalMicroseconds;

            var accumulatedMicroseconds = 0d;
            var lastTime = 0L;
            var lastTempo = Tempo.Default;

            foreach (var tempoChange in tempoMap.Tempo.Values)
            {
                var tempoChangeTime = tempoChange.Time;

                var microseconds = GetMicroseconds(tempoChangeTime - lastTime, lastTempo, ticksPerQuarterNote);
                if (IsGreaterOrEqual(accumulatedMicroseconds + microseconds, timeMicroseconds))
                    break;

                accumulatedMicroseconds += microseconds;
                lastTempo = tempoChange.Value;
                lastTime = tempoChangeTime;
            }

            return RoundMicroseconds(lastTime + (timeMicroseconds - accumulatedMicroseconds) * ticksPerQuarterNote / lastTempo.MicrosecondsPerQuarterNote);
        }

        private static double GetMicroseconds(long time, Tempo tempo, short ticksPerQuarterNote)
        {
            return time == 0
                ? 0
                : time * tempo.MicrosecondsPerQuarterNote / (double)ticksPerQuarterNote;
        }

        private static long RoundMicroseconds(double microseconds)
        {
            return (long)Math.Round(microseconds);
        }

        private static bool IsGreaterOrEqual(double value, long reference)
        {
            const double epsilon = 0.001;
            return value > reference || Math.Abs(value - reference) <= epsilon;
        }

        #endregion
    }
}
