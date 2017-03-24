# DryMIDI

DryMIDI is the .NET library to work with MIDI files. You need to understand MIDI file structure to effectively work with the library since it operates by low-level MIDI objects like message and track chunk. You will not find here any high-level entities like note or chord. I plan to implement this logic in another library in the future.

The library is under MIT license so you can do whatever you want with it. DryMIDI is written in C# 6 so if you want to modify it you need an IDE that supports this standard. Of course until you want to rewrite it in C# of another version :)

## Main Features

#### Flexible reading and writing MIDI files

DryMIDI has a lot of options in order to give you full control under reading and writing MIDI files. For example, you can choose format of the file being saved or reaction on some unexpected situations like missing End of Track message at the end of track chunk. This also gives you ability to read corrupted files.

#### Files customization

You can define your own chunks and meta messages which can be written to a file and read from it. So you have the ability to easily create new MIDI-based file format for your purposes.

## Getting Started

`Load` static method of the `MidiFile` class is all you need to read an existing MIDI file. After that you can work with the content of the file through chunks and messages.

```csharp
// Read the file ...
var midiFile = MidiFile.Load("My Great Song.mid");

// ... and get all track chunks in it ...
var trackChunks = midiFile.Chunks.OfType<TrackChunk>();

// ... and know their names
var trackNames = trackChunks.Select(track => track.Messages.OfType<SequenceTrackNameMessage>()
                                                           .FirstOrDefault()
                                                           ?.Text);
```

You may want to try to read a MIDI file containing corrupted data. It can be done by passing an instance of the `ReadingSettings` class to the `Load` method.

```csharp
// Read the file ignoring difference between actual chunk's size and the declared one
var midiFile = MidiFile.Load("My Another Great Song.mid",
                             new ReadingSettings
                             {
                                 InvalidChunkSizePolicy = InvalidChunkSizePolicy.Ignore                    
                             });
```
