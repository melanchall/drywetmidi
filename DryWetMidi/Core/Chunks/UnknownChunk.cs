using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

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
            if (settings.MaxMemorySize != null)
                reader.AddEstimatedMemory(MemorySizeEstimator.EstimateArray(typeof(byte), bytesCount), settings);
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
        protected override async Task ReadContentAsync(
            MidiReader reader,
            ReadingSettings settings,
            uint size,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (size == 0)
            {
                ReadContent(reader, settings, size);
                return;
            }

            byte[] bytes;
            if (reader.Length == long.MaxValue && settings.MaxMemorySize != null)
            {
                using var memory = new MemoryStream();
                long remaining = Math.Min(size, int.MaxValue);

                while (remaining > 0)
                {
                    var packet = await reader.ReadBytesAsync((int)Math.Min(remaining, 4096), cancellationToken).ConfigureAwait(false);
                    if (packet.Length == 0)
                        break;

                    reader.ThrowIfMemoryLimitExceeded(
                        MemorySizeEstimator.EstimateArray(typeof(byte), memory.Length + packet.Length),
                        settings);
                    memory.Write(packet, 0, packet.Length);
                    remaining -= packet.Length;
                }

                bytes = memory.ToArray();
                if (settings.MaxMemorySize != null)
                    reader.AddEstimatedMemory(MemorySizeEstimator.EstimateArray(typeof(byte), bytes.Length), settings);
            }
            else
            {
                var bytesCount = Math.Min(size, Math.Max(0, reader.Length - reader.Position));
                if (settings.MaxMemorySize != null)
                    reader.AddEstimatedMemory(MemorySizeEstimator.EstimateArray(typeof(byte), bytesCount), settings);

                bytes = await reader.ReadBytesAsync((int)Math.Min(bytesCount, int.MaxValue), cancellationToken).ConfigureAwait(false);
            }

            if (bytes.Length < size && settings.NotEnoughBytesPolicy == NotEnoughBytesPolicy.Abort)
                throw new NotEnoughBytesException(
                    "Unknown chunk's data cannot be read since the reader's underlying stream doesn't have enough bytes.",
                    size,
                    bytes.Length);

            Data = bytes;
        }

        /// <inheritdoc/>
        protected override Task WriteContentAsync(
            MidiWriter writer,
            WritingSettings settings,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Data == null ? Task.CompletedTask : writer.WriteDataAsync(Data, cancellationToken);
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
