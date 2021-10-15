---
uid: a_recording_overview
---

# Recording - Overview

To capture MIDI data from an input MIDI device (see [Input device](xref:a_dev_input) article) you can use [Recording](xref:Melanchall.DryWetMidi.Multimedia.Recording) class which will collect incoming MIDI events. To start recording you need create an instance of the `Recording` class passing [tempo map](xref:a_hldm_tempomap) and input device to its constructor:

```csharp
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Interaction;

// ...

using (var inputDevice = InputDevice.GetByName("Input MIDI device"))
{
    var recording = new Recording(TempoMap.Default, inputDevice);

    // ...
}
```

Don't forget to call [StartEventsListening](xref:Melanchall.DryWetMidi.Multimedia.IInputDevice.StartEventsListening) on [IInputDevice](xref:Melanchall.DryWetMidi.Multimedia.IInputDevice) before you start recording since `Recording` do nothing with the device you've specified.

To start recording call [Start](xref:Melanchall.DryWetMidi.Multimedia.Recording.Start) method. To stop it call [Stop](xref:Melanchall.DryWetMidi.Multimedia.Recording.Stop) method. You can resume recording after it has been stopped by calling `Start` again. To check whether recording is currently running or not, get a value of the [IsRunning](xref:Melanchall.DryWetMidi.Multimedia.Recording.IsRunning) property. `Start` and `Stop` methods fire [Started](xref:Melanchall.DryWetMidi.Multimedia.Recording.Started) and [Stopped](xref:Melanchall.DryWetMidi.Multimedia.Recording.Stopped) events respectively.

You can get recorded events as with [GetEvents](xref:Melanchall.DryWetMidi.Multimedia.Recording.GetEvents) method.

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