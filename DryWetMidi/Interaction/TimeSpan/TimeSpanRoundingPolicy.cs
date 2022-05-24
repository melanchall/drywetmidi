using System;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Specifies how a time span should be round.
    /// </summary>
    public enum TimeSpanRoundingPolicy
    {
        /// <summary>
        /// Don't round time span.
        /// </summary>
        NoRounding = 0,

        /// <summary>
        /// Round time span up (like, for example, <see cref="Math.Ceiling(double)"/>).
        /// </summary>
        RoundUp,

        // TODO: test
        /// <summary>
        /// Round time span down (like, for example, <see cref="Math.Floor(double)"/>).
        /// </summary>
        RoundDown
    }
}
