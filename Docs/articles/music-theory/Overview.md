﻿---
uid: a_mt_overview
---

# Music Theory – Overview

DryWetMIDI provides types and methods to work with music theory objects like scale or chord progression, and provides ways to use them within MIDI. Following list shows what objects you can work with:

* [Octave](Octave.md);
* [Interval](Interval.md);
* [Note](Note.md);
* [Chord](Chord.md);
* [Chord progression](Chord-progression.md);
* [Scale](Scale.md).

Note that DryWetMIDI uses [Scientific Pitch Notation](https://en.wikipedia.org/wiki/Scientific_pitch_notation) so the **middle C** note is the _C4_ one. [Octave.Middle](xref:Melanchall.DryWetMidi.MusicTheory.Octave.Middle) returns that 4th octave. You can read an interesting discussion about different notations here: [MIDI Octave and Note Numbering Standard](https://midi.org/community/midi-specifications/midi-octave-and-note-numbering-standard).

All corresponding classes have `Parse` and `TryParse` methods so you can get an instance of a class from string. For example, you can parse `"C#"` string to C# note.