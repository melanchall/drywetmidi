using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// The exception that is thrown when a MIDI file chunk doesn't end with an <c>End of Track</c> event.
    /// </summary>
    /// <remarks>
    /// <para>Note that this exception will be thrown only if <see cref="ReadingSettings.MissedEndOfTrackPolicy"/>
    /// is set to <see cref="MissedEndOfTrackPolicy.Abort"/> for the <see cref="ReadingSettings"/>
    /// used for reading a MIDI file.</para>
    /// </remarks>
    [Serializable]
    public sealed class MissedEndOfTrackEventException : MidiException
    {
        #region Constructors

        internal MissedEndOfTrackEventException()
            : base("Track chunk doesn't end with End Of Track event.")
        {
        }

        #endregion
    }
}
