namespace Melanchall.DryMidi
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
        public CompressionPolicy CompressionPolicy { get; set; }

        /// <summary>
        /// Gets or sets collection of custom meta messages types. These types must be derived from the
        /// <see cref="MetaMessage"/> class and have parameterless constructor. No exception will be thrown
        /// if some types don't meet these requirements.
        /// </summary>
        public MessageTypesCollection CustomMetaMessageTypes { get; set; }
    }
}
