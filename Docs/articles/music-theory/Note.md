---
uid: a_mt_note
---

# Note

[Note](xref:Melanchall.DryWetMidi.MusicTheory.Note) class holds a note's name and octave number. To get an instance of `Note`, call appropriate `Get` (for example, [Get(noteName, octave)](xref:Melanchall.DryWetMidi.MusicTheory.Note.Get(Melanchall.DryWetMidi.MusicTheory.NoteName,System.Int32))) static method or parse a note from string. You can also get specific note using corresponding property of [Octave](xref:Melanchall.DryWetMidi.MusicTheory.Octave) class.

Some examples of usage:

```csharp
// Get note with number of 100
var note1 = Note.Get((SevenBitNumber)100);

// Get A#2 note
var note2 = Note.Get(NoteName.ASharp, 2);

// ... or
var note3 = Octave.Get(2).ASharp;

// Parse a note
var note4 = Note.Parse("a3");
var note5Parsed = Note.TryParse("c#2", out var note5);
```

Also see [Interval](Interval.md) article for additional examples.

## Parsing

Following strings can be parsed to `Note`:

`NoteName Accidental Accidental ... Accidental OctaveNumber`

where

* **NoteName** is one of the letters: `C`, `D`, `E`, `F`, `G`, `A` or `B`.  
* **Accidental** is one of the following strings:  
  * `#` or `sharp` for sharp;
  * `b` or `flat` for flat.
* **OctaveNumber** is a number of octave. A number must be between `-1` and `9`.

Examples of valid note strings:

`D3`  
`F##3`  
`Fb 1`  
`C#b4`