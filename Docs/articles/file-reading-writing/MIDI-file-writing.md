---
uid: a_file_writing
---

# MIDI file writing

The simplest code for MIDI file writing is:

```csharp
file.Write("Some great song.mid");
```

If a file with this name already exists, you'll get an exception. To overwrite existing file pass `true` to `overwriteFile` parameter:

```csharp
file.Write("Some great song.mid", overwriteFile: true);
```

## Compression

You can set specific policies via [WritingSettings](xref:Melanchall.DryWetMidi.Core.WritingSettings) to reduce the size of an output file. For example, to use running status and thus don't write status bytes of channel events of the same type, set properties shown in the following code:

```csharp
file.Write("Some great song.mid", settings: new WritingSettings
{
    UseRunningStatus = true,
    NoteOffAsSilentNoteOn = true
});
```

Complete list of available properties is placed in the documentation of [WritingSettings](xref:Melanchall.DryWetMidi.Core.WritingSettings).
