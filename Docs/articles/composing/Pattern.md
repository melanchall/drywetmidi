---
uid: a_composing_pattern
---

# Pattern

DryWetMIDI provides a way to create a MIDI file in more "musical" manner. The key class here is the [PatternBuilder](xref:Melanchall.DryWetMidi.Composing.PatternBuilder) which allows to build a musical composition. `PatternBuilder` provides a fluent interface to program the music. For example, you can insert a note like this:

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

Please take a look at entire API provided by [PatternBuilder](xref:Melanchall.DryWetMidi.Composing.PatternBuilder).

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
    .Note(Interval.Zero)  // +0  (G#3)
    .Note(Interval.Five)  // +5  (C#4)
    .Note(Interval.Eight) // +8  (E4)
    .Repeat(3, 7)         // repeat three previous notes seven times
    
    // Add notes of the next two bars
                            // G#3
    .Note(Interval.One)   // +1  (A3)
    .Note(Interval.Five)  // +5  (C#4)
    .Note(Interval.Eight) // +8  (E4)
    .Repeat(3, 1)         // repeat three previous notes
    .Note(Interval.One)   // +1  (A3)
    .Note(Interval.Six)   // +6  (D4)
    .Note(Interval.Ten)   // +10 (F#4)
    .Repeat(3, 1)         // repeat three previous notes
                            // reaching the end of third bar
    .Note(Interval.Zero)  // +0  (G#3)
    .Note(Interval.Four)  // +4  (C4)
    .Note(Interval.Ten)   // +10 (F#4)
    .Note(Interval.Zero)  // +0  (G#3)
    .Note(Interval.Five)  // +5  (C#4)
    .Note(Interval.Eight) // +8  (E4)
    .Note(Interval.Zero)  // +0  (G#3)
    .Note(Interval.Five)  // +5  (C#4)
    .Note(Intervaln.Seven) // +7  (D#4)
    .Note(-Interval.Two)  // -2  (F#3)
    .Note(Interval.Four)  // +4  (C4)
    .Note(Interval.Seven) // +7  (D#4)
    
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

Also see [Extension methods](xref:Melanchall.DryWetMidi.Composing.Pattern#extensionmethods) section of the [Pattern](xref:Melanchall.DryWetMidi.Composing.Pattern) API.