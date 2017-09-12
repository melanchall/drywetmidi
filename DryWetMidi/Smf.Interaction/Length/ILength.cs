namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Represents length in custom format (other than MIDI length expressed as <see cref="long"/>).
    /// </summary>
    public interface ILength
    {
        ILength Add(ILength length);

        ILength Subtract(ILength length);

        ILength Multiply(int multiplier);

        ILength Divide(int divisor);
    }
}
