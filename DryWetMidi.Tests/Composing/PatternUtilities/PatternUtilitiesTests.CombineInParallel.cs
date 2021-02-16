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

        /// <summary>
        /// This test ensures that the final operation when combining patterns in parallel does not end with a move back to the previous time.
        /// </summary>
        [Test]
        public void CombineInParallel_No_Final_Move()
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

            var parallelPatterns = new[] { pattern1, pattern2 }.CombineInParallel();

            var pattern3 = new PatternBuilder()
                    .Note(Notes.C4)
                    .Build();

            var pattern = new[] { parallelPatterns, pattern3 }.CombineInSequence();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.DSharp, 2, null, PatternBuilder.DefaultNoteLength),

                new NoteInfo(NoteName.A, 4, null, noteLength),
                new NoteInfo(NoteName.ASharp, 4, noteLength, noteLength),

                new NoteInfo(NoteName.C, 4, noteLength*2, PatternBuilder.DefaultNoteLength),
            });
        }

        #endregion
    }
}
