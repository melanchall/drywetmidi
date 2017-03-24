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

        #region Overrides

        internal override void ReadContent(MidiReader reader, ReadingSettings settings, int size = -1)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(size),
                    size,
                    "Non-negative size have to be specified in order to read Instrument Name message.");

            InstrumentName = reader.ReadString(size);
        }

        internal override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteString(InstrumentName);
        }

        internal override int GetContentSize()
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

        #endregion
    }
}
