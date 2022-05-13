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

## Tempo map

Tempo map is a set of changes of time signature and tempo. You need to have a tempo map to convert [time and length](Time-and-length-overview.md) between different representations. Instead of messing with [Time Signature](xref:Melanchall.DryWetMidi.Core.TimeSignatureEvent) and [Set Tempo](xref:Melanchall.DryWetMidi.Core.SetTempoEvent) events DryWetMIDI provides [TempoMapManager](xref:Melanchall.DryWetMidi.Interaction.TempoMapManager) which helps to manage tempo map of a MIDI file:

```csharp
using (var tempoMapManager = new TempoMapManager(midiFile.TimeDivision,
                                        midiFile.GetTrackChunks()
                                                .Select(c => c.Events)))
{
    TempoMap tempoMap = tempoMapManager.TempoMap;

    Tempo tempoAt123 = tempoMap.GetTempoAtTime((MidiTimeSpan)123);

    // Change tempo to 400000 microseconds per quarter note at 20 seconds from
    // MIDI file start
    tempoMapManager.SetTempo(new MetricTimeSpan(0, 0, 20), new Tempo(400000));

    tempoMapManager.ClearTimeSignature(456);
}
```

To get tempo map being managed by the current `TempoMapManager` you need to use [TempoMap](xref:Melanchall.DryWetMidi.Interaction.TempoMapManager.TempoMap) property which returns an instance of the [TempoMap](xref:Melanchall.DryWetMidi.Interaction.TempoMap) class.

Once you've got an instance of [TempoMap](xref:Melanchall.DryWetMidi.Interaction.TempoMap) you can use [GetTempoChanges](xref:Melanchall.DryWetMidi.Interaction.TempoMap.GetTempoChanges) method to get all tempo changes. Use [GetTimeSignatureChanges](xref:Melanchall.DryWetMidi.Interaction.TempoMap.GetTimeSignatureChanges) method to get time signature changes. [GetTempoAtTime](xref:Melanchall.DryWetMidi.Interaction.TempoMap.GetTempoAtTime(Melanchall.DryWetMidi.Interaction.ITimeSpan)) and [GetTimeSignatureAtTime](xref:Melanchall.DryWetMidi.Interaction.TempoMap.GetTimeSignatureAtTime(Melanchall.DryWetMidi.Interaction.ITimeSpan)) methods allow to get tempo and time signature at the specified time.

You can also create new tempo map with `TempoMapManager`:

```csharp
using (var tempoMapManager = new TempoMapManager())
{
    // ...
}
```

There is another way to get an instance of the `TempoMapManager` – through the [ManageTempoMap](xref:Melanchall.DryWetMidi.Interaction.TempoMapManagingUtilities.ManageTempoMap*) extension method:

```csharp
using (var tempoMapManager = midiFile.ManageTempoMap())
{
    // ...
}
```

This method and another useful ones are placed in [TempoMapManagingUtilities](xref:Melanchall.DryWetMidi.Interaction.TempoMapManagingUtilities). For example, to get tempo map of a MIDI file you can write:

```csharp
TempoMap tempoMap = midiFile.GetTempoMap();
```

Also you can replace the entire tempo map of a MIDI file using [ReplaceTempoMap](xref:Melanchall.DryWetMidi.Interaction.TempoMapManagingUtilities.ReplaceTempoMap*) method:

```csharp
midiFile.ReplaceTempoMap(TempoMap.Create(Tempo.FromBeatsPerMinute(140)));
```