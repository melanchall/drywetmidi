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
        [Description("Add chord with default velocity and octave.")]
        public void Chord_DefaultOctave()
        {
            var defaultVelocity = (SevenBitNumber)90;
            var defaultOctave = Octave.Get(2);

            var chordLength = MusicalTimeSpan.Sixteenth.Triplet();
            var chordTime1 = new MetricTimeSpan(0, 1, 12);
            var chordTime2 = chordTime1.Add(chordLength, TimeSpanMode.TimeLength);

            var pattern = new PatternBuilder()
                .SetVelocity(defaultVelocity)
                .SetOctave(defaultOctave)

                .MoveToTime(chordTime1)
                .Chord(new[]
                {
                    NoteName.C,
                    NoteName.G
                }, chordLength)
                .Repeat()

                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.C, defaultOctave.Number, chordTime1, chordLength, defaultVelocity),
                new NoteInfo(NoteName.G, defaultOctave.Number, chordTime1, chordLength, defaultVelocity),
                new NoteInfo(NoteName.C, defaultOctave.Number, chordTime2, chordLength, defaultVelocity),
                new NoteInfo(NoteName.G, defaultOctave.Number, chordTime2, chordLength, defaultVelocity)
            });
        }

        [Test]
        [Description("Add several chords by interval.")]
        public void Chord_Interval()
        {
            var defaultNoteLength = MusicalTimeSpan.Quarter;
            var defaultVelocity = (SevenBitNumber)90;

            var pattern = new PatternBuilder()
                .SetNoteLength(defaultNoteLength)
                .SetVelocity(defaultVelocity)
                .SetRootNote(Notes.CSharp5)

                .Chord(new[] { Interval.Two, Interval.Five }, Notes.A2)
                .Chord(new[] { Interval.Two, -Interval.Ten }, Notes.B2)

                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 2, null, defaultNoteLength, defaultVelocity),
                new NoteInfo(NoteName.B, 2, null, defaultNoteLength, defaultVelocity),
                new NoteInfo(NoteName.D, 3, null, defaultNoteLength, defaultVelocity),

                new NoteInfo(NoteName.B, 2, defaultNoteLength, defaultNoteLength, defaultVelocity),
                new NoteInfo(NoteName.CSharp, 3, defaultNoteLength, defaultNoteLength, defaultVelocity),
                new NoteInfo(NoteName.CSharp, 2, defaultNoteLength, defaultNoteLength, defaultVelocity),
            });
        }

        #endregion
    }
}
