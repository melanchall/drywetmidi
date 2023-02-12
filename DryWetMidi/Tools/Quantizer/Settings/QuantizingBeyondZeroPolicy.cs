namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Policy which defines how a quantizer should act in case of an object is going
    /// to be moved beyond zero. The default value is <see cref="FixAtZero"/>.
    /// </summary>
    public enum QuantizingBeyondZeroPolicy
    {
        /// <summary>
        /// Object will be shrinked due to end time quantizing and fixed at zero.
        /// </summary>
        FixAtZero = 0,

        /// <summary>
        /// Object will be skipped so quantizing will not be applied to it.
        /// </summary>
        Skip,

        /// <summary>
        /// Throw an exception aborting quantizing.
        /// </summary>
        Abort
    }
}
