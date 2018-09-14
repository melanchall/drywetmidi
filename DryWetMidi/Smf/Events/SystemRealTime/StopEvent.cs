namespace Melanchall.DryWetMidi.Smf
{
    public sealed class StopEvent : SystemRealTimeEvent
    {
        #region Overrides

        protected override MidiEvent CloneEvent()
        {
            return new StopEvent();
        }

        public override string ToString()
        {
            return "Stop";
        }

        #endregion
    }
}
