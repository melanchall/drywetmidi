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
    }
}
