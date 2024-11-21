using System;
using System.Linq;
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
        public void Note_MixedLengthAndVelocity()
        {
            var defaultNoteLength = MusicalTimeSpan.Quarter;
            var defaultVelocity = (SevenBitNumber)90;

            var specifiedLength = new MetricTimeSpan(0, 0, 10);
            var specifiedVelocity = (SevenBitNumber)95;

            var pattern = new PatternBuilder()
                .SetNoteLength(defaultNoteLength)
                .SetVelocity(defaultVelocity)

                .Note(Notes.A0)
                .Note(Notes.C1, specifiedLength, specifiedVelocity)

                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 0, null, defaultNoteLength, defaultVelocity),
                new NoteInfo(NoteName.C, 1, MusicalTimeSpan.Quarter, specifiedLength, specifiedVelocity)
            });
        }

        [Test]
        public void Note_Multiple_MetricLengths()
        {
            var pattern = new PatternBuilder()
                .SetOctave(Octave.Get(2))

                .Note(NoteName.G, new MetricTimeSpan(0, 0, 24))
                .Note(NoteName.A, new MetricTimeSpan(0, 1, 0))
                .Note(NoteName.B, new MetricTimeSpan(0, 0, 5))

                .Build();

            var midiFile = PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.G, 2, null, new MetricTimeSpan(0, 0, 24)),
                new NoteInfo(NoteName.A, 2, new MetricTimeSpan(0, 0, 24), new MetricTimeSpan(0, 1, 0)),
                new NoteInfo(NoteName.B, 2, new MetricTimeSpan(0, 1, 24), new MetricTimeSpan(0, 0, 5)),
            });

            Assert.AreEqual(new MetricTimeSpan(0, 1, 29), midiFile.GetDuration<MetricTimeSpan>());
        }

        [Test]
        public void Note_Multiple_MetricLengths_TempoChanged()
        {
            var pattern = new PatternBuilder()
                .SetOctave(Octave.Get(2))

                .Note(NoteName.G, new MetricTimeSpan(0, 0, 24))
                .Note(NoteName.A, new MetricTimeSpan(0, 1, 0))
                .Note(NoteName.B, new MetricTimeSpan(0, 0, 5))

                .Build();

            var midiFile = PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.G, 2, null, new MetricTimeSpan(0, 0, 24)),
                new NoteInfo(NoteName.A, 2, new MetricTimeSpan(0, 0, 24), new MetricTimeSpan(0, 1, 0)),
                new NoteInfo(NoteName.B, 2, new MetricTimeSpan(0, 1, 24), new MetricTimeSpan(0, 0, 5)),
            },
            Enumerable.Range(0, 7)
                      .Select(i => Tuple.Create(i * 1000L, new Tempo(i * 100 + 10)))
                      .ToArray());

            Assert.AreEqual(new MetricTimeSpan(0, 1, 29).TotalMicroseconds,
                            midiFile.GetDuration<MetricTimeSpan>().TotalMicroseconds);
        }

        [Test]
        public void Note_Multiple_Interval()
        {
            var defaultNoteLength = MusicalTimeSpan.Quarter;
            var defaultVelocity = (SevenBitNumber)90;

            var pattern = new PatternBuilder()
                .SetNoteLength(defaultNoteLength)
                .SetVelocity(defaultVelocity)
                .SetRootNote(Notes.CSharp5)

                .Note(Interval.Two)
                .Note(-Interval.Four)

                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.DSharp, 5, null, defaultNoteLength, defaultVelocity),
                new NoteInfo(NoteName.A, 4, defaultNoteLength, defaultNoteLength, defaultVelocity)
            });
        }

        [Test]
        public void Note_ByInterval_OutOfRange_Up() => Assert.Throws<ArgumentException>(() =>
            new PatternBuilder()
                .SetRootNote(DryWetMidi.MusicTheory.Note.Get((SevenBitNumber)30))
                .Note(Interval.FromHalfSteps(100)));

        [Test]
        public void Note_ByInterval_OutOfRange_Down() => Assert.Throws<ArgumentException>(() =>
            new PatternBuilder()
                .SetRootNote(DryWetMidi.MusicTheory.Note.Get((SevenBitNumber)30))
                .Note(-Interval.FromHalfSteps(100)));

        [Test]
        public void Note_ByString()
        {
            var noteLength = MusicalTimeSpan.Eighth;
            var velocity = (SevenBitNumber)90;

            var pattern = new PatternBuilder()
                .Note("a0")
                .SetNoteLength(noteLength)
                .SetVelocity(velocity)
                .Note("C#5")
                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 0, null, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),
                new NoteInfo(NoteName.CSharp, 5, PatternBuilder.DefaultNoteLength, noteLength, velocity)
            });
        }

        [Test]
        public void Note_ByString_Velocity()
        {
            var noteLength = MusicalTimeSpan.Eighth;
            var velocity = (SevenBitNumber)90;

            var pattern = new PatternBuilder()
                .Note("a0")
                .SetNoteLength(noteLength)
                .Note("C#5", velocity: velocity)
                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 0, null, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),
                new NoteInfo(NoteName.CSharp, 5, PatternBuilder.DefaultNoteLength, noteLength, velocity)
            });
        }

        [Test]
        public void Note_ByString_Length()
        {
            var noteLength = MusicalTimeSpan.Eighth;
            var velocity = (SevenBitNumber)90;

            var pattern = new PatternBuilder()
                .Note("a0")
                .SetVelocity(velocity)
                .Note("C#5", noteLength)
                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 0, null, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),
                new NoteInfo(NoteName.CSharp, 5, PatternBuilder.DefaultNoteLength, noteLength, velocity)
            });
        }

        [Test]
        public void Note_ByString_Length_Velocity()
        {
            var noteLength = MusicalTimeSpan.Eighth;
            var velocity = (SevenBitNumber)90;

            var pattern = new PatternBuilder()
                .Note("C#5", noteLength, velocity)
                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.CSharp, 5, null, noteLength, velocity)
            });
        }

        #endregion
    }
}
