namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Represents a simple math operation used by the <see cref="MathTime"/> and the <see cref="MathLength"/>.
    /// The default is <see cref="Sum"/>.
    /// </summary>
    public enum MathOperation
    {
        /// <summary>
        /// Summation.
        /// </summary>
        Sum = 0,

        /// <summary>
        /// Subtraction.
        /// </summary>
        Subtract
    }
}
