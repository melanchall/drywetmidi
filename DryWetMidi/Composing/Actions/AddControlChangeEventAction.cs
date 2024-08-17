using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Composing
{
    internal sealed class AddControlChangeEventAction : PatternAction
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

        public override PatternActionResult Invoke(long time, PatternContext context)
        {
            if (State != PatternActionState.Enabled)
                return PatternActionResult.DoNothing;

            var controlChangeEvent = new ControlChangeEvent(ControlNumber, ControlValue) { Channel = context.Channel };
            var timedEvent = new TimedEvent(controlChangeEvent, time);

            return new PatternActionResult(time, new[] { timedEvent });
        }

        public override PatternAction Clone()
        {
            return new AddControlChangeEventAction(ControlNumber, ControlValue);
        }

        #endregion
    }
}
