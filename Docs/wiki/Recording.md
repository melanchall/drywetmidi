**Please note that Devices API is Windows only at now.**

To capture MIDI data from an input MIDI device (see [Input device](Input-device.md) article) you can use `Recording` class which will collect incoming MIDI events. To start recording you need create an instance of the `Recording` passing tempo map and input device to its constructor:

```csharp
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Interaction;

// ...

using (var inputDevice = InputDevice.GetByName("Input MIDI device"))
{
    var recording = new Recording(TempoMap.Default, inputDevice);

    // ...
}
```

Don't forget to call `StartEventsListening` on `InputDevice` before you start recording since `Recording` do nothing with the device you've specified.

To start recording call `Start` method. To stop it call `Stop` method. You can resume recording after it has been stopped by calling `Start` again. To check whether recording is currently running, get a value of the `IsRunning` property. `Start` and `Stop` methods fire `Started` and `Stopped` events respectively.

You can get recorded events as `IEnumerable<TimedEvent>` with the `GetEvents` method.

`GetDuration` method returns the total duration of a recording in the specified format.

Take a look at small example of MIDI data recording:

```csharp
using (var inputDevice = InputDevice.GetByName("Input MIDI device"))
{
    var recording = new Recording(TempoMap.Default, inputDevice);

    inputDevice.StartEventsListening();
    recording.Start();

    // ...

    recording.Stop();

    var recordedFile = recording.ToFile();
    recording.Dispose();
    recordedFile.Write("Recorded data.mid");
}
```