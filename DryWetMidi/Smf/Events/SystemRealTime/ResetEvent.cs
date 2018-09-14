namespace Melanchall.DryWetMidi.Smf
{
    public sealed class ResetEvent : SystemRealTimeEvent
    {
        #region Overrides

        protected override MidiEvent CloneEvent()
        {
            return new ResetEvent();
        }

        public override string ToString()
        {
            return "Reset";
        }

        #endregion
    }
}
