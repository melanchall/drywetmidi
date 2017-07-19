using System;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal sealed class MetricTimeConverter : ITimeConverter
    {
        #region ITimeConverter

        public ITime ConvertTo(long time, TempoMap tempoMap)
        {
            if (time < 0)
                throw new ArgumentOutOfRangeException(nameof(time), time, "Time is negative.");

            if (tempoMap == null)
                throw new ArgumentNullException(nameof(tempoMap));

            var ticksPerQuarterNoteTimeDivision = tempoMap.TimeDivision as TicksPerQuarterNoteTimeDivision;
            if (ticksPerQuarterNoteTimeDivision != null)
                return ConvertToByTicksPerQuarterNote(time, ticksPerQuarterNoteTimeDivision.TicksPerQuarterNote, tempoMap);

            throw new NotSupportedException($"Time division other than {nameof(TicksPerQuarterNoteTimeDivision)} not supported.");
        }

        public long ConvertFrom(ITime time, TempoMap tempoMap)
        {
            if (time == null)
                throw new ArgumentNullException(nameof(time));

            if (tempoMap == null)
                throw new ArgumentNullException(nameof(tempoMap));

            var metricTime = time as MetricTime;
            if (metricTime == null)
                throw new ArgumentException("Time is not a metric time.", nameof(time));

            var ticksPerQuarterNoteTimeDivision = tempoMap.TimeDivision as TicksPerQuarterNoteTimeDivision;
            if (ticksPerQuarterNoteTimeDivision != null)
                return ConvertFromByTicksPerQuarterNote(metricTime, ticksPerQuarterNoteTimeDivision.TicksPerQuarterNote, tempoMap);

            throw new NotSupportedException($"Time division other than {nameof(TicksPerQuarterNoteTimeDivision)} not supported.");
        }

        #endregion

        #region Methods

        private static MetricTime ConvertToByTicksPerQuarterNote(long time, short ticksPerQuarterNote, TempoMap tempoMap)
        {
            if (time < 0)
                throw new ArgumentOutOfRangeException(nameof(time), time, "Time is negative.");

            if (tempoMap == null)
                throw new ArgumentNullException(nameof(tempoMap));

            //

            var tempoLine = tempoMap.TempoLine;
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
            if (time == null)
                throw new ArgumentNullException(nameof(time));

            if (tempoMap == null)
                throw new ArgumentNullException(nameof(tempoMap));

            //

            var timeMicroseconds = time.TotalMicroseconds;

            var accumulatedMicroseconds = 0d;
            var lastTime = 0L;
            var lastTempo = Tempo.Default;

            foreach (var tempoChange in tempoMap.TempoLine.Values)
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
