using Melanchall.DryWetMidi.Core;

namespace DwmAotApp
{
    internal sealed class CustomMetaEvent : MetaEvent
    {
        public byte X { get; set; }

        protected override MidiEvent CloneEvent() =>
            new CustomMetaEvent { X = X };

        protected override int GetContentSize(WritingSettings settings) =>
            1;

        protected override void ReadContent(MidiReader reader, ReadingSettings settings, int size) =>
            X = reader.ReadByte();

        protected override void WriteContent(MidiWriter writer, WritingSettings settings) =>
            writer.WriteByte(X);

        public override bool Equals(object obj) =>
            obj is CustomMetaEvent customMetaEvent && customMetaEvent.X == X;

        public override int GetHashCode() =>
            X.GetHashCode();
    }
}
