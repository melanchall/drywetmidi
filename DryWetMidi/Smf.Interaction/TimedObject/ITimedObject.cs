namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Represents an object that has start time.
    /// </summary>
    public interface ITimedObject
    {
        /// <summary>
        /// Gets or sets start time of an object.
        /// </summary>
        long Time { get; set; }
    }
}
