namespace Melanchall.DryWetMidi
{
    internal sealed class EndOfTrackEvent : MetaEvent
    {
        #region Methods

        public bool Equals(EndOfTrackEvent endOfTrackEvent)
        {
            return Equals(endOfTrackEvent, true);
        }

        public bool Equals(EndOfTrackEvent endOfTrackEvent, bool respectDeltaTime)
        {
            if (ReferenceEquals(null, endOfTrackEvent))
                return false;

            if (ReferenceEquals(this, endOfTrackEvent))
                return true;

            return base.Equals(endOfTrackEvent, respectDeltaTime);
        }

        #endregion

        #region Overrides

        protected override void ReadContent(MidiReader reader, ReadingSettings settings, int size)
        {
        }

        protected override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
        }

        protected override int GetContentSize()
        {
            return 0;
        }

        protected override MidiEvent CloneEvent()
        {
            return new EndOfTrackEvent();
        }

        public override string ToString()
        {
            return "End Of Track";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as EndOfTrackEvent);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion
    }
}
