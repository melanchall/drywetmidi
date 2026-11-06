---
uid: a_file_lazy_reading_writing
---

# Lazy reading/writing

## Reading

When you call the [`MidiFile.Read`](xref:Melanchall.DryWetMidi.Core.MidiFile.Read*) method, the entire structure of a MIDI file will be put to memory as an object of the [`MidiFile`](xref:Melanchall.DryWetMidi.Core.MidiFile) type. It means that a more or less big MIDI file will span a significant number of bytes in the memory. Although it's much easier to work with such fully initialized objects, memory consumption can be a critical parameter for your application.

To solve the problem DryWetMIDI provides [`MidiFile.ReadLazy`](xref:Melanchall.DryWetMidi.Core.MidiFile.ReadLazy*) methods which return an instance of the [`MidiTokensReader`](xref:Melanchall.DryWetMidi.Core.MidiTokensReader) class that can read a MIDI file sequentially token by token:

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

With this approach each call of [`ReadToken`](xref:Melanchall.DryWetMidi.Core.MidiTokensReader.ReadToken) method will read next portion of data from a MIDI file (in fact, the internal reader in DryWetMIDI uses the buffer of `4096` bytes by default, the size of this buffer can be adjusted via [`BufferSize`](xref:Melanchall.DryWetMidi.Core.ReaderSettings.BufferSize) property of [`ReadingSettings.ReaderSettings`](xref:Melanchall.DryWetMidi.Core.ReadingSettings.ReaderSettings)). So, the memory consumption will be almost always constant (despite a MIDI file size) and low. But of course, there's the price – slower reading and much more difficult implementation of some logic and algorithms needed for your application. Also, many high level tools provided by DryWetMIDI will be unavailable.

Some useful methods can be found in the [`MidiTokensReaderUtilities`](xref:Melanchall.DryWetMidi.Core.MidiTokensReaderUtilities) class. The following example shows how you can calculate the count of all _A#_ notes for each track chunk (the [`EnumerateObjects`](xref:Melanchall.DryWetMidi.Interaction.GetObjectsUtilities.EnumerateObjects*) method is also used):

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

## Writing

The same applies to the process of writing a MIDI file. [`MidiFile.Write`](xref:Melanchall.DryWetMidi.Core.MidiFile.Write*) requires an instance of the [`MidiFile`](xref:Melanchall.DryWetMidi.Core.MidiFile), which can obviously occupy a lot of memory for big files.

Here is an example of how to write MIDI data in a lazy way using [`MidiTokensWriter`](xref:Melanchall.DryWetMidi.Core.MidiTokensWriter):

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

DryWetMIDI also provides the [`TimedObjectsWriter`](xref:Melanchall.DryWetMidi.Interaction.TimedObjectsWriter) class which simplifies writing of complex objects with [`MidiTokensWriter`](xref:Melanchall.DryWetMidi.Core.MidiTokensWriter):

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

The code above writes one million notes to a MIDI file. Of course, you can combine [`MidiTokensReader`](xref:Melanchall.DryWetMidi.Core.MidiTokensReader), [`EnumerateObjects`](xref:Melanchall.DryWetMidi.Interaction.GetObjectsUtilities.EnumerateObjects*), [`MidiTokensWriter`](xref:Melanchall.DryWetMidi.Core.MidiTokensWriter) and [`TimedObjectsWriter`](xref:Melanchall.DryWetMidi.Interaction.TimedObjectsWriter) to process MIDI objects transforming a MIDI file into another one.

## Asynchronous reading

[`MidiFile.ReadLazyAsync`](xref:Melanchall.DryWetMidi.Core.MidiFile.ReadLazyAsync*) creates a token reader using the same initialization steps as [`MidiFile.ReadLazy`](xref:Melanchall.DryWetMidi.Core.MidiFile.ReadLazy*), awaiting stream reads where needed. Subsequent tokens are requested with [`MidiTokensReader.ReadTokenAsync`](xref:Melanchall.DryWetMidi.Core.MidiTokensReader.ReadTokenAsync*). Unlike the complete-file APIs described in [MIDI file reading](xref:a_file_reading), this does not first construct a complete [`MidiFile`](xref:Melanchall.DryWetMidi.Core.MidiFile):

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

