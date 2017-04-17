using System;

namespace Melanchall.DryMidi
{
    public sealed class InstrumentNameMessage : MetaMessage
    {
        #region Constructor

        public InstrumentNameMessage()
        {
        }

        public InstrumentNameMessage(string instrumentName)
            : this()
        {
            InstrumentName = instrumentName;
        }

        #endregion

        #region Properties

        public string InstrumentName { get; set; }

        #endregion

        #region Methods

        public bool Equals(InstrumentNameMessage instrumentNameMessage)
        {
            if (ReferenceEquals(null, instrumentNameMessage))
                return false;

            if (ReferenceEquals(this, instrumentNameMessage))
                return true;

            return base.Equals(instrumentNameMessage) && InstrumentName == instrumentNameMessage.InstrumentName;
        }

        #endregion

        #region Overrides

        protected override void ReadContentData(MidiReader reader, ReadingSettings settings, int size)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(size),
                    size,
                    "Non-negative size have to be specified in order to read Instrument Name message.");

            InstrumentName = reader.ReadString(size);
        }

        protected override void WriteContentData(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteString(InstrumentName);
        }

        protected override int GetContentDataSize()
        {
            return InstrumentName?.Length ?? 0;
        }

        protected override Message CloneMessage()
        {
            return new InstrumentNameMessage(InstrumentName);
        }

        public override string ToString()
        {
            return $"Instrument Name (instrument name = {InstrumentName})";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as InstrumentNameMessage);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ (InstrumentName?.GetHashCode() ?? 0);
        }

        #endregion
    }
}
