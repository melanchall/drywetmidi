using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Core
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
        public const byte DefaultThirtySecondNotesPerBeat = 8;

        #endregion

        #region Fields

        private byte _denominator = DefaultDenominator;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSignatureEvent"/>.
        /// </summary>
        public TimeSignatureEvent()
            : base(MidiEventType.TimeSignature)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSignatureEvent"/> with the
        /// specified numerator and denominator.
        /// </summary>
        /// <param name="numerator">Numerator of the time signature.</param>
        /// <param name="denominator">Denominator of the time signature.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="denominator"/> is zero or is not a
        /// power of two.</exception>
        public TimeSignatureEvent(byte numerator, byte denominator)
            : this(numerator, denominator, DefaultClocksPerClick, DefaultThirtySecondNotesPerBeat)
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
        /// <param name="thirtySecondNotesPerBeat">Number of 32nd notes per beat.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="denominator"/> is zero or is not a
        /// power of two.</exception>
        public TimeSignatureEvent(byte numerator, byte denominator, byte clocksPerClick, byte thirtySecondNotesPerBeat)
            : this()
        {
            Numerator = numerator;
            Denominator = denominator;
            ClocksPerClick = clocksPerClick;
            ThirtySecondNotesPerBeat = thirtySecondNotesPerBeat;
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
                ThrowIfArgument.DoesntSatisfyCondition(
                    nameof(value),
                    value,
                    v => MathUtilities.IsPowerOfTwo(v),
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
        public byte ThirtySecondNotesPerBeat { get; set; } = DefaultThirtySecondNotesPerBeat;

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
                ThirtySecondNotesPerBeat = reader.ReadByte();
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
            writer.WriteByte(ThirtySecondNotesPerBeat);
        }

        /// <summary>
        /// Gets the size of the content of a MIDI meta event.
        /// </summary>
        /// <param name="settings">Settings according to which the event's content must be written.</param>
        /// <returns>Size of the event's content.</returns>
        protected override int GetContentSize(WritingSettings settings)
        {
            return 4;
        }

        /// <summary>
        /// Clones event by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the event.</returns>
        protected override MidiEvent CloneEvent()
        {
            return new TimeSignatureEvent
            {
                Numerator = Numerator,
                _denominator = _denominator,
                ClocksPerClick = ClocksPerClick,
                ThirtySecondNotesPerBeat = ThirtySecondNotesPerBeat
            };
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Time Signature ({Numerator}/{Denominator}, {ClocksPerClick} clock/click, {ThirtySecondNotesPerBeat} 32nd/beat)";
        }

        #endregion
    }
}
