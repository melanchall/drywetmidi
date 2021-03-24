namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Container for left and right parts of split lengthed object.
    /// </summary>
    /// <typeparam name="TObject">The type of split object.</typeparam>
    public sealed class SplitLengthedObject<TObject>
        where TObject : ILengthedObject
    {
        #region Constructor

        internal SplitLengthedObject(TObject leftPart, TObject rightPart)
        {
            LeftPart = leftPart;
            RightPart = rightPart;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The left part of a split object.
        /// </summary>
        public TObject LeftPart { get; }

        /// <summary>
        /// The right part of a split object.
        /// </summary>
        public TObject RightPart { get; }

        #endregion
    }
}
