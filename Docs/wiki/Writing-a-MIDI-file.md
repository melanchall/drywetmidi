To write MIDI data to a file you have to use `Write` method of the `MidiFile` class

```csharp
public void Write(string filePath,
                  bool overwriteFile = false,
                  MidiFileFormat format = MidiFileFormat.MultiTrack,
                  WritingSettings settings = null)
```

or

```csharp
public void Write(Stream stream,
                  MidiFileFormat format = MidiFileFormat.MultiTrack,
                  WritingSettings settings = null)
```

### Parameters

Name | Type | Description
---- | ---- | -----------
filePath | [string](https://msdn.microsoft.com/library/system.string(v=vs.110).aspx) | Full path of the file to write to.
stream | [Stream]((https://msdn.microsoft.com/library/system.io.stream(v=vs.110).aspx)) | Stream to write file's data to. Stream must support writing.
overwriteFile | [bool](https://msdn.microsoft.com/library/system.boolean.aspx) | If true and file specified by *filePath* already exists it will be overwritten; if false and the file exists exception will be thrown.
format | MidiFileFormat | MIDI file format to write in.
settings | [WritingSettings](Writing-settings.md) | Settings according to which the file must be written.

### Exceptions

Type | Description
---- | -----------
[ArgumentException](https://msdn.microsoft.com/library/system.argumentexception(v=vs.110).aspx) | *filePath* is a zero-length string, contains only white space, or contains one or more invalid characters as defined by [System.IO.Path.InvalidPathChars](https://msdn.microsoft.com/library/system.io.path.invalidpathchars(v=vs.110).aspx). *-or-* *stream* doesn't support writing.
[ArgumentNullException](https://msdn.microsoft.com/library/system.argumentnullexception(v=vs.110).aspx) | *filePath* is null. *-or* *stream* is null.
[InvalidEnumArgumentException](https://msdn.microsoft.com/library/system.componentmodel.invalidenumargumentexception(v=vs.110).aspx) | *format* specified an invalid value.
[PathTooLongException](https://msdn.microsoft.com/library/system.io.pathtoolongexception(v=vs.110).aspx) | The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.
[DirectoryNotFoundException](https://msdn.microsoft.com/library/system.io.directorynotfoundexception(v=vs.110).aspx) | The specified path is invalid, (for example, it is on an unmapped drive).
[IOException](https://msdn.microsoft.com/library/system.io.ioexception(v=vs.110).aspx) | An I/O error occurred while writing the file.
[ObjectDisposedException](https://msdn.microsoft.com/library/system.objectdisposedexception(v=vs.110).aspx) | *stream* is disposed. *-or-* Underlying stream writer is disposed.
[NotSupportedException](https://msdn.microsoft.com/library/system.notsupportedexception(v=vs.110).aspx) | *filePath* is in an invalid format.
[UnauthorizedAccessException](https://msdn.microsoft.com/library/system.unauthorizedaccessexception(v=vs.110).aspx) | This operation is not supported on the current platform. *-or-* *filePath* specified a directory. *-or-* The caller does not have the required permission.
[InvalidOperationException](https://msdn.microsoft.com/library/system.invalidoperationexception(v=vs.110).aspx) | Time division is null.
TooManyTrackChunksException | Count of track chunks presented in the file exceeds maximum value allowed for a MIDI file.
