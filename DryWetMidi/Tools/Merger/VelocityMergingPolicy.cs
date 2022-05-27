namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Determines how velocities should be merged during notes merging. The default value is <see cref="First"/>.
    /// </summary>
    public enum VelocityMergingPolicy
    {
        /// <summary>
        /// Take velocity of first note.
        /// </summary>
        First = 0,

        /// <summary>
        /// Take velocity of last note.
        /// </summary>
        Last,

        /// <summary>
        /// Take minimum velocity.
        /// </summary>
        Min,

        /// <summary>
        /// Take maximum velocity.
        /// </summary>
        Max,

        /// <summary>
        /// Take average velocity.
        /// </summary>
        Average
    }
}
