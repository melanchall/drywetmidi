namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Represents a group of snap points.
    /// </summary>
    public sealed class SnapPointsGroup
    {
        #region Constructor

        internal SnapPointsGroup()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether the current <see cref="SnapPointsGroup"/>
        /// is enabled or not.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        #endregion
    }
}
