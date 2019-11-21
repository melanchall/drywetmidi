namespace Melanchall.DryWetMidi.Composing
{
    /// <summary>
    /// Transforms a chord described by the specified <see cref="ChordDescriptor"/> and returns
    /// new <see cref="ChordDescriptor"/> that is result of transformation.
    /// </summary>
    /// <param name="chordDescriptor">Descriptor of a chord to transform.</param>
    /// <returns><see cref="ChordDescriptor"/> that is result of transformation.</returns>
    public delegate ChordDescriptor ChordTransformation(ChordDescriptor chordDescriptor);
}
