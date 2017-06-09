using System;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class MetricTimeConverter
    {
        #region Methods

        public MetricTime ConvertTo(long time, TempoMap tempoMap)
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

        public long ConvertFrom(MetricTime time, TempoMap tempoMap)
        {
            if (time == null)
                throw new ArgumentNullException(nameof(time));

            if (tempoMap == null)
                throw new ArgumentNullException(nameof(tempoMap));

            var ticksPerQuarterNoteTimeDivision = tempoMap.TimeDivision as TicksPerQuarterNoteTimeDivision;
            if (ticksPerQuarterNoteTimeDivision != null)
                return ConvertFromByTicksPerQuarterNote(time, ticksPerQuarterNoteTimeDivision.TicksPerQuarterNote, tempoMap);

            throw new NotSupportedException("Time division other than TicksPerQuarterNoteTimeDivision not supported.");
        }

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

        private static ValueChange<Tempo> CreateTempoChange(long time, ValuesLine<Tempo> tempoLine)
        {
            return new ValueChange<Tempo>(time, tempoLine.AtTime(time));
        }

        private static long GetMicroseconds(long time, Tempo tempo, short ticksPerQuarterNote)
        {
            return time * GetMicrosecondsInTick(tempo, ticksPerQuarterNote);
        }

        private static long GetMicrosecondsInTick(Tempo tempo, short ticksPerQuarterNote)
        {
            return tempo.MicrosecondsPerBeat / ticksPerQuarterNote;
        }

        #endregion
    }
}
