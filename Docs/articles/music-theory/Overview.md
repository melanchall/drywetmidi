---
uid: a_mt_overview
---

# Overview

DryWetMIDI provides types and methods to work with music theory objects like scale or chord progression, and provides ways to use them within MIDI. Following list shows what objects you can work with:

* [Octave](Octave.md);
* [Interval](Interval.md);
* [Note](Note.md);
* [Chord](Chord.md);
* [Chord progression](Chord-progression.md);
* [Scale](Scale.md).

Note that DryWetMIDI uses [Scientific Pitch Notation](https://en.wikipedia.org/wiki/Scientific_pitch_notation) so **middle C** note is C4 one. [Octave.Middle](xref:Melanchall.DryWetMidi.MusicTheory.Octave.Middle) returns that 4th octave. You can read interesting discussion about different notations here: [MIDI Octave and Note Numbering Standard](https://www.midi.org/forum/830-midi-octave-and-note-numbering-standard).

All corresponding classes have `Parse` and `TryParse` method so you can get an instance of a class from string. For example, you can parse `"C#"` string to C# note.