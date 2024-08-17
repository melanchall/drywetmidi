using Melanchall.DryWetMidi.Composing.Actions;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Composing
{
    internal sealed class AddPitchBendEventAction : AddChannelEventAction<PitchBendEvent>
    {
        #region Constructor

        public AddPitchBendEventAction(ushort pitchValue)
        {
            PitchValue = pitchValue;
        }

        #endregion

        #region Properties

        public ushort PitchValue { get; }

        #endregion

        #region Overrides

        public override PatternAction Clone()
        {
            return new AddPitchBendEventAction(PitchValue);
        }

        protected override PitchBendEvent CreateEvent()
        {
            return new PitchBendEvent(PitchValue);
        }

        #endregion
    }
}
