using System;

namespace Melanchall.DryWetMidi.Devices
{
    public class SnapPoint
    {
        #region Constructor

        public SnapPoint(TimeSpan time)
        {
            Time = time;
        }

        #endregion

        #region Properties

        public bool IsEnabled { get; set; } = true;

        public TimeSpan Time { get; }

        public SnapPointsGroup SnapPointsGroup { get; internal set; }

        #endregion
    }

    public sealed class SnapPoint<TData> : SnapPoint
    {
        #region Constructor

        public SnapPoint(TimeSpan time, TData data)
            : base(time)
        {
            Data = data;
        }

        #endregion

        #region Properties

        public TData Data { get; }

        #endregion
    }
}
