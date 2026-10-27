---
uid: a_file_reading
---

# MIDI file reading

The simplest code for MIDI file reading is:

```csharp
var file = MidiFile.Read("Some great song.mid");
```

After that you have an instance of the [MidiFile](xref:Melanchall.DryWetMidi.Core.MidiFile). Its [Chunks](xref:Melanchall.DryWetMidi.Core.MidiFile.Chunks) property returns a collection of chunks within the MIDI file read. Using this collection, you can manage chunks (add new, delete existing one and so on).

MIDI file reading process can be finely adjusted via [ReadingSettings](xref:Melanchall.DryWetMidi.Core.ReadingSettings). For example, if we want to abort reading on unknown chunk (about reading custom chunks see [Custom chunks](xref:a_custom_chunk) article), you can set corresponding policy:

```csharp
var file = MidiFile.Read("Some great song.mid", new ReadingSettings
{
    UnknownChunkIdPolicy = UnknownChunkIdPolicy.Abort
});
```

`ReadingSettings` has a lot of useful properties. You can read documentation on all of them to learn how you can adjust MIDI file reading.

## Asynchronous reading

[`MidiFile.ReadAsync`](xref:Melanchall.DryWetMidi.Core.MidiFile.ReadAsync*) awaits
file or stream I/O and accepts the same
[`ReadingSettings`](xref:Melanchall.DryWetMidi.Core.ReadingSettings) as synchronous
reading. For example:

```csharp
var midiFile = await MidiFile.ReadAsync("input.mid");
```

This is a bulk operation: the task completes with a fully constructed
[`MidiFile`](xref:Melanchall.DryWetMidi.Core.MidiFile) in memory. Asynchronous I/O
changes how the application waits for data, not how much MIDI data the resulting
object holds. For sequential token processing without constructing the complete
file, see [Lazy reading/writing](xref:a_file_lazy_reading_writing#asynchronous-reading).

Transport reads are awaited, but parsing and custom chunk or event processing
still use CPU time. Asynchronous I/O does not guarantee execution on a background
thread. For CPU-bound object processing, see [Getting objects](xref:a_getting_objects).
Custom chunks can supply asynchronous content readers; see
[Custom chunks](xref:a_custom_chunk#asynchronous-content-io).

### Cancellation, progress, and stream ownership

For example, cancel reading after ten seconds and report transported bytes:

```csharp
using (var cancellation = new CancellationTokenSource())
{
    cancellation.CancelAfter(TimeSpan.FromSeconds(10));

    var progress = new Progress<AsyncOperationProgress>(update =>
    {
        Console.WriteLine($"{update.ProcessedItems} bytes read");

        if (update.IsCompleted)
        {
            Console.WriteLine("Reading completed.");
        }
    });

    var midiFile = await MidiFile.ReadAsync(
        "input.mid",
        progress: progress,
        cancellationToken: cancellation.Token);
}
```

Cancellation is cooperative and is reported with `OperationCanceledException`.
It cannot interrupt arbitrary synchronous custom chunk or event implementations.
Optional [`AsyncOperationProgress`](xref:Melanchall.DryWetMidi.Common.AsyncOperationProgress)
notifications count bytes read from the transport, not events or notes. Reports
are throttled, the total is unknown, and successful completion emits a final
notification with `IsCompleted` set to `true`. A `Progress<T>` handler may execute
later; await the read operation itself to determine when reading has finished.

File-path overloads own and dispose their file streams. Stream overloads leave
caller-supplied streams open; dispose those streams separately after awaiting the
operation. Do not overlap operations on the same stream or mutate settings while
they are in use. Asynchronous methods do not make mutable objects thread-safe.

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

Please read more about these properties in the API section to learn about what options are available to handle each error. If some policies are set to `Abort`, an instance of corresponding exception will be thrown. Types of all such exceptions are derived from [MidiException](xref:Melanchall.DryWetMidi.Common.MidiException) and listed in the _Exceptions_ section of `Read` methods ([by file path](xref:Melanchall.DryWetMidi.Core.MidiFile.Read(System.String,Melanchall.DryWetMidi.Core.ReadingSettings)) or [by stream](xref:Melanchall.DryWetMidi.Core.MidiFile.Read(System.IO.Stream,Melanchall.DryWetMidi.Core.ReadingSettings))) on API documentation.