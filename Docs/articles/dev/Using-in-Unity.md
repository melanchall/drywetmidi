---
uid: a_develop_unity
---

# Using in Unity

This article describes how to use DryWetMIDI within a Unity project. All you need is:

1. Create _DryWetMIDI_ folder in project's _Assets_ folder.
2. Download the library main binary:
    * for _master_ branch take _DryWetMIDI.<release_number>-bin-netstandard20.zip_ archive from [Releases](https://github.com/melanchall/drywetmidi/releases) (<release_number> is the number of the library release you want to take binaries of);
    * for _develop_ branch go to https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=6&branchName=develop and download _DryWetMIDI.<release_number>-bin-netstandard20.zip_ from `Artifacts` → `Binaries`.
3. Download native binaries required to work with MIDI devices and default playback:
    * for _master_ branch take _DryWetMIDI.<release_number>-bin-native.zip_ archive from [Releases](https://github.com/melanchall/drywetmidi/releases);
    * for _develop_ branch go to https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=6&branchName=develop and download _DryWetMIDI.<release_number>-bin-native.zip_ from `Artifacts` → `Binaries`.

    (_master_ branch contains code that the library releases built on. _develop_ one is for current development so if you need the latest API, use this branch)
4. Extract those two archives into project's _Assets_ → _DryWetMIDI_ folder.

Now you can use DryWetMIDI API in your Unity scripts.

Following script will listen all [input devices](xref:a_dev_input) for incoming MIDI events and print them to `Debug.Log`:

```csharp
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Multimedia;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    private InputDevice[] _inputDevices;

    private void Start()
    {
        Debug.Log("Start MIDI events listening on all input device...");

        _inputDevices = InputDevice.GetAll().ToArray();

        foreach (var inputDevice in _inputDevices)
        {
            inputDevice.EventReceived += OnEventReceived;
            inputDevice.StartEventsListening();
        }

        Debug.Log("MIDI events listening started.");
    }

    private void OnDestroy()
    {
        Debug.Log("Releasing devices resources...");

        foreach (var inputDevice in _inputDevices)
        {
            inputDevice.EventReceived -= OnEventReceived;
            inputDevice.Dispose();
        }

        Debug.Log("Devices resources released.");
    }

    private void OnEventReceived(object sender, MidiEventReceivedEventArgs e)
    {
        Debug.Log($"Event received on '{((MidiDevice)sender).Name}': {e.Event}");
    }
}
```

> [!IMPORTANT]
> Pay attention to `OnDestroy` method. You should always take care about disposing MIDI devices. Without it all resources taken by the device will live until GC collect them. In case of Unity it means Unity may need be reopened to be able to use the same devices again (for example, on Windows).

Log will contain records like these:

```text
Start MIDI events listening on all input device...
MIDI events listening started.

Event received on 'MIDI B': Note On [0] (38, 82)
Event received on 'MIDI B': Note Off [0] (38, 0)
Event received on 'MIDI B': Control Change [0] (123, 0)
Event received on 'MIDI B': Control Change [0] (121, 0)
Event received on 'MIDI B': Control Change [1] (123, 0)
Event received on 'MIDI B': Control Change [1] (121, 0)
Event received on 'MIDI B': Control Change [2] (123, 0)
Event received on 'MIDI A': Note On [0] (67, 93)
Event received on 'MIDI A': Note Off [0] (67, 0)

Releasing devices resources...
Devices resources released.
```