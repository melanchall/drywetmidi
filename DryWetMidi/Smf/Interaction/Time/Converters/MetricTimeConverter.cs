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

            throw new NotSupportedException("Time division other than TicksPerQuarterNoteTimeDivision not supported.");
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

            throw new NotSupportedException("Time division other than TicksPerQuarterNoteTimeDivision not supported.");
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
                return new MetricTime(GetMicroseconds(time, Tempo.Default, ticksPerQuarterNote));

            //

            var accumulatedMicroseconds = 0L;
            var lastTime = 0L;
            var lastTempo = Tempo.Default;

            foreach (var tempoChange in tempoChanges.Concat(new[] { CreateTempoChange(time, tempoLine) }))
            {
                var tempoChangeTime = tempoChange.Time;

                accumulatedMicroseconds += GetMicroseconds(tempoChangeTime - lastTime, lastTempo, ticksPerQuarterNote);
                lastTempo = tempoChange.Value;
                lastTime = tempoChangeTime;
            }

            return new MetricTime(accumulatedMicroseconds);
        }

        private static long ConvertFromByTicksPerQuarterNote(MetricTime time, short ticksPerQuarterNote, TempoMap tempoMap)
        {
            if (time == null)
                throw new ArgumentNullException(nameof(time));

            if (tempoMap == null)
                throw new ArgumentNullException(nameof(tempoMap));

            //

            var timeMicroseconds = time.TotalMicroseconds;

            var accumulatedMicroseconds = 0L;
            var lastTime = 0L;
            var lastTempo = Tempo.Default;

            foreach (var tempoChange in tempoMap.TempoLine.Values)
            {
                var tempoChangeTime = tempoChange.Time;

                var microseconds = GetMicroseconds(tempoChangeTime - lastTime, lastTempo, ticksPerQuarterNote);
                if (accumulatedMicroseconds + microseconds >= timeMicroseconds)
                    break;

                accumulatedMicroseconds += microseconds;
                lastTempo = tempoChange.Value;
                lastTime = tempoChangeTime;
            }

            return lastTime + (timeMicroseconds - accumulatedMicroseconds) / GetMicrosecondsInTick(lastTempo, ticksPerQuarterNote);
        }

        private static ValueChange<Tempo> CreateTempoChange(long time, ValueLine<Tempo> tempoLine)
        {
            return new ValueChange<Tempo>(time, tempoLine.AtTime(time));
        }

        private static long GetMicroseconds(long time, Tempo tempo, short ticksPerQuarterNote)
        {
            if (time == 0)
                return 0;

            return time * GetMicrosecondsInTick(tempo, ticksPerQuarterNote);
        }

        private static long GetMicrosecondsInTick(Tempo tempo, short ticksPerQuarterNote)
        {
            return tempo.MicrosecondsPerQuarterNote / ticksPerQuarterNote;
        }

        #endregion
    }
}
