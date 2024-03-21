using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class MusicalTimeSpanConverter : ITimeSpanConverter
    {
        #region ITimeSpanConverter

        public ITimeSpan ConvertTo(long timeSpan, long time, TempoMap tempoMap)
        {
            var ticksPerQuarterNoteTimeDivision = tempoMap.TimeDivision as TicksPerQuarterNoteTimeDivision;
            if (ticksPerQuarterNoteTimeDivision == null)
                throw new ArgumentException("Time division is not supported for time span conversion.", nameof(tempoMap));

            if (timeSpan == 0)
                return new MusicalTimeSpan();

            var gcd = MathUtilities.GreatestCommonDivisor(timeSpan, 4 * ticksPerQuarterNoteTimeDivision.TicksPerQuarterNote);
            return new MusicalTimeSpan(timeSpan / gcd, 4 * ticksPerQuarterNoteTimeDivision.TicksPerQuarterNote / gcd);
        }

        public long ConvertFrom(ITimeSpan timeSpan, long time, TempoMap tempoMap)
        {
            var ticksPerQuarterNoteTimeDivision = tempoMap.TimeDivision as TicksPerQuarterNoteTimeDivision;
            if (ticksPerQuarterNoteTimeDivision == null)
                throw new ArgumentException("Time division is not supported for time span conversion.", nameof(tempoMap));

            var musicalTimeSpan = (MusicalTimeSpan)timeSpan;
            if (musicalTimeSpan.Numerator == 0)
                return 0;

            return MathUtilities.RoundToLong(4.0 * musicalTimeSpan.Numerator * ticksPerQuarterNoteTimeDivision.TicksPerQuarterNote / musicalTimeSpan.Denominator);
        }

        #endregion
    }
}
