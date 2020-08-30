namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Represents an object that has time.
    /// </summary>
    public interface ITimedObject
    {
        #region Properties

        /// <summary>
        /// Gets or sets the time of an object.
        /// </summary>
        long Time { get; set; }

        #endregion
    }
}
