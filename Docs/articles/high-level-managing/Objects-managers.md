---
uid: a_managers
---

# Objects managers

Working with low-level objects (like MIDI event) sometimes is not convenient. In this cases it's much more handy to manage MIDI data via concepts of [timed events](xref:Melanchall.DryWetMidi.Interaction.TimedEvent), [notes](xref:Melanchall.DryWetMidi.Interaction.Note) or [chords](xref:Melanchall.DryWetMidi.Interaction.Chord).

DryWetMIDI provides a way to work with such high-level objects - [TimedObjectsManager](xref:Melanchall.DryWetMidi.Interaction.TimedObjectsManager). This class allows to work with objects of different types within an events collection (see [TrackChunk.Events](xref:Melanchall.DryWetMidi.Core.TrackChunk.Events)):

```csharp
using (var manager = new TimedObjectsManager(trackChunk.Events, ObjectType.Note | ObjectType.TimedEvent))
{
    foreach (var obj in manager.Objects)
    {
        if (obj is Note note)
            note.Length -= 10;
        else if (obj is TimedEvent timedEvent && timedEvent.Event is BaseTextEvent textEvent)
            textEvent.Text = "Hello";
    }

    manager.Objects.RemoveAll(obj => obj is Note note && note.Channel == 9);

    manager.Objects.Add(new TimedEvent(new ProgramChangeEvent((SevenBitNumber)7), 100));
}
```

All changes made with a manager will not be saved until `SaveChanges` or `Dispose` method will be called. So the recommended practice to work with managers is

```csharp
using (var notesManager = trackChunk.ManageNotes())
{
    // ...
}
```

or if managing is happen in different parts of  a program

```csharp
var notesManager = new TimedObjectsManager<Note>(trackChunk.Events);
// ...
notesManager.SaveChanges();
```

Objects will be placed in the underlying events collection in chronological order of course. Also as you can see there is the generic constructor that allows to manage objects of the single type. `Objects` property will return in this case objects of this type, no need to cast them to the type.

## Simultaneous editing of events collection

On saving the manager will rewrite all content of the underlying events collection. So you shouldn't modify the collection while working with the manager since all unsaved changes will be lost on manager's `SaveChanges` or `Dispose`.

For example, this code

```csharp
using (var timedEventsManager = new TimedObjectsManager<TimedEvent>(trackChunk.Events))
using (var notesManager = new TimedObjectsManager<Note>(trackChunk.Events))
{
    // All changes made with the notesManager will be lost
}
```

or

```csharp
var timedEventsManager = new TimedObjectsManager<TimedEvent>(trackChunk.Events);

var notesManager = new TimedObjectsManager<Note>(trackChunk.Events);

// All changes made with the notesManager will be lost

timedEventsManager.SaveChanges();
```

will cause changes made with the `notesManager` will be lost because `SaveChanges` (or `Dispose` in first code snippet) of `timedEventsManager` executed after `SaveChanges` of `notesManager`, and thus rewrites underlying events collection. You need to save changes made with a previous manager before managing objects with next one.