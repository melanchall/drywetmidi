You can adjust process of writing a MIDI file with help of the `WritingSettings` class. Create an instance of it, set some properties and pass the instance to the `Write` method of the `MidiFile`. Members of the `WritingSettings` are described below.

### CompressionPolicy

Gets or sets compression rules for the writing engine. The default is `CompressionPolicy.NoCompression`. The type of this property is `CompressionPolicy` enum. It's a flags enum so you can combine its values with | operator. Values are listed in the table below.

Value | Description
----- | -----------
NoCompression | Don't use any compression on the MIDI data to write.
UseRunningStatus | Use *running status* to turn off writing of the status bytes of consecutive events of the same type.
NoteOffAsSilentNoteOn | Turn *Note Off* events into the *Note On* ones with zero velocity. Note that it helps to compress MIDI data in the case of `UseRunningStatus` is used only.
DeleteDefaultTimeSignature | Don't write default *Time Signature* event. Default signature is 4/4 with 24 clocks count and 8 32nd notes per beat.
DeleteDefaultKeySignature | Don't write default *Key Signature* event. Default signature is C major which is expressed as 0 for key and 0 for scale.
DeleteDefaultSetTempo | Don't write default *Set Tempo* event. Default tempo is 500,000 microseconds per beat.
DeleteUnknownMetaEvents | Don't write instances of the `UnknownMetaEvent`.
DeleteUnknownChunks | Don't write instances of the `UnknownChunk`.
Default | Use default compression on the MIDI data to write. This option is combination of other ones and turns on all options that don't lead to data losing (for example, unknown meta events): `UseRunningStatus`, `NoteOffAsSilentNoteOn`, `DeleteDefaultTimeSignature`, `DeleteDefaultKeySignature`, `DeleteDefaultSetTempo`.

### CustomMetaEventTypes

Gets or sets collection of custom meta events types. These types must be derived from the `MetaEvent` class and have parameterless constructor. No exception will be thrown while writing a MIDI file if some types don't meet these requirements.

The type of this property is `EventTypesCollection`.

Read the [Custom meta events](Custom-meta-events.md) page to learn more about custom meta events support.

### TextEncoding

Gets or sets an encoding that will be used to write the text of a text-based meta event like _Sequence/Track Name_ or _Lyric_. The default is [`Encoding.ASCII`](https://msdn.microsoft.com/library/system.text.encoding.ascii(v=vs.110).aspx).

The type of this property is [`Encoding`](https://msdn.microsoft.com/library/system.text.encoding(v=vs.110).aspx).