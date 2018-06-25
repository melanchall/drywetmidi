namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Represents an object that has length.
    /// </summary>
    public interface ILengthedObject : ITimedObject
    {
        /// <summary>
        /// Gets length of an object.
        /// </summary>
        long Length { get; }
    }
}
