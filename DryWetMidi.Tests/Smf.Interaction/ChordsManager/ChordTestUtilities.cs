using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction
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
