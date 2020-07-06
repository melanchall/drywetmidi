---
uid: a_playback_commonproblems
---

# Common problems

## Playback doesn't produce sound or events logs

Make sure an instance of [Playback](xref:Melanchall.DryWetMidi.Devices.Playback) class is holded by class field, global variable or something else that can live longer that method where you instantiate `Playback`. In this case

```csharp
private void StartPlayback()
{
    var playback = _midiFile.GetPlayback();
    playback.Start();
}
```

`playback` variable will "die" (will be ready to be collected by GC) after program exits `StartPlayback` method so playback won't work.

## Unity hangs or crash on entering Play mode second time

By default `Playback` uses [HighPrecisionTickGenerator](xref:Melanchall.DryWetMidi.Devices.HighPrecisionTickGenerator) tick generator (see [Tick generator](Tick-generator.md) article to learn more). `HighPrecisionTickGenerator` uses WinAPI methods by P/Invoke wich causes troubles in Unity environment. You may notice that Unity hangs forever if you hit Play button second time. Unfortunately it seems the [problem](https://issuetracker.unity3d.com/issues/editor-freezes-when-updating-a-nativearray-on-the-net-4-dot-x-scripting-runtime-and-entering-play-mode-a-second-time) won't be fixed. Answer of Unity tech support:

> Unfortunately, that is correct = we will not be able to fix this in the near term because it probably requires rewriting of internal threading functionality, which might introduce new issues. The main case has been tagged for a revisit internally, but it will probably be months until the case is re-valuated again.

Solution is to use either [RegularPrecisionTickGenerator](xref:Melanchall.DryWetMidi.Devices.RegularPrecisionTickGenerator) or a [custom](Tick-generator.md#custom-tick-generator) one. Related issues on GitHub:

* [Playback produces unexpected results in Unity](https://github.com/melanchall/drywetmidi/issues/31)
* [Visualization of notes](https://github.com/melanchall/drywetmidi/issues/79)

## SetActive can only be called from the main thread in Unity

Sometimes you want to handle [playback events](xref:Melanchall.DryWetMidi.Devices.Playback#events) or use [event](xref:Melanchall.DryWetMidi.Devices.Playback.EventCallback) or [note](xref:Melanchall.DryWetMidi.Devices.Playback.NoteCallback) callbacks. Your code can be executed on separate thread in these cases. It can happen because of `Playback`'s internals (like [tick generator](Tick-generator.md)'s tick handling) work on separate thread.

But UI related things like call of `SetActive` can be executed on UI thread only. You can use the solution from here: https://stackoverflow.com/a/56715254.

Related issues on GitHub:

* [trigger object with a note in Unity](https://github.com/melanchall/drywetmidi/issues/85)