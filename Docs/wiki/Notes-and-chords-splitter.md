With DryWetMIDI notes or chords can be easily splitted using following classes:
* `NotesSplitter`
* `ChordsSplitter`

Both classes provide the same set of methods giving you different ways objects can be splitted:
* [`SplitByStep`](#splitbystep)
* [`SplitByPartsNumber`](#splitbypartsnumber)
* [`SplitByGrid`](#splitbygrid)
* [`SplitAtDistance`](#splitatdistance)

Each method takes collection of objects and returns objects of the same type that are result of splitting the input ones. These methods are discussed in details below. Also there are useful methods in `NotesSplitterUtilities` and `ChordsSplitterUtilities` classes which allow to split objects inside `TrackChunk` or `MidiFile` without necessity to work with collection of notes or chords directly.

### `SplitByStep`

```csharp
IEnumerable<TObject> SplitByStep(IEnumerable<TObject> objects,
                                 ITimeSpan step,
                                 TempoMap tempoMap)
```
where `TObject` either `Note` (for `NotesSplitter`) or `Chord` (for `ChordsSplitter`).

This method splits each object by the specified step starting at the start of an object. For example, if step is 1 second, an object will be splitted at 1 second from its start, at 1 second from previous point of splitting (2 seconds from the object's start), at 1 second from previous point of splitting (3 seconds from the object's start) and so on. If an object's length is less than the specified step, the object will not be splitted and copy of it will be returned. The image below illustrates splitting notes and chord by the same step:

![Split by step](Images/LengthedObjectsSplitter/SplitByStep.png)

### `SplitByPartsNumber`

```csharp
IEnumerable<TObject> SplitByPartsNumber(IEnumerable<TObject> objects,
                                        int partsNumber,
                                        TimeSpanType lengthType,
                                        TempoMap tempoMap)
```
where `TObject` either `Note` (for `NotesSplitter`) or `Chord` (for `ChordsSplitter`).

This method splits each object into the specified number of parts of equal length. It is necessary to specify the `lengthType` argument to meet your expectations. For example, with metric type each part of an input object will last the same number of microseconds, while with musical type each part's length will represent the same fraction of the whole note's length. But the length of parts can be different in terms of MIDI ticks using different length type depending on tempo map passed to the method. The image below illustrates splitting notes and chord into 4 parts:

![Split by parts number](Images/LengthedObjectsSplitter/SplitByPartsNumber.png)

### `SplitByGrid`

```csharp
IEnumerable<TObject> SplitByGrid(IEnumerable<TObject> objects,
                                 IGrid grid,
                                 TempoMap tempoMap)
```
where `TObject` either `Note` (for `NotesSplitter`) or `Chord` (for `ChordsSplitter`).

This method splits each object by the specified grid. Objects will be splitted in points of crossing the specified grid. Grid can be an instance of one of the following classes:
* `SteppedGrid` for grid started at the specified time where times distanced from each other with the specified steps;
* `ArbitraryGrid` for grid where collection of times is specified as the constructor's argument.

The image below illustrates splitting notes and chord by the same grid:

![Split by grid](Images/LengthedObjectsSplitter/SplitByGrid.png)

### `SplitAtDistance`

```csharp
IEnumerable<TObject> SplitAtDistance(IEnumerable<TObject> objects,
                                     ITimeSpan distance,
                                     LengthedObjectTarget from,
                                     TempoMap tempoMap)
```
```csharp
IEnumerable<TObject> SplitAtDistance(IEnumerable<TObject> objects,
                                     double ratio,
                                     TimeSpanType lengthType,
                                     LengthedObjectTarget from,
                                     TempoMap tempoMap)
```
where `TObject` either `Note` (for `NotesSplitter`) or `Chord` (for `ChordsSplitter`).

This methods splits each object at the specified distance or by the specified ratio from start or end of an object (which is defined by `from` parameter). It is necessary to specify the `lengthType` argument for splitting by ratio to meet your expectations. The image below illustrates splitting notes and chord at the same distance from the start of an object:

![Split at distance by step from start](Images/LengthedObjectsSplitter/SplitAtDistanceByStepFromStart.png)

Next image illustrates splitting notes and chord by the same ratio (`0.25`) from the end of an object:

![Split at distance by ratio from end](Images/LengthedObjectsSplitter/SplitAtDistanceByRatioFromEnd.png)

### `Note.Split` and `Chord.Split`

Note or chord can also be splitted by the specified time into two parts without using `NotesSplitter` and `ChordsSplitter`. `Note` class has the method:

```csharp
SplittedLengthedObject<Note> Split(long time)
```

and `Chord` class has the method:

```csharp
SplittedLengthedObject<Chord> Split(long time)
```

`SplittedLengthedObject<TObject>` holds left and right parts of the object was splitted. If `time` is less than start time of an object, the left part will be `null`. If `time` is greater than end time of an object, the right part will be `null`.