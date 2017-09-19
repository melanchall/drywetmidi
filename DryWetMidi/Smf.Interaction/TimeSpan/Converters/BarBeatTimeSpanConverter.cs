using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal sealed class BarBeatTimeSpanConverter : ITimeSpanConverter
    {
        #region ITimeSpanConverter

        public ITimeSpan ConvertTo(long timeSpan, long time, TempoMap tempoMap)
        {
            var ticksPerQuarterNoteTimeDivision = tempoMap.TimeDivision as TicksPerQuarterNoteTimeDivision;
            if (ticksPerQuarterNoteTimeDivision == null)
                throw new ArgumentException("Time division is not supported for time span conversion.", nameof(tempoMap));

            //

            throw new NotImplementedException();
        }

        public long ConvertFrom(ITimeSpan timeSpan, long time, TempoMap tempoMap)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Methods

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
