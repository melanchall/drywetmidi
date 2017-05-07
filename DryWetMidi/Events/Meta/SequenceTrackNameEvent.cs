namespace Melanchall.DryWetMidi
{
    public sealed class SequenceTrackNameEvent : BaseTextEvent
    {
        #region Constructor

        public SequenceTrackNameEvent()
        {
        }

        public SequenceTrackNameEvent(string text)
            : base(text)
        {
        }

        #endregion

        #region Methods

        public bool Equals(SequenceTrackNameEvent sequenceTrackNameEvent)
        {
            return Equals(sequenceTrackNameEvent, true);
        }

        public bool Equals(SequenceTrackNameEvent sequenceTrackNameEvent, bool respectDeltaTime)
        {
            if (ReferenceEquals(null, sequenceTrackNameEvent))
                return false;

            if (ReferenceEquals(this, sequenceTrackNameEvent))
                return true;

            return base.Equals(sequenceTrackNameEvent, respectDeltaTime);
        }

        #endregion

        #region Overrides

        protected override MidiEvent CloneEvent()
        {
            return new SequenceTrackNameEvent(Text);
        }

        public override string ToString()
        {
            return $"Sequence/Track Name (text = {Text})";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SequenceTrackNameEvent);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion
    }
}
