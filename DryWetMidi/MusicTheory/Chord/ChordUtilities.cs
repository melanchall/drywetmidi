using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    /// <summary>
    /// Utilities for working with <see cref="Chord"/>.
    /// </summary>
    public static class ChordUtilities
    {
        #region Methods

        /// <summary>
        /// Gets intervals from the root note of the specified chord. For example, +4 and +7 for C major
        /// (+4 for C and E, +7 for C and G).
        /// </summary>
        /// <param name="chord">Chord to get intervals from root note.</param>
        /// <returns>Intervals from the root note of the <paramref name="chord"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="chord"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Some intervals are greater than <see cref="SevenBitNumber.MaxValue"/>.</exception>
        public static IEnumerable<Interval> GetIntervalsFromRootNote(this Chord chord)
        {
            ThrowIfArgument.IsNull(nameof(chord), chord);

            return GetIntervalsFromRootNote(chord.NotesNames);
        }

        /// <summary>
        /// Gets intervals between notes of the specified chord. For example, +4 and +3 for C major
        /// (+4 for C and E, +3 for E and G).
        /// </summary>
        /// <param name="chord">Chord to get intervals between notes.</param>
        /// <returns>Intervals between notes of the <paramref name="chord"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="chord"/> is <c>null</c>.</exception>
        public static IEnumerable<Interval> GetIntervalsBetweenNotes(this Chord chord)
        {
            ThrowIfArgument.IsNull(nameof(chord), chord);

            return GetIntervals(chord).Select(i => Interval.FromHalfSteps(i)).ToList();
        }

        /// <summary>
        /// Resolves root note of the specified chord.
        /// </summary>
        /// <param name="chord">Chord to resolve root note.</param>
        /// <param name="octave">Octave to resolve root note of the <paramref name="chord"/>.</param>
        /// <returns>Root note of the chord regarding to <paramref name="octave"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="chord"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="octave"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static Note ResolveRootNote(this Chord chord, Octave octave)
        {
            ThrowIfArgument.IsNull(nameof(chord), chord);
            ThrowIfArgument.IsNull(nameof(octave), octave);

            return octave.GetNote(chord.RootNoteName);
        }

        /// <summary>
        /// Resolves notes of the specified chord.
        /// </summary>
        /// <param name="chord">Chord to resolve notes.</param>
        /// <param name="octave">Octave to resolve notes of the <paramref name="chord"/>.</param>
        /// <returns>Notes of the chord regarding to <paramref name="octave"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="chord"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="octave"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static IEnumerable<Note> ResolveNotes(this Chord chord, Octave octave)
        {
            ThrowIfArgument.IsNull(nameof(chord), chord);
            ThrowIfArgument.IsNull(nameof(octave), octave);

            var rootNote = chord.ResolveRootNote(octave);
            var result = new List<Note> { rootNote };
            result.AddRange(chord.GetIntervalsFromRootNote().Select(i => rootNote + i));
            return result;
        }

        /// <summary>
        /// Gets the collection of chord's inversions.
        /// </summary>
        /// <returns>Collection of chord's inversions.</returns>
        public static IEnumerable<Chord> GetInversions(this Chord chord)
        {
            ThrowIfArgument.IsNull(nameof(chord), chord);

            foreach (var permutation in MathUtilities.GetPermutations(chord.NotesNames.ToArray()))
            {
                if (permutation[0] == chord.RootNoteName)
                    continue;

                yield return new Chord(permutation.ToArray());
            }
        }

        internal static IEnumerable<Interval> GetIntervalsFromRootNote(ICollection<NoteName> notesNames)
        {
            var lastInterval = SevenBitNumber.MinValue;
            var result = new List<Interval>();

            foreach (var interval in GetIntervals(notesNames))
            {
                if (lastInterval + interval > SevenBitNumber.MaxValue)
                    throw new InvalidOperationException($"Some interval(s) are greater than {SevenBitNumber.MaxValue}.");

                lastInterval += interval;
                result.Add(Interval.GetUp(lastInterval));
            }

            return result;
        }

        private static IEnumerable<SevenBitNumber> GetIntervals(Chord chord)
        {
            return GetIntervals(chord.NotesNames);
        }

        private static IEnumerable<SevenBitNumber> GetIntervals(ICollection<NoteName> notesNames)
        {
            var lastNoteNumber = (int)notesNames.First();

            foreach (var noteName in notesNames.Skip(1))
            {
                var offset = (int)noteName - lastNoteNumber;
                if (offset <= 0)
                    offset += Octave.OctaveSize;

                yield return (SevenBitNumber)offset;
                lastNoteNumber = (int)noteName;
            }
        }

        #endregion
    }
}
