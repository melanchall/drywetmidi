namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public static class LengthConverter
    {
        #region Methods

        public static TLength ConvertTo<TLength>(long length, long time, TempoMap tempoMap)
            where TLength : ILength
        {
            var converter = GetConverter<TLength>();
            return (TLength)converter.ConvertTo(length, time, tempoMap);
        }

        public static TLength ConvertTo<TLength>(ILength length, long time, TempoMap tempoMap)
            where TLength : ILength
        {
            return ConvertTo<TLength>(ConvertFrom(length, time, tempoMap), time, tempoMap);
        }

        public static long ConvertFrom<TLength>(TLength length, long time, TempoMap tempoMap)
            where TLength : ILength
        {
            var converter = GetConverter(length);
            return converter.ConvertFrom(length, time, tempoMap);
        }

        public static ILengthConverter GetConverter<TLength>()
            where TLength : ILength
        {
            return LengthConverterFactory.GetConverter<TLength>();
        }

        private static ILengthConverter GetConverter(ILength length)
        {
            return LengthConverterFactory.GetConverter(length.GetType());
        }

        #endregion
    }
}
