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
        public void SplitAtMarker_Empty()
        {
            var pattern = new PatternBuilder().Build();
            var patterns = pattern.SplitAtMarker("A");
            CollectionAssert.IsEmpty(patterns, "Pattern splitted incorrectly.");
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SplitAtMarker_NoActionsBetweenMarkers(bool removeEmptyPatterns)
        {
            var marker = "A";
            var pattern = new PatternBuilder()
                .Marker(marker)
                .Marker(marker)
                .Build();

            var patterns = pattern.SplitAtMarker(marker, removeEmptyPatterns).ToList();

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
        public void SplitAtMarker_SingleMarker(bool removeEmptyPatterns)
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

            var patterns = pattern.SplitAtMarker(marker, removeEmptyPatterns).ToList();

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
        public void SplitAtMarker_MultipleMarkers(bool removeEmptyPatterns)
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

            var patterns = pattern.SplitAtMarker(marker1, removeEmptyPatterns).ToList();

            var subPatternIndex = 0;

            if (removeEmptyPatterns)
                Assert.AreEqual(1, patterns.Count, "Sub-patterns count is invalid.");
            else
            {
                Assert.AreEqual(2, patterns.Count, "Sub-patterns count is invalid.");
                CollectionAssert.IsEmpty(patterns[0].Actions, "First sub-pattern is not empty.");
                subPatternIndex++;
            }

            PatternTestUtilities.TestNotes(patterns[subPatternIndex], new[]
            {
                new NoteInfo(NoteName.FSharp, 4, null, PatternBuilder.DefaultNoteLength),
                new NoteInfo(NoteName.DSharp, 1, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength),
                new NoteInfo(NoteName.DSharp, 0, PatternBuilder.DefaultNoteLength.Multiply(2), PatternBuilder.DefaultNoteLength)
            });
        }

        [Test]
        public void SplitAtMarker_MultipleMarkers_OrdinalIgnoreCase()
        {
            var marker1 = "A";
            var marker2 = "a";

            var pattern = new PatternBuilder()
                .Marker(marker1)
                .Note(Notes.FSharp4)
                .Note(Notes.DSharp1)
                .Marker(marker2)
                .Note(Notes.DSharp0)
                .Marker(marker1)
                .Build();

            var patterns = pattern.SplitAtMarker(marker1, true, System.StringComparison.OrdinalIgnoreCase).ToList();

            Assert.AreEqual(2, patterns.Count, "Sub-patterns count is invalid.");

            PatternTestUtilities.TestNotes(patterns[0], new[]
            {
                new NoteInfo(NoteName.FSharp, 4, null, PatternBuilder.DefaultNoteLength),
                new NoteInfo(NoteName.DSharp, 1, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength)
            });

            PatternTestUtilities.TestNotes(patterns[1], new[]
            {
                new NoteInfo(NoteName.DSharp, 0, null, PatternBuilder.DefaultNoteLength)
            });
        }

        #endregion
    }
}
