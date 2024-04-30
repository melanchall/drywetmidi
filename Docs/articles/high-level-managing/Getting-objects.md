---
uid: a_getting_objects
---

# Getting objects

This article describes ways to get different objects (like [timed events](xref:Melanchall.DryWetMidi.Interaction.TimedEvent) or [notes](xref:Melanchall.DryWetMidi.Interaction.Note)) from MIDI files, track chunks and collections of another objects.

## GetTimedEvents

`TimedEvent` is the basic MIDI object we will describe here. It's just a MIDI event along with its absolute time within a MIDI file or track chunk. To get all timed events in a MIDI file, you can just call [GetTimedEvents](xref:Melanchall.DryWetMidi.Interaction.TimedEventsManagingUtilities.GetTimedEvents*) method:

```csharp
using System;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace DwmExamples
{
    class Program
    {
        static void Main(string[] args)
        {
            var midiFile = MidiFile.Read("My Great Song.mid");
            var timedEvents = midiFile.GetTimedEvents();

            Console.WriteLine($"{timedEvents.Count} timed events found.");
        }
    }
}
```

Please examine [TimedEventsManagingUtilities](xref:Melanchall.DryWetMidi.Interaction.TimedEventsManagingUtilities) class to see other `GetTimedEvents` overloads.

## GetNotes

There is the [NotesManagingUtilities](xref:Melanchall.DryWetMidi.Interaction.NotesManagingUtilities) class which provides useful methods `GetNotes` to get notes from a MIDI file or track chunk. For example, you can get notes a MIDI file contains with this code:

```csharp
using System;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace DwmExamples
{
    class Program
    {
        static void Main(string[] args)
        {
            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)50)
                    {
                        Channel = (FourBitNumber)5,
                        DeltaTime = 10
                    },
                    new NoteOffEvent((SevenBitNumber)70, (SevenBitNumber)30)
                    {
                        Channel = (FourBitNumber)5,
                        DeltaTime = 70
                    }));

            Console.WriteLine("Notes:");

            foreach (var note in midiFile.GetNotes())
            {
                Console.Write($@"
note {note} (note number = {note.NoteNumber})
  time = {note.Time}
  length = {note.Length}
  velocity = {note.Velocity}
  off velocity = {note.OffVelocity}");
            }

            Console.ReadKey();
        }
    }
}
```

Running the program, we'll see following output:

```text
Notes:

note C-1 (note number = 0)
  time = 0
  length = 0
  velocity = 0
  off velocity = 0
note A#4 (note number = 70)
  time = 10
  length = 70
  velocity = 50
  off velocity = 30
```

Please examine [NotesManagingUtilities](xref:Melanchall.DryWetMidi.Interaction.NotesManagingUtilities) class to see other `GetNotes` overloads.

### Settings

All `GetNotes` overloads can accept [NoteDetectionSettings](xref:Melanchall.DryWetMidi.Interaction.NoteDetectionSettings) as last parameter. Via this parameter you can adjust the process of notes building. Let's see each setting in details.

#### `NoteStartDetectionPolicy`

The [NoteStartDetectionPolicy](xref:Melanchall.DryWetMidi.Interaction.NoteDetectionSettings.NoteStartDetectionPolicy) property defines how start event of a note should be found in case of overlapping notes with the same note number and channel. The default value is [NoteStartDetectionPolicy.FirstNoteOn](xref:Melanchall.DryWetMidi.Interaction.NoteStartDetectionPolicy.FirstNoteOn).

To understand how this policy works let's take a look at the following events sequence:

![NoteStartDetectionPolicy-Initial](images/Getting-objects-NoteStartDetectionPolicy-Initial.png)

where empty circle and filled one mean _Note On_ and _Note Off_ events correspondingly; cross means any other event. So we have two overlapped notes here (we assume all note events have the same note number and channel).

