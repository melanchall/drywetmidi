using System;

namespace Melanchall.DryWetMidi
{
    public sealed class LyricEvent : MetaEvent
    {
        #region Constructor

        public LyricEvent()
        {
        }

        public LyricEvent(string text)
            : this()
        {
            Text = text;
        }

        #endregion

        #region Properties

        public string Text { get; set; }

        #endregion

        #region Methods

        public bool Equals(LyricEvent lyricEvent)
        {
            if (ReferenceEquals(null, lyricEvent))
                return false;

            if (ReferenceEquals(this, lyricEvent))
                return true;

            return base.Equals(lyricEvent) && Text == lyricEvent.Text;
        }

        #endregion

        #region Overrides

        protected override void ReadContentData(MidiReader reader, ReadingSettings settings, int size)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(size),
                    size,
                    "Non-negative size have to be specified in order to read Lyric event.");

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
            return base.GetHashCode() ^ (Text?.GetHashCode() ?? 0);
        }

        #endregion
    }
}
