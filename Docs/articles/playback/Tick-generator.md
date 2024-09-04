﻿---
uid: a_playback_tickgen
---

# Tick generator

Playback uses a timer under the hood. In DryWetMIDI this timer is called **tick generator**. On every tick of the timer playback looks at what objects should be played by the current time, plays them and advances position within the objects list waiting for the next tick.

To make playback smooth and correct, the precision of the timer should be ~1ms. So tick will be generated every one millisecond. By default, DryWetMIDI uses [HighPrecisionTickGenerator](xref:Melanchall.DryWetMidi.Multimedia.HighPrecisionTickGenerator) which is the best option in terms of CPU usage, memory usage and precision.

> [!IMPORTANT]
> `HighPrecisionTickGenerator` is supported for Windows and macOS only at the moment.

You can also use [RegularPrecisionTickGenerator](xref:Melanchall.DryWetMidi.Multimedia.RegularPrecisionTickGenerator) which uses standard [Timer](xref:System.Timers.Timer) and thus provides precision about 16ms on Windows. But this tick generator is cross-platform.

Tick generator can be specified via `playbackSettings` parameter of [Playback](xref:Melanchall.DryWetMidi.Multimedia.Playback)'s constructors or `GetPlayback` extension methods within [PlaybackUtilities](xref:Melanchall.DryWetMidi.Multimedia.PlaybackUtilities):

```csharp
var playback = midiFile.GetPlayback(new PlaybackSettings
{
    ClockSettings = new MidiClockSettings
    {
        CreateTickGeneratorCallback = () => new RegularPrecisionTickGenerator()
    }
});
```

## Custom tick generator

All built-in tick generators extend the abstract [TickGenerator](xref:Melanchall.DryWetMidi.Multimedia.TickGenerator) class so you can create your own and use it for [Playback](xref:Melanchall.DryWetMidi.Multimedia.Playback) and [PlaybackCurrentTimeWatcher](xref:Melanchall.DryWetMidi.Multimedia.PlaybackCurrentTimeWatcher).

As an example we create a simple loop tick generator working in a separate thread. The code is:

```csharp
private sealed class ThreadTickGenerator : TickGenerator
{
    private Thread _thread;
    private bool _isRunning;
    private bool _disposed;

    protected override void Start(TimeSpan interval)
    {
        if (_thread != null)
            return;

        _thread = new Thread(() =>
        {
            var stopwatch = new Stopwatch();
            var lastMs = 0L;

            stopwatch.Start();
            _isRunning = true;

            while (_isRunning)
            {
                var elapsedMs = stopwatch.ElapsedMilliseconds;
                if (elapsedMs - lastMs >= interval.TotalMilliseconds)
                {
                    GenerateTick();
                    lastMs = elapsedMs;
                }
            }
        });

        _thread.Start();
    }

    protected override void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _isRunning = false;
        }

        _disposed = true;
    }
}
```

And then use it:

```csharp
var playback = midiFile.GetPlayback(new PlaybackSettings
{
    ClockSettings = new MidiClockSettings
    {
        CreateTickGeneratorCallback = () => new ThreadTickGenerator()
    }
});
```

Of course this tick generator will use a lot of CPU due to infinite loop but it's for demo purposes only.

## Manual ticking

Also you can tick playback's internal clock manually without a tick generator via the [TickClock](xref:Melanchall.DryWetMidi.Multimedia.Playback.TickClock) method of [Playback](xref:Melanchall.DryWetMidi.Multimedia.Playback). For example, you can use manual ticking within every frame update in Unity.

To use only manual ticking you should return `null` in [CreateTickGeneratorCallback](xref:Melanchall.DryWetMidi.Multimedia.MidiClockSettings.CreateTickGeneratorCallback):

```csharp
var playback = midiFile.GetPlayback(new PlaybackSettings
{
    ClockSettings = new MidiClockSettings
    {
        CreateTickGeneratorCallback = () => null
    }
});
```

and then call

```csharp
playback.TickClock();
```

when needed.

You also can use manual ticking in conjunction with a tick generator.