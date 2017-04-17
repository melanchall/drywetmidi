namespace Melanchall.DryMidi
{
    public sealed class EndOfTrackMessage : MetaMessage
    {
        #region Methods

        public bool Equals(EndOfTrackMessage endOfTrackMessage)
        {
            if (ReferenceEquals(null, endOfTrackMessage))
                return false;

            if (ReferenceEquals(this, endOfTrackMessage))
                return true;

            return base.Equals(endOfTrackMessage);
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

        protected override Message CloneMessage()
        {
            return new EndOfTrackMessage();
        }

        public override string ToString()
        {
            return "End Of Track";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as EndOfTrackMessage);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion
    }
}
