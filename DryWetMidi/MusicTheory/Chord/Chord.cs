using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    public sealed class Chord
    {
        #region Constructor

        public Chord(IEnumerable<NoteName> notes)
        {
            ThrowIfArgument.IsNull(nameof(notes), notes);
            ThrowIfArgument.ContainsInvalidEnumValue(nameof(notes), notes);
            ThrowIfArgument.IsEmptyCollection(nameof(notes), notes, "Notes collection is empty.");

            Notes = notes;
        }

        public Chord(params NoteName[] notes)
            : this(notes as IEnumerable<NoteName>)
        {
        }

        #endregion

        #region Properties

        public IEnumerable<NoteName> Notes { get; }

        #endregion

        #region Operators

        public static bool operator ==(Chord chord1, Chord chord2)
        {
            if (ReferenceEquals(chord1, chord2))
                return true;

            if (ReferenceEquals(null, chord1) || ReferenceEquals(null, chord2))
                return false;

            return chord1.Notes.SequenceEqual(chord2.Notes);
        }

        public static bool operator !=(Chord chord1, Chord chord2)
        {
            return !(chord1 == chord2);
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return string.Join(" ", Notes.Select(n => n.ToString().Replace(Note.SharpLongString, Note.SharpShortString)));
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

                foreach (var note in Notes)
                {
                    result = result * 23 + note.GetHashCode();
                }

                return result;
            }
        }

        #endregion
    }
}
