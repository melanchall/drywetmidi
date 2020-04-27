---
uid: a_mt_interval
---

# Interval

[Interval](xref:Melanchall.DryWetMidi.MusicTheory.Interval) holds number of half steps and used, for example, to transpose notes or describing chords when working with [PatternBuilder](xref:Melanchall.DryWetMidi.Composing.PatternBuilder). Some examples of usage:

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

## Parsing

Following strings can be parsed to `Interval`:

* `HalfSteps`
* `IntervalQuality IntervalNumber`

where

* **HalfSteps** is a number of half-steps, for example, `+4` or `-10`.
* **IntervalQuality** is one of the following letters:  
  * `P` for perfect interval;
  * `d` for diminished interval;
  * `A` for augmented interval;
  * `m` for minor interval;
  * `M` for major interval.
* **IntervalNumber** is a number of interval, for example, `1`. A number must be greater than zero.

Examples of valid interval strings:

`+8`  
`0`  
`7`  
`-123`  
`P5`  
`m3`  
`M3`  
`D21`  
`d8`  
`A7`  
`a18`  