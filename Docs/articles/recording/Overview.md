---
uid: a_recording_overview
---

# Recording – Overview

To capture MIDI data from an input MIDI endpoint (see [Input endpoint](xref:a_dev_input) article) you can use [Recording](xref:Melanchall.DryWetMidi.Multimedia.Recording) class which will collect incoming MIDI events. To start recording you need create an instance of the `Recording` class passing [tempo map](xref:Melanchall.DryWetMidi.Interaction.TempoMap) and input endpoint to its constructor:

```csharp
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Interaction;

// ...

using (var inputEndpoint = InputEndpoint.GetByName("Input MIDI endpoint"))
{
    var recording = new Recording(TempoMap.Default, inputEndpoint);

    // ...
}
```

Don't forget to call [StartEventsListening](xref:Melanchall.DryWetMidi.Multimedia.IInputEndpoint.StartEventsListening) on [IInputEndpoint](xref:Melanchall.DryWetMidi.Multimedia.IInputEndpoint) before you start recording since `Recording` does nothing with the endpoint you've specified.

To start recording, call the [Start](xref:Melanchall.DryWetMidi.Multimedia.Recording.Start) method. To stop it, call the [Stop](xref:Melanchall.DryWetMidi.Multimedia.Recording.Stop) method. You can resume recording after it has been stopped by calling `Start` again. To check whether recording is currently running or not, get a value of the [IsRunning](xref:Melanchall.DryWetMidi.Multimedia.Recording.IsRunning) property. `Start` and `Stop` methods fire [Started](xref:Melanchall.DryWetMidi.Multimedia.Recording.Started) and [Stopped](xref:Melanchall.DryWetMidi.Multimedia.Recording.Stopped) events respectively.

You can get recorded events as with the [GetEvents](xref:Melanchall.DryWetMidi.Multimedia.Recording.GetEvents) method.

Take a look at small example of MIDI data recording:

```csharp
using (var inputEndpoint = InputEndpoint.GetByName("Input MIDI endpoint"))
{
    var recording = new Recording(TempoMap.Default, inputEndpoint);

    inputEndpoint.StartEventsListening();
    recording.Start();

    // ...

    recording.Stop();

    var recordedFile = recording.ToFile();
    recording.Dispose();
    recordedFile.Write("Recorded data.mid");
}
```