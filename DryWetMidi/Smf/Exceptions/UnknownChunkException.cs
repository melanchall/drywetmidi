using Melanchall.DryWetMidi.Common;
using System;
using System.Runtime.Serialization;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// The exception that is thrown when the reading engine encountered a chunk with unknown ID.
    /// </summary>
    /// <remarks>
    /// Note that this exception will be thrown only if <see cref="ReadingSettings.UnknownChunkIdPolicy"/>
    /// is set to <see cref="UnknownChunkIdPolicy.Abort"/> for the <see cref="ReadingSettings"/>
    /// used for reading a MIDI file.
    /// </remarks>
    [Serializable]
    public sealed class UnknownChunkException : MidiException
    {
        #region Constants

        private const string ChunkIdSerializationPropertyName = "ChunkId";

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownChunkException"/>.
        /// </summary>
        public UnknownChunkException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownChunkException"/> with the
        /// specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public UnknownChunkException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownChunkException"/> class with the
        /// specified error message and a reference to the inner exception that is the
        /// cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception,
        /// or a null reference if no inner exception is specified.</param>
        public UnknownChunkException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownChunkException"/> with the
        /// specified error message and the ID of a chunk.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="chunkId">ID of a chunk.</param>
        public UnknownChunkException(string message, string chunkId)
            : this(message)
        {
            ChunkId = chunkId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownChunkException"/>
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information
        /// about the source or destination.</param>
        private UnknownChunkException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ChunkId = info.GetString(ChunkIdSerializationPropertyName);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the ID of a chunk.
        /// </summary>
        public string ChunkId { get; }

        #endregion

        #region Overrides

        /// <summary>
        /// Sets the <see cref="SerializationInfo"/> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information
        /// about the source or destination.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            ThrowIfArgument.IsNull(nameof(info), info);

            info.AddValue(ChunkIdSerializationPropertyName, ChunkId);

            base.GetObjectData(info, context);
        }

        #endregion
    }
}
