using System;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal sealed class MusicalTimeConverter : ITimeConverter
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

            var musicalTime = time as MusicalTime;
            if (musicalTime == null)
                throw new ArgumentException("Time is not a musical time.", nameof(time));

            var ticksPerQuarterNoteTimeDivision = tempoMap.TimeDivision as TicksPerQuarterNoteTimeDivision;
            if (ticksPerQuarterNoteTimeDivision != null)
                return ConvertFromByTicksPerQuarterNote(musicalTime, ticksPerQuarterNoteTimeDivision.TicksPerQuarterNote, tempoMap);

            throw new NotSupportedException("Time division other than TicksPerQuarterNoteTimeDivision not supported.");
        }

        #endregion

        #region Methods

        private static MusicalTime ConvertToByTicksPerQuarterNote(long time, short ticksPerQuarterNote, TempoMap tempoMap)
        {
            if (time < 0)
                throw new ArgumentOutOfRangeException(nameof(time), time, "Time is negative.");

            if (tempoMap == null)
                throw new ArgumentNullException(nameof(tempoMap));

            //

            var bars = 0;
            var lastTime = 0L;
            var lastTimeSignature = TimeSignature.Default;

            foreach (var timeSignatureChange in tempoMap.TimeSignatureLine.Values.Where(v => v.Time <= time))
            {
                var timeSignatureChangeTime = timeSignatureChange.Time;

                bars += GetBarsCount(timeSignatureChangeTime - lastTime, lastTimeSignature, ticksPerQuarterNote);
                lastTimeSignature = timeSignatureChange.Value;
                lastTime = timeSignatureChangeTime;
            }

            //

            var deltaTime = time - lastTime;
            var lastBars = GetBarsCount(deltaTime, lastTimeSignature, ticksPerQuarterNote);
            bars += lastBars;

            //

            deltaTime = deltaTime % GetBarLength(lastTimeSignature, ticksPerQuarterNote);
            var beatLength = GetBeatLength(lastTimeSignature, ticksPerQuarterNote);
            var beats = deltaTime / beatLength;

            //

            var ticks = deltaTime % beatLength;

            //

            return new MusicalTime(bars, (int)beats, (int)ticks, beatLength);
        }

        private static long ConvertFromByTicksPerQuarterNote(MusicalTime time, short ticksPerQuarterNote, TempoMap tempoMap)
        {
            if (time == null)
                throw new ArgumentNullException(nameof(time));

            if (tempoMap == null)
                throw new ArgumentNullException(nameof(tempoMap));

            //

            var timeBars = time.Bars;
            var accumulatedBars = 0;
            var lastTime = 0L;
            var lastTimeSignature = TimeSignature.Default;

            foreach (var timeSignatureChange in tempoMap.TimeSignatureLine.Values)
            {
                var timeSignatureChangeTime = timeSignatureChange.Time;

                var bars = GetBarsCount(timeSignatureChangeTime - lastTime, lastTimeSignature, ticksPerQuarterNote);
                if (accumulatedBars + bars >= timeBars)
                    break;

                accumulatedBars += bars;
                lastTimeSignature = timeSignatureChange.Value;
                lastTime = timeSignatureChangeTime;
            }

            var beatLength = GetBeatLength(lastTimeSignature, ticksPerQuarterNote);
            return lastTime + (timeBars - accumulatedBars) * GetBarLength(lastTimeSignature, ticksPerQuarterNote) +
                   time.Beats * beatLength +
                   beatLength * time.Ticks / time.BeatLength;
        }

        private static int GetBarsCount(long time, TimeSignature timeSignature, short ticksPerQuarterNote)
        {
            if (time == 0)
                return 0;

            var barLength = GetBarLength(timeSignature, ticksPerQuarterNote);
            return (int)(time / barLength);
        }

        private static int GetBarLength(TimeSignature timeSignature, short ticksPerQuarterNote)
        {
            var beatLength = GetBeatLength(timeSignature, ticksPerQuarterNote);
            return timeSignature.Numerator * beatLength;
        }

        private static int GetBeatLength(TimeSignature timeSignature, short ticksPerQuarterNote)
        {
            return 4 * ticksPerQuarterNote / timeSignature.Denominator;
        }

        #endregion
    }
}
