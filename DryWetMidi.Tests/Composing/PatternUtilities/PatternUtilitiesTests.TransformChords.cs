using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Composing
{
    [TestFixture]
    public sealed partial class PatternUtilitiesTests
    {
        #region Test methods

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

            PatternTestUtilities.TestNotes(pattern, new[]
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

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.B, 0, null, 3 * MusicalTimeSpan.ThirtySecond, (SevenBitNumber)80),
                new NoteInfo(NoteName.DSharp, 2, null, 3 * MusicalTimeSpan.ThirtySecond, (SevenBitNumber)80),

                new NoteInfo(NoteName.GSharp, 2, 3 * MusicalTimeSpan.ThirtySecond, 3 * MusicalTimeSpan.ThirtySecond, (SevenBitNumber)80),
                new NoteInfo(NoteName.GSharp, 3, 3 * MusicalTimeSpan.ThirtySecond, 3 * MusicalTimeSpan.ThirtySecond, (SevenBitNumber)80),
                new NoteInfo(NoteName.GSharp, 4, 3 * MusicalTimeSpan.ThirtySecond, 3 * MusicalTimeSpan.ThirtySecond, (SevenBitNumber)80)
            });
        }

        [Test]
        public void TransformChords_Changed_Pattern_Recursive()
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

            PatternTestUtilities.TestNotes(pattern, new[]
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

        [Test]
        public void TransformChords_Changed_Pattern_NonRecursive()
        {
            var noteLength = (MidiTimeSpan)200;
            var velocity = (SevenBitNumber)90;

            var subPattern = new PatternBuilder()
                .Chord(new[] { Notes.A0, Notes.CSharp2 })
                .Build();

            var pattern = new PatternBuilder()
                .SetNoteLength(noteLength)
                .SetVelocity(velocity)
                .Chord(new[] { Notes.A2, Notes.C3 })
                .Pattern(subPattern)
                .Build();

            pattern = pattern.TransformChords(d => new ChordDescriptor(
                d.Notes.Select(n => n.Transpose(Interval.One)),
                (SevenBitNumber)65,
                (MidiTimeSpan)100),
                recursive: false);

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.ASharp, 2, null, (MidiTimeSpan)100, (SevenBitNumber)65),
                new NoteInfo(NoteName.CSharp, 3, null, (MidiTimeSpan)100, (SevenBitNumber)65),

                new NoteInfo(NoteName.A, 0, new MidiTimeSpan(100), PatternBuilder.DefaultNoteLength),
                new NoteInfo(NoteName.CSharp, 2, new MidiTimeSpan(100), PatternBuilder.DefaultNoteLength)
            });
        }

        [Test]
        public void TransformChords_Selection_SelectNone()
        {
            var noteLength = MusicalTimeSpan.Eighth;
            var velocity = (SevenBitNumber)90;

            var pattern = new PatternBuilder()
                .SetNoteLength(noteLength)
                .SetVelocity(velocity)
                .Chord(new[] { Notes.A0, Notes.CSharp2 })
                .Chord(new[] { Notes.B2, Notes.B3, Notes.B4 })
                .Build();

            pattern = pattern.TransformChords((i, d) => false, d => new ChordDescriptor(Enumerable.Empty<DryWetMidi.MusicTheory.Note>(), SevenBitNumber.MinValue, MusicalTimeSpan.Half));

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 0, null, noteLength, velocity),
                new NoteInfo(NoteName.CSharp, 2, null, noteLength, velocity),

                new NoteInfo(NoteName.B, 2, noteLength, noteLength, velocity),
                new NoteInfo(NoteName.B, 3, noteLength, noteLength, velocity),
                new NoteInfo(NoteName.B, 4, noteLength, noteLength, velocity)
            });
        }

        [Test]
        public void TransformChords_Selection_SelectSome()
        {
            var noteLength = MusicalTimeSpan.Eighth;
            var velocity = (SevenBitNumber)90;

            var pattern = new PatternBuilder()
                .SetNoteLength(noteLength)
                .SetVelocity(velocity)
                .Chord(new[] { Notes.A0, Notes.CSharp2 })
                .Chord(new[] { Notes.B2, Notes.B3, Notes.B4 })
                .Build();

            pattern = pattern.TransformChords(
                (i, d) => d.Notes.Contains(Notes.CSharp2),
                d => new ChordDescriptor(
                    new[] { Notes.A2 },
                    SevenBitNumber.MinValue,
                    MusicalTimeSpan.Half));

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 2, null, MusicalTimeSpan.Half, SevenBitNumber.MinValue),

                new NoteInfo(NoteName.B, 2, MusicalTimeSpan.Half, noteLength, velocity),
                new NoteInfo(NoteName.B, 3, MusicalTimeSpan.Half, noteLength, velocity),
                new NoteInfo(NoteName.B, 4, MusicalTimeSpan.Half, noteLength, velocity)
            });
        }

        [Test]
        public void TransformChords_Selection_SelectSome_ByIndex()
        {
            var noteLength = MusicalTimeSpan.Eighth;
            var velocity = (SevenBitNumber)90;

            var pattern = new PatternBuilder()
                .SetNoteLength(noteLength)
                .SetVelocity(velocity)
                .Chord(new[] { Notes.A0, Notes.CSharp2 })
                .Chord(new[] { Notes.B2, Notes.B3, Notes.B4 })
                .Build();

            pattern = pattern.TransformChords(
                (i, d) => i == 0,
                d => new ChordDescriptor(
                    new[] { Notes.A2 },
                    SevenBitNumber.MinValue,
                    MusicalTimeSpan.Half));

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 2, null, MusicalTimeSpan.Half, SevenBitNumber.MinValue),

                new NoteInfo(NoteName.B, 2, MusicalTimeSpan.Half, noteLength, velocity),
                new NoteInfo(NoteName.B, 3, MusicalTimeSpan.Half, noteLength, velocity),
                new NoteInfo(NoteName.B, 4, MusicalTimeSpan.Half, noteLength, velocity)
            });
        }

        [Test]
        public void TransformChords_Selection_SelectAll()
        {
            var noteLength = MusicalTimeSpan.Eighth;
            var velocity = (SevenBitNumber)90;

            var pattern = new PatternBuilder()
                .SetNoteLength(noteLength)
                .SetVelocity(velocity)
                .Chord(new[] { Notes.A0, Notes.CSharp2 })
                .Chord(new[] { Notes.B2, Notes.B3, Notes.B4 })
                .Build();

            pattern = pattern.TransformChords(
                (i, d) => true,
                d => new ChordDescriptor(
                    new[] { Notes.A2 },
                    SevenBitNumber.MinValue,
                    MusicalTimeSpan.Half));

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 2, null, MusicalTimeSpan.Half, SevenBitNumber.MinValue),
                new NoteInfo(NoteName.A, 2, MusicalTimeSpan.Half, MusicalTimeSpan.Half, SevenBitNumber.MinValue),
            });
        }

        [Test]
        public void TransformChords_Selection_SelectSome_Pattern_Recursive()
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

            pattern = pattern.TransformChords(
                (i, d) => i == 1,
                d => new ChordDescriptor(
                    d.Notes.Select(n => n.Transpose(Interval.One)),
                    (SevenBitNumber)65,
                    (MidiTimeSpan)100),
                true);

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 2, null, noteLength, velocity),
                new NoteInfo(NoteName.C, 3, noteLength, noteLength, velocity),

                new NoteInfo(NoteName.A, 0, (MidiTimeSpan)(2 * noteLength), PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),
                new NoteInfo(NoteName.CSharp, 2, (MidiTimeSpan)(2 * noteLength), PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),

                new NoteInfo(NoteName.C, 3, ((MidiTimeSpan)(2 * noteLength)).Add(PatternBuilder.DefaultNoteLength, TimeSpanMode.TimeLength), (MidiTimeSpan)100, (SevenBitNumber)65),
                new NoteInfo(NoteName.C, 4, ((MidiTimeSpan)(2 * noteLength)).Add(PatternBuilder.DefaultNoteLength, TimeSpanMode.TimeLength), (MidiTimeSpan)100, (SevenBitNumber)65),
                new NoteInfo(NoteName.C, 5, ((MidiTimeSpan)(2 * noteLength)).Add(PatternBuilder.DefaultNoteLength, TimeSpanMode.TimeLength), (MidiTimeSpan)100, (SevenBitNumber)65)
            });
        }

        #endregion
    }
}
