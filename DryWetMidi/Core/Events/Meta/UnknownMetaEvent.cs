using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents an unknown meta event.
    /// </summary>
    /// <remarks>
    /// The structure of meta events allows custom ones to be implemented and stored within a MIDI file.
    /// Any meta event DryWetMIDI doesn't know about will be read as an instance of the
    /// <see cref="UnknownMetaEvent"/>.
    /// </remarks>
    public sealed class UnknownMetaEvent : MetaEvent
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownMetaEvent"/> with the
        /// specified status byte.
        /// </summary>
        /// <param name="statusByte">Status byte of the meta event.</param>
        internal UnknownMetaEvent(byte statusByte)
            : this(statusByte, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownMetaEvent"/> with the
        /// specified status byte and data.
        /// </summary>
        /// <param name="statusByte">Status byte of the meta event.</param>
        /// <param name="data">Data of an unknown meta event.</param>
        internal UnknownMetaEvent(byte statusByte, byte[]? data)
            : base(MidiEventType.UnknownMeta)
        {
            StatusByte = statusByte;
            Data = data;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the status byte of the meta event.
        /// </summary>
        public byte StatusByte { get; }

        /// <summary>
        /// Gets the content of the meta event as array of bytes.
        /// </summary>
        public byte[]? Data { get; private set; }

        #endregion

        #region Overrides

        /// <inheritdoc/>
        /// <exception cref="ArgumentOutOfRangeException">Unknown meta event cannot be read since the size is
        /// negative number.</exception>
        protected override void ReadContent(MidiReader reader, ReadingSettings settings, int size)
        {
            ThrowIfArgument.IsNegative(
                nameof(size),
                size,
                "Unknown meta event cannot be read since the size is negative number.");

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

            var data = reader.ReadBytes(size);
            if (data.Length != size && settings.NotEnoughBytesPolicy == NotEnoughBytesPolicy.Abort)
                throw new NotEnoughBytesException("Not enough bytes in the stream to read the data of an unknown meta event.", size, data.Length);

            Data = data;
        }

        /// <inheritdoc/>
        protected override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
            var data = Data;
            if (data != null)
                writer.WriteBytes(data);
        }

        /// <inheritdoc/>
        protected override int GetContentSize(WritingSettings settings)
        {
            return Data?.Length ?? 0;
        }

        /// <inheritdoc/>
        protected override MidiEvent CloneEvent()
        {
            return new UnknownMetaEvent(StatusByte, Data?.Clone() as byte[]);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Unknown meta event ({StatusByte})";
        }

        #endregion
    }
}
