using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Composing
{
    public sealed class NoteDescriptor
    {
        #region Constructor

        public NoteDescriptor(MusicTheory.Note note, SevenBitNumber velocity, ITimeSpan length)
        {
            Note = note;
            Velocity = velocity;
            Length = length;
        }

        #endregion

        #region Properties

        public MusicTheory.Note Note { get; }

        public SevenBitNumber Velocity { get; }

        public ITimeSpan Length { get; set; }

        #endregion
    }
}
