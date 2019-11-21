using System;
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
        [Description("Add unkeyed anchor after some time movings, jump to the anchor with MoveToFirstAnchor and add note.")]
        public void MoveToFirstAnchor_Unkeyed_OneUnkeyed()
        {
            var moveTime = new MetricTimeSpan(0, 0, 10);
            var step = new MetricTimeSpan(0, 0, 11);
            var anchorTime = moveTime + step;

            var pattern = new PatternBuilder()
                .MoveToTime(moveTime)
                .StepForward(step)
                .Anchor()
                .MoveToTime(new MetricTimeSpan(0, 0, 30))
                .StepBack(new MetricTimeSpan(0, 0, 5))
                .MoveToFirstAnchor()

                .Note(Notes.A0)

                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 0, anchorTime, MusicalTimeSpan.Quarter)
            });
        }

        [Test]
        [Description("Add unkeyed and keyed anchors after some time movings, jump to an anchor with MoveToFirstAnchor and add note.")]
        public void MoveToFirstAnchor_Unkeyed_OneUnkeyedAndOneKeyed()
        {
            var moveTime = new MetricTimeSpan(0, 0, 10);
            var step = new MetricTimeSpan(0, 0, 11);
            var anchorTime = moveTime + step;

            var pattern = new PatternBuilder()
                .MoveToTime(moveTime)
                .StepForward(step)
                .Anchor()
                .MoveToTime(new MetricTimeSpan(0, 0, 30))
                .Anchor("Test")
                .StepBack(new MetricTimeSpan(0, 0, 5))
                .MoveToFirstAnchor()

                .Note(Notes.A0)

                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 0, anchorTime, MusicalTimeSpan.Quarter)
            });
        }

        [Test]
        [Description("Add unkeyed and keyed anchors after some time movings, jump to an anchor with MoveToFirstAnchor(key) and add note.")]
        public void MoveToFirstAnchor_Keyed_OneUnkeyedAndOneKeyed()
        {
            var anchorTime = new MetricTimeSpan(0, 0, 30);

            var pattern = new PatternBuilder()
                .MoveToTime(new MetricTimeSpan(0, 0, 10))
                .StepForward(new MetricTimeSpan(0, 0, 11))
                .Anchor()
                .MoveToTime(anchorTime)
                .Anchor("Test")
                .StepBack(new MetricTimeSpan(0, 0, 5))
                .MoveToFirstAnchor("Test")

                .Note(Notes.A0)

                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 0, anchorTime, MusicalTimeSpan.Quarter)
            });
        }

        [Test]
        [Description("Add no anchors and try to jump to an anchor with MoveToFirstAnchor.")]
        public void MoveToFirstAnchor_Unkeyed_NoAnchors()
        {
            Assert.Throws<InvalidOperationException>(() =>
                new PatternBuilder()
                    .MoveToTime(new MetricTimeSpan(0, 0, 10))
                    .StepForward(new MetricTimeSpan(0, 0, 11))
                    .MoveToTime(new MetricTimeSpan(0, 0, 30))
                    .StepBack(new MetricTimeSpan(0, 0, 5))
                    .MoveToFirstAnchor()

                    .Note(Notes.A0)

                    .Build());
        }

        [Test]
        [Description("Add unkeyed anchor and try to jump to an anchor with MoveToFirstAnchor(key).")]
        public void MoveToFirstAnchor_Keyed_NoKeyedAnchors()
        {
            Assert.Throws<ArgumentException>(() =>
                new PatternBuilder()
                    .MoveToTime(new MetricTimeSpan(0, 0, 10))
                    .StepForward(new MetricTimeSpan(0, 0, 11))
                    .MoveToTime(new MetricTimeSpan(0, 0, 30))
                    .Anchor()
                    .StepBack(new MetricTimeSpan(0, 0, 5))
                    .MoveToFirstAnchor("Test")

                    .Note(Notes.A0)

                    .Build());
        }

        #endregion
    }
}
