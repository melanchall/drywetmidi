---
uid: a_removing_objects
---

# Removing objects

> [!WARNING]
> Please read [Getting objects](xref:a_getting_objects) and [Processing objects](xref:a_processing_objects) articles before reading this one.

This article describes ways to remove different objects (like [timed events](xref:Melanchall.DryWetMidi.Interaction.TimedEvent) or [notes](xref:Melanchall.DryWetMidi.Interaction.Note)) from MIDI files and track chunks.

## RemoveTimedEvents

[RemoveTimedEvents](xref:Melanchall.DryWetMidi.Interaction.TimedEventsManagingUtilities.RemoveTimedEvents*) methods is what you need to remove MIDI events by the specified conditions. For example, we have such a file:

```csharp
var midiFile = new MidiFile(
    new TrackChunk(
        new TextEvent("A") { DeltaTime = 10 },
        new NoteOnEvent(),
        new NoteOffEvent()),
    new TrackChunk(
        new TextEvent("B")));
```

And we want to remove all _Text_ events:

```csharp
midiFile.RemoveTimedEvents(
    e => e.Event.EventType == MidiEventType.Text);
```

After this instruction executed, the file will be equal to this one:

```csharp
var midiFile = new MidiFile(
    new TrackChunk(
        new NoteOnEvent() { DeltaTime = 10 },
        new NoteOffEvent()),
    new TrackChunk());
```

So `e => e.Event.EventType == MidiEventType.Text` defines what objects to remove. Of course, absolute times of all events will be preserved as you can see. We can also use the overload without predicate to remove all timed events:

```csharp
midiFile.RemoveTimedEvents();
```

Please examine the [TimedEventsManagingUtilities](xref:Melanchall.DryWetMidi.Interaction.TimedEventsManagingUtilities) class to see other `RemoveTimedEvents` overloads.

As you've learned from the [Processing objects: ProcessTimedEvents](xref:a_processing_objects#processtimedevents) article, you can instantiate timed events as custom classes derived from [TimedEvent](xref:Melanchall.DryWetMidi.Interaction.TimedEvent) and use those instances within a predicate to select objects. This article won't repeat information from there so please read it.

All [RemoveTimedEvents](xref:Melanchall.DryWetMidi.Interaction.TimedEventsManagingUtilities.RemoveTimedEvents*) methods return count of removed objects.

## RemoveNotes

Use [RemoveNotes](xref:Melanchall.DryWetMidi.Interaction.NotesManagingUtilities.RemoveNotes*) methods to remove notes. The approach is the same as for timed events removing (see above): you just work with notes instead of timed events.

Please read [Processing objects: ProcessNotes](xref:a_processing_objects#processnotes) article to learn about how to customize objects selection.

## RemoveChords

Obviously you can remove chords too – [RemoveChords](xref:Melanchall.DryWetMidi.Interaction.ChordsManagingUtilities.RemoveChords*) is what you need. It's redundant to describe how to use these methods since usage is the same as for [timed events](#removetimedevents) and [notes](#removenotes). Also please read [Processing objects: ProcessChords](xref:a_processing_objects#processchords) article.

## RemoveObjects

All methods we saw before remove objects of the **same type**. So you can remove only either notes or chords or timed events. But there is a way to remove objects of different types at the same time – [RemoveObjects](xref:Melanchall.DryWetMidi.Interaction.TimedObjectUtilities.RemoveObjects*). For example, if you want to remove notes that are not a part of some chord:

```csharp
midiFile.RemoveObjects(
    ObjectType.Note | ObjectType.Chord,
    obj => obj is Note);
```

Or we can remove objects based on the type of an object:

```csharp
midiFile.RemoveObjects(
    ObjectType.Note | ObjectType.Chord,
    obj => obj is Note note
        ? note.Velocity > 10
        : ((Chord)obj).Velocity > 20);
```


If you need to remove objects of several types simultaneously, [RemoveObjects](xref:Melanchall.DryWetMidi.Interaction.TimedObjectUtilities.RemoveObjects*) will be much faster than consecutive calls of methods for specific object types. I.e. this instruction

```csharp
midiFile.RemoveObjects(
    ObjectType.Note | ObjectType.Chord,
    /*...*/);
```

will execute faster than

```csharp
midiFile.RemoveChords(/*...*/);
midiFile.RemoveNotes(/*...*/);
```

More than that, some cases can be covered with [RemoveObjects](xref:Melanchall.DryWetMidi.Interaction.TimedObjectUtilities.RemoveObjects*) only like the example described above – _remove notes that are not a part of some chord_. Of course you can always use [objects managers](xref:a_managers) to remove objects with any desired logic, but that way is much slower.