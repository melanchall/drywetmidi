# Chords

To effectively manage groups of note events DryWetMIDI provides the [ChordsManager](xref:Melanchall.DryWetMidi.Interaction.ChordsManager) class. Instead of messing with Note On/Off events and grouping them into chords this manager does this for you via [Chord](xref:Melanchall.DryWetMidi.Interaction.Chord) objects.

```csharp
using (var chordsManager = new ChordsManager(trackChunk.Events))
{
    ChordsCollection chords = chordsManager.Chords;

    var threeNotesChords = chords.Where(c => c.Notes.Count() == 3);
    chords.Add(new Chord(new[]
                         {
                             new Note(NoteName.A, 4) { Velocity = (SevenBitNumber)90 },
                             new Note(NoteName.B, 4) { Velocity = (SevenBitNumber)90 },
                             new Note(NoteName.C, 5) { Velocity = (SevenBitNumber)90 }
                         },
                         123));
}
```

[Notes](xref:Melanchall.DryWetMidi.Interaction.Chord.Notes) property of `Chord` class returns collection of chord's notes which type is [NotesCollection](xref:Melanchall.DryWetMidi.Interaction.NotesCollection). Read the [Notes](Notes.md) article to learn more about notes managing.

Please read [general information about events managers](Events-managers-overview.md) prior to use `ChordsManager`.

To get collection of all chords being managed by the current `ChordsManager` you need to use [Chords](xref:Melanchall.DryWetMidi.Interaction.ChordsManager.Chords) property which returns [ChordsCollection](xref:Melanchall.DryWetMidi.Interaction.ChordsCollection). `ChordsCollection` has methods for adding and removing chords and also can be enumerated since it implements `IEnumerable<Chord>`. In addition to these methods there are extension ones contained in [TimedObjectUtilities](xref:Melanchall.DryWetMidi.Interaction.TimedObjectUtilities) and [LengthedObjectUtilities](xref:Melanchall.DryWetMidi.Interaction.LengthedObjectUtilities). For example, you can get all chords in the first track chunk of a MIDI file that end at 20 seconds from the start of the file and get length of the first found chord in _hours:minutes:seconds_ (see [MetricTimeSpan](MetricTimeSpan.md) page) format:

```csharp
TempoMap tempoMap = midiFile.GetTempoMap();

using (var chordsManager = midiFile.GetTrackChunks().First().ManageChords())
{
    var chordsEndedAt20Seconds = chordsManager
        .Chords.EndAtTime(new MetricTimeSpan(0, 0, 20), tempoMap);

    var firstChordLength = chordsEndedAt20Seconds
        .First().LengthAs<MetricTimeSpan>(tempoMap);
}
```

Read the [time and length overview](Time-and-length-overview.md) to learn more about different time and length representations.

As you can see there is another way to get an instance of the `ChordsManager` – through the `ManageChords` extension method. This method and another useful ones are placed in [ChordsManagingUtilities](xref:Melanchall.DryWetMidi.Interaction.ChordsManagingUtilities). For example, to get all chords contained in a MIDI file and consisted of notes with start times within tolerance of `100` you can write:

```csharp
IEnumerable<Chord> chords = midiFile.GetChords(100);
```