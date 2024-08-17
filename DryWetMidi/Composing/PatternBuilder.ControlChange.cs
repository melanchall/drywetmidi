using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Composing
{
    public sealed partial class PatternBuilder
    {
        #region Methods

        public PatternBuilder ControlChange(SevenBitNumber controlNumber, SevenBitNumber controlValue)
        {
            return AddAction(new AddControlChangeEventAction(controlNumber, controlValue));
        }

        #endregion
    }
}
