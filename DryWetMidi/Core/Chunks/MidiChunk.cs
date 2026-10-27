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
            var size = this is TrackChunk track
                ? track.GetContentSize(settings, cancellationToken)
                : GetContentSize(settings);

            await writer.WriteEncodedAsync(
                codec => WriteHeader(ChunkId, size, codec, settings),
                cancellationToken).ConfigureAwait(false);
            await WriteContentAsync(writer, settings, cancellationToken).ConfigureAwait(false);
        }

        internal async Task ReadAsync(
            MidiReader reader,
            ReadingSettings settings,
            uint size,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var start = reader.Position;
            var estimateMemory = settings.MaxMemorySize != null;
            var isCustomChunk = !(this is HeaderChunk || this is TrackChunk || this is UnknownChunk);

            if (estimateMemory && isCustomChunk)
                reader.ThrowIfMemoryLimitExceeded(Math.Min(size, Math.Max(0, reader.Length - start)), settings);

            var initialSize = 0L;
            if (estimateMemory && !(this is HeaderChunk))
            {
                initialSize = MemorySizeEstimator.EstimateDeep(this);
                reader.AddEstimatedMemory(initialSize, settings);
            }

            await ReadContentAsync(reader, settings, size, cancellationToken).ConfigureAwait(false);

            if (estimateMemory && isCustomChunk)
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
        /// Asynchronously reads the chunk's content, excluding its ID and size.
        /// </summary>
        /// <param name="reader">Reader to use. The reader and its underlying stream are left open.</param>
        /// <param name="settings">Settings according to which the content must be read.</param>
        /// <param name="size">Expected content size from the chunk's header.</param>
        /// <param name="cancellationToken">Token used to cancel reading.</param>
        /// <returns>A task representing reading the content.</returns>
        /// <remarks>
        /// Override this method to read directly using asynchronous reader methods. The default
        /// implementation buffers the content asynchronously and calls <see cref="ReadContent"/>
        /// with a synchronous in-memory reader for compatibility with existing custom chunks.
        /// The MIDI file reading APIs initialize the reader's buffering before invoking this method;
        /// overrides do not need to initialize the reader or construct a replacement.
        /// </remarks>
        /// <exception cref="OperationCanceledException">Reading was canceled.</exception>
        /// <exception cref="ObjectDisposedException">The reader or its underlying stream was disposed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        /// <exception cref="InvalidChunkSizeException">The content size is invalid and the settings require aborting.</exception>
        protected virtual async Task ReadContentAsync(
            MidiReader reader,
            ReadingSettings settings,
            uint size,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var start = reader.Position;
            using var memory = new MemoryStream();
            await reader.CopyBytesAsync(memory, size, cancellationToken).ConfigureAwait(false);
            var buffer = memory.Length == 0 ? new byte[1] : memory.GetBuffer();
            using var codec = new MidiReader(buffer, 0, checked((int)memory.Length), settings.ReaderSettings, start);
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
        /// Asynchronously writes the chunk's content, excluding its ID and size.
        /// </summary>
        /// <param name="writer">Writer to use. The writer and its underlying stream are left open.</param>
        /// <param name="settings">Settings according to which the content must be written.</param>
        /// <param name="cancellationToken">Token used to cancel writing.</param>
        /// <returns>A task representing writing the content.</returns>
        /// <remarks>
        /// Override this method to write directly using asynchronous writer methods. The default
        /// implementation calls <see cref="WriteContent"/> with a synchronous in-memory writer
        /// and then asynchronously writes the buffered content.
        /// </remarks>
        /// <exception cref="OperationCanceledException">Writing was canceled.</exception>
        /// <exception cref="ObjectDisposedException">The writer or its underlying stream was disposed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        protected virtual Task WriteContentAsync(
            MidiWriter writer,
            WritingSettings settings,
            CancellationToken cancellationToken = default)
        {
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
