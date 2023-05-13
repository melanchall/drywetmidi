namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Policy which defines how a quantizer should act in case of object's side
    /// is going to be moved beyond an opposite one that is fixed. The default value is
    /// <see cref="CollapseAndFix"/>.
    /// </summary>
    public enum QuantizingBeyondFixedEndPolicy
    {
        /// <summary>
        /// Object will be collapsed and fixed at fixed end's time.
        /// </summary>
        CollapseAndFix = 0,

        /// <summary>
        /// Object will be collapsed and moved to the new time calculated by a quantizer.
        /// </summary>
        CollapseAndMove,

        /// <summary>
        /// Ends of an object will be swapped.
        /// </summary>
        SwapEnds,

        /// <summary>
        /// Object will be skipped so quantization will not be applied to it.
        /// </summary>
        Skip,

        /// <summary>
        /// Throw an exception aborting quantization.
        /// </summary>
        Abort
    }
}
