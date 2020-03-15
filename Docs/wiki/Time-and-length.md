All times and lengths in a MIDI file are presented as some long values in units defined by the time division of a file. In practice it is much more convenient to operate by "human understandable" representations like seconds or bars/beats. In fact there is no difference between time and length since time within a MIDI file is just a length that always starts at zero. The _time span_ term will be used to describe both time and length. DryWetMIDI provides the following classes to represent time span:

* **MetricTimeSpan** for time span in terms of microseconds;
* **BarBeatTicksTimeSpan** for time span in terms of number of bars, beats and ticks;
* **BarBeatFractionTimeSpan** for time span in terms of number of bars and fractional beats (for example, 0.5 beats);
* **MusicalTimeSpan** for time span in terms of a fraction of the whole note length;
* **MidiTimeSpan** exists for unification purposes and simply holds long value in units defined by the time division of a file.

All time span classes implement `ITimeSpan` interface that has following public members:

```csharp
public interface ITimeSpan
{
    ITimeSpan Add(ITimeSpan timeSpan, TimeSpanMode mode);
    ITimeSpan Subtract(ITimeSpan timeSpan, TimeSpanMode mode);
    ITimeSpan Multiply(double multiplier);
    ITimeSpan Divide(double divisor);
    ITimeSpan Clone();
}
```

