`TempoMap.Tempo` property has been replaced with more explicit and straightforward methods: [GetTempoChanges](xref:Melanchall.DryWetMidi.Interaction.TempoMap.GetTempoChanges) and [GetTempoAtTime](xref:Melanchall.DryWetMidi.Interaction.TempoMap.GetTempoAtTime(Melanchall.DryWetMidi.Interaction.ITimeSpan)):

```csharp
foreach (var tempoChange in tempoMap.GetTempoChanges())
{
    var time = tempoChange.Time;
    var timeSignature = tempoChange.Value;
}

// ...

tempoMap.GetTempoAtTime(MusicalTimeSpan.Whole);
```