If we set `NoteStartDetectionPolicy` to [NoteStartDetectionPolicy.FirstNoteOn](xref:Melanchall.DryWetMidi.Interaction.NoteStartDetectionPolicy.FirstNoteOn), notes will be constructed in following way:

![NoteStartDetectionPolicy-FirstNoteOn](images/Getting-objects-NoteStartDetectionPolicy-FirstNoteOn.png)

So every _Note Off_ event will be combined with **first** free _Note On_ event into a note (events are processed one by one consecutively). But if set `NoteStartDetectionPolicy` to [NoteStartDetectionPolicy.LastNoteOn](xref:Melanchall.DryWetMidi.Interaction.NoteStartDetectionPolicy.LastNoteOn), we'll get another picture:

![NoteStartDetectionPolicy-LastNoteOn](images/Getting-objects-NoteStartDetectionPolicy-LastNoteOn.png)

So _Note Off_ events will be combined with **last** free _Note On_ event into a note.

## GetChords

There is the [ChordsManagingUtilities](xref:Melanchall.DryWetMidi.Interaction.ChordsManagingUtilities) class which provides useful methods `GetChords` to get notes from a MIDI file or track chunk. For example, you can get chords a MIDI file contains with this code:

```csharp
using System;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace DwmExamples
{
    class Program
    {
        static void Main(string[] args)
        {
            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new NoteOnEvent
                    {
                        Channel = (FourBitNumber)5,
                        DeltaTime = 10
                    },
                    new NoteOffEvent
                    {
                        Channel = (FourBitNumber)5
                    },
                    new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)50)
                    {
                        Channel = (FourBitNumber)5
                    },
                    new NoteOffEvent((SevenBitNumber)70, (SevenBitNumber)30)
                    {
                        Channel = (FourBitNumber)5,
                        DeltaTime = 70
                    }));

            Console.WriteLine("Chords:");

            foreach (var chord in midiFile.GetChords())
            {
                Console.Write($@"
chord
  channel = {chord.Channel}
  time = {chord.Time}
  length = {chord.Length}
  notes:");

                foreach (var note in chord.Notes)
                {

                    Console.Write($@"
  note {note} (note number = {note.NoteNumber})
    channel = {note.Channel}
    time = {note.Time}
    length = {note.Length}
    velocity = {note.Velocity}
    off velocity = {note.OffVelocity}");
                }
            }

            Console.ReadKey();
        }
    }
}
```

Running the program, we'll see following output:

```text
Chords:

chord
  channel = 0
  time = 0
  length = 0
  notes:
  note C-1 (note number = 0)
    channel = 0
    time = 0
    length = 0
    velocity = 0
    off velocity = 0
chord
  channel = 5
  time = 10
  length = 70
  notes:
  note C-1 (note number = 0)
    channel = 5
    time = 10
    length = 0
    velocity = 0
    off velocity = 0
  note A#4 (note number = 70)
    channel = 5
    time = 10
    length = 70
    velocity = 50
    off velocity = 30
```

Please examine [ChordsManagingUtilities](xref:Melanchall.DryWetMidi.Interaction.ChordsManagingUtilities) class to see other `GetChords` overloads.

### Settings

All `GetChords` overloads can accept [ChordDetectionSettings](xref:Melanchall.DryWetMidi.Interaction.ChordDetectionSettings) as last parameter. Via this parameter you can adjust the process of chords building. Let's see each setting in details.

#### `NoteDetectionSettings`

