using System;

namespace Melanchall.DryWetMidi
{
    public sealed class InstrumentNameEvent : MetaEvent
    {
        #region Constructor

        public InstrumentNameEvent()
        {
        }

        public InstrumentNameEvent(string instrumentName)
            : this()
        {
            InstrumentName = instrumentName;
        }

        #endregion

        #region Properties

        public string InstrumentName { get; set; }

        #endregion

        #region Methods

        public bool Equals(InstrumentNameEvent instrumentNameEvent)
        {
            if (ReferenceEquals(null, instrumentNameEvent))
                return false;

            if (ReferenceEquals(this, instrumentNameEvent))
                return true;

            return base.Equals(instrumentNameEvent) && InstrumentName == instrumentNameEvent.InstrumentName;
        }

        #endregion

        #region Overrides

        protected override void ReadContentData(MidiReader reader, ReadingSettings settings, int size)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(size),
                    size,
                    "Non-negative size have to be specified in order to read Instrument Name event.");

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

        protected override MidiEvent CloneEvent()
        {
            return new InstrumentNameEvent(InstrumentName);
        }

        public override string ToString()
        {
            return $"Instrument Name (instrument name = {InstrumentName})";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as InstrumentNameEvent);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ (InstrumentName?.GetHashCode() ?? 0);
        }

        #endregion
    }
}
