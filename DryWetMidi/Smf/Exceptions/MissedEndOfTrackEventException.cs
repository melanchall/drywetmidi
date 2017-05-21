using System;
using System.Runtime.Serialization;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// The exception that is thrown when a MIDI file chunk doesn't end with an End of Track event.
    /// </summary>
    /// <remarks>
    /// Note that this exception will be thrown only if <see cref="ReadingSettings.MissedEndOfTrackPolicy"/>
    /// is set to <see cref="MissedEndOfTrackPolicy.Abort"/> for the <see cref="ReadingSettings"/>
    /// used for reading a MIDI file.
    /// </remarks>
    [Serializable]
    public sealed class MissedEndOfTrackEventException : MidiException
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MissedEndOfTrackEventException"/>.
        /// </summary>
        public MissedEndOfTrackEventException()
            : base("Track chunk doesn't end with End Of Track event.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MissedEndOfTrackEventException"/>
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information
        /// about the source or destination.</param>
        private MissedEndOfTrackEventException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion
    }
}
