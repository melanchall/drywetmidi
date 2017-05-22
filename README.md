![Channel events class diagram](https://github.com/melanchall/drywetmidi/blob/develop/Images/dwm-logo.png)

----------------

DryWetMIDI is the .NET library to work with MIDI files. You need to understand MIDI file structure to effectively work with the library since it operates by low-level MIDI objects like *event* and *track chunk*. Visit [The MIDI Association site](https://www.midi.org) to get more information about MIDI.

The library is under MIT license so you can do whatever you want with it.

## Features

* DryWetMIDI provides the way to read and write [Standard MIDI Files (SMF)](https://www.midi.org/specifications/category/smf-specifications). It is also possible to read [RMID](https://www.loc.gov/preservation/digital/formats/fdd/fdd000120.shtml) files where SMF wrapped to RIFF chunk.
* Process of reading and writing can be finely adjusted with help of ```ReadingSettings``` and ```WritingSettings```. It allows, for example, to read corrupted files and repair them, or build MIDI file validators.
* The library gives you ability to implement custom [meta events](https://github.com/melanchall/drywetmidi/wiki/Custom-meta-events) and [custom chunks](https://github.com/melanchall/drywetmidi/wiki/Custom-chunks).
* All possible errors in a MIDI file are presented in the DryWetMIDI as separate exception classes so you can easily catch specific error.

## Getting Started

Let's see some examples of what you can do with DryWetMIDI.

To [read a MIDI file](https://github.com/melanchall/drymidi/wiki/Reading-a-MIDI-file) you have to use ```Read``` static method of the ```MidiFile```:

```csharp
var midiFile = MidiFile.Read("My Great Song.mid");
```

or, in more advanced form (visit [Reading settings](https://github.com/melanchall/drywetmidi/wiki/Reading-settings) page on Wiki to learn more about how to adjust process of reading)

```csharp
var midiFile = MidiFile.Read("My Great Song.mid",
                             new ReadingSettings
                             {
                                 CustomChunkTypes = new ChunkTypesCollection
                                 {
                                     { typeof(MyCustomChunk), "Cstm" }
                                 }
                             });
```

To [write MIDI data to a file](https://github.com/melanchall/drymidi/wiki/Writing-a-MIDI-file) you have to use ```Write``` method of the ```MidiFile```:

```csharp
midiFile.Write("My Great Song speeded.mid");
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

Suppose you want to remove all *C#* notes from a MIDI file. It can be done with this code:

```csharp
foreach (var trackChunk in midiFile.Chunks.OfType<TrackChunk>())
{
    trackChunk.Events.RemoveAll(e => (e as NoteOnEvent)?.GetNoteName() == NoteName.CSharp ||
                                     (e as NoteOffEvent)?.GetNoteName() == NoteName.CSharp);
}
```
------------------
Visit [Wiki](https://github.com/melanchall/drymidi/wiki) to learn more.
