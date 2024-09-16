---
uid: a_composing_pattern
---

# Pattern

DryWetMIDI provides a way to create a MIDI file in a more "musical" manner. The key class here is the [PatternBuilder](xref:Melanchall.DryWetMidi.Composing.PatternBuilder) which allows to build a musical composition. `PatternBuilder` provides a fluent interface to program the music. For example, you can insert a note like this:

```csharp
using Melanchall.DryWetMidi.Composing;

var patternBuilder = new PatternBuilder()

    // Insert the A4 note with length of 5/17 and velocity of 68
    .Note(Octave.Get(4).A, new MusicalTimeSpan(5, 17), (SevenBitNumber)68)

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

Please take a look at the entire API provided by [PatternBuilder](xref:Melanchall.DryWetMidi.Composing.PatternBuilder).

Following example shows how to create first four bars of Beethoven's 'Moonlight Sonata':

```csharp    
// Define a chord for bass part which is just an octave
var bassChord = new[] { Interval.Twelve };
    
// Build the composition
var pattern = new PatternBuilder()
     
    // The length of all main theme's notes within four first bars is
    // triplet eight so set it which will free us from necessity to specify
    // the length of each note explicitly

    .SetNoteLength(MusicalTimeSpan.Eighth.Triplet())
    
    // Anchor current time (start of the pattern) to jump to it
    // when we'll start to program bass part

    .Anchor()
    
    // We will add notes relative to G#3.
    // Instead of Octave.Get(3).GSharp it is possible to use Note.Get(NoteName.GSharp, 3)

    .SetRootNote(Octave.Get(3).GSharp)
    
    // Add first three notes and repeat them seven times which will
    // give us two bars of the main theme

                           // G#3
    .Note(Interval.Zero)   // +0  (G#3)
    .Note(Interval.Five)   // +5  (C#4)
    .Note(Interval.Eight)  // +8  (E4)

    .Repeat(3, 7)          // repeat three previous notes seven times
    
    // Add notes of the next two bars

                           // G#3
    .Note(Interval.One)    // +1  (A3)
    .Note(Interval.Five)   // +5  (C#4)
    .Note(Interval.Eight)  // +8  (E4)

    .Repeat(3, 1)          // repeat three previous notes

    .Note(Interval.One)    // +1  (A3)
    .Note(Interval.Six)    // +6  (D4)
    .Note(Interval.Ten)    // +10 (F#4)

    .Repeat(3, 1)          // repeat three previous notes
                           // reaching the end of third bar

    .Note(Interval.Zero)   // +0  (G#3)
    .Note(Interval.Four)   // +4  (C4)
    .Note(Interval.Ten)    // +10 (F#4)
    .Note(Interval.Zero)   // +0  (G#3)
    .Note(Interval.Five)   // +5  (C#4)
    .Note(Interval.Eight)  // +8  (E4)
    .Note(Interval.Zero)   // +0  (G#3)
    .Note(Interval.Five)   // +5  (C#4)
    .Note(Interval.Seven) // +7  (D#4)
    .Note(-Interval.Two)   // -2  (F#3)
    .Note(Interval.Four)   // +4  (C4)
    .Note(Interval.Seven)  // +7  (D#4)
    
    // Now we will program bass part. To start adding notes from the
    // beginning of the pattern we need to move to the anchor we set
    // above

    .MoveToFirstAnchor()
    
    // First two chords have whole length

    .SetNoteLength(MusicalTimeSpan.Whole)
    
                                            // insert a chord relative to
    .Chord(bassChord, Octave.Get(2).CSharp) // C#2 (C#2, C#3)
    .Chord(bassChord, Octave.Get(1).B)      // B1  (B1, B2)
    
    // Remaining four chords has half length

    .SetNoteLength(MusicalTimeSpan.Half)
    
    .Chord(bassChord, Octave.Get(1).A)      // A1  (A1, A2)
    .Chord(bassChord, Octave.Get(1).FSharp) // F#1 (F#1, F#2)
    .Chord(bassChord, Octave.Get(1).GSharp) // G#1 (G#1, G#2)

    .Repeat()                               // repeat the previous chord
    
    // Build a pattern that can be then saved to a MIDI file

    .Build();
