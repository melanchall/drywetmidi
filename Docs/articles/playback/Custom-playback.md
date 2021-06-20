---
uid: a_playback_custom
---

# Custom playback

You can subclass from [Playback](xref:Melanchall.DryWetMidi.Devices.Playback) to make your own playback by overriding some `protected virtual` methods:

* [bool TryPlayEvent(MidiEvent midiEvent, object metadata)](xref:Melanchall.DryWetMidi.Devices.Playback.TryPlayEvent(Melanchall.DryWetMidi.Core.MidiEvent,System.Object))
* [IEnumerable<TimedEvent> GetTimedEvents(ITimedObject timedObject)](xref:Melanchall.DryWetMidi.Devices.Playback.GetTimedEvents(Melanchall.DryWetMidi.Interaction.ITimedObject))

Let's see what each method needed for.

## TryPlayEvent

`TryPlayEvent` method called by playback each time an event should be played. Result value of the method tells playback whether the event was played or not. Default implementation of the method just sends a MIDI event to [output device](xref:Melanchall.DryWetMidi.Devices.Playback.OutputDevice) and returns `true`.

So you can implement your own logic of playing a MIDI event. Please pay attention to the second parameter of the method - `metadata`. If input objects of playback implement [IMetadata](xref:Melanchall.DryWetMidi.Common.IMetadata) interface, metadata will be passed via that parameter. For example, you can subclass from `TimedEvent` and implement `IMetadata` on a new class, and then create your own playback on instances of that class.

Sample code below shows how to play a MIDI file filtering out events within first track chunk:

```csharp
private sealed class TimedEventWithTrackChunkIndex : TimedEvent, IMetadata
{
    public TimedEventWithTrackChunkIndex(MidiEvent midiEvent, long time, int trackChunkIndex)
        : base(midiEvent, time)
    {
        Metadata = trackChunkIndex;
    }

    public object Metadata { get; set; }
}

private sealed class MyPlayback : Playback
{
    public MyPlayback(IEnumerable<ITimedObject> timedObjects, TempoMap tempoMap)
        : base(timedObjects, tempoMap)
    {
    }

    protected override bool TryPlayEvent(MidiEvent midiEvent, object metadata)
    {
        if (metadata == 0)
            return false;

        OutputDevice?.SendEvent(midiEvent);
        return true;
    }
}
```

Then create `MyPlayback`:

```csharp
var timedEvents = midiFile
    .GetTrackChunks()
    .SelectMany((c, i) => c.GetTimedEvents().Select(e => new TimedEventWithTrackChunkIndex(e.Event, e.Time, i)))
    .OrderBy(e => e.Time);

var tempoMap = midiFile.GetTempoMap();

var playback = new MyPlayback(timedEvents, tempoMap);
```

## GetTimedEvents

`Playback` internally transforms all input objects to instances of `TimedEvent`. So if some input objects implement [ITimedObject](xref:Melanchall.DryWetMidi.Interaction.ITimedObject) but their type is unknown for DryWetMIDI, we need to override `GetTimedEvents` method to provide transformation of our custom timed object to collection of timed events. Of course those timed events can be subclasses of `TimedEvent` and implement `IMetadata` (see previous section) so metadata will correctly go between a playback's internals. By default the method returns empty collection.