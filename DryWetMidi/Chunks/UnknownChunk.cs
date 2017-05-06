namespace Melanchall.DryWetMidi
{
    public sealed class UnknownChunk : MidiChunk
    {
        #region Constructor

        public UnknownChunk(string id)
            : base(id)
        {
        }

        #endregion

        #region Properties

        public byte[] Data { get; set; }

        #endregion

        #region Overrides

        protected override void ReadContent(MidiReader reader, ReadingSettings settings, uint size)
        {
            var availableSize = reader.Length - reader.Position;
            var bytes = reader.ReadBytes((int)(availableSize < size ? availableSize : size));
            if (bytes.Length < size && settings.NotEnoughBytesPolicy == NotEnoughBytesPolicy.Abort)
                throw new NotEnoughBytesException("Chunk's data cannot be read since the reader's underlying stream doesn't have enough bytes.",
                                                  size,
                                                  bytes.Length);

            Data = bytes;
        }

        protected override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
            var data = Data;
            if (data != null)
                writer.WriteBytes(data);
        }

        protected override uint GetContentSize(WritingSettings settings)
        {
            return (uint)(Data?.Length ?? 0);
        }

        public override string ToString()
        {
            return "Unknown Chunk";
        }

        #endregion
    }
}
