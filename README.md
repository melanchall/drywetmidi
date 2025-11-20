![DryWetMIDI Logo](https://raw.githubusercontent.com/melanchall/drywetmidi/develop/Resources/Images/dwm-logo.png)

[![NuGet (full)](https://img.shields.io/nuget/v/Melanchall.DryWetMidi.svg?label=NuGet%20(full)&color=5295D0)](https://www.nuget.org/packages/Melanchall.DryWetMidi/) [![NuGet (nativeless)](https://img.shields.io/nuget/v/Melanchall.DryWetMidi.Nativeless.svg?label=NuGet%20(nativeless)&color=5295D0)](https://www.nuget.org/packages/Melanchall.DryWetMidi.Nativeless/) [![Unity asset (full)](https://img.shields.io/static/v1?label=Unity%20Asset%20(full)&message=v8.0.2&color=5295D0)](https://assetstore.unity.com/packages/tools/audio/drywetmidi-222171) [![Unity asset (nativeless)](https://img.shields.io/static/v1?label=Unity%20Asset%20(nativeless)&message=v8.0.2&color=5295D0)](https://assetstore.unity.com/packages/tools/audio/drywetmidi-nativeless-228998)

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

## Projects using DryWetMIDI

Here the list of noticeable projects that use the library:

* [Playtonik](https://5of12.co.uk/#playtonik)  
  Playtonik is an app designed and developed by [5of12](https://5of12.co.uk). It uses physics-based interactions, immersive spatial audio and dynamic haptic feedback for a fun, relaxing experience. For the fidgeters, Playtonik comes with a selection of built in sounds and an on screen keyboard. Scale filters help keep you in tune, so you can focus on having fun! For the musicians, MIDI support lets you connect your favourite instrument. Playtonik can work as a note source or as a chaotic MIDI delay, great for adding texture to your sound.
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

Let's see small examples of what you can do with the library.

It's possible to [read a MIDI file](https://melanchall.github.io/drywetmidi/articles/file-reading-writing/MIDI-file-reading.html), then [collect all notes](https://melanchall.github.io/drywetmidi/articles/high-level-managing/Getting-objects.html) from it and print their time and length in the [metric](https://melanchall.github.io/drywetmidi/articles/high-level-managing/Time-and-length.html) (hours, minutes, second, ...) format:

```csharp
var midiFile = MidiFile.Read("MyFile.mid");
var tempoMap = midiFile.GetTempoMap();
 
foreach (var note in midiFile.GetNotes())
{
    var time = note.TimeAs<MetricTimeSpan>(tempoMap);
    var length = note.LengthAs<MetricTimeSpan>(tempoMap);
    Console.WriteLine($"{note} at {time} with length of {length}");
}
```

Or maybe you want to [record data](https://melanchall.github.io/drywetmidi/articles/recording/Overview.html) from a [MIDI device](https://melanchall.github.io/drywetmidi/articles/devices/Overview.html), then [quantize](https://melanchall.github.io/drywetmidi/articles/tools/Overview.html) recorded events by the grid with step of 1/8, and [play](https://melanchall.github.io/drywetmidi/articles/playback/Overview.html) the data via the default Windows synth:

```csharp
var inputDevice = InputDevice.GetByName("MyMidiKeyboard");
inputDevice.StartEventsListening();

var recording = new Recording(TempoMap.Default, inputDevice);
recording.Start();
 
// ...
 
recording.Stop();
inputDevice.Dispose();
 
var recordedFile = recording.ToFile();
recording.Dispose();
 
recordedFile.QuantizeObjects(
    ObjectType.TimedEvent,
    new SteppedGrid(MusicalTimeSpan.Eighth));
 
var outputDevice = OutputDevice.GetByName("Microsoft GS Wavetable Synth");
var playback = recordedFile.GetPlayback(outputDevice);
playback.Start();
 
// ...
 
playback.Dispose();
outputDevice.Dispose();
```

You can even [build a musical composition](https://melanchall.github.io/drywetmidi/articles/composing/Pattern.html):

```csharp
var pattern = new PatternBuilder()
     
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

var midiFile = pattern.ToFile(TempoMap.Create(Tempo.FromBeatsPerMinute(240)));
midiFile.Write("DrumPattern.mid");
```

Also you can check out sample applications from [CIRCE-EYES](https://github.com/CIRCE-EYES) (see the profile, VB.NET is used)