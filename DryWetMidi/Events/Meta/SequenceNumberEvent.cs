namespace Melanchall.DryWetMidi
{
    public sealed class SequenceNumberEvent : MetaEvent
    {
        #region Constructor

        public SequenceNumberEvent()
        {
        }

        public SequenceNumberEvent(short number)
            : this()
        {
            Number = number;
        }

        #endregion

        #region Properties

        public short Number { get; set; }

        #endregion

        #region Methods

        public bool Equals(SequenceNumberEvent sequenceNumberEvent)
        {
            if (ReferenceEquals(null, sequenceNumberEvent))
                return false;

            if (ReferenceEquals(this, sequenceNumberEvent))
                return true;

            return base.Equals(sequenceNumberEvent) && Number == sequenceNumberEvent.Number;
        }

        #endregion

        #region Overrides

        protected override void ReadContentData(MidiReader reader, ReadingSettings settings, int size)
        {
            Number = reader.ReadInt16();
        }

        protected override void WriteContentData(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteInt16(Number);
        }

        protected override int GetContentDataSize()
        {
            return 2;
        }

        protected override MidiEvent CloneEvent()
        {
            return new SequenceNumberEvent(Number);
        }

        public override string ToString()
        {
            return $"Sequence Number (number = {Number})";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SequenceNumberEvent);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ Number.GetHashCode();
        }

        #endregion
    }
}
