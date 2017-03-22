using System;
using System.Runtime.Serialization;

namespace Melanchall.DryMidi
{
    [Serializable]
    public sealed class MissedEndOfTrackMessageException : MidiException
    {
        #region Constructors

        public MissedEndOfTrackMessageException()
            : base("Track chunk doesn't end with End Of Track message.")
        {
        }

        private MissedEndOfTrackMessageException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion
    }
}
