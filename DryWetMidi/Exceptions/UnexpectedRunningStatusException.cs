using System.Runtime.Serialization;

namespace Melanchall.DryWetMidi
{
    public sealed class UnexpectedRunningStatusException : MidiException
    {
        #region Constructors

        public UnexpectedRunningStatusException()
            : base("Unexpected running status is encountered.")
        {
        }

        private UnexpectedRunningStatusException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion
    }
}