```

[Build](xref:Melanchall.DryWetMidi.Composing.PatternBuilder.Build) method returns an instance of [Pattern](xref:Melanchall.DryWetMidi.Composing.Pattern). Pattern can be transformed or altered by methods in [PatternUtilities](xref:Melanchall.DryWetMidi.Composing.PatternUtilities).

Pattern can be then saved to [MidiFile](xref:Melanchall.DryWetMidi.Core.MidiFile) (via [ToFile](xref:Melanchall.DryWetMidi.Composing.Pattern.ToFile*) method) or [TrackChunk](xref:Melanchall.DryWetMidi.Core.TrackChunk) (via [ToTrackChunk](xref:Melanchall.DryWetMidi.Composing.Pattern.ToTrackChunk*) method). You need to provide a [tempo map](xref:Melanchall.DryWetMidi.Interaction.TempoMap). Also you can optionally specify the channel that should be set to events. The default channel is `0`.

Also please see the [Extension methods](xref:Melanchall.DryWetMidi.Composing.Pattern#extensionmethods) section of the [Pattern](xref:Melanchall.DryWetMidi.Composing.Pattern) API.

## Piano roll

There is a way to create simple patterns easily – via the [PianoRoll](xref:Melanchall.DryWetMidi.Composing.PatternBuilder.PianoRoll*) method. To quickly dive into the method, just take a look at this example:

```csharp
var midiFile = new PatternBuilder()
    .SetNoteLength(MusicalTimeSpan.Eighth)
    .PianoRoll(@"
        F#2   ||||||||
        D2    --|---|-
        C2    |---|---")
    .Repeat(9)
    .Build()
    .ToFile(TempoMap.Default, (FourBitNumber)9);
midiFile.Write("pianoroll-simple.mid", true);
```

Each line starts with a note. Think about the line as a piano roll lane in your favorite DAW. So notes on the first line will be _F#2_, on the second line – _D2_ and on the third one – _C2_. Each character then **except spaces** means one cell. The length of a cell is determined by the [SetNoteLength](xref:Melanchall.DryWetMidi.Composing.PatternBuilder.SetNoteLength*) method.

`'|'` symbol means a single-cell note, i.e. the note's length is equal to a cell's length. So each note in the example will be an 8th one. By the way, you can alter this symbol with the [SingleCellNoteSymbol](xref:Melanchall.DryWetMidi.Composing.PianoRollSettings.SingleCellNoteSymbol) property of the [PianoRollSettings](xref:Melanchall.DryWetMidi.Composing.PianoRollSettings) passed to the [PianoRoll](xref:Melanchall.DryWetMidi.Composing.PatternBuilder.PianoRoll*) method.

Hyphen (`'-'`) means nothing except a step of a cell's length. We will call it as **fill symbol**.

> [!IMPORTANT]
> Spaces will be cut from the piano roll string before processing. So it's required to use a fill symbol to specify an empty space (rest) to get correct results. For example, this pattern:
> ```text
> F2   ||||
> D2     |
> C2   |
> ```
> will be transformed by the piano roll processing engine to these strings:
> ```text
> F2||||
> D2|
> C2|
> ```
> which is probably not what you want.

Be aware that fill symbol must not be the same as those used for notes and must not be a part of a collection of custom actions symbols (see [Customization](#customization) section further).

The example above demonstrates how to create a simple drum rhythm – standard 8th note groove – using General MIDI drum map. You can listen to the file produced – [pianoroll-simple.mid](files/pianoroll-simple.mid). By the way, you can use notes numbers instead of letters and octaves (and don't forget about string interpolation along with meaningful variables names):

```csharp
var bassDrum = 36;
var snareDrum = 38;
var closedHiHat = 42;

var midiFile = new PatternBuilder()
    .SetNoteLength(MusicalTimeSpan.Eighth)
    .PianoRoll(@$"
        {closedHiHat}   ||||||||
          {snareDrum}   --|---|-
           {bassDrum}   |---|---")
    .Repeat(9)
    .Build()
    .ToFile(TempoMap.Default, (FourBitNumber)9);
midiFile.Write("pianoroll-simple.mid", true);
```

But let's take a more interesting example which we looked at above – 'Moonlight Sonata'. The same first four bars of it can be constructed via piano roll like this:

```csharp
var midiFile = new PatternBuilder()
    .SetNoteLength(MusicalTimeSpan.Eighth.Triplet())
    .PianoRoll(@"
        F#4   ∙∙∙∙∙∙∙∙∙∙∙∙   ∙∙∙∙∙∙∙∙∙∙∙∙   ∙∙∙∙∙∙∙∙|∙∙|   ∙∙|∙∙∙∙∙∙∙∙∙
        E4    ∙∙|∙∙|∙∙|∙∙|   ∙∙|∙∙|∙∙|∙∙|   ∙∙|∙∙|∙∙∙∙∙∙   ∙∙∙∙∙|∙∙∙∙∙∙
        D#4   ∙∙∙∙∙∙∙∙∙∙∙∙   ∙∙∙∙∙∙∙∙∙∙∙∙   ∙∙∙∙∙∙∙∙∙∙∙∙   ∙∙∙∙∙∙∙∙|∙∙|
        C#4   ∙|∙∙|∙∙|∙∙|∙   ∙|∙∙|∙∙|∙∙|∙   ∙|∙∙|∙∙∙∙∙∙∙   ∙∙∙∙|∙∙|∙∙∙∙
        C4    ∙∙∙∙∙∙∙∙∙∙∙∙   ∙∙∙∙∙∙∙∙∙∙∙∙   ∙∙∙∙∙∙∙∙∙∙∙∙   ∙|∙∙∙∙∙∙∙∙|∙
        D4    ∙∙∙∙∙∙∙∙∙∙∙∙   ∙∙∙∙∙∙∙∙∙∙∙∙   ∙∙∙∙∙∙∙|∙∙|∙   ∙∙∙∙∙∙∙∙∙∙∙∙
        A3    ∙∙∙∙∙∙∙∙∙∙∙∙   ∙∙∙∙∙∙∙∙∙∙∙∙   |∙∙|∙∙|∙∙|∙∙   ∙∙∙∙∙∙∙∙∙∙∙∙
        G#3   |∙∙|∙∙|∙∙|∙∙   |∙∙|∙∙|∙∙|∙∙   ∙∙∙∙∙∙∙∙∙∙∙∙   |∙∙|∙∙|∙∙∙∙∙
        F#3   ∙∙∙∙∙∙∙∙∙∙∙∙   ∙∙∙∙∙∙∙∙∙∙∙∙   ∙∙∙∙∙∙∙∙∙∙∙∙   ∙∙∙∙∙∙∙∙∙|∙∙
                                                                       
        C#3   [==========]   ∙∙∙∙∙∙∙∙∙∙∙∙   ∙∙∙∙∙∙∙∙∙∙∙∙   ∙∙∙∙∙∙∙∙∙∙∙∙
        B2    ∙∙∙∙∙∙∙∙∙∙∙∙   [==========]   ∙∙∙∙∙∙∙∙∙∙∙∙   ∙∙∙∙∙∙∙∙∙∙∙∙
        A2    ∙∙∙∙∙∙∙∙∙∙∙∙   ∙∙∙∙∙∙∙∙∙∙∙∙   [====]∙∙∙∙∙∙   ∙∙∙∙∙∙∙∙∙∙∙∙
        G#2   ∙∙∙∙∙∙∙∙∙∙∙∙   ∙∙∙∙∙∙∙∙∙∙∙∙   ∙∙∙∙∙∙∙∙∙∙∙∙   [====][====]
        F#2   ∙∙∙∙∙∙∙∙∙∙∙∙   ∙∙∙∙∙∙∙∙∙∙∙∙   ∙∙∙∙∙∙[====]   ∙∙∙∙∙∙∙∙∙∙∙∙
        C#2   [==========]   ∙∙∙∙∙∙∙∙∙∙∙∙   ∙∙∙∙∙∙∙∙∙∙∙∙   ∙∙∙∙∙∙∙∙∙∙∙∙
        B1    ∙∙∙∙∙∙∙∙∙∙∙∙   [==========]   ∙∙∙∙∙∙∙∙∙∙∙∙   ∙∙∙∙∙∙∙∙∙∙∙∙
        A1    ∙∙∙∙∙∙∙∙∙∙∙∙   ∙∙∙∙∙∙∙∙∙∙∙∙   [====]∙∙∙∙∙∙   ∙∙∙∙∙∙∙∙∙∙∙∙
        G#1   ∙∙∙∙∙∙∙∙∙∙∙∙   ∙∙∙∙∙∙∙∙∙∙∙∙   ∙∙∙∙∙∙∙∙∙∙∙∙   [====][====]
        F#1   ∙∙∙∙∙∙∙∙∙∙∙∙   ∙∙∙∙∙∙∙∙∙∙∙∙   ∙∙∙∙∙∙[====]   ∙∙∙∙∙∙∙∙∙∙∙∙")
    .Build()
    .ToFile(TempoMap.Create(Tempo.FromBeatsPerMinute(69)));
midiFile.Write("pianoroll-moonlight-sonata.mid", true);
```

And here is the file – [pianoroll-moonlight-sonata.mid](files/pianoroll-moonlight-sonata.mid).

Along with usage of spaces to separate bars visually for more readability you can see several new symbols in this example:

* `'.'` and `'='` are both just fill symbols.
* `'['` and `']'` mean the start and the end of a multi-cell note correspondingly. These symbols can be changed too, see properties [MultiCellNoteStartSymbol](xref:Melanchall.DryWetMidi.Composing.PianoRollSettings.MultiCellNoteStartSymbol) and [MultiCellNoteEndSymbol](xref:Melanchall.DryWetMidi.Composing.PianoRollSettings.MultiCellNoteEndSymbol) of the [PianoRollSettings](xref:Melanchall.DryWetMidi.Composing.PianoRollSettings) that you can pass to the [PianoRoll](xref:Melanchall.DryWetMidi.Composing.PatternBuilder.PianoRoll*) method.

Well, we can define long notes (multi-cell) along with single-cell ones. As for length of such notes, let's see at this note:

```text
[====]
```

The length will be of six cells:

```text
[====]
123456
```

So if one cell means 8th triplet time span in our example, the length of the note will be `1/2`.

### Customization

It's time to discuss how you can adjust piano roll processing. First of all, as we said before, you can set custom symbols for a single-cell note, start and end of a multi-cell note:

```csharp
var midiFile = new PatternBuilder()
    .SetNoteLength(MusicalTimeSpan.Eighth.Triplet())
    .PianoRoll(@"
        F#4 ∙∙@∙∙@∙∙∙∙∙∙
        E4  ∙∙∙(~~~~~~~)", new PianoRollSettings
        {
            SingleCellNoteSymbol = '@',
            MultiCellNoteStartSymbol = '(',
            MultiCellNoteEndSymbol = ')',
        })
    .Build()
    .ToFile(TempoMap.Default);
```

So the way to customize the piano roll algorithm is to pass [PianoRollSettings](xref:Melanchall.DryWetMidi.Composing.PianoRollSettings). But you can also define your own actions triggered by specified symbols. Let's take a look at the following example (yes, drums again):

```csharp
var pianoRollSettings = new PianoRollSettings
{
    CustomActions = new Dictionary<char, Action<Melanchall.DryWetMidi.MusicTheory.Note, PatternBuilder>>
    {
        ['*'] = (note, pianoRollBuilder) => pianoRollBuilder
            .Note(note, velocity: (SevenBitNumber)(pianoRollBuilder.Velocity / 2)),
        ['║'] = (note, pianoRollBuilder) => pianoRollBuilder
            .Note(note, pianoRollBuilder.NoteLength.Divide(2))
            .Note(note, pianoRollBuilder.NoteLength.Divide(2), (SevenBitNumber)(pianoRollBuilder.Velocity / 2)),
        ['!'] = (note, pianoRollBuilder) => pianoRollBuilder
            .StepBack(MusicalTimeSpan.ThirtySecond)
            .Note(note, MusicalTimeSpan.ThirtySecond, (SevenBitNumber)(pianoRollBuilder.Velocity / 3))
            .Note(note),
    }
};

var bassDrum = 36;
var snareDrum = 38;
var closedHiHat = 42;

var midiFile = new PatternBuilder()
    .SetNoteLength(MusicalTimeSpan.Eighth)
    .PianoRoll(@$"
        {closedHiHat}   -------|   ║║|----|
          {snareDrum}   -*|---!-   --|--*!|
           {bassDrum}   |--║|---   |-|║|---",
        pianoRollSettings)
    .Repeat(9)
    .Build()
    .ToFile(TempoMap.Default, (FourBitNumber)9);
midiFile.Write("pianoroll-custom.mid", true);
```

And here the file – [pianoroll-custom.mid](files/pianoroll-custom.mid). But what we have in the piano roll string:

* `'*'` – ghost note (played with half of the current velocity);
* `'║'` – double note (two notes, each with length of half of the single-cell note);
* `'!'` – flam (ghost thirthy-second note right before main beat).

Right now it's possible to specify single-cell actions only. A way to put custom multi-cell actions will be implemented in the next release.