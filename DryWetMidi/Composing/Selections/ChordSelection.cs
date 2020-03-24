namespace Melanchall.DryWetMidi.Composing
{
    /// <summary>
    /// Defines a predicate to select chords from pattern.
    /// </summary>
    /// <param name="noteIndex">The index of currently processing chord. Index is continuous so
    /// it will not be set to zero for sub-patterns.</param>
    /// <param name="chordDescriptor">The descriptor of a chord containing all required information
    /// about the chord.</param>
    /// <returns><c>true</c> if a chord should be selected and processed; otherwise, <c>false</c>.</returns>
    public delegate bool ChordSelection(int noteIndex, ChordDescriptor chordDescriptor);
}
