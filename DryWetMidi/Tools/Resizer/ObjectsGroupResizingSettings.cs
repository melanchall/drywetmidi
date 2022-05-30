using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public sealed class ObjectsGroupResizingSettings
    {
        #region Fields

        private TimeSpanType _distanceCalculationType = TimeSpanType.Midi;

        #endregion

        #region Properties

        public TimeSpanType DistanceCalculationType
        {
            get { return _distanceCalculationType; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _distanceCalculationType = value;
            }
        }

        #endregion
    }
}
