using System;
using System.IO;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents an unknown chunk.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The structure of MIDI file chunks allows custom chunks to be implemented and written to a MIDI file.
    /// Chunks DryWetMIDI doesn't know about will be read as instances of the <see cref="UnknownChunk"/>.
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
        public byte[]? Data { get; internal set; }

        #endregion

        #region Overrides

        /// <inheritdoc/>
        public override MidiChunk Clone()
        {
            return new UnknownChunk(ChunkId)
            {
                Data = Data?.Clone() as byte[]
            };
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Content of an <see cref="UnknownChunk"/> is array of bytes.
        /// </remarks>
        /// <exception cref="NotEnoughBytesException">The reader's underlying stream doesn't have enough bytes
        /// to read the chunk's data and <see cref="ReadingSettings.NotEnoughBytesPolicy"/> is set to
        /// <see cref="NotEnoughBytesPolicy.Abort"/>.</exception>
        protected override void ReadContent(MidiReader reader, ReadingSettings settings, uint size)
        {
            if (size == 0)
            {
                switch (settings.ZeroLengthDataPolicy)
                {
                    case ZeroLengthDataPolicy.ReadAsEmptyObject:
                        Data = Array.Empty<byte>();
                        break;
                    case ZeroLengthDataPolicy.ReadAsNull:
                        Data = null;
                        break;
                }

                return;
            }

            var availableSize = reader.Length - reader.Position;
            var bytesCount = availableSize < size ? availableSize : size;
            reader.AddEstimatedMemory(48 + bytesCount, settings);
            var bytes = reader.ReadBytes((int)Math.Min(bytesCount, int.MaxValue));
            if (bytes.Length < size && settings.NotEnoughBytesPolicy == NotEnoughBytesPolicy.Abort)
                throw new NotEnoughBytesException(
                    "Unknown chunk's data cannot be read since the reader's underlying stream doesn't have enough bytes.",
                    size,
                    bytes.Length);

            Data = bytes;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Content of an <see cref="UnknownChunk"/> is array of bytes.
        /// </remarks>
        protected override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
            var data = Data;
            if (data != null)
                writer.WriteBytes(data);
        }

        /// <inheritdoc/>
        protected override uint GetContentSize(WritingSettings settings)
        {
            return (uint)(Data?.Length ?? 0);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Unknown chunk ({ChunkId})";
        }

        #endregion
    }
}
