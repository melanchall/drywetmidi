[WritingSettings.CompressionPolicy](xref:Melanchall.DryWetMidi.Core.WritingSettings.CompressionPolicy) has been replaced by corresponding properties of [WritingSettings](xref:Melanchall.DryWetMidi.Core.WritingSettings). No compression applied by default, as before.

For example, to write using running status and write Note Off events as Note On ones with zero velocity we need to write:

```csharp
midiFile.Write("Great MIDI file.mid", settings: new WritingSettings
{
    UseRunningStatus = true,
    NoteOffAsSilentNoteOn = true
});
```