---
uid: a_file_splitting
---

# MIDI file splitting

You can split a MIDI file in different ways using extension methods from the [Splitter](xref:Melanchall.DryWetMidi.Tools.Splitter) class. Available methods are described below. Please note that the article doesn't cover all possible methods and their settings. Please read API documentation on [Splitter](xref:Melanchall.DryWetMidi.Tools.Splitter) to get complete information.

## SplitByChannel

[SplitByChannel](xref:Melanchall.DryWetMidi.Tools.Splitter.SplitByChannel*) method splits a MIDI file by channel so all [channel events](xref:Melanchall.DryWetMidi.Core.ChannelEvent) will be separated by channel and copied to corresponding new files. All meta and system exclusive events will be copied to all the new files (that's default behavior that can be turned off). Thus each new file will contain all meta and sysex events and channel ones for a single channel. The image below illustrates this process:

![Split MIDI file by channel](images/Splitter/SplitByChannel.png)

## SplitByObjects

[SplitByObjects](xref:Melanchall.DryWetMidi.Tools.Splitter.SplitByObjects*) method splits a MIDI file by objects. The process can be adjusted via [SplitByObjectsSettings](xref:Melanchall.DryWetMidi.Tools.SplitByObjectsSettings) passed to the second parameter of the method.

For example, to split a file by notes copying all MIDI events to each new file:

```csharp
var newFiles = midiFile.SplitByObjects(
    ObjectType.Note | ObjectType.TimedEvent,
    new SplitByObjectsSettings
    {
        WriteToAllFilesPredicate = obj => obj is TimedEvent
    });
```

Here we specify that we need to split the file by notes and timed events, but every timed event must be written to all result files. So in fact we're splitting the file by notes having all non-notes timed events presented in each new file. The image below illustrates the process:

![Split MIDI file by objects](images/Splitter/SplitByObjects.png)

To split a file by objects the tool needs to determine the key of each object. Objects with the same key will be outputted in a separate file. In the example above default logic of key calculation is used. The following table shows what a default key is for each type of an object:

|Object type|Key|
|-----------|---|
|[TimedEvent](xref:Melanchall.DryWetMidi.Interaction.ObjectType.TimedEvent)|The type of the underlying event ([EventType](xref:Melanchall.DryWetMidi.Core.MidiEvent.EventType) of [TimedEvent.Event](xref:Melanchall.DryWetMidi.Interaction.TimedEvent.Event)).|
|[Note](xref:Melanchall.DryWetMidi.Interaction.ObjectType.Note)|Pair of the [channel](xref:Melanchall.DryWetMidi.Interaction.Note.Channel) and [note number](xref:Melanchall.DryWetMidi.Interaction.Note.NoteNumber) of a note.|
|[Chord](xref:Melanchall.DryWetMidi.Interaction.ObjectType.Chord)|Collection of keys of the underlying [notes](xref:Melanchall.DryWetMidi.Interaction.Chord.Notes).|
|[Rest](xref:Melanchall.DryWetMidi.Interaction.ObjectType.Rest)|Pair of the [channel](xref:Melanchall.DryWetMidi.Interaction.Rest.Channel) and [note number](xref:Melanchall.DryWetMidi.Interaction.Rest.NoteNumber) of a rest.|

You can alter key calculation logic providing custom key selector. For example, to separate notes by only note number ignoring a note's channel:

```csharp
var newFiles = midiFile.SplitByObjects(
    ObjectType.Note | ObjectType.TimedEvent,
    new SplitByObjectsSettings
    {
        KeySelector = obj => obj is Note note
            ? ObjectIdUtilities.GetObjectId(note.NoteNumber)
            : obj.GetObjectId(),
        WriteToAllFilesPredicate = obj => obj is TimedEvent
    });
```

[ObjectIdUtilities.GetObjectId(value)](xref:Melanchall.DryWetMidi.Interaction.ObjectIdUtilities.GetObjectId``1(``0)) returns an implementation of [IObjectId](xref:Melanchall.DryWetMidi.Interaction.IObjectId) which simply holds the provided value. [ObjectIdUtilities.GetObjectId(object)](xref:Melanchall.DryWetMidi.Interaction.ObjectIdUtilities.GetObjectId(Melanchall.DryWetMidi.Interaction.ITimedObject)) returns the default ID (key) for an object. So if an object is a note, we use its note number as the key; and default key for any other object types.

If custom logic of key selection is complex, you may decide to implement the [IObjectId](xref:Melanchall.DryWetMidi.Interaction.IObjectId) interface and return that implementation. Just for example, let's create an ID class that identifies a chord by its shortest name:

```csharp
private sealed class ChordNameId : IObjectId
{
    private readonly string _name;
    
    public ChordNameId(Chord chord)
    {
        _name = chord.GetMusicTheoryChord().GetNames().FirstOrDefault();
    }

    public override bool Equals(object obj) =>
        obj is ChordNameId chordNameId &&
        chordNameId._name == _name;

    public override int GetHashCode() =>
        _name.GetHashCode();
}
```

And now we can use it to split a file by chords of the same name:

```csharp
var newFiles = midiFile.SplitByObjects(
    ObjectType.Chord,
    new SplitByObjectsSettings
    {
        KeySelector = obj => new ChordNameId((Chord)obj)
    });
```

Please see documentation on [SplitByObjectsSettings](xref:Melanchall.DryWetMidi.Tools.SplitByObjectsSettings) to learn more about how you can adjust the process of splitting.

## SplitByGrid

[SplitByGrid](xref:Melanchall.DryWetMidi.Tools.Splitter.SplitByGrid*) method splits MIDI file by the specified grid. Each file will preserve original tempo map and all parameters changes (like a control value or program changes). The image below shows general case of splitting a MIDI file by grid:

![Split MIDI file by grid](images/Splitter/SplitFileByGrid.png)

Splitting can be adjusted via `settings` parameter of the [SliceMidiFileSettings](xref:Melanchall.DryWetMidi.Tools.SliceMidiFileSettings) type. [SplitNotes](xref:Melanchall.DryWetMidi.Tools.SliceMidiFileSettings.SplitNotes) and [PreserveTimes](xref:Melanchall.DryWetMidi.Tools.SliceMidiFileSettings.PreserveTimes) properties described below. Please see all available properties in documentation for [SliceMidiFileSettings](xref:Melanchall.DryWetMidi.Tools.SliceMidiFileSettings).

### SplitNotes

[SplitNotes](xref:Melanchall.DryWetMidi.Tools.SliceMidiFileSettings.SplitNotes) indicates whether notes should be split at points of grid intersection or not. The default value is `true`. If `false` used, notes treated as just Note On / Note Off events rather than note objects. Splitting notes produces new Note On / Note Off events at points where grid intersects notes if the property set to `true`. The following image shows splitting by grid if `SplitNotes` set to `false`:

![Split MIDI file by grid without splitting notes](images/Splitter/SplitByGridDontSplitNotes.png)

### PreserveTimes

[PreserveTimes](xref:Melanchall.DryWetMidi.Tools.SliceMidiFileSettings.PreserveTimes) indicates whether original times of events should be preserved or not. The default value is `false`. If `false` used, events will be moved to the start of a new file. If `true` used, events will be placed in new files at the same times as in the input file. The following image shows splitting in case of `PreserveTimes` set to `true`:

![Split MIDI file by grid preserving times](images/Splitter/SplitByGridPreserveTimes.png)

## SkipPart

[SkipPart](xref:Melanchall.DryWetMidi.Tools.Splitter.SkipPart*) method skips part of the specified length of a MIDI file and returns remaining part as an instance of [MidiFile](xref:Melanchall.DryWetMidi.Core.MidiFile). The image below shows general case of skipping a part of a MIDI file:

![Skip part of a MIDI file](images/Splitter/SkipPart.png)

## TakePart

[TakePart](xref:Melanchall.DryWetMidi.Tools.Splitter.TakePart*) methods take part of the specified length of a MIDI file (starting at the start or at the specified time within the file) and return it as an instance of [MidiFile](xref:Melanchall.DryWetMidi.Core.MidiFile). The image below shows both cases of taking a part of a MIDI file:

![Take part of a MIDI file at the start of the file](images/Splitter/TakePartAtStart.png)

![Take part of a MIDI file at the middle of the file](images/Splitter/TakePartAtMiddle.png)

## CutPart

[CutPart](xref:Melanchall.DryWetMidi.Tools.Splitter.CutPart*) method cuts a part of the specified length from a MIDI file (starting at the specified time within the file) and returns a new instance of [MidiFile](xref:Melanchall.DryWetMidi.Core.MidiFile) which is the original file without the part. The image below shows how the method works:

![Cut part from a MIDI file](images/Splitter/CutPart.png)