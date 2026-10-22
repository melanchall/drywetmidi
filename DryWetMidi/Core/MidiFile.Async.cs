using Melanchall.DryWetMidi.Common;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Core
{
    public sealed partial class MidiFile
    {
        /// <summary>Reads a MIDI file using asynchronous stream I/O.</summary>
        /// <param name="stream">Readable stream, left open on completion or failure.</param>
        /// <param name="settings">Reading settings, or <c>null</c> for defaults.</param>
        /// <param name="cancellationToken">Token used to cancel reading.</param>
        /// <returns>The MIDI file.</returns>
        /// <remarks>Synchronous custom codecs run on a worker. Transport I/O is asynchronous,
        /// with bounded buffers; the input is not copied into memory before parsing.</remarks>
        public static Task<MidiFile> ReadAsync(Stream stream, ReadingSettings? settings = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNull(nameof(stream), stream);
            var transport = new AsyncIoStream(stream);
            return transport.RunAsync(() => Read(transport, settings), cancellationToken);
        }

        /// <summary>Reads a MIDI file from disk using asynchronous I/O.</summary>
        /// <param name="filePath">Path of the MIDI file.</param>
        /// <param name="settings">Reading settings, or <c>null</c> for defaults.</param>
        /// <param name="cancellationToken">Token used to cancel reading.</param>
        /// <returns>The MIDI file.</returns>
        public static async Task<MidiFile> ReadAsync(string filePath, ReadingSettings? settings = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(filePath), filePath, "File path is null or white space.");
            cancellationToken.ThrowIfCancellationRequested();
            await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read,
                1, FileOptions.Asynchronous | FileOptions.SequentialScan);
            return await ReadAsync(stream, settings, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Writes this MIDI file using asynchronous stream I/O.</summary>
        /// <param name="stream">Writable stream, left open on completion or failure.</param>
        /// <param name="format">Output MIDI file format.</param>
        /// <param name="settings">Writing settings, or <c>null</c> for defaults.</param>
        /// <param name="cancellationToken">Token used to cancel writing.</param>
        /// <returns>A task representing writing.</returns>
        /// <remarks>Synchronous codecs run on a worker; transport writes and flushing are asynchronous.</remarks>
        public Task WriteAsync(Stream stream, MidiFileFormat format = MidiFileFormat.MultiTrack,
            WritingSettings? settings = null, CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNull(nameof(stream), stream);
            var transport = new AsyncIoStream(stream);
            return transport.RunAsync(() => Write(transport, format, settings), cancellationToken);
        }

        /// <summary>Writes this MIDI file to disk using asynchronous I/O.</summary>
        /// <param name="filePath">Output file path.</param>
        /// <param name="overwriteFile">Whether an existing file may be overwritten.</param>
        /// <param name="format">Output MIDI file format.</param>
        /// <param name="settings">Writing settings, or <c>null</c> for defaults.</param>
        /// <param name="cancellationToken">Token used to cancel writing.</param>
        /// <returns>A task representing writing.</returns>
        public async Task WriteAsync(string filePath, bool overwriteFile = false,
            MidiFileFormat format = MidiFileFormat.MultiTrack, WritingSettings? settings = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(filePath), filePath, "File path is null or white space.");
            ThrowIfArgument.IsInvalidEnumValue(nameof(format), format);
            cancellationToken.ThrowIfCancellationRequested();
            await using var stream = new FileStream(filePath, overwriteFile ? FileMode.Create : FileMode.CreateNew,
                FileAccess.Write, FileShare.None, 1, FileOptions.Asynchronous);
            await WriteAsync(stream, format, settings, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Creates a lazy reader without synchronous transport reads, including on nonseekable streams.</summary>
        /// <param name="stream">Readable stream, left open when the reader is disposed.</param>
        /// <param name="settings">Reading settings, or <c>null</c> for defaults.</param>
        /// <param name="cancellationToken">Token used to cancel initialization.</param>
        /// <returns>A lazy MIDI token reader.</returns>
        public static Task<MidiTokensReader> ReadLazyAsync(Stream stream, ReadingSettings? settings = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNull(nameof(stream), stream);
            var transport = new AsyncIoStream(stream);
            return transport.RunAsync(() => new MidiTokensReader(transport, settings, false), cancellationToken);
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
                var transport = new AsyncIoStream(stream);
                return await transport.RunAsync(() => new MidiTokensReader(transport, settings, true),
                    cancellationToken).ConfigureAwait(false);
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
        public static Task<MidiTokensWriter> WriteLazyAsync(Stream stream, WritingSettings? settings = null,
            MidiFileFormat format = MidiFileFormat.MultiTrack, TimeDivision? timeDivision = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNull(nameof(stream), stream);
            ThrowIfArgument.IsInvalidEnumValue(nameof(format), format);
            var transport = new AsyncIoStream(stream);
            return transport.RunAsync(() => WriteLazy(transport, false, settings, format, timeDivision), cancellationToken);
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
                var transport = new AsyncIoStream(stream);
                return await transport.RunAsync(() => WriteLazy(transport, true, settings, format, timeDivision),
                    cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                await stream.DisposeAsync().ConfigureAwait(false);
                throw;
            }
        }
    }
}
