Methods from `GetTimedEventsAndNotesUtilities` are now obsolete and you should use `GetObjects` methods from [GetObjectsUtilities](xref:Melanchall.DryWetMidi.Interaction.GetObjectsUtilities). Example how you can get timed events and notes:

```csharp
var timedEventsAndNotes = midiFile.GetObjects(ObjectType.TimedEvent | ObjectType.Note);
```