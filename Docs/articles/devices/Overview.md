---
uid: a_dev_overview
---

# Devices – Overview

DryWetMIDI provides the ability to send MIDI data to and receive it from MIDI devices. For that purpose there are following types:

* [`IInputEndpoint`](xref:Melanchall.DryWetMidi.Multimedia.IInputEndpoint) (see [Input endpoint](Input-endpoint.md) article);
* [`IOutputEndpoint`](xref:Melanchall.DryWetMidi.Multimedia.IOutputEndpoint) (see [Output endpoint](Output-endpoint.md) article);
* [`EndpointsConnector`](xref:Melanchall.DryWetMidi.Multimedia.EndpointsConnector) (see [Endpoints connector](Endpoints-connector.md) article).

Endpoints are "sockets" from the DryWetMIDI side to work with MIDI devices. An input endpoint is a "socket" to receive MIDI data from a MIDI device, an output endpoint is a "socket" to send MIDI data to a MIDI device. To understand what input and output endpoints are take a look at the following image:

![Devices](images/Devices.png)

So, as you can see, although a MIDI port is _MIDI IN_ for a MIDI device, it will be an **output endpoint** in DryWetMIDI because your application will **send MIDI data to** this port. _MIDI OUT_ of MIDI device will be an **input endpoint** in DryWetMIDI because a program will **receive MIDI data from** the port. In some other libraries and frameworks you may see that input endpoints named as sources and output endpoints named as destinations.

The library provides implementations for both `IInputEndpoint` and `IOutputEndpoint`: [`InputEndpoint`](xref:Melanchall.DryWetMidi.Multimedia.InputEndpoint) and [`OutputEndpoint`](xref:Melanchall.DryWetMidi.Multimedia.OutputEndpoint) correspondingly which represent endpoints of MIDI devices visible by the operating system. Both classes implement [`IDisposable`](xref:System.IDisposable) interface so you should always dispose them to free ports for use by other applications.

> [!WARNING]
> <os-specific-api `InputEndpoint`;`OutputEndpoint`/> Of course you can create your own implementations of `IInputEndpoint` and `IOutputEndpoint` (see below).

You may want to know information about a device an endpoint belongs to – just call [`GetDeviceInformation`](xref:Melanchall.DryWetMidi.Multimedia.MidiEndpoint.GetDeviceInformation) on `InputEndpoint` or `OutputEndpoint`. You can use returned object to group endpoints by devices, for example:

```csharp
var inputEndpoints = InputEndpoint.GetAll();
var outputEndpoints = OutputEndpoint.GetAll();
var devices = inputEndpoints
    .Select(e => e.GetDeviceInformation())
    .Concat(outputEndpoints.Select(e => e.GetDeviceInformation()))
    .Distinct()
    .Where(e => e != null)
    .ToArray();

void WriteDeviceEndpoints<TEndpoint>(DeviceInformation? device, IEnumerable<TEndpoint> endpoints, string prefix)
        where TEndpoint : MidiEndpoint
{
    Console.WriteLine(string.Join(
        Environment.NewLine,
        endpoints
            .Where(e => e.GetDeviceInformation()?.Equals(device) == true)
            .Select(e => $"    {prefix}: {e.Name}")));
}

foreach (var device in devices)
{
    Console.WriteLine($"{device.Name}:");
    WriteDeviceEndpoints(device, inputEndpoints, "in");
    WriteDeviceEndpoints(device, outputEndpoints, "out");
}

Console.WriteLine("No device endpoints:");
WriteDeviceEndpoints(null, inputEndpoints, "in");
WriteDeviceEndpoints(null, outputEndpoints, "out");
```

All classes that interact with devices work with interfaces mentioned above, so you can create custom implementations of your devices (see examples in [Input endpoint](Input-endpoint.md) and [Output endpoint](Output-endpoint.md) articles) and use them for [playback](xref:a_playback_overview) or [recording](xref:a_recording_overview), for example.

MIDI devices API classes are placed in the [`Melanchall.DryWetMidi.Multimedia`](xref:Melanchall.DryWetMidi.Multimedia) namespace.

If some error occurred during sending or receiving a MIDI event, the [`ErrorOccurred`](xref:Melanchall.DryWetMidi.Multimedia.MidiEndpoint.ErrorOccurred) event will be fired holding an exception object.
