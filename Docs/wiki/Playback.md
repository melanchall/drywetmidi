**Please note that Devices API is Windows only at now.**

`Playback` class allows to play MIDI events via an `OutputDevice` (see [Output device](Output-device.md) article). In other words it sends MIDI data to output MIDI device taking events delta-times into account. To get an instance of the `Playback` you must use its constructor passing collection of MIDI events, tempo map and output device:

```csharp
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

var eventsToPlay = new MidiEvent[]
{
    new NoteOnEvent(),
    new NoteOffEvent
    {
        DeltaTime = 100
    }
};

using (var outputDevice = OutputDevice.GetByName("Output MIDI device"))
using (var playback = new Playback(eventsToPlay, TempoMap.Default, outputDevice))
{
    // ...
}
```

There are also extension methods `GetPlayback` for `TrackChunk`, `IEnumerable<TrackChunk>`, `MidiFile` and `Pattern` classes which simplify obtaining a playback object for MIDI file entities and musical composition created with [patterns](Pattern.md):

```csharp
using (var outputDevice = OutputDevice.GetByName("Output MIDI device"))
using (var playback = MidiFile.Read("Some MIDI file.mid").GetPlayback(outputDevice))
{
    // ...
}
```

`GetDuration` method returns the total duration of a playback in the specified format.

There are two approaches of playing MIDI data: blocking and non-blocking.

### Blocking playback

