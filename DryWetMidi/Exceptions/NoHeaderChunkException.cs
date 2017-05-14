using System;
using System.Runtime.Serialization;

namespace Melanchall.DryWetMidi.Smf
{
    [Serializable]
    public sealed class NoHeaderChunkException : MidiException
    {
        #region Constructors

        public NoHeaderChunkException()
            : base("MIDI file doesn't contain the header chunk.")
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
