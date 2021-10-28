---
uid: a_develop_manual_build
---

# Manual build

This article describes how you can manually build the library from sources. Just follow the steps below:

1. Select branch you want to build sources from (_master_ or _develop_).
2. Download sources with any method you want:
    * via `Code` → `Download ZIP` button on GitHub, then extract archive; or
    * `git clone https://github.com/melanchall/drywetmidi.git`; or
    * somehow else.
3. Download native binaries required to work with MIDI devices and default playback:
    * for _master_ branch take _DryWetMIDI.<release_number>-bin-native.zip_ archive from [Releases](https://github.com/melanchall/drywetmidi/releases) (<release_number> is the number of the library release you want to build);
    * for _develop_ branch go to https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=6&branchName=develop and download _DryWetMIDI.<release_number>-bin-native.zip_ from `Artifacts` → `Binaries`.
4. Extract the archive and place extracted files near `<your_local_folder_with_repository>\DryWetMidi\Melanchall.DryWetMidi.csproj`.
5. Build the solution `<your_local_folder_with_repository>\Melanchall.DryWetMidi.sln`.

For build you can use any tool you want: `dotnet` CLI, Visual Studio, Rider and so on. The library uses "new" csproj format so your build tools should be modern enough.

_master_ branch contains code that the library releases built on. _develop_ one is for current development so if you need the latest code, use this branch.