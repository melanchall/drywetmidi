---
uid: a_file_async_reading_writing
---

# Async reading/writing

The examples use the `System`, `System.Threading`, `Melanchall.DryWetMidi.Common`,
`Melanchall.DryWetMidi.Core`, and `Melanchall.DryWetMidi.Interaction` namespaces.

## Reading and writing a complete file

[MidiFile.ReadAsync](xref:Melanchall.DryWetMidi.Core.MidiFile.ReadAsync*) and
[MidiFile.WriteAsync](xref:Melanchall.DryWetMidi.Core.MidiFile.WriteAsync*) provide
asynchronous file/stream I/O. Reading still constructs a complete
[MidiFile](xref:Melanchall.DryWetMidi.Core.MidiFile) in memory, just like
[MidiFile.Read](xref:Melanchall.DryWetMidi.Core.MidiFile.Read*).
Asynchronous I/O changes how the application waits for data, not how much MIDI
data the resulting object holds.

```csharp
var midiFile = await MidiFile.ReadAsync("input.mid");

await midiFile.WriteAsync("output.mid", overwriteFile: true);
```

Transport reads and writes are awaited, but parsing, formatting, and custom codecs
still use CPU time. Asynchronous I/O does not guarantee that all work runs on a
background thread. For CPU-bound object processing, see
[Getting objects](xref:a_getting_objects).

## Lazy token streaming

To process a file without first constructing a complete
[MidiFile](xref:Melanchall.DryWetMidi.Core.MidiFile), use
[MidiFile.ReadLazyAsync](xref:Melanchall.DryWetMidi.Core.MidiFile.ReadLazyAsync*)
and read tokens sequentially with
[MidiTokensReader.ReadTokenAsync](xref:Melanchall.DryWetMidi.Core.MidiTokensReader.ReadTokenAsync*):

```csharp
await using (var tokensReader = await MidiFile.ReadLazyAsync("input.mid"))
{
    MidiToken token;

    while ((token = await tokensReader.ReadTokenAsync()) != null)
    {
        Console.WriteLine(token);
    }
}
```

The reader holds its current buffers and token instead of a complete file object.
Consumers that collect the tokens or run detection algorithms may still need
additional memory. See [Lazy reading/writing](xref:a_file_lazy_reading_writing)
for the token model and its trade-offs.

[MidiFile.WriteLazyAsync](xref:Melanchall.DryWetMidi.Core.MidiFile.WriteLazyAsync*)
creates a [MidiTokensWriter](xref:Melanchall.DryWetMidi.Core.MidiTokensWriter)
with asynchronous track and event operations.
[TimedObjectsWriter.WriteObjectsAsync](xref:Melanchall.DryWetMidi.Interaction.TimedObjectsWriter.WriteObjectsAsync*)
accepts both ordinary and asynchronously produced object sequences. Objects must
still be supplied in non-descending time order.

```csharp
var notes = new[]
{
    new Note((SevenBitNumber)60, length: 120, time: 0),
    new Note((SevenBitNumber)64, length: 120, time: 120)
};

await using (var tokensWriter = await MidiFile.WriteLazyAsync(
    "output.mid",
    overwriteFile: true))
{
    await tokensWriter.StartTrackChunkAsync();

    await using (var objectsWriter = new TimedObjectsWriter(tokensWriter))
    {
        await objectsWriter.WriteObjectsAsync(notes);
    }

    await tokensWriter.EndTrackChunkAsync();
}
```

Await [TimedObjectsWriter.DisposeAsync](xref:Melanchall.DryWetMidi.Interaction.TimedObjectsWriter.DisposeAsync)
before ending the track so deferred note-offs are emitted into the open track.
The supplied token writer remains caller-owned; await
[MidiTokensWriter.DisposeAsync](xref:Melanchall.DryWetMidi.Core.MidiTokensWriter.DisposeAsync)
afterwards to flush remaining bytes and finalize MIDI headers. The nested
`await using` scopes above enforce this order.

## Sequential operations and disposal

Do not overlap operations on the same reader or writer, including synchronous
operations while an asynchronous operation is outstanding. These objects keep
mutable state; asynchronous methods do not make them thread-safe.

**Wrong:** the second read can start before the first finishes.

```csharp
await using (var tokensReader = await MidiFile.ReadLazyAsync("input.mid"))
{
    var pendingRead = tokensReader.ReadTokenAsync();
    var nextToken = tokensReader.ReadToken();

    await pendingRead;
}
```

**Correct:** await each operation before starting the next.

```csharp
await using (var tokensReader = await MidiFile.ReadLazyAsync("input.mid"))
{
    var firstToken = await tokensReader.ReadTokenAsync();
    var nextToken = await tokensReader.ReadTokenAsync();
}
```

Disposal is also an operation and must not overlap pending work.

**Wrong:** disposing the writer can race with output that is still in progress.

```csharp
var tokensWriter = await MidiFile.WriteLazyAsync("output.mid", overwriteFile: true);
await tokensWriter.StartTrackChunkAsync();

var pendingWrite = tokensWriter.WriteEventAsync(new TextEvent("Hello"));
await tokensWriter.DisposeAsync();

await pendingWrite;
```

**Correct:** finish writing before leaving the asynchronous disposal scope.

```csharp
await using (var tokensWriter = await MidiFile.WriteLazyAsync(
    "output.mid",
    overwriteFile: true))
{
    await tokensWriter.StartTrackChunkAsync();
    await tokensWriter.WriteEventAsync(new TextEvent("Hello"));
    await tokensWriter.EndTrackChunkAsync();
}
```

## Ownership, cancellation, and failures

File-path operations own their file streams. Stream overloads leave caller-supplied
streams open; the caller must dispose those streams separately.

Asynchronous output does not remove lazy writing's seekability requirements.
Track lengths and track counts must be patched during finalization. Cancellation
or failure can leave partially written data; neither rollback nor atomic replacement
is implied. After a failed or canceled token read, dispose the reader rather than
trying to resume from partially updated codec state.

Pass a cancellation token to cancel I/O cooperatively:

```csharp
using (var cancellation = new CancellationTokenSource())
{
    cancellation.CancelAfter(TimeSpan.FromSeconds(10));

    var midiFile = await MidiFile.ReadAsync(
        "input.mid",
        cancellationToken: cancellation.Token);
}
```

Cancellation cannot interrupt arbitrary synchronous custom codec code.
