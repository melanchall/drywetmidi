---
uid: a_playback_commonproblems
---

# Common problems

## Playback doesn't produce sound or events logs

Make sure an instance of [Playback](xref:Melanchall.DryWetMidi.Multimedia.Playback) class is held by class field, global variable or something else that can live longer than the method where you instantiate `Playback`. In this case

```csharp
private void StartPlayback()
{
    var playback = _midiFile.GetPlayback();
    playback.Start();
}
```

`playback` variable will "die" (will be ready to be collected by GC) after the program exits the `StartPlayback` method so playback won't work.

## SetActive can only be called from the main thread in Unity

Sometimes you want to handle [playback events](xref:Melanchall.DryWetMidi.Multimedia.Playback#events) or use [event](xref:Melanchall.DryWetMidi.Multimedia.Playback.EventCallback) or [note](xref:Melanchall.DryWetMidi.Multimedia.Playback.NoteCallback) callbacks. Your code can be executed on a separate thread in these cases. It can happen because `Playback`'s internals (like [tick generator](Tick-generator.md)'s tick handling) work on separate thread.

But UI related things like call of `SetActive` can be executed on UI thread only. You can use the solution from here: https://stackoverflow.com/a/56715254.

Related issues on GitHub:

* [trigger object with a note in Unity](https://github.com/melanchall/drywetmidi/issues/85)
