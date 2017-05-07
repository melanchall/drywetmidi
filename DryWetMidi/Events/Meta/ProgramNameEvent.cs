namespace Melanchall.DryWetMidi
{
    public sealed class ProgramNameEvent : BaseTextEvent
    {
        #region Constructor

        public ProgramNameEvent()
        {
        }

        public ProgramNameEvent(string programName)
            : base(programName)
        {
            Text = programName;
        }

        #endregion

        #region Methods

        public bool Equals(ProgramNameEvent programNameEvent)
        {
            return Equals(programNameEvent, true);
        }

        public bool Equals(ProgramNameEvent programNameEvent, bool respectDeltaTime)
        {
            if (ReferenceEquals(null, programNameEvent))
                return false;

            if (ReferenceEquals(this, programNameEvent))
                return true;

            return base.Equals(programNameEvent, respectDeltaTime);
        }

        #endregion

        #region Overrides

        protected override MidiEvent CloneEvent()
        {
            return new ProgramNameEvent(Text);
        }

        public override string ToString()
        {
            return $"Program Name (text = {Text})";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ProgramNameEvent);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion
    }
}
