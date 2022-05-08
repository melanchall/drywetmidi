`SetTimeAndLength` separate methods for [notes](xref:Melanchall.DryWetMidi.Interaction.Note) and [chords](xref:Melanchall.DryWetMidi.Interaction.Chord) have been replaced with generic [SetLength](xref:Melanchall.DryWetMidi.Interaction.LengthedObjectUtilities.SetLength*). Since [SetTime](xref:Melanchall.DryWetMidi.Interaction.TimedObjectUtilities.SetTime*) method also exists, the new way to set time and length with one instruction is:

```csharp
note
    .SetTime(new MetricTimeSpan(0, 0, 0, 500), tempoMap)
    .SetLength(new MetricTimeSpan(0, 0, 10), tempoMap);
```