---
uid: a_playback_overview
---

# Playback – Overview

[Playback](xref:Melanchall.DryWetMidi.Multimedia.Playback) class allows to play MIDI events via an [IOutputDevice](xref:Melanchall.DryWetMidi.Multimedia.IOutputDevice) (see [Output device](xref:a_dev_output) article) or without a device at all (see [Playback without device](#playback-without-device)). To get an instance of the `Playback` you can use its [constructors](xref:Melanchall.DryWetMidi.Multimedia.Playback#constructors) or `GetPlayback` extension methods in [PlaybackUtilities](xref:Melanchall.DryWetMidi.Multimedia.PlaybackUtilities).

Following example shows a simple console app where the specified MIDI file is played until the end of the file reached or `B` note is about to be played. So in our example `B` note means to stop playback.

```csharp
using System;
using System.Linq;
using System.Threading;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;

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

Please read [Tick generator](Tick-generator.md) article and [PlaybackSettings](xref:Melanchall.DryWetMidi.Multimedia.PlaybackSettings) class documentation to learn how you can adjust playback's internals.

Playback supports on-the-fly changes of the data being played. You can find detailed information on how to use this feature in the [Dynamic changes](xref:a_playback_dynamic) article.

If you call the [Start](xref:Melanchall.DryWetMidi.Multimedia.Playback.Start) method of the [Playback](xref:Melanchall.DryWetMidi.Multimedia.Playback), execution of the calling thread will continue immediately after the method is called. To stop playback use the [Stop](xref:Melanchall.DryWetMidi.Multimedia.Playback.Stop) method. Note that there is no any pausing method since it's useless. `Stop` leaves playback at the point where the method was called. To move to the start of the playback use the [MoveToStart](xref:Melanchall.DryWetMidi.Multimedia.Playback.MoveToStart) method.

> [!IMPORTANT]
> You should be very careful with `using` block. Example below shows the case where part of MIDI data **will not be played** because of playback is disposed before the last MIDI event will be played:
> 
> ```csharp
> using (var outputDevice = OutputDevice.GetByName("Output MIDI device"))
> using (var playback = MidiFile.Read("Some MIDI file.mid").GetPlayback(outputDevice))
> {
>     playback.Start();
> 
>     // ...
> }
> ```
> 
> You must call `Dispose` manually only after you've finished work with the playback object.

## Playback without device

There are constructors of [Playback](xref:Melanchall.DryWetMidi.Multimedia.Playback) that don't accept [IOutputDevice](xref:Melanchall.DryWetMidi.Multimedia.IOutputDevice) as an argument. It can be useful, for example, for notes visualization without sound. [Playback](xref:Melanchall.DryWetMidi.Multimedia.Playback) provides events that will be fired with or without an output device (see [Events](xref:Melanchall.DryWetMidi.Multimedia.Playback#events) section of the [Playback](xref:Melanchall.DryWetMidi.Multimedia.Playback) API page). Also all `GetPlayback` extensions methods have overloads without the `outputDevice` parameter.

Also if you don't specify an output device and use a [tick generator](Tick-generator.md) other than [HighPrecisionTickGenerator](xref:Melanchall.DryWetMidi.Multimedia.HighPrecisionTickGenerator), you can use `Playback` in a cross-platform app like Unity game that is supposed to be built for different platforms (you can find currently supported OS in the [Supported OS](xref:a_develop_supported_os) article).