using System;
using System.Collections.Generic;

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

        private static readonly Dictionary<Type, ITimeSpanConverter> Converters = new Dictionary<Type, ITimeSpanConverter>
        {
            [typeof(MidiTimeSpan)] = new MidiTimeSpanConverter(),
            [typeof(MetricTimeSpan)] = new MetricTimeSpanConverter(),
            [typeof(MusicalTimeSpan)] = new MusicalTimeSpanConverter(),
            [typeof(BarBeatTicksTimeSpan)] = new BarBeatTicksTimeSpanConverter(),
            [typeof(BarBeatFractionTimeSpan)] = new BarBeatFractionTimeSpanConverter(),
            [typeof(MathTimeSpan)] = new MathTimeSpanConverter()
        };

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

        private static ITimeSpanConverter GetConverter(TimeSpanType timeSpanType)
        {
            Type type;
            if (!TimeSpansTypes.TryGetValue(timeSpanType, out type))
                throw new NotSupportedException($"Converter for {timeSpanType} is not supported.");

            return GetConverter(type);
        }

        private static ITimeSpanConverter GetConverter(Type timeSpanType)
        {
            ITimeSpanConverter converter;
            if (Converters.TryGetValue(timeSpanType, out converter))
                return converter;

            throw new NotSupportedException($"Converter for {timeSpanType} is not supported.");
        }

        #endregion
    }
}
