---
uid: a_notes_merger
---

# Notes merger

To merge nearby notes into one DryWetMIDI provides [NotesMerger](xref:Melanchall.DryWetMidi.Tools.NotesMerger) class. Example of usage:

```csharp
var notes = midiFile.GetNotes().Where(n => n.NoteName == NoteName.CSharp);
var tempoMap = midiFile.GetTempoMap();

var notesMerger = new NotesMerger();
var mergedNotes = notesMerger.Merge(
    notes,
    tempoMap,
    new NotesMergingSettings
    {
        Tolerance = new MetricTimeSpan(0, 0, 1)
    });
```

Note that notes obtained via [GetNotes](xref:Melanchall.DryWetMidi.Interaction.NotesManagingUtilities.GetNotes(Melanchall.DryWetMidi.Core.MidiFile)) are detached from the file so if you call `GetNotes` on the file again, you will get original unmerged notes. To update notes in the file or specific track chunk you should use [NotesManager](xref:Melanchall.DryWetMidi.Interaction.NotesManager).

Also there are useful methods inside [NotesMergerUtilities](xref:Melanchall.DryWetMidi.Tools.NotesMergerUtilities) class that allows quickly merge nearby notes inside [TrackChunk](xref:Melanchall.DryWetMidi.Core.TrackChunk) or [MidiFile](xref:Melanchall.DryWetMidi.Core.MidiFile) without messing with updating notes via `NotesManager`. Example above can be rewritten to the following code:

```csharp
midiFile.MergeNotes(new NotesMergingSettings
                    {
                        Tolerance = new MetricTimeSpan(0, 0, 1)
                    },
                    n => n.NoteName == NoteName.CSharp);
```

Process of merging can be adjusted via [NotesMergingSettings](xref:Melanchall.DryWetMidi.Tools.NotesMergingSettings) which contains important property (along with other ones) - [Tolerance](xref:Melanchall.DryWetMidi.Tools.NotesMergingSettings.Tolerance). Tolerance is maximum distance between two notes to consider them as nearby. The default value is time span of zero length, so two notes should have no gap between them to me merged. The image below shows how tolerance (T) affects merging:

![Notes merger tolerance](images/NotesMerger/NotesMergerTolerance.png)