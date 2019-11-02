using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tests.Utilities
{
    internal static class NoteEquality
    {
        #region Nested classes

        private sealed class NoteComparer : IEqualityComparer<Note>
        {
            #region IEqualityComparer<Note>

            public bool Equals(Note note1, Note note2)
            {
                if (ReferenceEquals(note1, note2))
                    return true;

                if (ReferenceEquals(null, note1) || ReferenceEquals(null, note2))
                    return false;

                return note1.NoteNumber == note2.NoteNumber &&
                       note1.Channel == note2.Channel &&
                       note1.Velocity == note2.Velocity &&
                       note1.OffVelocity == note2.OffVelocity &&
                       note1.Time == note2.Time &&
                       note1.Length == note2.Length;
            }

            public int GetHashCode(Note note)
            {
                return note.NoteNumber.GetHashCode() ^
                       note.Channel.GetHashCode() ^
                       note.Velocity.GetHashCode() ^
                       note.OffVelocity.GetHashCode() ^
                       note.Time.GetHashCode() ^
                       note.Length.GetHashCode();
            }

            #endregion
        }

        #endregion

        #region Methods

        public static bool AreEqual(Note note1, Note note2)
        {
            return new NoteComparer().Equals(note1, note2);
        }

        public static bool AreEqual(IEnumerable<Note> notes1, IEnumerable<Note> notes2)
        {
            if (ReferenceEquals(notes1, notes2))
                return true;

            if (ReferenceEquals(null, notes1) || ReferenceEquals(null, notes2))
                return false;

            return notes1.SequenceEqual(notes2, new NoteComparer());
        }

        #endregion
    }
}
