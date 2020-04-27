# Events absolute time

[MidiEvent](xref:Melanchall.DryWetMidi.Core.MidiEvent) has the [DeltaTime](xref:Melanchall.DryWetMidi.Core.MidiEvent.DeltaTime) property which provides offset from previous event in units defined by the time division of a MIDI file. But in practice it is more useful to operate by **absolute times** rather than relative ones. Absolute time of a MIDI event is a sum of all delta-times before this event including delta-time of the current one. DryWetMIDI provides a way to manage MIDI events by absolute times:

```csharp
using (var manager = new TimedEventsManager(trackChunk.Events))
{
    TimedEventsCollection timedEvents = manager.Events;

    TimedEvent firstEvent = timedEvents.First();
    firstEvent.Time = 123; // It's absolute time

    timedEvents.Add(new TimedEvent(new InstrumentNameEvent("Guitar"), 456 /* Absolute time too */));
}
```

So to edit collection of events by absolute times you need to use [TimedEventsManager](xref:Melanchall.DryWetMidi.Interaction.TimedEventsManager). With this manager you operate by instances of the [TimedEvent](xref:Melanchall.DryWetMidi.Interaction.TimedEvent) which is wrapper for [MidiEvent](xref:Melanchall.DryWetMidi.Core.MidiEvent). `TimedEvent` has the [Time](xref:Melanchall.DryWetMidi.Interaction.TimedEvent.Time) property holding an absolute time of the underlying event that can be accessed with the [Event](xref:Melanchall.DryWetMidi.Interaction.TimedEvent.Event) property.

Please read [general information about events managers](Events-managers-overview.md) prior to use `TimedEventsManager`.

To get collection of all timed events being managed by the current `TimedEventsManager` you need to use [Events](xref:Melanchall.DryWetMidi.Interaction.TimedEventsManager.Events) property which returns [TimedEventsCollection](xref:Melanchall.DryWetMidi.Interaction.TimedEventsCollection). `TimedEventsCollection` has methods for adding and removing timed events and also can be enumerated since it implements `IEnumerable<TimedEvent>`. In addition to these methods there are extension ones contained in [TimedObjectUtilities](xref:Melanchall.DryWetMidi.Interaction.TimedObjectUtilities). For example, you can get all timed events in the first track chunk of a MIDI file at 20 seconds from the start of the file:

```csharp
TempoMap tempoMap = midiFile.GetTempoMap();

using (var timedEventsManager = midiFile.GetTrackChunks().First().ManageTimedEvents())
{
    var eventsAt20Seconds = timedEventsManager
        .Events.AtTime(new MetricTimeSpan(0, 0, 20), tempoMap);
}
```

Read the [time and length overview](Time-and-length-overview.md) to learn more about different time representations.

As you can see there is another way to get an instance of the `TimedEventsManager` – through the `ManageTimedEvents` extension method. This method and another useful ones are placed in [TimedEventsManagingUtilities](xref:Melanchall.DryWetMidi.Interaction.TimedEventsManagingUtilities).

Note that `TimedEventsManager` is the basic events manager provided by DryWetMIDI. There are several dedicated managers for managing special types of events. These managers built as wrappers for `TimedEventsManager`. For example, it is much more convenient to manage notes with the [NotesManager](Notes.md) rather than mess with instances of [NoteOnEvent](xref:Melanchall.DryWetMidi.Core.NoteOnEvent) and [NoteOffEvent](xref:Melanchall.DryWetMidi.Core.NoteOffEvent) through `TimedEventsManager`. These managers are:

* [NotesManager](Notes.md)
* [ChordsManager](Chords.md)
* [TempoMapManager](Tempo-map.md)