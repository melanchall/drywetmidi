---
uid: a_playback_overview
---

# Playback - Overview

[Playback](xref:Melanchall.DryWetMidi.Devices.Playback) class allows to play MIDI events via an [IOutputDevice](xref:Melanchall.DryWetMidi.Devices.IOutputDevice) (see [Output device](xref:a_dev_output) article) or without a device at all (see [Playback without device](#playback-without-device)). To get an instance of the `Playback` you can use its [constructors](xref:Melanchall.DryWetMidi.Devices.Playback#constructors) or `GetPlayback` extension methods in [PlaybackUtilities](xref:Melanchall.DryWetMidi.Devices.PlaybackUtilities).

Following example shows simple console app where specified MIDI file is played until end of the file reached or B note is about to be played. So in our example B note means to stop playback.

```csharp
using System;
using System.Linq;
using System.Threading;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Devices;

namespace SimplePlaybackApp
{
    class Program
    {
        private static Playback _playback;

        static void Main(string[] args)
        {
            var midiFile = MidiFile.Read("The Greatest Song Ever.mid");

            var outputDevice = OutputDevice.GetByName("Microsoft GS Wavetable Synth");

            _playback = midiFile.GetPlayback(outputDevice);
            _playback.NotesPlaybackStarted += OnNotesPlaybackStarted;
            _playback.Start();

            SpinWait.SpinUntil(() => !_playback.IsRunning);

            Console.WriteLine("Playback stopped or finished.");

            outputDevice.Dispose();
            _playback.Dispose();
        }

        private static void OnNotesPlaybackStarted(object sender, NotesEventArgs e)
        {
            if (e.Notes.Any(n => n.NoteName == Melanchall.DryWetMidi.MusicTheory.NoteName.B))
                _playback.Stop();
        }
    }
}
```

Please read [Tick generator](Tick-generator.md) article and [MidiClockSettings](xref:Melanchall.DryWetMidi.Devices.MidiClockSettings) class documentation to learn how you can adjust playback's internals.

## Playback without device

There are constructors of [Playback](xref:Melanchall.DryWetMidi.Devices.Playback) that don't accept [IOutputDevice](xref:Melanchall.DryWetMidi.Devices.IOutputDevice) as an argument. It can be useful, for example, for notes visualization without sound. [Playback](xref:Melanchall.DryWetMidi.Devices.Playback) provides events that will be fired with or without output device (see [Events](xref:Melanchall.DryWetMidi.Devices.Playback#events) section of the [Playback](xref:Melanchall.DryWetMidi.Devices.Playback) API page). Also all `GetPlayback` extensions methods have overloads without the `outputDevice` parameter.

Also if you don't specify output device and use [tick generator](Tick-generator.md) other than [HighPrecisionTickGenerator](xref:Melanchall.DryWetMidi.Devices.HighPrecisionTickGenerator), you can use `Playback` in cross-platform app like Unity game that is supposed to be built for different platforms.

## Blocking playback

If you call [Play](xref:Melanchall.DryWetMidi.Devices.Playback.Play) method of the `Playback`, the calling thread will be blocked until entire collection of MIDI events will be played. Note that execution of this method will be infinite if the [Loop](xref:Melanchall.DryWetMidi.Devices.Playback.Loop) property set to `true`.

There are also extension methods `Play` in [PlaybackUtilities](xref:Melanchall.DryWetMidi.Devices.PlaybackUtilities):

```csharp
using (var outputDevice = OutputDevice.GetByName("Output MIDI device"))
{
    MidiFile.Read("Some MIDI file.mid").Play(outputDevice);

    // ...
}
```

## Non-blocking playback

Is you call [Start](xref:Melanchall.DryWetMidi.Devices.Playback.Start) method of the [Playback](xref:Melanchall.DryWetMidi.Devices.Playback), execution of the calling thread will continue immediately after the method is called. To stop playback use [Stop](xref:Melanchall.DryWetMidi.Devices.Playback.Stop) method. Note that there is no any pausing method since it's useless. `Stop` leaves playback at the point where the method was called. To move to the start of the playback use [MoveToStart](xref:Melanchall.DryWetMidi.Devices.Playback.MoveToStart) method.

You should be very careful with this approach and `using` block. Example below shows the case where **part of MIDI data will not be played because of playback is disposed before the last MIDI event will be played**:

```csharp
using (var outputDevice = OutputDevice.GetByName("Output MIDI device"))
using (var playback = MidiFile.Read("Some MIDI file.mid").GetPlayback(outputDevice))
{
    playback.Start();

    // ...
}
```

With non-blocking approach you should call `Dispose` manually after you've finished work with playback object.