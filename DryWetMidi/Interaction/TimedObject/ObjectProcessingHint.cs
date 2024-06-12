using System;

namespace Melanchall.DryWetMidi.Interaction
{
    [Flags]
    public enum ObjectProcessingHint
    {
        None = 0,

        TimeOrLengthCanBeChanged = 1,
        NotesCollectionCanBeChanged = 2,
        NoteTimeOrLengthCanBeChanged = 4,

        Default = TimeOrLengthCanBeChanged,
    }
}
