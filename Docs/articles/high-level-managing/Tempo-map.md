---
uid: a_hldm_tempomap
---

# Tempo map

Tempo map is a set of changes of time signature and tempo. You need to have a tempo map to convert [time and length](Time-and-length-overview.md) between different representations. Instead of messing with [Time Signature](xref:Melanchall.DryWetMidi.Core.TimeSignatureEvent) and [Set Tempo](xref:Melanchall.DryWetMidi.Core.SetTempoEvent) events DryWetMIDI provides [TempoMapManager](xref:Melanchall.DryWetMidi.Interaction.TempoMapManager) which helps to manage tempo map of a MIDI file.

```csharp
using (var tempoMapManager = new TempoMapManager(midiFile.TimeDivision,
                                                 midiFile.GetTrackChunks()
                                                         .Select(c => c.Events)))
{
    TempoMap tempoMap = tempoMapManager.TempoMap;

    Tempo tempoAt123 = tempoMap.TempoLine.AtTime(123);

    // Change tempo to 400000 microseconds per quarter note at 20 seconds from
    // MIDI file start
    tempoMapManager.SetTempo(new MetricTimeSpan(0, 0, 20), new Tempo(400000));

    tempoMapManager.ClearTimeSignature(456);
}
```

To get tempo map being managed by the current `TempoMapManager` you need to use [TempoMap](xref:Melanchall.DryWetMidi.Interaction.TempoMapManager.TempoMap) property which returns an instance of the [TempoMap](xref:Melanchall.DryWetMidi.Interaction.TempoMap).

Once you've got an instance of [TempoMap](xref:Melanchall.DryWetMidi.Interaction.TempoMap) you can use [GetTempoChanges](xref:Melanchall.DryWetMidi.Interaction.TempoMap.GetTempoChanges) method to get all tempo changes. Use [GetTimeSignatureChanges](xref:Melanchall.DryWetMidi.Interaction.TempoMap.GetTimeSignatureChanges) method to get time signature changes. [GetTempoAtTime](xref:Melanchall.DryWetMidi.Interaction.TempoMap.GetTempoAtTime(Melanchall.DryWetMidi.Interaction.ITimeSpan)) and [GetTimeSignatureAtTime](xref:Melanchall.DryWetMidi.Interaction.TempoMap.GetTimeSignatureAtTime(Melanchall.DryWetMidi.Interaction.ITimeSpan)) methods allow to get tempo and time signature at the specified time.

Please read [general information about events managers](Events-managers-overview.md) prior to use `TempoMapManager`.

You can also create new tempo map with `TempoMapManager`:

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

This method and another useful ones are placed in [TempoMapManagingUtilities](xref:Melanchall.DryWetMidi.Interaction.TempoMapManagingUtilities). For example, to get tempo map of a MIDI file you can write:

```csharp
TempoMap tempoMap = midiFile.GetTempoMap();
```

Also you can replace the entire tempo map of a MIDI file using `ReplaceTempoMap` method:

```csharp
midiFile.ReplaceTempoMap(TempoMap.Create(Tempo.FromBeatsPerMinute(140)));
```