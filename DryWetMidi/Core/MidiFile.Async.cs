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
    }
}
