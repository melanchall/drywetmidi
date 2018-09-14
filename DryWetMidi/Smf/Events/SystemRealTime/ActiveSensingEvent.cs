namespace Melanchall.DryWetMidi.Smf
{
    public sealed class ActiveSensingEvent : SystemRealTimeEvent
    {
        #region Overrides

        protected override MidiEvent CloneEvent()
        {
            return new ActiveSensingEvent();
        }

        public override string ToString()
        {
            return "Active Sensing";
        }

        #endregion
    }
}
