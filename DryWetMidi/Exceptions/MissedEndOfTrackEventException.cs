using System;
using System.Runtime.Serialization;

namespace Melanchall.DryWetMidi
{
    [Serializable]
    public sealed class MissedEndOfTrackEventException : MidiException
    {
        #region Constructors

        public MissedEndOfTrackEventException()
            : base("Track chunk doesn't end with End Of Track event.")
        {
        }

        private MissedEndOfTrackEventException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion
    }
}
