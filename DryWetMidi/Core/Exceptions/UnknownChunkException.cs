using Melanchall.DryWetMidi.Common;
using System;
using System.Runtime.Serialization;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// The exception that is thrown when the reading engine encountered a chunk with unknown ID.
    /// </summary>
    /// <remarks>
    /// <para>Note that this exception will be thrown only if <see cref="ReadingSettings.UnknownChunkIdPolicy"/>
    /// is set to <see cref="UnknownChunkIdPolicy.Abort"/> for the <see cref="ReadingSettings"/>
    /// used for reading a MIDI file.</para>
    /// </remarks>
    [Serializable]
    public sealed class UnknownChunkException : MidiException
    {
        #region Constructors

        internal UnknownChunkException(string chunkId)
            : base($"'{chunkId}' chunk ID is unknown.")
        {
            ChunkId = chunkId;
        }

        private UnknownChunkException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ChunkId = info.GetString(nameof(ChunkId));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the ID of an unknown chunk.
        /// </summary>
        public string ChunkId { get; }

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
        }

        #endregion
    }
}
