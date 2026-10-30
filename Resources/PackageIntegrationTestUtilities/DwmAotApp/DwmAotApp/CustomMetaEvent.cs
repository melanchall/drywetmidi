using Melanchall.DryWetMidi.Core;
using System.Threading;
using System.Threading.Tasks;

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

        protected override async Task ReadContentAsync(
            MidiReader reader,
            ReadingSettings settings,
            int size,
            CancellationToken cancellationToken = default)
        {
            X = await reader.ReadByteAsync(cancellationToken).ConfigureAwait(false);
        }

        protected override async Task WriteContentAsync(
            MidiWriter writer,
            WritingSettings settings,
            CancellationToken cancellationToken = default)
        {
            await writer.WriteByteAsync(X, cancellationToken).ConfigureAwait(false);
        }

        public override bool Equals(object obj) =>
            obj is CustomMetaEvent customMetaEvent && customMetaEvent.X == X;

        public override int GetHashCode() =>
            X.GetHashCode();
    }
}
