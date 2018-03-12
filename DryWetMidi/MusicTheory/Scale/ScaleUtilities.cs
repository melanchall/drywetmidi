using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    /// <summary>
    /// Provides useful utilities for working with <see cref="Scale"/>.
    /// </summary>
    public static class ScaleUtilities
    {
        #region Methods

        /// <summary>
        /// Gets <see cref="NoteName"/> corresponding to the specified degree of a musical scale.
        /// </summary>
        /// <param name="scale"><see cref="Scale"/> to get degree of.</param>
        /// <param name="degree"><see cref="ScaleDegree"/> representing a degree of the
        /// <paramref name="scale"/>.</param>
        /// <returns>The degree of the scale.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="scale"/> is null.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="degree"/> specified an
        /// invalid value.</exception>
        public static NoteName GetDegree(this Scale scale, ScaleDegree degree)
        {
            ThrowIfArgument.IsNull(nameof(scale), scale);
            ThrowIfArgument.IsInvalidEnumValue(nameof(degree), degree);

            // TODO: check for invalid degree

            return scale.GetNotes()
                        .Skip((int)degree)
                        .First()
                        .NoteName;
        }

        /// <summary>
        /// Gets all notes that belong to a musical scale.
        /// </summary>
        /// <param name="scale"><see cref="Scale"/> to get notes of.</param>
        /// <returns>Notes that belong to the <paramref name="scale"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="scale"/> is null.</exception>
        public static IEnumerable<Note> GetNotes(this Scale scale)
        {
            ThrowIfArgument.IsNull(nameof(scale), scale);

            var noteNumber = Enumerable.Range(SevenBitNumber.MinValue, SevenBitNumber.MaxValue - SevenBitNumber.MinValue + 1)
                                       .SkipWhile(n => NoteUtilities.GetNoteName((SevenBitNumber)n) != scale.RootNote)
                                       .First();

            yield return Note.Get((SevenBitNumber)noteNumber);

            while (true)
            {
                foreach (var interval in scale.Intervals)
                {
                    noteNumber += interval;
                    if (!NoteUtilities.IsNoteNumberValid(noteNumber))
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
