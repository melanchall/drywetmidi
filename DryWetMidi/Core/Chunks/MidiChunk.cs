using Melanchall.DryWetMidi.Common;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents a chunk of Standard MIDI file.
    /// </summary>
    /// <remarks>
    /// <para>
    /// MIDI files are made up of chunks. Each chunk has a 4-character type ID and a 32-bit length,
    /// which is the number of bytes in the chunk. You manage chunks via <see cref="MidiFile.Chunks"/> property
    /// of the <see cref="MidiFile"/>.
    /// </para>
    /// <para>
    /// There are two standard types of MIDI chunks: header chunk and track chunk. The first one will
    /// not be presented in the <see cref="MidiFile.Chunks"/>. Its data is used by the reading engine to set properties
    /// of the <see cref="MidiFile"/> such as <see cref="MidiFile.TimeDivision"/> and <see cref="MidiFile.OriginalFormat"/>. You cannot add header
    /// chunks in the chunks collection of the file since an appropriate one will be written by writing engine automatically
    /// on <see cref="MidiFile.Write(string, bool, MidiFileFormat, WritingSettings)"/> or
    /// <see cref="MidiFile.Write(Stream, MidiFileFormat, WritingSettings)"/>.
    /// </para>
    /// <para>
    /// The structure of a MIDI chunk allows any custom chunks to be placed in a MIDI file along with the standard
    /// ones described above. You can implement custom chunks that can be read from and written to a MIDI
    /// file. See the <see href="xref:a_custom_chunk">Custom chunks</see> article to learn more. If you don't specify information
    /// about your custom chunk types, the reading engine will read them as instances of the <see cref="UnknownChunk"/> class where
    /// <see cref="UnknownChunk.Data"/> property will hold chunk's data and <see cref="ChunkId"/> will hold the ID of a chunk.
    /// </para>
    /// <para>
    /// See <see href="https://midi.org/standard-midi-files-specification"/> for detailed MIDI file specification.
    /// </para>
    /// </remarks>
    public abstract class MidiChunk
    {
        #region Constants

        /// <summary>
        /// The length of a chunk's ID. This field is constant.
        /// </summary>
        public const int IdLength = 4;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MidiChunk"/> with the specified ID.
        /// </summary>
        /// <param name="id">The type of the chunk.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="id"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="id"/> is empty, or consists only of white-space characters; or
        /// length of <paramref name="id"/> doesn't equal 4.
        /// </exception>
        protected MidiChunk(string id)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(id), id, "Chunk ID");

            if (id.Length != IdLength)
                throw new ArgumentException($"ID length doesn't equal {IdLength}.", nameof(id));

            ChunkId = id;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets 4-character ID of the chunk which specifies its type.
        /// </summary>
        public string ChunkId { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Clones chunk by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the chunk.</returns>
        public abstract MidiChunk Clone();

        /// <summary>
        /// Determines whether two specified <see cref="MidiChunk"/> objects have the same content.
        /// </summary>
        /// <param name="chunk1">The first chunk to compare, or <c>null</c>.</param>
        /// <param name="chunk2">The second chunk to compare, or <c>null</c>.</param>
        /// <returns><c>true</c> if the <paramref name="chunk1"/> is equal to the <paramref name="chunk2"/>;
        /// otherwise, <c>false</c>.</returns>
        public static bool Equals(MidiChunk? chunk1, MidiChunk? chunk2)
        {
            return Equals(chunk1, chunk2, out _);
        }

        /// <summary>
        /// Determines whether two specified <see cref="MidiChunk"/> objects have the same content.
        /// </summary>
        /// <param name="chunk1">The first chunk to compare, or <c>null</c>.</param>
        /// <param name="chunk2">The second chunk to compare, or <c>null</c>.</param>
        /// <param name="message">Message containing information about what exactly is different in
        /// <paramref name="chunk1"/> and <paramref name="chunk2"/>.</param>
        /// <returns><c>true</c> if the <paramref name="chunk1"/> is equal to the <paramref name="chunk2"/>;
        /// otherwise, <c>false</c>.</returns>
        public static bool Equals(MidiChunk? chunk1, MidiChunk? chunk2, out string? message)
        {
            return Equals(chunk1, chunk2, null, out message);
        }

        /// <summary>
        /// Determines whether two specified <see cref="MidiChunk"/> objects have the same content.
        /// </summary>
        /// <param name="chunk1">The first chunk to compare, or <c>null</c>.</param>
        /// <param name="chunk2">The second chunk to compare, or <c>null</c>.</param>
        /// <param name="settings">Settings according to which chunks should be compared.</param>
        /// <returns><c>true</c> if the <paramref name="chunk1"/> is equal to the <paramref name="chunk2"/>;
        /// otherwise, <c>false</c>.</returns>
        public static bool Equals(MidiChunk? chunk1, MidiChunk? chunk2, MidiChunkEqualityCheckSettings? settings)
        {
            return Equals(chunk1, chunk2, settings, out _);
        }

        /// <summary>
        /// Determines whether two specified <see cref="MidiChunk"/> objects have the same content using
        /// the specified comparison settings.
        /// </summary>
        /// <param name="chunk1">The first chunk to compare, or <c>null</c>.</param>
        /// <param name="chunk2">The second chunk to compare, or <c>null</c>.</param>
        /// <param name="settings">Settings according to which chunks should be compared.</param>
        /// <param name="message">Message containing information about what exactly is different in
        /// <paramref name="chunk1"/> and <paramref name="chunk2"/>.</param>
        /// <returns><c>true</c> if the <paramref name="chunk1"/> is equal to the <paramref name="chunk2"/>;
        /// otherwise, <c>false</c>.</returns>
        public static bool Equals(MidiChunk? chunk1, MidiChunk? chunk2, MidiChunkEqualityCheckSettings? settings, out string? message)
        {
            return MidiChunkEquality.Equals(chunk1, chunk2, settings ?? new MidiChunkEqualityCheckSettings(), out message);
        }

        /// <summary>
        /// Reads chunk from the <see cref="MidiReader"/>'s underlying stream according to
        /// specified <see cref="ReadingSettings"/>.
        /// </summary>
        /// <param name="reader">Reader to read the chunk's data with.</param>
        /// <param name="settings">Settings according to which the chunk's data must be read.</param>
        /// <exception cref="System.ObjectDisposedException">Method was called after <paramref name="reader"/>
        /// was disposed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred on the <paramref name="reader"/>'s
        /// underlying stream.</exception>
        /// <exception cref="InvalidChunkSizeException">Actual chunk's size differs from the one declared
        /// in its header.</exception>
        /// <exception cref="NotEnoughBytesException">Size of the chunk cannot be read since the reader's
        /// underlying stream doesn't have enough bytes.</exception>
        internal void Read(MidiReader reader, ReadingSettings settings)
        {
            var size = ReadSize(reader, out var readerPosition);

            var estimateMemory = settings.MaxMemorySize != null;
            var isCustomChunk = !(this is HeaderChunk || this is TrackChunk || this is UnknownChunk);

            // Custom chunks can allocate memory in an arbitrary way, so at least the declared size is expected
            if (estimateMemory && isCustomChunk)
                reader.ThrowIfMemoryLimitExceeded(Math.Min(size, reader.Length - reader.Position), settings);

            // Chunk object with its initial state (e.g. empty events collection). Header chunk is not stored in the file
            var initialSize = 0L;
            if (estimateMemory && !(this is HeaderChunk))
            {
                initialSize = MemorySizeEstimator.EstimateDeep(this);
                reader.AddEstimatedMemory(initialSize, settings);
            }

            ReadContent(reader, settings, size);

            // Everything a custom chunk has allocated while reading
            if (estimateMemory && isCustomChunk)
                reader.AddEstimatedMemory(Math.Max(0, MemorySizeEstimator.EstimateDeep(this) - initialSize), settings);

            var bytesReadCount = reader.Position - readerPosition;
            if (settings.InvalidChunkSizePolicy == InvalidChunkSizePolicy.Abort && bytesReadCount != size)
                throw new InvalidChunkSizeException(ChunkId, size, bytesReadCount);

            // Skip unread bytes

            var bytesUnread = size - bytesReadCount;
            if (bytesUnread > 0)
                reader.Position += Math.Min(bytesUnread, reader.Length);
        }

        /// <summary>
        /// Writes chunk to the <see cref="MidiWriter"/>'s underlying stream according to
        /// specified <see cref="WritingSettings"/>.
        /// </summary>
        /// <param name="writer">Writer to write the chunk's data with.</param>
        /// <param name="settings">Settings according to which the chunk's data must be written.</param>
        /// <exception cref="ObjectDisposedException">
        /// Method was called after <paramref name="writer"/> was disposed.
        /// </exception>
        /// <exception cref="IOException">
        /// An I/O error occurred on the <paramref name="writer"/>'s underlying stream.
        /// </exception>
        internal void Write(MidiWriter writer, WritingSettings settings)
        {
            var size = GetContentSize(settings);
            WriteHeader(ChunkId, size, writer, settings);
            WriteContent(writer, settings);
        }

        internal async Task WriteAsync(MidiWriter writer, WritingSettings settings, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (this is TrackChunk track)
            {
                await writer.WriteEncodedAsync(
                    codec => WriteHeader(ChunkId, track.GetContentSize(settings, cancellationToken), codec, settings),
                    cancellationToken).ConfigureAwait(false);
                await WriteContentAsync(writer, settings, cancellationToken).ConfigureAwait(false);
            }
            else if (this is UnknownChunk unknown)
            {
                await writer.WriteEncodedAsync(
                    codec => WriteHeader(ChunkId, GetContentSize(settings), codec, settings),
                    cancellationToken).ConfigureAwait(false);

                if (unknown.Data != null)
                    await writer.WriteDataAsync(unknown.Data, cancellationToken).ConfigureAwait(false);
            }
            else if (this is HeaderChunk)
                await writer.WriteEncodedAsync(codec => Write(codec, settings), cancellationToken).ConfigureAwait(false);
            else
            {
                await writer.WriteEncodedAsync(
                    codec => WriteHeader(ChunkId, GetContentSize(settings), codec, settings),
                    cancellationToken).ConfigureAwait(false);
                await WriteContentAsync(writer, settings, cancellationToken).ConfigureAwait(false);
            }
        }

        internal async Task ReadAsync(
            MidiReader reader,
            ReadingSettings settings,
            uint size,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var start = reader.Position;
            var initialSize = 0L;
            if (settings.MaxMemorySize != null)
            {
                reader.ThrowIfMemoryLimitExceeded(Math.Min(size, Math.Max(0, reader.Length - start)), settings);
                initialSize = MemorySizeEstimator.EstimateDeep(this);
                reader.AddEstimatedMemory(initialSize, settings);
            }

            await ReadContentAsync(reader, settings, size, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            if (settings.MaxMemorySize != null)
                reader.AddEstimatedMemory(Math.Max(0, MemorySizeEstimator.EstimateDeep(this) - initialSize), settings);

            var consumed = reader.Position - start;
            if (settings.InvalidChunkSizePolicy == InvalidChunkSizePolicy.Abort && consumed != size)
                throw new InvalidChunkSizeException(ChunkId, size, consumed);

            if (consumed < size)
                await reader.SetPositionAsync(Math.Min(start + size, reader.Length), cancellationToken).ConfigureAwait(false);
        }

        internal static uint ReadSize(MidiReader reader, out long readerPosition)
        {
            var size = reader.ReadDword();
            readerPosition = reader.Position;
            return size;
        }

        internal static void WriteHeader(string chunkId, uint size, MidiWriter writer, WritingSettings settings)
        {
            writer.WriteString(chunkId);
            writer.WriteDword(size);
        }

        /// <summary>
        /// Reads content of a chunk. Content is a part of chunk's data without its header (ID and size).
        /// </summary>
        /// <param name="reader">Reader to read the chunk's content with.</param>
        /// <param name="settings">Settings according to which the chunk's content must be read.</param>
        /// <param name="size">Expected size of the content taken from the chunk's header.</param>
        /// <exception cref="ObjectDisposedException">Method was called after <paramref name="reader"/>
        /// was disposed.</exception>
        /// <exception cref="IOException">An I/O error occurred on the <paramref name="reader"/>'s
        /// underlying stream.</exception>
        protected abstract void ReadContent(MidiReader reader, ReadingSettings settings, uint size);

        /// <summary>
        /// Asynchronously reads a chunk's content, excluding its ID and size.
        /// </summary>
        /// <param name="reader">Reader positioned at the content. The reader remains owned by the caller.</param>
        /// <param name="settings">Settings according to which the content must be read.</param>
        /// <param name="size">Expected content size from the chunk header, not a read boundary.</param>
        /// <param name="cancellationToken">Token used to cancel reading.</param>
        /// <returns>A task representing reading the content.</returns>
        /// <remarks>
        /// Override this method and use the reader's asynchronous methods to read directly from the
        /// input stream. The default implementation buffers the declared content and invokes
        /// <see cref="ReadContent"/> for compatibility with synchronous subclasses. Neither implementation
        /// should dispose the reader. As with synchronous reading, overrides are responsible for the
        /// number of bytes read; the chunk size policy is applied after reading.
        /// </remarks>
        /// <exception cref="OperationCanceledException">Reading was canceled.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="reader"/> or
        /// <paramref name="settings"/> is null.</exception>
        /// <exception cref="ObjectDisposedException">The underlying stream has been disposed.</exception>
        /// <exception cref="IOException">An I/O error occurred while reading.</exception>
        /// <exception cref="InvalidChunkSizeException">The default implementation read an unexpected
        /// content size and the size policy is <see cref="InvalidChunkSizePolicy.Abort"/>.</exception>
        protected virtual async Task ReadContentAsync(
            MidiReader reader,
            ReadingSettings settings,
            uint size,
            CancellationToken cancellationToken)
        {
            ThrowIfArgument.IsNull(nameof(reader), reader);
            ThrowIfArgument.IsNull(nameof(settings), settings);
            cancellationToken.ThrowIfCancellationRequested();
            var start = reader.Position;
            using var memory = new MemoryStream();
            memory.Write(new byte[4], 0, 4);
            await reader.CopyBytesAsync(memory, size, cancellationToken).ConfigureAwait(false);
            using var codec = new MidiReader(
                memory.GetBuffer(),
                0,
                checked((int)memory.Length),
                settings.ReaderSettings,
                start - 4);
            codec.ReadDword();
            codec.AddEstimatedMemory(reader.EstimatedMemorySize, settings);
            ReadContent(codec, settings, size);
            cancellationToken.ThrowIfCancellationRequested();

            var consumed = codec.Position - start;
            if (settings.InvalidChunkSizePolicy == InvalidChunkSizePolicy.Abort && consumed != size)
                throw new InvalidChunkSizeException(ChunkId, size, consumed);
        }

        /// <summary>
        /// Writes content of a chunk. Content is a part of chunk's data without its header (ID and size).
        /// </summary>
        /// <param name="writer">Writer to write the chunk's content with.</param>
        /// <param name="settings">Settings according to which the chunk's content must be written.</param>
        /// <exception cref="System.ObjectDisposedException">Method was called after <paramref name="writer"/>
        /// was disposed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred on the <paramref name="writer"/>'s
        /// underlying stream.</exception>
        protected abstract void WriteContent(MidiWriter writer, WritingSettings settings);

        /// <summary>
        /// Asynchronously writes a chunk's content, excluding its ID and size.
        /// </summary>
        /// <param name="writer">Writer to use. The writer remains owned by the caller.</param>
        /// <param name="settings">Settings according to which the content must be written.</param>
        /// <param name="cancellationToken">Token used to cancel writing.</param>
        /// <returns>A task representing writing the content.</returns>
        /// <remarks>
        /// Override this method and use the writer's asynchronous methods to write directly to the
        /// output stream. The default implementation buffers <see cref="WriteContent"/>'s output
        /// for compatibility with synchronous subclasses. Do not dispose the writer.
        /// </remarks>
        /// <exception cref="OperationCanceledException">Writing was canceled.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> or
        /// <paramref name="settings"/> is null.</exception>
        /// <exception cref="ObjectDisposedException">The underlying stream has been disposed.</exception>
        /// <exception cref="IOException">An I/O error occurred while writing.</exception>
        protected virtual Task WriteContentAsync(
            MidiWriter writer,
            WritingSettings settings,
            CancellationToken cancellationToken)
        {
            ThrowIfArgument.IsNull(nameof(writer), writer);
            ThrowIfArgument.IsNull(nameof(settings), settings);

            return writer.WriteEncodedAsync(codec => WriteContent(codec, settings), cancellationToken);
        }

        /// <summary>
        /// Gets size of chunk's content as number of bytes required to write it according to specified
        /// <see cref="WritingSettings"/>.
        /// </summary>
        /// <param name="settings">Settings according to which the chunk's content will be written.</param>
        /// <returns>Number of bytes required to write chunk's content.</returns>
        protected abstract uint GetContentSize(WritingSettings settings);

        #endregion
    }
}
