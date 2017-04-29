using System;

namespace Melanchall.DryMidi
{
    public sealed class SequenceTrackNameEvent : MetaEvent
    {
        #region Constructor

        public SequenceTrackNameEvent()
        {
        }

        public SequenceTrackNameEvent(string text)
            : this()
        {
            Text = text;
        }

        #endregion

        #region Properties

        public string Text { get; set; }

        #endregion

        #region Methods

        public bool Equals(SequenceTrackNameEvent sequenceTrackNameEvent)
        {
            if (ReferenceEquals(null, sequenceTrackNameEvent))
                return false;

            if (ReferenceEquals(this, sequenceTrackNameEvent))
                return true;

            return base.Equals(sequenceTrackNameEvent) && Text == sequenceTrackNameEvent.Text;
        }

        #endregion

        #region Overrides

        protected override void ReadContentData(MidiReader reader, ReadingSettings settings, int size)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(size),
                    size,
                    "Non-negative size have to be specified in order to read Sequence/Track Name event.");

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
            return base.GetHashCode() ^ (Text?.GetHashCode() ?? 0);
        }

        #endregion
    }
}
