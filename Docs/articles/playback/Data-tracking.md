---
uid: a_playback_datatrack
---

# Playback data tracking

[Playback](xref:Melanchall.DryWetMidi.Devices.Playback) provides a way to track some MIDI data to correctly handle jumps in time and get properly sounding data. There are two main groups of data to track:

* [notes](#notes-tracking)
* [MIDI parameters](#midi-parameters-values-tracking) (pitch bend, program, control value)

## Notes tracking

Let's take a look at the following events sequence to play:

```image
...A.....B...
```

where `A` and `B` means _Note On_ and _Note Off_ events correspondingly. `.` means any other MIDI event.

`Playback` class has [TrackNotes](xref:Melanchall.DryWetMidi.Devices.Playback.TrackNotes) property. If its value is `true`, playback will internally construct notes based on input objects to play. So in our example one note will be constructed:

```image
...[A.....B]...
```

Now let's imagine a playback's time is at some point and we want to jump to a new one (with [MoveToTime](xref:Melanchall.DryWetMidi.Devices.Playback.MoveToTime(Melanchall.DryWetMidi.Interaction.ITimeSpan)) for example):

```image
.│.[A..║..B]...
 │    ↑
 └────┘
```

First vertical line shows the current time of playback. If we now jump to a new time that falls in the middle of second note (second vertical line), behavior of the playback will be different depending on `TrackNotes` property value. In case of the property value is `false` nothing special will happen, just the current time of the playback will be changed. But if we set `TrackNotes` to `true`, new _Note On_ event will be generated and played when we jump to the new time.

The same situation with opposite case:

```image
...[A..│..B].║.
      │    ↑
      └────┘
```

So we want here to jump from the middle of a note to the time after the note. As in previous example if `TrackNotes` is `false`, just the current time of the playback will be changed. But if in case of `true`, new _Note Off_ event will be generated and played when we jump to the new time.

So `TrackNotes = true` tells playback to track time jumps when the current time pointer of the playback either leaves a note or enters one to finish or start the note correspondingly.

Of course in cases like this:

```image
...[A..│..B].[..A..║.].B...
      │         ↑
      └─────────┘
```

playback will play both _Note Off_ event (since we're leaving the first note) and _Note On_ one (since we're entering the second note).

## MIDI parameters values tracking

Let's imagine we have the following events sequence to play:

```image
...P...Q...A...
```

where `P` and `Q` means _Program Change_ events, `A` means _Note On_ event and `.` means any other MIDI event.

And now we want to jump from the current time of a playback to a new time (with [MoveToTime](xref:Melanchall.DryWetMidi.Devices.Playback.MoveToTime(Melanchall.DryWetMidi.Interaction.ITimeSpan)) for example):

```image
...P.│.Q.║.A...
     │   ↑
     └───┘
```

So by the current time `P` event is played and the current program corresponds to `P`. If the playback just change the current time, `A` will be played using program `P` which is wrong since `Q` should be used.

To track a program `Playback` class has [TrackProgram](xref:Melanchall.DryWetMidi.Devices.Playback.TrackProgram) property. If it's set to `false`, nothing will happen except changing the current time. All following notes can sound incorrectly due to possibly skipped program changes.

But if we set `TrackProgram` to `true`, playback will play required _Program Change_ event immediately after time changed. So in our example `Q` will be played and then playback continues from new time.

Program tracking works in opposite direction too of course:

```image
...P.║.Q.│.A...
     ↑   │
     └───┘
```

We have program `Q` active at the current time. But when we jump to a new time (before `Q` but after `P`), `P` event will be played.

`Playback` can track at now three MIDI parameters:

* program (see [ProgramChangeEvent](xref:Melanchall.DryWetMidi.Core.ProgramChangeEvent))
* pitch bend (see [PitchBendEvent](xref:Melanchall.DryWetMidi.Core.PitchBendEvent))
* control value (see [ControlChangeEvent](xref:Melanchall.DryWetMidi.Core.ControlChangeEvent))

We have discussed tracking program above. But tracking the remaining two parameters is absolutely the same. To track pitch bend value there is [TrackPitchValue](xref:Melanchall.DryWetMidi.Devices.Playback.TrackPitchValue) property. To track control value there is [TrackControlValue](xref:Melanchall.DryWetMidi.Devices.Playback.TrackControlValue) property.

Of course all these parameters are tracked separately for each MIDI channel and in addition to this control value tracked separately for each control number.