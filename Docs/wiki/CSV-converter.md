DryWetMIDI provides a way to convert MIDI objects to CSV representation and read them back. CSV allows you to edit MIDI data, for example, via Microsoft Excel or to write it to database. `CsvConverter` class performs such conversions. Let's see what methods it provides.

### `ConvertMidiFileToCsv`

```csharp
void ConvertMidiFileToCsv(MidiFile midiFile,
                          string filePath,
                          bool overwriteFile = false,
                          MidiFileCsvConversionSettings settings = null)
```
```csharp
void ConvertMidiFileToCsv(MidiFile midiFile,
                          Stream stream,
                          MidiFileCsvConversionSettings settings = null)
```

Converts `MidiFile` to CSV representation and writes it to either file or stream. CSV output can be different depending on provided `settings`. Properties of the `MidiFileCsvConversionSettings` are described below.

#### `CsvLayout`

Layout of CSV representation of a MIDI file. The default value is `MidiFileCsvLayout.DryWetMidi`. At now there are two layouts: `MidiFileCsvLayout.DryWetMidi` and `MidiFileCsvLayout.MidiCsv` which produces slightly different CSV representations. `MidiFileCsvLayout.DryWetMidi` gives more compact and more human readable CSV data. `MidiFileCsvLayout.MidiCsv` produces CSV representation in format used by [midicsv](http://www.fourmilab.ch/webtools/midicsv/) tool.

#### `TimeType`

Format of timestamps inside CSV representation. The default value is `TimeSpanType.Midi`. Note that it is recommended to use `TimeSpanType.Midi` if you use `MidiFileCsvLayout.MidiCsv` CSV layout to ensure produced CSV data can be read by other readers that supports format used by [midicsv](http://www.fourmilab.ch/webtools/midicsv/) program. Available options are:

Value | Description
----- | -----------
`Metric` | Time as hours, minutes, seconds.
`Musical` | Time as fraction of the whole note's length.
`BarBeatTicks` | Time as number of bars, beats and ticks.
`BarBeatFraction` | Time as number of bars and fractional beats.
`Midi` | Time as number of MIDI ticks.

#### `CsvDelimiter`

Delimiter used to separate values in CSV representation. The default value is `','` (comma).

#### `NoteLengthType`

The type of a note length (metric, bar/beat and so on) which should be used to write to or read from CSV. The default value is `TimeSpanType.Midi`. Possible values are listed in [TimeType](#timetype) section.

#### `NoteFormat`

The format which should be used to write notes to or read them from CSV. The default value is `NoteFormat.Events`. Available options are:

Value | Description
----- | -----------
`Note` | Notes are presented in CSV as note objects.
`Events` | Notes are presented in CSV as _Note On_ / _Note Off_ events.

#### `NoteNumberFormat`

The format which should be used to write a note's number to or read it from CSV. The default value is `NoteNumberFormat.NoteNumber`. Available options are:

Value | Description
----- | -----------
`NoteNumber` | A note's number is presented as just a number.
`Letter` | A note's number is presented as a letter (for example, _A#4_).

### `ConvertCsvToMidiFile`

```csharp
MidiFile ConvertCsvToMidiFile(string filePath,
                              MidiFileCsvConversionSettings settings = null)
```
```csharp
MidiFile ConvertCsvToMidiFile(Stream stream,
                              MidiFileCsvConversionSettings settings = null)
```

Converts CSV representation of a MIDI file to `MidiFile` reading it from either file or stream. `settings` parameter specifies format of CSV data so MIDi file can be correctly read. Available options are described in the previous section.

### `ConvertNotesToCsv`

```csharp
void ConvertNotesToCsv(IEnumerable<Note> notes,
                       string filePath,
                       TempoMap tempoMap,
                       bool overwriteFile = false,
                       NoteCsvConversionSettings settings = null)
```
```csharp
void ConvertNotesToCsv(IEnumerable<Note> notes,
                       Stream stream,
                       TempoMap tempoMap,
                       NoteCsvConversionSettings settings = null)
```

Converts collection of `Note` to CSV representation and writes it to either file or stream. CSV output can be different depending on provided `settings`. Properties of the `NoteCsvConversionSettings` are:

* [`CsvDelimiter`](#csvdelimiter)
* [`TimeType`](#timetype)
* [`NoteLengthType`](#notelengthtype)
* [`NoteNumberFormat`](#notenumberformat)

All these properties are described at the [`ConvertMidiFileToCsv`](#convertmidifiletocsv).

### `ConvertCsvToNotes`

```csharp
IEnumerable<Note> ConvertCsvToNotes(string filePath,
                                    TempoMap tempoMap,
                                    NoteCsvConversionSettings settings = null)
```
```csharp
IEnumerable<Note> ConvertCsvToNotes(Stream stream,
                                    TempoMap tempoMap,
                                    NoteCsvConversionSettings settings = null)
```

Converts CSV representation of a notes collection file to `IEnumerable<Note>` reading it from either file or stream. `settings` parameter specifies format of CSV data so notes can be correctly read. Available options are described in the previous section.