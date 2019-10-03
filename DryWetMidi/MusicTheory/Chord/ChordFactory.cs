using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    public static class ChordFactory
    {
        #region Constants

        private static readonly Dictionary<ChordQuality, int[]> IntervalsByQuality = new Dictionary<ChordQuality, int[]>
        {
            [ChordQuality.Major] = new[] { 0, 4, 7 },
            [ChordQuality.Minor] = new[] { 0, 3, 7 },
            [ChordQuality.Augmented] = new[] { 0, 4, 8 },
            [ChordQuality.Diminished] = new[] { 0, 3, 6 }
        };

        #endregion

        #region Methods

        public static Chord GetChord(NoteName rootNoteName, ChordQuality chordQuality)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(rootNoteName), rootNoteName);
            ThrowIfArgument.IsInvalidEnumValue(nameof(chordQuality), chordQuality);

            var intervals = IntervalsByQuality[chordQuality];
            var notesNames = new NoteName[intervals.Length];

            for (var i = 0; i < intervals.Length; i++)
            {
                notesNames[i] = (NoteName)(((int)rootNoteName + intervals[i]) % Octave.OctaveSize);
            }

            return new Chord(notesNames);
        }

        #endregion
    }
}
