using System;
using System.Runtime.Serialization;

namespace Melanchall.DryWetMidi
{
    [Serializable]
    public sealed class NoHeaderChunkException : MidiException
    {
        #region Constructors

        public NoHeaderChunkException()
            : base()
        {
        }

        public NoHeaderChunkException(string message)
            : base(message)
        {
        }

        private NoHeaderChunkException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion
    }
}
