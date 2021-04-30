using System;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents a Set Tempo meta event.
    /// </summary>
    /// <remarks>
    /// The MIDI set tempo meta message sets the tempo of a MIDI sequence in terms
    /// of microseconds per quarter note.
    /// </remarks>
    public sealed class SetTempoEvent : MetaEvent
    {
        #region Constants

        /// <summary>
        /// Default tempo.
        /// </summary>
        public const long DefaultMicrosecondsPerQuarterNote = 500000;

        /// <summary>
        /// Represents the smallest possible microseconds-per-quarter-note value.
        /// </summary>
        public const long MinMicrosecondsPerQuarterNote = 1;

        /// <summary>
        /// Represents the largest possible microseconds-per-quarter-note value.
        /// </summary>
        public const long MaxMicrosecondsPerQuarterNote = (1 << 24) - 1;

        #endregion

        #region Fields

        private long _microsecondsPerBeat = DefaultMicrosecondsPerQuarterNote;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SetTempoEvent"/>.
        /// </summary>
        public SetTempoEvent()
            : base(MidiEventType.SetTempo)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SetTempoEvent"/> with the
        /// specified number of microseconds per quarter note.
        /// </summary>
        /// <param name="microsecondsPerQuarterNote">Number of microseconds per quarter note.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="microsecondsPerQuarterNote"/> is out of
        /// [<see cref="MinMicrosecondsPerQuarterNote"/>; <see cref="MaxMicrosecondsPerQuarterNote"/>] range.</exception>
        public SetTempoEvent(long microsecondsPerQuarterNote)
            : this()
        {
            MicrosecondsPerQuarterNote = microsecondsPerQuarterNote;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets number of microseconds per quarter note.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is out of
        /// [<see cref="MinMicrosecondsPerQuarterNote"/>; <see cref="MaxMicrosecondsPerQuarterNote"/>] range.</exception>
        public long MicrosecondsPerQuarterNote
        {
            get { return _microsecondsPerBeat; }
            set
            {
                ThrowIfArgument.IsOutOfRange(
                    nameof(value),
                    value,
                    MinMicrosecondsPerQuarterNote,
                    MaxMicrosecondsPerQuarterNote,
                    $"Number of microseconds per quarter note is out of [{MinMicrosecondsPerQuarterNote}; {MaxMicrosecondsPerQuarterNote}] range.");

                _microsecondsPerBeat = value;
            }
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
            MicrosecondsPerQuarterNote = reader.Read3ByteDword();
        }

        /// <summary>
        /// Writes content of a MIDI meta event.
        /// </summary>
        /// <param name="writer">Writer to write the content with.</param>
        /// <param name="settings">Settings according to which the event's content must be written.</param>
        protected override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
            writer.Write3ByteDword((uint)MicrosecondsPerQuarterNote);
        }

        /// <summary>
        /// Gets the size of the content of a MIDI meta event.
        /// </summary>
        /// <param name="settings">Settings according to which the event's content must be written.</param>
        /// <returns>Size of the event's content.</returns>
        protected override int GetContentSize(WritingSettings settings)
        {
            return 3;
        }

        /// <summary>
        /// Clones event by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the event.</returns>
        protected override MidiEvent CloneEvent()
        {
            return new SetTempoEvent
            {
                _microsecondsPerBeat = _microsecondsPerBeat
            };
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Set Tempo ({MicrosecondsPerQuarterNote})";
        }

        #endregion
    }
}
