In DryWetMIDI an output MIDI device is represented by `OutputDevice` class. It allows to send events to a MIDI device. To understand what an output MIDI device is in DryWetMIDI, please read [MIDI devices](MIDI-devices.md) article.

To get an instance of `OutputDevice` you can use either `GetByName` or `GetById` static methods. ID of a MIDI device is a number from 0 to devices count minus one. To retrieve count of output MIDI devices presented in the system there is the `GetDevicesCount` method. You can get all output MIDI devices with `GetAll` method:

```csharp
using System;
using Melanchall.DryWetMidi.Devices;

// ...

foreach (var outputDevice in OutputDevice.GetAll())
{
    Console.WriteLine(outputDevice.Name);
}
```

After an instance of `OutputDevice` is obtained, you can send MIDI events to device via `SendEvent` method. You cannot send [meta events](Meta-events.md) since such events can be inside a MIDI file only. If you pass an instance of meta event class, `SendEvent` will do nothing. `EventSent` event will be fired for each event sent with `SendEvent` holding the MIDI event. The value of `DeltaTime` property of MIDI events will be ignored, events will be sent to device immediately. To take delta-times into account, use `Playback` class (read [Playback](Playback.md) article to learn more).

If you need to interrupt all currently sounding notes, call the `TurnAllNotesOff` method which will send _Note Off_ events on all channels for all note numbers (kind of "panic button" on MIDI devices).

See [MIDI devices](MIDI-devices.md) page for common members of a MIDI device class that are inherited by `OutputDevice` from the base class `MidiDevice`.

Small example that shows sending MIDI data:

```csharp
using System;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Core;

// ...

using (var outputDevice = OutputDevice.GetByName("Some MIDI device"))
{
    outputDevice.EventSent += OnEventSent;

    outputDevice.SendEvent(new NoteOnEvent());
    outputDevice.SendEvent(new NoteOffEvent());
}

// ...

private void OnEventSent(object sender, MidiEventSentEventArgs e)
{
    var midiDevice = (MidiDevice)sender;
    Console.WriteLine($"Event sent to '{midiDevice.Name}' at {DateTime.Now}: {e.Event}");
}
```

Note that you should always take care about disposing an `OutputDevice`, i.e. use it inside `using` block or call `Dispose` manually. Without it all resources taken by the device will live until GC collect them via finalizer of the `OutputDevice`. It means that sometimes you will not be able to use different instances of the same device across multiple applications or different pieces of a program.

First call of `SendEvent` method can take some time for allocating resources for device, so if you want to eliminate this operation on sending a MIDI event, you can call `PrepareForEventsSending` method before any MIDI event will be sent.

Sections below describes specific properties of the `OutputDevice` class.

#### `DeviceType`

Gets the type of the current `OutputDevice`. Possible values are listed in the table below:

Value | Description
----- | -----------
`MidiPort` | MIDI hardware port.
`Synth` | Synthesizer.
`SquareWaveSynth` | Square wave synthesizer.
`FmSynth` | FM synthesizer.
`MidiMapper` | Microsoft MIDI mapper.
`WavetableSynth` | Hardware wavetable synthesizer.
`SoftwareSynth` | Software synthesizer.

#### `VoicesNumber`

Gets the number of voices supported by an internal synthesizer device. If the device is a port, this member is not meaningful and will be 0.

#### `NotesNumber`

Gets the maximum number of simultaneous notes that can be played by an internal synthesizer device. If the device is a port, this member is not meaningful and will be 0.

#### `Channels`

Gets the channels that an internal synthesizer device responds to.

#### `SupportsPatchCaching`

Gets a value indicating whether device supports patch caching.

#### `SupportsVolumeControl`

Gets a value indicating whether device supports volume control.

#### `SupportsLeftRightVolumeControl`

Gets a value indicating whether device supports separate left and right volume control or not.

#### `Volume`

Gets or sets the volume of the output MIDI device. A value is an instance of the `Volume` class holding volume value for left and right channels. If `SupportsLeftRightVolumeControl` is `false` and you pass `Volume` with different values for each channel, an exception will be thrown.