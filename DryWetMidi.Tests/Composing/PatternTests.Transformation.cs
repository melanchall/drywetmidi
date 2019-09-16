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

            pattern = pattern.TransformNotes((n, v, l) => new NoteDescriptor(n, v, l));

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

            pattern = pattern.TransformNotes((n, v, l) => new NoteDescriptor(
                n == Notes.A2 ? n.Transpose(Interval.Two) : n.Transpose(-Interval.Three),
                (SevenBitNumber)(v - 10),
                l.Subtract(MusicalTimeSpan.ThirtySecond, TimeSpanMode.LengthLength)));

            TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.B, 2, null, 3 * MusicalTimeSpan.ThirtySecond, (SevenBitNumber)80),
                new NoteInfo(NoteName.A, 2, 3 * MusicalTimeSpan.ThirtySecond, 3 * MusicalTimeSpan.ThirtySecond, (SevenBitNumber)80)
            });
        }

        #endregion
    }
}
