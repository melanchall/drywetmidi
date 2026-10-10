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

        private static readonly string InvalidMicrosecondsPerQuarterNoteRangeMessage =
            $"Number of microseconds per quarter note is out of [{MinMicrosecondsPerQuarterNote}; {MaxMicrosecondsPerQuarterNote}] range.";

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
                    InvalidMicrosecondsPerQuarterNoteRangeMessage);

                _microsecondsPerBeat = value;
            }
        }

        #endregion

        #region Overrides

        /// <inheritdoc/>
        protected override void ReadContent(MidiReader reader, ReadingSettings settings, int size)
        {
            MicrosecondsPerQuarterNote = reader.Read3ByteDword();
        }

        /// <inheritdoc/>
        protected override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
            writer.Write3ByteDword((uint)MicrosecondsPerQuarterNote);
        }

        /// <inheritdoc/>
        protected override int GetContentSize(WritingSettings settings)
        {
            return 3;
        }

        /// <inheritdoc/>
        protected override MidiEvent CloneEvent()
        {
            return new SetTempoEvent
            {
                _microsecondsPerBeat = _microsecondsPerBeat
            };
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Set Tempo ({MicrosecondsPerQuarterNote})";
        }

        #endregion
    }
}
