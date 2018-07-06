namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Determines whether notes should be resized using absolute or relative
    /// length calculation.
    /// </summary>
    public enum ResizingMode
    {
        /// <summary>
        /// Resize notes adding to or subtracting constant value from a note's length.
        /// </summary>
        Absolute,

        /// <summary>
        /// Resize notes scaling a note's length by the ratio between old length and the new one.
        /// </summary>
        Relative
    }
}
