using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public static class LengthConverter2
    {
        #region Methods

        public static TTimeSpan ConvertTo<TTimeSpan>(long length, long time, TempoMap tempoMap)
            where TTimeSpan : ITimeSpan
        {
            ThrowIfLengthArgument.IsNegative(nameof(length), length);
            ThrowIfTimeArgument.IsNegative(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return TimeSpanConverter.ConvertTo<TTimeSpan>(length, time, tempoMap);
        }

        public static TTimeSpan ConvertTo<TTimeSpan>(long length, ITimeSpan time, TempoMap tempoMap)
            where TTimeSpan : ITimeSpan
        {
            ThrowIfLengthArgument.IsNegative(nameof(length), length);
            ThrowIfArgument.IsNull(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return TimeSpanConverter.ConvertTo<TTimeSpan>(length, TimeConverter2.ConvertFrom(time, tempoMap), tempoMap);
        }

        public static TTimeSpan ConvertTo<TTimeSpan>(ITimeSpan length, long time, TempoMap tempoMap)
            where TTimeSpan : ITimeSpan
        {
            ThrowIfArgument.IsNull(nameof(length), length);
            ThrowIfTimeArgument.IsNegative(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return TimeSpanConverter.ConvertTo<TTimeSpan>(length, time, tempoMap);
        }

        public static TTimeSpan ConvertTo<TTimeSpan>(ITimeSpan length, ITimeSpan time, TempoMap tempoMap)
            where TTimeSpan : ITimeSpan
        {
            ThrowIfArgument.IsNull(nameof(length), length);
            ThrowIfArgument.IsNull(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return TimeSpanConverter.ConvertTo<TTimeSpan>(length, TimeConverter2.ConvertFrom(time, tempoMap), tempoMap);
        }

        public static ITimeSpan ConvertTo(ITimeSpan length, Type lengthType, long time, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(length), length);
            ThrowIfArgument.IsNull(nameof(lengthType), lengthType);
            ThrowIfTimeArgument.IsNegative(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return TimeSpanConverter.ConvertTo(length, lengthType, time, tempoMap);
        }

        public static ITimeSpan ConvertTo(ITimeSpan length, Type lengthType, ITimeSpan time, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(length), length);
            ThrowIfArgument.IsNull(nameof(lengthType), lengthType);
            ThrowIfArgument.IsNull(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return TimeSpanConverter.ConvertTo(length, lengthType, TimeConverter2.ConvertFrom(time, tempoMap), tempoMap);
        }

        public static long ConvertFrom(ITimeSpan length, long time, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(length), length);
            ThrowIfTimeArgument.IsNegative(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return TimeSpanConverter.ConvertFrom(length, time, tempoMap);
        }

        public static long ConvertFrom(ITimeSpan length, ITimeSpan time, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(length), length);
            ThrowIfArgument.IsNull(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return TimeSpanConverter.ConvertFrom(length, TimeConverter2.ConvertFrom(time, tempoMap), tempoMap);
        }

        #endregion
    }
}
