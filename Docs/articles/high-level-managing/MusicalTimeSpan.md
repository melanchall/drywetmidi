# MusicalTimeSpan

[MusicalTimeSpan](xref:Melanchall.DryWetMidi.Interaction.MusicalTimeSpan) represents a time span as a fraction of the whole note, for example, 1/4 (quarter note length).

## Parsing

Following strings can be parsed to `MusicalTimeSpan`:

`Fraction Tuplet Dots`

where

* **Fraction** defines note length which is one of the following terms:  
  * `Numerator/Denominator` where **Numerator** and **Denominator** are nonnegative integer numbers; **Numerator** can be omitted assuming it's `1`;
  * `w`, `h`, `q`, `e` or `s` which mean whole, half, quarter, eighth or sixteenth note length respectively.  
* **Tuplet** represents tuplet definition which is one of the following terms:  
  * `[NotesCount : SpaceSize]` where **NotesCount** is positive integer count of notes with length defined by **Fraction** part; **SpaceSize** is the count of notes of normal length.
  * `t` or `d` which mean triplet and duplet respectively.
* **Dots** is any number of dots.

`Tuplet` and `Dots` parts can be omitted.

Examples:

`0/1` – zero time span  
`q` – quarter note length  
`1/4.` – dotted quarter note length  
`/8..` – double dotted eighth note length  
`wt.` – dotted whole triplet note length  
`w[3:10]` – length of 3 whole notes in space of 10 notes of normal length  
`s[3:10]...` – length of 3 sixteenth triple dotted notes in space of 10 notes of normal length