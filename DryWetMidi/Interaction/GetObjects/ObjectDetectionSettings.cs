namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Settings which define how objects should be detected and built.
    /// </summary>
    public sealed class ObjectDetectionSettings
    {
        #region Properties

        /// <summary>
        /// Gets or sets settings which define how timed events should be detected and built.
        /// </summary>
        public TimedEventDetectionSettings TimedEventDetectionSettings { get; set; }

        /// <summary>
        /// Gets or sets settings which define how notes should be detected and built.
        /// </summary>
        public NoteDetectionSettings NoteDetectionSettings { get; set; }

        /// <summary>
        /// Gets or sets settings which define how chords should be detected and built.
        /// </summary>
        public ChordDetectionSettings ChordDetectionSettings { get; set; }

        #endregion
    }
}
