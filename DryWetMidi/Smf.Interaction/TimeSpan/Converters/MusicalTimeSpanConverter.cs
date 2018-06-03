using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal sealed class MusicalTimeSpanConverter : ITimeSpanConverter
    {
        #region ITimeSpanConverter

        public ITimeSpan ConvertTo(long timeSpan, long time, TempoMap tempoMap)
        {
            var ticksPerQuarterNoteTimeDivision = tempoMap.TimeDivision as TicksPerQuarterNoteTimeDivision;
            if (ticksPerQuarterNoteTimeDivision == null)
                throw new ArgumentException("Time division is not supported for time span conversion.", nameof(tempoMap));

            //

            var xy = MathUtilities.SolveDiophantineEquation(4 * ticksPerQuarterNoteTimeDivision.TicksPerQuarterNote, -timeSpan);
            return new MusicalTimeSpan(Math.Abs(xy.Item1), Math.Abs(xy.Item2));
        }

        public long ConvertFrom(ITimeSpan timeSpan, long time, TempoMap tempoMap)
        {
            var ticksPerQuarterNoteTimeDivision = tempoMap.TimeDivision as TicksPerQuarterNoteTimeDivision;
            if (ticksPerQuarterNoteTimeDivision == null)
                throw new ArgumentException("Time division is not supported for time span conversion.", nameof(tempoMap));

            var musicalTimeSpan = (MusicalTimeSpan)timeSpan;

            return MathUtilities.RoundToLong(4.0 * musicalTimeSpan.Numerator * ticksPerQuarterNoteTimeDivision.TicksPerQuarterNote / musicalTimeSpan.Denominator);
        }

        #endregion
    }
}
