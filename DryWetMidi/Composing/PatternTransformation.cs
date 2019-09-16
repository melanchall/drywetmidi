using System.Linq;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Composing
{
    public static class PatternTransformation
    {
        #region Methods

        public static Pattern TransformNotes(this Pattern pattern, NoteTransformation noteTransformation)
        {
            ThrowIfArgument.IsNull(nameof(pattern), pattern);
            ThrowIfArgument.IsNull(nameof(noteTransformation), noteTransformation);

            return new Pattern(pattern.Actions.Select(a =>
            {
                var addNoteAction = a as AddNoteAction;
                if (addNoteAction != null)
                {
                    var noteDescriptor = noteTransformation(addNoteAction.NoteDescriptor);
                    return new AddNoteAction(noteDescriptor);
                }

                var addPatternAction = a as AddPatternAction;
                if (addPatternAction != null)
                    return new AddPatternAction(addPatternAction.Pattern.TransformNotes(noteTransformation));

                return a;
            })
            .ToList());
        }

        public static Pattern TransformChords(this Pattern pattern, ChordTransformation chordTransformation)
        {
            ThrowIfArgument.IsNull(nameof(pattern), pattern);
            ThrowIfArgument.IsNull(nameof(chordTransformation), chordTransformation);

            return new Pattern(pattern.Actions.Select(a =>
            {
                var addChordAction = a as AddChordAction;
                if (addChordAction != null)
                {
                    var chordDescriptor = chordTransformation(addChordAction.ChordDescriptor);
                    return new AddChordAction(chordDescriptor);
                }

                var addPatternAction = a as AddPatternAction;
                if (addPatternAction != null)
                    return new AddPatternAction(addPatternAction.Pattern.TransformChords(chordTransformation));

                return a;
            })
            .ToList());
        }

        #endregion
    }
}
