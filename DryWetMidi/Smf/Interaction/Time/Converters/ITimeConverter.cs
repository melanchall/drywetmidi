namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public interface ITimeConverter
    {
        ITime ConvertTo(long time, TempoMap tempoMap);

        long ConvertFrom(ITime time, TempoMap tempoMap);
    }
}