Tokens are read as requested, rather than collected into a complete file. Collecting tokens or applying detection algorithms can still require additional memory.

### Consecutive events and object detection

[`EnumerateEventsAsync`](xref:Melanchall.DryWetMidi.Core.MidiTokensReaderUtilities.EnumerateEventsAsync(Melanchall.DryWetMidi.Core.MidiTokensReader)) returns an [`EnumerateEventsAsyncResult`](xref:Melanchall.DryWetMidi.Core.EnumerateEventsAsyncResult) without reading ahead. Its `Events` property is a single-use `IAsyncEnumerable<MidiEvent>` that reads consecutive events until a non-event token or EOF is encountered. After normal completion, `IsCompleted` is `true` and `NextToken` contains the already-consumed boundary token, or `null` at EOF. Process `NextToken` before requesting another token from the reader.

Early disposal does not drain the events or dispose the reader, and leaves `IsCompleted` as `false`. Cancellation and failures also leave it `false`; dispose the reader after a failed or canceled read. Do not interleave other reader operations with event enumeration. The synchronous [`EnumerateEvents`](xref:Melanchall.DryWetMidi.Core.MidiTokensReaderUtilities.EnumerateEvents(Melanchall.DryWetMidi.Core.MidiTokensReader)) result also exposes `IsCompleted`, which becomes `true` only when enumeration observes a boundary or EOF.

Feed the event sequence into [`EnumerateObjectsAsync`](xref:Melanchall.DryWetMidi.Interaction.GetObjectsUtilities.EnumerateObjectsAsync*) to detect timed events, notes, and chords without constructing a complete track. The detector awaits its source; detection itself is synchronous CPU work. Create a separate detection sequence for each track so that delta-time accumulation and pending notes do not cross track boundaries. The following example counts notes per track:

```csharp
await using var reader = await MidiFile.ReadLazyAsync(
    "input.mid",
    cancellationToken: cancellationToken);

MidiToken currentToken = await reader.ReadTokenAsync(cancellationToken);
var trackIndex = 0;

while (currentToken != null)
{
    if (currentToken is ChunkHeaderToken chunkHeader &&
        chunkHeader.ChunkId == TrackChunk.Id)
    {
        var result = reader.EnumerateEventsAsync();
        var notesCount = 0;

        await foreach (var timedObject in result.Events.EnumerateObjectsAsync(
            ObjectType.Note,
            cancellationToken: cancellationToken))
        {
            if (timedObject is Note)
                notesCount++;
        }

        Console.WriteLine($"Track {trackIndex}: {notesCount} notes");
        currentToken = result.NextToken;
        trackIndex++;
    }
    else
    {
        currentToken = await reader.ReadTokenAsync(cancellationToken);
    }
}
```

Object detection can retain pending notes, chords, and later objects to preserve output order. A long or unmatched note can delay output until the end of the track; lazy enumeration does not guarantee constant memory. Cancellation or source failure is not treated as normal EOF and does not flush pending objects. Track-local sequences do not merge different tracks into global chronological order.

The resulting `IAsyncEnumerable<ITimedObject>` can also be passed directly to [`TimedObjectsWriter.WriteObjectsAsync`](xref:Melanchall.DryWetMidi.Interaction.TimedObjectsWriter.WriteObjectsAsync*). The following example writes detected notes and other timed events to a new file, processing each track separately:

