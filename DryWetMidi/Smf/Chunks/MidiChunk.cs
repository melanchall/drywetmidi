using Melanchall.DryWetMidi.Common;
using System;
using System.IO;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Represents a chunk of Standard MIDI file.
    /// </summary>
    /// <remarks>
    /// MIDI Files are made up of chunks. Each chunk has a 4-character ASCII string ID and a 32-bit length,
    /// which is the number of bytes in the chunk. This structure allows future chunk types to be designed
    /// which may be easily be ignored if encountered by a program written before the chunk type is introduced.
    /// The length of the chunk refers to the number of bytes of data which follow (the eight bytes of ID and length
    /// are not included).Therefore, a chunk with a length of 6 would actually occupy 14 bytes in the file.
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
        /// <paramref name="id"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="id"/> is empty, or consists only of white-space characters; or
        /// length of <paramref name="id"/> doesn't equal 4.
        /// </exception>
        public MidiChunk(string id)
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
            var size = reader.ReadDword();

            var readerPosition = reader.Position;
            ReadContent(reader, settings, size);

            var bytesRead = reader.Position - readerPosition;
            if (settings.InvalidChunkSizePolicy == InvalidChunkSizePolicy.Abort && bytesRead != size)
                throw new InvalidChunkSizeException(size, bytesRead);

            // Skip unread bytes

            var bytesUnread = size - bytesRead;
            if (bytesUnread > 0)
                reader.Position += bytesUnread;
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
            writer.WriteString(ChunkId);

            var size = GetContentSize(settings);
            writer.WriteDword(size);

            WriteContent(writer, settings);
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
