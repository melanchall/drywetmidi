---
uid: a_csv_serializer
---

# CSV serializer

With the [CsvSerializer](xref:Melanchall.DryWetMidi.Tools.CsvSerializer) you can either serialize objects to CSV or deserialize them back from CSV.

In this article the comma (`,`) used as a values delimiter. You can change it with the `Delimiter` property of the [CsvSerializationSettings](xref:Melanchall.DryWetMidi.Tools.CsvSerializationSettings) (for serialization) and [CsvDeserializationSettings](xref:Melanchall.DryWetMidi.Tools.CsvDeserializationSettings) (for deserialization).

## Example

Here is a quick example of CSV:

```text
0,"MThd",0,"Header",96
1,"MTrk",0,"Text",0,"A"
1,"MTrk",1,"Text",100,"B"
2,"MTrk",0,"Note",0,100,4,E7,127,0
2,"MTrk",1,"Note",100,100,3,D3,127,0
2,"MTrk",1,"Note",110,100,3,E2,127,0
```

The meaning of these lines:

* The file has the time division of 96 ticks per quarter note.
* Track chunk #1 contains two events:  
  * #0: _Text_ event with text of _A_;
  * #1: _Text_ event with text of _B_.
* Track chunk #2 contains two objects:  
  * #0: note _E7_;
  * #1: chord with two notes: _D3_ and _E2_.

See detailed description of the format in the next section.

## Format

Each record in CSV representation is in the following form:

```text
ChunkIndex,ChunkId,ObjectIndex,ObjectName,ObjectData
```

`ChunkIndex` is a number of a [MIDI chunk](xref:Melanchall.DryWetMidi.Core.MidiChunk) with an object. `ChunkId` is the ID of the chunk, for example, `"MTrk"` for a [track chunk](xref:Melanchall.DryWetMidi.Core.TrackChunk) or `"MThd"` for a header one.

`ObjectIndex` is a number of an object within the chunk. Also note that if two notes have the same `ObjectIndex`, it means they form a chord. For example:

```text
2,"MTrk",0,"Note",0,100,4,E7,127,0
2,"MTrk",1,"Note",100,100,3,D3,127,0
2,"MTrk",1,"Note",110,100,3,E2,127,0
```

