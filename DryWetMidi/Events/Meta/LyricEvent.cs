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

        #region Methods

        public bool Equals(LyricEvent lyricEvent)
        {
            return Equals(lyricEvent, true);
        }

        public bool Equals(LyricEvent lyricEvent, bool respectDeltaTime)
        {
            if (ReferenceEquals(null, lyricEvent))
                return false;

            if (ReferenceEquals(this, lyricEvent))
                return true;

            return base.Equals(lyricEvent, respectDeltaTime);
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
