using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Provide a hint for a playback creation. The hint can improve performance if you don't need
    /// some features of the <see cref="Playback"/>.
    /// </summary>
    [Flags]
    public enum PlaybackHint
    {
        /// <summary>
        /// Enable usage of all features of the <see cref="Playback"/>.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Disable usage of notes tracking. You won't be able to set <see cref="Playback.TrackNotes"/>
        /// to <c>true</c>.
        /// </summary>
        DisableNotesTracking = 1,

        /// <summary>
        /// Disable usage of program tracking. You won't be able to set <see cref="Playback.TrackProgram"/>
        /// to <c>true</c>.
        /// </summary>
        DisableProgramTracking = 1 << 1,

        /// <summary>
        /// Disable usage of pitch value tracking. You won't be able to set <see cref="Playback.TrackPitchValue"/>
        /// to <c>true</c>.
        /// </summary>
        DisablePitchValueTracking = 1 << 2,

        /// <summary>
        /// Disable usage of control value tracking. You won't be able to set <see cref="Playback.TrackControlValue"/>
        /// to <c>true</c>.
        /// </summary>
        DisableControlValueTracking = 1 << 3,

        /// <summary>
        /// Disable usage of data tracking at all. Includes <see cref="DisableNotesTracking"/>,
        /// <see cref="DisableProgramTracking"/>, <see cref="DisablePitchValueTracking"/> and
        /// <see cref="DisableControlValueTracking"/>.
        /// </summary>
        DisableDataTracking =
            DisableNotesTracking |
            DisableProgramTracking |
            DisablePitchValueTracking |
            DisableControlValueTracking
    }
}
