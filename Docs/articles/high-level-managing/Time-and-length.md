---
uid: a_time_length
---

# Time and length

All times and lengths in a MIDI file are presented as some long values in units defined by the time division of a MIDI file. In practice it is much more convenient to operate by "human understandable" representations like seconds or bars/beats. In fact there is no difference between time and length since time within a MIDI file is just a length that always starts at zero, so the _time span_ term will be used to describe both time and length. DryWetMIDI provides the following classes to represent time span:

* [MetricTimeSpan](#metric) for time span in terms of microseconds;
* [BarBeatTicksTimeSpan](#bars-beats-and-ticks) for time span in terms of number of bars, beats and ticks;
* [BarBeatFractionTimeSpan](#bars-beats-and-fraction) for time span in terms of number of bars and fractional beats (for example, `0.5` beats);
* [MusicalTimeSpan](#musical) for time span in terms of a fraction of the whole note length;
* [MidiTimeSpan](#midi) exists for unification purposes and simply holds long value in units defined by the time division of a file.

All time span classes implement [ITimeSpan](xref:Melanchall.DryWetMidi.Interaction.ITimeSpan) interface.

To convert time span between different representations you should use [TimeConverter](xref:Melanchall.DryWetMidi.Interaction.TimeConverter) or [LengthConverter](xref:Melanchall.DryWetMidi.Interaction.LengthConverter) classes (these conversions require [tempo map](xref:Melanchall.DryWetMidi.Interaction.TempoMap) of a MIDI file). (You can use `LengthConverter` for time conversions too but with the `TimeConverter` you don't need to specify time where time span starts since it is always zero.)

Examples of time conversions:

```csharp
var tempoMap = midiFile.GetTempoMap();

// Some time in MIDI ticks (we assume time division of a MIDI file is "ticks per quarter note")

long ticks = 123;

// Convert ticks to metric time

MetricTimeSpan metricTime = TimeConverter.ConvertTo<MetricTimeSpan>(ticks, tempoMap);

// Convert ticks to musical time

MusicalTimeSpan musicalTimeFromTicks = TimeConverter.ConvertTo<MusicalTimeSpan>(ticks, tempoMap);

// Convert metric time to musical time

MusicalTimeSpan musicalTimeFromMetric = TimeConverter.ConvertTo<MusicalTimeSpan>(metricTime, tempoMap);

// Convert metric time to bar/beat time

BarBeatTicksTimeSpan barBeatTicksTimeFromMetric = TimeConverter.ConvertTo<BarBeatTicksTimeSpan>(metricTime, tempoMap);

// Convert musical time back to ticks

long ticksFromMusical = TimeConverter.ConvertFrom(musicalTimeFromTicks, tempoMap);
```

Examples of length conversions:

```csharp
var tempoMap = midiFile.GetTempoMap();

// Convert ticks to metric length

MetricTimeSpan metricLength = LengthConverter.ConvertTo<MetricTimeSpan>(ticks, time, tempoMap);

// Convert metric length to musical length using metric time

MusicalTimeSpan musicalLengthFromMetric = LengthConverter.ConvertTo<MusicalTimeSpan>(metricLength,
                                                                                     metricTime,
                                                                                     tempoMap);

// Convert musical length back to ticks

long ticksFromMetricLength = LengthConverter.ConvertFrom(metricLength, time, tempoMap);
```

You could notice that `LengthConverter`'s methods take a `time` as parameter. In general case MIDI file has changes of the tempo and time signature. Thus the same `long` (X) value can represent different amount of seconds, for example, depending on the time of an object with length of this X value. The methods above can take time either as `long` or as `ITimeSpan`.

There are some useful methods in the [TimedObjectUtilities](xref:Melanchall.DryWetMidi.Interaction.TimedObjectUtilities) class. This class contains extension methods for types that implement the [ITimedObject](xref:Melanchall.DryWetMidi.Interaction.ITimedObject) interface – [TimedEvent](xref:Melanchall.DryWetMidi.Interaction.TimedEvent), [Note](xref:Melanchall.DryWetMidi.Interaction.Note) and [Chord](xref:Melanchall.DryWetMidi.Interaction.Chord). For example, you can get time of a timed event in hours, minutes, seconds with [TimeAs](xref:Melanchall.DryWetMidi.Interaction.TimedObjectUtilities.TimeAs``1(Melanchall.DryWetMidi.Interaction.ITimedObject,Melanchall.DryWetMidi.Interaction.TempoMap)) method:

```csharp
var metricTime = timedEvent.TimeAs<MetricTimeSpan>(tempoMap);
```

Or you can find all notes of a MIDI file that start at time of 10 bars and 4 beats:

```csharp
TempoMap tempoMap = midiFile.GetTempoMap();
IEnumerable<Note> notes = midiFile
    .GetNotes().AtTime(new BarBeatTicksTimeSpan(10, 4), tempoMap);
```

Also there is the [LengthedObjectUtilities](xref:Melanchall.DryWetMidi.Interaction.LengthedObjectUtilities) class. This class contains extension methods for types that implement the [ILengthedObject](xref:Melanchall.DryWetMidi.Interaction.ILengthedObject) interface – [Note](xref:Melanchall.DryWetMidi.Interaction.Note) and [Chord](xref:Melanchall.DryWetMidi.Interaction.Chord). For example, you can get length of a note as a fraction of the whole note with [LengthAs](xref:Melanchall.DryWetMidi.Interaction.LengthedObjectUtilities.LengthAs``1(Melanchall.DryWetMidi.Interaction.ILengthedObject,Melanchall.DryWetMidi.Interaction.TempoMap)) method:

```csharp
var musicalLength = note.LengthAs<MusicalTimeSpan>(tempoMap);
```

Or you can get all notes of a MIDI file that end exactly at 30 seconds from the start of the file:

```csharp
var tempoMap = midiFile.GetTempoMap();
var notesAt30sec = midiFile
    .GetNotes().EndAtTime(new MetricTimeSpan(0, 0, 30), tempoMap);
```

`TimeAs` (end `EndTimeAs`) and `LengthAs` methods have non-generic versions where the desired type of result should be passed as an argument of the [TimeSpanType](xref:Melanchall.DryWetMidi.Interaction.TimeSpanType) type.

`ITimeSpan` interface has several methods to perform arithmetic operations on time spans. For example, to add metric length to metric time you can write:

```csharp
var timeSpan1 = new MetricTimeSpan(0, 2, 20);
var timeSpan2 = new MetricTimeSpan(0, 0, 10);
ITimeSpan result = timeSpan1.Add(timeSpan2, TimeSpanMode.TimeLength);
```

You need to specify mode of the operation. In the example above `TimeLength` is used which means that first time span represents a time and the second one represents a length. This information is needed for conversion engine when operands are of different types. There are also `TimeTime` and `LengthLength` modes.

You can also subtract one time span from another one:

```csharp
var timeSpan1 = new MetricTimeSpan(0, 10, 0);
var timeSpan2 = new MusicalTimeSpan(3, 8);
ITimeSpan result = timeSpan1.Subtract(timeSpan2, TimeSpanMode.TimeTime);
```

If operands of the same type, result time span will be of this type too. But if you sum or subtract time spans of different types, the type of a result time span will be [MathTimeSpan](xref:Melanchall.DryWetMidi.Interaction.MathTimeSpan) which holds operands along with operation (addition or subtraction) and mode.

To stretch or shrink a time span use `Multiply` or `Divide` methods:

```csharp
ITimeSpan stretchedTimeSpan = new MetricTimeSpan(0, 0, 10).Multiply(2.5);
ITimeSpan shrinkedTimeSpan = new BarBeatTicksTimeSpan(0, 2).Divide(2);
```

There are some useful methods in the [TimeSpanUtilities](xref:Melanchall.DryWetMidi.Interaction.TimeSpanUtilities) class. These methods include `Parse` and `TryParse` ones that allows to parse a string to appropriate time span. Please read article corresponding to desired time span type to learn formats of strings that can be parsed to this type (use links at the start of this article).

## Representations

DryWetMIDI provides several representations of time spans.

### Metric

[MetricTimeSpan](xref:Melanchall.DryWetMidi.Interaction.MetricTimeSpan) represents time span as a number of microseconds.

Following strings can be parsed to `MetricTimeSpan`:

`Hours : Minutes : Seconds : Milliseconds`  
`HoursGroup : Minutes : Seconds`  
`Minutes : Seconds`  
`Hours h Minutes m Seconds s Milliseconds ms`  
`Hours h Minutes m Seconds s`  
`Hours h Minutes m Milliseconds ms`  
`Hours h Seconds s Milliseconds ms`  
`Minutes m Seconds s Milliseconds ms`  
`Hours h Minutes m`  
`Hours h Seconds s`  
`Hours h Milliseconds ms`  
`Minutes m Seconds s`  
`Hours h Milliseconds ms`  
`Seconds s Milliseconds ms`  
`Hours h`  
`Minutes m`  
`Seconds s`  
`Milliseconds ms`

where

* **Hours** is a number of hours.
* **Minutes** is a number of minutes.
* **Seconds** is a number of seconds.
* **Milliseconds** is a number of milliseconds.

Examples:

`0:0:0:0` – zero time span  
`0:0:0:156` – 156 milliseconds  
`2:0:156` – 2 hours and 156 seconds  
`1:156` – 1 minute and 156 seconds  
`1h2m3s4ms` – 1 hour 2 minutes 3 seconds 4 milliseconds  
`1h 2m3s` – 1 hour 2 minutes 3 seconds  
`1h2M 4ms` – 1 hour 2 minutes 4 milliseconds  
`1 h3s4ms` – 1 hour 3 seconds 4 milliseconds  
`2M3 S 4 MS` – 2 minutes 3 seconds 4 milliseconds  
`1h2m` – 1 hour 2 minutes  
`1h 3s` – 1 hour 3 seconds  
`1h4MS` – 1 hour 4 milliseconds  
`2M3s` – 2 minutes 3 seconds  
`2 m 4 Ms` – 2 minutes 4 milliseconds  
`3 s 4 mS` – 2 seconds 4 milliseconds

### Bars, beats and ticks

[BarBeatTicksTimeSpan](xref:Melanchall.DryWetMidi.Interaction.BarBeatTicksTimeSpan) represents a time span as a number of bars, beats and MIDI ticks.

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

### Bars, beats and fraction

[BarBeatFractionTimeSpan](xref:Melanchall.DryWetMidi.Interaction.BarBeatFractionTimeSpan) represents a time span as a number of bars and fractional beats (for example, `0.5` beats).

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

### Musical

[MusicalTimeSpan](xref:Melanchall.DryWetMidi.Interaction.MusicalTimeSpan) represents a time span as a fraction of the whole note, for example, 1/4 (quarter note length).

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

### MIDI

[MidiTimeSpan](xref:Melanchall.DryWetMidi.Interaction.MidiTimeSpan) exists for unification purposes and simply holds long value in units defined by the time division of a MIDI file.

Following strings can be parsed to `MidiTimeSpan`:

`Value`

where

* **Value** is a number of MIDI ticks.

Examples:

`0` – zero time span  
`100` – 100 ticks  
`123456` – 123456 ticks