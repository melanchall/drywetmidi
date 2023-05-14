Complex inheritance hierarchy of quantizer classes has been reduced to general-purpose new [Quantizer](xref:Melanchall.DryWetMidi.Tools.Quantizer) class which can quantize objects of different types at the same time. More than that, it can quantize both start and end time simultaneously.

For example, to quantize both ends of objects by the grid of `1/8` step:

```csharp
var quantizer = new Quantizer();
quantizer.Quantize(
    objects,
    new SteppedGrid(MusicalTimeSpan.Eighth),
    TempoMap.Default,
    new QuantizingSettings
    {
        Target = QuantizerTarget.Start | QuantizerTarget.End
    });
```

Of course, there is the new class with utility methods for quantizing objects within a [MIDI file](xref:Melanchall.DryWetMidi.Core.MidiFile) or [track chunk](xref:Melanchall.DryWetMidi.Core.TrackChunk) – [QuantizerUtilities](xref:Melanchall.DryWetMidi.Tools.QuantizerUtilities). Following example shows how to quantize both ends of [notes](xref:Melanchall.DryWetMidi.Interaction.Note) and [chords](xref:Melanchall.DryWetMidi.Interaction.Chord) by the grid of `1` second step within a MIDI file. We'll define a chord as a set of notes with minimum length of `2`:

```csharp
midiFile.QuantizeObjects(
    ObjectType.Note | ObjectType.Chord,
    new SteppedGrid(new MetricTimeSpan(0, 0, 1)),
    new QuantizingSettings
    {
        Target = QuantizerTarget.Start | QuantizerTarget.End
    },
    new ObjectDetectionSettings
    {
        ChordDetectionSettings = new ChordDetectionSettings
        {
            NotesMinCount = 2
        }
    });
```