using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.MusicTheory
{
    public static class ScaleUtilities
    {
        #region Methods

        public static NoteName GetDegree(this Scale scale, ScaleDegree degree)
        {
            ThrowIfArgument.IsNull(nameof(scale), scale);
            ThrowIfArgument.IsInvalidEnumValue(nameof(degree), degree);

            return scale.GetNotes()
                        .Skip((int)degree)
                        .First()
                        .NoteName;
        }

        public static IEnumerable<Note> GetNotes(this Scale scale)
        {
            ThrowIfArgument.IsNull(nameof(scale), scale);

            var noteNumber = Enumerable.Range(SevenBitNumber.MinValue, SevenBitNumber.MaxValue - SevenBitNumber.MinValue + 1)
                                       .SkipWhile(n => NoteUtilities.GetNoteName((SevenBitNumber)n) != scale.RootNoteName)
                                       .First();

            yield return Note.Get((SevenBitNumber)noteNumber);

            while (true)
            {
                foreach (var interval in scale.Intervals)
                {
                    noteNumber += interval;
                    if (noteNumber < SevenBitNumber.MinValue || noteNumber > SevenBitNumber.MaxValue)
                        yield break;

                    yield return Note.Get((SevenBitNumber)noteNumber);
                }
            }
        }

        public static IEnumerable<Note> GetAscendingNotes(this Scale scale, Note rootNote)
        {
            ThrowIfArgument.IsNull(nameof(scale), scale);
            ThrowIfArgument.IsNull(nameof(rootNote), rootNote);

            return scale.GetNotes()
                        .SkipWhile(n => n != rootNote);
        }

        public static IEnumerable<Note> GetDescendingNotes(this Scale scale, Note rootNote)
        {
            ThrowIfArgument.IsNull(nameof(scale), scale);
            ThrowIfArgument.IsNull(nameof(rootNote), rootNote);

            return new[] { rootNote }.Concat(scale.GetNotes()
                                                  .TakeWhile(n => n != rootNote)
                                                  .Reverse());
        }

        public static bool IsNoteInScale(this Scale scale, Note note)
        {
            ThrowIfArgument.IsNull(nameof(scale), scale);
            ThrowIfArgument.IsNull(nameof(note), note);

            return scale.GetNotes()
                        .Contains(note);
        }

        public static Note GetNextNote(this Scale scale, Note note)
        {
            ThrowIfArgument.IsNull(nameof(scale), scale);
            ThrowIfArgument.IsNull(nameof(note), note);

            return scale.GetAscendingNotes(note)
                        .Skip(1)
                        .FirstOrDefault();
        }

        public static Note GetPreviousNote(this Scale scale, Note note)
        {
            ThrowIfArgument.IsNull(nameof(scale), scale);
            ThrowIfArgument.IsNull(nameof(note), note);

            return scale.GetDescendingNotes(note)
                        .Skip(1)
                        .FirstOrDefault();
        }

        #endregion
    }
}
