You can adjust process of reading a MIDI file with help of the `ReadingSettings` class. Create an instance of it, set some properties and pass the instance to the `Read` method of the `MidiFile`. Members of the `ReadingSettings` are described below.

### UnexpectedTrackChunksCountPolicy

Gets or sets reaction of the reading engine on unexpected track chunks count. The default is `UnexpectedTrackChunksCountPolicy.Ignore`. This policy will be taken into account if actual track chunks count is less or greater than tracks number specified in the file's header chunk. If `UnexpectedTrackChunksCountPolicy.Abort` is used an instance of the `UnexpectedTrackChunksCountException` will be thrown while loading the file if track chunks count is unexpected.

The type of this property is `UnexpectedTrackChunksCountPolicy` enum. Values are listed in the table below.

Value | Description
----- | -----------
Ignore | Ignore unexpected track chunks count.
Abort | Abort reading and throw an `UnexpectedTrackChunksCountException`.

### ExtraTrackChunkPolicy

Gets or sets reaction of the reading engine on new track chunk if already read track chunks count is greater or equals the one declared in the file's header chunk. The default is `ExtraTrackChunkPolicy.Read`.

The type of this property is `ExtraTrackChunkPolicy` enum. Values are listed in the table below.

Value | Description
----- | -----------
Read | Read the track chunk anyway.
Skip | Skip the chunk and go to the next one.

### UnknownChunkIdPolicy

Gets or sets reaction of the reading engine on chunk with unknown ID. The default is `UnknownChunkIdPolicy.ReadAsUnknownChunk`. If `UnknownChunkIdPolicy.Abort` is used an instance of the `UnknownChunkIdException` will be thrown if a chunk to be read has unknown ID.

The type of this property is `UnknownChunkIdPolicy` enum. Values are listed in the table below.

Value | Description
----- | -----------
ReadAsUnknownChunk | Read the chunk as `UnknownChunk`.
Skip | Skip this chunk and go to the next one.
Abort | Abort reading and throw an `UnknownChunkIdException`.

### MissedEndOfTrackPolicy

Gets or sets reaction of the reading engine on missed *End Of Track* event. The default is `MissedEndOfTrackPolicy.Ignore`. If `MissedEndOfTrackPolicy.Abort` is used an instance of the `MissedEndOfTrackEventException` will be thrown if track chunk doesn't end with *End Of Track* event. Although this event is not optional and therefore missing of it must be treated as error, you can try to read a track chunk relying on the chunk's size only.

The type of this property is `MissedEndOfTrackPolicy` enum. Values are listed in the table below.

Value | Description
----- | -----------
Ignore | Ignore missing of the *End Of Track* event and try to read a track chunk relying on the chunk's size.
Abort | Abort reading and throw an `MissedEndOfTrackEventException`.

### SilentNoteOnPolicy

Gets or sets reaction of the reading engine on *Note On* events with velocity 0. The default is `SilentNoteOnPolicy.NoteOff`. Although it is recommended to treat silent *Note On* event as *Note Off* you can turn this behavior off to get original event stored in the file.

The type of this property is `SilentNoteOnPolicy` enum. Values are listed in the table below.

Value | Description
----- | -----------
NoteOff | Read the *Note On* event with zero velocity as `NoteOffEvent`.
NoteOn | Read the *Note On* event as `NoteOnEvent`.

### InvalidChunkSizePolicy

Gets or sets reaction of the reading engine on difference between actual chunk's size and the one declared in its header. The default is `InvalidChunkSizePolicy.Abort`. If `InvalidChunkSizePolicy.Abort` is used an instance of the `InvalidChunkSizeException` will be thrown if actual chunk's size differs from the one declared in chunk's header.

The type of this property is `InvalidChunkSizePolicy` enum. Values are listed in the table below.

Value | Description
----- | -----------
Abort | Abort reading and throw an `InvalidChunkSizeException`.
Ignore | Ignore difference between actual chunk's size and the declared one.

### UnknownFileFormatPolicy

Gets or sets reaction of the reading engine on unknown file format stored in a header chunk. The default is `UnknownFileFormatPolicy.Ignore`. If `UnknownFileFormatPolicy.Abort` is used an instance of the `UnknownFileFormatException` will be thrown if file format stored in a header chunk doesn't belong to values defined by the `MidiFileFormat` enumeration.

The type of this property is `UnknownFileFormatPolicy` enum. Values are listed in the table below.

Value | Description
----- | -----------
Ignore | Ignore unknown file format and try to read chunks.
Abort | Abort reading and throw an `UnknownFileFormatException`.

