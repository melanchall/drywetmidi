using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    public sealed class Chord
    {
        #region Constructor

        public Chord(params NoteName[] notes)
            : this(notes as IEnumerable<NoteName>)
        {

        }

        public Chord(IEnumerable<NoteName> notes)
        {
            ThrowIfArgument.IsNull(nameof(notes), notes);

            Notes = notes;
        }

        #endregion

        #region Properties

        public IEnumerable<NoteName> Notes { get; }

        #endregion

        #region Operators

        /// <summary>
        /// Determines if two <see cref="Chord"/> objects are equal.
        /// </summary>
        /// <param name="chord1">The first <see cref="Chord"/> to compare.</param>
        /// <param name="chord2">The second <see cref="Chord"/> to compare.</param>
        /// <returns>true if the notes are equal, false otherwise.</returns>
        public static bool operator ==(Chord chord1, Chord chord2)
        {
            if (ReferenceEquals(chord1, chord2))
                return true;

            if (ReferenceEquals(null, chord1) || ReferenceEquals(null, chord2))
                return false;

            return chord1.Notes.SequenceEqual(chord2.Notes);
        }

        /// <summary>
        /// Determines if two <see cref="Chord"/> objects are not equal.
        /// </summary>
        /// <param name="chord1">The first <see cref="Note"/> to compare.</param>
        /// <param name="chord2">The second <see cref="Note"/> to compare.</param>
        /// <returns>false if the notes are equal, true otherwise.</returns>
        public static bool operator !=(Chord chord1, Chord chord2)
        {
            return !(chord1 == chord2);
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return string.Join(" ", Notes);
        }

        public override bool Equals(object obj)
        {
            return this == (obj as Chord);
        }

        public override int GetHashCode()
        {
            // TODO: calculate appropriate hash code
            return Notes.GetHashCode();
        }

        #endregion
    }
}
