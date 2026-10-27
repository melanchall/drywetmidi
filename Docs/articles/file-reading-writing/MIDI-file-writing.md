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

Use [`MidiFile.WriteAsync`](xref:Melanchall.DryWetMidi.Core.MidiFile.WriteAsync*)
to await writing a complete [`MidiFile`](xref:Melanchall.DryWetMidi.Core.MidiFile)
to a file or stream. It accepts the same output format and
[`WritingSettings`](xref:Melanchall.DryWetMidi.Core.WritingSettings) options as
[`MidiFile.Write`](xref:Melanchall.DryWetMidi.Core.MidiFile.Write*):

```csharp
await file.WriteAsync("output.mid", overwriteFile: true);
```

This is a bulk operation over an existing file object, not lazy object streaming.
For writing events or timed objects incrementally, see
[Lazy reading/writing](xref:a_file_lazy_reading_writing).

Write with cancellation:

```csharp
using (var cancellation = new CancellationTokenSource())
{
    cancellation.CancelAfter(TimeSpan.FromSeconds(10));

    await file.WriteAsync(
        "output.mid",
        overwriteFile: true,
        cancellationToken: cancellation.Token);
}
```

File-path overloads own and dispose their streams. Stream overloads leave
caller-supplied streams open on completion or failure. Await writing before
disposing a supplied stream or starting another operation on it; do not mutate
the file while it is being written.

Cancellation raises `OperationCanceledException` and can leave partially written
output, as can other failures. Overwriting is not an atomic replacement and does
not roll back on failure. Encoding and custom chunk or event code still use CPU
time; asynchronous I/O does not guarantee background-thread execution or interrupt
arbitrary synchronous custom code. Custom chunks must implement their asynchronous
content methods; an override that throws `NotSupportedException` makes asynchronous
writing of that chunk fail. See [Custom chunks](xref:a_custom_chunk) for
asynchronous and synchronous-execution examples.

## Compression

You can set specific policies via [`WritingSettings`](xref:Melanchall.DryWetMidi.Core.WritingSettings) to reduce the size of an output file. For example, to use running status and thus not write status bytes of channel events of the same type, set the properties shown in the following code:

```csharp
file.Write("Some great song.mid", settings: new WritingSettings
{
    UseRunningStatus = true,
    NoteOffAsSilentNoteOn = true
});
```

Complete list of available properties is placed in the documentation of [`WritingSettings`](xref:Melanchall.DryWetMidi.Core.WritingSettings).
