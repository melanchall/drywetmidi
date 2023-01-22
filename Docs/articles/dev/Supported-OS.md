---
uid: a_develop_supported_os
---

# Supported OS

This article describes what operating systems the library supports and what parts of its API can be used on those systems.

First of all, please take a look at the start of the [Project health](xref:a_develop_project_health) article which states DryWetMIDI consists of two parts logically – **Core API** and **Multimedia API**. Now we can define what APIs and where you can use across different platforms:

## Core API

Can be used on any platform .NET supports. You can find the list of the supported platforms here – [https://github.com/dotnet/core/tree/main/release-notes](https://github.com/dotnet/core/tree/main/release-notes). Just open the folder that corresponds to the version of .NET you're interested in and open **supported-os.md** file there. For example, for .NET 7 the file is [https://github.com/dotnet/core/blob/main/release-notes/7.0/supported-os.md](https://github.com/dotnet/core/blob/main/release-notes/7.0/supported-os.md).

## Multimedia API

Can be used currently on Windows and macOS only. Here the list of the key classes:

* [InputDevice](xref:Melanchall.DryWetMidi.Multimedia.InputDevice);
* [OutputDevice](xref:Melanchall.DryWetMidi.Multimedia.OutputDevice);
* [VirtualDevice](xref:Melanchall.DryWetMidi.Multimedia.VirtualDevice) (implemented for **macOS** only);
* [DevicesWatcher](xref:Melanchall.DryWetMidi.Multimedia.DevicesWatcher) (implemented for **macOS** only);
* [HighPrecisionTickGenerator](xref:Melanchall.DryWetMidi.Multimedia.HighPrecisionTickGenerator).