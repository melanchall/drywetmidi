using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Composing
{
    [TestFixture]
    public sealed partial class PatternBuilderTests
    {
        #region Test methods

        [Test]
        public void Chord_Chord()
        {
            var pattern = new PatternBuilder()
                .SetNoteLength(MusicalTimeSpan.Whole)
                .SetOctave(Octave.Get(7))
                .Chord(new DryWetMidi.MusicTheory.Chord(NoteName.A, NoteName.C))
                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 7, null, MusicalTimeSpan.Whole, PatternBuilder.DefaultVelocity),
                new NoteInfo(NoteName.C, 8, null, MusicalTimeSpan.Whole, PatternBuilder.DefaultVelocity)
            });
        }

        [Test]
        public void Chord_Chord_Octave()
        {
            var pattern = new PatternBuilder()
                .SetNoteLength(MusicalTimeSpan.Whole)
                .SetOctave(Octave.Get(7))
                .Chord(new DryWetMidi.MusicTheory.Chord(NoteName.A, NoteName.C), Octave.Get(2))
                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 2, null, MusicalTimeSpan.Whole, PatternBuilder.DefaultVelocity),
                new NoteInfo(NoteName.C, 3, null, MusicalTimeSpan.Whole, PatternBuilder.DefaultVelocity)
            });
        }

        [Test]
        public void Chord_Chord_Length()
        {
            var pattern = new PatternBuilder()
                .SetNoteLength(MusicalTimeSpan.Whole)
                .SetOctave(Octave.Get(7))
                .Chord(new DryWetMidi.MusicTheory.Chord(NoteName.A, NoteName.C), MusicalTimeSpan.Eighth)
                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 7, null, MusicalTimeSpan.Eighth, PatternBuilder.DefaultVelocity),
                new NoteInfo(NoteName.C, 8, null, MusicalTimeSpan.Eighth, PatternBuilder.DefaultVelocity)
            });
        }

        [Test]
        public void Chord_Chord_Octave_Length()
        {
            var pattern = new PatternBuilder()
                .SetNoteLength(MusicalTimeSpan.Whole)
                .SetOctave(Octave.Get(7))
                .Chord(new DryWetMidi.MusicTheory.Chord(NoteName.A, NoteName.C), Octave.Get(2), MusicalTimeSpan.Eighth)
                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 2, null, MusicalTimeSpan.Eighth, PatternBuilder.DefaultVelocity),
                new NoteInfo(NoteName.C, 3, null, MusicalTimeSpan.Eighth, PatternBuilder.DefaultVelocity)
            });
        }

        [Test]
        public void Chord_Chord_Velocity()
        {
            var pattern = new PatternBuilder()
                .SetVelocity((SevenBitNumber)80)
                .Chord(new DryWetMidi.MusicTheory.Chord(NoteName.A, NoteName.C), (SevenBitNumber)70)
                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, PatternBuilder.DefaultOctave.Number, null, PatternBuilder.DefaultNoteLength, (SevenBitNumber)70),
                new NoteInfo(NoteName.C, PatternBuilder.DefaultOctave.Number + 1, null, PatternBuilder.DefaultNoteLength, (SevenBitNumber)70)
            });
        }

        [Test]
        public void Chord_Chord_Octave_Velocity()
        {
            var pattern = new PatternBuilder()
                .SetVelocity((SevenBitNumber)80)
                .SetOctave(Octave.Get(5))
                .Chord(new DryWetMidi.MusicTheory.Chord(NoteName.A, NoteName.C), Octave.Get(1),(SevenBitNumber)70)
                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 1, null, PatternBuilder.DefaultNoteLength, (SevenBitNumber)70),
                new NoteInfo(NoteName.C, 2, null, PatternBuilder.DefaultNoteLength, (SevenBitNumber)70)
            });
        }

        [Test]
        public void Chord_Chord_Length_Velocity()
        {
            var pattern = new PatternBuilder()
                .SetVelocity((SevenBitNumber)80)
                .SetNoteLength(MusicalTimeSpan.ThirtySecond)
                .Chord(new DryWetMidi.MusicTheory.Chord(NoteName.A, NoteName.C), MusicalTimeSpan.Half, (SevenBitNumber)70)
                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, PatternBuilder.DefaultOctave.Number, null, MusicalTimeSpan.Half, (SevenBitNumber)70),
                new NoteInfo(NoteName.C, PatternBuilder.DefaultOctave.Number + 1, null, MusicalTimeSpan.Half, (SevenBitNumber)70)
            });
        }

        [Test]
        public void Chord_Chord_Octave_Length_Velocity()
        {
            var pattern = new PatternBuilder()
                .SetVelocity((SevenBitNumber)80)
                .SetOctave(Octave.Get(8))
                .SetNoteLength(MusicalTimeSpan.ThirtySecond)
                .Chord(new DryWetMidi.MusicTheory.Chord(NoteName.A, NoteName.C), Octave.Get(1), MusicalTimeSpan.Half, (SevenBitNumber)70)
                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 1, null, MusicalTimeSpan.Half, (SevenBitNumber)70),
                new NoteInfo(NoteName.C, 2, null, MusicalTimeSpan.Half, (SevenBitNumber)70)
            });
        }

        [Test]
        public void Chord_Intervals_RootNoteName()
        {
            var defaultVelocity = (SevenBitNumber)90;
            var defaultOctave = Octave.Get(2);
            var defaultNoteLength = (MidiTimeSpan)300;

            var chordTime = MusicalTimeSpan.Eighth;

            var pattern = new PatternBuilder()
                .SetVelocity(defaultVelocity)
                .SetOctave(defaultOctave)
                .SetNoteLength(defaultNoteLength)
                .MoveToTime(chordTime)
                .Chord(new[] { Interval.FromHalfSteps(2), Interval.FromHalfSteps(4) }, NoteName.D)
                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.D, defaultOctave.Number, chordTime, defaultNoteLength, defaultVelocity),
                new NoteInfo(NoteName.E, defaultOctave.Number, chordTime, defaultNoteLength, defaultVelocity),
                new NoteInfo(NoteName.FSharp, defaultOctave.Number, chordTime, defaultNoteLength, defaultVelocity)
            });
        }

        [Test]
        public void Chord_Intervals_RootNoteName_Length()
        {
            var defaultVelocity = (SevenBitNumber)90;
            var defaultOctave = Octave.Get(2);

            var chordLength = (MidiTimeSpan)300;

            var pattern = new PatternBuilder()
                .SetVelocity(defaultVelocity)
                .SetOctave(defaultOctave)
                .Chord(new[] { Interval.FromHalfSteps(2), Interval.FromHalfSteps(4) }, NoteName.D, chordLength)
                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.D, defaultOctave.Number, null, chordLength, defaultVelocity),
                new NoteInfo(NoteName.E, defaultOctave.Number, null, chordLength, defaultVelocity),
                new NoteInfo(NoteName.FSharp, defaultOctave.Number, null, chordLength, defaultVelocity)
            });
        }

        [Test]
        public void Chord_Intervals_RootNoteName_Velocity()
        {
            var defaultOctave = Octave.Get(2);
            var defaultNoteLength = (MidiTimeSpan)300;

            var chordVelocity = (SevenBitNumber)70;

            var pattern = new PatternBuilder()
                .SetOctave(defaultOctave)
                .SetNoteLength(defaultNoteLength)
                .Chord(new[] { Interval.FromHalfSteps(2), Interval.FromHalfSteps(4) }, NoteName.D, chordVelocity)
                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.D, defaultOctave.Number, null, defaultNoteLength, chordVelocity),
                new NoteInfo(NoteName.E, defaultOctave.Number, null, defaultNoteLength, chordVelocity),
                new NoteInfo(NoteName.FSharp, defaultOctave.Number, null, defaultNoteLength, chordVelocity)
            });
        }

        [Test]
        public void Chord_Intervals_RootNoteName_Length_Velocity()
        {
            var defaultOctave = Octave.Get(2);

            var chordVelocity = (SevenBitNumber)70;
            var chordLength = (MidiTimeSpan)300;

            var pattern = new PatternBuilder()
                .SetOctave(defaultOctave)
                .Chord(new[] { Interval.FromHalfSteps(2), Interval.FromHalfSteps(4) }, NoteName.D, chordLength, chordVelocity)
                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.D, defaultOctave.Number, null, chordLength, chordVelocity),
                new NoteInfo(NoteName.E, defaultOctave.Number, null, chordLength, chordVelocity),
                new NoteInfo(NoteName.FSharp, defaultOctave.Number, null, chordLength, chordVelocity)
            });
        }

        [Test]
        public void Chord_Intervals_RootNote()
        {
            var defaultVelocity = (SevenBitNumber)90;
            var defaultOctave = Octave.Get(2);
            var defaultNoteLength = (MidiTimeSpan)300;

            var chordTime = MusicalTimeSpan.Eighth;

            var pattern = new PatternBuilder()
                .SetVelocity(defaultVelocity)
                .SetOctave(defaultOctave)
                .SetNoteLength(defaultNoteLength)
                .MoveToTime(chordTime)
                .Chord(new[] { Interval.FromHalfSteps(2), Interval.FromHalfSteps(4) }, Notes.D3)
                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.D, 3, chordTime, defaultNoteLength, defaultVelocity),
                new NoteInfo(NoteName.E, 3, chordTime, defaultNoteLength, defaultVelocity),
                new NoteInfo(NoteName.FSharp, 3, chordTime, defaultNoteLength, defaultVelocity)
            });
        }

        [Test]
        public void Chord_Intervals_RootNote_Length()
        {
            var defaultVelocity = (SevenBitNumber)90;
            var defaultOctave = Octave.Get(2);

            var chordLength = (MidiTimeSpan)300;

            var pattern = new PatternBuilder()
                .SetVelocity(defaultVelocity)
                .SetOctave(defaultOctave)
                .Chord(new[] { Interval.FromHalfSteps(2), Interval.FromHalfSteps(4) }, Notes.D3, chordLength)
                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.D, 3, null, chordLength, defaultVelocity),
                new NoteInfo(NoteName.E, 3, null, chordLength, defaultVelocity),
                new NoteInfo(NoteName.FSharp, 3, null, chordLength, defaultVelocity)
            });
        }

        [Test]
        public void Chord_Intervals_RootNote_Velocity()
        {
            var defaultOctave = Octave.Get(2);
            var defaultNoteLength = (MidiTimeSpan)300;

            var chordVelocity = (SevenBitNumber)70;

            var pattern = new PatternBuilder()
                .SetOctave(defaultOctave)
                .SetNoteLength(defaultNoteLength)
                .Chord(new[] { Interval.FromHalfSteps(2), Interval.FromHalfSteps(4) }, Notes.D3, chordVelocity)
                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.D, 3, null, defaultNoteLength, chordVelocity),
                new NoteInfo(NoteName.E, 3, null, defaultNoteLength, chordVelocity),
                new NoteInfo(NoteName.FSharp, 3, null, defaultNoteLength, chordVelocity)
            });
        }

        [Test]
        public void Chord_Intervals_RootNote_Length_Velocity()
        {
            var defaultOctave = Octave.Get(2);

            var chordVelocity = (SevenBitNumber)70;
            var chordLength = (MidiTimeSpan)300;

            var pattern = new PatternBuilder()
                .SetOctave(defaultOctave)
                .Chord(new[] { Interval.FromHalfSteps(2), Interval.FromHalfSteps(4) }, Notes.D3, chordLength, chordVelocity)
                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.D, 3, null, chordLength, chordVelocity),
                new NoteInfo(NoteName.E, 3, null, chordLength, chordVelocity),
                new NoteInfo(NoteName.FSharp, 3, null, chordLength, chordVelocity)
            });
        }

        [Test]
        public void Chord_NotesNames()
        {
            var defaultVelocity = (SevenBitNumber)90;
            var defaultOctave = Octave.Get(2);
            var defaultNoteLength = (MidiTimeSpan)300;

            var chordTime = MusicalTimeSpan.Eighth;

            var pattern = new PatternBuilder()
                .SetVelocity(defaultVelocity)
                .SetOctave(defaultOctave)
                .SetNoteLength(defaultNoteLength)
                .MoveToTime(chordTime)
                .Chord(new[] { NoteName.D, NoteName.E, NoteName.FSharp })
                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.D, defaultOctave.Number, chordTime, defaultNoteLength, defaultVelocity),
                new NoteInfo(NoteName.E, defaultOctave.Number, chordTime, defaultNoteLength, defaultVelocity),
                new NoteInfo(NoteName.FSharp, defaultOctave.Number, chordTime, defaultNoteLength, defaultVelocity)
            });
        }

        [Test]
        public void Chord_NotesNames_Length()
        {
            var defaultVelocity = (SevenBitNumber)90;
            var defaultOctave = Octave.Get(2);

            var chordLength = (MidiTimeSpan)300;

            var pattern = new PatternBuilder()
                .SetVelocity(defaultVelocity)
                .SetOctave(defaultOctave)
                .Chord(new[] { NoteName.D, NoteName.E, NoteName.FSharp }, chordLength)
                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.D, defaultOctave.Number, null, chordLength, defaultVelocity),
                new NoteInfo(NoteName.E, defaultOctave.Number, null, chordLength, defaultVelocity),
                new NoteInfo(NoteName.FSharp, defaultOctave.Number, null, chordLength, defaultVelocity)
            });
        }

        [Test]
        public void Chord_NotesNames_Velocity()
        {
            var defaultOctave = Octave.Get(2);
            var defaultNoteLength = (MidiTimeSpan)300;

            var chordVelocity = (SevenBitNumber)70;

            var pattern = new PatternBuilder()
                .SetOctave(defaultOctave)
                .SetNoteLength(defaultNoteLength)
                .Chord(new[] { NoteName.D, NoteName.E, NoteName.FSharp }, chordVelocity)
                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.D, defaultOctave.Number, null, defaultNoteLength, chordVelocity),
                new NoteInfo(NoteName.E, defaultOctave.Number, null, defaultNoteLength, chordVelocity),
                new NoteInfo(NoteName.FSharp, defaultOctave.Number, null, defaultNoteLength, chordVelocity)
            });
        }

        [Test]
        public void Chord_NotesNames_Length_Velocity()
        {
            var defaultOctave = Octave.Get(2);

            var chordVelocity = (SevenBitNumber)70;
            var chordLength = (MidiTimeSpan)300;

            var pattern = new PatternBuilder()
                .SetOctave(defaultOctave)
                .Chord(new[] { NoteName.D, NoteName.E, NoteName.FSharp }, chordLength, chordVelocity)
                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.D, defaultOctave.Number, null, chordLength, chordVelocity),
                new NoteInfo(NoteName.E, defaultOctave.Number, null, chordLength, chordVelocity),
                new NoteInfo(NoteName.FSharp, defaultOctave.Number, null, chordLength, chordVelocity)
            });
        }

        [Test]
        public void Chord_Notes()
        {
            var defaultVelocity = (SevenBitNumber)90;
            var defaultOctave = Octave.Get(2);
            var defaultNoteLength = (MidiTimeSpan)300;

            var chordTime = MusicalTimeSpan.Eighth;

            var pattern = new PatternBuilder()
                .SetVelocity(defaultVelocity)
                .SetOctave(defaultOctave)
                .SetNoteLength(defaultNoteLength)
                .MoveToTime(chordTime)
                .Chord(new[] { Notes.D3, Notes.E3, Notes.FSharp4 })
                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.D, 3, chordTime, defaultNoteLength, defaultVelocity),
                new NoteInfo(NoteName.E, 3, chordTime, defaultNoteLength, defaultVelocity),
                new NoteInfo(NoteName.FSharp, 4, chordTime, defaultNoteLength, defaultVelocity)
            });
        }

        [Test]
        public void Chord_Notes_Length()
        {
            var defaultVelocity = (SevenBitNumber)90;
            var defaultOctave = Octave.Get(2);

            var chordLength = (MidiTimeSpan)300;

            var pattern = new PatternBuilder()
                .SetVelocity(defaultVelocity)
                .SetOctave(defaultOctave)
                .Chord(new[] { Notes.D3, Notes.E3, Notes.FSharp4 }, chordLength)
                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.D, 3, null, chordLength, defaultVelocity),
                new NoteInfo(NoteName.E, 3, null, chordLength, defaultVelocity),
                new NoteInfo(NoteName.FSharp, 4, null, chordLength, defaultVelocity)
            });
        }

        [Test]
        public void Chord_Notes_Velocity()
        {
            var defaultOctave = Octave.Get(2);
            var defaultNoteLength = (MidiTimeSpan)300;

            var chordVelocity = (SevenBitNumber)70;

            var pattern = new PatternBuilder()
                .SetOctave(defaultOctave)
                .SetNoteLength(defaultNoteLength)
                .Chord(new[] { Notes.D3, Notes.E3, Notes.FSharp4 }, chordVelocity)
                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.D, 3, null, defaultNoteLength, chordVelocity),
                new NoteInfo(NoteName.E, 3, null, defaultNoteLength, chordVelocity),
                new NoteInfo(NoteName.FSharp, 4, null, defaultNoteLength, chordVelocity)
            });
        }

        [Test]
        public void Chord_Notes_Length_Velocity()
        {
            var defaultOctave = Octave.Get(2);

            var chordVelocity = (SevenBitNumber)70;
            var chordLength = (MidiTimeSpan)300;

            var pattern = new PatternBuilder()
                .SetOctave(defaultOctave)
                .Chord(new[] { Notes.D3, Notes.E3, Notes.FSharp4 }, chordLength, chordVelocity)
                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.D, 3, null, chordLength, chordVelocity),
                new NoteInfo(NoteName.E, 3, null, chordLength, chordVelocity),
                new NoteInfo(NoteName.FSharp, 4, null, chordLength, chordVelocity)
            });
        }

        #endregion
    }
}
