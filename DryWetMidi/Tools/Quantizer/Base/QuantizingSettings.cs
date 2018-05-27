using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public abstract class QuantizingSettings
    {
        #region Fields

        private TimeSpanType _distanceType = TimeSpanType.Midi;

        #endregion

        #region Properties

        public TimeSpanType DistanceType
        {
            get { return _distanceType; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _distanceType = value;
            }
        }

        #endregion
    }
}
