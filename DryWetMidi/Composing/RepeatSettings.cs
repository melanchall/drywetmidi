namespace Melanchall.DryWetMidi.Composing
{
    /// <summary>
    /// Defines additional options for a pattern's actions repeating.
    /// </summary>
    /// <seealso cref="PatternBuilder"/>
    public sealed class RepeatSettings
    {
        #region Properties

        /// <summary>
        /// Gets or sets a transformation for every repeated note. If set to <c>null</c>,
        /// no transformation will be applied.
        /// </summary>
        public NoteTransformation NoteTransformation { get; set; }

        /// <summary>
        /// Gets or sets a transformation for every repeated chord. If set to <c>null</c>,
        /// no transformation will be applied.
        /// </summary>
        public ChordTransformation ChordTransformation { get; set; }

        #endregion
    }
}
