using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// The exception that is thrown when a variable-length quantity exceeds the supported byte or value range.
    /// </summary>
    /// <remarks>
    /// In Standard MIDI Files, delta-times are variable-length quantities with values from 0 through
    /// 268,435,455 (0x0FFFFFFF), encoded in at most four bytes. The reader's 32-bit and 64-bit VLQ methods
    /// also support values up to <see cref="int.MaxValue"/> and <see cref="long.MaxValue"/>, respectively.
    /// </remarks>
    public sealed class VlqNumberOverflowException : MidiException
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VlqNumberOverflowException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public VlqNumberOverflowException(string message)
            : base(message)
        {
        }

        #endregion
    }
}
