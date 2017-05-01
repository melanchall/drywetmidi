namespace Melanchall.DryWetMidi
{
    public sealed class InstrumentNameEvent : BaseTextEvent
    {
        #region Constructor

        public InstrumentNameEvent()
        {
        }

        public InstrumentNameEvent(string instrumentName)
            : base(instrumentName)
        {
        }

        #endregion

        #region Overrides

        protected override MidiEvent CloneEvent()
        {
            return new InstrumentNameEvent(Text);
        }

        public override string ToString()
        {
            return $"Instrument Name (instrument name = {Text})";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as InstrumentNameEvent);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion
    }
}
