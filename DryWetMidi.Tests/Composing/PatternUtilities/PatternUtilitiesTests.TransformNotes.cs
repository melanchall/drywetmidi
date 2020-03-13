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
    }
}
