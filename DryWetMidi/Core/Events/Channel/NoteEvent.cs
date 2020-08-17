using System.ComponentModel;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Base class that represents a Note On or a Note Off message.
    /// </summary>
    public abstract class NoteEvent : ChannelEvent
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteEvent"/>.
        /// </summary>
        protected NoteEvent(MidiEventType eventType)
            : base(eventType)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteEvent"/> with the specified
        /// note number and velocity.
        /// </summary>
        /// <param name="eventType">The type of event.</param>
        /// <param name="noteNumber">Note number.</param>
        /// <param name="velocity">Velocity.</param>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="eventType"/> specified an invalid value.</exception>
        protected NoteEvent(MidiEventType eventType, SevenBitNumber noteNumber, SevenBitNumber velocity)
            : this(eventType)
        {
            NoteNumber = noteNumber;
            Velocity = velocity;
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
        /// Gets or sets velocity.
        /// </summary>
        public SevenBitNumber Velocity
        {
            get { return (SevenBitNumber)_dataByte2; }
            set { _dataByte2 = value; }
        }

        #endregion

        #region Overrides

        internal sealed override void Read(MidiReader reader, ReadingSettings settings, int size)
        {
            _dataByte1 = ReadDataByte(reader, settings);
            _dataByte2 = ReadDataByte(reader, settings);
        }

        internal sealed override void Write(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteByte(_dataByte1);
            writer.WriteByte(_dataByte2);
        }

        internal sealed override int GetSize(WritingSettings settings)
        {
            return 2;
        }

        #endregion
    }
}
