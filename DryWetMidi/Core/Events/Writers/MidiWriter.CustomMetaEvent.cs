using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Core
{
    public sealed partial class MidiWriter
    {
        internal async Task WriteCustomMetaEventPayloadAsync(
            MetaEvent midiEvent,
            WritingSettings settings,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Only custom payload callbacks require a synchronous writer. Preserve
            // its byte count without buffering the event's status or length.
            using var payload = new MemoryStream();
            using (var payloadWriter = new MidiWriter(payload, new WriterSettings { UseBuffering = false }))
            {
                payloadWriter._length = _length;
                midiEvent.Write(payloadWriter, settings);
            }

            await WriteBytesAsync(
                payload.GetBuffer(),
                0,
                checked((int)payload.Length),
                cancellationToken).ConfigureAwait(false);
        }
    }
}
