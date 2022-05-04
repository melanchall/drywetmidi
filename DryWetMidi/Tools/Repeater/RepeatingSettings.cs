using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public sealed class RepeatingSettings
    {
        #region Fields

        private ShiftPolicy _shiftPolicy = ShiftPolicy.ShiftByMaxTime;

        #endregion

        #region Properties

        public ShiftPolicy ShiftPolicy
        {
            get { return _shiftPolicy; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);
                
                _shiftPolicy = value;
            }
        }

        public ITimeSpan Shift { get; set; }

        // TODO: provide an option to round to floor and not to ceiling only
        public ITimeSpan ShiftStep { get; set; }

        public bool SaveTempoMap { get; set; } = true;

        #endregion
    }
}
