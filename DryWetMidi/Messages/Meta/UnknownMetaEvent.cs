using System;

namespace Melanchall.DryWetMidi
{
    public sealed class UnknownMetaEvent : MetaEvent
    {
        #region Constructor

        internal UnknownMetaEvent(byte statusByte)
        {
            StatusByte = statusByte;
        }

        #endregion

        #region Properties

        public byte StatusByte { get; set; }

        public byte[] Data { get; set; }

        #endregion

        #region Methods

        public bool Equals(UnknownMetaEvent unknownMetaEvent)
        {
            if (ReferenceEquals(null, unknownMetaEvent))
                return false;

            if (ReferenceEquals(this, unknownMetaEvent))
                return true;

            return base.Equals(unknownMetaEvent) && ArrayUtilities.Equals(Data, unknownMetaEvent.Data);
        }

        #endregion

        #region Overrides

        protected override void ReadContentData(MidiReader reader, ReadingSettings settings, int size)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(size),
                    size,
                    "Non-negative size have to be specified in order to read Unknown meta event.");

            Data = reader.ReadBytes(size);
        }

        protected override void WriteContentData(MidiWriter writer, WritingSettings settings)
        {
            var data = Data;
            if (data != null)
                writer.WriteBytes(data);
        }

        protected override int GetContentDataSize()
        {
            return Data?.Length ?? 0;
        }

        protected override MidiEvent CloneEvent()
        {
            return new UnknownMetaEvent(StatusByte);
        }

        public override string ToString()
        {
            return $"Unknown meta (status-byte = {StatusByte})";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as UnknownMetaEvent);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ (Data?.GetHashCode() ?? 0);
        }

        #endregion
    }
}
