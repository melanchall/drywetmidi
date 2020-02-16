using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Composing
{
    [TestFixture]
    public sealed class PatternUtilitiesTests
    {
        #region Test methods

        #region TransformNotes

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

            PatternTestUtilities.TestNotes(pattern, new[]
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

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.B, 2, null, 3 * MusicalTimeSpan.ThirtySecond, (SevenBitNumber)80),
                new NoteInfo(NoteName.A, 2, 3 * MusicalTimeSpan.ThirtySecond, 3 * MusicalTimeSpan.ThirtySecond, (SevenBitNumber)80)
            });
        }

        [Test]
        public void TransformNotes_Changed_Pattern_Recursive()
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

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.D, 9, null, (MidiTimeSpan)100, (SevenBitNumber)65),
                new NoteInfo(NoteName.D, 9, (MidiTimeSpan)100, (MidiTimeSpan)100, (SevenBitNumber)65),

                new NoteInfo(NoteName.D, 9, (MidiTimeSpan)200, (MidiTimeSpan)100, (SevenBitNumber)65),
                new NoteInfo(NoteName.D, 9, (MidiTimeSpan)300, (MidiTimeSpan)100, (SevenBitNumber)65)
            });
        }

        [Test]
        public void TransformNotes_Changed_Pattern_NonRecursive()
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

            pattern = pattern.TransformNotes(d => new NoteDescriptor(Notes.D9, (SevenBitNumber)65, (MidiTimeSpan)100), recursive: false);

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.D, 9, null, (MidiTimeSpan)100, (SevenBitNumber)65),
                new NoteInfo(NoteName.D, 9, (MidiTimeSpan)100, (MidiTimeSpan)100, (SevenBitNumber)65),

                new NoteInfo(NoteName.DSharp, 3, (MidiTimeSpan)200, PatternBuilder.DefaultNoteLength),
                new NoteInfo(NoteName.B, 1, new MidiTimeSpan(200).Add(PatternBuilder.DefaultNoteLength, TimeSpanMode.LengthLength), PatternBuilder.DefaultNoteLength)
            });
        }

        [Test]
        public void TransformNotes_Selection_SelectNone()
        {
            var noteLength = MusicalTimeSpan.Eighth;
            var velocity = (SevenBitNumber)90;

            var pattern = new PatternBuilder()
                .SetNoteLength(noteLength)
                .SetVelocity(velocity)
                .Note(Notes.A0)
                .Note(Notes.C1)
                .Build();

            pattern = pattern.TransformNotes((i, d) => false, d => new NoteDescriptor(Notes.A2, (SevenBitNumber)70, MusicalTimeSpan.Half));

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 0, null, noteLength, velocity),
                new NoteInfo(NoteName.C, 1, noteLength, noteLength, velocity)
            });
        }

        [Test]
        public void TransformNotes_Selection_SelectSome()
        {
            var noteLength = MusicalTimeSpan.Eighth;
            var velocity = (SevenBitNumber)90;

            var pattern = new PatternBuilder()
                .SetNoteLength(noteLength)
                .SetVelocity(velocity)
                .Note(Notes.A2)
                .Note(Notes.C3)
                .Build();

            pattern = pattern.TransformNotes(
                (i, d) => d.Note == Notes.A2,
                d => new NoteDescriptor(
                    d.Note.Transpose(Interval.Two),
                    (SevenBitNumber)(d.Velocity - 10),
                    d.Length.Subtract(MusicalTimeSpan.ThirtySecond, TimeSpanMode.LengthLength)));

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.B, 2, null, 3 * MusicalTimeSpan.ThirtySecond, (SevenBitNumber)80),
                new NoteInfo(NoteName.C, 3, 3 * MusicalTimeSpan.ThirtySecond, noteLength, velocity)
            });
        }

        [Test]
        public void TransformNotes_Selection_SelectSome_ByIndex()
        {
            var noteLength = MusicalTimeSpan.Eighth;
            var velocity = (SevenBitNumber)90;

            var pattern = new PatternBuilder()
                .SetNoteLength(noteLength)
                .SetVelocity(velocity)
                .Note(Notes.A2)
                .Note(Notes.C3)
                .Build();

            pattern = pattern.TransformNotes(
                (i, d) => i == 0,
                d => new NoteDescriptor(
                    d.Note.Transpose(Interval.Two),
                    (SevenBitNumber)(d.Velocity - 10),
                    d.Length.Subtract(MusicalTimeSpan.ThirtySecond, TimeSpanMode.LengthLength)));

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.B, 2, null, 3 * MusicalTimeSpan.ThirtySecond, (SevenBitNumber)80),
                new NoteInfo(NoteName.C, 3, 3 * MusicalTimeSpan.ThirtySecond, noteLength, velocity)
            });
        }

        [Test]
        public void TransformNotes_Selection_SelectAll()
        {
            var noteLength = MusicalTimeSpan.Eighth;
            var velocity = (SevenBitNumber)90;

            var pattern = new PatternBuilder()
                .SetNoteLength(noteLength)
                .SetVelocity(velocity)
                .Note(Notes.A2)
                .Note(Notes.C3)
                .Build();

            pattern = pattern.TransformNotes(
                (i, d) => true,
                d => new NoteDescriptor(
                    d.Note == Notes.A2 ? d.Note.Transpose(Interval.Two) : d.Note.Transpose(-Interval.Three),
                    (SevenBitNumber)(d.Velocity - 10),
                    d.Length.Subtract(MusicalTimeSpan.ThirtySecond, TimeSpanMode.LengthLength)));

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.B, 2, null, 3 * MusicalTimeSpan.ThirtySecond, (SevenBitNumber)80),
                new NoteInfo(NoteName.A, 2, 3 * MusicalTimeSpan.ThirtySecond, 3 * MusicalTimeSpan.ThirtySecond, (SevenBitNumber)80)
            });
        }

        [Test]
        public void TransformNotes_Selection_SelectSome_Pattern_Recursive()
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

            pattern = pattern.TransformNotes(
                (i, d) => d.Note.Octave == 3,
                d => new NoteDescriptor(Notes.D9, (SevenBitNumber)65, (MidiTimeSpan)100));

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 2, null, noteLength, velocity),
                new NoteInfo(NoteName.D, 9, noteLength, (MidiTimeSpan)100, (SevenBitNumber)65),

                new NoteInfo(NoteName.D, 9, noteLength.Add((MidiTimeSpan)100, TimeSpanMode.TimeLength), (MidiTimeSpan)100, (SevenBitNumber)65),
                new NoteInfo(NoteName.B, 1, noteLength.Add((MidiTimeSpan)200, TimeSpanMode.TimeLength), PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity)
            });
        }

        [Test]
        public void TransformNotes_Selection_SelectSome_ByIndex_Pattern_Recursive()
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

            pattern = pattern.TransformNotes(
                (i, d) => i == 1 || i == 2,
                d => new NoteDescriptor(Notes.D9, (SevenBitNumber)65, (MidiTimeSpan)100));

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 2, null, noteLength, velocity),
                new NoteInfo(NoteName.D, 9, noteLength, (MidiTimeSpan)100, (SevenBitNumber)65),

                new NoteInfo(NoteName.D, 9, noteLength.Add((MidiTimeSpan)100, TimeSpanMode.TimeLength), (MidiTimeSpan)100, (SevenBitNumber)65),
                new NoteInfo(NoteName.B, 1, noteLength.Add((MidiTimeSpan)200, TimeSpanMode.TimeLength), PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity)
            });
        }

        #endregion

        #region TransformChords

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

        #region SplitAtAnchor

        [Test]
        public void SplitAtAnchor_Empty()
        {
            var pattern = new PatternBuilder().Build();
            var patterns = pattern.SplitAtAnchor(new object());
            CollectionAssert.IsEmpty(patterns, "Pattern splitted incorrectly.");
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SplitAtAnchor_NoActionsBetweenAnchors(bool removeEmptyPatterns)
        {
            var anchor = "A";
            var pattern = new PatternBuilder()
                .Anchor(anchor)
                .Anchor(anchor)
                .Build();

            var patterns = pattern.SplitAtAnchor(anchor, removeEmptyPatterns).ToList();

            if (removeEmptyPatterns)
                CollectionAssert.IsEmpty(patterns, "Pattern splitted incorrectly.");
            else
            {
                Assert.AreEqual(2, patterns.Count, "Sub-patterns count is invalid.");
                CollectionAssert.IsEmpty(patterns[0].Actions, "First sub-pattern is not empty.");
                CollectionAssert.IsEmpty(patterns[1].Actions, "Second sub-pattern is not empty.");
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SplitAtAnchor_SingleAnchor(bool removeEmptyPatterns)
        {
            var anchor = "A";
            var pattern = new PatternBuilder()
                .Anchor(anchor)
                .Note(Notes.FSharp4)
                .Note(Notes.DSharp1)
                .Anchor(anchor)
                .Note(Notes.DSharp0)
                .Anchor(anchor)
                .Build();

            var patterns = pattern.SplitAtAnchor(anchor, removeEmptyPatterns).ToList();

            var firstPatternIndex = 0;
            var secondPatternIndex = 1;

            if (removeEmptyPatterns)
                Assert.AreEqual(2, patterns.Count, "Sub-patterns count is invalid.");
            else
            {
                Assert.AreEqual(3, patterns.Count, "Sub-patterns count is invalid.");
                CollectionAssert.IsEmpty(patterns[0].Actions, "First sub-pattern is not empty.");
                firstPatternIndex++;
                secondPatternIndex++;
            }

            PatternTestUtilities.TestNotes(patterns[firstPatternIndex], new[]
            {
                new NoteInfo(NoteName.FSharp, 4, null, PatternBuilder.DefaultNoteLength),
                new NoteInfo(NoteName.DSharp, 1, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength)
            });

            PatternTestUtilities.TestNotes(patterns[secondPatternIndex], new[]
            {
                new NoteInfo(NoteName.DSharp, 0, null, PatternBuilder.DefaultNoteLength)
            });
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SplitAtAnchor_MultipleAnchors(bool removeEmptyPatterns)
        {
            var anchor1 = "A";
            var anchor2 = "B";

            var pattern = new PatternBuilder()
                .Anchor(anchor1)
                .Note(Notes.FSharp4)
                .Note(Notes.DSharp1)
                .Anchor(anchor2)
                .Note(Notes.DSharp0)
                .Anchor(anchor1)
                .Build();

            var patterns = pattern.SplitAtAnchor(anchor1, removeEmptyPatterns).ToList();

            var subPatternIndex = 0;

            if (removeEmptyPatterns)
                Assert.AreEqual(1, patterns.Count, "Sub-patterns count is invalid.");
            else
            {
                Assert.AreEqual(2, patterns.Count, "Sub-patterns count is invalid.");
                CollectionAssert.IsEmpty(patterns[0].Actions, "First sub-pattern is not empty.");
                subPatternIndex++;
            }

            PatternTestUtilities.TestNotes(patterns[subPatternIndex], new[]
            {
                new NoteInfo(NoteName.FSharp, 4, null, PatternBuilder.DefaultNoteLength),
                new NoteInfo(NoteName.DSharp, 1, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength),
                new NoteInfo(NoteName.DSharp, 0, PatternBuilder.DefaultNoteLength.Multiply(2), PatternBuilder.DefaultNoteLength)
            });
        }

        #endregion

        #region SpliAtAllAnchors

        [Test]
        public void SplitAtAllAnchors_Empty()
        {
            var pattern = new PatternBuilder().Build();
            var patterns = pattern.SplitAtAllAnchors();
            CollectionAssert.IsEmpty(patterns, "Pattern splitted incorrectly.");
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SplitAtAllAnchors_NoActionsBetweenAnchors(bool removeEmptyPatterns)
        {
            var anchor = "A";
            var pattern = new PatternBuilder()
                .Anchor(anchor)
                .Anchor(anchor)
                .Build();

            var patterns = pattern.SplitAtAllAnchors(removeEmptyPatterns).ToList();

            if (removeEmptyPatterns)
                CollectionAssert.IsEmpty(patterns, "Pattern splitted incorrectly.");
            else
            {
                Assert.AreEqual(2, patterns.Count, "Sub-patterns count is invalid.");
                CollectionAssert.IsEmpty(patterns[0].Actions, "First sub-pattern is not empty.");
                CollectionAssert.IsEmpty(patterns[1].Actions, "Second sub-pattern is not empty.");
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SplitAtAllAnchors_SingleAnchor(bool removeEmptyPatterns)
        {
            var anchor = "A";
            var pattern = new PatternBuilder()
                .Anchor(anchor)
                .Note(Notes.FSharp4)
                .Note(Notes.DSharp1)
                .Anchor(anchor)
                .Note(Notes.DSharp0)
                .Anchor(anchor)
                .Build();

            var patterns = pattern.SplitAtAllAnchors(removeEmptyPatterns).ToList();

            var firstPatternIndex = 0;
            var secondPatternIndex = 1;

            if (removeEmptyPatterns)
                Assert.AreEqual(2, patterns.Count, "Sub-patterns count is invalid.");
            else
            {
                Assert.AreEqual(3, patterns.Count, "Sub-patterns count is invalid.");
                CollectionAssert.IsEmpty(patterns[0].Actions, "First sub-pattern is not empty.");
                firstPatternIndex++;
                secondPatternIndex++;
            }

            PatternTestUtilities.TestNotes(patterns[firstPatternIndex], new[]
            {
                new NoteInfo(NoteName.FSharp, 4, null, PatternBuilder.DefaultNoteLength),
                new NoteInfo(NoteName.DSharp, 1, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength)
            });

            PatternTestUtilities.TestNotes(patterns[secondPatternIndex], new[]
            {
                new NoteInfo(NoteName.DSharp, 0, null, PatternBuilder.DefaultNoteLength)
            });
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SplitAtAllAnchors_MultipleAnchors(bool removeEmptyPatterns)
        {
            var anchor1 = "A";
            var anchor2 = "B";

            var pattern = new PatternBuilder()
                .Anchor(anchor1)
                .Note(Notes.FSharp4)
                .Note(Notes.DSharp1)
                .Anchor(anchor2)
                .Note(Notes.DSharp0)
                .Anchor(anchor1)
                .Build();

            var patterns = pattern.SplitAtAllAnchors(removeEmptyPatterns).ToList();

            var firstPatternIndex = 0;
            var secondPatternIndex = 1;

            if (removeEmptyPatterns)
                Assert.AreEqual(2, patterns.Count, "Sub-patterns count is invalid.");
            else
            {
                Assert.AreEqual(3, patterns.Count, "Sub-patterns count is invalid.");
                CollectionAssert.IsEmpty(patterns[0].Actions, "First sub-pattern is not empty.");
                firstPatternIndex++;
                secondPatternIndex++;
            }

            PatternTestUtilities.TestNotes(patterns[firstPatternIndex], new[]
            {
                new NoteInfo(NoteName.FSharp, 4, null, PatternBuilder.DefaultNoteLength),
                new NoteInfo(NoteName.DSharp, 1, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength)
            });

            PatternTestUtilities.TestNotes(patterns[secondPatternIndex], new[]
            {
                new NoteInfo(NoteName.DSharp, 0, null, PatternBuilder.DefaultNoteLength)
            });
        }

        #endregion

        #region SplitAtMarker

        [Test]
        public void SplitAtMarker_Empty()
        {
            var pattern = new PatternBuilder().Build();
            var patterns = pattern.SplitAtMarker("A");
            CollectionAssert.IsEmpty(patterns, "Pattern splitted incorrectly.");
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SplitAtMarker_NoActionsBetweenMarkers(bool removeEmptyPatterns)
        {
            var marker = "A";
            var pattern = new PatternBuilder()
                .Marker(marker)
                .Marker(marker)
                .Build();

            var patterns = pattern.SplitAtMarker(marker, removeEmptyPatterns).ToList();

            if (removeEmptyPatterns)
                CollectionAssert.IsEmpty(patterns, "Pattern splitted incorrectly.");
            else
            {
                Assert.AreEqual(2, patterns.Count, "Sub-patterns count is invalid.");
                CollectionAssert.IsEmpty(patterns[0].Actions, "First sub-pattern is not empty.");
                CollectionAssert.IsEmpty(patterns[1].Actions, "Second sub-pattern is not empty.");
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SplitAtMarker_SingleMarker(bool removeEmptyPatterns)
        {
            var marker = "A";
            var pattern = new PatternBuilder()
                .Marker(marker)
                .Note(Notes.FSharp4)
                .Note(Notes.DSharp1)
                .Marker(marker)
                .Note(Notes.DSharp0)
                .Marker(marker)
                .Build();

            var patterns = pattern.SplitAtMarker(marker, removeEmptyPatterns).ToList();

            var firstPatternIndex = 0;
            var secondPatternIndex = 1;

            if (removeEmptyPatterns)
                Assert.AreEqual(2, patterns.Count, "Sub-patterns count is invalid.");
            else
            {
                Assert.AreEqual(3, patterns.Count, "Sub-patterns count is invalid.");
                CollectionAssert.IsEmpty(patterns[0].Actions, "First sub-pattern is not empty.");
                firstPatternIndex++;
                secondPatternIndex++;
            }

            PatternTestUtilities.TestNotes(patterns[firstPatternIndex], new[]
            {
                new NoteInfo(NoteName.FSharp, 4, null, PatternBuilder.DefaultNoteLength),
                new NoteInfo(NoteName.DSharp, 1, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength)
            });

            PatternTestUtilities.TestNotes(patterns[secondPatternIndex], new[]
            {
                new NoteInfo(NoteName.DSharp, 0, null, PatternBuilder.DefaultNoteLength)
            });
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SplitAtMarker_MultipleMarkers(bool removeEmptyPatterns)
        {
            var marker1 = "A";
            var marker2 = "B";

            var pattern = new PatternBuilder()
                .Marker(marker1)
                .Note(Notes.FSharp4)
                .Note(Notes.DSharp1)
                .Marker(marker2)
                .Note(Notes.DSharp0)
                .Marker(marker1)
                .Build();

            var patterns = pattern.SplitAtMarker(marker1, removeEmptyPatterns).ToList();

            var subPatternIndex = 0;

            if (removeEmptyPatterns)
                Assert.AreEqual(1, patterns.Count, "Sub-patterns count is invalid.");
            else
            {
                Assert.AreEqual(2, patterns.Count, "Sub-patterns count is invalid.");
                CollectionAssert.IsEmpty(patterns[0].Actions, "First sub-pattern is not empty.");
                subPatternIndex++;
            }

            PatternTestUtilities.TestNotes(patterns[subPatternIndex], new[]
            {
                new NoteInfo(NoteName.FSharp, 4, null, PatternBuilder.DefaultNoteLength),
                new NoteInfo(NoteName.DSharp, 1, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength),
                new NoteInfo(NoteName.DSharp, 0, PatternBuilder.DefaultNoteLength.Multiply(2), PatternBuilder.DefaultNoteLength)
            });
        }

        [Test]
        public void SplitAtMarker_MultipleMarkers_OrdinalIgnoreCase()
        {
            var marker1 = "A";
            var marker2 = "a";

            var pattern = new PatternBuilder()
                .Marker(marker1)
                .Note(Notes.FSharp4)
                .Note(Notes.DSharp1)
                .Marker(marker2)
                .Note(Notes.DSharp0)
                .Marker(marker1)
                .Build();

            var patterns = pattern.SplitAtMarker(marker1, true, System.StringComparison.OrdinalIgnoreCase).ToList();

            Assert.AreEqual(2, patterns.Count, "Sub-patterns count is invalid.");

            PatternTestUtilities.TestNotes(patterns[0], new[]
            {
                new NoteInfo(NoteName.FSharp, 4, null, PatternBuilder.DefaultNoteLength),
                new NoteInfo(NoteName.DSharp, 1, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength)
            });

            PatternTestUtilities.TestNotes(patterns[1], new[]
            {
                new NoteInfo(NoteName.DSharp, 0, null, PatternBuilder.DefaultNoteLength)
            });
        }

        #endregion

        #region SpliAtAllMarkers

        [Test]
        public void SplitAtAllMarkers_Empty()
        {
            var pattern = new PatternBuilder().Build();
            var patterns = pattern.SplitAtAllMarkers();
            CollectionAssert.IsEmpty(patterns, "Pattern splitted incorrectly.");
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SplitAtAllMarkers_NoActionsBetweenMarkers(bool removeEmptyPatterns)
        {
            var marker = "A";
            var pattern = new PatternBuilder()
                .Marker(marker)
                .Marker(marker)
                .Build();

            var patterns = pattern.SplitAtAllMarkers(removeEmptyPatterns).ToList();

            if (removeEmptyPatterns)
                CollectionAssert.IsEmpty(patterns, "Pattern splitted incorrectly.");
            else
            {
                Assert.AreEqual(2, patterns.Count, "Sub-patterns count is invalid.");
                CollectionAssert.IsEmpty(patterns[0].Actions, "First sub-pattern is not empty.");
                CollectionAssert.IsEmpty(patterns[1].Actions, "Second sub-pattern is not empty.");
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SplitAtAllMarkers_SingleMarker(bool removeEmptyPatterns)
        {
            var marker = "A";
            var pattern = new PatternBuilder()
                .Marker(marker)
                .Note(Notes.FSharp4)
                .Note(Notes.DSharp1)
                .Marker(marker)
                .Note(Notes.DSharp0)
                .Marker(marker)
                .Build();

            var patterns = pattern.SplitAtAllMarkers(removeEmptyPatterns).ToList();

            var firstPatternIndex = 0;
            var secondPatternIndex = 1;

            if (removeEmptyPatterns)
                Assert.AreEqual(2, patterns.Count, "Sub-patterns count is invalid.");
            else
            {
                Assert.AreEqual(3, patterns.Count, "Sub-patterns count is invalid.");
                CollectionAssert.IsEmpty(patterns[0].Actions, "First sub-pattern is not empty.");
                firstPatternIndex++;
                secondPatternIndex++;
            }

            PatternTestUtilities.TestNotes(patterns[firstPatternIndex], new[]
            {
                new NoteInfo(NoteName.FSharp, 4, null, PatternBuilder.DefaultNoteLength),
                new NoteInfo(NoteName.DSharp, 1, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength)
            });

            PatternTestUtilities.TestNotes(patterns[secondPatternIndex], new[]
            {
                new NoteInfo(NoteName.DSharp, 0, null, PatternBuilder.DefaultNoteLength)
            });
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SplitAtAllMarkers_MultipleMarkers(bool removeEmptyPatterns)
        {
            var marker1 = "A";
            var marker2 = "B";

            var pattern = new PatternBuilder()
                .Marker(marker1)
                .Note(Notes.FSharp4)
                .Note(Notes.DSharp1)
                .Marker(marker2)
                .Note(Notes.DSharp0)
                .Marker(marker1)
                .Build();

            var patterns = pattern.SplitAtAllMarkers(removeEmptyPatterns).ToList();

            var firstPatternIndex = 0;
            var secondPatternIndex = 1;

            if (removeEmptyPatterns)
                Assert.AreEqual(2, patterns.Count, "Sub-patterns count is invalid.");
            else
            {
                Assert.AreEqual(3, patterns.Count, "Sub-patterns count is invalid.");
                CollectionAssert.IsEmpty(patterns[0].Actions, "First sub-pattern is not empty.");
                firstPatternIndex++;
                secondPatternIndex++;
            }

            PatternTestUtilities.TestNotes(patterns[firstPatternIndex], new[]
            {
                new NoteInfo(NoteName.FSharp, 4, null, PatternBuilder.DefaultNoteLength),
                new NoteInfo(NoteName.DSharp, 1, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength)
            });

            PatternTestUtilities.TestNotes(patterns[secondPatternIndex], new[]
            {
                new NoteInfo(NoteName.DSharp, 0, null, PatternBuilder.DefaultNoteLength)
            });
        }

        #endregion

        #region CombineInSequence

        [Test]
        public void CombineInSequence_EmptyPatterns()
        {
            var pattern1 = new PatternBuilder().Build();
            var pattern2 = new PatternBuilder().Build();

            var pattern = new[] { pattern1, pattern2 }.CombineInSequence();

            CollectionAssert.IsEmpty(pattern.ToTrackChunk(TempoMap.Default).Events, "Pattern is not empty.");
        }

        [Test]
        public void CombineInSequence_Nulls()
        {
            var pattern = new Pattern[] { null, new PatternBuilder().Build() }.CombineInSequence();

            CollectionAssert.IsEmpty(pattern.ToTrackChunk(TempoMap.Default).Events, "Pattern is not empty.");
        }

        [Test]
        public void CombineInSequence()
        {
            var pattern1 = new PatternBuilder()
                .Note(Notes.DSharp2)
                .Build();

            var noteLength = MusicalTimeSpan.Sixteenth;
            var pattern2 = new PatternBuilder()
                .SetNoteLength(noteLength)
                .Note(Notes.A4)
                .Note(Notes.ASharp4)
                .Build();

            var pattern = new[] { pattern1, pattern2 }.CombineInSequence();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.DSharp, 2, null, PatternBuilder.DefaultNoteLength),

                new NoteInfo(NoteName.A, 4, PatternBuilder.DefaultNoteLength, noteLength),
                new NoteInfo(NoteName.ASharp, 4, PatternBuilder.DefaultNoteLength.Add(noteLength, TimeSpanMode.LengthLength), noteLength)
            });
        }

        #endregion

        #region CombineInParallel

        [Test]
        public void CombineInParallel_EmptyPatterns()
        {
            var pattern1 = new PatternBuilder().Build();
            var pattern2 = new PatternBuilder().Build();

            var pattern = new[] { pattern1, pattern2 }.CombineInParallel();

            CollectionAssert.IsEmpty(pattern.ToTrackChunk(TempoMap.Default).Events, "Pattern is not empty.");
        }

        [Test]
        public void CombineInParallel_Nulls()
        {
            var pattern = new Pattern[] { null, new PatternBuilder().Build() }.CombineInParallel();

            CollectionAssert.IsEmpty(pattern.ToTrackChunk(TempoMap.Default).Events, "Pattern is not empty.");
        }

        [Test]
        public void CombineInParallel()
        {
            var pattern1 = new PatternBuilder()
                .Note(Notes.DSharp2)
                .Build();

            var noteLength = MusicalTimeSpan.Sixteenth;
            var pattern2 = new PatternBuilder()
                .SetNoteLength(noteLength)
                .Note(Notes.A4)
                .Note(Notes.ASharp4)
                .Build();

            var pattern = new[] { pattern1, pattern2 }.CombineInParallel();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.DSharp, 2, null, PatternBuilder.DefaultNoteLength),

                new NoteInfo(NoteName.A, 4, null, noteLength),
                new NoteInfo(NoteName.ASharp, 4, noteLength, noteLength)
            });
        }

        #endregion

        #region SetNotesState

        [Test]
        public void SetNotesState_SelectAll_Enabled()
        {
            var pattern = new PatternBuilder()
                .Note(Notes.A2)
                .Note(Notes.D3)
                .Build();

            pattern.SetNotesState((i, d) => true, PatternActionState.Enabled);

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 2, null, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),
                new NoteInfo(NoteName.D, 3, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity)
            });
        }

        [Test]
        public void SetNotesState_SelectAll_Disabled()
        {
            var pattern = new PatternBuilder()
                .Note(Notes.A2)
                .Note(Notes.D3)
                .Build();

            pattern.SetNotesState((i, d) => true, PatternActionState.Disabled);

            PatternTestUtilities.TestNotes(pattern, new NoteInfo[] { });
        }

        [Test]
        public void SetNotesState_SelectAll_Excluded()
        {
            var pattern = new PatternBuilder()
                .Note(Notes.A2)
                .Note(Notes.D3)
                .Build();

            pattern.SetNotesState((i, d) => true, PatternActionState.Excluded);

            PatternTestUtilities.TestNotes(pattern, new NoteInfo[] { });
        }

        [Test]
        public void SetNotesState_SelectSome_Enabled()
        {
            var pattern = new PatternBuilder()
                .Note(Notes.A2)
                .Note(Notes.D3)
                .Build();

            pattern.SetNotesState((i, d) => d.Note.Octave == 2, PatternActionState.Enabled);

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 2, null, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),
                new NoteInfo(NoteName.D, 3, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity)
            });
        }

        [Test]
        public void SetNotesState_SelectSome_Disabled()
        {
            var pattern = new PatternBuilder()
                .Note(Notes.A2)
                .Note(Notes.D3)
                .Build();

            pattern.SetNotesState((i, d) => i == 0, PatternActionState.Disabled);

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.D, 3, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity)
            });
        }

        [Test]
        public void SetNotesState_SelectSome_Excluded()
        {
            var pattern = new PatternBuilder()
                .Note(Notes.A2)
                .Note(Notes.D3)
                .Build();

            pattern.SetNotesState((i, d) => d.Note == Notes.A2, PatternActionState.Excluded);

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.D, 3, null, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity)
            });
        }

        [Test]
        public void SetNotesState_SelectNone([Values] PatternActionState state)
        {
            var pattern = new PatternBuilder()
                .Note(Notes.A2)
                .Note(Notes.D3)
                .Build();

            pattern.SetNotesState((i, d) => false, PatternActionState.Excluded);

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 2, null, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),
                new NoteInfo(NoteName.D, 3, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity)
            });
        }

        [Test]
        public void SetNotesState_SelectSome_Enabled_Recursive()
        {
            var pattern = new PatternBuilder()
                .Note(Notes.A2)
                .Note(Notes.C3)
                .Pattern(new PatternBuilder()
                    .Note(Notes.DSharp3)
                    .Note(Notes.B1)
                    .Build())
                .Build();

            pattern.SetNotesState((i, d) => i == 0 || i == 2, PatternActionState.Enabled, true);

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 2, null, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),
                new NoteInfo(NoteName.C, 3, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),

                new NoteInfo(NoteName.DSharp, 3, PatternBuilder.DefaultNoteLength.Multiply(2), PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),
                new NoteInfo(NoteName.B, 1, PatternBuilder.DefaultNoteLength.Multiply(3), PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity)
            });
        }

        [Test]
        public void SetNotesState_SelectSome_Disabled_Recursive()
        {
            var pattern = new PatternBuilder()
                .Note(Notes.A2)
                .Note(Notes.C3)
                .Pattern(new PatternBuilder()
                    .Note(Notes.DSharp3)
                    .Note(Notes.B1)
                    .Build())
                .Build();

            pattern.SetNotesState((i, d) => i == 0 || i == 2, PatternActionState.Disabled, true);

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.C, 3, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),

                new NoteInfo(NoteName.B, 1, PatternBuilder.DefaultNoteLength.Multiply(3), PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity)
            });
        }

        [Test]
        public void SetNotesState_SelectSome_Excluded_Recursive()
        {
            var pattern = new PatternBuilder()
                .Note(Notes.A2)
                .Note(Notes.C3)
                .Pattern(new PatternBuilder()
                    .Note(Notes.DSharp3)
                    .Note(Notes.B1)
                    .Build())
                .Build();

            pattern.SetNotesState((i, d) => i == 0 || i == 2, PatternActionState.Excluded, true);

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.C, 3, null, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),

                new NoteInfo(NoteName.B, 1, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity)
            });
        }

        #endregion

        #region SetChordsState

        [Test]
        public void SetChordsState_SelectAll_Enabled()
        {
            var pattern = new PatternBuilder()
                .Note(Notes.A2)
                .Chord(new[] { Notes.D3, Notes.A1 })
                .Build();

            pattern.SetChordsState((i, d) => true, PatternActionState.Enabled);

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 2, null, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),
                new NoteInfo(NoteName.D, 3, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),
                new NoteInfo(NoteName.A, 1, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity)
            });
        }

        [Test]
        public void SetChordsState_SelectAll_Disabled()
        {
            var pattern = new PatternBuilder()
                .Chord(new[] { Notes.A2 })
                .Chord(new[] { Notes.D3, Notes.ASharp5 })
                .Build();

            pattern.SetChordsState((i, d) => true, PatternActionState.Disabled);

            PatternTestUtilities.TestNotes(pattern, new NoteInfo[] { });
        }

        [Test]
        public void SetChordsState_SelectAll_Excluded()
        {
            var pattern = new PatternBuilder()
                .Chord(new[] { Notes.A2 })
                .Chord(new[] { Notes.D3, Notes.ASharp5 })
                .Build();

            pattern.SetChordsState((i, d) => true, PatternActionState.Excluded);

            PatternTestUtilities.TestNotes(pattern, new NoteInfo[] { });
        }

        [Test]
        public void SetChordsState_SelectSome_Enabled()
        {
            var pattern = new PatternBuilder()
                .Chord(new[] { Notes.A2 })
                .Chord(new[] { Notes.D3, Notes.ASharp5 })
                .Build();

            pattern.SetChordsState((i, d) => d.Notes.Any(n => n.Octave == 2), PatternActionState.Enabled);

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 2, null, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),
                new NoteInfo(NoteName.D, 3, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),
                new NoteInfo(NoteName.ASharp, 5, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity)
            });
        }

        [Test]
        public void SetChordsState_SelectSome_Disabled()
        {
            var pattern = new PatternBuilder()
                .Chord(new[] { Notes.A2 })
                .Chord(new[] { Notes.D3, Notes.ASharp5 })
                .Build();

            pattern.SetChordsState((i, d) => i == 0, PatternActionState.Disabled);

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.D, 3, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),
                new NoteInfo(NoteName.ASharp, 5, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity)
            });
        }

        [Test]
        public void SetChordsState_SelectSome_Excluded()
        {
            var pattern = new PatternBuilder()
                .Chord(new[] { Notes.A2 })
                .Chord(new[] { Notes.D3, Notes.ASharp5 })
                .Build();

            pattern.SetChordsState((i, d) => d.Notes.Contains(Notes.A2), PatternActionState.Excluded);

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.D, 3, null, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),
                new NoteInfo(NoteName.ASharp, 5, null, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity)
            });
        }

        [Test]
        public void SetChordsState_SelectNone([Values] PatternActionState state)
        {
            var pattern = new PatternBuilder()
                .Chord(new[] { Notes.A2 })
                .Chord(new[] { Notes.D3, Notes.ASharp5 })
                .Build();

            pattern.SetChordsState((i, d) => false, PatternActionState.Excluded);

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 2, null, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),
                new NoteInfo(NoteName.D, 3, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),
                new NoteInfo(NoteName.ASharp, 5, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity)
            });
        }

        [Test]
        public void SetChordsState_SelectSome_Enabled_Recursive()
        {
            var pattern = new PatternBuilder()
                .Chord(new[] { Notes.A2 })
                .Pattern(new PatternBuilder()
                    .Chord(new[] { Notes.D3, Notes.ASharp5 })
                    .Build())
                .Build();

            pattern.SetChordsState((i, d) => i == 0, PatternActionState.Enabled, true);

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 2, null, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),
                new NoteInfo(NoteName.D, 3, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),
                new NoteInfo(NoteName.ASharp, 5, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity)
            });
        }

        [Test]
        public void SetChordsState_SelectSome_Disabled_Recursive()
        {
            var pattern = new PatternBuilder()
                .Chord(new[] { Notes.A2 })
                .Pattern(new PatternBuilder()
                    .Chord(new[] { Notes.D3, Notes.ASharp5 })
                    .Chord(new[] { Notes.D6 })
                    .Build())
                .Build();

            pattern.SetChordsState((i, d) => i == 1, PatternActionState.Disabled, true);

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 2, null, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),
                new NoteInfo(NoteName.D, 6, PatternBuilder.DefaultNoteLength.Multiply(2), PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity)
            });
        }

        [Test]
        public void SetChordsState_SelectSome_Excluded_Recursive()
        {
            var pattern = new PatternBuilder()
                .Chord(new[] { Notes.A2 })
                .Pattern(new PatternBuilder()
                    .Chord(new[] { Notes.D3, Notes.ASharp5 })
                    .Chord(new[] { Notes.D6 })
                    .Build())
                .Build();

            pattern.SetChordsState((i, d) => i == 0 || i == 1, PatternActionState.Excluded, true);

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.D, 6, null, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity)
            });
        }

        #endregion

        #endregion
    }
}
