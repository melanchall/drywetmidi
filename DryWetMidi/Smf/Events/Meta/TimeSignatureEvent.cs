using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Represents a Time Signature meta event.
    /// </summary>
    /// <remarks>
    /// The MIDI time signature meta message defines the musical time signature of a MIDI sequence.
    /// </remarks>
    public sealed class TimeSignatureEvent : MetaEvent
    {
        #region Constants

        /// <summary>
        /// Numerator of the default time signature.
        /// </summary>
        public const byte DefaultNumerator = 4;

        /// <summary>
        /// Denominator of the default time signature.
        /// </summary>
        public const byte DefaultDenominator = 4;

        /// <summary>
        /// Default number of MIDI clock ticks per metronome click.
        /// </summary>
        public const byte DefaultClocksPerClick = 24;

        /// <summary>
        /// Default number of 32nd notes per beat.
        /// </summary>
        public const byte Default32ndNotesPerBeat = 8;

        #endregion

        #region Fields

        private byte _denominator = DefaultDenominator;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSignatureEvent"/>.
        /// </summary>
        public TimeSignatureEvent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSignatureEvent"/> with the
        /// specified numerator and denominator.
        /// </summary>
        /// <param name="numerator">Numerator of the time signature.</param>
        /// <param name="denominator">Denominator of the time signature.</param>
        public TimeSignatureEvent(byte numerator, byte denominator)
            : this(numerator, denominator, DefaultClocksPerClick, Default32ndNotesPerBeat)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSignatureEvent"/> with the
        /// specified numerator, denominator, number of MIDI clocks per metronome click
        /// and number of 32nd notes per beat.
        /// </summary>
        /// <param name="numerator">Numerator of the time signature.</param>
        /// <param name="denominator">Denominator of the time signature.</param>
        /// <param name="clocksPerClick">Number of MIDI clocks per metronome click.</param>
        /// <param name="numberOf32ndNotesPerBeat">Number of 32nd notes per beat.</param>
        public TimeSignatureEvent(byte numerator, byte denominator, byte clocksPerClick, byte numberOf32ndNotesPerBeat)
            : this()
        {
            Numerator = numerator;
            Denominator = denominator;
            ClocksPerClick = clocksPerClick;
            NumberOf32ndNotesPerBeat = numberOf32ndNotesPerBeat;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets numerator of the time signature.
        /// </summary>
        public byte Numerator { get; set; } = DefaultNumerator;

        /// <summary>
        /// Gets or sets denominator of the time signature.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Denominator is zero or is not a
        /// power of two.</exception>
        public byte Denominator
        {
            get { return _denominator; }
            set
            {
                if (!NumberUtilities.IsPowerOfTwo(value))
                    throw new ArgumentOutOfRangeException(nameof(value),
                                                          value,
                                                          "Denominator is zero or is not a power of two.");

                _denominator = value;
            }
        }

        /// <summary>
        /// Gets or sets number of MIDI clock ticks per metronome click.
        /// </summary>
        public byte ClocksPerClick { get; set; } = DefaultClocksPerClick;

        /// <summary>
        /// Gets or sets number of 32nd notes per beat.
        /// </summary>
        public byte NumberOf32ndNotesPerBeat { get; set; } = Default32ndNotesPerBeat;

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the specified event is equal to the current one.
        /// </summary>
        /// <param name="timeSignatureEvent">The event to compare with the current one.</param>
        /// <returns>true if the specified event is equal to the current one; otherwise, false.</returns>
        public bool Equals(TimeSignatureEvent timeSignatureEvent)
        {
            return Equals(timeSignatureEvent, true);
        }

        /// <summary>
        /// Determines whether the specified event is equal to the current one.
        /// </summary>
        /// <param name="timeSignatureEvent">The event to compare with the current one.</param>
        /// <param name="respectDeltaTime">If true the <see cref="MidiEvent.DeltaTime"/> will be taken into an account
        /// while comparing events; if false - delta-times will be ignored.</param>
        /// <returns>true if the specified event is equal to the current one; otherwise, false.</returns>
        public bool Equals(TimeSignatureEvent timeSignatureEvent, bool respectDeltaTime)
        {
            if (ReferenceEquals(null, timeSignatureEvent))
                return false;

            if (ReferenceEquals(this, timeSignatureEvent))
                return true;

            return base.Equals(timeSignatureEvent, respectDeltaTime) &&
                   Numerator == timeSignatureEvent.Numerator &&
                   Denominator == timeSignatureEvent.Denominator &&
                   ClocksPerClick == timeSignatureEvent.ClocksPerClick &&
                   NumberOf32ndNotesPerBeat == timeSignatureEvent.NumberOf32ndNotesPerBeat;
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Reads content of a MIDI meta event.
        /// </summary>
        /// <param name="reader">Reader to read the content with.</param>
        /// <param name="settings">Settings according to which the event's content must be read.</param>
        /// <param name="size">Size of the event's content.</param>
        protected override void ReadContent(MidiReader reader, ReadingSettings settings, int size)
        {
            Numerator = reader.ReadByte();
            Denominator = (byte)Math.Pow(2, reader.ReadByte());

            if (size >= 4)
            {
                ClocksPerClick = reader.ReadByte();
                NumberOf32ndNotesPerBeat = reader.ReadByte();
            }
        }

        /// <summary>
        /// Writes content of a MIDI meta event.
        /// </summary>
        /// <param name="writer">Writer to write the content with.</param>
        /// <param name="settings">Settings according to which the event's content must be written.</param>
        protected override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteByte(Numerator);
            writer.WriteByte((byte)Math.Log(Denominator, 2));
            writer.WriteByte(ClocksPerClick);
            writer.WriteByte(NumberOf32ndNotesPerBeat);
        }

        /// <summary>
        /// Gets the size of the content of a MIDI meta event.
        /// </summary>
        /// <returns>Size of the event's content.</returns>
        protected override int GetContentSize()
        {
            return 4;
        }

        /// <summary>
        /// Clones event by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the event.</returns>
        protected override MidiEvent CloneEvent()
        {
            return new TimeSignatureEvent(Numerator, Denominator, ClocksPerClick, NumberOf32ndNotesPerBeat);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Time Signature ({Numerator}/{Denominator}, {ClocksPerClick} clock/click, {NumberOf32ndNotesPerBeat} 32nd/beat)";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as TimeSignatureEvent);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode() ^ Numerator.GetHashCode() ^
                                        Denominator.GetHashCode() ^
                                        ClocksPerClick.GetHashCode() ^
                                        NumberOf32ndNotesPerBeat.GetHashCode();
        }

        #endregion
    }
}