If you call `Play` method of the `Playback`, the calling thread will be blocked until entire collection of MIDI events will be sent to MIDI device. Note that execution of this method will be infinite if the `Loop` property set to `true`. See [playback properties](#playback-properties) below to learn more.

There are also extension methods `Play` for `TrackChunk`, `IEnumerable<TrackChunk>`, `MidiFile` and `Pattern` classes which simplify playing MIDI file entities and musical compositions created with [patterns](Pattern.md):

```csharp
using (var outputDevice = OutputDevice.GetByName("Output MIDI device"))
{
    MidiFile.Read("Some MIDI file.mid").Play(outputDevice);

    // ...
}
```

### Non-blocking playback

Is you call `Start` method of the `Playback`, execution of the calling thread will continue immediately after the method is called. To stop playback use `Stop` method. Note that there is no `Pause` method since it useless. `Stop` leaves playback at the point where the method was called. To move to the start of the playback use `MoveToStart` method described in the [Time management](#time-management) section below.

You should be very careful with this approach and `using` block. Example below shows the case where part of MIDI data will not be played because of playback is disposed before the last MIDI event will be sent to output device:

```csharp
using (var outputDevice = OutputDevice.GetByName("Output MIDI device"))
using (var playback = MidiFile.Read("Some MIDI file.mid").GetPlayback(outputDevice))
{
    playback.Start();

    // ...
}
```

With non-blocking approach it is recommended to call `Dispose` manually after you've finished work with playback object.

After playback finished the `Finished` event will be fired. `Started` and `Stopped` events will be fired on `Start` (`Play`) and `Stop` calls respectively.

### Playback properties

Let's see public properties of the `Playback` class.

#### `Loop`

Gets or sets a value indicating whether playing should automatically start from the first event after the last one played. If you set it to `true` and call `Play` method, calling thread will be blocked forever. The default value is `false`.

#### `Speed`

Gets or sets the speed of events playing. `1.0` means normal speed which is the default. For example, to play MIDI data twice slower this property should be set to `0.5`. Pass `10.0` to play MIDI events ten times faster.

#### `InterruptNotesOnStop`

Gets or sets a value indicating whether currently playing notes must be stopped when `Stop` method called on playback.

#### `TrackNotes`

Gets or sets a value indicating whether notes must be tracked or not. If `false`, notes will be treated as just Note On/Note Off events. If `true`, notes will be treated as lengthed objects.

If playback stopped in middle of a note, then _Note Off_ event will be sent on stop, and _Note On_ – on playback start. If one of [time management](#time-management) methods is called:

* and old playback's position was on a note, then _Note Off_ will be send for that note;
* and new playback's position is on a note, then _Note On_ will be send for that note.

#### `IsRunning`

Gets a value indicating whether playing is currently running or not.

#### `Snapping`

Provides a way to manage snap points for playback. See [Snapping](#snapping) section below to learn more.

### Playback events

#### `Started`

Occurs when playback started via `Start` or `Play` methods.

#### `Stopped`

Occurs when playback stopped via `Stop` method.

#### `Finished`

Occurs when playback finished, i.e. last event has been played and no need to restart playback due to value of the `Loop` property.

#### `NotesPlaybackStarted`

Occurs when notes started to play. It will raised if playback's cursor gets in to notes (due to normal playing or [time management](#time-management) operations).

Related notes can be obtained via instance of the `NotesEventArgs` passed to event handler.

#### `NotesPlaybackFinished`

Occurs when notes finished to play. It will raised if playback's cursor gets out from notes (due to normal playing or [time management](#time-management) operations).

Related notes can be obtained via instance of the `NotesEventArgs` passed to event handler.

### Snapping

`Snapping` property of the `Playback` gives an instance of the `PlaybackSnapping` class which manages snap points for playback. Snap points are markers at speicified times which can be used to move to (see [Time management](#time-management) section below).

`PlaybackSnapping` provides following members:

* `IEnumerable<SnapPoint> SnapPoints`  
  Returns all snap points currently set for playback (including disabled ones).
* `SnapPoint<TData> AddSnapPoint<TData>(ITimeSpan time, TData data)`  
  Add a snap point with the specified data at given time. The data will be available through `Data` property of the snap point returned by the method.
* `SnapPoint<Guid> AddSnapPoint(ITimeSpan time)`  
  Add a snap point at the specified time without user data. `Data` will hold unique `Guid` value.
* `RemoveSnapPoint<TData>(SnapPoint<TData> snapPoint)`  
  Remove a snap point.
* `RemoveSnapPointsByData<TData>(Predicate<TData> predicate)`  
  Remove all snap points that match the conditions defined by the specified predicate.
* `SnapPointsGroup SnapToGrid(IGrid grid)`  
  Add snap points at times defined by the specified grid. Implementations of `IGrid` are `SteppedGrid` (grid where times are defined by collection of repeating steps) and `ArbitraryGrid` (grid where times are specified explicitly). An instance of `SnapPointsGroup` will be returned which can be used to manage all the snap points added by the method.
* `SnapPointsGroup SnapToNotesStarts()`  
  Adds snap points at start times of notes. Returned group can be used to manage added snap points.
* `SnapPointsGroup SnapToNotesEnds()`  
  Adds snap points at end times of notes. Returned group can be used to manage added snap points.

#### `SnapPoint`

`SnapPoint<>` returned by the methods above is inherited from `SnapPoint` and just adds `Data` property that holds user data attached to a snap point. `SnapPoint` has following members:

##### `IsEnabled`

Gets or sets a value indicating whether a snap point is enabled or not. Can be used to turn a snap point on or off. If a snap point is disabled, it won't participate in [time management](#time-management) operations.

##### `Time`

Gets the time of a snap point as an instance of `TimeSpan`.

##### `SnapPointsGroup`

Gets the group a snap point belongs to as an instance of `SnapPointsGroup`. It will be `null` for snap points added by `AddSnapPoint` method.

#### `SnapPointsGroup`

`SnapPointsGroup` has `IsEnabled` property that is similar to that the `SnapPoint` has. If a snap points group is disabled, its snap points won't participate in [time management](#time-management) operations.

### Time management

You have several options to manipulate by the current time of playback:

* `GetCurrentTime()`  
  Returns the current time of a playback in the specified format.
* `MoveToStart()`  
  Sets playback position to the beginning of the MIDI data.
* `MoveToTime()`  
  Sets playback position to the specified time from the beginning of the MIDI data. If new position is greater than playback duration, position will be set to the end of the playback.
* `MoveForward()`  
  Shifts playback position forward by the specified step. If new position is greater than playback duration, position will be set to the end of the playback.
* `MoveBack()`  
  Shifts playback position back by the specified step. If step is greater than the elapsed time of playback, position will be set to the start of the playback.
* `MoveToSnapPoint(SnapPoint snapPoint)`  
  Sets playback position to the time of the specified snap point. If `snapPoint` is disabled, nothing will happen.
* `MoveToPreviousSnapPoint(SnapPointsGroup snapPointsGroup)`  
  Sets playback position to the time of the previous snap point (relative to the current time of playback) that belongs to `snapPointsGroup`.
* `MoveToPreviousSnapPoint()`  
  Sets playback position to the time of the previous snap point (relative to the current time of playback).
* `MoveToNextSnapPoint(SnapPointsGroup snapPointsGroup)`  
  Sets playback position to the time of the next snap point (relative to the current time of playback) that belongs to the specified `SnapPointsGroup`.
* `MoveToNextSnapPoint()`  
  Sets playback position to the time of the next snap point (relative to the current time of playback).

You don't need to call `Stop` method if you want to call any method that changes the current playback position.