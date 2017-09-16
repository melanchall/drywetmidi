using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal sealed class MathTimeSpanConverter : ITimeSpanConverter
    {
        #region ITimeSpanConverter

        public ITimeSpan ConvertTo(long timeSpan, long time, TempoMap tempoMap)
        {
            throw new NotSupportedException($"Conversion to the {nameof(MathTimeSpan)} is not supported.");
        }

        public long ConvertFrom(ITimeSpan timeSpan, long time, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan), timeSpan);
            ThrowIfTimeArgument.IsNegative(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            var mathTimeSpan = timeSpan as MathTimeSpan;
            if (mathTimeSpan == null)
                throw new ArgumentException($"Time span is not an instance of the {nameof(MathTimeSpan)}.", nameof(timeSpan));

            var convertedTimeSpan1 = LengthConverter2.ConvertFrom(mathTimeSpan.TimeSpan1, time, tempoMap);
            var endTime1 = time + convertedTimeSpan1;

            switch (mathTimeSpan.Operation)
            {
                case MathOperation.Add:
                    return convertedTimeSpan1 + LengthConverter2.ConvertFrom(mathTimeSpan.TimeSpan2, endTime1, tempoMap);

                case MathOperation.Subtract:
                    return convertedTimeSpan1 - LengthConverter2.ConvertFrom(mathTimeSpan.TimeSpan2, endTime1, tempoMap.Flip(endTime1));
            }

            throw new NotImplementedException($"Conversion from the {nameof(MathTimeSpan)} with {mathTimeSpan.Operation} operation is not implemented.");
        }

        #endregion
    }
}
