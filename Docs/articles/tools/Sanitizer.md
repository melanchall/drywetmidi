---
uid: a_sanitizer
---

# Sanitizer

DryWetMIDI provides a tool to clean a MIDI file up removing redundant events and so on – [Sanitizer](xref:Melanchall.DryWetMidi.Tools.Sanitizer). The process can be run via the [Sanitize](xref:Melanchall.DryWetMidi.Tools.Sanitizer.Sanitize*) extension method:

```csharp
midiFile.Sanitize();
```

It's a simple example that shows how you can perform all supported cleaning procedures. You can pass an instance of the [SanitizingSettings](xref:Melanchall.DryWetMidi.Tools.SanitizingSettings) to the method turning off or on desired options:

```csharp
midiFile.Sanitize(new SanitizingSettings
{
    NoteMinLength = MusicalTimeSpan.Quarter,
    RemoveEmptyTrackChunks = false
});
```

Here we're removing all notes that are shorter than `1/4` leaving track chunks without events untouched. Please see detailed descriptions of settings below.

## Settings

The [Sanitizer](xref:Melanchall.DryWetMidi.Tools.Sanitizer) class provides a rich set of options you can manage sanitizing of a MIDI file with.

### NoteMinLength

[NoteMinLength](xref:Melanchall.DryWetMidi.Tools.SanitizingSettings.NoteMinLength) property of the [SanitizingSettings](xref:Melanchall.DryWetMidi.Tools.SanitizingSettings) sets a minimum length for notes within an input file. All notes that are shorter than this value will be removed.

The default value is `null` which means notes can have any length. The same behavior can be achieved with a value of zero length. This should be read as "a note should not be removed if its length is zero or greater".

Image below shows how you can get rid of short notes with this option:

![SanitizingSettings.NoteMinLength](images/Sanitizer/NoteMinLength.png)

