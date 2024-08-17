namespace Melanchall.DryWetMidi.Composing
{
    public sealed partial class PatternBuilder
    {
        #region Methods

        public PatternBuilder PitchBend(ushort pitchValue)
        {
            return AddAction(new AddPitchBendEventAction(pitchValue));
        }

        #endregion
    }
}
