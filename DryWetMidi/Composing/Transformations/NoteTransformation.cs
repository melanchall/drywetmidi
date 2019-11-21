namespace Melanchall.DryWetMidi.Composing
{
    /// <summary>
    /// Transforms a note described by the specified <see cref="NoteDescriptor"/> and returns
    /// new <see cref="NoteDescriptor"/> that is result of transformation.
    /// </summary>
    /// <param name="noteDescriptor">Descriptor of a note to transform.</param>
    /// <returns><see cref="NoteDescriptor"/> that is result of transformation.</returns>
    public delegate NoteDescriptor NoteTransformation(NoteDescriptor noteDescriptor);
}
