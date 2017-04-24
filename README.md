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

To reading a MIDI file you have to use ```Read``` static method of the ```MidiFile```:

```csharp
var midiFile = MidiFile.Read("My Great Song.mid");
```

or

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

To write MIDI data to a file you have to use ```Write``` method of the ```MidiFile```:

```csharp
midiFile.Write("My Great Song speeded.mid");
```

or

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
var midiFile = new MidiFile();

var trackChunk = new TrackChunk();
trackChunk.Messages.Add(new TextMessage("It's just empty track..."));

midiFile.Chunks.Add(trackChunk);
midiFile.Write("My Future Great Song.mid");
```

If you want to speed up playing back a MIDI file by two times you can do it with this code:

```csharp                   
foreach (var trackChunk in midiFile.Chunks.OfType<TrackChunk>())
{
    foreach (var setTempoMessage in trackChunk.Messages.OfType<SetTempoMessage>())
    {
        setTempoMessage.MicrosecondsPerBeat /= 2;
    }
}
```

Of course this code is simplified. In practice a MIDI file may not contain SetTempo message which means it has the default one (500000 microseconds per beat).

Suppose you want to remove all *C#* notes from a MIDI file. It can be done with this code:

```csharp
foreach (var trackChunk in midiFile.Chunks.OfType<TrackChunk>())
{
    trackChunk.Messages.RemoveAll(m => (m as NoteOnMessage)?.GetNoteLetter() == NoteLetter.CSharp ||
                                       (m as NoteOffMessage)?.GetNoteLetter() == NoteLetter.CSharp);
}
```
------------------
Visit [Wiki](https://github.com/melanchall/drymidi/wiki) to learn more.
