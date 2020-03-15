To effectively manage groups of note events DryWetMIDI provides the `ChordsManager` class to help with this. Instead of messing with Note On/Off events and grouping them into chords this manager allows to operate by `Chord` objects.

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

`Chord` has following public members:

```csharp
public sealed class Chord : ILengthedObject
{
    // ...
    public Chord() { /*...*/ }
    public Chord(IEnumerable<Note> notes) { /*...*/ }
    public Chord(IEnumerable<Note> notes, long time) { /*...*/ }
    public Chord(params Note[] notes) { /*...*/ }
    // ...
    public NotesCollection Notes { get; }
    public long Time { get; set; }
    public long Length { get; set; }
    public FourBitNumber Channel { get; set; }
    public SevenBitNumber Velocity { get; set; }
    public SevenBitNumber OffVelocity { get; set; }
    // ...
}
```

`Notes` property returns collection of chord's notes which type is `NotesCollection`. Read the [Notes](Notes.md) page to learn more about notes managing.

Please read [general information about events managers](Events-managers.md) prior to use `ChordsManager`.

To get collection of all chords being managed by the current `ChordsManager` you need to use `Chords` property which returns `ChordsCollection`. `ChordsCollection` has methods for adding and removing chords and also can be enumerated since it implements `IEnumerable<Chord>`. In addition to these methods there are extension ones contained in `TimedObjectUtilities` and `LenghthedObjectUtilities`. For example, you can get all chords in the first track chunk of a MIDI file that end at 20 seconds from the start of the file and get length of the first found chord in hours:minutes:seconds format:

```csharp
TempoMap tempoMap = midiFile.GetTempoMap();

using (var chordsManager = midiFile.GetTrackChunks().First().ManageChords())
{
    var chordsEndedAt20Seconds = chordsManager.Chords
                                              .EndAtTime(new MetricTimeSpan(0, 0, 20),
                                                         tempoMap);

    var firstChordLength = chordsEndedAt20Seconds.First()
                                                 .LengthAs<MetricTimeSpan>(tempoMap);
}
```

Read the [Time and length](Time-and-length.md) page to learn more about custom time and length.

As you can see there is another way to get an instance of the `ChordsManager` – through the `ManageChords` extension method. This method and another useful ones are placed in `ChordsManagingUtilities`. For example, to get all chords contained in a MIDI file and consisted of notes with start times within tolerance of 100 you can write:

```csharp
IEnumerable<Chord> chords = midiFile.GetChords(100);
```