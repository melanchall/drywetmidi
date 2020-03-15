Tempo map is a set of changes of time signature and tempo. You need to have a tempo map to use custom time and length objects. Instead of messing with Time Signature and Set Tempo events DryWetMIDI provides `TempoMapManager` which helps to manage tempo map of a MIDI file.

```csharp
using (var tempoMapManager = new TempoMapManager(midiFile.TimeDivision,
                                                 midiFile.GetTrackChunks()
                                                         .Select(c => c.Events)))
{
    TempoMap tempoMap = tempoMapManager.TempoMap;

    Tempo tempoAt123 = tempoMap.TempoLine.AtTime(123);
    tempoMapManager.SetTempo(new MetricTimeSpan(0, 0, 20),
                             new Tempo(400000));

    tempoMapManager.ClearTimeSignature(456);
}
```

To get tempo map being managed by the current `TempoMapManager` you need to use `TempoMap` property which returns an instance of the `TempoMap`. `TempoLine` and `TimeSignatureLine` properties provide changes of tempo and time signature through the time.

```csharp
public sealed class TempoMap
{
    // ...
    public TempoMap(TimeDivision timeDivision) { /*...*/ }
    // ...
    public TimeDivision TimeDivision { get; }
    public ValueLine<TimeSignature> TimeSignatureLine { get; }
    public ValueLine<Tempo> TempoLine { get; }
    // ...
}
```

`TimeSignature` has following public members:

```csharp
public sealed class TimeSignature
{
    // ...
    public static readonly TimeSignature Default;
    // ...
    public TimeSignature(int numerator, int denominator) { /*...*/ }
    // ...
    public int Numerator { get; }
    public int Denominator { get; }
    // ...
}
```

`Tempo` has following public members:

```csharp
public sealed class Tempo
{
    // ...
    public static readonly Tempo Default;
    // ...
    public Tempo(long microsecondsPerQuarterNote) { /*...*/ }
    // ...
    public long MicrosecondsPerQuarterNote { get; }
    public long BeatsPerMinute { get; }
    // ...
    public static Tempo FromMillisecondsPerQuarterNote(long millisecondsPerQuarterNote) { /*...*/ }
    public static Tempo FromBeatsPerMinute(int beatsPerMinute) { /*...*/ }
    // ...
}
```

Please read [general information about events managers](Events-managers.md) prior to use `TempoMapManager`.

You can also manage new tempo map:

```csharp
using (var tempoMapManager = new TempoMapManager())
{
    // ...
}
```

There is another way to get an instance of the `TempoMapManager` – through the `ManageTempoMap` extension method:

```csharp
using (var tempoMapManager = midiFile.ManageTempoMap())
{
    // ...
}
```

This method and another useful ones are placed in `TempoMapManagingUtilities`. For example, to get tempo map of a MIDI file you can write:

```csharp
TempoMap tempoMap = midiFile.GetTempoMap();
```

Also you can replace the entire tempo map of a MIDI file using `ReplaceTempoMap` method:

```csharp
midiFile.ReplaceTempoMap(TempoMap.Create(Tempo.FromBeatsPerMinute(140)));
```

As you can see you can obtain a simple tempo map via static methods of the `TempoMap` class:

```csharp
public static TempoMap Create(Tempo tempo, TimeSignature timeSignature) { /*...*/ }
public static TempoMap Create(Tempo tempo) { /*...*/ }
```