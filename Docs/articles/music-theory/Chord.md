---
uid: a_mt_chord
---

# Chord

[Chord](xref:Melanchall.DryWetMidi.MusicTheory.Chord) represents a musical chord as a collection of notes names. Some examples of usage:

```csharp
// Create chord by root note name and intervals from root
var chordByIntervals = new Chord(NoteName.A, Interval.FromHalfSteps(2), Interval.FromHalfSteps(5));

// Create A B chord
var chordByNotesNames = new Chord(NoteName.A, NoteName.B);

// Create C Major chord
var cMajorTriad = Chord.GetByTriad(NoteName.C, ChordQuality.Major);

// ... or parse it from string
var cMajorTriadFromString = Chord.Parse("C");

// Parse more complex chord from string
var cAug7Chord = Chord.Parse("Caug7");
```

Last chord will contain C, E, G# and A# notes as expected.

Chords parsing uses [Chord Names Table (CNT)](#chords-names-table) so not all possible chords can be parsed since CNT contains limited set of chords naming rules. If you notice that some known chord name is not parsed by DryWetMIDI, please [create an issue on GitHub](https://github.com/melanchall/drywetmidi/issues/new) and CNT will be extended. See following section for details.

## Parsing

Following strings can be parsed to `Chord`:

`RootNote ChordCharacteristic`
`RootNote ChordCharacteristic BassNote`

where

* **RootNote** is the root note of a chord, for example, `A` or `C#`.
* **ChordCharacteristic** is combination of chord quality, altered and added tones and so on. See [Chords names table](#chords-names-table) section below to learn what characteristics are supported by the library.
* **BassNote** is an added bass note the chord will be placed over.

Examples of valid chord strings:

`C`  
`Caug7`  
`Cm`  
`Am`  
`Cm7`  
`CmM7`  
`Csus2`  
`C9sus4`  
`F/G`  
`C7b5`

## Chords names table

Following table shows what chords currently supported for parsing from string and for retrieving names by chords notes. First column lists names and second one shows intervals from root. For example, `0 1 5` for `C` give us following chord: `C C# E`.

Please note that some interval sets start from number greater than zero. Currently the library doesn't support chords with omitted root, but the feature will be implemented in the future.

|Names|Intervals|
|-----|---------|
|`maj`<br/>`M`<br/>` `|`0 4 7`|
|`min`<br/>`m`|`0 3 7`|
|`sus4`|`0 5 7`|
|`sus2`|`0 2 7`|
|`b5`|`0 4 6`|
|`dim`|`0 3 6`|
|`aug`|`0 4 8`|
|`min6`<br/>`m6`|`0 3 7 9`|
|`maj6`<br/>`M6`<br/>`6`|`0 4 7 9`|
|`7`|`0 4 7 10`|
|`7sus4`|`0 5 7 10`|
|`7sus2`|`0 2 7 10`|
|`min7`<br/>`m7`|`0 3 7 10`|
|`min9`<br/>`min7(9)`<br/>`m9`<br/>`m7(9)`|`0 3 7 10 14`<br/>`0 3 10 14`<br/>`3 10 14` (not supported)<br/>`3 7 10 14` (not supported)|
|`min11`<br/>`min7(9,11)`<br/>`m11`<br/>`m7(9,11)`|`0 3 7 10 14 17`<br/>`0 3 10 14 17`<br/>`3 10 14 17` (not supported)<br/>`3 7 10 14 17` (not supported)|
|`maj7`|`0 4 7 11`|
|`maj7(9)`<br/>`M7(9)`|`0 4 7 11 14`<br/>`0 4 11 14`<br/>`4 11 14` (not supported)<br/>`4 7 11 14` (not supported)|
|`maj7(#11)`<br/>`M7(#11)`|`0 4 7 11 14 18`<br/>`0 4 11 14 18`<br/>`4 11 14 18` (not supported)<br/>`4 7 11 14 18` (not supported)|
|`maj7(13)`<br/>`M7(13)`|`0 4 7 11 21`<br/>`0 4 11 21`<br/>`4 11 21` (not supported)<br/>`4 7 11 21` (not supported)|
|`maj7(9,13)`<br/>`M7(9,13)`|`0 4 7 11 14 21`<br/>`0 4 11 14 21`<br/>`4 11 14 21` (not supported)<br/>`4 7 11 14 21` (not supported)|
|`maj7#5`<br/>`M7#5`|`0 4 8 11`|
|`maj7#5(9)`<br/>`M7#5(9)`|`0 4 8 11 14`<br/>`4 8 11 14` (not supported)|
|`minMaj7`<br/>`mM7`|`0 3 7 11`|
|`minMaj7(9)`<br/>`mM7(9)`|`0 3 7 11 14`<br/>`0 3 11 14`<br/>`3 11 14` (not supported)<br/>`3 7 11 14` (not supported)|
|`5`|`0 7`|
|`7b5`<br/>`dom7dim5`<br/>`7dim5`|`0 4 6 10`|
|`ø`<br/>`ø7`<br/>`m7b5`<br/>`min7dim5`<br/>`m7dim5`<br/>`min7b5`<br/>`m7b5`|`0 3 6 10`|
|`aug7`|`0 4 8 10`|
|`dim7`|`0 3 6 9`|
|`add9`|`0 4 7 14`|
|`minAdd9`<br/>`mAdd9`|`0 3 7 14`|
|`maj6(9)`<br/>`6(9)`<br/>`6/9`<br/>`M6/9`<br/>`M6(9)`|`0 4 7 9 14`<br/>`4 7 9 14` (not supported)<br/>`0 4 9 14`<br/>`4 9 14` (not supported)|
|`min6(9)`<br/>`m6(9)`<br/>`m6/9`<br/>`min6/9`|`0 3 7 9 14`<br/>`3 7 9 14` (not supported)<br/>`0 3 9 14`<br/>`3 9 14` (not supported)|
|`9`|`0 4 7 10 14`|
|`9sus2`|`0 2 7 10 14`|
|`9sus4`|`0 5 7 10 14`|
|`11`|`0 4 7 10 14 17`|