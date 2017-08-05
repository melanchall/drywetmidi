using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class NoteDefinition
    {
        #region Constructor

        public NoteDefinition(NoteName noteName, int octave)
            : this(NoteUtilities.GetNoteNumber(noteName, octave))
        {
        }

        public NoteDefinition(SevenBitNumber noteNumber)
        {
            NoteNumber = noteNumber;
        }

        #endregion

        #region Properties

        public SevenBitNumber NoteNumber { get; }

        public NoteName NoteName => NoteUtilities.GetNoteName(NoteNumber);

        public int Octave => NoteUtilities.GetNoteOctave(NoteNumber);

        #endregion
    }
}
