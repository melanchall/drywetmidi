using System.Runtime.Serialization;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// The exception that is thrown when the reading engine encountered unexpected running
    /// status.
    /// </summary>
    public sealed class UnexpectedRunningStatusException : MidiException
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UnexpectedRunningStatusException"/>.
        /// </summary>
        public UnexpectedRunningStatusException()
            : base("Unexpected running status is encountered.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnexpectedRunningStatusException"/>
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information
        /// about the source or destination.</param>
        private UnexpectedRunningStatusException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion
    }
}
