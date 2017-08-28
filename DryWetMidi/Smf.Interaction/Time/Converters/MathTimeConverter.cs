using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal sealed class MathTimeConverter : ITimeConverter
    {
        #region ITimeConverter

        public ITime ConvertTo(long time, TempoMap tempoMap)
        {
            throw new NotSupportedException($"Conversion to the {nameof(MathTime)} is not supported.");
        }

        public long ConvertFrom(ITime time, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            var mathTime = time as MathTime;
            if (mathTime == null)
                throw new ArgumentException($"Time is not an instance of the {nameof(MathTime)}.", nameof(time));

            var result = TimeConverter.ConvertFrom(mathTime.Time, tempoMap);
            
            switch (mathTime.Operation)
            {
                case MathOperation.Add:
                    return result + LengthConverter.ConvertFrom(mathTime.Offset, result, tempoMap);

                case MathOperation.Subtract:
                    return result - LengthConverter.ConvertFrom(mathTime.Offset, result, tempoMap.Flip(result));
            }

            throw new NotImplementedException($"Conversion from the {nameof(MathTime)} with {mathTime.Operation} operation is not implemented.");
        }

        #endregion
    }
}
