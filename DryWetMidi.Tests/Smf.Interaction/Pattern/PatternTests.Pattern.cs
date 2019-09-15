using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Smf.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction
{
    [TestFixture]
    public sealed partial class PatternTests
    {
        #region Test methods

        [Test]
        public void Pattern()
        {
            var noteLength1 = MusicalTimeSpan.Quarter;
            var pattern1 = new PatternBuilder()
                .SetNoteLength(noteLength1)
                .Note(Octave.Get(0).A)
                .Note(Octave.Get(1).C)
                .Build();

            var noteLength2 = MusicalTimeSpan.Quarter;
            var pattern2 = new PatternBuilder()
                .SetNoteLength(noteLength2)
                .Note(Octave.Get(2).ASharp)
                .Note(Octave.Get(2).CSharp)
                .Pattern(pattern1)
                .Build();

            TestNotes(pattern2, new[]
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
                .Note(Octave.Get(0).A)
                .Note(Octave.Get(1).C)
                .Build();

            var noteLength2 = MusicalTimeSpan.Quarter;
            var pattern2 = new PatternBuilder()
                .Pattern(pattern1)
                .Repeat(2)
                .Build();

            TestNotes(pattern2, new[]
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
