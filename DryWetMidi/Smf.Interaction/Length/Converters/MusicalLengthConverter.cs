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
                return new MusicalLength(FractionUtilities.FromTicks(length, ticksPerQuarterNoteTimeDivision.TicksPerQuarterNote));

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
                return FractionUtilities.ToTicks(musicalLength.Fraction, ticksPerQuarterNoteTimeDivision.TicksPerQuarterNote);

            throw new NotSupportedException("Time division other than TicksPerQuarterNoteTimeDivision not supported.");
        }

        public long ConvertFrom(ILength length, ITime time, TempoMap tempoMap)
        {
            return ConvertFrom(length, TimeConverter.ConvertFrom(time, tempoMap), tempoMap);
        }

        #endregion
    }
}
