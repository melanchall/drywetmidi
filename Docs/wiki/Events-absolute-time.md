`MidiEvent` has the `DeltaTime` property which provides offset from previous event in units defined by the time division of a MIDI file. But in practice it is more useful to operate by absolute times rather than relative ones. DryWetMIDI provides a way for this:

```csharp
using (var manager = new TimedEventsManager(trackChunk.Events))
{
    TimedEventsCollection timedEvents = manager.Events;

    TimedEvent firstEvent = timedEvents.First();
    firstEvent.Time = 123;

    timedEvents.Add(new TimedEvent(new InstrumentNameEvent("Guitar"), 456));
}
```

So to edit collection of events by absolute times you need to use `TimedEventsManager`. With this manager you operate by instances of the `TimedEvent` which is wrapper for `MidiEvent`. `TimedEvent` has the `Time` property holding an absolute time of the underlying event that can be accessed with the `Event` property.

```csharp
public sealed class TimedEvent : ITimedObject
{
    // ...
    public TimedEvent(MidiEvent midiEvent) { /*...*/}
    public TimedEvent(MidiEvent midiEvent, long time) { /*...*/}
    // ...
    public MidiEvent Event { get; }
    public long Time { get; set; }
    // ...
}
```

Please read [general information about events managers](Events-managers.md) prior to use `TimedEventsManager`.

To get collection of all timed events being managed by the current `TimedEventsManager` you need to use `Events` property which returns `TimedEventsCollection`. `TimedEventsCollection` has methods for adding and removing timed events and also can be enumerated since it implements `IEnumerable<TimedEvent>`. In addition to these methods there are extension ones contained in `TimedObjectUtilities`. For example, you can get all timed events in the first track chunk of a MIDI file at 20 seconds from the start of the file:

```csharp
TempoMap tempoMap = midiFile.GetTempoMap();

using (var timedEventsManager = midiFile.GetTrackChunks().First().ManageTimedEvents())
{
    var eventsAt20Seconds = timedEventsManager.Events
                                              .AtTime(new MetricTimeSpan(0, 0, 20),
                                                      tempoMap);
}
```

Read the [Time and length](Time-and-length.md) page to learn more about custom time.

As you can see there is another way to get an instance of the `TimedEventsManager` – through the `ManageTimedEvents` extension method. This method and another useful ones are placed in `TimedEventsManagingUtilities`.

Note that `TimedEventsManager` is the basic events manager provided by DryWetMIDI. There are several dedicated managers for managing special types of events. These managers built as wrappers for `TimedEventsManager`. For example, it is much more convenient to manage notes with the `NotesManager` rather than mess with instances of `NoteOnEvent` and `NoteOffEvent` through `TimedEventsManager`. These managers are:

* [NotesManager](Notes.md)
* [ChordsManager](Chords.md)
* [TempoMapManager](Tempo-map.md)