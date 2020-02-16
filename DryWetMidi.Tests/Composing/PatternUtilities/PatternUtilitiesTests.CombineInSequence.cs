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
    }
}
