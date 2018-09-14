namespace Melanchall.DryWetMidi.Smf
{
    public sealed class ContinueEvent : SystemRealTimeEvent
    {
        #region Overrides

        protected override MidiEvent CloneEvent()
        {
            return new ContinueEvent();
        }

        public override string ToString()
        {
            return "Continue";
        }

        #endregion
    }
}
