using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tests.Utilities
{
    internal static class ChordEquality
    {
        #region Nested classes

        private sealed class ChordComparer : IEqualityComparer<Chord>
        {
            #region IEqualityComparer<Chord>

            public bool Equals(Chord chord1, Chord chord2)
            {
                if (ReferenceEquals(chord1, chord2))
                    return true;

                if (ReferenceEquals(null, chord1) || ReferenceEquals(null, chord2))
                    return false;

                return NoteEquality.AreEqual(chord1.Notes, chord2.Notes);
            }

            public int GetHashCode(Chord chord)
            {
                return chord.GetHashCode();
            }

            #endregion
        }

        #endregion

        #region Methods

        public static bool AreEqual(Chord chord1, Chord chord2)
        {
            return new ChordComparer().Equals(chord1, chord2);
        }

        public static bool AreEqual(IEnumerable<Chord> chords1, IEnumerable<Chord> chords2)
        {
            if (ReferenceEquals(chords1, chords2))
                return true;

            if (ReferenceEquals(null, chords1) || ReferenceEquals(null, chords2))
                return false;

            return chords1.SequenceEqual(chords2, new ChordComparer());
        }

        #endregion
    }
}
