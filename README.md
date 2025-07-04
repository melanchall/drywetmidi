![DryWetMIDI Logo](https://raw.githubusercontent.com/melanchall/drywetmidi/develop/Resources/Images/dwm-logo.png)

[![NuGet (full)](https://img.shields.io/nuget/v/Melanchall.DryWetMidi.svg?label=NuGet%20(full)&color=5295D0)](https://www.nuget.org/packages/Melanchall.DryWetMidi/) [![NuGet (nativeless)](https://img.shields.io/nuget/v/Melanchall.DryWetMidi.Nativeless.svg?label=NuGet%20(nativeless)&color=5295D0)](https://www.nuget.org/packages/Melanchall.DryWetMidi.Nativeless/) [![Unity asset (full)](https://img.shields.io/static/v1?label=Unity%20Asset%20(full)&message=v8.0.1&color=5295D0)](https://assetstore.unity.com/packages/tools/audio/drywetmidi-222171) [![Unity asset (nativeless)](https://img.shields.io/static/v1?label=Unity%20Asset%20(nativeless)&message=v8.0.1&color=5295D0)](https://assetstore.unity.com/packages/tools/audio/drywetmidi-nativeless-228998)

<!--OVERVIEW-->

DryWetMIDI is the .NET library to work with MIDI data and MIDI devices. It allows:

* Read, write and create [Standard MIDI Files (SMF)](https://midi.org/standard-midi-files-specification). It is also possible to:  
  * read [RMID](https://www.loc.gov/preservation/digital/formats/fdd/fdd000120.shtml) files where SMF wrapped to RIFF chunk;
  * easily catch specific error when reading or writing a MIDI file since all possible errors in a file are presented as separate exception classes;
  * finely adjust the process of reading and writing (it allows, for example, to read corrupted files and repair them, or build MIDI file validators);
  * implement [custom meta events](https://melanchall.github.io/drywetmidi/articles/custom-data-structures/Custom-meta-events.html) and [custom chunks](https://melanchall.github.io/drywetmidi/articles/custom-data-structures/Custom-chunks.html) that can be written to and read from MIDI files.
* [Send](https://melanchall.github.io/drywetmidi/articles/devices/Output-device.html) MIDI events to/[receive](https://melanchall.github.io/drywetmidi/articles/devices/Input-device.html) them from MIDI devices.
* [Play](https://melanchall.github.io/drywetmidi/articles/playback/Overview.html) MIDI data and [record](https://melanchall.github.io/drywetmidi/articles/recording/Overview.html) it.
* Manage MIDI data either with low-level objects, like a MIDI event, or high-level ones, like a note, using different time and length representations (read the **High-level data managing** section of the [library docs](https://melanchall.github.io/drywetmidi)).
* Build musical compositions (see [Pattern](https://melanchall.github.io/drywetmidi/articles/composing/Pattern.html) page of the library docs) and use music theory API (see [Music Theory - Overview](https://melanchall.github.io/drywetmidi/articles/music-theory/Overview.html) article).
* Perform complex tasks like quantizing, notes splitting or converting MIDI file to CSV representation (see [Tools](https://melanchall.github.io/drywetmidi/articles/tools/Overview.html) page of the library docs).

Please see [Getting started](#getting-started) section below for quick jump into the library.

> **Warning**  
> If you want to create an issue or a discussion, read this article first – [Support](https://melanchall.github.io/drywetmidi/articles/dev/Support.html).

## Useful Links

* [Documentation](https://melanchall.github.io/drywetmidi)
* [NuGet](https://www.nuget.org/packages/Melanchall.DryWetMidi)
* [Project health](https://melanchall.github.io/drywetmidi/articles/dev/Project-health.html)

## Projects using DryWetMIDI

Here the list of noticeable projects that use DryWetMIDI:

* [EMU – Sound to Light Controller](https://www.enttec.com/product/dmx-lighting-control-software/emu-sound-to-light-controller)  
  EMU (DMXIS’s next generation) is a state-of-the-art, intuitive sound-to-light controller designed for professional live musicians and DJs. Easy to use software, EMU allows you to run automated or responsive DMX light shows, leaving you to focus on your show!
* [Musical Bits](https://musicalbits.de)  
  Musical Bits creates software that helps you creating music. Our software uses latest technology, including AI, to model all layers of creativity of a human composer. These layers are implemented as reusable and combinable software components. Musical Bits software is available as co-pilot for producers and composers under the name KLANGMACHT and helps you create, drumsounds, beats, guitars, background choirs, lyrics and more. We even create and distribute full virtual bands, albums and songs. For example, check out the Frostbite Orckings.
* [CoyoteMIDI](https://coyotemidi.com)  
  CoyoteMIDI extends the functionality of your MIDI devices to include keyboard and mouse input, including complex key combinations and multi-step macros.
* [Clone Hero](https://clonehero.net)  
  Free rhythm game, which can be played with any 5 or 6 button guitar controller, game controllers, or just your standard computer keyboard. The game is a clone of Guitar Hero.
* [Electrophonics](https://kaiclavier.itch.io/electrophonics)  
  A collection of virtual musical instruments that features real MIDI output.
* [Rustissimo](https://store.steampowered.com/app/1222580/Rustissimo)  
  Using Rustissimo you can create a concert with your friends and play instruments with synchronization.

If you find that DryWetMIDI has been useful for your project, please put a link to the library in your project's _About_ section or something like that.

## Getting Started

Let's see some examples of what you can do with DryWetMIDI (also you can check out sample applications from [CIRCE-EYES](https://github.com/CIRCE-EYES) (see the profile, VB.NET used)).

To [read a MIDI file](https://melanchall.github.io/drywetmidi/articles/file-reading-writing/MIDI-file-reading.html) you have to use the `Read` static method of the `MidiFile`:

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

To [write MIDI data to a file](https://melanchall.github.io/drywetmidi/articles/file-reading-writing/MIDI-file-writing.html) you have to use the `Write` method of the `MidiFile`:

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
        UseRunningStatus = true,
        NoteOffAsSilentNoteOn = true
    });
```

Of course you can create a MIDI file from scratch by creating an instance of the `MidiFile` and writing it:

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
    NotesCollection notes = notesManager.Objects;
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

Of course this code is simplified. In practice a MIDI file may not contain SetTempo event which means it has the default one (`500000` microseconds per beat).

Instead of modifying a MIDI file you can use [`Playback`](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Multimedia.Playback.html) class:

```csharp
using (var outputDevice = OutputDevice.GetByName("Microsoft GS Wavetable Synth"))
using (var playback = midiFile.GetPlayback(outputDevice))
{
    playback.Speed = 2.0;
    playback.Play();
}
```

To get duration of a MIDI file as standard `TimeSpan` use this code:

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

Suppose you want to remove all _C#_ notes from a MIDI file. It can be done with this code:

```csharp
foreach (var trackChunk in midiFile.GetTrackChunks())
{
    using (var notesManager = trackChunk.ManageNotes())
    {
        notesManager.Objects.RemoveAll(n => n.NoteName == NoteName.CSharp);
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
    notesManager.Objects.Add(note);
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

    // Insert a simple drum pattern
    .PianoRoll(@"
        F#2   ||||||||
        D2    --|---|-
        C2    |---|---")
    .Repeat(9)

    // Get pattern
    .Build();

MidiFile midiFile = pattern.ToFile(TempoMap.Default);
```

DryWetMIDI provides [devices API](https://melanchall.github.io/drywetmidi/articles/devices/Overview.html) allowing to send MIDI events to and receive them from MIDI devices. Following example shows how to send events to MIDI device and handle them as they are received by the device:

```csharp
using System;
using Melanchall.DryWetMidi.Multimedia;
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
