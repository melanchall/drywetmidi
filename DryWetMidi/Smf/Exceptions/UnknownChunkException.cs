using System;
using System.Runtime.Serialization;

namespace Melanchall.DryWetMidi.Smf
{
    [Serializable]
    public sealed class UnknownChunkException : MidiException
    {
        #region Constants

        private const string ChunkIdSerializationPropertyName = "ChunkId";

        #endregion

        #region Constructors

        public UnknownChunkException()
            : base()
        {
        }

        public UnknownChunkException(string message)
            : base(message)
        {
        }

        public UnknownChunkException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        private UnknownChunkException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ChunkId = info.GetString(ChunkIdSerializationPropertyName);
        }

        public UnknownChunkException(string message, string chunkId)
            : this(message)
        {
            ChunkId = chunkId;
        }

        #endregion

        #region Properties

        public string ChunkId { get; }

        #endregion

        #region Overrides

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            info.AddValue(ChunkIdSerializationPropertyName, ChunkId);

            base.GetObjectData(info, context);
        }

        #endregion
    }
}
