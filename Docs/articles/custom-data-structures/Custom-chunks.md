---
uid: a_custom_chunk
---

# Custom chunks

MIDI files are made up of **chunks**. Each chunk has a 4-character ID and a 32-bit length, which is the number of bytes in the chunk. This structure allows future or custom chunk types to be designed which may be easily ignored if encountered by a program written before a chunk type is introduced or if the program doesn't know about the type. DryWetMIDI allows you to implement custom chunks which can be written to a MIDI file and be read from it.

For example, we want to design a chunk that will contain information about changes in whatever we want. A change is described by **date** (day, month, year) and **comment**. Let's create a class to store a single change.

```csharp
public sealed class Change
{
    public Change(DateTime date, string comment)
    {
        Date = date;
        Comment = comment;
    }

    public DateTime Date { get; }

    public string Comment { get; }
}
```

Now we are going to implement a custom chunk. Custom chunk class must be derived from the [MidiChunk](xref:Melanchall.DryWetMidi.Core.MidiChunk) and must implement four abstract methods:

* [ReadContent](xref:Melanchall.DryWetMidi.Core.MidiChunk.ReadContent(Melanchall.DryWetMidi.Core.MidiReader,Melanchall.DryWetMidi.Core.ReadingSettings,System.UInt32));
* [WriteContent](xref:Melanchall.DryWetMidi.Core.MidiChunk.WriteContent(Melanchall.DryWetMidi.Core.MidiWriter,Melanchall.DryWetMidi.Core.WritingSettings));
* [GetContentSize](xref:Melanchall.DryWetMidi.Core.MidiChunk.GetContentSize(Melanchall.DryWetMidi.Core.WritingSettings));
* [Clone](xref:Melanchall.DryWetMidi.Core.MidiChunk.Clone).

Also, the class must have a parameterless constructor which calls the constructor of the base class ([MidiChunk](xref:Melanchall.DryWetMidi.Core.MidiChunk)) passing chunk's ID to it. ID is a 4-character string which will be **Hstr** for our chunk. The ID of a custom chunk should not be the same as one of standard chunks IDs.

The class will look like this:

```csharp
public sealed class HistoryChunk : MidiChunk
{
    private const string Id = "Hstr";

    private readonly List<Change> _changes = new List<Change>();

    public HistoryChunk()
        : base(Id)
    {
    }

    public void AddChange(DateTime dateTime, string comment)
    {
        _changes.Add(new Change(dateTime, comment));
    }

    protected override void ReadContent(MidiReader reader, ReadingSettings settings, uint size)
    {
        throw new NotImplementedException();
    }

    protected override void WriteContent(MidiWriter writer, WritingSettings settings)
    {
        throw new NotImplementedException();
    }

    protected override uint GetContentSize(WritingSettings settings)
    {
        throw new NotImplementedException();
    }

    public override MidiChunk Clone()
    {
        throw new NotImplementedException();
    }
}
```

Before we start to implement the four methods mentioned above, we need to determine the structure of change records according to which it should be read and written.

Chunk's content will start with the count of changes. We will write this count as a [variable-length quantity](https://en.wikipedia.org/wiki/Variable-length_quantity) (VLQ) number. The count is followed by change records.

Each change is:

* one byte for **day**;
* one byte for **month**;
* two bytes for **year**;
* VLQ number bytes representing size of bytes array which is encoded comment;
* bytes which represent an encoded comment string.

To store comments, we will use [](xref:System.Text.Encoding.Unicode?title=Encoding.Unicode) encoding.

Let's implement the `ReadContent` method:

```csharp
protected override void ReadContent(MidiReader reader, ReadingSettings settings, uint size)
{
    // Read changes count

    var changesCount = reader.ReadVlqNumber();

    for (int i = 0; i < changesCount; i++)
    {
        // Read date

        var day = reader.ReadByte();
        var month = reader.ReadByte();
        var year = reader.ReadWord(); // ushort

        // Read comment

        var commentLength = reader.ReadVlqNumber();
        var commentBytes = reader.ReadBytes(commentLength);
        var comment = Encoding.Unicode.GetString(commentBytes);

        // Add change to changes list

        AddChange(new DateTime(year, month, day), comment);
    }
}
```

It is highly recommended that the count of the bytes read by this method is equal to the value passed to `size` parameter.

To be able to write the chunk we need to implement `WriteContent` method:

```csharp
protected override void WriteContent(MidiWriter writer, WritingSettings settings)
{
    // Write changes count

    writer.WriteVlqNumber(_changes.Count);

    foreach (var change in _changes)
    {
        // Write date

        var date = change.Date;
        writer.WriteByte((byte)date.Day);
        writer.WriteByte((byte)date.Month);
        writer.WriteWord((ushort)date.Year);

        // Write comment

        var comment = change.Comment;
        if (string.IsNullOrEmpty(comment))
        {
            writer.WriteVlqNumber(0);
            continue;
        }

        var commentBytes = Encoding.Unicode.GetBytes(comment.ToCharArray());
        writer.WriteVlqNumber(commentBytes.Length);
        writer.WriteBytes(commentBytes);
    }
}
```

