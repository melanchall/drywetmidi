using Melanchall.DryWetMidi.Common;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Core
{
    internal static class MidiFileReadingUtilities
    {
        #region Constants

        private const string RiffChunkId = "RIFF";
        private const int RmidPreambleSize = 12; // RMID_size (4) + 'RMID' (4) + 'data' (4)

        #endregion

        #region Methods

        public static void ReadRmidPreamble(MidiReader reader, out long? smfEndPosition)
        {
            smfEndPosition = null;

            var chunkId = reader.ReadString(RiffChunkId.Length);
            if (chunkId == RiffChunkId)
            {
                reader.Position += RmidPreambleSize;
                var smfSize = reader.ReadDword();
                smfEndPosition = reader.Position + smfSize;
            }
            else
                reader.Position -= chunkId.Length;
        }

        public static MidiChunk? TryCreateChunk(string chunkId, ChunkTypesCollection? chunksTypes)
        {
            // TODO: get rid of Activator
            if (chunksTypes?.TryGetType(chunkId, out var type) == true && type != null && IsChunkType(type))
            {
                // TODO: proper exception
                var chunkObject = Activator.CreateInstance(type);
                if (chunkObject == null)
                    throw new InvalidOperationException($"Cannot create an instance of '{type.FullName}' type.");

                return (MidiChunk)chunkObject;
            }

            return null;
        }

        internal static async ValueTask<long?> ReadRmidPreambleAsync(
            MidiReader reader,
            CancellationToken cancellationToken)
        {
            var chunkId = await reader.ReadStringAsync(RiffChunkId.Length, cancellationToken).ConfigureAwait(false);
            if (chunkId == RiffChunkId)
            {
                await reader.SetPositionAsync(reader.Position + RmidPreambleSize, cancellationToken).ConfigureAwait(false);
                var smfSize = await reader.ReadDwordAsync(cancellationToken).ConfigureAwait(false);

                return reader.Position + smfSize;
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

        private static bool IsChunkType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type? type)
        {
            return type != null &&
                   type.IsSubclassOf(typeof(MidiChunk)) &&
                   type.GetConstructor(Type.EmptyTypes) != null;
        }

        #endregion
    }
}
