---
uid: a_file_lazy_reading_writing
---

# Lazy reading/writing

## Reading

When you call [MidiFile.Read](xref:Melanchall.DryWetMidi.Core.MidiFile.Read*) method, the entire structure of a MIDI file will be put to memory as an object of the [MidiFile](xref:Melanchall.DryWetMidi.Core.MidiFile) type. It means that more or less big MIDI file will span significant amount of bytes in the memory. Although it's much easy to work with such fully initialized object, memory consumption can be a critical parameter for your application.

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

With this approach each call of [ReadToken](xref:Melanchall.DryWetMidi.Core.MidiTokensReader.ReadToken) method will read next portion of data from a MIDI file (in fact, the internal reader in DryWetMIDI uses the buffer of `4096` bytes by default, the size of this buffer can be adjusted via [BufferSize](xref:Melanchall.DryWetMidi.Core.ReaderSettings.BufferSize) property of [ReadingSettings.ReaderSettings](xref:Melanchall.DryWetMidi.Core.ReadingSettings.ReaderSettings)). So the memory consumption will be almost always constant (despite a MIDI file size) and low. But of course there's the price – slower reading and much more difficult implementation of some logic and algorithms needed for your application. Also many high level tools provided by DryWetMIDI will be unavailable.

Some useful methods can be found in the [MidiTokensReaderUtilities](xref:Melanchall.DryWetMidi.Core.MidiTokensReaderUtilities) class. Following example shows how you can calculate count of all _A#_ notes for each track chunk ([EnumerateObjects](xref:Melanchall.DryWetMidi.Interaction.GetObjectsUtilities.EnumerateObjects*) method also used):

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

In comparison, standard approach takes much less lines of code (but takes much more memory of course):

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

The same applied to the process of writing a MIDI file. [MidiFile.Write](xref:Melanchall.DryWetMidi.Core.MidiFile.Write*) requires an instance of the [MidiFile](xref:Melanchall.DryWetMidi.Core.MidiFile) obviously which can occupy a lot of memory for big files.

Here an example of how to write MIDI data in a lazy way using [MidiTokensWriter](xref:Melanchall.DryWetMidi.Core.MidiTokensWriter):

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

The code above writes one million notes to a MIDI file. Of course you can combine [MidiTokensReader](xref:Melanchall.DryWetMidi.Core.MidiTokensReader), [EnumerateObjects](xref:Melanchall.DryWetMidi.Interaction.GetObjectsUtilities.EnumerateObjects*), [MidiTokensWriter](xref:Melanchall.DryWetMidi.Core.MidiTokensWriter) and [TimedObjectsWriter](xref:Melanchall.DryWetMidi.Interaction.TimedObjectsWriter) to process MIDI objects transforming a MIDI file into another one.