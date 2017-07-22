using Melanchall.DryWetMidi.Common;
using System;
using System.Runtime.Serialization;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// The exception that is thrown when the actual size of a MIDI file chunk differs from
    /// the one declared in its header.
    /// </summary>
    /// <remarks>
    /// Note that this exception will be thrown only if <see cref="ReadingSettings.InvalidChunkSizePolicy"/>
    /// is set to <see cref="InvalidChunkSizePolicy.Abort"/> for the <see cref="ReadingSettings"/>
    /// used for reading a MIDI file.
    /// </remarks>
    [Serializable]
    public sealed class InvalidChunkSizeException : MidiException
    {
        #region Constants

        private const string ExpectedSizeSerializationPropertyName = "ExpectedSize";
        private const string ActualSizeSerializationPropertyName = "ActualSize";

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidChunkSizeException"/>.
        /// </summary>
        public InvalidChunkSizeException()
            : base("Actual size of a chunk differs from the one declared in the chunk's header.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidChunkSizeException"/> with
        /// the specified expected size of a MIDI chunk and the actual one.
        /// </summary>
        /// <param name="expectedSize">Expected size of a chunk written in its header.</param>
        /// <param name="actualSize">Actual size of a chunk.</param>
        public InvalidChunkSizeException(long expectedSize, long actualSize)
            : this()
        {
            ExpectedSize = expectedSize;
            ActualSize = actualSize;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidChunkSizeException"/>
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information
        /// about the source or destination.</param>
        private InvalidChunkSizeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ExpectedSize = info.GetInt64(ExpectedSizeSerializationPropertyName);
            ActualSize = info.GetInt64(ActualSizeSerializationPropertyName);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the expected size of a chunk written in its header.
        /// </summary>
        public long ExpectedSize { get; }

        /// <summary>
        /// Gets the actual size of a chunk.
        /// </summary>
        public long ActualSize { get; }

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

            info.AddValue(ExpectedSizeSerializationPropertyName, ExpectedSize);
            info.AddValue(ActualSizeSerializationPropertyName, ActualSize);

            base.GetObjectData(info, context);
        }

        #endregion
    }
}
