using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    public sealed class Chord
    {
        #region Constructor

        public Chord(IEnumerable<NoteName> notesNames)
        {
            ThrowIfArgument.IsNull(nameof(notesNames), notesNames);
            ThrowIfArgument.ContainsInvalidEnumValue(nameof(notesNames), notesNames);
            ThrowIfArgument.IsEmptyCollection(nameof(notesNames), notesNames, "Notes names collection is empty.");

            NotesNames = notesNames;
        }

        public Chord(params NoteName[] notesNames)
            : this(notesNames as IEnumerable<NoteName>)
        {
        }

        #endregion

        #region Properties

        public IEnumerable<NoteName> NotesNames { get; }

        public NoteName RootNoteName => NotesNames.First();

        #endregion

        #region Methods

        public IEnumerable<Interval> GetIntervalsFromRootNote()
        {
            var lastNoteNumber = (int)NotesNames.First();
            var lastInterval = SevenBitNumber.MinValue;

            var result = new List<Interval>();

            foreach (var noteName in NotesNames.Skip(1))
            {
                var offset = (int)noteName - lastNoteNumber;
                if (offset <= 0)
                    offset += Octave.OctaveSize;

                if (lastInterval + (SevenBitNumber)offset > SevenBitNumber.MaxValue)
                    throw new InvalidOperationException($"Some interval(s) are greater than {SevenBitNumber.MaxValue}.");

                lastInterval += (SevenBitNumber)offset;
                result.Add(Interval.GetUp(lastInterval));
                lastNoteNumber = (int)noteName;
            }

            return result;
        }

        public Note ResolveRootNote(Octave octave)
        {
            ThrowIfArgument.IsNull(nameof(octave), octave);

            return octave.GetNote(RootNoteName);
        }

        public IEnumerable<Note> ResolveNotes(Octave octave)
        {
            ThrowIfArgument.IsNull(nameof(octave), octave);

            var rootNote = ResolveRootNote(octave);
            var result = new List<Note> { rootNote };
            result.AddRange(GetIntervalsFromRootNote().Select(i => rootNote + i));
            return result;
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
