using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Composing.Actions;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Composing
{
    internal sealed class AddControlChangeEventAction : AddChannelEventAction<ControlChangeEvent>
    {
        #region Constructor

        public AddControlChangeEventAction(SevenBitNumber controlNumber, SevenBitNumber controlValue)
        {
            ControlNumber = controlNumber;
            ControlValue = controlValue;
        }

        #endregion

        #region Properties

        public SevenBitNumber ControlNumber { get; }

        public SevenBitNumber ControlValue { get; }

        #endregion

        #region Overrides

        public override PatternAction Clone()
        {
            return new AddControlChangeEventAction(ControlNumber, ControlValue);
        }

        protected override ControlChangeEvent CreateEvent()
        {
            return new ControlChangeEvent(ControlNumber, ControlValue);
        }

        #endregion
    }
}
