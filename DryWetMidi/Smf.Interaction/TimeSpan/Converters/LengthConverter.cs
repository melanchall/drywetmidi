using Melanchall.DryWetMidi.Common;

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

            return TimeSpanConverter.ConvertTo<TTimeSpan>(length, time, tempoMap);
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

            return TimeSpanConverter.ConvertTo<TTimeSpan>(length, time, tempoMap);
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

            return TimeSpanConverter.ConvertFrom(length, time, tempoMap);
        }

        #endregion
    }
}
