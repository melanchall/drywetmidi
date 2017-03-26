# DryMIDI

DryMIDI is the .NET library to work with MIDI files. You need to understand MIDI file structure to effectively work with the library since it operates by low-level MIDI objects like *message* and *track chunk*.

The library is under MIT license so you can do whatever you want with it.

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

midiFile.Save("My Great Song Speeded.mid");
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
