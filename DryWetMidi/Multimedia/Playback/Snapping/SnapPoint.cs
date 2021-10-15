using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Snap point for <see cref="Playback"/>.
    /// </summary>
    public class SnapPoint
    {
        #region Constructor

        internal SnapPoint(TimeSpan time)
        {
            Time = time;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether the current <see cref="SnapPoint"/>
        /// is enabled or not.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Gets the time of the current <see cref="SnapPoint"/>.
        /// </summary>
        public TimeSpan Time { get; }

        /// <summary>
        /// Gets an instance of the <see cref="SnapPointsGroup"/> the current <see cref="SnapPoint"/>
        /// belongs to; or <c>null</c> if the snap point doesn't belong to a group.
        /// </summary>
        public SnapPointsGroup SnapPointsGroup { get; internal set; }

        #endregion
    }

    /// <summary>
    /// Snap point for <see cref="Playback"/> with attached data.
    /// </summary>
    /// <typeparam name="TData">The type of data attached to snap point.</typeparam>
    public sealed class SnapPoint<TData> : SnapPoint
    {
        #region Constructor

        internal SnapPoint(TimeSpan time, TData data)
            : base(time)
        {
            Data = data;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the data attached to the current <see cref="SnapPoint{TData}"/>.
        /// </summary>
        public TData Data { get; }

        #endregion
    }
}
