namespace Melanchall.DryWetMidi
{
    public sealed class LyricEvent : BaseTextEvent
    {
        #region Constructor

        public LyricEvent()
        {
        }

        public LyricEvent(string text)
            : base(text)
        {
        }

        #endregion

        #region Overrides

        protected override MidiEvent CloneEvent()
        {
            return new LyricEvent(Text);
        }

        public override string ToString()
        {
            return $"Lyric (text = {Text})";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as LyricEvent);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion
    }
}
