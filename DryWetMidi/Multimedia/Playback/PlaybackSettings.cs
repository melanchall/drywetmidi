using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Settings according to which an instance of the <see cref="Playback"/> should be created.
    /// </summary>
    public sealed class PlaybackSettings
    {
        #region Properties

        /// <summary>
        /// Gets or sets settings of the internal playback's clock.
        /// </summary>
        public MidiClockSettings ClockSettings { get; set; }

        /// <summary>
        /// Gets or sets settings which define how timed events should be detected and built internally
        /// by the <see cref="Playback"/>.
        /// </summary>
        public TimedEventDetectionSettings TimedEventDetectionSettings { get; set; }

        /// <summary>
        /// Gets or sets settings which define how notes should be detected and built internally
        /// by the <see cref="Playback"/>.
        /// </summary>
        public NoteDetectionSettings NoteDetectionSettings { get; set; }

        /// <summary>
        /// Gets or sets a hint for a playback creation. The hint can improve performance if you don't need
        /// some features of the <see cref="Playback"/>.
        /// </summary>
        public PlaybackHint Hint { get; set; } = PlaybackHint.Default;

        #endregion
    }
}
