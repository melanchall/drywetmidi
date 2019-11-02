namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Part of an <see cref="ILengthedObject"/>.
    /// </summary>
    public enum LengthedObjectPart
    {
        /// <summary>
        /// Start of an object.
        /// </summary>
        Start,

        /// <summary>
        /// End of an object.
        /// </summary>
        End,

        /// <summary>
        /// Entire object from its start to its end.
        /// </summary>
        Entire
    }
}
