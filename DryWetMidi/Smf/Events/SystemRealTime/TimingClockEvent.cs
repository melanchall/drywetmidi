namespace Melanchall.DryWetMidi.Smf
{
    public sealed class TimingClockEvent : SystemRealTimeEvent
    {
        #region Overrides

        protected override MidiEvent CloneEvent()
        {
            return new TimingClockEvent();
        }

        public override string ToString()
        {
            return "Timing Clock";
        }

        #endregion
    }
}
