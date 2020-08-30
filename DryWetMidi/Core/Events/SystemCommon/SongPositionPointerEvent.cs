using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
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
        #region Fields

        private SevenBitNumber _lsb;
        private SevenBitNumber _msb;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SongPositionPointerEvent"/>.
        /// </summary>
        public SongPositionPointerEvent()
            : base(MidiEventType.SongPositionPointer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SongPositionPointerEvent"/> with the specified
        /// MSB and LSB parts of the pointer value.
        /// </summary>
        /// <param name="pointerValue">The value of a song position pointer.</param>
        public SongPositionPointerEvent(ushort pointerValue)
            : this()
        {
            PointerValue = pointerValue;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the song position pointer value.
        /// </summary>
        public ushort PointerValue
        {
            get
            {
                return DataTypesUtilities.Combine(_msb, _lsb);
            }
            set
            {
                _msb = value.GetHead();
                _lsb = value.GetTail();
            }
        }

        #endregion

        #region Methods

        private SevenBitNumber ProcessValue(byte value, string property, InvalidSystemCommonEventParameterValuePolicy policy)
        {
            if (value > SevenBitNumber.MaxValue)
            {
                switch (policy)
                {
                    case InvalidSystemCommonEventParameterValuePolicy.Abort:
                        throw new InvalidSystemCommonEventParameterValueException(EventType, property, value);
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
            _lsb = ProcessValue(reader.ReadByte(), "LSB", settings.InvalidSystemCommonEventParameterValuePolicy);
            _msb = ProcessValue(reader.ReadByte(), "MSB", settings.InvalidSystemCommonEventParameterValuePolicy);
        }

        internal override void Write(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteByte(_lsb);
            writer.WriteByte(_msb);
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
            return new SongPositionPointerEvent(PointerValue);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Song Position Pointer ({PointerValue})";
        }

        #endregion
    }
}
