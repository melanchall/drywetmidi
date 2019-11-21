using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    internal static class NoteTestUtilities
    {
        #region Constants

        private static readonly SevenBitNumber NoteNumber = (SevenBitNumber)0;
        private const long NoteLength = 100;

        #endregion

        #region Methods

        public static Note GetNote()
        {
            return new Note(NoteNumber);
        }

        public static Note GetNoteByTime(long time)
        {
            return new Note(NoteNumber, NoteLength, time);
        }

        #endregion
    }
}
