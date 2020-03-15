To read a MIDI file you have to use `Read` static method of the `MidiFile` class

```csharp
public static MidiFile Read(string filePath, ReadingSettings settings = null)
```

or

```csharp
public static MidiFile Read(Stream stream, ReadingSettings settings = null)
```

### Parameters

Name | Type | Description
---- | ---- | -----------
filePath | [string](https://msdn.microsoft.com/library/system.string(v=vs.110).aspx) | Path to the file to read.
stream | [Stream](https://msdn.microsoft.com/library/system.io.stream(v=vs.110).aspx) | Stream to read file from. Stream must be readable, seekable and be able to provide its position and length via [Stream.Position](https://msdn.microsoft.com/library/system.io.stream.position(v=vs.110).aspx) and [Stream.Length](https://msdn.microsoft.com/library/system.io.stream.length(v=vs.110).aspx) properties.
settings | [ReadingSettings](Reading-settings.md) | Settings according to which the file must be read. You can pass `null` to this parameter to use default readings settings.

### Return value

An instance of the `MidiFile` class representing a MIDI file.

### Exceptions

Type | Description
---- | -----------
[ArgumentException](https://msdn.microsoft.com/library/system.argumentexception(v=vs.110).aspx) | *filePath* is a zero-length string, contains only white space, or contains one or more invalid characters as defined by [System.IO.Path.InvalidPathChars](https://msdn.microsoft.com/library/system.io.path.invalidpathchars(v=vs.110).aspx). *-or-* *stream* doesn't support reading. *-or* *stream* doesn't support seeking. *-or* *stream* is already read.
[ArgumentNullException](https://msdn.microsoft.com/library/system.argumentnullexception(v=vs.110).aspx) | *filePath* is null. *-or* *stream* is null.
[PathTooLongException](https://msdn.microsoft.com/library/system.io.pathtoolongexception(v=vs.110).aspx) | The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.
[DirectoryNotFoundException](https://msdn.microsoft.com/library/system.io.directorynotfoundexception(v=vs.110).aspx) | The specified path is invalid, (for example, it is on an unmapped drive).
[IOException](https://msdn.microsoft.com/library/system.io.ioexception(v=vs.110).aspx) | An I/O error occurred while reading the file.
[ObjectDisposedException](https://msdn.microsoft.com/library/system.objectdisposedexception(v=vs.110).aspx) | *stream* is disposed. *-or-* Underlying stream reader is disposed.
[NotSupportedException](https://msdn.microsoft.com/library/system.notsupportedexception(v=vs.110).aspx) | *filePath* is in an invalid format.
[UnauthorizedAccessException](https://msdn.microsoft.com/library/system.unauthorizedaccessexception(v=vs.110).aspx) | This operation is not supported on the current platform. *-or-* *filePath* specified a directory. *-or-* The caller does not have the required permission.
NoHeaderChunkException | There is no header chunk in a file and that should be treated as error accordng to the *settings*.
InvalidChunkSizeException | Actual header or track chunk's size differs from the one declared in its header and that should be treated as error according to the *settings*.
UnknownChunkException | Chunk to be read has unknown ID and that should be treated as error accordng to the *settings*.
UnexpectedTrackChunksCountException | Actual track chunks count differs from the expected one and that should be treated as error according to the specified *settings*.
UnknownFileFormatException | The header chunk contains unknown file format and that should be treated as error accordng to the *settings*.
InvalidChannelEventParameterValueException | Value of a channel event's parameter just read is invalid and that should be treated as error accordng to the *settings*.
InvalidMetaEventParameterValueException | Value of a meta event's parameter just read is invalid and that should be treated as error accordng to the *settings*.
UnknownChannelEventException | Reader has encountered an unknown channel event.
NotEnoughBytesException | MIDI file cannot be read since the reader's underlying stream doesn't have enough bytes and that should be treated as error accordng to the *settings*.
UnexpectedRunningStatusException | Unexpected running status is encountered.
MissedEndOfTrackEventException | Track chunk doesn't end with *End Of Track* event and that should be treated as error accordng to the specified *settings*.

---------------

Once you've read the file you can manage its content via `Chunks` property. It may contain instances of following types:
* `TrackChunk`;
* `UnknownChunk`;
* any of the types passed to `CustomChunksTypes` property of the `ReadingSettings` was used to read the file (read the [Custom chunks](Custom-chunks.md) page to learn more about custom chunks support). Visit [Chunks](Chunks.md) page to learn more.

Along with low level data managing mentioned above you can use high-level objects like `TimedEvent`, `Note` and `Chord` and also different time and length representation classes, for example, `MusicalTimeSpan` or `MetricTimeSpan`. Read the **High-level data managing** section of the Wiki to learn more.