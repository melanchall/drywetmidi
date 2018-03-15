using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Smf.Interaction;

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

                return NoteEquality.Equals(chord1.Notes, chord2.Notes);
            }

            public int GetHashCode(Chord chord)
            {
                return chord.Notes
                            .Select(n => n.GetHashCode())
                            .Aggregate((x, y) => x ^ y);
            }

            #endregion
        }

        #endregion

        #region Methods

        public static bool Equals(Chord chord1, Chord chord2)
        {
            return new ChordComparer().Equals(chord1, chord2);
        }

        public static bool Equals(IEnumerable<Chord> chord1, IEnumerable<Chord> chord2)
        {
            if (ReferenceEquals(chord1, chord2))
                return true;

            if (ReferenceEquals(null, chord1) || ReferenceEquals(null, chord2))
                return false;

            return chord1.SequenceEqual(chord2, new ChordComparer());
        }

        #endregion
    }
}
