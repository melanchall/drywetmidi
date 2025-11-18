---
uid: a_processing_objects
---

# Processing objects

> [!WARNING]
> Please read the [Getting objects](xref:a_getting_objects) article before reading this one.

This article describes ways to process different objects (like [timed events](xref:Melanchall.DryWetMidi.Interaction.TimedEvent) or [notes](xref:Melanchall.DryWetMidi.Interaction.Note)) within MIDI files and track chunks. Processing means changing properties of the objects, including time or length.

## ProcessTimedEvents

Let's dive into the [ProcessTimedEvents](xref:Melanchall.DryWetMidi.Interaction.TimedEventsManagingUtilities.ProcessTimedEvents*) methods immediately. Well, we have such a file:

```csharp
var midiFile = new MidiFile(
    new TrackChunk(
        new TextEvent("A"),
        new NoteOnEvent(),
        new NoteOffEvent()),
    new TrackChunk(
        new TextEvent("B")));
```

And now we want to change the text of all _Text_ events to `"C"`:

```csharp
midiFile.ProcessTimedEvents(
    e => ((TextEvent)e.Event).Text = "C",
    e => e.Event.EventType == MidiEventType.Text);
```

After this instruction executed, the file will be equal to this one:

```csharp
var midiFile = new MidiFile(
    new TrackChunk(
        new TextEvent("C"),
        new NoteOnEvent(),
        new NoteOffEvent()),
    new TrackChunk(
        new TextEvent("C")));
```

So the first argument (`e => ((TextEvent)e.Event).Text = "C"`) is what we want to do and the second one (`e => e.Event.EventType == MidiEventType.Text`) defines what objects to process. We can also use the overload without predicate to process all timed events:

```csharp
midiFile.ProcessTimedEvents(e => e.Time += 10);
```

So the first event in each track chunk will have [DeltaTime](xref:Melanchall.DryWetMidi.Core.MidiEvent.DeltaTime) of `10` now so the absolute time of each event increases by `10`:

```csharp
var midiFile = new MidiFile(
    new TrackChunk(
        new TextEvent("C") { DeltaTime = 10 },
        new NoteOnEvent(),
        new NoteOffEvent()),
    new TrackChunk(
        new TextEvent("C") { DeltaTime = 10 }));
```

Please examine the [TimedEventsManagingUtilities](xref:Melanchall.DryWetMidi.Interaction.TimedEventsManagingUtilities) class to see other `ProcessTimedEvents` overloads.

By the way, what if we want to process only events with even indices? First of all we need to define subclass of the [TimedEvent](xref:Melanchall.DryWetMidi.Interaction.TimedEvent):

```csharp
private sealed class CustomTimedEvent : TimedEvent
{
    public CustomTimedEvent(MidiEvent midiEvent, long time, int eventIndex)
        : base(midiEvent, time)
    {
        EventIndex = eventIndex;
    }

    public int EventIndex { get; }
}
```

And now we are ready to increase the time of even-indexed events by `10` ticks:

```csharp
midiFile.ProcessTimedEvents(
    e => e.Time += 10,
    e => ((CustomTimedEvent)e).EventIndex % 2 == 0,
    new TimedEventDetectionSettings
    {
        Constructor = data => new CustomTimedEvent(data.Event, data.Time, data.EventIndex)
    });
```

New file will be:

```csharp
var midiFile = new MidiFile(
    new TrackChunk(
        new NoteOnEvent(),
        new TextEvent("C") { DeltaTime = 10 },
        new NoteOffEvent()),
    new TrackChunk(
        new TextEvent("C") { DeltaTime = 10 }));
```

Looks right. In the first track chunk _Text_ and _Note Off_ events are even-indexed ones (`0` and `2`), so the time of each one will be `10`. _Note On_ event will be left untouched so its time remains `0`. As for the second track chunk, the only event is even-indexed of course and thus is processed.

