namespace Melanchall.DryWetMidi
{
    public sealed class TextEvent : BaseTextEvent
    {
        #region Constructor

        public TextEvent()
        {
        }

        public TextEvent(string text)
            : base(text)
        {
        }

        #endregion

        #region Methods

        public bool Equals(TextEvent textEvent)
        {
            return Equals(textEvent, true);
        }

        public bool Equals(TextEvent textEvent, bool respectDeltaTime)
        {
            if (ReferenceEquals(null, textEvent))
                return false;

            if (ReferenceEquals(this, textEvent))
                return true;

            return base.Equals(textEvent, respectDeltaTime);
        }

        #endregion

        #region Overrides

        protected override MidiEvent CloneEvent()
        {
            return new TextEvent(Text);
        }

        public override string ToString()
        {
            return $"Text (text = {Text})";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as TextEvent);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion
    }
}
