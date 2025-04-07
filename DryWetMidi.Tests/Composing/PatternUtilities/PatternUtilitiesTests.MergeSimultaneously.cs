using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Composing
{
    [TestFixture]
    public sealed partial class PatternUtilitiesTests
    {
        #region Test methods

        [Test]
        public void MergeSimultaneously_EmptyPatterns()
        {
            var pattern1 = new PatternBuilder().Build();
            var pattern2 = new PatternBuilder().Build();

            var pattern = new[] { pattern1, pattern2 }.MergeSimultaneously();

            CollectionAssert.IsEmpty(pattern.ToTrackChunk(TempoMap.Default).Events, "Pattern is not empty.");
        }

        [Test]
        public void MergeSimultaneously_Nulls()
        {
            var pattern = new Pattern[] { null, new PatternBuilder().Build() }.MergeSimultaneously();

            CollectionAssert.IsEmpty(pattern.ToTrackChunk(TempoMap.Default).Events, "Pattern is not empty.");
        }

        [Test]
        public void MergeSimultaneously()
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

            var pattern = new[] { pattern1, pattern2 }.MergeSimultaneously();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.DSharp, 2, null, PatternBuilder.DefaultNoteLength),

                new NoteInfo(NoteName.A, 4, null, noteLength),
                new NoteInfo(NoteName.ASharp, 4, noteLength, noteLength)
            });
        }

        [Test]
        [Description("This test ensures that the final operation when combining patterns in parallel does not end with a move back to the previous time.")]
        public void MergeSimultaneously_No_Final_Move()
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

            var parallelPatterns = new[] { pattern1, pattern2 }.MergeSimultaneously();

            var pattern = new PatternBuilder()
                .Pattern(parallelPatterns)
                .Note(Notes.C4) // <-- this note should play AFTER the end of the parallel patterns
                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.DSharp, 2, null, PatternBuilder.DefaultNoteLength),

                new NoteInfo(NoteName.A, 4, null, noteLength),
                new NoteInfo(NoteName.ASharp, 4, noteLength, noteLength),

                new NoteInfo(NoteName.C, 4, noteLength * 2, PatternBuilder.DefaultNoteLength),
            });
        }

        #endregion
    }
}
