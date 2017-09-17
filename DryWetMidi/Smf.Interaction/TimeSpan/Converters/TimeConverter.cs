using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public static class TimeConverter2
    {
        #region Methods

        public static TTimeSpan ConvertTo<TTimeSpan>(long time, TempoMap tempoMap)
            where TTimeSpan : ITimeSpan
        {
            ThrowIfTimeArgument.IsNegative(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return TimeSpanConverter.ConvertTo<TTimeSpan>(time, 0, tempoMap);
        }

        public static TTimeSpan ConvertTo<TTimeSpan>(ITimeSpan time, TempoMap tempoMap)
            where TTimeSpan : ITimeSpan
        {
            ThrowIfArgument.IsNull(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return ConvertTo<TTimeSpan>(ConvertFrom(time, tempoMap), tempoMap);
        }

        public static ITimeSpan ConvertTo(ITimeSpan time, Type timeType, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(timeType), timeType);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return TimeSpanConverter.ConvertTo(time, timeType, 0, tempoMap);
        }

        public static long ConvertFrom(ITimeSpan time, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return TimeSpanConverter.ConvertFrom(time, 0, tempoMap);
        }

        #endregion
    }
}
