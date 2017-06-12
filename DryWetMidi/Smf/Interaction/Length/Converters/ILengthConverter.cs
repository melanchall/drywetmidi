namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public interface ILengthConverter
    {
        ILength ConvertTo(long length, long time, TempoMap tempoMap);

        ILength ConvertTo(long length, ITime time, TempoMap tempoMap);

        long ConvertFrom(ILength length, long time, TempoMap tempoMap);

        long ConvertFrom(ILength length, ITime time, TempoMap tempoMap);
    }
}
