Separate manager classes for each MIDI object type are replaced with [TimedObjectsManager](xref:Melanchall.DryWetMidi.Interaction.TimedObjectsManager) which can manage objects of different types simultaneously.

For example, to manage just [timed events](xref:Melanchall.DryWetMidi.Interaction.TimedEvent):

```csharp
using (var objectsManager = new TimedObjectsManager(trackChunk.Events, ObjectType.TimedEvent))
{
    var firstTimedEvent = (TimedEvent)objectsManager.Objects.FirstOrDefault();
}
```

or

```csharp
using (var objectsManager = new TimedObjectsManager<TimedEvent>(trackChunk.Events))
{
    var firstTimedEvent = objectsManager.Objects.FirstOrDefault();
}
```

To manage both [timed events](xref:Melanchall.DryWetMidi.Interaction.TimedEvent) and [notes](xref:Melanchall.DryWetMidi.Interaction.Note):

```csharp
using (var objectsManager = new TimedObjectsManager(trackChunk.Events, ObjectType.TimedEvent | ObjectType.Note))
{
    var firstObject = objectsManager.Objects.FirstOrDefault();
    if (firstObject is Note note)
    {
        // do smth with note
    }
    else
    {
        // do smth with timed event
    }
}
```