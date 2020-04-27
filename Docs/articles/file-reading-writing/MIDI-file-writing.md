---
uid: a_file_writing
---

# MIDI file writing

The simplest code for MIDI file writing is:

```csharp
file.Write("Some great song.mid");
```

If file with this name already exist, you'll get an excption. To overwrite existing file pass `true` to `overwriteFile` parameter:

```csharp
file.Write("Some great song.mid", overwriteFile: true);
```

## Compression

You can set specific policies via [WritingSettings](xref:Melanchall.DryWetMidi.Core.WritingSettings) to reduce size of an output file. For example, to use running status and thus don't write status bytes of channel events of the same type, set [CompressionPolicy](xref:Melanchall.DryWetMidi.Core.WritingSettings.CompressionPolicy) property according to this code:

```csharp
file.Write("Some great song.mid", settings: new WritingSettings
{
    CompressionPolicy = CompressionPolicy.UseRunningStatus | CompressionPolicy.NoteOffAsSilentNoteOn
});
```

Complete list of available compression rules is placed in documentation of [CompressionPolicy](xref:Melanchall.DryWetMidi.Core.CompressionPolicy).