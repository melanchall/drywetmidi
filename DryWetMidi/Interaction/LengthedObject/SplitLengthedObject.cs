namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Container for left and right parts of split lengthed object.
    /// </summary>
    public sealed class SplitLengthedObject
    {
        #region Constructor

        internal SplitLengthedObject(ILengthedObject leftPart, ILengthedObject rightPart)
        {
            LeftPart = leftPart;
            RightPart = rightPart;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The left part of a split object.
        /// </summary>
        public ILengthedObject LeftPart { get; }

        /// <summary>
        /// The right part of a split object.
        /// </summary>
        public ILengthedObject RightPart { get; }

        #endregion
    }
}
