Randomization is now moved to quantizer so you can quantize and randomize MIDI data at the same time. Small example of how to perform only randomization:

```csharp
midiFile.QuantizeNotes(
    new ArbitraryGrid(),
    new NotesQuantizingSettings
    {
        RandomizingSettings = new RandomizingSettings
        {
            Bounds = bounds
        }
    });
```