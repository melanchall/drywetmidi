---
uid: a_mt_chord_progression
---

# Chord progression

[ChordProgression](xref:Melanchall.DryWetMidi.MusicTheory.ChordProgression) represents set of [chords](Chord.md) which represents musical chord progression. Some examples of usage:

```csharp
// Get I-II-IV chord progression for C Major scale
var chordProgression = ChordProgression.Parse("I-II-IV", Scale.Parse("C major"));

// Get chords of chord progression
var chords = chordProgression.Chords;
```

## Parsing

Following strings can be parsed to `ChordProgression`:

`DegreeNumber ChordCharacteristic - DegreeNumber ChordCharacteristic - ... - DegreeNumber ChordCharacteristic`

where

* **DegreeNumber** is a scale degree as a roman number, for example, `I` or `IV`.
* **ChordCharacteristic** is combination of chord quality, altered and added tones and so on. See [Parsing](Chord.md#parsing) section of [Chord](Chord.md) article to learn more.

Examples of valid chord progression strings:

`I-II-IV`  
`Im-II7-V`