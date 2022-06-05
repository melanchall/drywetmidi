`NotesMerger` class has been generalized by the [Merger](xref:Melanchall.DryWetMidi.Tools.Merger) class which can merge objects of different types simultaneously.

To merge objects within a collection of [timed objects](xref:Melanchall.DryWetMidi.Interaction.ITimedObject):

```csharp
var newObjects = objects.MergeObjects(
    TempoMap.Default,
    new ObjectsMergingSettings
    {
        Tolerance = new MetricTimeSpan(0, 0, 1)
    });
```

Quick example of how to merge notes within a MIDI file with the new tool:

```csharp
midiFile.MergeObjects(
    ObjectType.Note,
    new ObjectsMergingSettings
    {
        VelocityMergingPolicy = VelocityMergingPolicy.Max,
        Tolerance = (MidiTimeSpan)0
    });
```