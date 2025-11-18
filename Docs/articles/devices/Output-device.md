---
uid: a_dev_output
---

# Output device

In DryWetMIDI an output MIDI device is represented by the [IOutputDevice](xref:Melanchall.DryWetMidi.Multimedia.IOutputDevice) interface. It allows to send events to a MIDI device. To understand what an output MIDI device is in DryWetMIDI, please read the [Overview](Overview.md) article.

The library provides built-in implementation of `IOutputDevice`: [OutputDevice](xref:Melanchall.DryWetMidi.Multimedia.OutputDevice). To get an instance of `OutputDevice` you can use either [GetByName](xref:Melanchall.DryWetMidi.Multimedia.OutputDevice.GetByName(System.String)) or [GetByIndex](xref:Melanchall.DryWetMidi.Multimedia.OutputDevice.GetByIndex(System.Int32)) static methods. To retrieve the count of output MIDI devices presented in the system there is the [GetDevicesCount](xref:Melanchall.DryWetMidi.Multimedia.OutputDevice.GetDevicesCount) method. You can get all output MIDI devices with [GetAll](xref:Melanchall.DryWetMidi.Multimedia.OutputDevice.GetAll) method:

```csharp
using System;
using Melanchall.DryWetMidi.Multimedia;

// ...

foreach (var outputDevice in OutputDevice.GetAll())
{
    Console.WriteLine(outputDevice.Name);
}
```

> [!WARNING]
> You can use `OutputDevice` built-in implementation of `IOutputDevice` only on the systems listed in the [Supported OS](xref:a_develop_supported_os) article. Of course you can create your own implementation of `IOutputDevice` as described in the [Custom output device](#custom-output-device) section below.

After an instance of `OutputDevice` is obtained, you can send MIDI events to the device via [SendEvent](xref:Melanchall.DryWetMidi.Multimedia.OutputDevice.SendEvent(Melanchall.DryWetMidi.Core.MidiEvent)) method. You cannot send [meta events](xref:Melanchall.DryWetMidi.Core.MetaEvent) since such events can be inside a MIDI file only. If you pass an instance of meta event class, `SendEvent` will do nothing. [EventSent](xref:Melanchall.DryWetMidi.Multimedia.IOutputDevice.EventSent) event will be fired for each event sent with `SendEvent` (except meta events) holding the MIDI event sent. The value of [DeltaTime](xref:Melanchall.DryWetMidi.Core.MidiEvent.DeltaTime) property of MIDI events will be ignored, events will be sent to the device immediately. To take delta-times into account, use [Playback](xref:Melanchall.DryWetMidi.Multimedia.Playback) class.

If you need to interrupt all currently sounding notes, call the [TurnAllNotesOff](xref:Melanchall.DryWetMidi.Multimedia.OutputDevice.TurnAllNotesOff) method which will send _Note Off_ events on all channels for all note numbers (a kind of "panic" button on MIDI devices).

Small example that shows sending MIDI data:

```csharp
using System;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Core;

// ...

private OutputDevice _outputDevice;

// ...

_outputDevice = OutputDevice.GetByName("Some MIDI device");
_outputDevice.EventSent += OnEventSent;

_outputDevice.SendEvent(new NoteOnEvent());
_outputDevice.SendEvent(new NoteOffEvent());

// ...

private void OnEventSent(object sender, MidiEventSentEventArgs e)
{
    var midiDevice = (MidiDevice)sender;
    Console.WriteLine($"Event sent to '{midiDevice.Name}' at {DateTime.Now}: {e.Event}");
}

// ...

_outputDevice?.Dispose();
```

> [!WARNING]
> You must always take care about disposing an `OutputDevice`, so use it inside `using` block or call `Dispose` manually. Without it all resources taken by the device will live until GC collects them via the finalizer of the `OutputDevice`. It means that sometimes you will not be able to use different instances of the same device across multiple applications or different pieces of a program.

> [!WARNING]
> If you use an instance of the `OutputDevice` within a `using` block, you need to be very careful. In general it's not a good practice and can cause problems. For example, with this code
> ```csharp
> using (var outputDevice = OutputDevice.GetByName("Some MIDI device"))
> {
>     outputDevice.SendEvent(new NoteOnEvent());
> }
> ```
> the `NoteOnEvent` can be not sent (it's a matter of race conditions) since the program leaves the `using` block before that, and thus the device instance will be destroyed.

First call of the `SendEvent` method can take some time for allocating resources for a device, so if you want to eliminate this operation on sending a MIDI event, you can call [PrepareForEventsSending](xref:Melanchall.DryWetMidi.Multimedia.IOutputDevice.PrepareForEventsSending) method before any MIDI event will be sent.

## Custom output device

You can create your own output device implementation and use it in your app. For example, let's create super simple device that just outputs MIDI events to console:

```csharp
private sealed class ConsoleOutputDevice : IOutputDevice
{
    public event EventHandler<MidiEventSentEventArgs> EventSent;

    public void PrepareForEventsSending()
    {
    }

    public void SendEvent(MidiEvent midiEvent)
    {
        Console.WriteLine(midiEvent);
    }

    public void Dispose()
    {
    }
}
```

You can then use this device, for example, for debugging in [Playback](xref:Melanchall.DryWetMidi.Multimedia.Playback).

Another use case for custom output device is plugging some synth. So you create an output device where [SendEvent](xref:Melanchall.DryWetMidi.Multimedia.IOutputDevice.SendEvent(Melanchall.DryWetMidi.Core.MidiEvent)) will redirect MIDI events to the synth.