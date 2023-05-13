using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Holds information about new time for an object that was calculated during quantization.
    /// </summary>
    public sealed class QuantizedTime
    {
        #region Constructor

        internal QuantizedTime(
            long newTime,
            long gridTime,
            ITimeSpan shift,
            long distanceToGridTime,
            ITimeSpan convertedDistanceToGridTime)
        {
            NewTime = newTime;
            GridTime = gridTime;
            Shift = shift;
            DistanceToGridTime = distanceToGridTime;
            ConvertedDistanceToGridTime = convertedDistanceToGridTime;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the new time of an object.
        /// </summary>
        public long NewTime { get; }

        /// <summary>
        /// Gets a grid time that was selected for an object as the nearest one.
        /// </summary>
        public long GridTime { get; }

        /// <summary>
        /// Gets the distance an object is going to be moved toward the new time.
        /// </summary>
        public ITimeSpan Shift { get; }

        /// <summary>
        /// Gets the distance between an object's current time and the nearest grid time.
        /// </summary>
        public long DistanceToGridTime { get; }

        /// <summary>
        /// Gets the distance between an object's current time and the nearest grid time as time span
        /// of the type specified by <see cref="QuantizingSettings.DistanceCalculationType"/>.
        /// </summary>
        public ITimeSpan ConvertedDistanceToGridTime { get; }

        #endregion
    }
}
