using System;
using System.Linq;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public static class ChordsProcessingUtilities
    {
        #region Methods

        public static Tuple<Chord, Chord> Split(this Chord chord, long time)
        {
            ThrowIfArgument.IsNull(nameof(chord), chord);
            ThrowIfTimeArgument.IsNegative(nameof(time), time);

            //

            var startTime = chord.Time;
            var endTime = startTime + chord.Length;

            if (time <= startTime)
                return Tuple.Create(default(Chord), chord.Clone());

            if (time >= endTime)
                return Tuple.Create(chord.Clone(), default(Chord));

            //

            var parts = chord.Notes.Select(n => n.Split(time)).ToArray();

            var part1 = new Chord(parts.Select(p => p.Item1).Where(p => p != null));
            var part2 = new Chord(parts.Select(p => p.Item2).Where(p => p != null));

            return Tuple.Create(part1, part2);
        }

        #endregion
    }
}
