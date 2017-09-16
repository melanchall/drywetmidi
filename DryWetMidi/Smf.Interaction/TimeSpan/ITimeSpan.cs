namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public interface ITimeSpan
    {
        ITimeSpan Add(ITimeSpan timeSpan);

        ITimeSpan Subtract(ITimeSpan timeSpan);

        ITimeSpan Multiply(double multiplier);

        ITimeSpan Divide(double divisor);
    }
}
