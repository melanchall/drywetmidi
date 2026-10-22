using Melanchall.DryWetMidi.Common;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Core
{
    internal static partial class MidiFileReadingUtilities
    {
        internal static async ValueTask<long?> ReadRmidPreambleAsync(MidiReader reader, CancellationToken cancellationToken)
        {
            var bytes = await reader.ReadBytesAsync(4, cancellationToken).ConfigureAwait(false);
            if (SmfConstants.DefaultTextEncoding.GetString(bytes) != RiffChunkId)
            {
                await reader.SetPositionAsync(reader.Position - bytes.Length, cancellationToken).ConfigureAwait(false);
                return null;
            }
            await reader.SetPositionAsync(reader.Position + RmidPreambleSize, cancellationToken).ConfigureAwait(false);
            var size = await reader.ReadDwordAsync(cancellationToken).ConfigureAwait(false);
            return reader.Position + size;
        }

        internal static async Task<(MidiEvent? Event, byte? Status)> ReadEventAsync(
            MidiReader reader, ReadingSettings settings, byte? runningStatus, CancellationToken cancellationToken)
        {
            var position = reader.Position;
            using var frame = new MemoryStream();
            await ReadVlqAsync(reader, frame, 9, long.MaxValue >> 7, cancellationToken).ConfigureAwait(false);
            var status = await reader.ReadByteAsync(cancellationToken).ConfigureAwait(false);
            frame.WriteByte(status);
            var effectiveStatus = status;
            var firstDataByte = false;
            if (status < 0x80)
            {
                if (runningStatus == null)
                    throw new UnexpectedRunningStatusException();
                effectiveStatus = runningStatus.Value;
                firstDataByte = true;
            }

            if (effectiveStatus >= 0x80 && effectiveStatus < 0xF0)
            {
                var size = (effectiveStatus & 0xE0) == 0xC0 ? 1 : 2;
                if (firstDataByte)
                    size--;
                await reader.CopyBytesAsync(frame, size, cancellationToken).ConfigureAwait(false);
            }
            else if (effectiveStatus == 0xFF || effectiveStatus == 0xF0 || effectiveStatus == 0xF7)
            {
                byte? metaType = null;
                if (effectiveStatus == 0xFF)
                {
                    metaType = await reader.ReadByteAsync(cancellationToken).ConfigureAwait(false);
                    frame.WriteByte(metaType.Value);
                }
                var size = await ReadVlqAsync(reader, frame, 5, int.MaxValue >> 7, cancellationToken).ConfigureAwait(false);
                var minimum = metaType switch
                {
                    0x20 => 1,
                    0x51 => 3,
                    0x54 => 5,
                    0x59 => 2,
                    _ => 0
                };
                size = Math.Max(size, minimum);
                reader.ThrowIfMemoryLimitExceeded(Math.Min(size, Math.Max(0, reader.Length - reader.Position)), settings);
                await reader.CopyBytesAsync(frame, size, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var skip = 0;
                var head = (FourBitNumber)(effectiveStatus >> 4);
                var channel = (FourBitNumber)(effectiveStatus & 15);
                switch (settings.UnknownChannelEventPolicy)
                {
                    case UnknownChannelEventPolicy.Abort:
                        throw new UnknownChannelEventException(head, channel);
                    case UnknownChannelEventPolicy.SkipStatusByteAndOneDataByte:
                        skip = 1;
                        break;
                    case UnknownChannelEventPolicy.SkipStatusByteAndTwoDataBytes:
                        skip = 2;
                        break;
                    case UnknownChannelEventPolicy.UseCallback:
                        var callback = settings.UnknownChannelEventCallback ??
                            throw new InvalidOperationException("Unknown channel event callback is not set.");
                        var action = callback(head, channel);
                        if (action.Instruction == UnknownChannelEventInstruction.Abort)
                            throw new UnknownChannelEventException(head, channel);
                        skip = action.DataBytesToSkipCount;
                        break;
                }
                await reader.SetPositionAsync(reader.Position + skip, cancellationToken).ConfigureAwait(false);
                return (null, runningStatus);
            }

            using var codec = new MidiReader(frame.GetBuffer(), 0, checked((int)frame.Length), settings.ReaderSettings, position);
            codec.AddEstimatedMemory(reader.EstimatedMemorySize, settings);
            var midiEvent = TrackChunk.ReadEvent(codec, settings, ref runningStatus);
            return (midiEvent, runningStatus);
        }

        private static async ValueTask<long> ReadVlqAsync(MidiReader reader, MemoryStream frame,
            int maxBytes, long lastCheck, CancellationToken cancellationToken)
        {
            long value = 0;
            for (var count = 1; ; count++)
            {
                if (count > maxBytes)
                    throw new VlqNumberOverflowException("VLQ number is too large.");
                var b = await reader.ReadByteAsync(cancellationToken).ConfigureAwait(false);
                frame.WriteByte(b);
                if (count == maxBytes && ((b >> 7) != 0 || value > lastCheck))
                    throw new VlqNumberOverflowException("VLQ number is too large.");
                value = (value << 7) + (b & 127);
                if (b < 128)
                    return value;
            }
        }
    }
}
