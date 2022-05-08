Separate splitter classes for notes and chords have been replaced by [Splitter](xref:Melanchall.DryWetMidi.Tools.Splitter) class which can split objects of different types simultaneously.

For example, to split [notes](xref:Melanchall.DryWetMidi.Interaction.Note) and [chords](xref:Melanchall.DryWetMidi.Interaction.Chord) into `4` parts:

```csharp
midiFile.SplitObjectsByPartsNumber(
    ObjectType.Note | ObjectType.Chord,
    4,
    TimeSpanType.Metric);
```