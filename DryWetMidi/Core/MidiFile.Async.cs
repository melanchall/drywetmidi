using Melanchall.DryWetMidi.Common;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Core
{
    public sealed partial class MidiFile
    {
        /// <summary>
        /// Reads a MIDI file using asynchronous stream I/O.
        /// </summary>
        /// <param name="stream">Readable stream, left open on completion or failure.</param>
        /// <param name="settings">Reading settings, or <c>null</c> for defaults.</param>
        /// <param name="progress">
        /// Optional throttled progress in bytes read from the transport.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel reading.</param>
        /// <returns>The MIDI file.</returns>
        /// <remarks>
        /// Transport I/O is awaited. Synchronous codecs decode individual buffered events.
        /// Custom chunks can override <see cref="MidiChunk.ReadContentAsync"/> to read content
        /// directly with asynchronous reader methods. The default implementation buffers custom
        /// content for synchronous decoding. Whole-file buffering is used only when requested
        /// by <see cref="ReaderSettings.BufferingPolicy"/>.
        /// </remarks>
        /// <inheritdoc cref="Read(Stream, ReadingSettings)" path="/member/exception"/>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was canceled.</exception>
        public static async Task<MidiFile> ReadAsync(
            Stream stream,
            ReadingSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNull(nameof(stream), stream);

            var reporting = progress == null ? null : new MidiIoProgress(progress, "Reading");
            var file = await ReadCoreAsync(stream, settings ?? new ReadingSettings(), cancellationToken, reporting).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();
            reporting?.Complete();

            return file;
        }

        /// <summary>
        /// Reads a MIDI file from disk using asynchronous I/O.
        /// </summary>
        /// <param name="filePath">Path of the MIDI file.</param>
        /// <param name="settings">Reading settings, or <c>null</c> for defaults.</param>
        /// <param name="progress">
        /// Optional throttled progress in bytes read from the transport.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel reading.</param>
        /// <returns>The MIDI file.</returns>
        /// <inheritdoc cref="Read(string, ReadingSettings)" path="/member/exception"/>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was canceled.</exception>
        public static async Task<MidiFile> ReadAsync(
            string filePath,
            ReadingSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(filePath), filePath, "File path is null or white space.");
            cancellationToken.ThrowIfCancellationRequested();

            var reporting = progress == null ? null : new MidiIoProgress(progress, "Reading");
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
                file = await ReadCoreAsync(stream, settings ?? new ReadingSettings(), cancellationToken, reporting).ConfigureAwait(false);
            }

            cancellationToken.ThrowIfCancellationRequested();
            reporting?.Complete();

            return file;
        }

        /// <summary>
        /// Writes this MIDI file using asynchronous stream I/O.
        /// </summary>
        /// <param name="stream">Writable stream, left open on completion or failure.</param>
        /// <param name="format">Output MIDI file format.</param>
        /// <param name="settings">Writing settings, or <c>null</c> for defaults.</param>
        /// <param name="progress">
        /// Optional throttled progress in bytes written to the transport.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel writing.</param>
        /// <returns>A task representing writing.</returns>
        /// <remarks>
        /// Synchronous codecs encode individual events in memory. Custom chunks can override
        /// <see cref="MidiChunk.WriteContentAsync"/> to write content directly with asynchronous
        /// writer methods. The default implementation buffers synchronously encoded custom
        /// content. Transport writes and flushing are awaited. Cancellation or failure can
        /// leave partially written data in the stream.
        /// </remarks>
        /// <inheritdoc cref="Write(Stream, MidiFileFormat, WritingSettings)" path="/member/exception"/>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was canceled.</exception>
        public async Task WriteAsync(
            Stream stream,
            MidiFileFormat format = MidiFileFormat.MultiTrack,
            WritingSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNull(nameof(stream), stream);
            ThrowIfArgument.IsInvalidEnumValue(nameof(format), format);
            cancellationToken.ThrowIfCancellationRequested();

            if (!stream.CanWrite)
                throw new ArgumentException("Stream doesn't support writing.", nameof(stream));

            var reporting = progress == null ? null : new MidiIoProgress(progress, "Writing");
            await WriteCoreAsync(stream, format, settings, reporting, cancellationToken).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();
            reporting?.Complete();
        }

        private async Task WriteCoreAsync(
            Stream stream,
            MidiFileFormat format,
            WritingSettings? settings,
            MidiIoProgress? reporting,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (TimeDivision == null)
                throw new InvalidOperationException("Time division is null.");

            settings ??= new WritingSettings();
            settings.WriterSettings ??= new WriterSettings();
            var writer = new MidiWriter(stream, settings.WriterSettings) { AsyncProgress = reporting };

            await using (writer.ConfigureAwait(false))
            {
                var chunks = ChunksConverterFactory.GetConverter(format).Convert(Chunks);
                if (settings.WriteHeaderChunk)
                {
                    var count = chunks.Count(c => c is TrackChunk);
                    if (count > ushort.MaxValue)
                        throw new TooManyTrackChunksException(count);

                    await new HeaderChunk
                    {
                        FileFormat = (ushort)format,
                        TimeDivision = TimeDivision,
                        TracksNumber = (ushort)count
                    }.WriteAsync(writer, settings, cancellationToken).ConfigureAwait(false);
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

        /// <summary>
        /// Writes this MIDI file to disk using asynchronous I/O.
        /// </summary>
        /// <param name="filePath">Output file path.</param>
        /// <param name="overwriteFile">Whether an existing file may be overwritten.</param>
        /// <param name="format">Output MIDI file format.</param>
        /// <param name="settings">Writing settings, or <c>null</c> for defaults.</param>
        /// <param name="progress">
        /// Optional throttled progress in bytes written to the transport.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
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
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(filePath), filePath, "File path is null or white space.");
            ThrowIfArgument.IsInvalidEnumValue(nameof(format), format);
            cancellationToken.ThrowIfCancellationRequested();

            var reporting = progress == null ? null : new MidiIoProgress(progress, "Writing");
            var stream = new FileStream(
                filePath,
                overwriteFile ? FileMode.Create : FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                1,
                FileOptions.Asynchronous);

            await using (stream.ConfigureAwait(false))
            {
                await WriteCoreAsync(stream, format, settings, reporting, cancellationToken).ConfigureAwait(false);
            }

            cancellationToken.ThrowIfCancellationRequested();
            reporting?.Complete();
        }

        /// <summary>
        /// Creates a lazy reader without synchronous transport reads, including on nonseekable streams.
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

            var reader = new MidiTokensReader(stream, settings, false, true);
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
                var reader = new MidiTokensReader(stream, settings, true, true);
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
            var writer = new MidiTokensWriter(stream, settings, false, format, timeDivision, true);
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
                var writer = new MidiTokensWriter(stream, settings, true, format, timeDivision, true);
                await writer.InitializeAsync(format, timeDivision, cancellationToken).ConfigureAwait(false);

                return writer;
            }
            catch
            {
                await stream.DisposeAsync().ConfigureAwait(false);
                throw;
            }
        }
    }
}
