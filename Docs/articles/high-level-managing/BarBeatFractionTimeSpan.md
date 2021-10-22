# BarBeatFractionTimeSpan

[BarBeatFractionTimeSpan](xref:Melanchall.DryWetMidi.Interaction.BarBeatFractionTimeSpan) represents a time span as a number of bars and fractional beats (for example, `0.5` beats).

## Parsing

Following strings can be parsed to `BarBeatFractionTimeSpan`:

`Bars_BeatsIntegerPart.BeatsFractionalPart`

where

* **Bars** is a number of bars.
* **BeatsIntegerPart** is an integer part of fractional beats number.
* **BeatsFractionalPart** is a fractional part of fractional beats number.

Examples:

`0_0.0` – zero time span  
`1_0.0` – 1 bar  
`0_10.5` – 10.5 beats  
`100_20.2` – 100 bars and 20.2 beats