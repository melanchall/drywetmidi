---
uid: a_develop_unity
---

# Using in Unity

This article describes how to integrate DryWetMIDI in a Unity project. You have two main ways:

* import the [DryWetMIDI asset](https://assetstore.unity.com/packages/tools/audio/drywetmidi-222171) (or [DryWetMIDI.Nativeless](https://assetstore.unity.com/packages/tools/audio/drywetmidi-nativeless-228998) one) from the Unity Asset Store;
* install the library manually.

There are also ways to import a NuGet package via 3rd-party tools (for example, [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity)).

## Unity asset

It's the simplest way. Just use built-in ways to import the official [DryWetMIDI asset](https://assetstore.unity.com/packages/tools/audio/drywetmidi-222171) (or [DryWetMIDI.Nativeless](https://assetstore.unity.com/packages/tools/audio/drywetmidi-nativeless-228998) one) into your Unity project from the Asset Store.

## Manual installation

Instruction below shows how to integrate the full version of the DryWetMIDI into your Unity project manually. If you want to use the [nativeless version](xref:a_develop_nativeless), just take an archive with _-nativeless_ suffix on the second step and skip the third one.

1. Create _Melanchall_ folder in project's _Assets_ folder and _DryWetMIDI_ subfolder within the _Melanchall_ one.
2. Download the library main binary:
    * for _master_ branch take _DryWetMIDI.<release_number>-bin-netstandard20.zip_ archive from [Releases](https://github.com/melanchall/drywetmidi/releases) (_<release_number>_ is the number of the library release you want to take binaries of);
    * for _develop_ branch go to https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=6&branchName=develop and download _DryWetMIDI.<release_number>-bin-netstandard20.zip_ from `Artifacts` → `Binaries`.
3. Download native binaries required to work with MIDI devices and default playback:
    * for _master_ branch take _DryWetMIDI.<release_number>-bin-native.zip_ archive from [Releases](https://github.com/melanchall/drywetmidi/releases);
    * for _develop_ branch go to https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=6&branchName=develop and download _DryWetMIDI.<release_number>-bin-native.zip_ from `Artifacts` → `Binaries`.

    (_master_ branch contains code that the library releases built on. _develop_ one is for current development so if you need the latest API, use this branch)
4. Extract archives into project's _Assets_ → _Melanchall_ → _DryWetMIDI_ folder.

## Example

Now you can use DryWetMIDI API in your Unity scripts.

Following sample script (included in demo scene within the [full DryWetMIDI package](https://assetstore.unity.com/packages/tools/audio/drywetmidi-222171)) will create a MIDI file containing all possible notes with length of `1/8` and will play the file via `Microsoft GS Wavetable Synth` output endpoint:

```csharp
using System;
using System.Linq;
using System.Text;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Standards;
using UnityEngine;

public class DemoScript : MonoBehaviour
{
    private const string OutputEndpointName = "Microsoft GS Wavetable Synth";

    private OutputEndpoint _outputEndpoint;
    private Playback _playback;

    private void Start()
    {
        InitializeOutputEndpoint();
        var midiFile = CreateTestFile();
        InitializeFilePlayback(midiFile);
        StartPlayback();
    }

    private void OnApplicationQuit()
    {
        Debug.Log("Releasing playback and endpoint...");

        if (_playback != null)
        {
            _playback.NotesPlaybackStarted -= OnNotesPlaybackStarted;
            _playback.NotesPlaybackFinished -= OnNotesPlaybackFinished;
            _playback.Dispose();
        }

        if (_outputEndpoint != null)
            _outputEndpoint.Dispose();

        Debug.Log("Playback and endpoint released.");
    }

    private void InitializeOutputEndpoint()
    {
        Debug.Log($"Initializing output endpoint [{OutputEndpointName}]...");

        var allOutputEndpoints = OutputEndpoint.GetAll();
        if (!allOutputEndpoints.Any(d => d.Name == OutputEndpointName))
        {
            var allEndpointsList = string.Join(Environment.NewLine, allOutputEndpoints.Select(d => $"  {d.Name}"));
            Debug.Log($"There is no [{OutputEndpointName}] endpoint presented in the system. Here the list of all endpoints:{Environment.NewLine}{allEndpointsList}");
            return;
        }

        _outputEndpoint = OutputEndpoint.GetByName(OutputEndpointName);
        Debug.Log($"Output endpoint [{OutputEndpointName}] initialized.");
    }

    private MidiFile CreateTestFile()
    {
        Debug.Log("Creating test MIDI file...");

        var patternBuilder = new PatternBuilder()
            .SetNoteLength(MusicalTimeSpan.Eighth)
            .SetVelocity(SevenBitNumber.MaxValue)
            .ProgramChange(GeneralMidiProgram.Harpsichord);

        foreach (var noteNumber in SevenBitNumber.Values)
        {
            patternBuilder.Note(Melanchall.DryWetMidi.MusicTheory.Note.Get(noteNumber));
        }

        var midiFile = patternBuilder.Build().ToFile(TempoMap.Default);
        Debug.Log("Test MIDI file created.");

        return midiFile;
    }

    private void InitializeFilePlayback(MidiFile midiFile)
    {
        Debug.Log("Initializing playback...");

        _playback = midiFile.GetPlayback(_outputEndpoint);
        _playback.Loop = true;
        _playback.NotesPlaybackStarted += OnNotesPlaybackStarted;
        _playback.NotesPlaybackFinished += OnNotesPlaybackFinished;
       
        Debug.Log("Playback initialized.");
    }

    private void StartPlayback()
    {
        Debug.Log("Starting playback...");
        _playback.Start();
    }

    private void OnNotesPlaybackFinished(object sender, NotesEventArgs e)
    {
        LogNotes("Notes finished:", e);
    }

    private void OnNotesPlaybackStarted(object sender, NotesEventArgs e)
    {
        LogNotes("Notes started:", e);
    }

    private void LogNotes(string title, NotesEventArgs e)
    {
        var message = new StringBuilder()
            .AppendLine(title)
            .AppendLine(string.Join(Environment.NewLine, e.Notes.Select(n => $"  {n}")))
            .ToString();
        Debug.Log(message.Trim());
    }
}
```

> [!WARNING]
> Pay attention to `OnApplicationQuit` method. You should always take care about disposing of MIDI endpoints. Without it all resources taken by an endpoint will live until GC collects them. In the case of Unity, it means Unity may need to be reopened to be able to use the same endpoints again (for example, on Windows).