```csharp
await using var reader = await MidiFile.ReadLazyAsync(
    "input.mid",
    cancellationToken: cancellationToken);
await using var writer = await MidiFile.WriteLazyAsync(
    "output.mid",
    overwriteFile: true,
    cancellationToken: cancellationToken);

MidiToken currentToken = await reader.ReadTokenAsync(cancellationToken);

while (currentToken != null)
{
    if (currentToken is ChunkHeaderToken chunkHeader &&
        chunkHeader.ChunkId == TrackChunk.Id)
    {
        await writer.StartTrackChunkAsync(cancellationToken);
        var result = reader.EnumerateEventsAsync();

        await using (var objectsWriter = new TimedObjectsWriter(writer))
        {
            await objectsWriter.WriteObjectsAsync(
                result.Events.EnumerateObjectsAsync(
                    ObjectType.Note | ObjectType.TimedEvent,
                    cancellationToken: cancellationToken),
                cancellationToken);
        }

        await writer.EndTrackChunkAsync(cancellationToken);
        currentToken = result.NextToken;
    }
    else
    {
        currentToken = await reader.ReadTokenAsync(cancellationToken);
    }
}
```

The objects writer is asynchronously disposed before ending each output track so deferred events are flushed. This example skips non-track chunks and uses the default output time division; use the input `FileHeaderToken.TimeDivision` when the output must retain the input timing.

## Asynchronous writing

[`MidiFile.WriteLazyAsync`](xref:Melanchall.DryWetMidi.Core.MidiFile.WriteLazyAsync*) creates a [`MidiTokensWriter`](xref:Melanchall.DryWetMidi.Core.MidiTokensWriter) whose track and event operations can be awaited. [`TimedObjectsWriter.WriteObjectsAsync`](xref:Melanchall.DryWetMidi.Interaction.TimedObjectsWriter.WriteObjectsAsync*) accepts either `IEnumerable<ITimedObject>` or `IAsyncEnumerable<ITimedObject>`. Supply objects in non-descending time order:

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

Await [`TimedObjectsWriter.DisposeAsync`](xref:Melanchall.DryWetMidi.Interaction.TimedObjectsWriter.DisposeAsync) before ending the track: it emits deferred note-offs into the open track without disposing the caller-owned token writer. Subsequently await [`MidiTokensWriter.DisposeAsync`](xref:Melanchall.DryWetMidi.Core.MidiTokensWriter.DisposeAsync) to finish writing and finalize MIDI headers. The nested scopes above enforce this order.

### Sequential operations and disposal

Do not overlap operations on the same reader or writer, including synchronous operations while an asynchronous operation is outstanding. These objects retain mutable state and are not made thread-safe by asynchronous I/O.

**Wrong:** the synchronous read can race with the pending asynchronous read:

```csharp
var pendingRead = tokensReader.ReadTokenAsync();
var nextToken = tokensReader.ReadToken();

await pendingRead;
```

**Correct:** await each operation before starting the next:

```csharp
var firstToken = await tokensReader.ReadTokenAsync();
var nextToken = await tokensReader.ReadTokenAsync();
```

Disposal must also wait for pending work.

**Wrong:** disposal can race with output:

```csharp
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

File-path operations own their streams. Stream overloads leave supplied streams open when the token reader or writer is disposed; the caller must dispose them. Asynchronous output still requires a seekable stream to patch track lengths and track counts during finalization.

Custom meta events use their mandatory async content overrides during async event reading and writing. These receive the original reader or writer without an intermediate payload reader or writer. See [Custom meta events](xref:a_custom_meta_event) for implementations and ownership and cancellation guidance.

Pass cancellation tokens to both initialization and subsequent operations. For example, using a `CancellationToken` named `cancellationToken`:

```csharp
await using (var tokensReader = await MidiFile.ReadLazyAsync(
    "input.mid",
    cancellationToken: cancellationToken))
{
    MidiToken token;

    while ((token = await tokensReader.ReadTokenAsync(cancellationToken)) != null)
    {
        Console.WriteLine(token);
    }
}
```

Cancellation is cooperative and raises `OperationCanceledException`; it cannot interrupt arbitrary synchronous custom chunk or event code. After a canceled or failed token read, dispose the reader instead of resuming partially updated parsing state. Cancellation or failure during writing can leave partial output; neither rollback nor atomic replacement is implied.