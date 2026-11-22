using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// The exception that is thrown when a variable-length quantity exceeds the supported range.
    /// </summary>
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
