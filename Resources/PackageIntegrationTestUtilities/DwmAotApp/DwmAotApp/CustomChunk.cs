using Melanchall.DryWetMidi.Core;

namespace DwmAotApp
{
    internal sealed class CustomChunk : MidiChunk
    {
        public CustomChunk()
                : base("cust")
        {
        }

        public byte X { get; set; }

        public override MidiChunk Clone() =>
            new CustomChunk { X = X };

        protected override uint GetContentSize(WritingSettings settings) =>
            1;

        protected override void ReadContent(MidiReader reader, ReadingSettings settings, uint size) =>
            X = reader.ReadByte();

        protected override void WriteContent(MidiWriter writer, WritingSettings settings) =>
            writer.WriteByte(X);

        public override bool Equals(object obj) =>
            obj is CustomChunk customChunk && customChunk.X == X;

        public override int GetHashCode() =>
            X.GetHashCode();
    }
}
