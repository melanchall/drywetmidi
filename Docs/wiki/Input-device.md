In DryWetMIDI an input MIDI device is represented by `InputDevice` class. It allows to receive events from a MIDI device. To understand what an input MIDI device is in DryWetMIDI, please read [MIDI devices](MIDI-devices.md) article.

To get an instance of `InputDevice` you can use either `GetByName` or `GetById` static methods. ID of a MIDI device is a number from 0 to devices count minus one. To retrieve count of input MIDI devices presented in the system there is the `GetDevicesCount` method. You can get all input MIDI devices with `GetAll` method.

After an instance of `InputDevice` is obtained, call `StartEventsListening` to start listening incoming MIDI events going from an input MIDI device. If you don't need to listen for events anymore, call `StopEventsListening`. To check whether `InputDevice` is currently listening for events use `IsListeningForEvents` property.

If an input device is listening for events, it will fire `EventReceived` event for each incoming MIDI event. Args of the event hold a `MidiEvent` received.

See [MIDI devices](MIDI-devices.md) page for common members of a MIDI device class that are inherited by `InputDevice` from the base class `MidiDevice`.

Small example that shows receiving MIDI data:

```csharp
using System;
using Melanchall.DryWetMidi.Devices;

// ...

using (var inputDevice = InputDevice.GetByName("Some MIDI device"))
{
    inputDevice.EventReceived += OnEventReceived;
    inputDevice.StartEventsListening();
}

// ...

private void OnEventReceived(object sender, MidiEventReceivedEventArgs e)
{
    var midiDevice = (MidiDevice)sender;
    Console.WriteLine($"Event received from '{midiDevice.Name}' at {DateTime.Now}: {e.Event}");
}
```

Note that you should always take care about disposing an `InputDevice`, i.e. use it inside `using` block or call `Dispose` manually. Without it all resources taken by the device will live until GC collect them via finalizer of the `InputDevice`. It means that sometimes you will not be able to use different instances of the same device across multiple applications or different pieces of a program.

By default `InputDevice` will fire `MidiTimeCodeReceived` event when all MIDI Time Code components (`MidiTimeCodeEvent` events) are received forming _hours:minutes:seconds:frames_ timestamp. You can turn this behavior off by setting `RaiseMidiTimeCodeReceived` to `false`.

If an invalid channel, system common or system real-time event received, `InvalidShortEventReceived` event will be fired holding the bytes that form the invalid event. If invalid system exclusive event received, `InvalidSysExEventReceived` event will be fired holding sysex data.

To reset an input device call `Reset` method.