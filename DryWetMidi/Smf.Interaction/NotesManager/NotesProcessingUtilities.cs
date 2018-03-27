using System;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public static class NotesProcessingUtilities
    {
        #region Methods

        public static Tuple<Note, Note> Split(this Note note, long time)
        {
            ThrowIfArgument.IsNull(nameof(note), note);
            ThrowIfTimeArgument.IsNegative(nameof(time), time);

            //

            var startTime = note.Time;
            var endTime = startTime + note.Length;

            if (time <= startTime)
                return Tuple.Create(default(Note), note.Clone());

            if (time >= endTime)
                return Tuple.Create(note.Clone(), default(Note));

            //

            var part1 = note.Clone();
            part1.Length = time - startTime;

            var part2 = note.Clone();
            part2.Time = time;
            part2.Length = endTime - time;

            return Tuple.Create(part1, part2);
        }

        #endregion
    }
}
