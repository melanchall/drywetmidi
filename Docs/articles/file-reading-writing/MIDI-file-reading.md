---
uid: a_file_reading
---

# MIDI file reading

The simplest code for MIDI file reading is:

```csharp
var file = MidiFile.Read("Some great song.mid");
```

After that you have instance of the [MidiFile](xref:Melanchall.DryWetMidi.Core.MidiFile). Its [Chunks](xref:Melanchall.DryWetMidi.Core.MidiFile.Chunks) property returns collection of chunks within the MIDI file read. Using this collection you can manage chunks (add new, delete existing one and so on).

MIDI file reading process can be finely adjusted via [ReadingSettings](xref:Melanchall.DryWetMidi.Core.ReadingSettings). For example, if we want to abort reading on unknown chunk (about reading custom chunks see [Custom chunks](xref:a_custom_chunk) article), you can set corresponding policy:

```csharp
var file = MidiFile.Read("Some great song.mid", new ReadingSettings
{
    UnknownChunkIdPolicy = UnknownChunkIdPolicy.Abort
});
```

`ReadingSettings` has a lot of useful properties. You can read documentation on all of them to learn how you can adjust MIDI file reading.

## Reading corrupted files

DryWetMIDI allows to read MIDI files with various violations of [SMF](https://www.midi.org/specifications-old/category/smf-specifications) standard. Example below shows how to read a MIDI file with different errors:

```csharp
var file = MidiFile.Read("Some great song.mid", new ReadingSettings
{
    InvalidChannelEventParameterValuePolicy = InvalidChannelEventParameterValuePolicy.ReadValid,
    InvalidChunkSizePolicy = InvalidChunkSizePolicy.Ignore,
    InvalidMetaEventParameterValuePolicy = InvalidMetaEventParameterValuePolicy.SnapToLimits,
    MissedEndOfTrackPolicy = MissedEndOfTrackPolicy.Ignore,
    NoHeaderChunkPolicy = NoHeaderChunkPolicy.Ignore,
    NotEnoughBytesPolicy = NotEnoughBytesPolicy.Ignore,
    UnexpectedTrackChunksCountPolicy = UnexpectedTrackChunksCountPolicy.Ignore,
    UnknownChannelEventPolicy = UnknownChannelEventPolicy.SkipStatusByteAndOneDataByte,
    UnknownChunkIdPolicy = UnknownChunkIdPolicy.ReadAsUnknownChunk,
    UnknownFileFormatPolicy = UnknownFileFormatPolicy.Ignore,
});
```

Please read more about these properties in the API section to learn about what options are available to handle each error. If some policies set to `Abort`, an instance of corresponding exception will be thrown. All such exceptions types are derived from [MidiException](xref:Melanchall.DryWetMidi.Core.MidiException) and listed in _Exceptions_ section of `Read` methods ([by file path](xref:Melanchall.DryWetMidi.Core.MidiFile.Read(System.String,Melanchall.DryWetMidi.Core.ReadingSettings)) or [by stream](xref:Melanchall.DryWetMidi.Core.MidiFile.Read(System.IO.Stream,Melanchall.DryWetMidi.Core.ReadingSettings))) on API documentation.