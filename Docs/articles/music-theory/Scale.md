---
uid: a_mt_scale
---

# Scale

DryWetMIDI provides [Scale](xref:Melanchall.DryWetMidi.MusicTheory.Scale) class to work with musical scales. Some examples of usage:

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

Last example shows parsing of custom scale defined by tonic (C#) and intervals between adjacent degrees (+1, +7 and +2). So notes of the scale are C#, D, A and B.

## Parsing

Following strings can be parsed to `Scale`:

* `RootNote ScaleName`
* `RootNote Interval Interval ... Interval`

where

* **RootNote** is a note name (with or without accidental), for example, `C` or `A#`.
* **ScaleName** is the name of a known scale, for example, `major` or `bebop minor`. You can take a look at [ScaleIntervals](xref:Melanchall.DryWetMidi.MusicTheory.ScaleIntervals) to see all known scales supported by the library.
* **Interval** is a string a musical interval can be parsed from. See [Parsing](Interval.md#parsing) section of the [Interval](Interval.md) article to see how an interval can be represented as a string.

Examples of valid scale strings:

`C major`  
`D blues`  
`A# P5 d7`  
`Bb +3 +1 +4`  