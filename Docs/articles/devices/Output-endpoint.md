---
uid: a_dev_output
---

# Output endpoint

In DryWetMIDI an output MIDI endpoint is represented by the [IOutputEndpoint](xref:Melanchall.DryWetMidi.Multimedia.IOutputEndpoint) interface. It allows to send events to a MIDI device. To understand what an output MIDI endpoint is in DryWetMIDI, please read the [Overview](Overview.md) article.

The library provides built-in implementation of `IOutputEndpoint`: [OutputEndpoint](xref:Melanchall.DryWetMidi.Multimedia.OutputEndpoint). To get an instance of `OutputEndpoint` you can use [GetByName](xref:Melanchall.DryWetMidi.Multimedia.OutputEndpoint.GetByName(System.String)) static method. To retrieve the count of output MIDI endpoints presented in the system there is the [GetEndpointsCount](xref:Melanchall.DryWetMidi.Multimedia.OutputEndpoint.GetEndpointsCount) method. You can get all output MIDI endpoints with [GetAll](xref:Melanchall.DryWetMidi.Multimedia.OutputEndpoint.GetAll) method:

```csharp
using System;
using Melanchall.DryWetMidi.Multimedia;

// ...

foreach (var outputEndpoint in OutputEndpoint.GetAll())
{
    Console.WriteLine(outputEndpoint.Name);
}
```

> [!WARNING]
> You can use `OutputEndpoint` built-in implementation of `IOutputEndpoint` only on the systems listed in the [Supported OS](xref:a_develop_supported_os) article. Of course you can create your own implementation of `IOutputEndpoint` as described in the [Custom output endpoint](#custom-output-endpoint) section below.

After an instance of `OutputEndpoint` is obtained, you can send MIDI events to the device via [SendEvent](xref:Melanchall.DryWetMidi.Multimedia.OutputEndpoint.SendEvent(Melanchall.DryWetMidi.Core.MidiEvent)) method. You cannot send [meta events](xref:Melanchall.DryWetMidi.Core.MetaEvent) since such events can be inside a MIDI file only. If you pass an instance of meta event class, `SendEvent` will do nothing. [EventSent](xref:Melanchall.DryWetMidi.Multimedia.IOutputEndpoint.EventSent) event will be fired for each event sent with `SendEvent` (except meta events) holding the MIDI event sent. The value of [DeltaTime](xref:Melanchall.DryWetMidi.Core.MidiEvent.DeltaTime) property of MIDI events will be ignored, events will be sent to the device immediately. To take delta-times into account, use [Playback](xref:Melanchall.DryWetMidi.Multimedia.Playback) class.

If you need to interrupt all currently sounding notes, call the [TurnAllNotesOff](xref:Melanchall.DryWetMidi.Multimedia.OutputEndpoint.TurnAllNotesOff) method which will send _Note Off_ events on all channels for all note numbers (a kind of "panic" button on MIDI devices).

Small example that shows sending MIDI data:

```csharp
using System;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Core;

// ...

private IOutputEndpoint _outputEndpoint;

// ...

_outputEndpoint = OutputEndpoint.GetByName("Some MIDI device");
_outputEndpoint.EventSent += OnEventSent;

_outputEndpoint.SendEvent(new NoteOnEvent());
_outputEndpoint.SendEvent(new NoteOffEvent());

// ...

private void OnEventSent(object sender, MidiEventSentEventArgs e)
{
    var midiEndpoint = (MidiEndpoint)sender;
    Console.WriteLine($"Event sent to '{midiEndpoint.Name}' at {DateTime.Now}: {e.Event}");
}

// ...

_outputEndpoint?.Dispose();
```

> [!WARNING]
> You must always take care about disposing an `OutputEndpoint`, so use it inside `using` block or call `Dispose` manually. Without it all resources taken by the endpoint will live until GC collects them via the finalizer of the `OutputEndpoint`. It means that sometimes you will not be able to use different instances of the same endpoint across multiple applications or different pieces of a program.

> [!WARNING]
> If you use an instance of the `OutputEndpoint` within a `using` block, you need to be very careful. In general it's not a good practice and can cause problems. For example, with this code
> ```csharp
> using (var outputEndpoint = OutputEndpoint.GetByName("Some MIDI device"))
> {
>     outputEndpoint.SendEvent(new NoteOnEvent());
> }
> ```
> the `NoteOnEvent` can be not sent (it's a matter of race conditions) since the program leaves the `using` block before that, and thus the endpoint instance will be destroyed.

First call of the `SendEvent` method can take some time for allocating resources for a device, so if you want to eliminate this operation on sending a MIDI event, you can call [PrepareForEventsSending](xref:Melanchall.DryWetMidi.Multimedia.IOutputEndpoint.PrepareForEventsSending) method before any MIDI event will be sent.

## Custom output endpoint

You can create your own output endpoint implementation and use it in your app. For example, let's create super simple endpoint that just outputs MIDI events to console:

```csharp
private sealed class ConsoleOutputEndpoint : IOutputEndpoint
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

You can then use this endpoint, for example, for debugging in [Playback](xref:Melanchall.DryWetMidi.Multimedia.Playback).

Another use case for custom output endpoint is plugging some synth. So you create an output endpoint where [SendEvent](xref:Melanchall.DryWetMidi.Multimedia.IOutputEndpoint.SendEvent(Melanchall.DryWetMidi.Core.MidiEvent)) will redirect MIDI events to the synth.