Here we have a single note _E7_ (the object #0) and a chord (the object #1) with two notes: _D3_ and _E2_.

Note that when you're deserializing a MIDI file, you can put any number to `ObjectIndex`. This property is irrelevant for MIDI file deserialization, but required to correctly deserialize objects ([DeserializeObjectsFromCsv](xref:Melanchall.DryWetMidi.Tools.CsvSerializer.DeserializeObjectsFromCsv*)). The property exists to have single CSV schema for both file and objects deserialization.

If `ObjectName` is `"Header"` then `ObjectData` will contain a single number – [time division](xref:Melanchall.DryWetMidi.Core.TimeDivision).

If `ObjectName` is `"Note"` then `ObjectData` will be in the following format:

```text
Time,Length,Channel,Note,Velocity,OffVelocity
```

where `Time`, `Length`, `Channel`, `Velocity` and `OffVelocity` hold values of the corresponding properties of a [Note](xref:Melanchall.DryWetMidi.Interaction.Note) object. You can change `Time` and `Length` representations via [TimeType](xref:Melanchall.DryWetMidi.Tools.CsvSerializationSettings.TimeType) and [LengthType](xref:Melanchall.DryWetMidi.Tools.CsvSerializationSettings.LengthType) properties of the [CsvSerializationSettings](xref:Melanchall.DryWetMidi.Tools.CsvSerializationSettings) correspondingly (there are also the same properties in [CsvDeserializationSettings](xref:Melanchall.DryWetMidi.Tools.CsvDeserializationSettings) for deserialization). `Note` can be either the note number or letter (_C4_, for example) depending on the `NoteFormat` property value.

In other cases of `ObjectName` it's assumed that this component represents the [type](xref:Melanchall.DryWetMidi.Core.MidiEvent.EventType) of a MIDI event. For example, you can see `"NoteOn"` or `"SequenceTrackName"` as a value for `ObjectName`. `ObjectData` for such records will be in the following format:

```text
Time,EventData
```

As for the `Time`, it holds the time of a [TimedEvent](xref:Melanchall.DryWetMidi.Interaction.TimedEvent) object. You can change `Time` representation used for serialization via the [TimeType](xref:Melanchall.DryWetMidi.Tools.CsvSerializationSettings.TimeType) property of the [CsvSerializationSettings](xref:Melanchall.DryWetMidi.Tools.CsvSerializationSettings) (and the same property of the [CsvDeserializationSettings](xref:Melanchall.DryWetMidi.Tools.CsvDeserializationSettings) for deserialization). `EventData` depends on the type of an event:

|ObjectName|EventData|Modifiers|
|---|---|---|
|"SequenceTrackName"|[Text](xref:Melanchall.DryWetMidi.Core.BaseTextEvent.Text)||
|"CopyrightNotice"|[Text](xref:Melanchall.DryWetMidi.Core.BaseTextEvent.Text)||
|"InstrumentName"|[Text](xref:Melanchall.DryWetMidi.Core.BaseTextEvent.Text)||
|"Marker"|[Text](xref:Melanchall.DryWetMidi.Core.BaseTextEvent.Text)||
|"CuePoint"|[Text](xref:Melanchall.DryWetMidi.Core.BaseTextEvent.Text)||
|"Lyric"|[Text](xref:Melanchall.DryWetMidi.Core.BaseTextEvent.Text)||
|"Text"|[Text](xref:Melanchall.DryWetMidi.Core.BaseTextEvent.Text)||
|"ProgramName"|[Text](xref:Melanchall.DryWetMidi.Core.BaseTextEvent.Text)||
|"DeviceName"|[Text](xref:Melanchall.DryWetMidi.Core.BaseTextEvent.Text)||
|"SequenceNumber"|[Number](xref:Melanchall.DryWetMidi.Core.SequenceNumberEvent.Number)||
|"PortPrefix"|[Port](xref:Melanchall.DryWetMidi.Core.PortPrefixEvent.Port)||
|"ChannelPrefix"|[Channel](xref:Melanchall.DryWetMidi.Core.ChannelPrefixEvent.Channel)||
|"TimeSignature"|[Numerator](xref:Melanchall.DryWetMidi.Core.TimeSignatureEvent.Numerator),[Denominator](xref:Melanchall.DryWetMidi.Core.TimeSignatureEvent.Denominator),[ClocksPerClick](xref:Melanchall.DryWetMidi.Core.TimeSignatureEvent.ClocksPerClick),[ThirtySecondNotesPerBeat](xref:Melanchall.DryWetMidi.Core.TimeSignatureEvent.ThirtySecondNotesPerBeat)||
|"KeySignature"|[Key](xref:Melanchall.DryWetMidi.Core.KeySignatureEvent.Key),[Scale](xref:Melanchall.DryWetMidi.Core.KeySignatureEvent.Scale)||
|"SetTempo"|[MicrosecondsPerQuarterNote](xref:Melanchall.DryWetMidi.Core.SetTempoEvent.MicrosecondsPerQuarterNote)||
|"SmpteOffset"|[Format](xref:Melanchall.DryWetMidi.Core.SmpteOffsetEvent.Format),[Hours](xref:Melanchall.DryWetMidi.Core.SmpteOffsetEvent.Hours),[Minutes](xref:Melanchall.DryWetMidi.Core.SmpteOffsetEvent.Minutes),[Seconds](xref:Melanchall.DryWetMidi.Core.SmpteOffsetEvent.Seconds),[Frames](xref:Melanchall.DryWetMidi.Core.SmpteOffsetEvent.Frames),[SubFrames](xref:Melanchall.DryWetMidi.Core.SmpteOffsetEvent.SubFrames)||
|"SequencerSpecific"|[Data](xref:Melanchall.DryWetMidi.Core.SequencerSpecificEvent.Data)|BytesArray|
|"UnknownMeta"|[StatusByte](xref:Melanchall.DryWetMidi.Core.UnknownMetaEvent.StatusByte),[Data](xref:Melanchall.DryWetMidi.Core.UnknownMetaEvent.Data)|BytesArray|
|"NoteOn"|[Channel](xref:Melanchall.DryWetMidi.Core.ChannelEvent.Channel),[Note](xref:Melanchall.DryWetMidi.Core.NoteEvent.NoteNumber),[Velocity](xref:Melanchall.DryWetMidi.Core.NoteEvent.Velocity)|Note|
|"NoteOff"|[Channel](xref:Melanchall.DryWetMidi.Core.ChannelEvent.Channel),[Note](xref:Melanchall.DryWetMidi.Core.NoteEvent.NoteNumber),[Velocity](xref:Melanchall.DryWetMidi.Core.NoteEvent.Velocity)|Note|
|"PitchBend"|[Channel](xref:Melanchall.DryWetMidi.Core.ChannelEvent.Channel),[PitchValue](xref:Melanchall.DryWetMidi.Core.PitchBendEvent.PitchValue)||
|"ControlChange"|[Channel](xref:Melanchall.DryWetMidi.Core.ChannelEvent.Channel),[ControlNumber](xref:Melanchall.DryWetMidi.Core.ControlChangeEvent.ControlNumber),[ControlValue](xref:Melanchall.DryWetMidi.Core.ControlChangeEvent.ControlValue)||
|"ProgramChange"|[Channel](xref:Melanchall.DryWetMidi.Core.ChannelEvent.Channel),[ProgramNumber](xref:Melanchall.DryWetMidi.Core.ProgramChangeEvent.ProgramNumber)||
|"ChannelAftertouch"|[Channel](xref:Melanchall.DryWetMidi.Core.ChannelEvent.Channel),[AftertouchValue](xref:Melanchall.DryWetMidi.Core.ChannelAftertouchEvent.AftertouchValue)||
|"NoteAftertouch"|[Channel](xref:Melanchall.DryWetMidi.Core.ChannelEvent.Channel),[Note](xref:Melanchall.DryWetMidi.Core.NoteAftertouchEvent.NoteNumber),[AftertouchValue](xref:Melanchall.DryWetMidi.Core.NoteAftertouchEvent.AftertouchValue)|Note|
|"NormalSysEx"|[Data](xref:Melanchall.DryWetMidi.Core.SysExEvent.Data)|BytesArray|
|"EscapeSysEx"|[Data](xref:Melanchall.DryWetMidi.Core.SysExEvent.Data)|BytesArray|
|"ActiveSensing"|||
|"Start"|||
|"Stop"|||
|"Reset"|||
|"Continue"|||
|"TimingClock"|||
|"TuneRequest"|||
|"MidiTimeCode"|[Component](xref:Melanchall.DryWetMidi.Core.MidiTimeCodeEvent.Component),[ComponentValue](xref:Melanchall.DryWetMidi.Core.MidiTimeCodeEvent.ComponentValue)||
|"SongSelect"|[Number](xref:Melanchall.DryWetMidi.Core.SongSelectEvent.Number)||
|"SongPositionPointer"|[PointerValue](xref:Melanchall.DryWetMidi.Core.SongPositionPointerEvent.PointerValue)||

where **Modifiers** are:

* **BytesArray** – `Data` will be serialized as `"B1 B2 B3 ..."` where format of the string controlled by the `BytesArrayFormat` and `BytesArrayDelimiter` properties of the [CsvSerializationSettings](xref:Melanchall.DryWetMidi.Tools.CsvSerializationSettings) and [CsvDeserializationSettings](xref:Melanchall.DryWetMidi.Tools.CsvDeserializationSettings);
* **Note** – `Note` property is serialized as either the note number or letter (_C4_, for example) depending on the `NoteFormat` property of the [CsvSerializationSettings](xref:Melanchall.DryWetMidi.Tools.CsvSerializationSettings) and [CsvDeserializationSettings](xref:Melanchall.DryWetMidi.Tools.CsvDeserializationSettings).

By the way, when you're deserializing CSV data, you can set [TimeType](xref:Melanchall.DryWetMidi.Tools.CsvDeserializationSettings.TimeType), [LengthType](xref:Melanchall.DryWetMidi.Tools.CsvDeserializationSettings.LengthType) and [NoteFormat](xref:Melanchall.DryWetMidi.Tools.CsvDeserializationSettings.NoteFormat) properties of the [CsvDeserializationSettings](xref:Melanchall.DryWetMidi.Tools.CsvDeserializationSettings) to `null` to let the library detect formats automatically (which is the default behavior). Of course, you can tell the engine exact formats to simplify its work.

Note that if you deserialize a MIDI file from CSV, you don't need to manually sort records by time in text representation before deserialization. DryWetMIDI will do it for you. So, for example, these lines

```text
0,"MThd",0,"Header",96
1,"MTrk",0,"NoteOff",6/4,4,100,0
1,"MTrk",1,"Text",1/4,"A"
1,"MTrk",2,"NoteOn",3/4,4,100,127
1,"MTrk",3,"Text",2/4,"B"
```

will be correctly deserialized to the file with single track chunk containing following events:

1. _Text_ event with text of _A_ at time of `1/4`.
2. _Text_ event with text of _B_ at time of `2/4`.
3. _Note On_ event with note number `100` and velocity `127` at time of `3/4`.
4. _Note Off_ event with note number `100` and velocity `0` at time of `6/4`.