---
uid: a_file_writing
---

# MIDI file writing

The simplest code for MIDI file writing is:

```csharp
file.Write("Some great song.mid");
```

If a file with this name already exists, you'll get an exception. To overwrite existing file pass `true` to `overwriteFile` parameter:

```csharp
file.Write("Some great song.mid", overwriteFile: true);
```

## Asynchronous writing

[`MidiFile.WriteAsync`](xref:Melanchall.DryWetMidi.Core.MidiFile.WriteAsync*) awaits
file or stream I/O and supports the same
[`WritingSettings`](xref:Melanchall.DryWetMidi.Core.WritingSettings) as synchronous
writing. For example, read a complete file and write it to another path:

```csharp
var midiFile = await MidiFile.ReadAsync("input.mid");

await midiFile.WriteAsync("output.mid", overwriteFile: true);
```

This is a bulk API: await the task to finish writing the file. It requires a
[`MidiFile`](xref:Melanchall.DryWetMidi.Core.MidiFile) in memory; asynchronous I/O
does not make the object model lazy or reduce its memory use. For token-based
output, see [Lazy reading/writing](xref:a_file_lazy_reading_writing#asynchronous-writing).

Transport writes are awaited, but formatting and custom chunk or event processing
still use CPU time. Asynchronous I/O does not guarantee execution on a background
thread. Custom chunks can supply asynchronous content writers; see
[Custom chunks](xref:a_custom_chunk#asynchronous-content-io).

### Cancellation, progress, and stream ownership

For example, cancel writing after ten seconds and report transported bytes:

```csharp
using (var cancellation = new CancellationTokenSource())
{
    cancellation.CancelAfter(TimeSpan.FromSeconds(10));

    var progress = new Progress<AsyncOperationProgress>(update =>
    {
        Console.WriteLine($"{update.ProcessedItems} bytes written");

        if (update.IsCompleted)
        {
            Console.WriteLine("Writing completed.");
        }
    });

    await midiFile.WriteAsync(
        "output.mid",
        overwriteFile: true,
        progress: progress,
        cancellationToken: cancellation.Token);
}
```

Cancellation is cooperative and is reported with `OperationCanceledException`.
It cannot interrupt arbitrary synchronous custom chunk or event implementations.
Cancellation or failure can leave partially written data; neither rollback nor
atomic replacement is implied.

Optional [`AsyncOperationProgress`](xref:Melanchall.DryWetMidi.Common.AsyncOperationProgress)
notifications count bytes written to the transport, not events or notes. Reports
are throttled, the total is unknown, and successful completion emits a final
notification after output has been flushed. A `Progress<T>` handler may execute
later; await the write operation itself to determine when writing has finished.

File-path overloads own and dispose their file streams. Stream overloads leave
caller-supplied streams open; dispose those streams separately after awaiting the
operation. Do not overlap operations on the same stream, dispose it during pending
work, or mutate the file, its chunks, events, or settings while writing uses them.
Asynchronous methods do not make mutable objects thread-safe.

## Compression

You can set specific policies via [WritingSettings](xref:Melanchall.DryWetMidi.Core.WritingSettings) to reduce the size of an output file. For example, to use running status and thus not write status bytes of channel events of the same type, set the properties shown in the following code:

```csharp
file.Write("Some great song.mid", settings: new WritingSettings
{
    UseRunningStatus = true,
    NoteOffAsSilentNoteOn = true
});
```

Complete list of available properties is placed in the documentation of [WritingSettings](xref:Melanchall.DryWetMidi.Core.WritingSettings).
