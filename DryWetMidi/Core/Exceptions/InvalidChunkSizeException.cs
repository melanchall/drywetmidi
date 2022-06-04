using Melanchall.DryWetMidi.Common;
using System;
using System.Runtime.Serialization;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// The exception that is thrown when the actual size of a MIDI file chunk differs from
    /// the one declared in its header.
    /// </summary>
    /// <remarks>
    /// <para>Note that this exception will be thrown only if <see cref="ReadingSettings.InvalidChunkSizePolicy"/>
    /// is set to <see cref="InvalidChunkSizePolicy.Abort"/> for the <see cref="ReadingSettings"/>
    /// used for reading a MIDI file.</para>
    /// </remarks>
    [Serializable]
    public sealed class InvalidChunkSizeException : MidiException
    {
        #region Constructors

        internal InvalidChunkSizeException(string chunkId, long expectedSize, long actualSize)
            : base($"Actual size ({actualSize}) of a {chunkId} chunk differs from the one declared in the chunk's header ({expectedSize}).")
        {
            ChunkId = chunkId;
            ExpectedSize = expectedSize;
            ActualSize = actualSize;
        }

        private InvalidChunkSizeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ChunkId = info.GetString(nameof(ChunkId));
            ExpectedSize = info.GetInt64(nameof(ExpectedSize));
            ActualSize = info.GetInt64(nameof(ActualSize));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the ID of a chunk caused this exception.
        /// </summary>
        public string ChunkId { get; }

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
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data
        /// about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information
        /// about the source or destination.</param>
        /// <exception cref="ArgumentNullException"><paramref name="info"/> is <c>null</c>.</exception>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(ChunkId), ChunkId);
            info.AddValue(nameof(ExpectedSize), ExpectedSize);
            info.AddValue(nameof(ActualSize), ActualSize);
        }

        #endregion
    }
}
