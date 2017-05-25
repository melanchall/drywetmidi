DryWetMIDI is the .NET library to work with MIDI files. It allows:

* Read, write and create Standard MIDI Files (SMF). It is also possible to read RMID files where SMF wrapped to RIFF chunk.
* Finely adjust process of reading and writing. It allows, for example, to read corrupted files and repair them, or build MIDI file validators.
* Implement custom meta events and custom chunks that can be write to and read from MIDI files.
* Easily catch specific error when reading or writing MIDI file since all possible errors in a MIDI file are presented as separate exception classes.