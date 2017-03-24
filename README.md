# DryMIDI

DryMIDI is the .NET library to work with MIDI files. You need to understand MIDI file structure to effectively work with the library since it operates by low-level MIDI objects like message and track chunk. You will not find here any high-level entities like note or chord.

The library is under MIT license so you can do whatever you want with it.

## Main Features

#### Flexible reading and writing MIDI files

DryMIDI has a lot of options in order to give you full control under reading and writing MIDI files. For example, you can choose format of the file being saved or reaction on some unexpected situations like missing End of Track message at the end of track chunk. This also gives you ability to read corrupted files.

#### Files customization

You can define your own chunks and meta messages which can be written to a file and read from it. So you have the ability to easily create new MIDI-based file format for your purposes.

## Getting Started

`Load` static method of the `MidiFile` class is all you need to read an existing MIDI file. After that you can work with the content of the file through chunks and messages.

For example, if you want to speed up a MIDI file by two times you can do it with this code:

```csharp
var midiFile = MidiFile.Load("My Great Song.mid");
                             
foreach (var trackChunk in midiFile.Chunks.OfType<TrackChunk>())
{
    foreach (var setTempoMessage in trackChunk.Messages.OfType<SetTempoMessage>())
    {
        setTempoMessage.MicrosecondsPerBeat /= 2;
    }
}
```
