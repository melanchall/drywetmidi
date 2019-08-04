using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;

namespace Melanchall.DryWetMidi.Devices
{
    public sealed class NotePlaybackData
    {
        #region Constants

        public static readonly NotePlaybackData SkipNote = new NotePlaybackData(false);

        #endregion

        #region Constructor

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

        public SevenBitNumber NoteNumber { get; }

        public SevenBitNumber Velocity { get; }

        public SevenBitNumber OffVelocity { get; }

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
