![DryWetMIDI Logo](https://github.com/melanchall/drywetmidi/blob/develop/Resources/Images/dwm-logo.png?raw=true)

[![NuGet](https://img.shields.io/nuget/v/Melanchall.DryWetMidi.svg)](https://www.nuget.org/packages/Melanchall.DryWetMidi/)

DryWetMIDI is the .NET library to work with MIDI files and MIDI devices. Visit [Documentation](https://melanchall.github.io/drywetmidi) to learn how to use the DryWetMIDI. The library was tested on 113,270 files taken from [here](https://www.reddit.com/r/WeAreTheMusicMakers/comments/3ajwe4/the_largest_midi_collection_on_the_internet/). Thanks *midi-man* for this great collection. You can get the latest version via [NuGet](https://www.nuget.org/packages/Melanchall.DryWetMidi).

## Status

|   |Windows (.NET Framework)|Windows (.NET Core)|macOS (.NET Core)|Linux (.NET Core)|
|---|---|---|---|---|
|**Core**|[![Build Status](https://dev.azure.com/Melanchall/DryWetMIDI/_apis/build/status/Windows/%5BWindows%5D%20Test%20core%20part%20on%20.NET%20Framework?branchName=develop)](https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=1&branchName=develop)|[![Build Status](https://dev.azure.com/Melanchall/DryWetMIDI/_apis/build/status/Windows/%5BWindows%5D%20Test%20core%20part%20on%20.NET%20Core?branchName=develop)](https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=13&branchName=develop)|[![Build Status](https://dev.azure.com/Melanchall/DryWetMIDI/_apis/build/status/macOS/%5BmacOS%5D%20Test%20core%20part%20on%20.NET%20Core?branchName=develop)](https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=11&branchName=develop)|[![Build Status](https://dev.azure.com/Melanchall/DryWetMIDI/_apis/build/status/Linux/%5BLinux%5D%20Test%20core%20part%20on%20.NET%20Core?branchName=develop)](https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=12&branchName=develop)|
|**Devices**|[![Build Status](https://dev.azure.com/Melanchall/DryWetMIDI/_apis/build/status/Windows/%5BWindows%5D%20Test%20devices%20part%20on%20.NET%20Framework?branchName=develop)](https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=10&branchName=develop)|[![Build Status](https://dev.azure.com/Melanchall/DryWetMIDI/_apis/build/status/Windows/%5BWindows%5D%20Test%20devices%20part%20on%20.NET%20Core?branchName=develop)](https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=14&branchName=develop)|Not supported|Not supported|

## Features

With the DryWetMIDI you can:

* Read, write and create [Standard MIDI Files (SMF)](https://www.midi.org/specifications/category/smf-specifications). It is also possible to read [RMID](https://www.loc.gov/preservation/digital/formats/fdd/fdd000120.shtml) files where SMF wrapped to RIFF chunk.
* [Send](https://melanchall.github.io/drywetmidi/articles/devices/Output-device.html) MIDI events to/[receive](https://melanchall.github.io/drywetmidi/articles/devices/Input-device.html) them from MIDI devices, [play](https://melanchall.github.io/drywetmidi/articles/playback/Overview.html) MIDI data and [record](https://melanchall.github.io/drywetmidi/articles/recording/Overview.html) it.
* Finely adjust process of reading and writing. It allows, for example, to read corrupted files and repair them, or build MIDI file validators.
* Implement [custom meta events](https://melanchall.github.io/drywetmidi/articles/custom-data-structures/Custom-meta-events.html) and [custom chunks](https://melanchall.github.io/drywetmidi/articles/custom-data-structures/Custom-chunks.html) that can be written to and read from MIDI files.
* Easily catch specific error when reading or writing MIDI file since all possible errors in a MIDI file are presented as separate exception classes.
* Manage content of a MIDI file either with low-level objects, like event, or high-level ones, like note (read the **High-level data managing** section of the library docs).
* Build musical compositions (see [Pattern](https://melanchall.github.io/drywetmidi/articles/composing/Pattern.html) page of the library docs).
* Perform complex tasks like quantizing, notes splitting or converting MIDI file to CSV representation (see [Tools](https://melanchall.github.io/drywetmidi/articles/tools/Overview.html) page of the library docs).

## Documentation

Complete documentation including API reference is available on https://melanchall.github.io/drywetmidi.

## Projects using DryWetMIDI

Here the list of noticeable projects that use DryWetMIDI:

* [Clone Hero](https://clonehero.net)  
  Free rhythm game, which can be played with any 5 or 6 button guitar controller, game controllers, or just your standard computer keyboard. The game is a clone of Guitar Hero.
* [Electrophonics](https://kaiclavier.itch.io/electrophonics)  
  A collection of virtual musical instruments that features real MIDI output.
* [Rustissimo](https://store.steampowered.com/app/1222580/Rustissimo)  
  Using Rustissimo you can create a concert with your friends and play instruments with synchronization.

## Getting Started

There are several articles that can help you dive into API provided by DryWetMIDI:

* [DryWetMIDI: High-Level Processing of MIDI Files](https://www.codeproject.com/Articles/1200014/DryWetMIDI-High-level-processing-of-MIDI-files)
* [DryWetMIDI: Notes Quantization](https://www.codeproject.com/Articles/1204629/DryWetMIDI-Notes-Quantization)
* [DryWetMIDI: Working with MIDI Devices](https://www.codeproject.com/Articles/1275475/DryWetMIDI-Working-with-MIDI-Devices)

Let's see some examples of what you can do with DryWetMIDI.

To [read a MIDI file](https://melanchall.github.io/drywetmidi/articles/file-reading-writing/MIDI-file-reading.html) you have to use ```Read``` static method of the ```MidiFile```:

```csharp
var midiFile = MidiFile.Read("Some Great Song.mid");
```

or, in more advanced form (visit [Reading settings](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Core.ReadingSettings.html) page on the library docs to learn more about how to adjust process of reading)

```csharp
var midiFile = MidiFile.Read(
    "Some Great Song.mid",
    new ReadingSettings
    {
        NoHeaderChunkPolicy = NoHeaderChunkPolicy.Abort,
        CustomChunkTypes = new ChunkTypesCollection
        {
            { typeof(MyCustomChunk), "Cstm" }
        }
    });
```

To [write MIDI data to a file](https://melanchall.github.io/drywetmidi/articles/file-reading-writing/MIDI-file-writing.html) you have to use ```Write``` method of the ```MidiFile```:

```csharp
midiFile.Write("My Great Song.mid");
```

or, in more advanced form (visit [Writing settings](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Core.WritingSettings.html) page on the library docs to learn more about how to adjust process of writing)

```csharp
midiFile.Write(
    "My Great Song.mid",
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
    notes.Add(new Note(
        NoteName.A,
        4,
        LengthConverter.ConvertFrom(
            new MetricTimeSpan(hours: 0, minutes: 0, seconds: 10),
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
        setTempoEvent.MicrosecondsPerQuarterNote /= 2;
    }
}
```

Of course this code is simplified. In practice a MIDI file may not contain SetTempo event which means it has the default one (500,000 microseconds per beat).

Instead of modifying a MIDI file you can use [`Playback`](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Devices.Playback.html) class:

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
TimeSpan midiFileDuration = midiFile
    .GetTimedEvents()
    .LastOrDefault(e => e.Event is NoteOffEvent)
    ?.TimeAs<MetricTimeSpan>(tempoMap) ?? new MetricTimeSpan();
```

or simply:

```csharp
TimeSpan midiFileDuration = midiFile.GetDuration<MetricTimeSpan>();
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
IEnumerable<Chord> chordsAt20seconds = midiFile
    .GetChords()
    .AtTime(
        new MetricTimeSpan(0, 0, 20),
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
    var length = LengthConverter.ConvertFrom(
        2 * MusicalTimeSpan.Eighth.Triplet(),
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
    .SetOctave(Octave.Get(5))

    // Now we can add triplet eighth notes of the 5th octave in a simple way
    .Note(NoteName.A)
    .Note(NoteName.B)
    .Note(NoteName.GSharp)

    // Get pattern
    .Build();

MidiFile midiFile = pattern.ToFile(TempoMap.Default);
```

DryWetMIDI provides [devices API](https://melanchall.github.io/drywetmidi/articles/devices/Overview.html) allowing to send MIDI events to and receive them from MIDI devices. Following example shows how to send events to MIDI device and handle them as they are received by the device:

```csharp
using System;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Core;

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
