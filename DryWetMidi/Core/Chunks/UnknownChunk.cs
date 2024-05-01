using System;
using System.IO;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents an unknown chunk.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Structure of MIDI file chunks allows custom chunks be implemented and written to a MIDI file.
    /// Chunks DryWetMIDI doesn't know about will be read as an instances of the <see cref="UnknownChunk"/>.
    /// </para>
    /// <para>
    /// See <see href="https://midi.org/standard-midi-files-specification"/> for detailed MIDI file specification.
    /// </para>
    /// </remarks>
    public sealed class UnknownChunk : MidiChunk
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownChunk"/> with the specified ID.
        /// </summary>
        /// <param name="id">Chunk's ID.</param>
        internal UnknownChunk(string id)
            : base(id)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets data contained in the current <see cref="UnknownChunk"/>.
        /// </summary>
        public byte[] Data { get; internal set; }

        #endregion

        #region Overrides

        /// <summary>
        /// Clones chunk by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the chunk.</returns>
        public override MidiChunk Clone()
        {
            return new UnknownChunk(ChunkId)
            {
                Data = Data?.Clone() as byte[]
            };
        }

        /// <summary>
        /// Reads content of a <see cref="UnknownChunk"/>.
        /// </summary>
        /// <remarks>
        /// Content of an <see cref="UnknownChunk"/> is array of bytes.
        /// </remarks>
        /// <param name="reader">Reader to read the chunk's content with.</param>
        /// <param name="settings">Settings according to which the chunk's content must be read.</param>
        /// <param name="size">Expected size of the content taken from the chunk's header.</param>
        /// <exception cref="ObjectDisposedException">Method was called after the reader's underlying stream was disposed.</exception>
        /// <exception cref="IOException">An I/O error occurred on the reader's underlying stream.</exception>
        protected override void ReadContent(MidiReader reader, ReadingSettings settings, uint size)
        {
            if (size == 0)
            {
                switch (settings.ZeroLengthDataPolicy)
                {
                    case ZeroLengthDataPolicy.ReadAsEmptyObject:
                        Data = new byte[0];
                        break;
                    case ZeroLengthDataPolicy.ReadAsNull:
                        Data = null;
                        break;
                }

                return;
            }

            var availableSize = reader.Length - reader.Position;
            var bytesCount = availableSize < size ? availableSize : size;
            var bytes = reader.ReadBytes((int)Math.Min(bytesCount, int.MaxValue));
            if (bytes.Length < size && settings.NotEnoughBytesPolicy == NotEnoughBytesPolicy.Abort)
                throw new NotEnoughBytesException(
                    "Unknown chunk's data cannot be read since the reader's underlying stream doesn't have enough bytes.",
                    size,
                    bytes.Length);

            Data = bytes;
        }

        /// <summary>
        /// Writes content of a <see cref="UnknownChunk"/>.
        /// </summary>
        /// <remarks>
        /// Content of an <see cref="UnknownChunk"/> is array of bytes.
        /// </remarks>
        /// <param name="writer">Writer to write the chunk's content with.</param>
        /// <param name="settings">Settings according to which the chunk's content must be written.</param>
        /// <exception cref="ObjectDisposedException">Method was called after the writer's underlying stream was disposed.</exception>
        /// <exception cref="IOException">An I/O error occurred on the writer's underlying stream.</exception>
        protected override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
            var data = Data;
            if (data != null)
                writer.WriteBytes(data);
        }

        /// <summary>
        /// Gets size of <see cref="UnknownChunk"/>'s content as number of bytes required to write it according
        /// to the specified <see cref="WritingSettings"/>.
        /// </summary>
        /// <param name="settings">Settings according to which the chunk's content will be written.</param>
        /// <returns>Number of bytes required to write <see cref="UnknownChunk"/>'s content.</returns>
        protected override uint GetContentSize(WritingSettings settings)
        {
            return (uint)(Data?.Length ?? 0);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Unknown chunk ({ChunkId})";
        }

        #endregion
    }
}
