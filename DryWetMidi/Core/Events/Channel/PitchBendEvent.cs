using System;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents a Pitch Bend Change message.
    /// </summary>
    /// <remarks>
    /// This message is sent to indicate a change in the pitch bender (wheel or lever, typically).
    /// The pitch bender is measured by a fourteen bit value. Center (no pitch change) is 0x2000.
    /// </remarks>
    public sealed class PitchBendEvent : ChannelEvent
    {
        #region Constants

        /// <summary>
        /// Represents the smallest possible pitch value.
        /// </summary>
        public const ushort MinPitchValue = 0;

        /// <summary>
        /// Represents the largest possible pitch value.
        /// </summary>
        public const ushort MaxPitchValue = (1 << 14) - 1;

        /// <summary>
        /// Represents the default pitch value which means no pitch bend applied.
        /// </summary>
        public const ushort DefaultPitchValue = 1 << 13;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PitchBendEvent"/>.
        /// </summary>
        public PitchBendEvent()
            : this(DefaultPitchValue)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PitchBendEvent"/> with the specified
        /// pitch value.
        /// </summary>
        /// <param name="pitchValue">Pitch value.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pitchValue"/> is out of
        /// [<see cref="MinPitchValue"/>; <see cref="MaxPitchValue"/>] range.</exception>
        public PitchBendEvent(ushort pitchValue)
            : base(MidiEventType.PitchBend)
        {
            PitchValue = pitchValue;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets pitch value.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is out of
        /// [<see cref="MinPitchValue"/>; <see cref="MaxPitchValue"/>] range.</exception>
        public ushort PitchValue
        {
            get
            {
                return DataTypesUtilities.CombineAsSevenBitNumbers(_dataByte2, _dataByte1);
            }
            set
            {
                ThrowIfArgument.IsOutOfRange(
                    nameof(value),
                    value,
                    MinPitchValue,
                    MaxPitchValue,
                    $"Pitch value is out of [{MinPitchValue}; {MaxPitchValue}] range.");

                _dataByte1 = value.GetTail();
                _dataByte2 = value.GetHead();
            }
        }

        #endregion

        #region Overrides

        internal override void Read(MidiReader reader, ReadingSettings settings, int size)
        {
            _dataByte1 = ReadDataByte(reader, settings);
            _dataByte2 = ReadDataByte(reader, settings);
        }

        internal override void Write(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteByte(_dataByte1);
            writer.WriteByte(_dataByte2);
        }

        internal override int GetSize(WritingSettings settings)
        {
            return 2;
        }

        /// <summary>
        /// Clones event by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the event.</returns>
        protected override MidiEvent CloneEvent()
        {
            return new PitchBendEvent
            {
                _dataByte1 = _dataByte1,
                _dataByte2 = _dataByte2,
                Channel = Channel
            };
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Pitch Bend [{Channel}] ({PitchValue})";
        }

        #endregion
    }
}
