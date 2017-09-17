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

            return (MusicalTimeSpan)TicksToFraction(timeSpan, ticksPerQuarterNoteTimeDivision.TicksPerQuarterNote);
        }

        public long ConvertFrom(ITimeSpan timeSpan, long time, TempoMap tempoMap)
        {
            var ticksPerQuarterNoteTimeDivision = tempoMap.TimeDivision as TicksPerQuarterNoteTimeDivision;
            if (ticksPerQuarterNoteTimeDivision == null)
                throw new ArgumentException("Time division is not supported for time span conversion.", nameof(tempoMap));

            return FractionToTicks(((MusicalTimeSpan)timeSpan).Fraction, ticksPerQuarterNoteTimeDivision.TicksPerQuarterNote);
        }

        #endregion

        #region Methods

        private static Fraction TicksToFraction(long ticks, short ticksPerQuarterNote)
        {
            var xy = MathUtilities.SolveDiophantineEquation(4 * ticksPerQuarterNote, -ticks);
            return new Fraction(Math.Abs(xy.Item1), Math.Abs(xy.Item2));
        }

        private static long FractionToTicks(Fraction fraction, short ticksPerQuarterNote)
        {
            return 4 * fraction.Numerator * ticksPerQuarterNote / fraction.Denominator;
        }

        #endregion
    }
}
