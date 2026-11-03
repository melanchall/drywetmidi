using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Core
{
    public sealed partial class MidiFile
    {
        #region Methods

        /// <summary>
        /// Reads a MIDI file using asynchronous stream I/O.
        /// </summary>
        /// <param name="stream">Readable stream, left open on completion or failure.</param>
        /// <param name="settings">Reading settings, or <c>null</c> for defaults.</param>
        /// <param name="cancellationToken">Token used to cancel reading.</param>
        /// <returns>The MIDI file.</returns>
        /// <remarks>
        /// Stream reads are awaited. Chunks and events read their content using asynchronous
        /// reader methods. Whole-file buffering is used only when requested
        /// by <see cref="ReaderSettings.BufferingPolicy"/>.
        /// </remarks>
        /// <inheritdoc cref="Read(Stream, ReadingSettings)" path="/member/exception"/>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was canceled.</exception>
        public static async Task<MidiFile> ReadAsync(
            Stream stream,
            ReadingSettings? settings = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNull(nameof(stream), stream);

            var file = await ReadCoreAsync(stream, settings ?? new ReadingSettings(), cancellationToken).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            return file;
        }

        /// <summary>
        /// Reads a MIDI file from disk using asynchronous I/O.
        /// </summary>
        /// <param name="filePath">Path of the MIDI file.</param>
        /// <param name="settings">Reading settings, or <c>null</c> for defaults.</param>
        /// <param name="cancellationToken">Token used to cancel reading.</param>
        /// <returns>The MIDI file.</returns>
        /// <inheritdoc cref="Read(string, ReadingSettings)" path="/member/exception"/>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was canceled.</exception>
        public static async Task<MidiFile> ReadAsync(
            string filePath,
            ReadingSettings? settings = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(filePath), filePath, "File path is null or white space.");
            cancellationToken.ThrowIfCancellationRequested();

            MidiFile file;
            var stream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                1,
                FileOptions.Asynchronous | FileOptions.SequentialScan);

            await using (stream.ConfigureAwait(false))
            {
                file = await ReadCoreAsync(stream, settings ?? new ReadingSettings(), cancellationToken).ConfigureAwait(false);
            }

            cancellationToken.ThrowIfCancellationRequested();

            return file;
        }

        /// <summary>
        /// Writes this MIDI file using asynchronous stream I/O.
        /// </summary>
        /// <param name="stream">Writable stream, left open on completion or failure.</param>
        /// <param name="format">Output MIDI file format.</param>
        /// <param name="settings">Writing settings, or <c>null</c> for defaults.</param>
        /// <param name="cancellationToken">Token used to cancel writing.</param>
        /// <returns>A task representing writing.</returns>
        /// <remarks>
        /// Chunks and events write their content using asynchronous writer methods.
        /// Stream writes and flushing are awaited. Cancellation or failure can
        /// leave partially written data in the stream.
        /// </remarks>
        /// <inheritdoc cref="Write(Stream, MidiFileFormat, WritingSettings)" path="/member/exception"/>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was canceled.</exception>
        public async Task WriteAsync(
            Stream stream,
            MidiFileFormat format = MidiFileFormat.MultiTrack,
            WritingSettings? settings = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNull(nameof(stream), stream);
            ThrowIfArgument.IsInvalidEnumValue(nameof(format), format);
            cancellationToken.ThrowIfCancellationRequested();

            if (!stream.CanWrite)
                throw new ArgumentException("Stream doesn't support writing.", nameof(stream));

            await WriteCoreAsync(stream, format, settings, cancellationToken).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();
        }

        /// <summary>
        /// Writes this MIDI file to disk using asynchronous I/O.
        /// </summary>
        /// <param name="filePath">Output file path.</param>
        /// <param name="overwriteFile">Whether an existing file may be overwritten.</param>
        /// <param name="format">Output MIDI file format.</param>
        /// <param name="settings">Writing settings, or <c>null</c> for defaults.</param>
        /// <param name="cancellationToken">Token used to cancel writing.</param>
        /// <returns>A task representing writing.</returns>
        /// <remarks>Cancellation or failure can leave a partially written output file.</remarks>
        /// <inheritdoc cref="Write(string, bool, MidiFileFormat, WritingSettings)" path="/member/exception"/>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was canceled.</exception>
        public async Task WriteAsync(
            string filePath,
            bool overwriteFile = false,
            MidiFileFormat format = MidiFileFormat.MultiTrack,
            WritingSettings? settings = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(filePath), filePath, "File path is null or white space.");
            ThrowIfArgument.IsInvalidEnumValue(nameof(format), format);
            cancellationToken.ThrowIfCancellationRequested();

            var stream = new FileStream(
                filePath,
                overwriteFile ? FileMode.Create : FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                1,
                FileOptions.Asynchronous);

            await using (stream.ConfigureAwait(false))
            {
                await WriteCoreAsync(stream, format, settings, cancellationToken).ConfigureAwait(false);
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        /// <summary>
        /// Creates a lazy reader without synchronous stream reads, including on nonseekable streams.
        /// </summary>
        /// <param name="stream">Readable stream, left open when the reader is disposed.</param>
        /// <param name="settings">Reading settings, or <c>null</c> for defaults.</param>
        /// <param name="cancellationToken">Token used to cancel initialization.</param>
        /// <returns>A lazy MIDI token reader.</returns>
        /// <remarks>MIDI format validation is deferred until tokens are read.</remarks>
        /// <inheritdoc cref="ReadLazy(Stream, ReadingSettings)"
        /// path="/member/exception[not(@cref='T:System.UnauthorizedAccessException')]"/>
        /// <exception cref="InvalidOperationException">A custom buffer was requested but not supplied.</exception>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was canceled.</exception>
        public static async Task<MidiTokensReader> ReadLazyAsync(
            Stream stream,
            ReadingSettings? settings = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNull(nameof(stream), stream);
            cancellationToken.ThrowIfCancellationRequested();

            var reader = new MidiTokensReader(stream, settings, false);
            await reader.InitializeAsync(cancellationToken).ConfigureAwait(false);

            return reader;
        }

        /// <summary>
        /// Opens a MIDI file for lazy asynchronous reading.
        /// </summary>
        /// <param name="filePath">Path of the MIDI file.</param>
        /// <param name="settings">Reading settings, or <c>null</c> for defaults.</param>
        /// <param name="cancellationToken">Token used to cancel initialization.</param>
        /// <returns>A token reader that owns the opened stream.</returns>
        /// <remarks>MIDI format validation is deferred until tokens are read.</remarks>
        /// <inheritdoc cref="ReadLazy(string, ReadingSettings)" path="/member/exception"/>
        /// <exception cref="ArgumentException">The file is empty.</exception>
        /// <exception cref="InvalidOperationException">A custom buffer was requested but not supplied.</exception>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was canceled.</exception>
        public static async Task<MidiTokensReader> ReadLazyAsync(
            string filePath,
            ReadingSettings? settings = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(filePath), filePath, "File path is null or white space.");
            cancellationToken.ThrowIfCancellationRequested();

            var stream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                1,
                FileOptions.Asynchronous | FileOptions.SequentialScan);

            try
            {
                var reader = new MidiTokensReader(stream, settings, true);
                await reader.InitializeAsync(cancellationToken).ConfigureAwait(false);

                return reader;
            }
            catch
            {
                await stream.DisposeAsync().ConfigureAwait(false);
                throw;
            }
        }

        /// <summary>
        /// Creates a lazy writer and asynchronously writes its file header.
        /// </summary>
        /// <param name="stream">Seekable writable stream, left open when the writer is disposed.</param>
        /// <param name="settings">Writing settings, or <c>null</c> for defaults.</param>
        /// <param name="format">Output MIDI file format.</param>
        /// <param name="timeDivision">Time division, or <c>null</c> for the default.</param>
        /// <param name="cancellationToken">Token used to cancel initialization.</param>
        /// <returns>A lazy MIDI token writer.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="stream"/> cannot be written or sought.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="format"/> is invalid.</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="stream"/> is disposed.</exception>
        /// <exception cref="IOException">An I/O error occurred while writing the header.</exception>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was canceled.</exception>
        public static async Task<MidiTokensWriter> WriteLazyAsync(
            Stream stream,
            WritingSettings? settings = null,
            MidiFileFormat format = MidiFileFormat.MultiTrack,
            TimeDivision? timeDivision = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNull(nameof(stream), stream);
            ThrowIfArgument.IsInvalidEnumValue(nameof(format), format);
            cancellationToken.ThrowIfCancellationRequested();

            timeDivision ??= new TicksPerQuarterNoteTimeDivision();
            var writer = new MidiTokensWriter(stream, settings, false);
            await writer.InitializeAsync(format, timeDivision, cancellationToken).ConfigureAwait(false);

            return writer;
        }

        /// <summary>
        /// Opens a file for lazy asynchronous writing and writes its header.
        /// </summary>
        /// <param name="filePath">Output file path.</param>
        /// <param name="overwriteFile">Whether an existing file may be overwritten.</param>
        /// <param name="settings">Writing settings, or <c>null</c> for defaults.</param>
        /// <param name="format">Output MIDI file format.</param>
        /// <param name="timeDivision">Time division, or <c>null</c> for the default.</param>
        /// <param name="cancellationToken">Token used to cancel initialization.</param>
        /// <returns>A token writer that owns the opened stream.</returns>
        /// <inheritdoc cref="WriteLazy(string, bool, MidiFileFormat, WritingSettings, TimeDivision)"
        /// path="/member/exception[not(@cref='T:System.InvalidOperationException') and not(@cref='T:Melanchall.DryWetMidi.Core.TooManyTrackChunksException')]"/>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was canceled.</exception>
        public static async Task<MidiTokensWriter> WriteLazyAsync(
            string filePath,
            bool overwriteFile = false,
            MidiFileFormat format = MidiFileFormat.MultiTrack,
            WritingSettings? settings = null,
            TimeDivision? timeDivision = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(filePath), filePath, "File path is null or white space.");
            ThrowIfArgument.IsInvalidEnumValue(nameof(format), format);
            cancellationToken.ThrowIfCancellationRequested();

            var stream = new FileStream(
                filePath,
                overwriteFile ? FileMode.Create : FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                1,
                FileOptions.Asynchronous);

            try
            {
                timeDivision ??= new TicksPerQuarterNoteTimeDivision();
                var writer = new MidiTokensWriter(stream, settings, true);
                await writer.InitializeAsync(format, timeDivision, cancellationToken).ConfigureAwait(false);

                return writer;
            }
            catch
            {
                await stream.DisposeAsync().ConfigureAwait(false);
                throw;
            }
        }

        private async Task WriteCoreAsync(
            Stream stream,
            MidiFileFormat format,
            WritingSettings? settings,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (TimeDivision == null)
                throw new InvalidOperationException("Time division is null.");

            settings ??= new WritingSettings();
            settings.WriterSettings ??= new WriterSettings();
            var writer = new MidiWriter(stream, settings.WriterSettings);

            await using (writer.ConfigureAwait(false))
            {
                var chunks = ChunksConverterFactory.GetConverter(format).Convert(Chunks);
                if (settings.WriteHeaderChunk)
                {
                    var trackChunksCount = chunks.Count(chunk => chunk is TrackChunk);
                    if (trackChunksCount > ushort.MaxValue)
                        throw new TooManyTrackChunksException(trackChunksCount);

                    var headerChunk = new HeaderChunk
                    {
                        FileFormat = (ushort)format,
                        TimeDivision = TimeDivision,
                        TracksNumber = (ushort)trackChunksCount
                    };
                    await headerChunk.WriteAsync(writer, settings, cancellationToken).ConfigureAwait(false);
                }

                foreach (var chunk in chunks)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (chunk is UnknownChunk && settings.DeleteUnknownChunks)
                        continue;

                    await chunk.WriteAsync(writer, settings, cancellationToken).ConfigureAwait(false);
                }
            }
        }

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
            int? expectedTrackChunksCount = null;
            var actualTrackChunksCount = 0;
            var hasHeaderChunk = false;
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

                var smfEndPosition = await MidiFileReadingUtilities.ReadRmidPreambleAsync(reader, cancellationToken).ConfigureAwait(false);

                while (smfEndPosition == null || reader.Position < smfEndPosition)
                {
                    if (expectedTrackChunksCount != null &&
                        actualTrackChunksCount == expectedTrackChunksCount &&
                        settings.StopReadingOnExpectedTrackChunksCountReached)
                        return file;

                    if (await reader.EndReachedAsync(cancellationToken).ConfigureAwait(false))
                        break;

                    var chunk = await ReadChunkAsync(
                        reader,
                        settings,
                        actualTrackChunksCount,
                        expectedTrackChunksCount,
                        cancellationToken).ConfigureAwait(false);
                    if (chunk == null)
                        continue;

                    if (chunk is HeaderChunk headerChunk)
                    {
                        if (!hasHeaderChunk)
                        {
                            expectedTrackChunksCount = headerChunk.TracksNumber;
                            file.TimeDivision = headerChunk.TimeDivision;
                            file._originalFormat = headerChunk.FileFormat;

                            if (settings.MaxMemorySize != null)
                                reader.AddEstimatedMemory(MemorySizeEstimator.EstimateShallow(headerChunk.TimeDivision), settings);
                        }

                        hasHeaderChunk = true;
                        continue;
                    }

                    if (chunk is TrackChunk)
                        actualTrackChunksCount++;

                    var chunksCapacity = file.Chunks.Capacity;
                    file.Chunks.Add(chunk);

                    if (settings.MaxMemorySize != null)
                    {
                        if (chunksCapacity != file.Chunks.Capacity)
                            reader.AddEstimatedMemory(
                                MemorySizeEstimator.EstimateReferencesArray(file.Chunks.Capacity) -
                                MemorySizeEstimator.EstimateReferencesArray(chunksCapacity),
                                settings);

                        file.EstimatedMemorySize = reader.EstimatedMemorySize;
                    }
                }

                if (expectedTrackChunksCount != null && actualTrackChunksCount != expectedTrackChunksCount)
                    ReactOnUnexpectedTrackChunksCount(
                        settings.UnexpectedTrackChunksCountPolicy,
                        actualTrackChunksCount,
                        expectedTrackChunksCount.Value);

                if (!hasHeaderChunk)
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
            int actualTrackChunksCount,
            int? expectedTrackChunksCount,
            CancellationToken cancellationToken)
        {
            var chunkIdBytes = await reader.ReadBytesAsync(4, cancellationToken).ConfigureAwait(false);
            if (chunkIdBytes.Length < 4)
            {
                if (settings.NotEnoughBytesPolicy == NotEnoughBytesPolicy.Abort)
                    throw new NotEnoughBytesException("Not enough bytes to read a chunk ID.", 4, chunkIdBytes.Length);

                return null;
            }

            var chunkId = SmfConstants.DefaultTextEncoding.GetString(chunkIdBytes);
            MidiChunk? chunk = chunkId == HeaderChunk.Id ? new HeaderChunk() :
                chunkId == TrackChunk.Id ? new TrackChunk() : MidiFileReadingUtilities.TryCreateChunk(chunkId, settings.CustomChunkTypes);
            if (chunk == null && settings.UnknownChunkIdPolicy == UnknownChunkIdPolicy.Abort)
                throw new UnknownChunkException(chunkId);

            if (chunk == null && settings.UnknownChunkIdPolicy == UnknownChunkIdPolicy.ReadAsUnknownChunk)
                chunk = new UnknownChunk(chunkId);

            uint chunkSize;

            try
            {
                chunkSize = await reader.ReadDwordAsync(cancellationToken).ConfigureAwait(false);
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

            var readerPosition = reader.Position;
            if ((chunk == null && settings.UnknownChunkIdPolicy == UnknownChunkIdPolicy.Skip) ||
                (chunk is TrackChunk &&
                 expectedTrackChunksCount != null &&
                 actualTrackChunksCount >= expectedTrackChunksCount &&
                 settings.ExtraTrackChunkPolicy == ExtraTrackChunkPolicy.Skip))
            {
                await reader.SetPositionAsync(Math.Min(readerPosition + chunkSize, reader.Length), cancellationToken).ConfigureAwait(false);

                return null;
            }

            chunk ??= new UnknownChunk(chunkId);

            try
            {
                await chunk.ReadAsync(reader, settings, chunkSize, cancellationToken).ConfigureAwait(false);
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

        #endregion
    }
}