To convert time span between different representations you should use `TimeConverter` or `LengthConverter` classes (these conversions require [tempo map](Tempo-map.md) of a MIDI file). (You can use `LengthConverter` for time conversions but with the `TimeConverter` you don't need to specify time where time span starts since it is always zero.)

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

You could notice that `LengthConverter`'s methods take a time. In general case MIDI file has changes of the tempo and time signature. Thus the same long value can represent different amount of seconds, for example, depending on the time of an object with length of this value. The methods above can take time either as `long` or as `ITimeSpan`.

There are some useful methods in the `TimedObjectUtilities` class. This class contains extension methods for types that implement the `ITimedObject` interface – `TimedEvent`, `Note` and `Chord`. For example, you can get time of a timed event in hours, minutes, seconds with `TimeAs` method:

```csharp
var metricTime = timedEvent.TimeAs<MetricTimeSpan>(tempoMap);
```

Or you can find all notes of a MIDI file that start at time of 10 bars and 4 beats:

```csharp
TempoMap tempoMap = midiFile.GetTempoMap();
IEnumerable<Note> notes = midiFile.GetNotes()
                                  .AtTime(new BarBeatTicksTimeSpan(10, 4), tempoMap);
```

Also there is the `LengthedObjectUtilities` class. This class contains extension methods for types that implement the `ILengthedObject` interface – `Note` and `Chord`. For example, you can get length of a note as a fraction of the whole note with `LengthAs` method:

```csharp
var musicalLength = note.LengthAs<MusicalTimeSpan>(tempoMap);
```

Or you can get all notes of a MIDI file that end exactly at 30 seconds from the start of the file:

```csharp
var tempoMap = midiFile.GetTempoMap();
var notesAt30sec = midiFile.GetNotes()
                           .EndAtTime(new MetricTimeSpan(0, 0, 30), tempoMap);
```

If you want, for example, to know length of a MIDI file in minutes and seconds, you can use this code:

```csharp
var tempoMap = midiFile.GetTempoMap();
var midiFileDuration = midiFile.GetTimedEvents()
                               .LastOrDefault(e => e.Event is NoteOffEvent)
                              ?.TimeAs<MetricTimeSpan>(tempoMap)
                              ?? new MetricTimeSpan();
```

Both `TimeAs` and `LengthAs` methods have non-generic versions where the desired type of result should be passed as an argument of the `TimeSpanType` type. `TimeSpanType` enum has following values:

Value | Description
----- | -----------
`Metric` | Result will be of the `MetricTimeSpan` type.
`Musical` | Result will be of the `MusicalTimeSpan` type.
`BarBeatTicks` | Result will be of the `BarBeatTicksTimeSpan` type.
`BarBeatFraction` | Result will be of the `BarBeatFractionTimeSpan` type.
`Midi` | Result will be of the `MidiTimeSpan` type.

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

If operands of the same type, result time span will be of this type too. But if you sum or subtract time spans of different types, the type of a result time span will be `MathTimeSpan` which holds operands along with operation (addition or subtraction) and mode. `MathTimeSpan` has following public members:

```csharp
public sealed class MathTimeSpan : ITimeSpan
{
    // ...
    public ITimeSpan TimeSpan1 { get; }
    public ITimeSpan TimeSpan2 { get; }
    public MathOperation Operation { get; }
    public TimeSpanMode Mode { get; }
    // ...
}
```

To stretch or shrink a time span use `Multiply` or `Divide` methods:

```csharp
ITimeSpan stretchedTimeSpan = new MetricTimeSpan(0, 0, 10).Multiply(2.5);
ITimeSpan shrinkedTimeSpan = new BarBeatTicksTimeSpan(0, 2).Divide(2);
```

## Utilities

There are some useful methods in the `TimeSpanUtilities` class.

To get a time span representing a maximum value for the specified time span type use `GetMaxTimeSpan` method:

```csharp
ITimeSpan GetMaxTimeSpan(TimeSpanType timeSpanType)
```

To get zero time span use `GetZeroTimeSpan` method:

```csharp
ITimeSpan GetZeroTimeSpan(TimeSpanType timeSpanType)

TTimeSpan GetZeroTimeSpan<TTimeSpan>()
```

There are `Parse` and `TryParse` methods that allows to parse a string to appropriate time span:

```csharp
bool TryParse(string input, out ITimeSpan timeSpan)

bool TryParse(string input, TimeSpanType timeSpanType, out ITimeSpan timeSpan)

ITimeSpan Parse(string input)
```

Formats of time span strings are described below.

#### `MidiTimeSpan`

```
Value
```

Examples:
* `0` – zero time span
* `100` – 100 ticks
* `123456` – 123456 ticks

where _Value_ is a nonnegative integer number.

#### `MetricTimeSpan`

```
Hours : Minutes : Seconds : Milliseconds
HoursGroup : Minutes : Seconds
Minutes : Seconds

Hours h Minutes m Seconds s Milliseconds ms
Hours h Minutes m Seconds s
Hours h Minutes m Milliseconds ms
Hours h Seconds s Milliseconds ms
Minutes m Seconds s Milliseconds ms
Hours h Minutes m
Hours h Seconds s
Hours h Milliseconds ms
Minutes m Seconds s
Hours h Milliseconds ms
Seconds s Milliseconds ms
Hours h
Minutes m
Seconds s
Milliseconds ms
```

where _Hours_, _Minutes_, _Seconds_ and _Milliseconds_ are nonnegative integer numbers.

Examples:
* `0:0:0:0` – zero time span
* `0:0:0:156` – 156 milliseconds
* `2:0:156` – 2 hours and 156 seconds
* `1:156` – 1 minute and 156 seconds
* `1h2m3s4ms` – 1 hour 2 minutes 3 seconds 4 milliseconds
* `1h 2m3s` – 1 hour 2 minutes 3 seconds
* `1h2M 4ms` – 1 hour 2 minutes 4 milliseconds
* `1 h3s4ms` – 1 hour 3 seconds 4 milliseconds
* `2M3 S 4 MS` – 2 minutes 3 seconds 4 milliseconds
* `1h2m` – 1 hour 2 minutes
* `1h 3s` – 1 hour 3 seconds
* `1h4MS` – 1 hour 4 milliseconds
* `2M3s` – 2 minutes 3 seconds
* `2 m 4 Ms` – 2 minutes 4 milliseconds
* `3 s 4 mS` – 2 seconds 4 milliseconds

#### `BarBeatTicksTimeSpan`

```
Bars.Beats.Ticks
```

where _Bars_, _Beats_ and _Ticks_ are nonnegative integer numbers.

Examples:
* `0.0.0` – zero time span
* `1.0.0` – 1 bar
* `0.10.5` – 10 beats and 5 ticks
* `100.20.0` – 100 bars and 20 ticks

#### `BarBeatFractionTimeSpan`

```
Bars.BeatsIntegerPart.BeatsFractionalPart
```

where _Bars_, _BeatsIntegerPart_, _BeatsFractionalPart_ are nonnegative integer numbers.

Examples:
* `0.0.0` – zero time span
* `1.0.0` – 1 bar
* `0.10.5` – 10.5 beats
* `100.20.2` – 100 bars and 20.2 beats

#### `MusicalTimeSpan`

```
Fraction Tuplet Dots
```

where _Fraction_ defines note length which is one of the following terms:
* `Numerator/Denominator` where _Numerator_ and _Denominator_ are nonnegative integer numbers; `Numerator` can be omitted assuming it's `1`;
* `w`, `h`, `q`, `e` or `s` which mean whole, half, quarter, eighth or sixteenth note length respectively;  
where `Tuplet` represents tuplet definition which is one of the following terms:
* `[NotesCount : SpaceSize]` where _NotesCount_ is positive integer count of notes with length defined by _Fraction_ part; _SpaceSize_ is count of notes of normal length;
* `t` or `d` which mean triplet and duplet respectively;  
where `Dots` is any number of dots.

`Tuplet` and `Dots` can be omitted.

Examples:
* `0/1` – zero time span
* `q` – quarter note length
* `1/4.` – dotted quarter note length
* `/8..` – double dotted eighth note length
* `wt.` – dotted whole triplet note length
* `w[3:10]` – length of 3 whole notes in space of 10 notes of normal length
* `s[3:10]...` – length of 3 sixteenth triple dotted notes in space of 10 notes of normal length