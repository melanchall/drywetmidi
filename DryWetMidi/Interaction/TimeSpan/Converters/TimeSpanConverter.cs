using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Melanchall.DryWetMidi.Interaction
{
    internal static class TimeSpanConverter
    {
        #region Constants

        private static readonly Dictionary<TimeSpanType, Type> TimeSpansTypes = new Dictionary<TimeSpanType, Type>
        {
            [TimeSpanType.Midi] = typeof(MidiTimeSpan),
            [TimeSpanType.Metric] = typeof(MetricTimeSpan),
            [TimeSpanType.Musical] = typeof(MusicalTimeSpan),
            [TimeSpanType.BarBeatTicks] = typeof(BarBeatTicksTimeSpan),
            [TimeSpanType.BarBeatFraction] = typeof(BarBeatFractionTimeSpan)
        };

        private static readonly ITimeSpanConverter MidiTimeSpanConverter = new MidiTimeSpanConverter();
        private static readonly ITimeSpanConverter MetricTimeSpanConverter = new MetricTimeSpanConverter();
        private static readonly ITimeSpanConverter MusicalTimeSpanConverter = new MusicalTimeSpanConverter();
        private static readonly ITimeSpanConverter BarBeatTicksTimeSpanConverter = new BarBeatTicksTimeSpanConverter();
        private static readonly ITimeSpanConverter BarBeatFractionTimeSpanConverter = new BarBeatFractionTimeSpanConverter();
        private static readonly ITimeSpanConverter MathTimeSpanConverter = new MathTimeSpanConverter();

        #endregion

        #region Methods

        public static TTimeSpan ConvertTo<TTimeSpan>(long timeSpan, long time, TempoMap tempoMap)
            where TTimeSpan : ITimeSpan
        {
            return (TTimeSpan)GetConverter<TTimeSpan>().ConvertTo(timeSpan, time, tempoMap);
        }

        public static ITimeSpan ConvertTo(long timeSpan, TimeSpanType timeSpanType, long time, TempoMap tempoMap)
        {
            return GetConverter(timeSpanType).ConvertTo(timeSpan, time, tempoMap);
        }

        public static TTimeSpan ConvertTo<TTimeSpan>(ITimeSpan timeSpan, long time, TempoMap tempoMap)
            where TTimeSpan : ITimeSpan
        {
            if (timeSpan is TTimeSpan)
                return (TTimeSpan)timeSpan.Clone();

            return ConvertTo<TTimeSpan>(ConvertFrom(timeSpan, time, tempoMap), time, tempoMap);
        }

        public static ITimeSpan ConvertTo(ITimeSpan timeSpan, TimeSpanType timeSpanType, long time, TempoMap tempoMap)
        {
            if (timeSpan.GetType() == TimeSpansTypes[timeSpanType])
                return timeSpan.Clone();

            return ConvertTo(ConvertFrom(timeSpan, time, tempoMap), timeSpanType, time, tempoMap);
        }

        public static ITimeSpan ConvertTo(ITimeSpan timeSpan, Type timeSpanType, long time, TempoMap tempoMap)
        {
            if (timeSpan.GetType() == timeSpanType)
                return timeSpan.Clone();

            return GetConverter(timeSpanType).ConvertTo(ConvertFrom(timeSpan, time, tempoMap), time, tempoMap);
        }

        public static long ConvertFrom(ITimeSpan timeSpan, long time, TempoMap tempoMap)
        {
            return GetConverter(timeSpan.GetType()).ConvertFrom(timeSpan, time, tempoMap);
        }

        private static ITimeSpanConverter GetConverter<TTimeSpan>()
            where TTimeSpan : ITimeSpan
        {
            return GetConverter(typeof(TTimeSpan));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ITimeSpanConverter GetConverter(TimeSpanType timeSpanType)
        {
            switch (timeSpanType)
            {
                case TimeSpanType.Midi:
                    return MidiTimeSpanConverter;
                case TimeSpanType.Metric:
                    return MetricTimeSpanConverter;
                case TimeSpanType.Musical:
                    return MusicalTimeSpanConverter;
                case TimeSpanType.BarBeatTicks:
                    return BarBeatTicksTimeSpanConverter;
                case TimeSpanType.BarBeatFraction:
                    return BarBeatFractionTimeSpanConverter;
            }

            throw new NotSupportedException($"Converter is not supported.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ITimeSpanConverter GetConverter(Type timeSpanType)
        {
            if (timeSpanType == typeof(MidiTimeSpan))
                return MidiTimeSpanConverter;
            if (timeSpanType == typeof(MetricTimeSpan))
                return MetricTimeSpanConverter;
            if (timeSpanType == typeof(MusicalTimeSpan))
                return MusicalTimeSpanConverter;
            if (timeSpanType == typeof(BarBeatTicksTimeSpan))
                return BarBeatTicksTimeSpanConverter;
            if (timeSpanType == typeof(BarBeatFractionTimeSpan))
                return BarBeatFractionTimeSpanConverter;
            if (timeSpanType == typeof(MathTimeSpan))
                return MathTimeSpanConverter;

            throw new NotSupportedException($"Converter is not supported.");
        }

        #endregion
    }
}
