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

        #region Methods

        public bool Equals(UnknownMetaMessage unknownMetaMessage)
        {
            if (ReferenceEquals(null, unknownMetaMessage))
                return false;

            if (ReferenceEquals(this, unknownMetaMessage))
                return true;

            return base.Equals(unknownMetaMessage) && ArrayUtilities.Equals(Data, unknownMetaMessage.Data);
        }

        #endregion

        #region Overrides

        protected override void ReadContentData(MidiReader reader, ReadingSettings settings, int size)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(size),
                    size,
                    "Non-negative size have to be specified in order to read Unknown meta message.");

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

        protected override Message CloneMessage()
        {
            return new UnknownMetaMessage(StatusByte);
        }

        public override string ToString()
        {
            return $"Unknown meta (status-byte = {StatusByte})";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as UnknownMetaMessage);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ (Data?.GetHashCode() ?? 0);
        }

        #endregion
    }
}
