using System;

namespace Melanchall.DryWetMidi.Interaction
{
    [Flags]
    public enum NoteProcessingHint
    {
        TimeOrLengthCanBeChanged = 1,

        None = 0,
        Default = TimeOrLengthCanBeChanged,
    }
}
