using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    public static class ChordUtilities
    {
        #region Methods

        public static IEnumerable<Interval> GetIntervalsFromRootNote(this Chord chord)
        {
            ThrowIfArgument.IsNull(nameof(chord), chord);

            var lastNoteNumber = (int)chord.NotesNames.First();
            var lastInterval = SevenBitNumber.MinValue;

            var result = new List<Interval>();

            foreach (var interval in GetIntervals(chord))
            {
                if (lastInterval + interval > SevenBitNumber.MaxValue)
                    throw new InvalidOperationException($"Some interval(s) are greater than {SevenBitNumber.MaxValue}.");

                lastInterval += interval;
                result.Add(Interval.GetUp(lastInterval));
            }

            return result;
        }

        public static IEnumerable<Interval> GetIntervalsBetweenNotes(this Chord chord)
        {
            ThrowIfArgument.IsNull(nameof(chord), chord);

            return GetIntervals(chord).Select(i => Interval.FromHalfSteps(i)).ToList();
        }

        public static Note ResolveRootNote(this Chord chord, Octave octave)
        {
            ThrowIfArgument.IsNull(nameof(chord), chord);
            ThrowIfArgument.IsNull(nameof(octave), octave);

            return octave.GetNote(chord.RootNoteName);
        }

        public static IEnumerable<Note> ResolveNotes(this Chord chord, Octave octave)
        {
            ThrowIfArgument.IsNull(nameof(chord), chord);
            ThrowIfArgument.IsNull(nameof(octave), octave);

            var rootNote = chord.ResolveRootNote(octave);
            var result = new List<Note> { rootNote };
            result.AddRange(chord.GetIntervalsFromRootNote().Select(i => rootNote + i));
            return result;
        }

        private static IEnumerable<SevenBitNumber> GetIntervals(Chord chord)
        {
            var lastNoteNumber = (int)chord.NotesNames.First();

            foreach (var noteName in chord.NotesNames.Skip(1))
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
