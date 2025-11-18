---
uid: a_dev_overview
---

# Overview

DryWetMIDI provides the ability to send MIDI data to or receive it from MIDI devices. For that purpose there are following types:

* [IInputDevice](xref:Melanchall.DryWetMidi.Multimedia.IInputDevice) (see [Input device](Input-device.md) article);
* [IOutputDevice](xref:Melanchall.DryWetMidi.Multimedia.IOutputDevice) (see [Output device](Output-device.md) article);
* [DevicesConnector](xref:Melanchall.DryWetMidi.Multimedia.DevicesConnector) (see [Devices connector](Devices-connector.md) article).

The library provides implementations for both `IInputDevice` and `IOutputDevice`: [InputDevice](xref:Melanchall.DryWetMidi.Multimedia.InputDevice) and [OutputDevice](xref:Melanchall.DryWetMidi.Multimedia.OutputDevice) correspondingly which represent MIDI devices visible by the operating system. Both classes implement [IDisposable](xref:System.IDisposable) interface so you should always dispose of them to free devices for use by other applications.

> [!WARNING]
> You can use `InputDevice` and `OutputDevice` built-in implementations of `IInputDevice` and `IOutputDevice` only on the systems listed in the [Supported OS](xref:a_develop_supported_os) article. Of course you can create your own implementations of `IInputDevice` and `IOutputDevice`.

All classes that interact with devices work with interfaces mentioned above, so you can create custom implementations of your devices (see examples in [Input device](Input-device.md) and [Output device](Output-device.md) articles) and use it for playback or recording, for example.

MIDI devices API classes are placed in the [Melanchall.DryWetMidi.Multimedia](xref:Melanchall.DryWetMidi.Multimedia) namespace.

To understand what is an input and an output device in DryWetMIDI take a look at the following image:

![Devices](images/Devices.png)

So, as you can see, although a MIDI port is _MIDI IN_ for a MIDI device, it will be an **output device** in DryWetMIDI because your application will **send MIDI data to** this port. _MIDI OUT_ of MIDI device will be an **input device** in DryWetMIDI because a program will **receive MIDI data from** the port.

If some error occurred during sending or receiving a MIDI event, the [ErrorOccurred](xref:Melanchall.DryWetMidi.Multimedia.MidiDevice.ErrorOccurred) event will be fired holding an exception caused by the error.
