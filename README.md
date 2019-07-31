![DryWetMIDI Logo](https://github.com/melanchall/drywetmidi/blob/master/Resources/Images/dwm-logo.png)

[![NuGet](https://img.shields.io/nuget/v/Melanchall.DryWetMidi.svg)](https://www.nuget.org/packages/Melanchall.DryWetMidi/) [![Build](https://ci.appveyor.com/api/projects/status/github/melanchall/drywetmidi?svg=true&branch=master)](https://ci.appveyor.com/project/melanchall/drywetmidi/branch/master)

DryWetMIDI is the .NET library to work with MIDI files and MIDI devices. Visit [Wiki](https://github.com/melanchall/drymidi/wiki) to learn how to use the DryWetMIDI. The library was tested on 130,000 files taken from [here](https://www.reddit.com/r/WeAreTheMusicMakers/comments/3ajwe4/the_largest_midi_collection_on_the_internet/). Thanks *midi-man* for this great collection. You can get the latest version via [NuGet](https://www.nuget.org/packages/Melanchall.DryWetMidi).

## Features

With the DryWetMIDI you can:

* Read, write and create [Standard MIDI Files (SMF)](https://www.midi.org/specifications/category/smf-specifications). It is also possible to read [RMID](https://www.loc.gov/preservation/digital/formats/fdd/fdd000120.shtml) files where SMF wrapped to RIFF chunk.
* [Send](https://github.com/melanchall/drywetmidi/wiki/Output-device) MIDI events to/[receive](https://github.com/melanchall/drywetmidi/wiki/Input-device) them from MIDI devices, [play](https://github.com/melanchall/drywetmidi/wiki/Playback) MIDI data and [record](https://github.com/melanchall/drywetmidi/wiki/Recording) it.
* Finely adjust process of reading and writing. It allows, for example, to read corrupted files and repair them, or build MIDI file validators.
* Implement [custom meta events](https://github.com/melanchall/drywetmidi/wiki/Custom-meta-events) and [custom chunks](https://github.com/melanchall/drywetmidi/wiki/Custom-chunks) that can be written to and read from MIDI files.
* Easily catch specific error when reading or writing MIDI file since all possible errors in a MIDI file are presented as separate exception classes.
* Manage content of a MIDI file either with low-level objects, like event, or high-level ones, like note (read the **High-level data managing** section of the Wiki).
* Build musical compositions (see [Pattern](https://github.com/melanchall/drywetmidi/wiki/Pattern) page of the library Wiki).
* Perform complex tasks like quantizing, notes splitting or converting MIDI file to CSV representation (see [Tools](https://github.com/melanchall/drywetmidi/wiki/Tools) page of the Wiki).

## Projects using DryWetMIDI

Here the list of noticeable projects that use DryWetMIDI:

* [Clone Hero](https://clonehero.net)  
  Free rhythm game, which can be played with any 5 or 6 button guitar controller, game controllers, or just your standard computer keyboard. The game is a clone of Guitar Hero.

## Getting Started

There are several articles that can help you dive into API provided by DryWetMIDI:

* [DryWetMIDI: High-Level Processing of MIDI Files](https://www.codeproject.com/Articles/1200014/DryWetMIDI-High-level-processing-of-MIDI-files)
* [DryWetMIDI: Notes Quantization](https://www.codeproject.com/Articles/1204629/DryWetMIDI-Notes-Quantization)
* [DryWetMIDI: Working with MIDI Devices](https://www.codeproject.com/Articles/1275475/DryWetMIDI-Working-with-MIDI-Devices)

Let's see some examples of what you can do with DryWetMIDI.

To [read a MIDI file](https://github.com/melanchall/drymidi/wiki/Reading-a-MIDI-file) you have to use ```Read``` static method of the ```MidiFile```:

```csharp
var midiFile = MidiFile.Read("Some Great Song.mid");
```

or, in more advanced form (visit [Reading settings](https://github.com/melanchall/drywetmidi/wiki/Reading-settings) page on Wiki to learn more about how to adjust process of reading)

```csharp
var midiFile = MidiFile.Read("Some Great Song.mid",
                             new ReadingSettings
                             {
                                 NoHeaderChunkPolicy = NoHeaderChunkPolicy.Abort,
                                 CustomChunkTypes = new ChunkTypesCollection
                                 {
                                     { typeof(MyCustomChunk), "Cstm" }
                                 }
                             });
```

To [write MIDI data to a file](https://github.com/melanchall/drymidi/wiki/Writing-a-MIDI-file) you have to use ```Write``` method of the ```MidiFile```:

```csharp
midiFile.Write("My Great Song.mid");
```

or, in more advanced form (visit [Writing settings](https://github.com/melanchall/drywetmidi/wiki/Writing-settings) page on Wiki to learn more about how to adjust process of writing)

```csharp
midiFile.Write("My Great Song.mid",
               true,
               MidiFileFormat.SingleTrack,
               new WritingSettings
               {
                   CompressionPolicy = CompressionPolicy.Default
               });
```

Of course you can create a MIDI file from scratch by creating an instance of the ```MidiFile``` and writing it:

```csharp
var midiFile = new MidiFile(
                   new TrackChunk(
                       new SetTempoEvent(500000)),
                   new TrackChunk(
                       new TextEvent("It's just single note track..."),
                       new NoteOnEvent((SevenBitNumber)60, (SevenBitNumber)45),
                       new NoteOffEvent((SevenBitNumber)60, (SevenBitNumber)0)
                       {
                           DeltaTime = 400
                       }));
midiFile.Write("My Future Great Song.mid");
```

or

```csharp
var midiFile = new MidiFile();
TempoMap tempoMap = midiFile.GetTempoMap();

var trackChunk = new TrackChunk();
using (var notesManager = trackChunk.ManageNotes())
{
    NotesCollection notes = notesManager.Notes;
    notes.Add(new Note(NoteName.A,
                       4,
                       LengthConverter.ConvertFrom(new MetricTimeSpan(hours: 0,
                                                                      minutes: 0,
                                                                      seconds: 10),
                                                   0,
                                                   tempoMap)));
}

midiFile.Chunks.Add(trackChunk);
midiFile.Write("My Future Great Song.mid");
```

If you want to speed up playing back a MIDI file by two times you can do it with this code:

```csharp                   
foreach (var trackChunk in midiFile.Chunks.OfType<TrackChunk>())
{
    foreach (var setTempoEvent in trackChunk.Events.OfType<SetTempoEvent>())
    {
        setTempoEvent.MicrosecondsPerBeat /= 2;
    }
}
```

Of course this code is simplified. In practice a MIDI file may not contain SetTempo event which means it has the default one (500,000 microseconds per beat).

Instead of modifying a MIDI file you can use [`Playback`](https://github.com/melanchall/drywetmidi/wiki/Playback) class:

```csharp
using (var outputDevice = OutputDevice.GetByName("Microsoft GS Wavetable Synth"))
using (var playback = midiFile.GetPlayback(outputDevice))
{
    playback.Speed = 2.0;
    playback.Play();
}
```

To get duration of a MIDI file as `TimeSpan` use this code:

```csharp
TempoMap tempoMap = midiFile.GetTempoMap();
TimeSpan midiFileDuration = midiFile.GetTimedEvents()
                                    .LastOrDefault(e => e.Event is NoteOffEvent)
                                    ?.TimeAs<MetricTimeSpan>(tempoMap) ?? new MetricTimeSpan();
```

Suppose you want to remove all C# notes from a MIDI file. It can be done with this code:

```csharp
foreach (var trackChunk in midiFile.GetTrackChunks())
{
    using (var notesManager = trackChunk.ManageNotes())
    {
        notesManager.Notes.RemoveAll(n => n.NoteName == NoteName.CSharp);
    }
}
```

or

```csharp
midiFile.RemoveNotes(n => n.NoteName == NoteName.CSharp);
```

To get all chords of a MIDI file at 20 seconds from the start of the file write this:

```csharp
TempoMap tempoMap = midiFile.GetTempoMap();
IEnumerable<Chord> chordsAt20seconds = midiFile.GetChords()
                                               .AtTime(new MetricTimeSpan(0, 0, 20),
                                                       tempoMap,
                                                       LengthedObjectPart.Entire);
```

To create a MIDI file with single note which length will be equal to length of two triplet eighth notes you can use this code:

```csharp
var midiFile = new MidiFile();
var tempoMap = midiFile.GetTempoMap();

var trackChunk = new TrackChunk();
using (var notesManager = trackChunk.ManageNotes())
{
    var length = LengthConverter.ConvertFrom(2 * MusicalTimeSpan.Eighth.Triplet(),
                                             0,
                                             tempoMap);
    var note = new Note(NoteName.A, 4, length);
    notesManager.Notes.Add(note);
}

midiFile.Chunks.Add(trackChunk);
midiFile.Write("Single note great song.mid");
```

You can even build a musical composition:

```csharp
Pattern pattern = new PatternBuilder()
     
    // Insert a pause of 5 seconds
    .StepForward(new MetricTimeSpan(0, 0, 5))

    // Insert an eighth C# note of the 4th octave
    .Note(Octave.Get(4).CSharp, MusicalTimeSpan.Eighth)

    // Set default note length to triplet eighth and default octave to 5
    .SetNoteLength(MusicalTimeSpan.Eighth.Triplet())
    .SetOctave(5)

    // Now we can add triplet eighth notes of the 5th octave in a simple way
    .Note(NoteName.A)
    .Note(NoteName.B)
    .Note(NoteName.GSharp)

    // Get pattern
    .Build();

MidiFile midiFile = pattern.ToFile(TempoMap.Default);
```

DryWetMIDI provides [devices API](https://github.com/melanchall/drywetmidi/wiki/MIDI-devices) allowing to send MIDI events to and receive them from MIDI devices. Following example shows how to send events to MIDI device and handle them as they are received by the device:

```csharp
using System;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Smf;

// ...

using (var outputDevice = OutputDevice.GetByName("MIDI Device"))
{
    outputDevice.EventSent += OnEventSent;

    using (var inputDevice = InputDevice.GetByName("MIDI Device"))
    {
        inputDevice.EventReceived += OnEventReceived;
        inputDevice.StartEventsListening();

        outputDevice.SendEvent(new NoteOnEvent());
        outputDevice.SendEvent(new NoteOffEvent());
    }
}

// ...

private void OnEventReceived(object sender, MidiEventReceivedEventArgs e)
{
    var midiDevice = (MidiDevice)sender;
    Console.WriteLine($"Event received from '{midiDevice.Name}' at {DateTime.Now}: {e.Event}");
}

private void OnEventSent(object sender, MidiEventSentEventArgs e)
{
    var midiDevice = (MidiDevice)sender;
    Console.WriteLine($"Event sent to '{midiDevice.Name}' at {DateTime.Now}: {e.Event}");
}
```
