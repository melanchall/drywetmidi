using System;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Represents Song Position Pointer event.
    /// </summary>
    /// <remarks>
    /// A MIDI event that carries the MIDI song position pointer message tells a MIDI device
    /// to cue to a point in the MIDI sequence to be ready to play.
    /// </remarks>
    public sealed class SongPositionPointerEvent : SystemCommonEvent
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SongPositionPointerEvent"/>.
        /// </summary>
        public SongPositionPointerEvent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SongPositionPointerEvent"/> with the specified
        /// MSB and LSB parts of the pointer value.
        /// </summary>
        /// <param name="msb"></param>
        /// <param name="lsb"></param>
        public SongPositionPointerEvent(SevenBitNumber msb, SevenBitNumber lsb)
        {
            Msb = msb;
            Lsb = lsb;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets MSB of the song position pointer value.
        /// </summary>
        public SevenBitNumber Msb { get; set; }

        /// <summary>
        /// Gets or sets LSB of the song position pointer value.
        /// </summary>
        public SevenBitNumber Lsb { get; set; }

        #endregion

        #region Methods

        private static SevenBitNumber ProcessValue(byte value, string property, InvalidSystemCommonEventParameterValuePolicy policy)
        {
            if (value > SevenBitNumber.MaxValue)
            {
                switch (policy)
                {
                    case InvalidSystemCommonEventParameterValuePolicy.Abort:
                        throw new InvalidSystemCommonEventParameterValueException($"{value} is invalid value for the {property} of a Song Position Pointer event.", value);
                    case InvalidSystemCommonEventParameterValuePolicy.SnapToLimits:
                        return SevenBitNumber.MaxValue;
                }
            }

            return (SevenBitNumber)value;
        }

        #endregion

        #region Overrides

        internal override void Read(MidiReader reader, ReadingSettings settings, int size)
        {
            Lsb = ProcessValue(reader.ReadByte(), nameof(Lsb), settings.InvalidSystemCommonEventParameterValuePolicy);
            Msb = ProcessValue(reader.ReadByte(), nameof(Msb), settings.InvalidSystemCommonEventParameterValuePolicy);
        }

        internal override void Write(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteByte(Lsb);
            writer.WriteByte(Msb);
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
            return new SongPositionPointerEvent(Msb, Lsb);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Song Position Pointer ({Msb}, {Lsb})";
        }

        #endregion
    }
}
