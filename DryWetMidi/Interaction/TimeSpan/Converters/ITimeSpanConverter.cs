namespace Melanchall.DryWetMidi.Interaction
{
    internal interface ITimeSpanConverter
    {
        ITimeSpan ConvertTo(long timeSpan, long time, TempoMap tempoMap);

        long ConvertFrom(ITimeSpan timeSpan, long time, TempoMap tempoMap);
    }
}
