---
uid: a_dev_overview
---

# Overview

**Please note that Devices API is supported for Windows only at now.**

DryWetMIDI provides ability to send MIDI data to or receive it from MIDI devices. For that purpose there are following types:

* [IInputDevice](xref:Melanchall.DryWetMidi.Devices.IInputDevice) (see [Input device](Input-device.md));
* [IOutputDevice](xref:Melanchall.DryWetMidi.Devices.IOutputDevice) (see [Output device](Output-device.md));
* [DevicesConnector](xref:Melanchall.DryWetMidi.Devices.DevicesConnector) (see [Devices connector](Devices-connector.md)).

The library provides implementations for both `IInputDevice` and `IOutputDevice`: [InputDevice](xref:Melanchall.DryWetMidi.Devices.InputDevice) and [OutputDevice](xref:Melanchall.DryWetMidi.Devices.OutputDevice) correspondingly which represent MIDI devices visible by the operating system. Both classes implement [IDisposable](xref:System.IDisposable) interface so you should always dispose them to free devices for using by another applications.

All classes that interact with devices work with interfaces mentioned above, so you can create custom implementation of your devices (see examples in [Input device](Input-device.md) and [Output device](Output-device.md) articles) and use it for playback or recording, for example.

MIDI devices API classes are placed in the [Melanchall.DryWetMidi.Devices](xref:Melanchall.DryWetMidi.Devices) namespace.

To understand what is input and output device in DryWetMIDI take a look at following image:

![Devices](images/Devices.png)

So, as you can see, although a MIDI port is _MIDI IN_ for MIDI device, it will be an **output device** in DryWetMIDI because your application will **send MIDI data to** this port. _MIDI OUT_ of MIDI device will be an **input device** in DryWetMIDI because a program will **receive MIDI data from** the port.

If some error occured during sending or receiving a MIDI event, the [ErrorOccurred](xref:Melanchall.DryWetMidi.Devices.MidiDevice.ErrorOccurred) event will be fired holding an exception caused the error.