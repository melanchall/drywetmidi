# Performance

`MidiFile.Read` and `MidiFile.Write` methods now work much faster (#64). For `MidiFile.Read` new setting `ReadingSettings.ReaderSettings.ReadFromMemory` was added:

```csharp
MidiFile.Read("file.mid", new ReadingSettings
{
    ReaderSettings = new ReaderSettings
    {
        ReadFromMemory = true
    }
});
```

With `ReadFromMemory` set to `true` entire MIDI file will be put to memory and read from there which gives big speed up.

# New features

* Added `IInputDevice` interface (#54), all methods that accept `InputDevice` as an argument accept now `IInputDevice`, which implemented by `InputDevice`. This interface gives ability to create custom input device implementations.
* Added `IOutputDevice` interface, all methods that accept `OutputDevice` as an argument accept now `IOutputDevice`, which implemented by `OutputDevice`. This interface gives ability to create custom output device implementations.
* Added `BytesToMidiEventConverter` class to convert bytes to MIDI events (#55).
* Added `MidiEventToBytesConverter` class to convert MIDI events to bytes.
* Added `Interaction.Chord.GetMusicTheoryChord` method.
* Added `Interaction.Note.GetMusicTheoryNote` method.
* Added `Interval.GetIntervalDefinitions` method returning collection of `IntervalDefinition` which is interval number and quality.
* Added `Interval.FromDefinition` method to create `Interval` from interval number and quality.
* Added `MusicTheory.Chord.GetNames` method returning names of a chord (for example, `A#` or `Ddim`) (#58).
* Added General MIDI Level 2 percussion API to `Melanchall.DryWetMidi.Standards` namespace (#65).
* Added `ReadingHandlers` property to `ReadingSettings` which provides collection of objects that handle MIDI data reading. Also added three handler classes:  
  * `TimedEventsReadingHandler`
  * `NotesReadingHandler`
  * `TempoMapReadingHandler`  
  
  These classes can speed up getting MIDI data from a MIDI file since information will be gathered during MIDI data reading rather than after data is read which involves additional iteration over MIDI data.
  
  You can create custom reading handler and process MIDI file reading stages.
  
* Added `UnknownChannelEventPolicy` property to `ReadingSettings` (#69) which specifies how reading engine should react on unknown channel event status byte. Also added `UnknownChannelEvent` property which specifies callback that will be called on unknown channel event if `UnknownChannelEventPolicy` set to `UnknownChannelEventPolicy.UseCallback`.
* Added `ReaderSettings` property to `ReadingSettings`. `ReaderSettings` holds I/O settings for MIDI data reader.
* Implemented reading MIDI file from non-seekable stream. Settings related to reading from such streams are placed at `ReadingSettings.ReaderSettings`. Names of properties corresponding to these settings are start with `NonSeekableStream`.
* Added `Equals` static methods for `MidiEvent`, `MidiChunk` and `MidiFile` classes.
* Added `PatternUtilities.TransformNotes` accepting `NoteSelection` which is predicate to select notes to transform.
* Added `PatternUtilities.TransformChords` accepting `ChordSelection` which is predicate to select chords to transform.
* Added `PatternUtilities.SetNotesState` method (#74).
* Added `PatternUtilities.SetChordsState` method.
* `Pattern.Clone` creates now deep copy instead of shallow one.
* Added `GetStandardChunkIds` static method to `MidiChunk`.
* Added `GetStandardMetaEventStatusBytes` static method to `MetaEvent`.

# Small changes and bug fixes

* Implemented `IComparable` on `Interval`.
* Implemented `IComparable` on `MusicTheory.Note`.
* Prevent creation of sysex event with status byte as a first data byte.
* **Fixed:** `Playback` looped on current time change inside `EventPlayed` event handler (#56).
* **Fixed:** `CsvConverter` closes passed stream after data is read or written (#73).
* **Fixed:** `OutputDevice.Volume` get/set fails (#75).
* **Fixed:** `ResetEvent` not received by input device.
* **Fixed:** Fixed CSV parsing with `\r` and `\n` in texts of meta events.