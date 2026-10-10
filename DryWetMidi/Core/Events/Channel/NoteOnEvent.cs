using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents a Note On message.
    /// </summary>
    /// <remarks>
    /// This message is sent when a note is depressed (start).
    /// </remarks>
    public sealed class NoteOnEvent : NoteEvent
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteOnEvent"/>.
        /// </summary>
        public NoteOnEvent()
            : base(MidiEventType.NoteOn)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteOnEvent"/> with the specified
        /// note number and velocity.
        /// </summary>
        /// <param name="noteNumber">Note number.</param>
        /// <param name="velocity">Velocity.</param>
        public NoteOnEvent(SevenBitNumber noteNumber, SevenBitNumber velocity)
            : base(MidiEventType.NoteOn, noteNumber, velocity)
        {
        }

        #endregion

        #region Overrides

        /// <inheritdoc/>
        protected override MidiEvent CloneEvent()
        {
            return new NoteOnEvent
            {
                _dataByte1 = _dataByte1,
                _dataByte2 = _dataByte2,
                Channel = Channel
            };
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Note On [{Channel}] ({NoteNumber}, {Velocity})";
        }

        #endregion
    }
}