Optionally you can pass [TimedEventProcessingHint](xref:Melanchall.DryWetMidi.Interaction.TimedEventProcessingHint) to set a hint for the processing engine which can improve performance. Please read about available options by the specified link. For example, if you know that time of events won't be changed, you can set [TimedEventProcessingHint.None](xref:Melanchall.DryWetMidi.Interaction.TimedEventProcessingHint.None) option to eliminate events resorting (which is required if times can be changed) and to reduce the time needed to perform the processing:

```csharp
midiFile.ProcessTimedEvents(
    e => ((TextEvent)e.Event).Text = "C",
    e => e.Event.EventType == MidiEventType.Text,
    hint: TimedEventProcessingHint.None);
```

All [ProcessTimedEvents](xref:Melanchall.DryWetMidi.Interaction.TimedEventsManagingUtilities.ProcessTimedEvents*) methods return count of processed objects.

## ProcessNotes

Use [ProcessNotes](xref:Melanchall.DryWetMidi.Interaction.NotesManagingUtilities.ProcessNotes*) methods to process notes. There is nothing tricky here and the approach is the same as for timed events processing (see above): you just work with notes instead of timed events.

Please read [GetNotes: Settings](xref:a_getting_objects#settings) article to learn more about how to customize notes detection and building. Be aware that note is in fact a pair of _Note On_ / _Note Off_ events. So to have full control on the notes construction process, [ProcessNotes](xref:Melanchall.DryWetMidi.Interaction.NotesManagingUtilities.ProcessNotes*) methods accept [TimedEventDetectionSettings](xref:Melanchall.DryWetMidi.Interaction.TimedEventDetectionSettings) along with [NoteDetectionSettings](xref:Melanchall.DryWetMidi.Interaction.NoteDetectionSettings).

As with timed events you can create subclass of the [Note](xref:Melanchall.DryWetMidi.Interaction.Note) class and use it to customize the matching logic:

```csharp
private sealed class CustomNote : Note
{
    public CustomNote(TimedEvent timedNoteOnEvent, TimedEvent timedNoteOffEvent, bool isNoteOnEventUnevenIndexed)
        : base(timedNoteOnEvent, timedNoteOffEvent)
    {
        IsNoteOnEventUnevenIndexed = isNoteOnEventUnevenIndexed;
    }

    public bool IsNoteOnEventUnevenIndexed { get; }
}

// ...

midiFile.ProcessNotes(
    n => n.Velocity += (SevenBitNumber)10,
    n => ((CustomNote)n).IsNoteOnEventUnevenIndexed,
    new NoteDetectionSettings
    {
        Constructor = data => new CustomNote(
            data.TimedNoteOnEvent,
            data.TimedNoteOffEvent,
            ((CustomTimedEvent)data.TimedNoteOnEvent).EventIndex % 2 != 0)
    },
    new TimedEventDetectionSettings
    {
        Constructor = data => new CustomTimedEvent(
            data.Event,
            data.Time,
            data.EventIndex)
    });
```

So the velocity of a note will be increased by `10` if the note's _Note On_ event is uneven-indexed.

And of course you can manage performance with notes processing too via [NoteProcessingHint](xref:Melanchall.DryWetMidi.Interaction.NoteProcessingHint). By default it's set to the value allowing you to change time and length. But you can reduce execution time via the [NoteProcessingHint.None](xref:Melanchall.DryWetMidi.Interaction.NoteProcessingHint.None) flag. With this option any changes of time or length of a note won't be saved.

## ProcessChords

Obviously you can process chords too – [ProcessChords](xref:Melanchall.DryWetMidi.Interaction.ChordsManagingUtilities.ProcessChords*) is what you need. It's redundant to describe how to use these methods since usage is the same as for [timed events](#processtimedevents) and [notes](#processnotes).

A chord is in fact a collection of notes, each one is a pair of _Note On_ / _Note Off_ events (please read about [notes processing](#processnotes) above). So to have full control on the chord construction process, [ProcessChords](xref:Melanchall.DryWetMidi.Interaction.ChordsManagingUtilities.ProcessChords*) methods accept [TimedEventDetectionSettings](xref:Melanchall.DryWetMidi.Interaction.TimedEventDetectionSettings) and [NoteDetectionSettings](xref:Melanchall.DryWetMidi.Interaction.NoteDetectionSettings) along with [ChordDetectionSettings](xref:Melanchall.DryWetMidi.Interaction.ChordDetectionSettings).

As with timed events and notes you can create a subclass of the [Chord](xref:Melanchall.DryWetMidi.Interaction.Chord) class and use it to customize the matching logic.

But for chords processing there are two additional options to control performance of the methods. These flags from [ChordProcessingHint](xref:Melanchall.DryWetMidi.Interaction.ChordProcessingHint) are:

* [NoteTimeOrLengthCanBeChanged](xref:Melanchall.DryWetMidi.Interaction.ChordProcessingHint.NoteTimeOrLengthCanBeChanged): time or length of a note within a chord's notes can be changed;
* [NotesCollectionCanBeChanged](xref:Melanchall.DryWetMidi.Interaction.ChordProcessingHint.NotesCollectionCanBeChanged): chord's notes collection can be changed, for example, a note can be added or removed.

By default those two options are not enabled. So if you want to say the engine that all chord's properties can be changed:

```csharp
midiFile.ProcessChords(
    c => c.Velocity += (SevenBitNumber)10,
    hint: ChordProcessingHint.AllPropertiesCanBeChanged);
```

or if you want more precise control:

```csharp
midiFile.ProcessChords(
    c => c.Velocity += (SevenBitNumber)10,
    hint: ChordProcessingHint.TimeOrLengthCanBeChanged | ChordProcessingHint.NoteTimeOrLengthCanBeChanged);
```

## ProcessObjects

All methods we saw before process objects of the **same type**. So you can process only either notes or chords or timed events. But there is a way to process objects of different types at the same time – [ProcessObjects](xref:Melanchall.DryWetMidi.Interaction.TimedObjectUtilities.ProcessObjects*). For example, if you want to increase velocity of notes that are not a part of some chord:

```csharp
midiFile.ProcessObjects(
    ObjectType.Note | ObjectType.Chord,
    obj => ((Note)obj).Velocity += (SevenBitNumber)10,
    obj => obj is Note);
```

Or we can increase velocity based on the type of an object:

```csharp
midiFile.ProcessObjects(
    ObjectType.Note | ObjectType.Chord,
    obj =>
    {
        if (obj is Note note)
            note.Velocity += (SevenBitNumber)10;
        else
            ((Chord)obj).Velocity += (SevenBitNumber)20;
    });
```

Performance of the [ProcessObjects](xref:Melanchall.DryWetMidi.Interaction.TimedObjectUtilities.ProcessObjects*) methods can be managed too – via passing [ObjectProcessingHint](xref:Melanchall.DryWetMidi.Interaction.ObjectProcessingHint). Possible values and how they affect the processing is the subject already described in previous sections.

If you need to process objects of several types simultaneously, [ProcessObjects](xref:Melanchall.DryWetMidi.Interaction.TimedObjectUtilities.ProcessObjects*) will be much faster than consecutive calls of methods for specific object types. I.e. this instruction

```csharp
midiFile.ProcessObjects(
    ObjectType.Note | ObjectType.Chord,
    /*...*/);
```

will execute faster than

```csharp
midiFile.ProcessChords(/*...*/);
midiFile.ProcessNotes(/*...*/);
```

More than that, some cases can be covered with [ProcessObjects](xref:Melanchall.DryWetMidi.Interaction.TimedObjectUtilities.ProcessObjects*) only like the example described above – _increase velocity of notes that are not a part of some chord_. Of course you can always use [objects managers](xref:a_managers) to perform any transformations you want, but processing objects in that way is much slower.