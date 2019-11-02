using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    internal static class ChordTestUtilities
    {
        #region Methods

        public static Chord GetChordByTime(long time)
        {
            return new Chord(new[] { NoteTestUtilities.GetNote() }, time);
        }

        #endregion
    }
}
