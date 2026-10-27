---
uid: a_file_reading
---

# MIDI file reading

The simplest code for MIDI file reading is:

```csharp
var file = MidiFile.Read("Some great song.mid");
```

After that you have an instance of the [`MidiFile`](xref:Melanchall.DryWetMidi.Core.MidiFile). Its [`Chunks`](xref:Melanchall.DryWetMidi.Core.MidiFile.Chunks) property returns a collection of chunks within the MIDI file read. Using this collection, you can manage chunks (add new, delete existing one and so on).

MIDI file reading process can be finely adjusted via [`ReadingSettings`](xref:Melanchall.DryWetMidi.Core.ReadingSettings). For example, if we want to abort reading on unknown chunk (about reading custom chunks see [Custom chunks](xref:a_custom_chunk) article), you can set corresponding policy:

```csharp
var file = MidiFile.Read("Some great song.mid", new ReadingSettings
{
    UnknownChunkIdPolicy = UnknownChunkIdPolicy.Abort
});
```

`ReadingSettings` has a lot of useful properties. You can read documentation on all of them to learn how you can adjust MIDI file reading.

## Asynchronous reading

Use [`MidiFile.ReadAsync`](xref:Melanchall.DryWetMidi.Core.MidiFile.ReadAsync*)
to await file or stream I/O. Like
[`MidiFile.Read`](xref:Melanchall.DryWetMidi.Core.MidiFile.Read*), it constructs a
complete [`MidiFile`](xref:Melanchall.DryWetMidi.Core.MidiFile) in memory before
returning; it does not yield chunks or events lazily. For token-by-token processing,
see [Lazy reading/writing](xref:a_file_lazy_reading_writing).

Read a file with cancellation:

```csharp
using (var cancellation = new CancellationTokenSource())
{
    cancellation.CancelAfter(TimeSpan.FromSeconds(10));

    var file = await MidiFile.ReadAsync(
        "input.mid",
        settings: new ReadingSettings
        {
            UnknownChunkIdPolicy = UnknownChunkIdPolicy.Abort
        },
        cancellationToken: cancellation.Token);
}
```

File-path overloads own and dispose their streams. Stream overloads leave
caller-supplied streams open on completion or failure, so dispose them separately.
Cancellation is cooperative and raises `OperationCanceledException`; it cannot
interrupt arbitrary synchronous custom chunk or event code. Parsing still uses
CPU time, and asynchronous I/O does not move all work to a background thread.
Custom chunks must implement their asynchronous content methods; an override
that throws `NotSupportedException` makes asynchronous reading of that chunk fail.
See [Custom chunks](xref:a_custom_chunk) for asynchronous and synchronous-execution
examples.

## Reading corrupted files

DryWetMIDI allows to read MIDI files with various violations of [SMF](https://midi.org/standard-midi-files-specification) standard. Example below shows how to read a MIDI file with different errors:

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

Please read more about these properties in the API section to learn about what options are available to handle each error. If some policies are set to `Abort`, an instance of corresponding exception will be thrown. Types of all such exceptions are derived from [`MidiException`](xref:Melanchall.DryWetMidi.Common.MidiException) and listed in the _Exceptions_ section of `Read` methods ([by file path](xref:Melanchall.DryWetMidi.Core.MidiFile.Read(System.String,Melanchall.DryWetMidi.Core.ReadingSettings)) or [by stream](xref:Melanchall.DryWetMidi.Core.MidiFile.Read(System.IO.Stream,Melanchall.DryWetMidi.Core.ReadingSettings))) on API documentation.