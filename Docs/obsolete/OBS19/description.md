`Splitter.SplitByNotes` method is now obsolere since it has been replaced by [Splitter.SplitByObjects](xref:Melanchall.DryWetMidi.Tools.Splitter.SplitByObjects*) one which can split a MIDI file by objects of difefrent types and with more flexibility.

Simple example of how to split a file by notes with the new tool:

```csharp
var newFiles = midiFile.SplitByObjects(ObjectType.Note);
```

To split ignoring a note's channel:

```csharp
var newFiles = midiFile.SplitByObjects(
    ObjectType.Note,
    new SplitByObjectsSettings
    {
        KeySelector = obj => ObjectIdUtilities.GetObjectId(((Note)obj).NoteNumber)
    });
```

To split by notes with all other events transferred to each new file:

```csharp
var newFiles = midiFile.SplitByObjects(
    ObjectType.Note | ObjectType.TimedEvent,
    new SplitByObjectsSettings
    {
        KeySelector = obj => ObjectIdUtilities.GetObjectId(((Note)obj).NoteNumber),
        WriteToAllFilesPredicate = obj => obj is TimedEvent
    });
```