Every chunk starts with an ID and its size. DryWetMIDI calls the `GetContentSize` method of the `MidiChunk` to write its return value as the chunk's size. You must calculate the real size of the chunk's content so that programs that read a MIDI file with your custom chunk can skip it by advancing the reader's position by this size. Let's implement `GetContentSize`:

```csharp
protected override uint GetContentSize(WritingSettings settings)
{
    return (uint)(
        _changes.Count.GetVlqLength() +
        _changes.Select(c =>
        {
            var commentLength = Encoding.Unicode.GetByteCount(c.Comment ?? string.Empty);
            return 4 /* 1 for day, 1 for month, 2 for year */ +
                    commentLength.GetVlqLength() +
                    commentLength;
        })
        .DefaultIfEmpty()
        .Sum());
}
```

Implementation of `Clone` method is pretty easy:

```csharp
public override MidiChunk Clone()
{
    var result = new HistoryChunk();
    result._changes.AddRange(_changes);
    return result;
}
```

That's all! The custom chunk is completely implemented. See code sample below to know how to read and write it:

```csharp
// Create a history chunk and populate it by some changes

var historyChunk = new HistoryChunk();
historyChunk.AddChange(new DateTime(2017, 3, 23), "Start the history!");
historyChunk.AddChange(new DateTime(2156, 11, 3), "Comment from the future.");
historyChunk.AddChange(new DateTime(9999, 2, 12), null);

// Add the chunk to an existing MIDI file

var file = MidiFile.Read("My Great Song.mid");
file.Chunks.Add(historyChunk);
file.Write("My Great Song.mid", true);

// Read the file with our chunk

var fileWithHistoryChunk = MidiFile.Read(
    "My Great Song.mid",
    new ReadingSettings
    {
        CustomChunkTypes = new ChunkTypesCollection
        {
            { typeof(HistoryChunk), "Hstr" }
        }
    });

var historyChunks = fileWithHistoryChunk.Chunks.OfType<HistoryChunk>();
```

If you don't provide information about your custom chunk in `ReadingSettings`, the chunks will be read as [UnknownChunk](xref:Melanchall.DryWetMidi.Core.UnknownChunk).

## Asynchronous content I/O

Keep the four synchronous methods above, even when adding asynchronous support.
Existing subclasses remain compatible: the default
[`MidiChunk.ReadContentAsync`](xref:Melanchall.DryWetMidi.Core.MidiChunk.ReadContentAsync(Melanchall.DryWetMidi.Core.MidiReader,Melanchall.DryWetMidi.Core.ReadingSettings,System.UInt32,System.Threading.CancellationToken))
buffers the declared chunk content before invoking `ReadContent`, and the default
[`MidiChunk.WriteContentAsync`](xref:Melanchall.DryWetMidi.Core.MidiChunk.WriteContentAsync(Melanchall.DryWetMidi.Core.MidiWriter,Melanchall.DryWetMidi.Core.WritingSettings,System.Threading.CancellationToken))
buffers the output of `WriteContent` before asynchronously transporting it.
These fallbacks can allocate memory proportional to the custom chunk size and
cannot interrupt arbitrary synchronous custom code with cancellation.

Override these protected virtual methods to process content incrementally with
the supplied reader or writer instead. The framework owns the chunk header:
read or write only content, not the chunk ID or size. Do not dispose the supplied
reader or writer, start overlapping operations, or wrap the synchronous overrides
in `Task.Run`.

For example, add these overrides to the `HistoryChunk` defined above:

```csharp
protected override async Task ReadContentAsync(
    MidiReader reader,
    ReadingSettings settings,
    uint size,
    CancellationToken cancellationToken)
{
    var changes = new List<Change>();
    var changesCount = await reader.ReadVlqNumberAsync(cancellationToken)
        .ConfigureAwait(false);

    for (var i = 0; i < changesCount; i++)
    {
        var day = await reader.ReadByteAsync(cancellationToken).ConfigureAwait(false);
        var month = await reader.ReadByteAsync(cancellationToken).ConfigureAwait(false);
        var year = await reader.ReadWordAsync(cancellationToken).ConfigureAwait(false);

        var commentLength = await reader.ReadVlqNumberAsync(cancellationToken)
            .ConfigureAwait(false);
        var commentBytes = await reader.ReadBytesAsync(commentLength, cancellationToken)
            .ConfigureAwait(false);

        if (commentBytes.Length != commentLength)
            throw new EndOfStreamException("The history comment is incomplete.");

        var comment = Encoding.Unicode.GetString(commentBytes);
        changes.Add(new Change(new DateTime(year, month, day), comment));
    }

    cancellationToken.ThrowIfCancellationRequested();
    _changes.AddRange(changes);
}

protected override async Task WriteContentAsync(
    MidiWriter writer,
    WritingSettings settings,
    CancellationToken cancellationToken)
{
    await writer.WriteVlqNumberAsync(_changes.Count, cancellationToken)
        .ConfigureAwait(false);

    foreach (var change in _changes)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var date = change.Date;
        await writer.WriteByteAsync((byte)date.Day, cancellationToken)
            .ConfigureAwait(false);
        await writer.WriteByteAsync((byte)date.Month, cancellationToken)
            .ConfigureAwait(false);
        await writer.WriteWordAsync((ushort)date.Year, cancellationToken)
            .ConfigureAwait(false);

        var commentBytes = Encoding.Unicode.GetBytes(change.Comment ?? string.Empty);
        await writer.WriteVlqNumberAsync(commentBytes.Length, cancellationToken)
            .ConfigureAwait(false);
        await writer.WriteBytesAsync(commentBytes, cancellationToken)
            .ConfigureAwait(false);
    }
}
```

