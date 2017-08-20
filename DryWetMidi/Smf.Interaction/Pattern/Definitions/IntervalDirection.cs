namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Direction of a musical interval represented by the <see cref="IntervalDefinition"/>.
    /// The default is <see cref="Up"/>.
    /// </summary>
    public enum IntervalDirection
    {
        /// <summary>
        /// Upward interval (for example, 5 half steps up).
        /// </summary>
        Up = 0,

        /// <summary>
        /// Downward interval (for example, 5 half steps down).
        /// </summary>
        Down
    }
}
