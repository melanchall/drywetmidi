using System;

namespace Melanchall.DryMidi
{
    public sealed class TextEvent : MetaEvent
    {
        #region Constructor

        public TextEvent()
        {
        }

        public TextEvent(string text)
            : this()
        {
            Text = text;
        }

        #endregion

        #region Properties

        public string Text { get; set; }

        #endregion

        #region Methods

        public bool Equals(TextEvent textEvent)
        {
            if (ReferenceEquals(null, textEvent))
                return false;

            if (ReferenceEquals(this, textEvent))
                return true;

            return Equals(textEvent) && Text == textEvent.Text;
        }

        #endregion

        #region Overrides

        protected override void ReadContentData(MidiReader reader, ReadingSettings settings, int size)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(size),
                    size,
                    "Non-negative size have to be specified in order to read Text event.");

            Text = reader.ReadString(size);
        }

        protected override void WriteContentData(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteString(Text);
        }

        protected override int GetContentDataSize()
        {
            return Text?.Length ?? 0;
        }

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
            return base.GetHashCode() ^ (Text?.GetHashCode() ?? 0);
        }

        #endregion
    }
}
