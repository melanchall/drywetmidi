---
uid: a_develop_nativeless
---

# Nativeless package

DryWetMIDI is shipped in two versions:

* [Melanchall.DryWetMidi](https://www.nuget.org/packages/Melanchall.DryWetMidi);
* [Melanchall.DryWetMidi.Nativeless](https://www.nuget.org/packages/Melanchall.DryWetMidi.Nativeless).

First one is the version containing all the features of the library and you should use it in most cases. But some things require platform-specific code which is placed in native binaries packed along with the main library. If you've encountered problems with such code and you don't need an API that depends on native binaries, you can use [Melanchall.DryWetMidi.Nativeless](https://www.nuget.org/packages/Melanchall.DryWetMidi.Nativeless) package where such things are cut out. Following types are unavailable in the nativeless package:

* [VirtualDevice](xref:Melanchall.DryWetMidi.Multimedia.VirtualDevice);
* [EndpointsWatcher](xref:Melanchall.DryWetMidi.Multimedia.EndpointsWatcher);
* [EndpointAddedRemovedEventArgs](xref:Melanchall.DryWetMidi.Multimedia.EndpointAddedRemovedEventArgs);
* [MidiEndpoint](xref:Melanchall.DryWetMidi.Multimedia.MidiEndpoint);
* [InputEndpoint](xref:Melanchall.DryWetMidi.Multimedia.InputEndpoint);
* [InputEndpointProperty](xref:Melanchall.DryWetMidi.Multimedia.InputEndpointProperty);
* [MidiTimeCodeReceivedEventArgs](xref:Melanchall.DryWetMidi.Multimedia.MidiTimeCodeReceivedEventArgs);
* [OutputEndpoint](xref:Melanchall.DryWetMidi.Multimedia.OutputEndpoint);
* [OutputEndpointOption](xref:Melanchall.DryWetMidi.Multimedia.OutputEndpointOption);
* [OutputEndpointProperty](xref:Melanchall.DryWetMidi.Multimedia.OutputEndpointProperty);
* [OutputEndpointTechnology](xref:Melanchall.DryWetMidi.Multimedia.OutputEndpointTechnology);
* [TickGeneratorException](xref:Melanchall.DryWetMidi.Multimedia.TickGeneratorException);
* [HighPrecisionTickGenerator](xref:Melanchall.DryWetMidi.Multimedia.HighPrecisionTickGenerator).

Also default tick generator for [Playback](xref:Melanchall.DryWetMidi.Multimedia.Playback) there is [RegularPrecisionTickGenerator](xref:Melanchall.DryWetMidi.Multimedia.RegularPrecisionTickGenerator) instead of [HighPrecisionTickGenerator](xref:Melanchall.DryWetMidi.Multimedia.HighPrecisionTickGenerator).

Although built-in implementations of [IInputEndpoint](xref:Melanchall.DryWetMidi.Multimedia.IInputEndpoint) and [IOutputEndpoint](xref:Melanchall.DryWetMidi.Multimedia.IOutputEndpoint) are unavailable in the nativeless package, you are still able to create your own implementations and use them across the library API (in [Playback](xref:Melanchall.DryWetMidi.Multimedia.Playback) for example).