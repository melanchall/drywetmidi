using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal sealed class MusicalLengthConverter : ILengthConverter
    {
        #region ILengthConverter

        public ILength ConvertTo(long length, long time, TempoMap tempoMap)
        {
            ThrowIfLengthArgument.IsNegative(nameof(length), length);
            ThrowIfTimeArgument.IsNegative(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            var ticksPerQuarterNoteTimeDivision = tempoMap.TimeDivision as TicksPerQuarterNoteTimeDivision;
            if (ticksPerQuarterNoteTimeDivision != null)
                return new MusicalLength(FractionUtilities.FromTicks(length, ticksPerQuarterNoteTimeDivision.TicksPerQuarterNote));

            ThrowIfTimeDivision.IsNotSupportedForLengthConversion(tempoMap.TimeDivision);
            return null;
        }

        public long ConvertFrom(ILength length, long time, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(length), length);
            ThrowIfTimeArgument.IsNegative(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            var musicalLength = length as MusicalLength;
            if (musicalLength == null)
                throw new ArgumentException($"Length is not an instance of the {nameof(MusicalLength)}.", nameof(length));

            var ticksPerQuarterNoteTimeDivision = tempoMap.TimeDivision as TicksPerQuarterNoteTimeDivision;
            if (ticksPerQuarterNoteTimeDivision != null)
                return musicalLength.Fraction.ToTicks(ticksPerQuarterNoteTimeDivision.TicksPerQuarterNote);

            ThrowIfTimeDivision.IsNotSupportedForLengthConversion(tempoMap.TimeDivision);
            return 0;
        }

        #endregion
    }
}
