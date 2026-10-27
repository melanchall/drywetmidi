---
uid: a_file_lazy_reading_writing
---

# Lazy reading/writing

For bulk asynchronous I/O that uses a complete `MidiFile`, see
[MIDI file reading](xref:a_file_reading#asynchronous-reading) and
[MIDI file writing](xref:a_file_writing#asynchronous-writing). Lazy APIs instead
perform work as tokens or objects are requested or supplied.

## Reading

When you call the [MidiFile.Read](xref:Melanchall.DryWetMidi.Core.MidiFile.Read*) method, the entire structure of a MIDI file will be put to memory as an object of the [MidiFile](xref:Melanchall.DryWetMidi.Core.MidiFile) type. It means that a more or less big MIDI file will span a significant number of bytes in the memory. Although it's much easier to work with such fully initialized objects, memory consumption can be a critical parameter for your application.

To solve the problem DryWetMIDI provides [MidiFile.ReadLazy](xref:Melanchall.DryWetMidi.Core.MidiFile.ReadLazy*) methods which return an instance of the [MidiTokensReader](xref:Melanchall.DryWetMidi.Core.MidiTokensReader) class that can read a MIDI file sequentially token by token:

```csharp
using (var tokensReader = MidiFile.ReadLazy("test.mid"))
{
    for (MidiToken token; (token = tokensReader.ReadToken()) != null;)
    {
        switch (token)
        {
            case FileHeaderToken fileHeaderToken:
                Console.WriteLine($"File header: {fileHeaderToken}");
                break;
            case ChunkHeaderToken chunkHeaderToken:
                Console.WriteLine($"Chunk {chunkHeaderToken.ChunkId}");
                break;
            case MidiEventToken midiEventToken:
                Console.WriteLine($"Event {midiEventToken.Event}");
                break;
            case BytesPacketToken bytesPacketToken:
                Console.WriteLine($"Part of chunk with length {bytesPacketToken.Data.Length}");
                break;
        }
    }
}
```

With this approach each call of [ReadToken](xref:Melanchall.DryWetMidi.Core.MidiTokensReader.ReadToken) method will read next portion of data from a MIDI file (in fact, the internal reader in DryWetMIDI uses the buffer of `4096` bytes by default, the size of this buffer can be adjusted via [BufferSize](xref:Melanchall.DryWetMidi.Core.ReaderSettings.BufferSize) property of [ReadingSettings.ReaderSettings](xref:Melanchall.DryWetMidi.Core.ReadingSettings.ReaderSettings)). So, the memory consumption will be almost always constant (despite a MIDI file size) and low. But of course, there's the price – slower reading and much more difficult implementation of some logic and algorithms needed for your application. Also, many high level tools provided by DryWetMIDI will be unavailable.

Some useful methods can be found in the [MidiTokensReaderUtilities](xref:Melanchall.DryWetMidi.Core.MidiTokensReaderUtilities) class. The following example shows how you can calculate the count of all _A#_ notes for each track chunk (the [EnumerateObjects](xref:Melanchall.DryWetMidi.Interaction.GetObjectsUtilities.EnumerateObjects*) method is also used):

```csharp
bool IsTrackChunkHeaderToken(MidiToken token) =>
    token is ChunkHeaderToken chunkHeaderToken &&
    chunkHeaderToken.ChunkId == TrackChunk.Id;

bool ReadUntilTrackChunk(MidiTokensReader reader, MidiToken currentToken)
{
    if (IsTrackChunkHeaderToken(currentToken))
        return true;

    foreach (var token in reader.EnumerateTokens())
    {
        if (IsTrackChunkHeaderToken(token))
            return true;
    }

    return false;
}

using (var tokensReader = MidiFile.ReadLazy("test.mid"))
{
    MidiToken currentToken = null;
    var i = 0;

    while (true)
    {
        if (!ReadUntilTrackChunk(tokensReader, currentToken))
            break;

        var result = tokensReader.EnumerateEvents();
        var notes = result.Events.EnumerateObjects(ObjectType.Note).OfType<Note>();
        var aSharpCount = notes.Count(n => n.NoteName == Melanchall.DryWetMidi.MusicTheory.NoteName.ASharp);
        Console.WriteLine($"Track chunk {i}: {aSharpCount} A# notes");

        currentToken = result.NextToken;
        i++;
    }
}
```

The reader holds its current buffers and token instead of a complete file object.
Consumers that collect tokens or run detection algorithms may still need additional
memory.

In comparison, the standard approach takes far fewer lines of code (but takes much more memory, of course):

```csharp
var midiFile = MidiFile.Read("test.mid");
var i = 0;

foreach (var trackChunk in midiFile.GetTrackChunks())
{
    var notes = trackChunk.GetNotes();
    var aSharpCount = notes.Count(n => n.NoteName == Melanchall.DryWetMidi.MusicTheory.NoteName.ASharp);
    Console.WriteLine($"Track chunk {i}: {aSharpCount} A# notes");

    i++;
}
```

### Asynchronous reading

[`MidiFile.ReadLazyAsync`](xref:Melanchall.DryWetMidi.Core.MidiFile.ReadLazyAsync*)
opens an asynchronous token reader. Await
[`MidiTokensReader.ReadTokenAsync`](xref:Melanchall.DryWetMidi.Core.MidiTokensReader.ReadTokenAsync*)
to request each token; opening the reader does not materialize a complete file:

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

Pass a cancellation token both when opening the reader and to subsequent read
operations as needed. After a failed or canceled token read, dispose the reader
rather than trying to resume from partially updated chunk or event reading state.
Cancellation cannot interrupt arbitrary synchronous custom chunk or event code.

Lazy reading returns nonstandard chunk content as
[`BytesPacketToken`](xref:Melanchall.DryWetMidi.Core.BytesPacketToken) objects; it
does not construct registered custom chunk instances or invoke their content
readers. To deserialize custom chunks, use
[`MidiFile.ReadAsync`](xref:Melanchall.DryWetMidi.Core.MidiFile.ReadAsync*) and
the hooks described in [Custom chunks](xref:a_custom_chunk#asynchronous-content-io).

## Writing

The same applies to the process of writing a MIDI file. [MidiFile.Write](xref:Melanchall.DryWetMidi.Core.MidiFile.Write*) requires an instance of the [MidiFile](xref:Melanchall.DryWetMidi.Core.MidiFile), which can obviously occupy a lot of memory for big files.

Here is an example of how to write MIDI data in a lazy way using [MidiTokensWriter](xref:Melanchall.DryWetMidi.Core.MidiTokensWriter):

```csharp
using (var tokensWriter = MidiFile.WriteLazy("test.mid", true))
{
    tokensWriter.StartTrackChunk();
    tokensWriter.WriteEvent(
        new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue));
    tokensWriter.WriteEvent(
        new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue));
    tokensWriter.EndTrackChunk();

    tokensWriter.StartTrackChunk();
    tokensWriter.WriteEvent(
        new TextEvent("A"));
    tokensWriter.EndTrackChunk();
}
```

DryWetMIDI also provides the [TimedObjectsWriter](xref:Melanchall.DryWetMidi.Interaction.TimedObjectsWriter) class which simplifies writing of complex objects with [MidiTokensWriter](xref:Melanchall.DryWetMidi.Core.MidiTokensWriter):

```csharp
IEnumerable<ITimedObject> GetObjects() => Enumerable
    .Range(0, 1_000_000)
    .Select((_, i) => new Note((SevenBitNumber)(i % SevenBitNumber.MaxValue), 2 * i, i));

using (var tokensWriter = MidiFile.WriteLazy("test.mid", true))
using (var objectsWriter = new TimedObjectsWriter(tokensWriter))
{
    objectsWriter.WriteObjects(GetObjects());
}
```

The code above writes one million notes to a MIDI file. Of course, you can combine [MidiTokensReader](xref:Melanchall.DryWetMidi.Core.MidiTokensReader), [EnumerateObjects](xref:Melanchall.DryWetMidi.Interaction.GetObjectsUtilities.EnumerateObjects*), [MidiTokensWriter](xref:Melanchall.DryWetMidi.Core.MidiTokensWriter) and [TimedObjectsWriter](xref:Melanchall.DryWetMidi.Interaction.TimedObjectsWriter) to process MIDI objects transforming a MIDI file into another one.

### Asynchronous writing

[`MidiFile.WriteLazyAsync`](xref:Melanchall.DryWetMidi.Core.MidiFile.WriteLazyAsync*)
creates a [`MidiTokensWriter`](xref:Melanchall.DryWetMidi.Core.MidiTokensWriter)
with asynchronous track and event operations.
[`TimedObjectsWriter.WriteObjectsAsync`](xref:Melanchall.DryWetMidi.Interaction.TimedObjectsWriter.WriteObjectsAsync*)
accepts both ordinary and asynchronously produced object sequences. Objects must
still be supplied in non-descending time order:

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

Await [`TimedObjectsWriter.DisposeAsync`](xref:Melanchall.DryWetMidi.Interaction.TimedObjectsWriter.DisposeAsync)
before ending the track so deferred note-offs are emitted into the open track.
The supplied token writer remains caller-owned; await
[`MidiTokensWriter.DisposeAsync`](xref:Melanchall.DryWetMidi.Core.MidiTokensWriter.DisposeAsync)
afterwards to flush remaining bytes and finalize MIDI headers. The nested
`await using` scopes above enforce this order.

Optional [`AsyncOperationProgress`](xref:Melanchall.DryWetMidi.Common.AsyncOperationProgress)
notifications from `WriteObjectsAsync` count accepted non-null input objects, not
emitted MIDI events. Reports are throttled, totals can be unknown without
pre-enumerating the input, and successful completion emits a final notification.
Completion of a `WriteObjectsAsync` call does not replace disposal: deferred
note-offs still need to be written before the track ends.

### Sequential operations and disposal

Do not overlap operations on the same reader or writer, including synchronous
operations while an asynchronous operation is outstanding. These objects keep
mutable state; asynchronous methods do not make them thread-safe.

**Wrong:** the second read can start before the first finishes:

```csharp
await using (var tokensReader = await MidiFile.ReadLazyAsync("input.mid"))
{
    var pendingRead = tokensReader.ReadTokenAsync();
    var nextToken = tokensReader.ReadToken();

    await pendingRead;
}
```

**Correct:** await each operation before starting the next:

```csharp
await using (var tokensReader = await MidiFile.ReadLazyAsync("input.mid"))
{
    var firstToken = await tokensReader.ReadTokenAsync();
    var nextToken = await tokensReader.ReadTokenAsync();
}
```

Disposal is also an operation and must not overlap pending work.

**Wrong:** disposing the writer can race with output that is still in progress:

```csharp
var tokensWriter = await MidiFile.WriteLazyAsync("output.mid", overwriteFile: true);
await tokensWriter.StartTrackChunkAsync();

var pendingWrite = tokensWriter.WriteEventAsync(new TextEvent("Hello"));
await tokensWriter.DisposeAsync();

await pendingWrite;
```

**Correct:** finish writing before leaving the asynchronous disposal scope:

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

### Ownership, cancellation, and failures

File-path operations own their file streams. Stream overloads leave caller-supplied
streams open; the caller must dispose those streams separately after the reader
or writer is finished.

Pass cancellation tokens to the factory and subsequent asynchronous operations
as needed; cancellation is cooperative and is reported with
`OperationCanceledException`. Opening a reader or writer with a token does not
replace passing a token to later operations.

Asynchronous output does not remove lazy writing's seekability requirements.
Track lengths and track counts must be patched during finalization. Cancellation
or failure can leave partially written data; neither rollback nor atomic replacement
is implied. Await pending work before disposing the writer, including after an
operation fails.