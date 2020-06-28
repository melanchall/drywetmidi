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
        public void Pattern()
        {
            var noteLength1 = MusicalTimeSpan.Quarter;
            var pattern1 = new PatternBuilder()
                .SetNoteLength(noteLength1)
                .Note(Notes.A0)
                .Note(Notes.C1)
                .Build();

            var noteLength2 = MusicalTimeSpan.Quarter;
            var pattern2 = new PatternBuilder()
                .SetNoteLength(noteLength2)
                .Note(Notes.ASharp2)
                .Note(Notes.CSharp2)
                .Pattern(pattern1)
                .Build();

            PatternTestUtilities.TestNotes(pattern2, new[]
            {
                new NoteInfo(NoteName.ASharp, 2, null, noteLength2),
                new NoteInfo(NoteName.CSharp, 2, noteLength2, noteLength2),
                new NoteInfo(NoteName.A, 0, 2 * noteLength2, noteLength1),
                new NoteInfo(NoteName.C, 1, 2 * noteLength2 + noteLength1, noteLength1)
            });
        }

        [Test]
        public void Pattern_Repeat()
        {
            var noteLength1 = MusicalTimeSpan.Quarter;
            var pattern1 = new PatternBuilder()
                .SetNoteLength(noteLength1)
                .Note(Notes.A0)
                .Note(Notes.C1)
                .Build();

            var pattern2 = new PatternBuilder()
                .Pattern(pattern1)
                .Repeat(2)
                .Build();

            PatternTestUtilities.TestNotes(pattern2, new[]
            {
                new NoteInfo(NoteName.A, 0, null, noteLength1),
                new NoteInfo(NoteName.C, 1, noteLength1, noteLength1),
                new NoteInfo(NoteName.A, 0, 2 * noteLength1, noteLength1),
                new NoteInfo(NoteName.C, 1, 3 * noteLength1, noteLength1),
                new NoteInfo(NoteName.A, 0, 4 * noteLength1, noteLength1),
                new NoteInfo(NoteName.C, 1, 5 * noteLength1, noteLength1)
            });
        }

        #endregion
    }
}
