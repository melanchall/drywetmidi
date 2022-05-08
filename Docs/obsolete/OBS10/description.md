`Randomizer` tool is obsolete now. Randomization feature has been moved to [Quantizer](xref:Melanchall.DryWetMidi.Tools.Quantizer) so you can quantize and randomize MIDI data at the same time.

Following small example shows how to randomize [timed events](xref:Melanchall.DryWetMidi.Interaction.TimedEvent) and start times of [notes](xref:Melanchall.DryWetMidi.Interaction.Note) in range from `-100` to `+100` ticks:

```csharp
midiFile.QuantizeObjects(
    ObjectType.Note | ObjectType.TimedEvent,
    new ArbitraryGrid(),
    new QuantizingSettings
    {
        RandomizingSettings = new RandomizingSettings
        {
            Bounds = new ConstantBounds((MidiTimeSpan)100)
        }
    });
```