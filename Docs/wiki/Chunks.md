MIDI files are made up of *chunks*. Each chunk has a 4-character type and a 32-bit length, which is the number of bytes in the chunk. With DryWetMIDI you manage chunks via `ChunksCollection` returned by the `Chunks` property of the `MidiFile`.

There are two standard types of MIDI chunks: *header chunk* and *track chunk*. The first one will not be presented in the `ChunksCollection`. Its data is used by the reading engine to set some properties of the `MidiFile` such as `TimeDivision` and `OriginalFormat`. You cannot add header chunks in the chunks collection of the file, the appropriate one will be written by writing engine automatically on `MidiFile.Write`.

Instances of the `TrackChunk` can be added to the `ChunksCollection` via `Add`, `AddRange` and `Insert` methods. Of course you can remove chunks with corresponding methods.

`ChunksCollection` implements `IEnumerable<MidiChunk>` so you can use all the power of LINQ for working with MIDI file's chunks.

The structure of the chunk allows any custom chunks be placed in a MIDI file along with the standard ones described below. DryWetMIDI allows to implement custom chunks that can be read from and written to a MIDI file. Visit [Custom chunks](Custom-chunks.md) page to learn more. If you doesn't specify information about your custom chunk types the reading engine will read them as instances of the `UnknownChunk` class which `Data` property will hold chunk's data and `ChunkId` will return ID of the chunk.

## MidiChunk

```csharp
public abstract class MidiChunk
{
    // ...

    public string ChunkId { get; }

    // ...

    protected abstract void ReadContent(MidiReader reader, ReadingSettings settings, uint size);

    protected abstract void WriteContent(MidiWriter writer, WritingSettings settings);

    protected abstract uint GetContentSize(WritingSettings settings);

    // ...
}
```

It is the base class for all chunks. It has the `ChunkId` property which holds a 4-character identifier of the chunk and some protected abstract methods you need to implement when you develop a custom chunk class.

## TrackChunk

```csharp
public sealed class TrackChunk : MidiChunk
{
    public TrackChunk() { /* ... */ }

    public TrackChunk(IEnumerable<MidiEvent> events) { /* ... */ }

    public TrackChunk(params MidiEvent[] events) { /* ... */ }

    // ...

    public EventsCollection Events { get; }

    // ...
}
```

This class represents a MIDI file's track chunk. This chunk is where you manage MIDI events via `EventsCollection` returned by the `Events` property of the `TrackChunk`. `EventsCollection` implements `IEnumerable<MidiEvent>` and provide methods for adding and removing events.

## UnknownChunk

```csharp
public sealed class UnknownChunk : MidiChunk
{
    // ...

    public byte[] Data { get; }

    // ...
}
```

This class represents any unknown chunk was encountered by the reading engine. You cannot create an instance of the `UnknownChunk` or set its `Data` property. If you want that chunks with specific ID be read as instances of your custom class you need to implement custom chunk according with instructions provided on the [Custom chunks](Custom-chunks.md) page and pass information about it to the `Read` method of the `MidiFile`.