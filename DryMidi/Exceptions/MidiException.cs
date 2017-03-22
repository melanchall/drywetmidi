using System;
using System.Runtime.Serialization;

namespace Melanchall.DryMidi
{
    public abstract class MidiException : Exception
    {
        #region Constructors

        public MidiException()
        {
        }

        public MidiException(string message)
            : base(message)
        {
        }

        public MidiException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected MidiException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion
    }
}
