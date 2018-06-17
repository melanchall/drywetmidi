using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public sealed class QuantizedTime
    {
        #region Constructor

        internal QuantizedTime(long newTime,
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

        public long NewTime { get; }

        public long GridTime { get; }

        public ITimeSpan Shift { get; }

        public long DistanceToGridTime { get; }

        public ITimeSpan ConvertedDistanceToGridTime { get; }

        #endregion
    }
}
