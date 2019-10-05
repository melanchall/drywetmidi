using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    public static class ChordFactory
    {
        #region Constants

        private static readonly Dictionary<ChordQuality, Interval[]> IntervalsByQuality = new Dictionary<ChordQuality, Interval[]>
        {
            [ChordQuality.Major] = new[] { Interval.Get(IntervalQuality.Major, 3), Interval.Get(IntervalQuality.Perfect, 5) },
            [ChordQuality.Minor] = new[] { Interval.Get(IntervalQuality.Minor, 3), Interval.Get(IntervalQuality.Perfect, 5) },
            [ChordQuality.Augmented] = new[] { Interval.Get(IntervalQuality.Major, 3), Interval.Get(IntervalQuality.Augmented, 5) },
            [ChordQuality.Diminished] = new[] { Interval.Get(IntervalQuality.Minor, 3), Interval.Get(IntervalQuality.Diminished, 5) }
        };

        #endregion

        #region Methods

        public static Chord GetChord(NoteName rootNoteName, ChordQuality chordQuality)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(rootNoteName), rootNoteName);
            ThrowIfArgument.IsInvalidEnumValue(nameof(chordQuality), chordQuality);

            var intervals = IntervalsByQuality[chordQuality];
            return new Chord(rootNoteName, intervals);
        }

        #endregion
    }
}
