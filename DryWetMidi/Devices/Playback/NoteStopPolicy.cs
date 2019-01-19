namespace Melanchall.DryWetMidi.Devices
{
    /// <summary>
    /// Determines how currently playing notes should react on playback stopped.
    /// </summary>
    public enum NoteStopPolicy
    {
        /// <summary>
        /// Do nothing and let notes playing.
        /// </summary>
        Hold = 0,

        /// <summary>
        /// Interrupt notes by sending corresponding Note Off events.
        /// </summary>
        Interrupt,

        /// <summary>
        /// Split notes at the moment of playback stopped. Notes will be interrupted by
        /// sending corresponding Note Off events, but when playback will be resumed,
        /// they will be played from point of split via sending Note On events.
        /// </summary>
        Split
    }
}
