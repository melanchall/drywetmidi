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
