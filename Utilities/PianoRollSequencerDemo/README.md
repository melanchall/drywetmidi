# Melanchall.PianoRollSequencerDemo

Avalonia demo app that shows a simple DAW-like piano roll powered by `Melanchall.DryWetMidi` `9.0.0-prerelease8`.

## Features

- Draw notes on the piano roll (left drag on empty area).
- Move existing notes (left drag on a note).
- Remove notes (right click on a note).
- Edit notes while playback is running (changes are applied through `ObservableTimedObjectsCollection` used by `Playback`).

## Run

```bash
dotnet run --project Utilities/PianoRollSequencerDemo/Melanchall.PianoRollSequencerDemo.csproj
```

## Screenshots

### Animated demo

![Animated piano roll demo](Screenshots/pianoroll-demo.gif)

### Overview

![Piano roll overview](Screenshots/pianoroll-overview.png)

### Editing during playback

![Editing during playback](Screenshots/pianoroll-editing-during-playback.png)
