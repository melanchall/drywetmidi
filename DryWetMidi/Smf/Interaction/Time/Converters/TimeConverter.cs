namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public static class TimeConverter
    {
        #region Methods

        public static TTime ConvertTo<TTime>(long time, TempoMap tempoMap)
            where TTime : ITime
        {
            var converter = GetConverter<TTime>();
            return (TTime)converter.ConvertTo(time, tempoMap);
        }

        public static TTime ConvertTo<TTime>(ITime time, TempoMap tempoMap)
            where TTime : ITime
        {
            return ConvertTo<TTime>(ConvertFrom(time, tempoMap), tempoMap);
        }

        public static long ConvertFrom<TTime>(TTime time, TempoMap tempoMap)
            where TTime : ITime
        {
            var converter = GetConverter(time);
            return converter.ConvertFrom(time, tempoMap);
        }

        public static ITimeConverter GetConverter<TTime>()
            where TTime : ITime
        {
            return TimeConverterFactory.GetConverter<TTime>();
        }

        private static ITimeConverter GetConverter(ITime time)
        {
            return TimeConverterFactory.GetConverter(time.GetType());
        }

        #endregion
    }
}
