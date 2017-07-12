using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal sealed class MusicalLengthConverter : ILengthConverter
    {
        #region ILengthConverter

        public ILength ConvertTo(long length, long time, TempoMap tempoMap)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "Length is negative.");

            if (time < 0)
                throw new ArgumentOutOfRangeException(nameof(time), time, "Time is negative.");

            if (tempoMap == null)
                throw new ArgumentNullException(nameof(tempoMap));

            var ticksPerQuarterNoteTimeDivision = tempoMap.TimeDivision as TicksPerQuarterNoteTimeDivision;
            if (ticksPerQuarterNoteTimeDivision != null)
                return ConvertToByTicksPerQuarterNote(length, ticksPerQuarterNoteTimeDivision.TicksPerQuarterNote);

            throw new NotSupportedException("Time division other than TicksPerQuarterNoteTimeDivision not supported.");
        }

        public ILength ConvertTo(long length, ITime time, TempoMap tempoMap)
        {
            return ConvertTo(length, TimeConverter.ConvertFrom(time, tempoMap), tempoMap);
        }

        public long ConvertFrom(ILength length, long time, TempoMap tempoMap)
        {
            if (length == null)
                throw new ArgumentNullException(nameof(length));

            if (time < 0)
                throw new ArgumentOutOfRangeException(nameof(time), time, "Time is negative.");

            if (tempoMap == null)
                throw new ArgumentNullException(nameof(tempoMap));

            var musicalLength = length as MusicalLength;
            if (musicalLength == null)
                throw new ArgumentException("Length is not a musical length.", nameof(length));

            var ticksPerQuarterNoteTimeDivision = tempoMap.TimeDivision as TicksPerQuarterNoteTimeDivision;
            if (ticksPerQuarterNoteTimeDivision != null)
                return ConvertFromByTicksPerQuarterNote(musicalLength, ticksPerQuarterNoteTimeDivision.TicksPerQuarterNote);

            throw new NotSupportedException("Time division other than TicksPerQuarterNoteTimeDivision not supported.");
        }

        public long ConvertFrom(ILength length, ITime time, TempoMap tempoMap)
        {
            return ConvertFrom(length, TimeConverter.ConvertFrom(time, tempoMap), tempoMap);
        }

        #endregion

        #region Methods

        private static MusicalLength ConvertToByTicksPerQuarterNote(long length, short ticksPerQuarterNote)
        {
            var xy = MathUtilities.SolveDiophantineEquation(4 * ticksPerQuarterNote, -length);
            return new MusicalLength(new Fraction(Math.Abs(xy.Item1), Math.Abs(xy.Item2)));
        }

        private static long ConvertFromByTicksPerQuarterNote(MusicalLength length, short ticksPerQuarterNote)
        {
            return 4 * length.Fraction.Numerator * ticksPerQuarterNote / length.Fraction.Denominator;
        }

        #endregion
    }
}
