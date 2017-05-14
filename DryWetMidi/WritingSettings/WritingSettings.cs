namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Settings of the writing engine.
    /// </summary>
    public class WritingSettings
    {
        /// <summary>
        /// Gets or sets compression rules for the writing engine. The default is
        /// <see cref="CompressionPolicy.NoCompression"/>.
        /// </summary>
        public CompressionPolicy CompressionPolicy { get; set; } = CompressionPolicy.NoCompression;

        /// <summary>
        /// Gets or sets collection of custom meta events types. These types must be derived from the
        /// <see cref="MetaEvent"/> class and have parameterless constructor. No exception will be thrown
        /// while writing a MIDI file if some types don't meet these requirements.
        /// </summary>
        public EventTypesCollection CustomMetaEventTypes { get; set; }
    }
}
