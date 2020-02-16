using System.Linq;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Composing
{
    [TestFixture]
    public sealed partial class PatternUtilitiesTests
    {
        #region Test methods

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
    }
}
