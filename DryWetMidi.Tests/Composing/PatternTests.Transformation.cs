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

        #endregion
    }
}
