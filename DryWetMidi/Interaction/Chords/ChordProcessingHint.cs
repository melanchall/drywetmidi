using System;

namespace Melanchall.DryWetMidi.Interaction
{
    [Flags]
    public enum ChordProcessingHint
    {
        TimeOrLengthCanBeChanged = 1,
        NotesCollectionCanBeChanged = 2,

        None = 0,
        Default = TimeOrLengthCanBeChanged,
    }
}
