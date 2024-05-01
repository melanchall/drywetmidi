using Melanchall.DryWetMidi.Common;
using System;
using System.IO;

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
    /// The structure of a MIDI chunk allows any custom chunks be placed in a MIDI file along with the standard
    /// ones described above. You can implement custom chunks that can be read from and written to a MIDI
    /// file. See <see href="xref:a_custom_chunk">Custom chunks</see> article to learn more. If you doesn't specify information
    /// about your custom chunk types the reading engine will read them as instances of the <see cref="UnknownChunk"/> class where
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
            ThrowIfArgument.IsNull(nameof(id), id);

            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("ID is empty string.", nameof(id));

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
        /// Returns array of IDs of standard chunks.
        /// </summary>
        /// <remarks>
        /// Standard chunks are header chunk (ID is MThd) and track chunk (ID is MTrk).
        /// </remarks>
        /// <returns>Array of IDs of standard chunks.</returns>
        public static string[] GetStandardChunkIds()
        {
            return StandardChunkIds.GetIds();
        }

        /// <summary>
        /// Determines whether two specified <see cref="MidiChunk"/> objects have the same content.
        /// </summary>
        /// <param name="chunk1">The first chunk to compare, or <c>null</c>.</param>
        /// <param name="chunk2">The second chunk to compare, or <c>null</c>.</param>
        /// <returns><c>true</c> if the <paramref name="chunk1"/> is equal to the <paramref name="chunk2"/>;
        /// otherwise, <c>false</c>.</returns>
        public static bool Equals(MidiChunk chunk1, MidiChunk chunk2)
        {
            string message;
            return Equals(chunk1, chunk2, out message);
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
        public static bool Equals(MidiChunk chunk1, MidiChunk chunk2, out string message)
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
        public static bool Equals(MidiChunk chunk1, MidiChunk chunk2, MidiChunkEqualityCheckSettings settings)
        {
            string message;
            return Equals(chunk1, chunk2, settings, out message);
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
        public static bool Equals(MidiChunk chunk1, MidiChunk chunk2, MidiChunkEqualityCheckSettings settings, out string message)
        {
            return MidiChunkEquality.Equals(chunk1, chunk2, settings ?? new MidiChunkEqualityCheckSettings(), out message);
        }

        /// <summary>
        /// Reads chunk from the <see cref="MidiReader"/>'s underlying stream according to
        /// specified <see cref="ReadingSettings"/>.
        /// </summary>
        /// <param name="reader">Reader to read the chunk's data with.</param>
        /// <param name="settings">Settings according to which the chunk's data must be read.</param>
        /// <exception cref="ObjectDisposedException">Method was called after <paramref name="reader"/>
        /// was disposed.</exception>
        /// <exception cref="IOException">An I/O error occurred on the <paramref name="reader"/>'s
        /// underlying stream.</exception>
        /// <exception cref="InvalidChunkSizeException">Actual chunk's size differs from the one declared
        /// in its header.</exception>
        /// <exception cref="NotEnoughBytesException">Size of the chunk cannot be read since the reader's
        /// underlying stream doesn't have enough bytes.</exception>
        internal void Read(MidiReader reader, ReadingSettings settings)
        {
            long readerPosition;
            var size = ReadSize(reader, out readerPosition);

            ReadContent(reader, settings, size);

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
        protected abstract void ReadContent(MidiReader reader, ReadingSettings settings, uint size);

        /// <summary>
        /// Writes content of a chunk. Content is a part of chunk's data without its header (ID and size).
        /// </summary>
        /// <param name="writer">Writer to write the chunk's content with.</param>
        /// <param name="settings">Settings according to which the chunk's content must be written.</param>
        protected abstract void WriteContent(MidiWriter writer, WritingSettings settings);

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
