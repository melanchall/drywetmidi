DryWetMIDI provides set of classes that help manage MIDI objects:

* [TimedEventsManager](Events-absolute-time.md) to manage MIDI events by absolute time;
* [NotesManager](Notes.md) to manage notes of a MIDI file;
* [ChordsManager](Chords.md) to manage chords of a MIDI file;
* [TempoMapManager](Tempo-map.md) to manage tempo map of a MIDI file.

There are some importants things you should remember when working with all these events managers.

### Saving changes

All changes made with a manager will not be saved until `SaveChanges` or `Dispose` method will be called. So the recommended practice to work with managers is

```csharp
using (var notesManager = trackChunk.ManageNotes())
{
    // ...
}
```

or

```csharp
var notesManager = trackChunk.ManageNotes();
// ...
notesManager.SaveChanges();
```

### Simultaneous editing of `EventsCollection`

On saving the manager will rewrite all content of the underlying events collection. So you shouldn't modify `EventsCollection` of a `TrackChunk` and any single event while managing events with the manager since all unsaved changes will be lost on manager's `SaveChanges` or `Dispose`.

Using this code

```csharp
using (var timedEventsManager = trackChunk.ManageTimedEvents())
using (var notesManager = trackChunk.ManageNotes())
{
    // All changes made with the notesManager will be lost
}
```

or

```csharp
var timedEventsManager = trackChunk.ManageTimedEvents();

var notesManager = trackChunk.ManageNotes();

// All changes made with the notesManager will be lost

timedEventsManager.SaveChanges();
```

will cause changes made with the instance of the `NotesManager` will be lost bacause `SaveChanges` of `timedEventsManager` executed after `SaveChanges` of `notesManager` and thus rewrites underlying events collection. You need to save changes made with a previous manager before managing events with the new one.