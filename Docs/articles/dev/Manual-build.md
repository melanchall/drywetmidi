---
uid: a_develop_manual_build
---

# Manual build

This article describes how you can manually build the library from sources. Just follow the steps below:

1. Select the branch you want to build sources from (_master_ or _develop_).
2. Download sources with any method you want:
    * via `Code` → `Download ZIP` button on GitHub, then extract archive; or
    * `git clone https://github.com/melanchall/drywetmidi.git`; or
    * somehow else.
3. Download native binaries required to work with MIDI devices and default playback:
    * for _master_ branch take _DryWetMIDI.<release_number>-bin-native.zip_ archive from [Releases](https://github.com/melanchall/drywetmidi/releases) (<release_number> is the number of the library release you want to build);
    * for _develop_ branch go to https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=6&branchName=develop and download _DryWetMIDI.<release_number>-bin-native.zip_ from `Artifacts` → `Binaries`.
4. Extract the archive and place extracted files near `<your_local_folder_with_repository>\DryWetMidi\Melanchall.DryWetMidi.csproj`.
5. Build the solution `<your_local_folder_with_repository>\Melanchall.DryWetMidi.slnx`.

For build you can use any tool you want: `dotnet` CLI, Visual Studio, Rider and so on. The library uses "new" csproj format so your build tools should be modern enough.

_master_ branch contains code that the library releases built on. _develop_ one is for current development so if you need the latest code, use this branch.

## Build configuration

There are four build configurations available:

* `Debug` – for development, debugging and testing. The library is not optimized and contains all debug information. [Traces](#traces) are generated.
* `Release` – for production. The library is optimized and does not contain debug information. It's the configuration used to build official releases. [Traces](#traces) are disabled.
* `ReleaseTest` – for running tests in release mode. The library is optimized and does not contains debug information. Additional tests are enabled in this configuration. Use it to run tests without generating additional files on your machine. [Traces](#traces) are disabled.
* `ReleaseTestFull` – for running tests in release mode with gathering additional information (playback traces, for example). The library is optimized and does not contain debug information. [Traces](#traces) are generated.

### Traces

When you build the library in `Debug` or `ReleaseTestFull` configuration and run tests that use playback functionality, the following happens:

  * `PlaybackTraces` folder is created in the temp folder of your machine (you can get the path to it by calling `Path.GetTempPath()` method);
  * inside the folder _log_ files are created that contain information about playback events (when playback started, when it stopped, what events were sent to the output device and when they were sent, etc.);
  * inside the folder _png_ files are created that contain timelines of playback events along with MIDI clock ticks times and delays.