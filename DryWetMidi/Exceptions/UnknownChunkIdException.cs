using System;
using System.Runtime.Serialization;

namespace Melanchall.DryWetMidi
{
    [Serializable]
    public sealed class UnknownChunkIdException : MidiException
    {
        #region Constants

        private const string ChunkIdSerializationPropertyName = "ChunkId";

        #endregion

        #region Constructors

        public UnknownChunkIdException()
            : base()
        {
        }

        public UnknownChunkIdException(string message)
            : base(message)
        {
        }

        public UnknownChunkIdException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        private UnknownChunkIdException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ChunkId = info.GetString(ChunkIdSerializationPropertyName);
        }

        public UnknownChunkIdException(string message, string chunkId)
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
