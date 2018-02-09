using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    public static class ChordUtilities
    {
        #region Methods

        // TODO: optimize
        public static Chord GetInversion(this Chord chord, int inversionNumber)
        {
            ThrowIfArgument.IsNull(nameof(chord), chord);

            var notes = chord.Notes;
            var inversionsCount = notes.Count() - 1;

            ThrowIfArgument.IsOutOfRange(nameof(inversionNumber),
                                         inversionNumber,
                                         1,
                                         inversionsCount,
                                         "Inversion number is zero or negative.");

            var head = notes.Take(inversionNumber);
            var tail = notes.Skip(inversionNumber);

            return new Chord(tail.Concat(head).ToArray());
        }

        // TODO: optimize
        public static IEnumerable<Chord> GetInversions(this Chord chord)
        {
            ThrowIfArgument.IsNull(nameof(chord), chord);

            var inversionsCount = chord.Notes.Count() - 1;
            return Enumerable.Range(1, inversionsCount)
                             .Select(i => chord.GetInversion(i));
        }

        #endregion
    }
}
