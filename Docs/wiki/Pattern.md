DryWetMIDI provides a way to create a MIDI file in more "musical" manner. The key class here is the `PatternBuilder` which allows to build a musical composition. `PatternBuilder` provides a fluent interface to program the music. For example, you can insert a note like this:

```csharp
using Melanchall.DryWetMidi.Composing;

var patternBuilder = new PatternBuilder()

    // Insert the A4 note with length of 5/17 and velocity of 68
    .Note(Octave.Get(4).A,
          new MusicalTimeSpan(5, 17),
          (SevenBitNumber)68)

    // Insert the G#3 note with default length and velocity
    .Note(Octave.Get(3).GSharp);
```

In the example above the last `Note` method inserts a note with default length and velocity that can be set via corresponding methods at any moment:

```csharp
var patternBuilder = new PatternBuilder()
    .SetNoteLength(MusicalTimeSpan.Half)
    .SetVelocity((SevenBitNumber)50)

    // All the following notes will have half length and velocity of 50
    .Note(Octave.Get(4).A)
    .Note(Note.Get(NoteName.B, 1))
    .Note(-Interval.Two)

    .SetNoteLength(MusicalTimeSpan.Quarter)

    // All the following notes will have quarter length
    .Note(Octave.Get(2).A);
```

`.Note(-Interval.Two)` inserts a note that has the pitch of two half steps down of a root note. The default root note is C4 but you can set it to another one:

```csharp
var patternBuilder = new PatternBuilder()
    .SetRootNote(Note.Get(NoteName.A, 2))

    // All the following intervals will be calculated relative to the A2
    .Note(Interval.One)          // A2 + 1  (A#2)
    .Note(Interval.Four)         // A2 + 4  (C#3)
    .Note(-Interval.One)         // A2 - 1  (G#2)
    .Note(-Interval.Twelve * 2)  // A2 - 24 (A0);
```

Also you can set default octave:

```csharp
var patternBuilder = new PatternBuilder()
    .SetOctave(Octave.Get(4))
     
    // All the following notes will be of 5th octave
    .Note(NoteName.CSharp)
    .Note(NoteName.G);
```

All the `Note`, `Octave` and `Inerval` classes are placed in `Melanchall.DryWetMidi.MusicTheory` namespace. Read [Music theory](Music-theory.md) article to learn more.

Of course you can insert chords. Chord is just a group of notes starting at the same time, so all the techniques described above work for chords too:

```csharp
var patternBuilder = new PatternBuilder()
    .SetNoteLength(MusicalTimeSpan.Half)
    .SetVelocity((SevenBitNumber)50)

    // Insert a chord of A5 and A6
    .Chord(new[] { Note.Get(NoteName.A, 5),
                   Note.Get(NoteName.A, 6) })

    // Insert a chord of E5, F#5 and A#5
    .Chord(new[] { -Interval.Two,
                   Interval.Four },
           Octave.Get(5).FSharp);
```

The last `Chord` method in the code above also takes a root note for it to know what notes should be produced by the passed intervals. The root note also will be a part of the chord.

Both `Note` and `Chord` methods insert an object at the current time that can be changed:

```csharp
var patternBuilder = new PatternBuilder()

    // Add a pause of 1 minute and 30 seconds
    .StepForward(new MetricTimeSpan(0, 1, 30))

    // Now a note will be inserted at time of 1 minute and 30 seconds
    .Note(NoteName.G)

    // Jump to time of 2 bars
    .MoveToTime(new BarBeatTicksTimeSpan(2, 0))

    // Step back by quarter note length
    .StepBack(MusicalTimeSpan.Quarter)

    // Now a note will be inserted at time of 2 bars minus
    // quarter note length
    .Note(NoteName.A)
    .Note(NoteName.CSharp)

    // Step back by default step
    StepBack();
```

Of course you can specify an instrument that should be used to play notes. There is method `ProgramChange` for that:

```csharp
var patternBuilder = new PatternBuilder()

    // You can set program with just a program number...
    .ProgramChange((SevenBitNumber)100)
    .Note(NoteName.A)

    // ... or via General MIDI constant...
    .ProgramChange(GeneralMidiProgram.Accordion)

    // ... or via General MIDI Level 2 constant
    .ProgramChange(GeneralMidi2Program.AcousticGrandPianoWide);
```

If you call `StepForward` or `StepBack` without an argument the default step size will be used. It is quarter note length by default but it can be changed:

```csharp
var patternBuilder = new PatternBuilder()

    // Set default step to 2 minutes
    .SetStep(new MetricTimeSpan(0, 2, 0))

    // Step forward by two minutes
    .StepForward()

    // Step back by two minutes
    .StepBack();
```

Every call of `Note` and `Chord` moves the current time by the length of inserted object. You can return to the previous current time using `MoveToPreviousTime` method.

Another feature of the pattern building is anchoring of the current time:

```csharp
var patternBuilder = new PatternBuilder()

    // Add a pause of 1 minute and 30 seconds
    .StepForward(new MetricTimeSpan(0, 1, 30))

    // Save the current time
    .Anchor()

    // Save the current time attaching some key
    .Anchor("Label 1");
```

Key of an anchor is an arbitrary `object`. But anchors are useless without ability to jump to them. There are following methods for moving to anchors:

* `MoveToFirstAnchor` that changes the current time to the time of first added anchor;
* `MoveToLastAnchor` that changes the current time to the time of last added anchor;
* `MoveToNthAnchor` that changes the current time to the time of n-th anchor.

If you use overloads without a key all anchors (keyed and unkeyed) will be taken into consideration to jump to specific one.

Once you've finished building a composition call the `Build` method which returns an instance of the `Pattern` class. Pattern contains all the actions made with the builder and can be exported to a file or track chunk:

```csharp
Pattern pattern = patternBuilder.Build();

MidiFile midiFile = pattern.ToFile(TempoMap.Default);
```

You can also insert a pattern working with `PatternBuilder`:

```csharp
var patternBuilder = new PatternBuilder()
    .Pattern(pattern1)
    .Pattern(pattern2);
```