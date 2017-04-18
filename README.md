# DryMIDI

DryMIDI is the .NET library to work with MIDI files. You need to understand MIDI file structure to effectively work with the library since it operates by low-level MIDI objects like *message* and *track chunk*. Visit [The MIDI Association site](https://www.midi.org) to get more information about MIDI.

The library is under MIT license so you can do whatever you want with it.

## Features

* DryMIDI provides core functionality to read and write [standard MIDI files](https://www.midi.org/specifications/category/smf-specifications) giving you set of basic MIDI objects: ```MidiFile```, ```Message```, ```Chunk``` (see examples below of how you can use these objects).
* The library gives you ability to implement custom meta messages (by deriving from the ```MetaMessage```) and custom chunks (by deriving from the ```Chunk```).
* Process of reading or writing can be finely adjusted with help of ```ReadingSettings``` and ```WritingSettings```. It allows, for example, to read files with some corruptions like mismatch of actual track chunk's length and expected one written in the chunk's header.
* All possible errors in a MIDI file are presented in the DryMIDI as separate exception classes so you can easily catch specific error. Some of these exceptions are thrown only if specific option is set in an instance of the ```ReadingSettings``` used to read the file.

## Getting Started

Let's see some examples of what you can do with DryMIDI.

If you want to speed up playing back a MIDI file by two times you can do it with this code:

```csharp
var midiFile = MidiFile.Load("My Great Song.mid");
                             
foreach (var trackChunk in midiFile.Chunks.OfType<TrackChunk>())
{
    foreach (var setTempoMessage in trackChunk.Messages.OfType<SetTempoMessage>())
    {
        setTempoMessage.MicrosecondsPerBeat /= 2;
    }
}

midiFile.Save("My Great Song speeded.mid");
```

Of course this code is simplified. In practice a MIDI file may not contain SetTempo message which means it has the default one (500000 microseconds per beat).

You can try to minimize size of a MIDI file with this code:

```csharp
var midiFile = MidiFile.Load("My Great Song.mid");

midiFile.Save("My Great Song.mid",
              true,
              MidiFileFormat.SingleTrack,
              new WritingSettings
              {
                  CompressionPolicy = CompressionPolicy.Default
              });
```

Using of the `CompressionPolicy.Default` option doesn't lead to losing of any data, so any unknown chunks and meta messages will be presented in the file.

And another one example. Suppose you want to remove all *C#* notes from a MIDI file. It can be done with this code:

```csharp
var midiFile = MidiFile.Load("My Great Song.mid");

foreach (var trackChunk in midiFile.Chunks.OfType<TrackChunk>())
{
    trackChunk.Messages.RemoveAll(m => (m as NoteOnMessage)?.GetNoteLetter() == NoteLetter.CSharp ||
                                       (m as NoteOffMessage)?.GetNoteLetter() == NoteLetter.CSharp);
}
            
midiFile.Save("My Great Song without C Sharp notes.mid");
```
------------------
Visit [Wiki](https://github.com/melanchall/drymidi/wiki) to learn more.
