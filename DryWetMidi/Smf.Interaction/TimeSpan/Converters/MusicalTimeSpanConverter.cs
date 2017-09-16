using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal sealed class MusicalTimeSpanConverter : ITimeSpanConverter
    {
        #region ITimeSpanConverter

        public ITimeSpan ConvertTo(long length, long time, TempoMap tempoMap)
        {
            ThrowIfLengthArgument.IsNegative(nameof(length), length);
            ThrowIfTimeArgument.IsNegative(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            var ticksPerQuarterNoteTimeDivision = tempoMap.TimeDivision as TicksPerQuarterNoteTimeDivision;
            if (ticksPerQuarterNoteTimeDivision == null)
                throw new ArgumentException("Time division is not supported for time span conversion.", nameof(tempoMap));

            //

            return (MusicalTimeSpan)TicksToFraction(length, ticksPerQuarterNoteTimeDivision.TicksPerQuarterNote);
        }

        public long ConvertFrom(ITimeSpan length, long time, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(length), length);
            ThrowIfTimeArgument.IsNegative(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            var musicalTimeSpan = length as MusicalTimeSpan;
            if (musicalTimeSpan == null)
                throw new ArgumentException($"Time span is not an instance of the {nameof(MusicalTimeSpan)}.", nameof(length));

            var ticksPerQuarterNoteTimeDivision = tempoMap.TimeDivision as TicksPerQuarterNoteTimeDivision;
            if (ticksPerQuarterNoteTimeDivision == null)
                throw new ArgumentException("Time division is not supported for time span conversion.", nameof(tempoMap));

            //

            return FractionToTicks(musicalTimeSpan.Fraction, ticksPerQuarterNoteTimeDivision.TicksPerQuarterNote);
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