Please notice that how notes will be detected is handled by the [NoteDetectionSettings](xref:Melanchall.DryWetMidi.Tools.SanitizingSettings.NoteDetectionSettings) property. You can read more about the [NoteDetectionSettings](xref:Melanchall.DryWetMidi.Interaction.NoteDetectionSettings) class in the [Getting objects: GetNotes: Settings](xref:a_getting_objects#settings) article.

### RemoveEmptyTrackChunks

[RemoveEmptyTrackChunks](xref:Melanchall.DryWetMidi.Tools.SanitizingSettings.RemoveEmptyTrackChunks) property determines whether track chunks without MIDI events should be removed or not.

### RemoveOrphanedNoteOnEvents

In some MIDI files you can encounter [Note On](xref:Melanchall.DryWetMidi.Core.NoteOnEvent) events without corresponding [Note Off](xref:Melanchall.DryWetMidi.Core.NoteOffEvent) ones. For example, in this example we're creating a file with the first event is an orphaned [Note On](xref:Melanchall.DryWetMidi.Core.NoteOnEvent) one:

```csharp
var midiFile = new MidiFile(
    new TrackChunk(
        new NoteOnEvent(),
        new TextEvent("A")));
```

Of course how notes are detected is controlled by the [NoteDetectionSettings](xref:Melanchall.DryWetMidi.Tools.SanitizingSettings.NoteDetectionSettings) property. As stated above you can read more about the [NoteDetectionSettings](xref:Melanchall.DryWetMidi.Interaction.NoteDetectionSettings) class in the [Getting objects: GetNotes: Settings](xref:a_getting_objects#settings) article.

[Sanitizer](xref:Melanchall.DryWetMidi.Tools.Sanitizer) allows to remove such orphaned [Note On](xref:Melanchall.DryWetMidi.Core.NoteOnEvent) events with the [RemoveOrphanedNoteOnEvents](xref:Melanchall.DryWetMidi.Tools.SanitizingSettings.RemoveOrphanedNoteOnEvents) property:

![SanitizingSettings.RemoveOrphanedNoteOnEvents](images/Sanitizer/RemoveOrphanedNoteOnEvents.png)

### RemoveOrphanedNoteOffEvents

Obviously [Note Off](xref:Melanchall.DryWetMidi.Core.NoteOffEvent) events can be orphaned too (see [RemoveOrphanedNoteOnEvents](#removeorphanednoteonevents) above) and you can remove such events too using the [RemoveOrphanedNoteOffEvents](xref:Melanchall.DryWetMidi.Tools.SanitizingSettings.RemoveOrphanedNoteOffEvents) property:

![SanitizingSettings.RemoveOrphanedNoteOffEvents](images/Sanitizer/RemoveOrphanedNoteOffEvents.png)

### RemoveDuplicatedSetTempoEvents

If we create a MIDI file like that:

```csharp
var midiFile = new MidiFile(
    new TrackChunk(
        new SetTempoEvent(100),
        new TextEvent("A"),
        new SetTempoEvent(100),
        new TextEvent("B"),
        new SetTempoEvent(200),
        new TextEvent("C"),
        new SetTempoEvent(200)));
```

we'll notice that the second and last [Set Tempo](xref:Melanchall.DryWetMidi.Core.SetTempoEvent) events are redundant. The file above is equivalent to this one:

```csharp
var midiFile = new MidiFile(
    new TrackChunk(
        new SetTempoEvent(100),
        new TextEvent("A"),
        new TextEvent("B"),
        new SetTempoEvent(200),
        new TextEvent("C")));
```

[RemoveDuplicatedSetTempoEvents](xref:Melanchall.DryWetMidi.Tools.SanitizingSettings.RemoveDuplicatedSetTempoEvents) property allows to remove redundant [Set Tempo](xref:Melanchall.DryWetMidi.Core.SetTempoEvent) events:

![SanitizingSettings.RemoveDuplicatedSetTempoEvents](images/Sanitizer/RemoveDuplicatedSetTempoEvents.png)

### RemoveDuplicatedTimeSignatureEvents

[RemoveDuplicatedTimeSignatureEvents](xref:Melanchall.DryWetMidi.Tools.SanitizingSettings.RemoveDuplicatedTimeSignatureEvents) does the same actions as the [RemoveDuplicatedSetTempoEvents](#removeduplicatedsettempoevents) one but for duplicated [Time Signature](xref:Melanchall.DryWetMidi.Core.TimeSignatureEvent) events.

### RemoveDuplicatedPitchBendEvents

[RemoveDuplicatedPitchBendEvents](xref:Melanchall.DryWetMidi.Tools.SanitizingSettings.RemoveDuplicatedPitchBendEvents) does the same actions as the [RemoveDuplicatedSetTempoEvents](#removeduplicatedsettempoevents) or [RemoveDuplicatedTimeSignatureEvents](#removeduplicatedtimesignatureevents) one but for duplicated [Pitch Bend](xref:Melanchall.DryWetMidi.Core.PitchBendEvent) events.

### RemoveEventsOnUnusedChannels

If a MIDI file doesn't contain notes on some channel, we can safely remove all events on that channel. For example, the [Control Change](xref:Melanchall.DryWetMidi.Core.ControlChangeEvent) event on channel `2` in the following example is redundant:

```csharp
var midiFile = new MidiFile(
    new TrackChunk(
        new ControlChangeEvent { Channel = (FourBitNumber)2 },
        new ControlChangeEvent { Channel = (FourBitNumber)5 },
        new NoteOnEvent { Channel = (FourBitNumber)5 },
        new NoteOffEvent { Channel = (FourBitNumber)5 }));
```

[RemoveEventsOnUnusedChannels](xref:Melanchall.DryWetMidi.Tools.SanitizingSettings.RemoveEventsOnUnusedChannels) property turns on or off removing such channel events that have no effect since ther are no notes on those channels:

![SanitizingSettings.RemoveEventsOnUnusedChannels](images/Sanitizer/RemoveEventsOnUnusedChannels.png)

### Trim

[Trim](xref:Melanchall.DryWetMidi.Tools.SanitizingSettings.Trim) property allows to automatically remove an empty space at the start of a MIDI file. For example:

```csharp
var midiFile = new MidiFile(
    new TrackChunk(
        new TextEvent("A") { DeltaTime = 20 }),
    new TrackChunk(
        new TextEvent("B") { DeltaTime = 30 },
        new TextEvent("C") { DeltaTime = 15 }));
```

Here we have a silence of 20 ticks at the start of the file. So after sanitizing with `Trim` property set to `true` we will have this file:

```csharp
var midiFile = new MidiFile(
    new TrackChunk(
        new TextEvent("A") { DeltaTime = 0 }),
    new TrackChunk(
        new TextEvent("B") { DeltaTime = 10 },
        new TextEvent("C") { DeltaTime = 15 }));
```