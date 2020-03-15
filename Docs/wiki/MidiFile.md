`MidiFile` class represents a MIDI file. You can get an instance of it with `Read` static method to manage content of an existing file or by calling one of `MidiFile`'s constructors to create a MIDI file from scratch.

## Constructors

```csharp
public MidiFile()
```

Initializes a new instance of the `MidiFile` with the default time division and empty collection of chunks. Default time division is 96 ticks per quarter note.

```csharp
public MidiFile(IEnumerable<MidiChunk> chunks)
```

Initializes a new instance of the `MidiFile` with the default time division the specified chunks.

Note that header chunks cannot be added into the collection since it may cause inconsistence in the file structure. Header chunk with appropriate information will be written to a file automatically on `MidiFile.Write`.

Exception | Description
--------- | -----------
[ArgumentNullException](https://msdn.microsoft.com/library/system.argumentnullexception(v=vs.110).aspx) | *chunks* is null.
[ArgumentException](https://msdn.microsoft.com/library/system.argumentexception(v=vs.110).aspx) | *chunks* contain null.

## Properties

### TimeDivision

```csharp
public TimeDivision TimeDivision { get; set; }
```

Gets or sets a time division of a MIDI file. Time division specifies the meaning of the delta-times of events. There are two types of the time division: ticks per quarter note and SMPTE. The first type represented in the DryWetMIDI by `TicksPerQuarterNoteTimeDivision` class and the second one represented by `SmpteTimeDivision` class.

Default value is an instance of the `TicksPerQuarterNoteTimeDivision` with 96 ticks per quarter note. If the file was read without the header chunk presented in it the `TimeDivision` will be set to null.

Type of this property is `TimeDivision` which is base class for all the divisions described above.

### Chunks

```csharp
public ChunksCollection Chunks { get; }
```

Gets collection of chunks of a MIDI file. MIDI Files are made up of chunks. Сollection returned by this property may contain chunks of the following types: `TrackChunk`, `UnknownChunk`, and any custom chunk types you've defined (visit [Custom chunks](Custom-chunks.md) page to learn more).

This property returns collection which always is not null so it is safe to omit checking it for null every time you want to use its members.

Type of this property is `ChunksCollection` which contains methods to manage chunks of the file.

### OriginalFormat

```csharp
public MidiFileFormat OriginalFormat { get; }
```

Gets original format of the file was read. This property will throw an [InvalidOperationException](https://msdn.microsoft.com/library/system.invalidoperationexception(v=vs.110).aspx) if the `MidiFile` was created by constructor or was read without header chunk presented in it.

Type of this property is `MidiFileFormat`.

Exception | Description
--------- | -----------
UnknownFileFormatException | File format is unknown.
[InvalidOperationException](https://msdn.microsoft.com/library/system.invalidoperationexception(v=vs.110).aspx) | Unable to get original format of the file.

## Methods

### Read

```csharp
public static MidiFile Read(string filePath, ReadingSettings settings = null)
```

Reads a MIDI file specified by its full path. Visit [Reading a MIDI file](Reading-a-MIDI-file.md) page to get more information about this method.

### Write

```csharp
public void Write(string filePath,
                  bool overwriteFile = false,
                  MidiFileFormat format = MidiFileFormat.MultiTrack,
                  WritingSettings settings = null)
```

Writes the MIDI file to location specified by full path. Visit [Writing a MIDI file](Writing-a-MIDI-file.md) page to get more information about this method.