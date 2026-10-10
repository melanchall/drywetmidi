namespace Melanchall.DryWetMidi.Common
{
    /// <summary>
    /// Provides a way to attach arbitrary data to an object.
    /// </summary>
    public interface IMetadata
    {
        /// <summary>
        /// Gets or sets the metadata associated with the current object.
        /// </summary>
        object? Metadata { get; set; }
    }
}
