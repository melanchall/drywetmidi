namespace Melanchall.DryWetMidi.Smf
{
    public sealed class StartEvent : SystemRealTimeEvent
    {
        #region Overrides

        protected override MidiEvent CloneEvent()
        {
            return new StartEvent();
        }

        public override string ToString()
        {
            return "Start";
        }

        #endregion
    }
}