The public reader primitives include
[`ReadByteAsync`](xref:Melanchall.DryWetMidi.Core.MidiReader.ReadByteAsync(System.Threading.CancellationToken)),
[`ReadWordAsync`](xref:Melanchall.DryWetMidi.Core.MidiReader.ReadWordAsync(System.Threading.CancellationToken)),
[`ReadBytesAsync`](xref:Melanchall.DryWetMidi.Core.MidiReader.ReadBytesAsync(System.Int32,System.Threading.CancellationToken)),
and [`ReadVlqNumberAsync`](xref:Melanchall.DryWetMidi.Core.MidiReader.ReadVlqNumberAsync(System.Threading.CancellationToken)).
`ReadBytesAsync` can return fewer bytes at end of input; validate the returned
length when the format requires an exact number. Corresponding writer primitives
include
[`WriteByteAsync`](xref:Melanchall.DryWetMidi.Core.MidiWriter.WriteByteAsync(System.Byte,System.Threading.CancellationToken)),
[`WriteWordAsync`](xref:Melanchall.DryWetMidi.Core.MidiWriter.WriteWordAsync(System.UInt16,System.Threading.CancellationToken)),
[`WriteBytesAsync`](xref:Melanchall.DryWetMidi.Core.MidiWriter.WriteBytesAsync*),
and [`WriteVlqNumberAsync`](xref:Melanchall.DryWetMidi.Core.MidiWriter.WriteVlqNumberAsync*).

These methods are public and can also be used with directly constructed
[`MidiReader`](xref:Melanchall.DryWetMidi.Core.MidiReader) and
[`MidiWriter`](xref:Melanchall.DryWetMidi.Core.MidiWriter) instances outside chunk
overrides. Await each operation before starting the next. Reader construction
is still synchronous: a policy that buffers all input can perform synchronous
I/O during construction. Do not select
[`BufferingPolicy.BufferAllData`](xref:Melanchall.DryWetMidi.Core.BufferingPolicy.BufferAllData)
when the underlying stream must not be read synchronously.

The `size` parameter is the declared content size, not a bounded substream.
Overrides must avoid reading into the following chunk; the configured
[`InvalidChunkSizePolicy`](xref:Melanchall.DryWetMidi.Core.ReadingSettings.InvalidChunkSizePolicy)
is applied after the override returns. Keep `GetContentSize` consistent with both
writers: it still determines the content size in the header and remains synchronous.

The example avoids a separate whole-chunk transport buffer, but it still
materializes the `HistoryChunk` and its comments. The temporary list also keeps
incomplete or canceled reads from committing partial changes to `_changes`.
This is not constant-memory deserialization. CPU-bound decoding, encoding,
size calculation, and other custom work remain synchronous unless they genuinely
await I/O. Pass the supplied cancellation token to every awaited I/O operation
and check it during long CPU-bound processing; cancellation is cooperative.
Cancellation or failure can leave partial output, without rollback or atomic
replacement.

[`MidiFile.ReadAsync`](xref:Melanchall.DryWetMidi.Core.MidiFile.ReadAsync*) uses the
read hook for registered custom chunks, and
[`MidiFile.WriteAsync`](xref:Melanchall.DryWetMidi.Core.MidiFile.WriteAsync*) uses
the write hook. Registration through `ReadingSettings.CustomChunkTypes` is
unchanged. For bulk file examples, progress, and stream ownership, see
[MIDI file reading](xref:a_file_reading#asynchronous-reading) and
[MIDI file writing](xref:a_file_writing#asynchronous-writing).

Lazy reading preserves the token model: nonstandard chunk content is returned
as [`BytesPacketToken`](xref:Melanchall.DryWetMidi.Core.BytesPacketToken) objects
instead of invoking the custom read hook.
[`MidiTokensWriter.WriteChunkAsync`](xref:Melanchall.DryWetMidi.Core.MidiTokensWriter.WriteChunkAsync(Melanchall.DryWetMidi.Core.MidiChunk,System.Threading.CancellationToken))
invokes the custom write hook when writing a chunk. See
[Lazy reading/writing](xref:a_file_lazy_reading_writing) for token sequencing,
seekability requirements, and asynchronous disposal.