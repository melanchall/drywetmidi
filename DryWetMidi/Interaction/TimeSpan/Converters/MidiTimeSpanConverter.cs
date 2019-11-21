namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class MidiTimeSpanConverter : ITimeSpanConverter
    {
        #region ITimeSpanConverter

        public ITimeSpan ConvertTo(long timeSpan, long time, TempoMap tempoMap)
        {
            return (MidiTimeSpan)timeSpan;
        }

        public long ConvertFrom(ITimeSpan timeSpan, long time, TempoMap tempoMap)
        {
            return ((MidiTimeSpan)timeSpan).TimeSpan;
        }

        #endregion
    }
}
