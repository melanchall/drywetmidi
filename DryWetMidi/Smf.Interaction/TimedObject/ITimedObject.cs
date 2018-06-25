namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Represents an object that has start time.
    /// </summary>
    public interface ITimedObject
    {
        /// <summary>
        /// Gets start time of an object.
        /// </summary>
        long Time { get; }
    }
}
