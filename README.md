# DryMIDI
DryMIDI is the .NET library to work with MIDI files. You need to understand MIDI file structure to effectively work with the library since it operates by low-level MIDI objects like message and track chunk. You will not find here any high-level entities like note or chord. I plan to implement this logic in another library in the future.

## Main Features

1. **Flexible reading and writing MIDI files.** DryMIDI has a lot of options in order to give you full control under reading and writing MIDI files. For example, you can choose format of the file being saved or reaction on some unexpected situations like missing End of Track message at the end of track chunk.
1. **It's fast.** All MIDI files I've tested on my far-from-top computer were read by DryMIDI for ~30ms for multitrack files and for ~10ms for singletrack ones. Writing process is slower: ~150ms for singletrack files and ~100ms for multitrack ones.

## Getting Started

It's really easy to read an existing MIDI file. 

```csharp
using System.Linq;
using Melanchall.DryMidi;

var midiFile = MidiFile.Load("My Great Song.mid");

// Get all track chunks in the file ..
var trackChunks = midiFile.Chunks.OfType<TrackChunk>();

// .. and know their names
var trackNames = trackChunks.Select(track => track.Messages.OfType<SequenceTrackNameMessage>()
                                                           .FirstOrDefault()
                                                           ?.Text);
```
