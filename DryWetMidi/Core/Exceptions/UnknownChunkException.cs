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

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(ChunkId), ChunkId);
        }

        #endregion
    }
}
