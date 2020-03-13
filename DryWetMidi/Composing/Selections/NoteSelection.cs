namespace Melanchall.DryWetMidi.Composing
{
    /// <summary>
    /// Defines a predicate to select notes from pattern.
    /// </summary>
    /// <param name="noteIndex">The index of currently processing note. Index is continuous so
    /// it will not be set to zero for sub-patterns.</param>
    /// <param name="noteDescriptor">The descriptor of a note containing all required information
    /// about the note.</param>
    /// <returns>true if a note should be selected and processed; otherwise, false.</returns>
    public delegate bool NoteSelection(int noteIndex, NoteDescriptor noteDescriptor);
}
