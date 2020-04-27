# BarBeatTicksTimeSpan

[BarBeatTicksTimeSpan](xref:Melanchall.DryWetMidi.Interaction.BarBeatTicksTimeSpan) represents a time span as a number of bars, beats and MIDI ticks.

## Parsing

Following strings can be parsed to `BarBeatTicksTimeSpan`:

`Bars.Beats.Ticks`

where

* **Bars** is a number of bars.
* **Beats** is a number of beats.
* **Ticks** is a number of MIDI ticks.

Examples:

`0.0.0` – zero time span  
`1.0.0` – 1 bar  
`0.10.5` – 10 beats and 5 ticks  
`100.20.0` – 100 bars and 20 ticks