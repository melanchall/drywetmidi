using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal sealed class MathLengthConverter : ILengthConverter
    {
        #region ILengthConverter

        public ILength ConvertTo(long length, long time, TempoMap tempoMap)
        {
            throw new NotSupportedException($"Conversion to the {nameof(MathLength)} is not supported.");
        }

        public ILength ConvertTo(long length, ITime time, TempoMap tempoMap)
        {
            throw new NotSupportedException($"Conversion to the {nameof(MathLength)} is not supported.");
        }

        public long ConvertFrom(ILength length, long time, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(length), length);
            ThrowIfTimeArgument.IsNegative(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            var mathLength = length as MathLength;
            if (mathLength == null)
                throw new ArgumentException($"Length is not an instance of the {nameof(MathLength)}.", nameof(length));

            var convertedLength1 = LengthConverter.ConvertFrom(mathLength.Length1, time, tempoMap);
            var endTime1 = time + convertedLength1;

            switch (mathLength.Operation)
            {
                case MathOperation.Add:
                    return convertedLength1 + LengthConverter.ConvertFrom(mathLength.Length2, endTime1, tempoMap);

                case MathOperation.Subtract:
                    return convertedLength1 - LengthConverter.ConvertFrom(mathLength.Length2, endTime1, tempoMap.Flip(endTime1));
            }

            throw new NotImplementedException($"Conversion from the {nameof(MathLength)} with {mathLength.Operation} operation is not implemented.");
        }

        public long ConvertFrom(ILength length, ITime time, TempoMap tempoMap)
        {
            return ConvertFrom(length, TimeConverter.ConvertFrom(time, tempoMap), tempoMap);
        }

        #endregion
    }
}
