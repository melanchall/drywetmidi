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

        #region Methods

        public bool Equals(InstrumentNameEvent instrumentNameEvent)
        {
            return Equals(instrumentNameEvent, true);
        }

        public bool Equals(InstrumentNameEvent instrumentNameEvent, bool respectDeltaTime)
        {
            if (ReferenceEquals(null, instrumentNameEvent))
                return false;

            if (ReferenceEquals(this, instrumentNameEvent))
                return true;

            return base.Equals(instrumentNameEvent, respectDeltaTime);
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
