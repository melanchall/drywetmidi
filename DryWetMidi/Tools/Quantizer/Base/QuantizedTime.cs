using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public sealed class QuantizedTime
    {
        #region Constructor

        internal QuantizedTime(long time, long distance, ITimeSpan convertedDistance)
        {
            Time = time;
            Distance = distance;
            ConvertedDistance = convertedDistance;
        }

        #endregion

        #region Properties

        public long Time { get; }

        public long Distance { get; }

        public ITimeSpan ConvertedDistance { get; }

        #endregion
    }
}
