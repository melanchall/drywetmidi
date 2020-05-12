namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Represents an object that has length.
    /// </summary>
    public interface ILengthedObject : ITimedObject
    {
        #region Properties

        /// <summary>
        /// Gets length of an object.
        /// </summary>
        long Length { get; }

        #endregion
    }
}
