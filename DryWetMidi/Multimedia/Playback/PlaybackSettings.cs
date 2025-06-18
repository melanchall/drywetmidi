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
        /// Gets or sets a value indicating whether to calculate tempo map for a playback
        /// on data initializing. It means to process Set Tempo and Time Signature events
        /// iterating through the objects passed to the <see cref="Playback"/>'s constructor.
        /// <see cref="Playback.TempoMap"/> will reflect these changes. The default value is <c>false</c>
        /// which requires to pass correct tempo map to the <see cref="Playback"/>'s constructor.
        /// </summary>
        /// <remarks>
        /// The property can be useful when the <see cref="Playback"/> is created with a collection of objects
        /// and you don't want to create a tempo map manually. In this case you can just set the property
        /// to <c>true</c> and pass <see cref="TempoMap.Default"/>, and playback will construct the tempo map
        /// for you.
        /// </remarks>
        public bool CalculateTempoMap { get; set; }

        internal bool UseNoteEventsDirectly { get; set; }

        #endregion
    }
}
