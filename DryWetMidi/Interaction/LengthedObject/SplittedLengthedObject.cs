namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Container for left and right parts of splitted lengthed object.
    /// </summary>
    /// <typeparam name="TObject">The type of splitted object.</typeparam>
    public sealed class SplittedLengthedObject<TObject>
        where TObject : ILengthedObject
    {
        #region Constructor

        internal SplittedLengthedObject(TObject leftPart, TObject rightPart)
        {
            LeftPart = leftPart;
            RightPart = rightPart;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The left part of a splitted object.
        /// </summary>
        public TObject LeftPart { get; }

        /// <summary>
        /// The right part of a splitted object.
        /// </summary>
        public TObject RightPart { get; }

        #endregion
    }
}
