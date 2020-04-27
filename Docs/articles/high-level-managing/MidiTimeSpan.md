# MidiTimeSpan

[MidiTimeSpan](xref:Melanchall.DryWetMidi.Interaction.MidiTimeSpan) exists for unification purposes and simply holds long value in units defined by the time division of a MIDI file.

## Parsing

Following strings can be parsed to `MidiTimeSpan`:

`Value`

where

* **Value** is a number of MIDI ticks.

Examples:

`0` – zero time span  
`100` – 100 ticks  
`123456` – 123456 ticks