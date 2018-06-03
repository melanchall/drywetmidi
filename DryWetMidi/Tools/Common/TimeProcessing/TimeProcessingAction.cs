namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Indicates whether an object is being processed should be skipped or not.
    /// The default value is <see cref="Apply"/>.
    /// </summary>
    public enum TimeProcessingAction
    {
        /// <summary>
        /// Set new time to an object.
        /// </summary>
        Apply = 0,

        /// <summary>
        /// Skip an object and leave its time untouched.
        /// </summary>
        Skip
    }
}
