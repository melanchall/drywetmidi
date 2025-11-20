namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Specifies the site within playback where an error occurred.
    /// </summary>
    /// <seealso cref="Playback.ErrorOccurred"/>
    public enum PlaybackErrorSite
    {
        /// <summary>
        /// Playing a MIDI event (sending to an <see cref="IOutputDevice"/>).
        /// </summary>
        PlayEvent,

        /// <summary>
        /// MIDI clock tick processing.
        /// </summary>
        Tick,

        /// <summary>
        /// <see cref="Playback.NoteCallback"/>.
        /// </summary>
        NoteCallback,

        /// <summary>
        /// <see cref="Playback.EventCallback"/>.
        /// </summary>
        EventCallback,

        /// <summary>
        /// <see cref="Playback.Started"/> event handler.
        /// </summary>
        Started,

        /// <summary>
        /// <see cref="Playback.Stopped"/> event handler.
        /// </summary>
        Stopped,

        /// <summary>
        /// <see cref="Playback.Finished"/> event handler.
        /// </summary>
        Finished,

        /// <summary>
        /// <see cref="Playback.EventPlayed"/> event handler.
        /// </summary>
        EventPlayed,

        /// <summary>
        /// <see cref="Playback.RepeatStarted"/> event handler.
        /// </summary>
        RepeatStarted,

        /// <summary>
        /// <see cref="Playback.NotesPlaybackStarted"/> event handler.
        /// </summary>
        NotesPlaybackStarted,

        /// <summary>
        /// <see cref="Playback.NotesPlaybackFinished"/> event handler.
        /// </summary>
        NotesPlaybackFinished,
    }
}
