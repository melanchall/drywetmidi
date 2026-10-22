using Melanchall.DryWetMidi.Common;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Core
{
    public sealed partial class MidiFile
    {
        /// <summary>Reads a MIDI file using asynchronous stream I/O.</summary>
        /// <param name="stream">Readable stream, left open on completion or failure.</param>
        /// <param name="settings">Reading settings, or <c>null</c> for defaults.</param>
        /// <param name="progress">Optional throttled progress in bytes read from the transport.</param>
        /// <param name="cancellationToken">Token used to cancel reading.</param>
        /// <returns>The MIDI file.</returns>
        /// <remarks>Transport I/O is awaited. Synchronous codecs decode individual buffered
        /// events or custom chunks; tracks and files are not buffered as a whole.</remarks>
        public static async Task<MidiFile> ReadAsync(Stream stream, ReadingSettings? settings = null,
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

        /// <summary>Reads a MIDI file from disk using asynchronous I/O.</summary>
        /// <param name="filePath">Path of the MIDI file.</param>
        /// <param name="settings">Reading settings, or <c>null</c> for defaults.</param>
        /// <param name="progress">Optional throttled progress in bytes read from the transport.</param>
        /// <param name="cancellationToken">Token used to cancel reading.</param>
        /// <returns>The MIDI file.</returns>
        public static async Task<MidiFile> ReadAsync(string filePath, ReadingSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(filePath), filePath, "File path is null or white space.");
            cancellationToken.ThrowIfCancellationRequested();
            var reporting = progress == null ? null : new MidiIoProgress(progress, "Reading");
            MidiFile file;
            await using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read,
                1, FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                file = await ReadCoreAsync(stream, settings ?? new ReadingSettings(), cancellationToken, reporting).ConfigureAwait(false);
            }
            cancellationToken.ThrowIfCancellationRequested();
            reporting?.Complete();
            return file;
        }

        /// <summary>Writes this MIDI file using asynchronous stream I/O.</summary>
        /// <param name="stream">Writable stream, left open on completion or failure.</param>
        /// <param name="format">Output MIDI file format.</param>
        /// <param name="settings">Writing settings, or <c>null</c> for defaults.</param>
        /// <param name="progress">Optional throttled progress in bytes written to the transport.</param>
        /// <param name="cancellationToken">Token used to cancel writing.</param>
        /// <returns>A task representing writing.</returns>
        /// <remarks>Synchronous codecs encode individual events or custom chunks in memory;
        /// transport writes and flushing are awaited.</remarks>
        public async Task WriteAsync(Stream stream, MidiFileFormat format = MidiFileFormat.MultiTrack,
            WritingSettings? settings = null, IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            var reporting = progress == null ? null : new MidiIoProgress(progress, "Writing");
            await WriteCoreAsync(stream, format, settings, reporting, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            reporting?.Complete();
        }

        private async Task WriteCoreAsync(Stream stream, MidiFileFormat format, WritingSettings? settings,
            MidiIoProgress? reporting, CancellationToken cancellationToken)
        {
            ThrowIfArgument.IsNull(nameof(stream), stream);
            ThrowIfArgument.IsInvalidEnumValue(nameof(format), format);
            cancellationToken.ThrowIfCancellationRequested();
            if (TimeDivision == null)
                throw new InvalidOperationException("Time division is null.");
            if (!stream.CanWrite)
                throw new ArgumentException("Stream doesn't support writing.", nameof(stream));
            settings ??= new WritingSettings();
            settings.WriterSettings ??= new WriterSettings();
            await using (var writer = new MidiWriter(stream, settings.WriterSettings) { AsyncProgress = reporting })
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

        /// <summary>Writes this MIDI file to disk using asynchronous I/O.</summary>
        /// <param name="filePath">Output file path.</param>
        /// <param name="overwriteFile">Whether an existing file may be overwritten.</param>
        /// <param name="format">Output MIDI file format.</param>
        /// <param name="settings">Writing settings, or <c>null</c> for defaults.</param>
        /// <param name="progress">Optional throttled progress in bytes written to the transport.</param>
        /// <param name="cancellationToken">Token used to cancel writing.</param>
        /// <returns>A task representing writing.</returns>
        public async Task WriteAsync(string filePath, bool overwriteFile = false,
            MidiFileFormat format = MidiFileFormat.MultiTrack, WritingSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(filePath), filePath, "File path is null or white space.");
            ThrowIfArgument.IsInvalidEnumValue(nameof(format), format);
            cancellationToken.ThrowIfCancellationRequested();
            var reporting = progress == null ? null : new MidiIoProgress(progress, "Writing");
            await using (var stream = new FileStream(filePath, overwriteFile ? FileMode.Create : FileMode.CreateNew,
                FileAccess.Write, FileShare.None, 1, FileOptions.Asynchronous))
            {
                await WriteCoreAsync(stream, format, settings, reporting, cancellationToken).ConfigureAwait(false);
            }
            cancellationToken.ThrowIfCancellationRequested();
            reporting?.Complete();
        }

        /// <summary>Creates a lazy reader without synchronous transport reads, including on nonseekable streams.</summary>
        /// <param name="stream">Readable stream, left open when the reader is disposed.</param>
        /// <param name="settings">Reading settings, or <c>null</c> for defaults.</param>
        /// <param name="cancellationToken">Token used to cancel initialization.</param>
        /// <returns>A lazy MIDI token reader.</returns>
        public static async Task<MidiTokensReader> ReadLazyAsync(Stream stream, ReadingSettings? settings = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNull(nameof(stream), stream);
            cancellationToken.ThrowIfCancellationRequested();
            var reader = new MidiTokensReader(stream, settings, false, true);
            await reader.InitializeAsync(cancellationToken).ConfigureAwait(false);
            return reader;
        }

        /// <summary>Opens a MIDI file for lazy asynchronous reading.</summary>
        /// <param name="filePath">Path of the MIDI file.</param>
        /// <param name="settings">Reading settings, or <c>null</c> for defaults.</param>
        /// <param name="cancellationToken">Token used to cancel initialization.</param>
        /// <returns>A token reader that owns the opened stream.</returns>
        public static async Task<MidiTokensReader> ReadLazyAsync(string filePath, ReadingSettings? settings = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(filePath), filePath, "File path is null or white space.");
            cancellationToken.ThrowIfCancellationRequested();
            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read,
                1, FileOptions.Asynchronous | FileOptions.SequentialScan);
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

        /// <summary>Creates a lazy writer and asynchronously writes its file header.</summary>
        /// <param name="stream">Seekable writable stream, left open when the writer is disposed.</param>
        /// <param name="settings">Writing settings, or <c>null</c> for defaults.</param>
        /// <param name="format">Output MIDI file format.</param>
        /// <param name="timeDivision">Time division, or <c>null</c> for the default.</param>
        /// <param name="cancellationToken">Token used to cancel initialization.</param>
        /// <returns>A lazy MIDI token writer.</returns>
        public static async Task<MidiTokensWriter> WriteLazyAsync(Stream stream, WritingSettings? settings = null,
            MidiFileFormat format = MidiFileFormat.MultiTrack, TimeDivision? timeDivision = null,
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

        /// <summary>Opens a file for lazy asynchronous writing and writes its header.</summary>
        /// <param name="filePath">Output file path.</param>
        /// <param name="overwriteFile">Whether an existing file may be overwritten.</param>
        /// <param name="settings">Writing settings, or <c>null</c> for defaults.</param>
        /// <param name="format">Output MIDI file format.</param>
        /// <param name="timeDivision">Time division, or <c>null</c> for the default.</param>
        /// <param name="cancellationToken">Token used to cancel initialization.</param>
        /// <returns>A token writer that owns the opened stream.</returns>
        public static async Task<MidiTokensWriter> WriteLazyAsync(string filePath, bool overwriteFile = false,
            MidiFileFormat format = MidiFileFormat.MultiTrack, WritingSettings? settings = null,
            TimeDivision? timeDivision = null, CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(filePath), filePath, "File path is null or white space.");
            ThrowIfArgument.IsInvalidEnumValue(nameof(format), format);
            cancellationToken.ThrowIfCancellationRequested();
            var stream = new FileStream(filePath, overwriteFile ? FileMode.Create : FileMode.CreateNew,
                FileAccess.Write, FileShare.None, 1, FileOptions.Asynchronous);
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
