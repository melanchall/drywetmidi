namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public interface ITimeSpan
    {
        ITimeSpan Add(ITimeSpan timeSpan, TimeSpanMode mode);

        ITimeSpan Subtract(ITimeSpan timeSpan, TimeSpanMode mode);

        ITimeSpan Multiply(double multiplier);

        ITimeSpan Divide(double divisor);

        ITimeSpan Clone();
    }
}
