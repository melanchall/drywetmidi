`TempoMap.TimeSignature` property has been replaced with more explicit and straightforward methods: [GetTimeSignatureChanges](xref:Melanchall.DryWetMidi.Interaction.TempoMap.GetTimeSignatureChanges) and [GetTimeSignatureAtTime](xref:Melanchall.DryWetMidi.Interaction.TempoMap.GetTimeSignatureAtTime(Melanchall.DryWetMidi.Interaction.ITimeSpan)):

```csharp
foreach (var timeSignatureChange in tempoMap.GetTimeSignatureChanges())
{
    var time = timeSignatureChange.Time;
    var timeSignature = timeSignatureChange.Value;
}

// ...

tempoMap.GetTimeSignatureAtTime(MusicalTimeSpan.Whole);
```