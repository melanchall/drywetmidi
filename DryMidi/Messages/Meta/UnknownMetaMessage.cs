using System;

namespace Melanchall.DryMidi
{
    public sealed class UnknownMetaMessage : MetaMessage
    {
        #region Constructor

        internal UnknownMetaMessage(byte statusByte)
        {
            StatusByte = statusByte;
        }

        #endregion

        #region Properties

        public byte StatusByte { get; set; }

        public byte[] Data { get; set; }

        #endregion

        #region Overrides

        internal override void ReadContent(MidiReader reader, ReadingSettings settings, int size = -1)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(size),
                    size,
                    "Non-negative size have to be specified in order to read Unknown meta message.");

            Data = reader.ReadBytes(size);
        }

        internal override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
            var data = Data;
            if (data != null)
                writer.WriteBytes(data);
        }

        internal override int GetContentSize()
        {
            return Data?.Length ?? 0;
        }

        protected override Message CloneMessage()
        {
            return new UnknownMetaMessage(StatusByte);
        }

        public override string ToString()
        {
            return $"Unknown meta (status-byte = {StatusByte})";
        }

        #endregion
    }
}
