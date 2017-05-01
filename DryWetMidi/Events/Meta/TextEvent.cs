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
