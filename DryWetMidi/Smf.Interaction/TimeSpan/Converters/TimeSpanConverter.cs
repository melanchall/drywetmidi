using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal static class TimeSpanConverter
    {
        #region Constants

        private static readonly Dictionary<Type, ITimeSpanConverter> Converters = new Dictionary<Type, ITimeSpanConverter>
        {
            [typeof(MidiTimeSpan)] = new MidiTimeSpanConverter(),
            [typeof(MetricTimeSpan)] = new MetricTimeSpanConverter(),
            [typeof(MusicalTimeSpan)] = new MusicalTimeSpanConverter(),
        };

        #endregion

        #region Methods

        public static TTimeSpan ConvertTo<TTimeSpan>(long timeSpan, long time, TempoMap tempoMap)
            where TTimeSpan : ITimeSpan
        {
            return (TTimeSpan)GetConverter<TTimeSpan>().ConvertTo(timeSpan, time, tempoMap);
        }

        public static TTimeSpan ConvertTo<TTimeSpan>(long timeSpan, ITimeSpan time, TempoMap tempoMap)
            where TTimeSpan : ITimeSpan
        {
            return ConvertTo<TTimeSpan>(timeSpan, TimeToMidi(time, tempoMap), tempoMap);
        }

        public static TTimeSpan ConvertTo<TTimeSpan>(ITimeSpan timeSpan, long time, TempoMap tempoMap)
            where TTimeSpan : ITimeSpan
        {
            return ConvertTo<TTimeSpan>(ConvertFrom(timeSpan, time, tempoMap), time, tempoMap);
        }

        public static TTimeSpan ConvertTo<TTimeSpan>(ITimeSpan timeSpan, ITimeSpan time, TempoMap tempoMap)
            where TTimeSpan : ITimeSpan
        {
            return ConvertTo<TTimeSpan>(timeSpan, TimeToMidi(time, tempoMap), tempoMap);
        }

        public static long ConvertFrom(ITimeSpan timeSpan, long time, TempoMap tempoMap)
        {
            return GetConverter(timeSpan.GetType()).ConvertFrom(timeSpan, time, tempoMap);
        }

        public static long ConvertFrom(ITimeSpan timeSpan, ITimeSpan time, TempoMap tempoMap)
        {
            return ConvertFrom(timeSpan, TimeToMidi(time, tempoMap), tempoMap);
        }

        private static long TimeToMidi(ITimeSpan time, TempoMap tempoMap)
        {
            return ConvertFrom(time, 0, tempoMap);
        }

        private static ITimeSpanConverter GetConverter<TTimeSpan>()
            where TTimeSpan : ITimeSpan
        {
            return GetConverter(typeof(TTimeSpan));
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