### InvalidChannelEventParameterValuePolicy

Gets or sets reaction of the reading engine on invalid value of a channel event's parameter value. Valid values are 0-127 so, for example, 128 is the invalid one and will be processed according with this policy. The default is `InvalidChannelEventParameterValuePolicy.Abort`. If `InvalidChannelEventParameterValuePolicy.Abort` is used an instance of the `InvalidChannelEventParameterValueException` will be thrown if event's parameter value just read is invalid.

The type of this property is `InvalidChannelEventParameterValuePolicy` enum. Values are listed in the table below.

Value | Description
----- | -----------
Abort | Abort reading and throw an `InvalidChannelEventParameterValueException`.
ReadValid | Read byte and take its lower seven bits as the final value.
SnapToLimits | Read value and snap it to limits of the allowable range if it is out of them.

### InvalidMetaEventParameterValuePolicy

Gets or sets reaction of the reading engine on invalid value of a meta event's parameter value. For example, 255 is the invalid value for the Key of a Key SignatureEvent event and will be processed according with this policy. The default is
`InvalidMetaEventParameterValuePolicy.Abort`. If `InvalidMetaEventParameterValuePolicy.Abort` is used an instance of the `InvalidMetaEventParameterValueException` will be thrown if event's parameter value just read is invalid.

The type of this property is `InvalidMetaEventParameterValuePolicy` enum. Values are listed in the table below.

Value | Description
----- | -----------
Abort | Abort reading and throw an `InvalidMetaEventParameterValueException`.
SnapToLimits | Read value and snap it to limits of the allowable range if it is out of them.

### NotEnoughBytesPolicy

Gets or sets reaction of the reading engine on lack of bytes in the underlying stream that are needed to read some value (for example, DWORD requires 4 bytes available). The default is `NotEnoughBytesPolicy.Abort`. If `NotEnoughBytesPolicy.Abort` is used an instance of the `NotEnoughBytesException` will be thrown if the reader's underlying stream doesn't have enough bytes to read a value.

The type of this property is `NotEnoughBytesPolicy` enum. Values are listed in the table below.

Value | Description
----- | -----------
Abort | Abort reading and throw an `NotEnoughBytesException`.
Ignore | Ignore lack of bytes in the reader's underlying stream and just end reading.

### NoHeaderChunkPolicy

Gets or sets reaction of the reading engine on missing of the header chunk in the MIDI file. The default is `NoHeaderChunkPolicy.Abort`. If `NoHeaderChunkPolicy.Abort` is used an instance of the `NoHeaderChunkException` will be thrown if the MIDI file doesn't contain the header chunk. If `NoHeaderChunkPolicy.Ignore` is used the result file's `TimeDivision` will be null and `OriginalFormat` will throw an [InvalidOperationException](https://msdn.microsoft.com/library/system.invalidoperationexception(v=vs.110).aspx).

The type of this property is `NoHeaderChunkPolicy` enum. Values are listed in the table below.

Value | Description
----- | -----------
Abort | Abort reading and throw an `NoHeaderChunkException`.
Ignore | Ignore missing of the header chunk.

### CustomChunksTypes

Gets or sets collection of custom chunks types. These types must be derived from the `MidiChunk` class and have parameterless constructor. No exception will be thrown if some types don't meet these requirements.

The type of this property is `ChunkTypesCollection`.

Read the [Custom chunks](Custom-chunks.md) page to learn more about custom chunks support.

### CustomMetaEventTypes

Gets or sets collection of custom meta events types. These types must be derived from the `MetaEvent` class and have parameterless constructor. No exception will be thrown while reading a MIDI file if some types don't meet these requirements.

The type of this property is `EventTypesCollection`.

Read the [Custom meta events](Custom-meta-events.md) page to learn more about custom meta events support.

### TextEncoding

Gets or sets an encoding that will be used to read the text of a text-based meta event like _Sequence/Track Name_ or _Lyric_. The default is [`Encoding.ASCII`](https://msdn.microsoft.com/library/system.text.encoding.ascii(v=vs.110).aspx).

The type of this property is [`Encoding`](https://msdn.microsoft.com/library/system.text.encoding(v=vs.110).aspx).

### DecodeTextCallback

Gets or sets a callback used to decode a string from the specified bytes during reading a text-based meta event. The default is `null`.

The type of this property is `DecodeTextCallback` defined as:

```csharp
delegate string DecodeTextCallback(byte[] bytes, ReadingSettings settings)
```