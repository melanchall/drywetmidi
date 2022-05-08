`TimedEventsManagingUtilities.SetTime` method has been replaced with generic [TimedObjectUtilities.SetTime](xref:Melanchall.DryWetMidi.Interaction.TimedObjectUtilities.SetTime*) allowing to set time for objects of different types, not for only [timed events](xref:Melanchall.DryWetMidi.Interaction.TimedEvent).

Small example:

```csharp
timedEvent.SetTime(new BarBeatTicksTimeSpan(2), tempoMap);
note.SetTime(MusicalTimeSpan.Half.SingleDotted(), tempoMap);
chord.SetTime(new MetricTimeSpan(0, 1, 10), tempoMap);
```