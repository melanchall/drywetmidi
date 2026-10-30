using Melanchall.DryWetMidi.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Core
{
    internal static partial class MidiFileReadingUtilities
    {
        internal static async ValueTask<long?> ReadRmidPreambleAsync(MidiReader reader, CancellationToken cancellationToken)
        {
            var chunkId = await reader.ReadStringAsync(RiffChunkId.Length, cancellationToken).ConfigureAwait(false);
            if (chunkId == RiffChunkId)
            {
                await reader.SetPositionAsync(reader.Position + RmidPreambleSize, cancellationToken).ConfigureAwait(false);
                var size = await reader.ReadDwordAsync(cancellationToken).ConfigureAwait(false);
                return reader.Position + size;
            }

            await reader.SetPositionAsync(reader.Position - chunkId.Length, cancellationToken).ConfigureAwait(false);
            return null;
        }

        internal static async Task<(MidiEvent? Event, byte? Status)> ReadEventAsync(
            MidiReader reader,
            ReadingSettings settings,
            byte? runningStatus,
            CancellationToken cancellationToken)
        {
            var deltaTime = await reader.ReadVlqLongNumberAsync(cancellationToken).ConfigureAwait(false);
            if (deltaTime < 0)
                deltaTime = 0;

            var statusByte = await reader.ReadByteAsync(cancellationToken).ConfigureAwait(false);
            if (statusByte <= SevenBitNumber.MaxValue)
            {
                if (runningStatus == null)
                    throw new UnexpectedRunningStatusException();

                statusByte = runningStatus.Value;
                await reader.SetPositionAsync(reader.Position - 1, cancellationToken).ConfigureAwait(false);
            }

            var eventReader = EventReaderFactory.GetReader(statusByte, smfOnly: true);
            var midiEvent = await eventReader.ReadAsync(reader, settings, statusByte, cancellationToken).ConfigureAwait(false);

            if (midiEvent is ChannelEvent)
                runningStatus = statusByte;

            if (midiEvent != null)
                midiEvent._deltaTime = deltaTime;

            return (midiEvent, runningStatus);
        }
    }
}
