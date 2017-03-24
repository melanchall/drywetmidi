using System;

namespace Melanchall.DryMidi
{
    public sealed class TextMessage : MetaMessage
    {
        #region Constructor

        public TextMessage()
        {
        }

        public TextMessage(string text)
            : this()
        {
            Text = text;
        }

        #endregion

        #region Properties

        public string Text { get; set; }

        #endregion

        #region Overrides

        internal override void ReadContent(MidiReader reader, ReadingSettings settings, int size = -1)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(size),
                    size,
                    "Non-negative size have to be specified in order to read Text message.");

            Text = reader.ReadString(size);
        }

        internal override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteString(Text);
        }

        internal override int GetContentSize()
        {
            return Text?.Length ?? 0;
        }

        protected override Message CloneMessage()
        {
            return new TextMessage(Text);
        }

        public override string ToString()
        {
            return $"Text (text = {Text})";
        }

        #endregion
    }
}