Chords are built on top of notes. So to build chords we need to build notes. The process of notes building is adjustable via [NoteDetectionSettings](xref:Melanchall.DryWetMidi.Interaction.ChordDetectionSettings.NoteDetectionSettings) property. Properties of the [NoteDetectionSettings](xref:Melanchall.DryWetMidi.Interaction.NoteDetectionSettings) are described in detail [above](#settings).

#### `NotesTolerance`

The [NotesTolerance](xref:Melanchall.DryWetMidi.Interaction.ChordDetectionSettings.NotesTolerance) property defines the maximum distance of notes from the start of the first note of a chord. Notes within this tolerance will be included in a chord. The default value is `0`.

To understand how this property works let's take a look at the following notes (cross means any non-note event):

![NotesTolerance-Initial](images/Getting-objects-NotesTolerance-Initial.png)

If we set notes tolerance to `0` (which is default value), we'll get three different chords (each of one note):

![NotesTolerance-0](images/Getting-objects-NotesTolerance-0.png)

Different colors denotes different chords. If we set notes tolerance to `1`, we'll get two chords:

![NotesTolerance-1](images/Getting-objects-NotesTolerance-1.png)

With tolerance of `2` we'll finally get a single chord:

![NotesTolerance-2](images/Getting-objects-NotesTolerance-2.png)

#### `NotesMinCount`

The [NotesMinCount](xref:Melanchall.DryWetMidi.Interaction.ChordDetectionSettings.NotesMinCount) property defines the minimum count of notes a chord can contain. So if count of simultaneously sounding notes is less than this value, they won't make up a chord. The default value is `1` which means a single note can be turned to a chord.

To understand how this property works let's take a look at the following notes (cross means any non-note event):

![NotesMinCount-Initial](images/Getting-objects-NotesMinCount-Initial.png)

So we have three notes. For simplicity we'll assume that [NotesTolerance](xref:Melanchall.DryWetMidi.Interaction.ChordDetectionSettings.NotesTolerance) is `0` (default value). If we set notes min count to `1` (which is default value), we'll get two different chords:

![NotesMinCount-1](images/Getting-objects-NotesMinCount-1.png)

If we set notes min count to `2`, we'll get only one chord:

![NotesMinCount-2](images/Getting-objects-NotesMinCount-2.png)

Last note will not be turned into a chord because count of notes for a chord will be `1` which is less than the specified minimum count. With minimum count of notes of `3` we'll get no chords:

![NotesMinCount-3](images/Getting-objects-NotesMinCount-3.png)

First possible chord will contain two notes and second chord will contain one note. In both cases count of notes is less than the specified minimum count.

## GetObjects

All methods we saw before return collection of objects of the **same type**. So you can get only either notes or chords or timed events. To highlight the problem, let's take a look at the following events sequence:

![GetObjects-Initial](images/Getting-objects-GetObjects-Initial.png)

where empty circle and filled one mean _Note On_ and _Note Off_ events correspondingly; cross means any other event. We assume all note events have the same note number and channel.

With `GetTimedEvents` we'll just get all these events as is. `GetNotes` will give us only notes:

![GetObjects-GetNotes](images/Getting-objects-GetObjects-GetNotes.png)

`GetChords` will return only chords (single one in this example):

![GetObjects-GetChords](images/Getting-objects-GetObjects-GetChords.png)

So if we run following simple program:

```csharp
using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace DwmExamples
{
    class Program
    {
        static void Main(string[] args)
        {
            var midiFile = new MidiFile(
                new TrackChunk(
                    new TextEvent("1"),
                    new NoteOnEvent { DeltaTime = 1 },
                    new TextEvent("2") { DeltaTime = 1 },
                    new NoteOffEvent { DeltaTime = 1 },
                    new TextEvent("3") { DeltaTime = 1 },
                    new NoteOnEvent { DeltaTime = 1 },
                    new TextEvent("4") { DeltaTime = 1 },
                    new NoteOffEvent { DeltaTime = 1 },
                    new TextEvent("5") { DeltaTime = 1 },
                    new NoteOnEvent { DeltaTime = 1 },
                    new TextEvent("6")),
                new TrackChunk(
                    new TextEvent("A"),
                    new TextEvent("B") { DeltaTime = 1 },
                    new TextEvent("C") { DeltaTime = 1 },
                    new TextEvent("D") { DeltaTime = 1 },
                    new TextEvent("E") { DeltaTime = 1 },
                    new NoteOnEvent { DeltaTime = 1 },
                    new TextEvent("F") { DeltaTime = 1 },
                    new NoteOffEvent { DeltaTime = 1 },
                    new TextEvent("G") { DeltaTime = 1 },
                    new TextEvent("H") { DeltaTime = 1 },
                    new TextEvent("I")));

            Console.WriteLine("Getting timed events...");
            WriteTimedObjects(midiFile.GetTimedEvents());
            Console.WriteLine("Getting notes...");
            WriteTimedObjects(midiFile.GetNotes());
            Console.WriteLine("Getting chords...");
            WriteTimedObjects(midiFile.GetChords(new ChordDetectionSettings
            {
                NotesMinCount = 2
            }));

            Console.ReadKey();
        }

        private static void WriteTimedObjects<TObject>(ICollection<TObject> timedObjects)
            where TObject : ITimedObject
        {
            foreach (var timedObject in timedObjects)
            {
                Console.WriteLine($"[{timedObject.GetType().Name}] {timedObject} (time = {timedObject.Time})");
            }
        }
    }
}
```

we'll get this output:

```text
Getting timed events...
[TimedEvent] Event at 0: Text (1) (time = 0)
[TimedEvent] Event at 0: Text (A) (time = 0)
[TimedEvent] Event at 1: Note On [0] (0, 0) (time = 1)
[TimedEvent] Event at 1: Text (B) (time = 1)
[TimedEvent] Event at 2: Text (2) (time = 2)
[TimedEvent] Event at 2: Text (C) (time = 2)
[TimedEvent] Event at 3: Note Off [0] (0, 0) (time = 3)
[TimedEvent] Event at 3: Text (D) (time = 3)
[TimedEvent] Event at 4: Text (3) (time = 4)
[TimedEvent] Event at 4: Text (E) (time = 4)
[TimedEvent] Event at 5: Note On [0] (0, 0) (time = 5)
[TimedEvent] Event at 5: Note On [0] (0, 0) (time = 5)
[TimedEvent] Event at 6: Text (4) (time = 6)
[TimedEvent] Event at 6: Text (F) (time = 6)
[TimedEvent] Event at 7: Note Off [0] (0, 0) (time = 7)
[TimedEvent] Event at 7: Note Off [0] (0, 0) (time = 7)
[TimedEvent] Event at 8: Text (5) (time = 8)
[TimedEvent] Event at 8: Text (G) (time = 8)
[TimedEvent] Event at 9: Note On [0] (0, 0) (time = 9)
[TimedEvent] Event at 9: Text (6) (time = 9)
[TimedEvent] Event at 9: Text (H) (time = 9)
[TimedEvent] Event at 9: Text (I) (time = 9)
Getting notes...
[Note] C-1 (time = 1)
[Note] C-1 (time = 5)
[Note] C-1 (time = 5)
Getting chords...
[Chord] C-1 C-1 (time = 5)
```

As you can see there is "free" _Note On_ event without corresponding _Note Off_ one so we can't build a note for it. What if we want to get all possible notes and all remaining timed events? DryWetMIDI provides [GetObjectsUtilities](xref:Melanchall.DryWetMidi.Interaction.GetObjectsUtilities) class which contains `GetObjects` methods (for the same MIDI structures as previous methods). We can change printing part of the program above to:


```csharp
Console.WriteLine("Getting notes and timed events...");
WriteTimedObjects(midiFile.GetObjects(ObjectType.Note | ObjectType.TimedEvent));
```

which will give us following output:

```text
Getting notes and timed events...
[TimedEvent] Event at 0: Text (1) (time = 0)
[TimedEvent] Event at 0: Text (A) (time = 0)
[Note] C-1 (time = 1)
[TimedEvent] Event at 1: Text (B) (time = 1)
[TimedEvent] Event at 2: Text (2) (time = 2)
[TimedEvent] Event at 2: Text (C) (time = 2)
[TimedEvent] Event at 3: Text (D) (time = 3)
[TimedEvent] Event at 4: Text (3) (time = 4)
[TimedEvent] Event at 4: Text (E) (time = 4)
[Note] C-1 (time = 5)
[Note] C-1 (time = 5)
[TimedEvent] Event at 6: Text (4) (time = 6)
[TimedEvent] Event at 6: Text (F) (time = 6)
[TimedEvent] Event at 8: Text (5) (time = 8)
[TimedEvent] Event at 8: Text (G) (time = 8)
[TimedEvent] Event at 9: Note On [0] (0, 0) (time = 9)
[TimedEvent] Event at 9: Text (6) (time = 9)
[TimedEvent] Event at 9: Text (H) (time = 9)
[TimedEvent] Event at 9: Text (I) (time = 9)
```

So all note events that build up a note were turned into instances of [Note](xref:Melanchall.DryWetMidi.Interaction.Note), and all remaining events (including "free" _Note On_ one) were returned as instances of [TimedEvent](xref:Melanchall.DryWetMidi.Interaction.TimedEvent).

We can go further and collect all possible chords, notes and timed events:

```csharp
Console.WriteLine("Getting chords, notes and timed events...");
WriteTimedObjects(midiFile.GetObjects(
    ObjectType.Chord | ObjectType.Note | ObjectType.TimedEvent,
    new ObjectDetectionSettings
    {
        ChordDetectionSettings = new ChordDetectionSettings
        {
            NotesMinCount = 2
        }
    }));
```

which will give us following output:

```text
Getting chords, notes and timed events...
[TimedEvent] Event at 0: Text (1) (time = 0)
[TimedEvent] Event at 0: Text (A) (time = 0)
[Note] C-1 (time = 1)
[TimedEvent] Event at 1: Text (B) (time = 1)
[TimedEvent] Event at 2: Text (2) (time = 2)
[TimedEvent] Event at 2: Text (C) (time = 2)
[TimedEvent] Event at 3: Text (D) (time = 3)
[TimedEvent] Event at 4: Text (3) (time = 4)
[TimedEvent] Event at 4: Text (E) (time = 4)
[Chord] C-1 C-1 (time = 5)
[TimedEvent] Event at 6: Text (4) (time = 6)
[TimedEvent] Event at 6: Text (F) (time = 6)
[TimedEvent] Event at 8: Text (5) (time = 8)
[TimedEvent] Event at 8: Text (G) (time = 8)
[TimedEvent] Event at 9: Note On [0] (0, 0) (time = 9)
[TimedEvent] Event at 9: Text (6) (time = 9)
[TimedEvent] Event at 9: Text (H) (time = 9)
[TimedEvent] Event at 9: Text (I) (time = 9)
```

Or in visual representation:

![GetObjects-GetChordsAndNotesAndTimedEvents](images/Getting-objects-GetObjects-GetChordsAndNotesAndTimedEvents.png)

Currently `GetObjects` can build objects of the following types:

* [TimedEvent](xref:Melanchall.DryWetMidi.Interaction.TimedEvent)
* [Note](xref:Melanchall.DryWetMidi.Interaction.Note)
* [Chord](xref:Melanchall.DryWetMidi.Interaction.Chord)
* [Rest](xref:Melanchall.DryWetMidi.Interaction.Rest)

## Rests

To build rests you need to use extension methods from the [RestsUtilities](xref:Melanchall.DryWetMidi.Interaction.RestsUtilities) class.

If you take a look into the class, you'll discover two methods – [WithRests](xref:Melanchall.DryWetMidi.Interaction.RestsUtilities.WithRests*) and [GetRests](xref:Melanchall.DryWetMidi.Interaction.RestsUtilities.GetRests*). The first one adds rests to a collection of objects you've passed to the method. The second method returns rests only.

It will be much easier to understand how rests building works with examples. So let's look on [WithRests](xref:Melanchall.DryWetMidi.Interaction.RestsUtilities.WithRests*) (there is no great value to discuss [GetRests](xref:Melanchall.DryWetMidi.Interaction.RestsUtilities.GetRests*) since it works in the same way but just returns rests only).

Supposing we have following notes (with two different note numbers on two different channels):

![GetObjects-Rests-Initial](images/Getting-objects-GetObjects-Rests-Initial.png)

Using following code:

```csharp
var notesAndRests = notes
    .WithRests(new RestDetectionSettings
    {
        KeySelector = obj => 0
    });
```

we'll get only one rest:

![GetObjects-Rests-NoSeparation](images/Getting-objects-GetObjects-Rests-NoSeparation.png)

An important concept we need to discuss is a key selection. Key is used to calculate rests. Rests are always calculated only between objects with the same key. If an object with different key is encountered, rests will be calculated for that key.

In the code above we're saying: _The key of each object is 0_. So for the rests building algorithm all objects are same, there is no difference between channels and note numbers, for example. So rests will be constructed only at spaces where there are no notes at all (with any channels and any note numbers).

Also please take a look at the predefined key selectors available via constants of the [RestDetectionSettings](xref:Melanchall.DryWetMidi.Interaction.RestDetectionSettings). We can rewrite code above:

```csharp
var notesAndRests = notes
    .WithRests(RestDetectionSettings.NoNotes);
```

Using following code:

```csharp
var notesAndRests = notes
    .WithRests(new RestDetectionSettings
    {
        KeySelector = obj => (obj as Note)?.Channel
    });
```

 we'll get three rests now:

![GetObjects-Rests-SeparateByChannel](images/Getting-objects-GetObjects-Rests-SeparateByChannel.png)

So rests are separated by channels. Channel is the key of an object. Note number of a note doesn't matter, all numbers are treated as the same one. So rests will be constructed separately for each channel at spaces where there are no notes (with any note numbers).

The key for which a rest has been built will be store in the [Key](xref:Melanchall.DryWetMidi.Interaction.Rest.Key) property of [Rest](xref:Melanchall.DryWetMidi.Interaction.Rest) class. `notesAndRests` is a collection containing both `notes` and calculated rests, and elements of this collection are sorted by their times.

Note that you can build rests for objects of different types. Why not to get chords from a MIDI file and add rests between them?

```csharp
var chordsAndRests = midiFile
    .GetObjects(ObjectType.Chord)
    .WithRests(new RestDetectionSettings
    {
        KeySelector = obj => (obj as Chord)?.Channel
    });
```

And a couple of words about return value of key selector. If `null` is returned, an object won't participate in rests building process. It allows you to have rests for desired objects only. For example:

```csharp
var notesAndChordsAndRests = midiFile
    .GetObjects(ObjectType.Note | ObjectType.Chord)
    .WithRests(new RestDetectionSettings
    {
        KeySelector = obj => (obj as Note)?.Channel
    });
```

Here we specify that rests will be built for notes only (key selector will return `null` for an object other than note). So the result collection will have chords, notes and rests between notes with channel as the key.

And a couple of additional examples with notes presented on the picture above.

Code:

```csharp
var notesAndRests = notes
    .WithRests(new RestDetectionSettings
    {
        KeySelector = obj => (obj as Note)?.NoteNumber
    });
```

Rests:

![GetObjects-Rests-SeparateByNoteNumber](images/Getting-objects-GetObjects-Rests-SeparateByNoteNumber.png)

As you can see rests now are separated by note number (channel doesn't matter). So rests will be constructed for each note number at spaces where there are no notes (with any channel).

Code:

```csharp
var notesAndRests = notes
    .WithRests(new RestDetectionSettings
    {
        KeySelector = obj => ((obj as Note)?.NoteNumber, (obj as Note)?.NoteNumber)
    });
```

Now we'll get rests at every "free" space (since the key is a pair of channel and note's number):

![GetObjects-Rests-SeparateByChannelAndNoteNumber](images/Getting-objects-GetObjects-Rests-SeparateByChannelAndNoteNumber.png)