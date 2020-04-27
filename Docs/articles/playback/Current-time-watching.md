---
uid: a_playback_curtime
---

# Current time watching

To watch current time of a playback you can create a timer and call [GetCurrentTime](xref:Melanchall.DryWetMidi.Devices.Playback.GetCurrentTime(Melanchall.DryWetMidi.Interaction.TimeSpanType)) method on each timer's tick. To simplify this task (especially if you're running multiple playbacks simultaneously) DryWetMIDI provides [PlaybackCurrentTimeWatcher](xref:Melanchall.DryWetMidi.Devices.PlaybackCurrentTimeWatcher) class. This class is singleton in order to prevent too many high resolution tick generators are created (which is not good since it can affect whole system performance). Please read [Tick generator](Tick-generator.md) article to learn how you can ajust internals of the `PlaybackCurrentTimeWatcher`.

Small example:

```csharp
PlaybackCurrentTimeWatcher.Instance.AddPlayback(playback, TimeSpanType.Midi);
PlaybackCurrentTimeWatcher.Instance.CurrentTimeChanged += OnCurrentTimeChanged;
PlaybackCurrentTimeWatcher.Instance.Start();

// ...

private static void OnCurrentTimeChanged(object sender, PlaybackCurrentTimeChangedEventArgs e)
{
    foreach (var playbackTime in e.Times)
    {
        var playback = playbackTime.Playback;
        var time = (MidiTimeSpan)playbackTime.Time;

        Console.WriteLine($"Current time is {time}.");
    }
}
```

In this example we watch current time of playback and request to report time in MIDI format which is ticks (and thus we cast received time to [MidiTimeSpan](xref:Melanchall.DryWetMidi.Interaction.MidiTimeSpan)). You can set any desired time format and cast to corresponding implementation of the [ITimeSpan](xref:Melanchall.DryWetMidi.Interaction.ITimeSpan).

You can add multiple different playbacks to watch their current times. When you don't want to watch playback anymore remove it from the watcher:

```csharp
PlaybackCurrentTimeWatcher.Instance.RemovePlayback(playback);
```

By default polling interval of watcher is `100` ms, but you can alter it:

```csharp
PlaybackCurrentTimeWatcher.Instance.PollingInterval = TimeSpan.FromMilliseconds(50);
```

Please don't set too small intervals. Polling interval defines how often [CurrentTimeChanged](xref:Melanchall.DryWetMidi.Devices.PlaybackCurrentTimeWatcher.CurrentTimeChanged) event will be fired. If you want to pause firing the event, call [Stop](xref:Melanchall.DryWetMidi.Devices.PlaybackCurrentTimeWatcher.Stop) method.

When your application is about to close, dispose watcher to kill the internal timer:

```csharp
PlaybackCurrentTimeWatcher.Instance.Dispose();
```