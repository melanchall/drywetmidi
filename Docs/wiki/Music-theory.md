DryWetMIDI has some classes that represent music theory subjects. These classes are placed in `Melanchall.DryWetMidi.MusicTheory` namespace and actively used by `PatternBuilder` class (read [Pattern](Pattern.md) article to learn more). Let's look at them.

### Note

`Note` class holds a note's name and octave number. To get an instance of `Note`, call appropriate `Get` static method or parse a note from string:

```csharp
// Get note with number of 100
var note1 = Note.Get((SevenBitNumber)100);

// Get A#2 note
var note2 = Note.Get(NoteName.ASharp, 2);

// Parse a note
var note3 = Note.Parse("a3");
var note4Parsed = Note.TryParse("c#2", out var note4);
```

Some manipulations with notes are described in [Interval](#interval) section.

There is also `Note` class inside `Melanchall.DryWetMidi.Interaction` namespace so pay attention what `Note` class you are using.

### Octave

The main purpose of the `Octave` class is to provide alternative way to get an instance of `Note`:

```csharp
// Get first octave
var firstOctave = Octave.Get(1);

// Then we can get notes from this octave in a simple way
var aSharpNote = firstOctave.ASharp;
var bNote = firstOctave.B;
```

### Interval

`Interval` holds number of half steps and used, for example, to transpose notes or describing chords when working with `PatternBuilder`. Examples of usage:

```csharp
// Get A4 note
var a4 = Octave.Get(4).A;

// Transpose the note up by two half steps
var twoHalfTonesUp = Interval.FromHalfSteps(2);
var b4 = a4 + twoHalfTonesUp;

// Transpose the note down by two octaves
var twoOctavesDown = Interval.FromHalfSteps(-24);
var a2 = a4.Transpose(twoOctavesDown);

// Transpose the note up by three half steps
var c5 = a4 + Interval.Three;

// Invert interval
var twoOctavesUp = -twoOctavesDown;
```

### Scale

Using `Scale` class you can get notes laying in specific musical scale. The following code shows what you can do with scales:

```csharp
// Create C Major scale
var cMajorScale = new Scale(ScaleIntervals.Major, NoteName.C);

// Get a note that belongs to C Major scale next to the C2.
var d2 = cMajorScale.GetNextNote(Octave.Get(2).C);

// Get a note that belongs to C Major scale previous to the F2.
var e2 = cMajorScale.GetPreviousNote(Octave.Get(2).F);

// Get note by scale degree
var c = cMajorScale.GetDegree(ScaleDegree.Tonic);

// Get ten ascending notes that belong to C Major scale starting from D2
var tenAscendingNotes = cMajorScale.GetAscendingNotes(Octave.Get(2).D).Take(10);

// Get five descending notes that belong to C Major scale starting from F4
var fiveDescendingNotes = cMajorScale.GetDescendingNotes(Octave.Get(4).F).Take(5);

// Parse a scale
var dMinor = Scale.Parse("D minor");

// Create custom scale by the specified intervals between adjacent notes and root note
var customScale1 = new Scale(new[] { Interval.One, Interval.Seven, Interval.Two }, NoteName.CSharp);

// Parse custom scale
var customScaleParsed = Scale.TryParse("C# +1 +7 +2", out var customScale2);
```

There are other useful methods placed in `ScaleUtilities` class.