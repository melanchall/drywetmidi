---
uid: a_custom_chunk
---

# Custom chunks

MIDI files are made up of **chunks**. Each chunk has a 4-character ID and a 32-bit length, which is the number of bytes in the chunk. This structure allows future or custom chunk types to be designed which may be easily ignored if encountered by a program written before a chunk type is introduced or if the program doesn't know about the type. DryWetMIDI allows you to implement custom chunks which can be written to a MIDI file and be read from it.

For example, we want to design a chunk that will contain information about changes in whatever we want. A change is described by **date** (day, month, year) and **comment**. Let's create the class to store single change.

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

Also the class must have parameterless constructor which calls constructor of the base class ([MidiChunk](xref:Melanchall.DryWetMidi.Core.MidiChunk)) passing chunk's ID to it. ID is a 4-character string which will be **Hstr** for our chunk. ID of custom chunk should not be the same as one of standard chunks IDs. To get IDs of standard chunks you can call [MidiChunk.GetStandardChunkIds](xref:Melanchall.DryWetMidi.Core.MidiChunk.GetStandardChunkIds).

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

Before we will start to implement four methods mentioned above we need to determine the structure of change records according to which it should be read and written.

Chunk's content will be started with the count of changes. We will write this count as [variable-length quantity](https://en.wikipedia.org/wiki/Variable-length_quantity) (VLQ) number. The count followed by change records.

Each change is:

* one byte for **day**;
* one byte for **month**;
* two bytes for **year**;
* VLQ number bytes representing size of bytes array which is encoded comment;
* bytes which represent encoded comment string.

To store comments we will use [](xref:System.Text.Encoding.Unicode?title=Encoding.Unicode) encoding.

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

It is highly recommended that count of the bytes were read by this method is equal to the value passed to `size` parameter.

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

Every chunk starts with ID and its size. DryWetMIDI calls `GetContentSize` method of the `MidiChunk` to write its return value as chunk's size. You must calculate real size of the chunk's content in order to programs which will be read a MIDI file with your custom chunk will be able to skip it by advancing position of the reader on this size. Let's implement `GetContentSize`:

```csharp
protected override uint GetContentSize(WritingSettings settings)
{
    return (uint)(
        _changes.Count.GetVlqLength() +
        _changes.Select(c =>
        {
            var commentLength = Encoding.Unicode.GetByteCount(c.Comment.ToCharArray());
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

That's all! Custom chunk is completely implemented. See code sample below to know how to read and write it:

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