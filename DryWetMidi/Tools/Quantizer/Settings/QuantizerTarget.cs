using System;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// An object's target (start, end, both ends) to quantize.
    /// </summary>
    [Flags]
    public enum QuantizerTarget
    {
        /// <summary>
        /// Start time of an object.
        /// </summary>
        Start = 1,

        /// <summary>
        /// End time of an object.
        /// </summary>
        End = 2
    }
}
