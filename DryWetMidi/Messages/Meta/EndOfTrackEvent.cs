namespace Melanchall.DryMidi
{
    public sealed class EndOfTrackEvent : MetaEvent
    {
        #region Methods

        public bool Equals(EndOfTrackEvent endOfTrackEvent)
        {
            if (ReferenceEquals(null, endOfTrackEvent))
                return false;

            if (ReferenceEquals(this, endOfTrackEvent))
                return true;

            return base.Equals(endOfTrackEvent);
        }

        #endregion

        #region Overrides

        protected override void ReadContentData(MidiReader reader, ReadingSettings settings, int size)
        {
        }

        protected override void WriteContentData(MidiWriter writer, WritingSettings settings)
        {
        }

        protected override int GetContentDataSize()
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
