namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Represents an object that has length.
    /// </summary>
    public interface ILengthedObject : ITimedObject
    {
        #region Properties

        /// <summary>
        /// Gets or sets the length of an object.
        /// </summary>
        long Length { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Splits the current object by the specified time.
        /// </summary>
        /// <remarks>
        /// If <paramref name="time"/> is less than time of the object, the left part will be <c>null</c>.
        /// If <paramref name="time"/> is greater than end time of the object, the right part
        /// will be <c>null</c>.
        /// </remarks>
        /// <param name="time">Time to split the object by.</param>
        /// <returns>An object containing left and right parts of the split object.
        /// Both parts have the same type as the original object.</returns>
        SplitLengthedObject Split(long time);

        #endregion
    }
}
