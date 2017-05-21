using System;
using System.Runtime.Serialization;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// The exception that is thrown when a MIDI file doesn't contain a header chunk.
    /// </summary>
    /// <remarks>
    /// Note that this exception will be thrown only if <see cref="ReadingSettings.NoHeaderChunkPolicy"/>
    /// is set to <see cref="NoHeaderChunkPolicy.Abort"/> for the <see cref="ReadingSettings"/>
    /// used for reading a MIDI file.
    /// </remarks>
    [Serializable]
    public sealed class NoHeaderChunkException : MidiException
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NoHeaderChunkException"/>.
        /// </summary>
        public NoHeaderChunkException()
            : base("MIDI file doesn't contain the header chunk.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoHeaderChunkException"/> with the
        /// specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public NoHeaderChunkException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoHeaderChunkException"/>
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information
        /// about the source or destination.</param>
        private NoHeaderChunkException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion
    }
}
