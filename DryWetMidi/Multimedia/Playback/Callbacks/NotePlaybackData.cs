using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Data related to MIDI note.
    /// </summary>
    /// <seealso cref="NoteCallback"/>
    public sealed class NotePlaybackData
    {
        #region Constants

        /// <summary>
        /// Data which instructs playback to skip note.
        /// </summary>
        public static readonly NotePlaybackData SkipNote = new NotePlaybackData(false);

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NotePlaybackData"/> with the specified
        /// note number, velocity, off velocity and channel.
        /// </summary>
        /// <param name="noteNumber">Note number.</param>
        /// <param name="velocity">Velocity of Note On event of the note.</param>
        /// <param name="offVelocity">Velocity of Note Off event of the note.</param>
        /// <param name="channel">Note channel.</param>
        public NotePlaybackData(SevenBitNumber noteNumber, SevenBitNumber velocity, SevenBitNumber offVelocity, FourBitNumber channel)
            : this(true)
        {
            NoteNumber = noteNumber;
            Velocity = velocity;
            OffVelocity = offVelocity;
            Channel = channel;
        }

        private NotePlaybackData(bool playNote)
        {
            PlayNote = playNote;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the note number.
        /// </summary>
        public SevenBitNumber NoteNumber { get; }

        /// <summary>
        /// Gets the velocity of Note On event of the note.
        /// </summary>
        public SevenBitNumber Velocity { get; }

        /// <summary>
        /// Gets the velocity of Note Off event of the note.
        /// </summary>
        public SevenBitNumber OffVelocity { get; }

        /// <summary>
        /// Gets the note channel.
        /// </summary>
        public FourBitNumber Channel { get; }

        internal bool PlayNote { get; }

        #endregion

        #region Methods

        internal NoteOnEvent GetNoteOnEvent()
        {
            return new NoteOnEvent(NoteNumber, Velocity) { Channel = Channel };
        }

        internal NoteOffEvent GetNoteOffEvent()
        {
            return new NoteOffEvent(NoteNumber, OffVelocity) { Channel = Channel };
        }

        #endregion
    }
}
