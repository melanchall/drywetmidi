using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Core
{
    public sealed partial class MidiFile
    {
        private static async Task<MidiFile> ReadCoreAsync(
            Stream stream,
            ReadingSettings settings,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!stream.CanRead)
                throw new ArgumentException("Stream doesn't support reading.", nameof(stream));

            settings.ReaderSettings ??= new ReaderSettings();
            var file = new MidiFile();
            int? expected = null;
            var actual = 0;
            var hasHeader = false;
            using var reader = new MidiReader(stream, settings.ReaderSettings);
            try
            {
                if (await reader.EndReachedAsync(cancellationToken).ConfigureAwait(false))
                    throw new ArgumentException("Stream is already read.", nameof(stream));

                if (settings.MaxMemorySize != null)
                    reader.AddEstimatedMemory(
                        MemorySizeEstimator.EstimateShallow(file) +
                        MemorySizeEstimator.EstimateShallow(file.Chunks) +
                        MemorySizeEstimator.EstimateShallow(new List<MidiChunk>()),
                        settings);

                var end = await MidiFileReadingUtilities.ReadRmidPreambleAsync(reader, cancellationToken).ConfigureAwait(false);

                while (!await reader.EndReachedAsync(cancellationToken).ConfigureAwait(false) &&
                    (end == null || reader.Position < end))
                {
                    if (expected != null && actual == expected && settings.StopReadingOnExpectedTrackChunksCountReached)
                        return file;

                    var chunk = await ReadChunkAsync(reader, settings, actual, expected, cancellationToken).ConfigureAwait(false);
                    if (chunk == null)
                        continue;

                    if (chunk is HeaderChunk header)
                    {
                        if (!hasHeader)
                        {
                            expected = header.TracksNumber;
                            file.TimeDivision = header.TimeDivision;
                            file._originalFormat = header.FileFormat;
                            if (settings.MaxMemorySize != null)
                                reader.AddEstimatedMemory(MemorySizeEstimator.EstimateShallow(header.TimeDivision), settings);
                        }

                        hasHeader = true;
                        continue;
                    }

                    if (chunk is TrackChunk)
                        actual++;

                    var capacity = file.Chunks.Capacity;
                    file.Chunks.Add(chunk);
                    if (settings.MaxMemorySize != null)
                    {
                        if (capacity != file.Chunks.Capacity)
                            reader.AddEstimatedMemory(
                                MemorySizeEstimator.EstimateReferencesArray(file.Chunks.Capacity) -
                                MemorySizeEstimator.EstimateReferencesArray(capacity),
                                settings);

                        file.EstimatedMemorySize = reader.EstimatedMemorySize;
                    }
                }

                if (expected != null && actual != expected)
                    ReactOnUnexpectedTrackChunksCount(settings.UnexpectedTrackChunksCountPolicy, actual, expected.Value);

                if (!hasHeader)
                {
                    file.TimeDivision = null;
                    if (settings.NoHeaderChunkPolicy == NoHeaderChunkPolicy.Abort)
                        throw new NoHeaderChunkException();
                }
            }
            catch (NotEnoughBytesException exception)
            {
                ReactOnNotEnoughBytes(settings.NotEnoughBytesPolicy, exception);
            }
            catch (EndOfStreamException exception)
            {
                ReactOnNotEnoughBytes(settings.NotEnoughBytesPolicy, exception);
            }

            return file;
        }

        private static async Task<MidiChunk?> ReadChunkAsync(
            MidiReader reader,
            ReadingSettings settings,
            int actual,
            int? expected,
            CancellationToken cancellationToken)
        {
            var idBytes = await reader.ReadBytesAsync(4, cancellationToken).ConfigureAwait(false);
            if (idBytes.Length < 4)
            {
                if (settings.NotEnoughBytesPolicy == NotEnoughBytesPolicy.Abort)
                    throw new NotEnoughBytesException("Not enough bytes to read a chunk ID.", 4, idBytes.Length);

                return null;
            }

            var id = SmfConstants.DefaultTextEncoding.GetString(idBytes);
            MidiChunk? chunk = id == HeaderChunk.Id ? new HeaderChunk() :
                id == TrackChunk.Id ? new TrackChunk() : MidiFileReadingUtilities.TryCreateChunk(id, settings.CustomChunkTypes);
            if (chunk == null && settings.UnknownChunkIdPolicy == UnknownChunkIdPolicy.Abort)
                throw new UnknownChunkException(id);

            if (chunk == null && settings.UnknownChunkIdPolicy == UnknownChunkIdPolicy.ReadAsUnknownChunk)
                chunk = new UnknownChunk(id);

            uint size;
            try
            {
                size = await reader.ReadDwordAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (NotEnoughBytesException exception)
            {
                ReactOnNotEnoughBytes(settings.NotEnoughBytesPolicy, exception);
                return chunk;
            }
            catch (EndOfStreamException exception)
            {
                ReactOnNotEnoughBytes(settings.NotEnoughBytesPolicy, exception);
                return chunk;
            }

            var start = reader.Position;
            if ((chunk == null && settings.UnknownChunkIdPolicy == UnknownChunkIdPolicy.Skip) ||
                (chunk is TrackChunk && expected != null && actual >= expected && settings.ExtraTrackChunkPolicy == ExtraTrackChunkPolicy.Skip))
            {
                await reader.SetPositionAsync(Math.Min(start + size, reader.Length), cancellationToken).ConfigureAwait(false);
                return null;
            }

            chunk ??= new UnknownChunk(id);

            try
            {
                await chunk.ReadAsync(reader, settings, size, cancellationToken).ConfigureAwait(false);
            }
            catch (NotEnoughBytesException exception)
            {
                ReactOnNotEnoughBytes(settings.NotEnoughBytesPolicy, exception);
            }
            catch (EndOfStreamException exception)
            {
                ReactOnNotEnoughBytes(settings.NotEnoughBytesPolicy, exception);
            }

            return chunk;
        }
    }
}
