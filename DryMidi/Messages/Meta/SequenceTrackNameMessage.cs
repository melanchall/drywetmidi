using System;

namespace Melanchall.DryMidi
{
    public sealed class SequenceTrackNameMessage : MetaMessage
    {
        #region Constructor

        public SequenceTrackNameMessage()
        {
        }

        public SequenceTrackNameMessage(string text)
            : this()
        {
            Text = text;
        }

        #endregion

        #region Properties

        public string Text { get; set; }

        #endregion

        #region Methods

        public bool Equals(SequenceTrackNameMessage sequenceTrackNameMessage)
        {
            if (ReferenceEquals(null, sequenceTrackNameMessage))
                return false;

            if (ReferenceEquals(this, sequenceTrackNameMessage))
                return true;

            return base.Equals(sequenceTrackNameMessage) && Text == sequenceTrackNameMessage.Text;
        }

        #endregion

        #region Overrides

        internal override void ReadContent(MidiReader reader, ReadingSettings settings, int size = -1)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(size),
                    size,
                    "Non-negative size have to be specified in order to read Sequence/Track Name message.");

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
            return new SequenceTrackNameMessage(Text);
        }

        public override string ToString()
        {
            return $"Sequence/Track Name (text = {Text})";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SequenceTrackNameMessage);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ (Text?.GetHashCode() ?? 0);
        }

        #endregion
    }
}
