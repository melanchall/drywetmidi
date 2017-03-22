using System;

namespace Melanchall.DryMidi
{
    public sealed class LyricMessage : MetaMessage
    {
        #region Constructor

        public LyricMessage()
        {
        }

        public LyricMessage(string text)
            : this()
        {
            Text = text;
        }

        #endregion

        #region Properties

        public string Text { get; set; }

        #endregion

        #region Overrides

        public override void ReadContent(MidiReader reader, ReadingSettings settings, int size = -1)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(size),
                    size,
                    "Non-negative size have to be specified in order to read Lyric message.");

            Text = reader.ReadString(size);
        }

        public override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteString(Text);
        }

        public override int GetContentSize()
        {
            return Text?.Length ?? 0;
        }

        protected override Message CloneMessage()
        {
            return new LyricMessage(Text);
        }

        public override string ToString()
        {
            return $"Lyric (text = {Text})";
        }

        #endregion
    }
}
