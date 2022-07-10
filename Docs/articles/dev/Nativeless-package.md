---
uid: a_develop_nativeless
---

# Nativeless package

DryWetMIDI is shipped in two versions:

* [Melanchall.DryWetMidi](https://www.nuget.org/packages/Melanchall.DryWetMidi);
* [Melanchall.DryWetMidi.Nativeless](https://www.nuget.org/packages/Melanchall.DryWetMidi.Nativeless).

First one is the version containing all the features of the library and you should use it in most cases. But some things require platform-specific code which placed in native binaries packed along with the main library. If you've encountered problems with such code and you don't need API that depends on native binaries, you can use [Melanchall.DryWetMidi.Nativeless](https://www.nuget.org/packages/Melanchall.DryWetMidi.Nativeless) package where such things are cut out. Following types are unavailable in the nativeless package:

* [VirtualDevice](xref:Melanchall.DryWetMidi.Multimedia.VirtualDevice);
* [DevicesWatcher](xref:Melanchall.DryWetMidi.Multimedia.DevicesWatcher);
* [DeviceAddedRemovedEventArgs](xref:Melanchall.DryWetMidi.Multimedia.DeviceAddedRemovedEventArgs);
* [MidiDevice](xref:Melanchall.DryWetMidi.Multimedia.MidiDevice);
* [InputDevice](xref:Melanchall.DryWetMidi.Multimedia.InputDevice);
* [InputDeviceProperty](xref:Melanchall.DryWetMidi.Multimedia.InputDeviceProperty);
* [MidiTimeCodeReceivedEventArgs](xref:Melanchall.DryWetMidi.Multimedia.MidiTimeCodeReceivedEventArgs);
* [OutputDevice](xref:Melanchall.DryWetMidi.Multimedia.OutputDevice);
* [OutputDeviceOption](xref:Melanchall.DryWetMidi.Multimedia.OutputDeviceOption);
* [OutputDeviceProperty](xref:Melanchall.DryWetMidi.Multimedia.OutputDeviceProperty);
* [OutputDeviceTechnology](xref:Melanchall.DryWetMidi.Multimedia.OutputDeviceTechnology);
* [TickGeneratorException](xref:Melanchall.DryWetMidi.Multimedia.TickGeneratorException);
* [HighPrecisionTickGenerator](xref:Melanchall.DryWetMidi.Multimedia.HighPrecisionTickGenerator).

Also default tick generator for [Playback](xref:Melanchall.DryWetMidi.Multimedia.Playback) there is [RegularPrecisionTickGenerator](xref:Melanchall.DryWetMidi.Multimedia.RegularPrecisionTickGenerator) instead of [HighPrecisionTickGenerator](xref:Melanchall.DryWetMidi.Multimedia.HighPrecisionTickGenerator).

Although built-in implementations of [IInputDevice](xref:Melanchall.DryWetMidi.Multimedia.IInputDevice) and [IOutputDevice](xref:Melanchall.DryWetMidi.Multimedia.IOutputDevice) are unavailable in the nativeless package, you are still able to create your own implementations and use across the library API (in [Playback](xref:Melanchall.DryWetMidi.Multimedia.Playback) for example).