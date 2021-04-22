using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents a Polyphonic Key Pressure (Aftertouch) message.
    /// </summary>
    /// <remarks>
    /// This message is most often sent by pressing down on the key after it "bottoms out".
    /// </remarks>
    public sealed class NoteAftertouchEvent : ChannelEvent
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteAftertouchEvent"/>.
        /// </summary>
        public NoteAftertouchEvent()
            : base(MidiEventType.NoteAftertouch)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteAftertouchEvent"/> with the specified
        /// note number and aftertouch (pressure) value.
        /// </summary>
        /// <param name="noteNumber">Note number.</param>
        /// <param name="aftertouchValue">Aftertouch (pressure) value.</param>
        public NoteAftertouchEvent(SevenBitNumber noteNumber, SevenBitNumber aftertouchValue)
            : this()
        {
            NoteNumber = noteNumber;
            AftertouchValue = aftertouchValue;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets note number.
        /// </summary>
        public SevenBitNumber NoteNumber
        {
            get { return (SevenBitNumber)_dataByte1; }
            set { _dataByte1 = value; }
        }

        /// <summary>
        /// Gets or sets aftertouch (pressure) value.
        /// </summary>
        public SevenBitNumber AftertouchValue
        {
            get { return (SevenBitNumber)_dataByte2; }
            set { _dataByte2 = value; }
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
            return new NoteAftertouchEvent
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
            return $"Note Aftertouch [{Channel}] ({NoteNumber}, {AftertouchValue})";
        }

        #endregion
    }
}
