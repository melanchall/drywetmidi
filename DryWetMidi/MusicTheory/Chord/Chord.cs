using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    public sealed class Chord
    {
        #region Constructor

        public Chord(ICollection<NoteName> notesNames)
        {
            ThrowIfArgument.IsNull(nameof(notesNames), notesNames);
            ThrowIfArgument.ContainsInvalidEnumValue(nameof(notesNames), notesNames);
            ThrowIfArgument.IsEmptyCollection(nameof(notesNames), notesNames, "Notes names collection is empty.");

            NotesNames = notesNames;
        }

        public Chord(NoteName rootNoteName, params NoteName[] notesNamesAboveRoot)
            : this(new[] { rootNoteName }.Concat(notesNamesAboveRoot ?? Enumerable.Empty<NoteName>()).ToArray())
        {
        }

        public Chord(NoteName rootNoteName, IEnumerable<Interval> intervalsFromRoot)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(rootNoteName), rootNoteName);

            NotesNames = new[] { Interval.Zero }.Concat(intervalsFromRoot)
                .Where(i => i != null)
                .OrderBy(i => i.HalfSteps)
                .Select(i => rootNoteName.Transpose(i))
                .ToArray();
        }

        public Chord(NoteName rootNoteName, params Interval[] intervalsFromRoot)
            : this(rootNoteName, intervalsFromRoot as IEnumerable<Interval>)
        {
        }

        #endregion

        #region Properties

        public ICollection<NoteName> NotesNames { get; }

        public NoteName RootNoteName => NotesNames.First();

        #endregion

        #region Methods

        public IEnumerable<Chord> GetInversions()
        {
            var notesNames = new Queue<NoteName>(NotesNames);

            for (var i = 1; i < NotesNames.Count; i++)
            {
                var noteName = notesNames.Dequeue();
                notesNames.Enqueue(noteName);

                yield return new Chord(notesNames.ToArray());
            }
        }

        #endregion

        #region Operators

        public static bool operator ==(Chord chord1, Chord chord2)
        {
            if (ReferenceEquals(chord1, chord2))
                return true;

            if (ReferenceEquals(null, chord1) || ReferenceEquals(null, chord2))
                return false;

            return chord1.NotesNames.SequenceEqual(chord2.NotesNames);
        }

        public static bool operator !=(Chord chord1, Chord chord2)
        {
            return !(chord1 == chord2);
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return string.Join(" ", NotesNames.Select(n => n.ToString().Replace(Note.SharpLongString, Note.SharpShortString)));
        }

        public override bool Equals(object obj)
        {
            return this == (obj as Chord);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var result = 17;

                foreach (var note in NotesNames)
                {
                    result = result * 23 + note.GetHashCode();
                }

                return result;
            }
        }

        #endregion
    }
}
