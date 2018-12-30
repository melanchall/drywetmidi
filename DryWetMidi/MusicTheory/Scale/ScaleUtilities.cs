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
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="degree"/> is out of
        /// range for the <paramref name="scale"/>.</exception>
        public static NoteName GetDegree(this Scale scale, ScaleDegree degree)
        {
            ThrowIfArgument.IsNull(nameof(scale), scale);
            ThrowIfArgument.IsInvalidEnumValue(nameof(degree), degree);

            var degreeNumber = (int)degree;
            if (degreeNumber >= scale.Intervals.Count())
                throw new ArgumentOutOfRangeException(nameof(degree),
                                                      degree,
                                                      "Degree is out of range for the scale.");

            return scale.GetNotes()
                        .Skip(degreeNumber)
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

            int noteNumber = SevenBitNumber.Values
                                           .SkipWhile(number => NoteUtilities.GetNoteName(number) != scale.RootNote)
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

        /// <summary>
        /// Gets notes that belong to a musical scale in ascending order starting with the specified
        /// root note.
        /// </summary>
        /// <param name="scale"><see cref="Scale"/> to get notes of.</param>
        /// <param name="rootNote"><see cref="Note"/> to start a sequence of scale's notes with.</param>
        /// <returns>Notes that belong to the <paramref name="scale"/> in ascending order starting with
        /// the <paramref name="rootNote"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="scale"/> is null. -or-
        /// <paramref name="rootNote"/> is null.</exception>
        public static IEnumerable<Note> GetAscendingNotes(this Scale scale, Note rootNote)
        {
            ThrowIfArgument.IsNull(nameof(scale), scale);
            ThrowIfArgument.IsNull(nameof(rootNote), rootNote);

            return scale.GetNotes()
                        .SkipWhile(n => n != rootNote);
        }

        /// <summary>
        /// Gets notes that belong to a musical scale in descending order starting with the specified
        /// root note.
        /// </summary>
        /// <param name="scale"><see cref="Scale"/> to get notes of.</param>
        /// <param name="rootNote"><see cref="Note"/> to start a sequence of scale's notes with.</param>
        /// <returns>Notes that belong to the <paramref name="scale"/> in descending order starting with
        /// the <paramref name="rootNote"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="scale"/> is null. -or-
        /// <paramref name="rootNote"/> is null.</exception>
        public static IEnumerable<Note> GetDescendingNotes(this Scale scale, Note rootNote)
        {
            ThrowIfArgument.IsNull(nameof(scale), scale);
            ThrowIfArgument.IsNull(nameof(rootNote), rootNote);

            return new[] { rootNote }.Concat(scale.GetNotes()
                                                  .TakeWhile(n => n != rootNote)
                                                  .Reverse());
        }

        /// <summary>
        /// Checks if the specified note belongs to a scale or not.
        /// </summary>
        /// <param name="scale"><see cref="Scale"/> to check the note.</param>
        /// <param name="note"><see cref="Note"/> to check if it belongs to the <paramref name="scale"/>
        /// or not.</param>
        /// <returns>true if <paramref name="note"/> belongs to the <paramref name="scale"/>;
        /// otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="scale"/> is null. -or-
        /// <paramref name="note"/> is null.</exception>
        public static bool IsNoteInScale(this Scale scale, Note note)
        {
            ThrowIfArgument.IsNull(nameof(scale), scale);
            ThrowIfArgument.IsNull(nameof(note), note);

            return scale.GetNotes()
                        .Contains(note);
        }

        /// <summary>
        /// Gets a note that belongs to a musical scale next to the specified note.
        /// </summary>
        /// <param name="scale"><see cref="Scale"/> to get the next note of.</param>
        /// <param name="note"><see cref="Note"/> to get a note next to.</param>
        /// <returns>A note next to the <paramref name="note"/> that belongs to the
        /// <paramref name="scale"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="scale"/> is null. -or-
        /// <paramref name="note"/> is null.</exception>
        public static Note GetNextNote(this Scale scale, Note note)
        {
            ThrowIfArgument.IsNull(nameof(scale), scale);
            ThrowIfArgument.IsNull(nameof(note), note);

            return scale.GetAscendingNotes(note)
                        .Skip(1)
                        .FirstOrDefault();
        }

        /// <summary>
        /// Gets a note that belongs to a musical scale previous to the specified note.
        /// </summary>
        /// <param name="scale"><see cref="Scale"/> to get the previous note of.</param>
        /// <param name="note"><see cref="Note"/> to get a note previous to.</param>
        /// <returns>A note previous to the <paramref name="note"/> that belongs to the
        /// <paramref name="scale"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="scale"/> is null. -or-
        /// <paramref name="note"/> is null.</exception>
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
