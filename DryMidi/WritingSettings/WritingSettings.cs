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
    }
}
