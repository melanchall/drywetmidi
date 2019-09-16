using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Smf.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Composing
{
    [TestFixture]
    public sealed partial class PatternTests
    {
        #region Test methods

        [Test]
        public void TransformNotes_Original()
        {
            var noteLength = MusicalTimeSpan.Eighth;
            var velocity = (SevenBitNumber)90;

            var pattern = new PatternBuilder()
                .SetNoteLength(noteLength)
                .SetVelocity(velocity)
                .Note(Notes.A0)
                .Note(Notes.C1)
                .Build();

            pattern = pattern.TransformNotes(d => d);

            TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 0, null, noteLength, velocity),
                new NoteInfo(NoteName.C, 1, noteLength, noteLength, velocity)
            });
        }

        [Test]
        public void TransformNotes_Changed()
        {
            var noteLength = MusicalTimeSpan.Eighth;
            var velocity = (SevenBitNumber)90;

            var pattern = new PatternBuilder()
                .SetNoteLength(noteLength)
                .SetVelocity(velocity)
                .Note(Notes.A2)
                .Note(Notes.C3)
                .Build();

            pattern = pattern.TransformNotes(d => new NoteDescriptor(
                d.Note == Notes.A2 ? d.Note.Transpose(Interval.Two) : d.Note.Transpose(-Interval.Three),
                (SevenBitNumber)(d.Velocity - 10),
                d.Length.Subtract(MusicalTimeSpan.ThirtySecond, TimeSpanMode.LengthLength)));

            TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.B, 2, null, 3 * MusicalTimeSpan.ThirtySecond, (SevenBitNumber)80),
                new NoteInfo(NoteName.A, 2, 3 * MusicalTimeSpan.ThirtySecond, 3 * MusicalTimeSpan.ThirtySecond, (SevenBitNumber)80)
            });
        }

        [Test]
        public void TransformNotes_Changed_Pattern()
        {
            var noteLength = MusicalTimeSpan.Eighth;
            var velocity = (SevenBitNumber)90;

            var subPattern = new PatternBuilder()
                .Note(Notes.DSharp3)
                .Note(Notes.B1)
                .Build();

            var pattern = new PatternBuilder()
                .SetNoteLength(noteLength)
                .SetVelocity(velocity)
                .Note(Notes.A2)
                .Note(Notes.C3)
                .Pattern(subPattern)
                .Build();

            pattern = pattern.TransformNotes(d => new NoteDescriptor(Notes.D9, (SevenBitNumber)65, (MidiTimeSpan)100));

            TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.D, 9, null, (MidiTimeSpan)100, (SevenBitNumber)65),
                new NoteInfo(NoteName.D, 9, (MidiTimeSpan)100, (MidiTimeSpan)100, (SevenBitNumber)65),

                new NoteInfo(NoteName.D, 9, (MidiTimeSpan)200, (MidiTimeSpan)100, (SevenBitNumber)65),
                new NoteInfo(NoteName.D, 9, (MidiTimeSpan)300, (MidiTimeSpan)100, (SevenBitNumber)65)
            });
        }

        [Test]
        public void TransformChords_Original()
        {
            var noteLength = MusicalTimeSpan.Eighth;
            var velocity = (SevenBitNumber)90;

            var pattern = new PatternBuilder()
                .SetNoteLength(noteLength)
                .SetVelocity(velocity)
                .Chord(new[] { Notes.A0, Notes.CSharp2 })
                .Chord(new[] { Notes.B2, Notes.B3, Notes.B4 })
                .Build();

            pattern = pattern.TransformChords(d => d);

            TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 0, null, noteLength, velocity),
                new NoteInfo(NoteName.CSharp, 2, null, noteLength, velocity),

                new NoteInfo(NoteName.B, 2, noteLength, noteLength, velocity),
                new NoteInfo(NoteName.B, 3, noteLength, noteLength, velocity),
                new NoteInfo(NoteName.B, 4, noteLength, noteLength, velocity)
            });
        }

        [Test]
        public void TransformChords_Changed()
        {
            var noteLength = MusicalTimeSpan.Eighth;
            var velocity = (SevenBitNumber)90;

            var pattern = new PatternBuilder()
                .SetNoteLength(noteLength)
                .SetVelocity(velocity)
                .Chord(new[] { Notes.A0, Notes.CSharp2 })
                .Chord(new[] { Notes.B2, Notes.B3, Notes.B4 })
                .Build();

            pattern = pattern.TransformChords(d => new ChordDescriptor(
                d.Notes.Count() == 2 ? d.Notes.Select(n => n.Transpose(Interval.Two)) : d.Notes.Select(n => n.Transpose(-Interval.Three)),
                (SevenBitNumber)(d.Velocity - 10),
                d.Length.Subtract(MusicalTimeSpan.ThirtySecond, TimeSpanMode.LengthLength)));

            TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.B, 0, null, 3 * MusicalTimeSpan.ThirtySecond, (SevenBitNumber)80),
                new NoteInfo(NoteName.DSharp, 2, null, 3 * MusicalTimeSpan.ThirtySecond, (SevenBitNumber)80),

                new NoteInfo(NoteName.GSharp, 2, 3 * MusicalTimeSpan.ThirtySecond, 3 * MusicalTimeSpan.ThirtySecond, (SevenBitNumber)80),
                new NoteInfo(NoteName.GSharp, 3, 3 * MusicalTimeSpan.ThirtySecond, 3 * MusicalTimeSpan.ThirtySecond, (SevenBitNumber)80),
                new NoteInfo(NoteName.GSharp, 4, 3 * MusicalTimeSpan.ThirtySecond, 3 * MusicalTimeSpan.ThirtySecond, (SevenBitNumber)80)
            });
        }

        [Test]
        public void TransformChords_Changed_Pattern()
        {
            var noteLength = (MidiTimeSpan)200;
            var velocity = (SevenBitNumber)90;

            var subPattern = new PatternBuilder()
                .Chord(new[] { Notes.A0, Notes.CSharp2 })
                .Chord(new[] { Notes.B2, Notes.B3, Notes.B4 })
                .Build();

            var pattern = new PatternBuilder()
                .SetNoteLength(noteLength)
                .SetVelocity(velocity)
                .Note(Notes.A2)
                .Note(Notes.C3)
                .Pattern(subPattern)
                .Build();

            pattern = pattern.TransformChords(d => new ChordDescriptor(
                d.Notes.Select(n => n.Transpose(Interval.One)),
                (SevenBitNumber)65,
                (MidiTimeSpan)100));

            TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 2, null, noteLength, velocity),
                new NoteInfo(NoteName.C, 3, noteLength, noteLength, velocity),

                new NoteInfo(NoteName.ASharp, 0, (MidiTimeSpan)(2 * noteLength), (MidiTimeSpan)100, (SevenBitNumber)65),
                new NoteInfo(NoteName.D, 2, (MidiTimeSpan)(2 * noteLength), (MidiTimeSpan)100, (SevenBitNumber)65),

                new NoteInfo(NoteName.C, 3, (MidiTimeSpan)(2 * noteLength) + (MidiTimeSpan)100, (MidiTimeSpan)100, (SevenBitNumber)65),
                new NoteInfo(NoteName.C, 4, (MidiTimeSpan)(2 * noteLength) + (MidiTimeSpan)100, (MidiTimeSpan)100, (SevenBitNumber)65),
                new NoteInfo(NoteName.C, 5, (MidiTimeSpan)(2 * noteLength) + (MidiTimeSpan)100, (MidiTimeSpan)100, (SevenBitNumber)65)
            });
        }

        #endregion
    }
}
