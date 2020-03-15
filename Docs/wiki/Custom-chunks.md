MIDI files are made up of *chunks*. Each chunk has a 4-character ID and a 32-bit length, which is the
number of bytes in the chunk. This structure allows future or custom chunk types to be designed which may be easily be
ignored if encountered by a program written before the chunk type is introduced or if the program doesn't know about the type. DryWetMIDI allows you to implement custom chunks which can be written to a MIDI file and be read from it.

For example, we want to design a chunk that will contain information about changes in a MIDI file. A change is described by date (day, month, year) and comment. Let's create the class to store single change.

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

Now we are going to implement a custom chunk. Custom chunk class must be derived from the `MidiChunk` and must implement three abstract protected methods:

* `ReadContent`;
* `WriteContent`;
* `GetContentSize`.

Also the class must have parameterless constructor which calls constructor of the base class (`MidiChunk`) passing chunk's ID to it. ID is a 4-character string which is *Hstr* for our chunk. The class will look like this:

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
}
```

Before we will start to implement three methods mentioned above we need to determine the format in which changes have to be read and be written. Chunk's content will be started with a count of changes represented by [variable-length quantity](https://en.wikipedia.org/wiki/Variable-length_quantity) (VLQ) number. The count followed by changes. Each change is represented by:

* one byte for day;
* one byte for month;
* two bytes for year;
* VLQ number for size of bytes array representing comment;
* bytes which represent comment.

To store comments we will use `Encoding.Unicode`.

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
        var year = reader.ReadWord(); // two byte unsigned integer

        // Read comment

        var commentLength = reader.ReadVlqNumber();
        var commentBytes = reader.ReadBytes(commentLength);
        var comment = Encoding.Unicode.GetString(commentBytes);

        // Add read change to changes list

        AddChange(new DateTime(year, month, day), comment);
    }
}
```

It is highly recommended that count of the bytes were read by this method will be equal to the value passed to *size* parameter.

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

Every chunk starts with ID and its size. DryWetMIDI calls `GetContentSize` method of the `Chunk` to write its return value as chunk's size. You must calculate real size of the chunk's content in order to programs which will be read a MIDI file with your custom chunk will be able to skip it by advancing position of the reader on this size. Let's implement `GetContentSize`:

```csharp
protected override uint GetContentSize(WritingSettings settings)
{
    return (uint)(_changes.Count.GetVlqLength() +
                  _changes.Select(c =>
                                  {
                                      var commentLength = Encoding.Unicode.GetByteCount(c.Comment.ToCharArray());
                                      return 4 // 1 for day, 1 for month, 2 for year
                                             + commentLength.GetVlqLength() + commentLength;
                                  })
                          .DefaultIfEmpty()
                          .Sum());
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

var fileWithHistory = MidiFile.Read("My Great Song.mid",
                                    new ReadingSettings
                                    {
                                        CustomChunksTypes = new ChunkTypesCollection
                                        {
                                            { typeof(HistoryChunk), "Hstr" }
                                        }
                                    });
```