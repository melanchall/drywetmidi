---
uid: a_mt_octave
---

# Octave

The main purpose of the [Octave](xref:Melanchall.DryWetMidi.MusicTheory.Octave) class is to provide alternative way to get an instance of the [Note](xref:Melanchall.DryWetMidi.MusicTheory.Note) class. Some examples of usage:

```csharp
// Get first octave
var firstOctave = Octave.Get(1);

// Then we can get notes from this octave in a simple way
var aSharpNote = firstOctave.ASharp;
var bNote = firstOctave.B;

// Get middle C note
var middleC = Octave.Middle.C;
```

## Parsing

Following strings can be parsed to `Octave`:

`OctaveNumber`

where

* **OctaveNumber** is a number of octave. A number must be between `-1` and `9`.

Examples of valid interval strings:

`-1`  
`8`  
`0`